var window=UnityEditor.EditorWindow.GetWindow<MobileGame.Foundation.Editor.BalanceEditorWindow>();
var so=new UnityEditor.SerializedObject(window);
so.FindProperty("selectedTopic").stringValue="invasion";
so.FindProperty("selectedId").stringValue="settings.invasion";
so.ApplyModifiedPropertiesWithoutUndo(); window.CreateGUI();
var field=UnityEngine.UIElements.UQueryExtensions.Query<UnityEngine.UIElements.TextField>(window.rootVisualElement).ToList().Single(f=>f.label=="목표 점수 (점)");
field.value="10";window.ValidateDraft();window.Focus();
return new {selected=window.SelectedId,dirty=window.Session.Dirty,labels=UnityEngine.UIElements.UQueryExtensions.Query<UnityEngine.UIElements.Label>(window.rootVisualElement).ToList().Where(l=>l.text.Contains("targetScore")).Select(l=>l.text).ToArray()};
