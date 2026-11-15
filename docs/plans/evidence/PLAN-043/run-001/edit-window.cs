var window = MobileGame.Foundation.Editor.BalanceEditorWindow.OpenDefinition(UnityEditor.AssetDatabase.LoadAssetAtPath<MobileGame.Foundation.FacilityDefinition>("Assets/SO/Facility/Gathering.asset"));
var field = UnityEngine.UIElements.UQueryExtensions.Query<UnityEngine.UIElements.TextField>(window.rootVisualElement).ToList().Single(f=>f.label=="건설 시간 (초)");
field.value="6";
UnityEditor.Undo.FlushUndoRecordObjects();
return new { changes=window.Session.changes.Count, value=window.Session.changes.Single().value, dirty=window.hasUnsavedChanges };
