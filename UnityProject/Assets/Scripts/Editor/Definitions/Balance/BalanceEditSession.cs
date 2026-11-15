using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileGame.Foundation.Editor
{
    [Serializable]
    public sealed class BalanceChange
    {
        public string topic, id, field, signature, original, value;
    }
    [Serializable]
    public sealed class BalanceSourceSnapshot
    {
        public string topic, path, hash;
    }
    [Serializable]
    public sealed class BalanceEditSession
    {
        public List<BalanceSourceSnapshot> sources = new List<BalanceSourceSnapshot>();
        public List<BalanceChange> changes = new List<BalanceChange>();
        public bool Dirty => changes.Count > 0;
        public void Set(BalanceDocument document, string id, BalanceFieldInfo field, string value)
        {
            var entry = document.Entry(id);
            if (field.Diagnostic(entry) != "편집 가능") throw new InvalidOperationException(field.Diagnostic(entry));
            if (sources.Count == 0) throw new InvalidOperationException("초안 기준선이 없습니다.");
            var prior = changes.SingleOrDefault(c => c.topic == document.Topic.Key && c.id == id && c.field == field.Field.Name);
            if (prior != null) changes.Remove(prior);
            var original = entry.Fields[field.Field.Name].Raw;
            if (original != value) changes.Add(new BalanceChange { topic = document.Topic.Key, id = id, field = field.Field.Name,
                signature = field.Signature, original = original, value = value });
        }
        public void Reset(IEnumerable<BalanceDocument> documents)
        {
            changes.Clear(); sources = documents.Select(d => new BalanceSourceSnapshot { topic = d.Topic.Key, path = d.Path, hash = d.Hash }).ToList();
        }
        public List<string> Conflicts(IReadOnlyList<BalanceDocument> documents)
        {
            var errors = new List<string>();
            if (Dirty && (sources.Count != documents.Count || sources.Any(s => !documents.Any(d => d.Topic.Key == s.topic && d.Path == s.path && d.Hash == s.hash))))
                errors.Add("원본 파일 또는 등록이 변경되었습니다. 초안을 확인한 후 버리고 다시 읽으세요.");
            foreach (var change in changes)
            {
                var label = change.topic + "/" + change.id + "/" + change.field;
                try
                {
                    var document = documents.Single(d => d.Topic.Key == change.topic);
                    var entry = document.Entry(change.id);
                    var field = BalanceMetadataReader.Read(document.Topic.DtoType).Single(f => f.Field.Name == change.field);
                    if (field.Signature != change.signature || field.Diagnostic(entry) != "편집 가능"
                        || entry.Fields[change.field].Raw != change.original) errors.Add(label + ": 필드/자료형/조건/원본 충돌");
                }
                catch (Exception e) when (e is InvalidOperationException || e is KeyNotFoundException) { errors.Add(label + ": 대상이 없어졌습니다."); }
            }
            return errors;
        }
    }
}
