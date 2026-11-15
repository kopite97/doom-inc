using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public sealed class BalanceDocument
    {
        public JsonDefinitionTopic Topic { get; }
        public string Path { get; }
        public byte[] Bytes { get; }
        public string Hash => Digest(Bytes);
        public string Text { get; }
        private readonly bool bom;
        public IReadOnlyList<BalanceJsonToken> Entries { get; }
        public BalanceDocument(JsonDefinitionTopic topic, string path)
        {
            Topic = topic; Path = System.IO.Path.GetFullPath(path); Bytes = File.ReadAllBytes(Path);
            bom = Bytes.Length >= 3 && Bytes[0] == 239 && Bytes[1] == 187 && Bytes[2] == 191;
            Text = new UTF8Encoding(false, true).GetString(Bytes, bom ? 3 : 0, Bytes.Length - (bom ? 3 : 0));
            var root = BalanceJsonToken.Parse(Text);
            if (root.Kind != '{' || !root.Fields.TryGetValue("schemaVersion", out var version) || version.Kind != 'n'
                || !decimal.TryParse(version.Raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var schema) || schema != 1
                || !root.Fields.TryGetValue("entries", out var entries) || entries.Kind != '[') throw new FormatException(topic.Key + ": 지원하지 않는 문서 구조");
            Entries = entries.Items;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in Entries)
                if (entry.Kind != '{' || !entry.Fields.TryGetValue("definitionId", out var id) || string.IsNullOrWhiteSpace(id.StringValue) || !ids.Add(id.StringValue))
                    throw new FormatException(topic.Key + ": definitionId 누락/중복");
        }
        public BalanceJsonToken Entry(string id) => Entries.Single(e => e.Fields["definitionId"].StringValue == id);
        public string Candidate(IEnumerable<BalanceChange> changes)
        {
            var edits = new List<(int start, int length, string value)>();
            foreach (var change in changes.Where(c => c.topic == Topic.Key))
            {
                var entry = Entry(change.id); var field = BalanceMetadataReader.Read(Topic.DtoType).Single(f => f.Field.Name == change.field);
                if (field.Diagnostic(entry) != "편집 가능" || field.Signature != change.signature) throw new InvalidOperationException("편집 필드가 변경되었습니다: " + change.field);
                field.ValidateNumber(change.value); var token = entry.Fields[change.field];
                if (token.Raw != change.original) throw new InvalidOperationException("원본 값 충돌: " + change.field);
                if (edits.Any(e => e.start == token.Start)) throw new InvalidOperationException("중복 변경");
                edits.Add((token.Start, token.Length, change.value));
            }
            var result = Text;
            foreach (var edit in edits.OrderByDescending(e => e.start)) result = result.Remove(edit.start, edit.length).Insert(edit.start, edit.value);
            return result;
        }
        public byte[] Encode(string text)
        {
            var body = new UTF8Encoding(false, true).GetBytes(text);
            return bom ? new byte[] { 239, 187, 191 }.Concat(body).ToArray() : body;
        }
        public static string Digest(byte[] bytes) { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
    }

    public sealed class BalanceDocumentStore
    {
        public IReadOnlyList<BalanceDocument> Documents { get; }
        public string RecoveryRoot { get; }
        private readonly Action<IReadOnlyDictionary<string, string>> validate;
        private readonly Action reimport;
        // Optional fault injection exercises file/refresh boundaries without terminating the Editor.
        private readonly Action<string> checkpoint;
        public BalanceDocumentStore(IReadOnlyList<BalanceDocument> documents, string recoveryRoot,
            Action<IReadOnlyDictionary<string, string>> validate, Action reimport, Action<string> checkpoint = null)
        { Documents = documents; RecoveryRoot = System.IO.Path.GetFullPath(recoveryRoot); this.validate = validate; this.reimport = reimport; this.checkpoint = checkpoint; }
        public Dictionary<string, string> Preview(BalanceEditSession session)
        {
            var conflicts = session.Conflicts(Documents);
            if (conflicts.Count > 0) throw new InvalidOperationException(string.Join("\n", conflicts));
            foreach (var doc in Documents)
            foreach (var field in BalanceMetadataReader.Read(doc.Topic.DtoType))
                if (field.Error != null) throw new InvalidOperationException(doc.Topic.Key + "/" + field.Field.Name + ": " + field.Error);
            return Documents.ToDictionary(d => d.Topic.Key, d => d.Candidate(session.changes), StringComparer.Ordinal);
        }
        public void Validate(BalanceEditSession session) { EnsureUnchanged(); validate(Preview(session)); }
        public void EnsureUnchanged()
        {
            foreach (var doc in Documents)
                if (!File.Exists(doc.Path) || BalanceDocument.Digest(File.ReadAllBytes(doc.Path)) != doc.Hash)
                    throw new IOException("외부 수정/삭제 충돌: " + doc.Path);
        }
        public IEnumerable<string> Pending()
        {
            if (!Directory.Exists(RecoveryRoot)) return Array.Empty<string>();
            return Directory.GetFiles(RecoveryRoot, "journal.json", SearchOption.AllDirectories)
                .Where(p => { try { var j = ReadJournal(p); return j.state != "complete" && j.state != "restored"; } catch { return true; } }).ToArray();
        }
        public void Save(BalanceEditSession session)
        {
            if (Pending().Any()) throw new IOException("미완료 복구 기록이 있습니다. 복구 후 다시 저장하세요.");
            Validate(session); if (!session.Dirty) return;
            var candidates = Preview(session);
            var folder = System.IO.Path.Combine(RecoveryRoot, Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
            var path = System.IO.Path.Combine(folder, "journal.json");
            var journal = new Journal { state = "prepared" };
            // All snapshots are written before touching any product file.
            foreach (var doc in Documents.Where(d => candidates[d.Topic.Key] != d.Text))
            {
                var index = journal.files.Count.ToString();
                var original = System.IO.Path.Combine(folder, "original", index + ".json");
                var candidate = System.IO.Path.Combine(folder, "candidate", index + ".json");
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(original)); Directory.CreateDirectory(System.IO.Path.GetDirectoryName(candidate));
                DurableWrite(original, doc.Bytes); var bytes = doc.Encode(candidates[doc.Topic.Key]); DurableWrite(candidate, bytes);
                journal.files.Add(new RecoveryFile { topic = doc.Topic.Key, target = doc.Path, original = original, candidate = candidate,
                    before = doc.Hash, after = BalanceDocument.Digest(bytes) });
            }
            WriteJournal(path, journal);
            try
            {
                EnsureUnchanged(); journal.state = "applying"; WriteJournal(path, journal);
                foreach (var file in journal.files)
                {
                    if (BalanceDocument.Digest(File.ReadAllBytes(file.target)) != file.before) throw new IOException("저장 중 외부 수정: " + file.target);
                    checkpoint?.Invoke("before-write:" + file.topic);
                    AtomicWrite(file.target, File.ReadAllBytes(file.candidate), folder);
                }
                journal.state = "importing"; WriteJournal(path, journal); checkpoint?.Invoke("before-import"); reimport();
                checkpoint?.Invoke("before-final-validation"); validate(ReadTexts());
                foreach (var file in journal.files)
                    if (BalanceDocument.Digest(File.ReadAllBytes(file.target)) != file.after) throw new IOException("저장 후 외부 수정: " + file.target);
                journal.state = "complete"; WriteJournal(path, journal); CleanupSnapshots(journal);
            }
            catch (Exception error)
            {
                try { Recover(path); }
                catch (Exception recovery) { throw new IOException("저장 실패. 복구 미완료: " + path + "\n" + error.Message + "\n" + recovery.Message, error); }
                throw new IOException("저장 실패, 원본 복구 완료. 초안을 유지합니다.\n" + error.Message, error);
            }
        }
        private Dictionary<string, string> ReadTexts() => Documents.ToDictionary(d => d.Topic.Key,
            d => new BalanceDocument(d.Topic, d.Path).Text, StringComparer.Ordinal);
        public void Recover(string path)
        {
            path = System.IO.Path.GetFullPath(path);
            if (!path.StartsWith(RecoveryRoot + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new IOException("잘못된 복구 경로");
            var journal = ReadJournal(path); if (journal.state == "complete" || journal.state == "restored") return;
            var folder = System.IO.Path.GetDirectoryName(path);
            // Verify every target before restoring anything; never clobber outside modifications.
            foreach (var file in journal.files)
            {
                if (!Documents.Any(d => d.Topic.Key == file.topic && d.Path == file.target) || !File.Exists(file.target)) throw new IOException("복구 대상 변경/삭제: " + file.target);
                if (!System.IO.Path.GetFullPath(file.original).StartsWith(folder + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new IOException("잘못된 백업 경로");
                var current = BalanceDocument.Digest(File.ReadAllBytes(file.target));
                if (current != file.before && current != file.after) throw new IOException("복구 대상 외부 수정: " + file.target);
                if (!File.Exists(file.original) || BalanceDocument.Digest(File.ReadAllBytes(file.original)) != file.before) throw new IOException("원본 복구본 손상: " + file.original);
            }
            journal.state = "restoring"; WriteJournal(path, journal);
            foreach (var file in journal.files)
            {
                var current = BalanceDocument.Digest(File.ReadAllBytes(file.target));
                if (current == file.before) continue;
                if (current != file.after) throw new IOException("복구 중 외부 수정: " + file.target);
                AtomicWrite(file.target, File.ReadAllBytes(file.original), folder);
            }
            reimport(); validate(ReadTexts());
            foreach (var file in journal.files) if (BalanceDocument.Digest(File.ReadAllBytes(file.target)) != file.before) throw new IOException("복구 검증 실패: " + file.target);
            journal.state = "restored"; WriteJournal(path, journal); CleanupSnapshots(journal);
        }
        private static void CleanupSnapshots(Journal journal)
        {
            // Completion is already durable. Cleanup failure must not be reported as a failed save/rollback.
            foreach (var file in journal.files)
            foreach (var path in new[] { file.original, file.candidate })
                try { File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        private static void DurableWrite(string path, byte[] bytes)
        { using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); } }
        private static void AtomicWrite(string target, byte[] bytes, string folder)
        {
            var stage = System.IO.Path.Combine(folder, Guid.NewGuid().ToString("N") + ".tmp");
            try { DurableWrite(stage, bytes); if (File.Exists(target)) File.Replace(stage, target, null); else File.Move(stage, target); }
            finally { if (File.Exists(stage)) File.Delete(stage); }
        }
        private static Journal ReadJournal(string path) => JsonUtility.FromJson<Journal>(File.ReadAllText(path)) ?? throw new IOException("복구 기록 손상");
        private static void WriteJournal(string path, Journal journal) => AtomicWrite(path, Encoding.UTF8.GetBytes(JsonUtility.ToJson(journal, true)), System.IO.Path.GetDirectoryName(path));
        [Serializable] private sealed class Journal { public string state; public List<RecoveryFile> files = new List<RecoveryFile>(); }
        [Serializable] private sealed class RecoveryFile { public string topic, target, original, candidate, before, after; }
    }
}
