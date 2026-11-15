using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MobileGame.Foundation.Editor
{
    public sealed class BalanceEditorWindow : EditorWindow
    {
        [SerializeField] private BalanceEditSession session = new BalanceEditSession();
        [SerializeField] private string selectedTopic = "facilities", selectedId = "", search = "";
        [SerializeField] private bool diagnostics;
        private BalanceDocumentStore store;
        private Label notice;
        private VisualElement body;
        private string message = "";
        public BalanceEditSession Session => session;
        public string SelectedId => selectedId;
        public string SelectedTopic => selectedTopic;

        [MenuItem("Tools/Mobile Game/Balance/Open Balance Editor")]
        public static void Open() => GetWindow<BalanceEditorWindow>("게임 밸런스").Show();
        public static BalanceEditorWindow OpenDefinition(ThingDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            var window = GetWindow<BalanceEditorWindow>("게임 밸런스");
            window.Load(false);
            var matches = window.store.Documents.SelectMany(d => d.Entries.Select(e => (d, e)))
                .Where(x => x.e.Fields["definitionId"].StringValue == definition.DefinitionId).ToArray();
            if (matches.Length != 1 || !MatchesKind(matches[0].d.Topic.DtoType, definition)) throw new InvalidOperationException("SO 종류/ID와 JSON이 일치하지 않거나 중복입니다: " + definition.DefinitionId);
            window.selectedTopic = matches[0].d.Topic.Key; window.selectedId = definition.DefinitionId; window.search = "";
            window.Show(); window.Focus(); window.CreateGUI(); return window;
        }
        private static bool MatchesKind(Type dto, ThingDefinition definition) => definition.Kind == ThingKind.Facility ? dto == typeof(FacilityDto)
            : definition.Kind == ThingKind.Creature ? dto == typeof(CreatureDto) : definition.Kind == ThingKind.Resource && dto == typeof(ResourceDto);
        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndo;
            EditorApplication.playModeStateChanged += PlayChanged;
            EditorApplication.wantsToQuit += WantsQuit;
            saveChangesMessage = "JSON에 저장하지 않은 변경이 있습니다. 저장·버리기·취소를 선택하세요.";
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo; EditorApplication.playModeStateChanged -= PlayChanged;
            EditorApplication.wantsToQuit -= WantsQuit;
        }
        private void Load(bool discard)
        {
            store = BalanceEditorData.Load();
            if (discard || !session.Dirty) session.Reset(store.Documents);
            hasUnsavedChanges = session.Dirty;
        }
        public void CreateGUI()
        {
            rootVisualElement.Clear(); minSize = new Vector2(780, 430);
            try { Load(false); } catch (Exception e) { rootVisualElement.Add(new HelpBox(e.Message, HelpBoxMessageType.Error)); return; }
            var toolbar = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            AddButton(toolbar, "검증", ValidateDraft); AddButton(toolbar, "JSON 저장", SaveDraft);
            AddButton(toolbar, "다시 읽기 / 버리기", ReloadRequested);
            AddButton(toolbar, "Undo", Undo.PerformUndo); AddButton(toolbar, "Redo", Undo.PerformRedo);
            var diagnostic = new Toggle("개발자 진단") { value = diagnostics };
            diagnostic.RegisterValueChangedCallback(e => { diagnostics = e.newValue; Render(); }); toolbar.Add(diagnostic);
            rootVisualElement.Add(toolbar);
            var guidance = new Label("게임 밸런스 수치 전용 · 저장 후 Play를 다시 시작하면 적용 · 변경 시 기존 저장과 호환되지 않을 수 있습니다.") { style = { whiteSpace = WhiteSpace.Normal } };
            rootVisualElement.Add(guidance);
            notice = new Label { style = { whiteSpace = WhiteSpace.Normal, marginTop = 6, marginBottom = 6 } }; rootVisualElement.Add(notice);
            var tabs = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            foreach (var topic in JsonDefinitionTopics.All) AddButton(tabs, topic.Name, () => { selectedTopic = topic.Key; selectedId = ""; Render(); });
            rootVisualElement.Add(tabs);
            var filter = new TextField("이름 / ID 검색") { value = search };
            filter.RegisterValueChangedCallback(e => { search = e.newValue; Render(); }); rootVisualElement.Add(filter);
            body = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } }; rootVisualElement.Add(body);
            Render();
        }
        private static void AddButton(VisualElement parent, string text, Action callback) => parent.Add(new Button(callback) { text = text });
        private static string Id(BalanceJsonToken entry) => entry.Fields["definitionId"].StringValue;
        private static string Name(BalanceJsonToken entry) => entry.Fields.TryGetValue("displayNameKey", out var name) ? name.StringValue : Id(entry);
        private bool ReadOnly => EditorApplication.isPlayingOrWillChangePlaymode;
        private void Render()
        {
            if (body == null || store == null) return;
            body.Clear(); hasUnsavedChanges = session.Dirty;
            var errors = session.Conflicts(store.Documents);
            try { store.EnsureUnchanged(); BalanceEditorData.CheckCurrentBindings(); } catch (Exception e) { errors.Add(e.Message); }
            var pending = store.Pending().ToArray();
            notice.text = (ReadOnly ? "Play 중: 조회 전용\n" : "") + "미저장 변경 " + session.changes.Count + "개\n" + message + (errors.Count > 0 ? "\n" + string.Join("\n", errors) : "");
            var document = store.Documents.FirstOrDefault(d => d.Topic.Key == selectedTopic) ?? store.Documents.First(); selectedTopic = document.Topic.Key;
            var list = new ScrollView { style = { width = 330, flexShrink = 0 } }; body.Add(list);
            foreach (var entry in document.Entries.Where(e => (Id(e) + " " + Name(e)).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var id = Id(entry); AddButton(list, id == selectedId ? "▶ " + id : id, () => { selectedId = id; Render(); });
                var values = BalanceMetadataReader.Read(document.Topic.DtoType).Where(f => f.Diagnostic(entry) == "편집 가능")
                    .Select(f => f.Attribute.Name + " " + Current(document, entry, f) + " " + f.Attribute.Unit);
                list.Add(new Label(string.Join(" · ", values)) { style = { whiteSpace = WhiteSpace.Normal, marginBottom = 12 } });
            }
            var detail = new ScrollView { style = { flexGrow = 1, paddingLeft = 12 } }; body.Add(detail);
            foreach (var path in pending)
            {
                detail.Add(new HelpBox("미완료 저장 기록: " + path, HelpBoxMessageType.Warning));
                AddButton(detail, "원본 복구", () => Run(() => { if (ReadOnly) throw new InvalidOperationException("Play를 종료하세요."); store.Recover(path); Load(false); }));
            }
            var chosen = document.Entries.FirstOrDefault(e => Id(e) == selectedId);
            if (chosen == null) { detail.Add(new Label("왼쪽 목록에서 항목을 선택하세요.")); return; }
            detail.Add(new Label(selectedId) { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16, marginBottom = 10 } });
            var count = 0;
            foreach (var field in BalanceMetadataReader.Read(document.Topic.DtoType))
            {
                var diagnostic = field.Diagnostic(chosen);
                if (diagnostic == "편집 가능")
                {
                    count++; var original = chosen.Fields[field.Field.Name].Raw;
                    // Text input preserves Int64 and exact decimal/exponent tokens; type validation runs before save.
                    var input = new TextField(field.Attribute.Name + (field.Attribute.Unit == "" ? "" : " (" + field.Attribute.Unit + ")"))
                    { value = Current(document, chosen, field), isDelayed = true, tooltip = field.Attribute.Description + "\n" + field.Field.Name };
                    input.SetEnabled(!ReadOnly && errors.Count == 0 && pending.Length == 0);
                    input.RegisterValueChangedCallback(e => Edit(document, chosen, field, e.newValue));
                    detail.Add(input); detail.Add(new Label("저장 값: " + original + " · " + field.Attribute.Description) { style = { whiteSpace = WhiteSpace.Normal, marginBottom = 8 } });
                }
                if (diagnostics)
                {
                    chosen.Fields.TryGetValue(field.Field.Name, out var token);
                    detail.Add(new Label(document.Topic.DtoType.Name + "." + field.Field.Name + " [" + field.Field.FieldType.Name + "] " + diagnostic
                        + " · 값=" + (token?.Raw ?? "생략") + (field.Attribute == null ? "" : " · 조건=" + field.Attribute.ConditionField + ":" + string.Join(",", field.Attribute.ConditionValues)))
                    { style = { whiteSpace = WhiteSpace.Normal, marginBottom = 5 } });
                }
            }
            if (count == 0) detail.Add(new HelpBox("이 항목에는 현재 편집 가능한 밸런스 수치가 없습니다. 조회 전용입니다.", HelpBoxMessageType.Info));
            detail.Add(new Label("변경 내역") { style = { marginTop = 12, unityFontStyleAndWeight = FontStyle.Bold } });
            foreach (var change in session.changes) detail.Add(new Label(change.topic + "/" + change.id + "/" + change.field + ": " + change.original + " → " + change.value) { style = { whiteSpace = WhiteSpace.Normal } });
        }
        private string Current(BalanceDocument doc, BalanceJsonToken entry, BalanceFieldInfo field) => session.changes
            .FirstOrDefault(c => c.topic == doc.Topic.Key && c.id == Id(entry) && c.field == field.Field.Name)?.value ?? entry.Fields[field.Field.Name].Raw;
        private void Edit(BalanceDocument document, BalanceJsonToken entry, BalanceFieldInfo field, string value)
        {
            Run(() => { if (ReadOnly) throw new InvalidOperationException("Play 중 편집할 수 없습니다."); Undo.RecordObject(this, "밸런스 수치 수정");
                session.Set(document, Id(entry), field, value.Trim()); EditorUtility.SetDirty(this); message = "초안에 반영했습니다. 검증 후 저장하세요."; });
        }
        public void ValidateDraft() => Run(() => { store.Validate(session); message = "전체 JSON·Registry·실행 파일 검증 통과"; });
        public void SaveDraft()
        {
            Run(() =>
            {
                if (ReadOnly) throw new InvalidOperationException("Play를 종료한 뒤 저장하세요.");
                AssetDatabase.DisallowAutoRefresh();
                try { store.Save(session); } finally { AssetDatabase.AllowAutoRefresh(); }
                Load(true); Undo.ClearUndo(this); message = "JSON 저장·재임포트·최종 검증 완료. 다음 Play에 적용됩니다.";
            });
        }
        public override void SaveChanges() { SaveDraft(); if (!session.Dirty) base.SaveChanges(); else throw new InvalidOperationException("저장되지 않았습니다. 오류를 확인하세요."); }
        public override void DiscardChanges() { Load(true); message = "초안을 버리고 원본을 다시 읽었습니다."; Undo.ClearUndo(this); base.DiscardChanges(); Render(); }
        private void ReloadRequested()
        {
            if (!session.Dirty || EditorUtility.DisplayDialog("초안 버리기", "미저장 변경을 버리고 JSON을 다시 읽을까요?", "버리기", "취소")) Run(DiscardChanges);
        }
        private void Run(Action action)
        { try { action(); } catch (Exception e) { message = e.Message; } finally { hasUnsavedChanges = session.Dirty; Render(); } }
        private void OnUndo() { try { Load(false); Render(); } catch (Exception e) { message = e.Message; } }
        private bool ResolvePendingChanges()
        {
            if (!session.Dirty) return true;
            var choice = EditorUtility.DisplayDialogComplex("미저장 밸런스", "Play/종료 전에 초안을 처리하세요.", "저장", "취소", "버리기");
            if (choice == 1) return false;
            if (choice == 2) { DiscardChanges(); return true; }
            // Play transition is cancelled before the save, so normal editing guards remain effective.
            SaveDraft(); return !session.Dirty;
        }
        private bool WantsQuit() => ResolvePendingChanges();
        private void PlayChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && session.Dirty)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += () => { if (this != null && ResolvePendingChanges()) EditorApplication.isPlaying = true; };
            }
            Render();
        }
    }
}
