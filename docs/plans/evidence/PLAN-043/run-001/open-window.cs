var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<MobileGame.Foundation.FacilityDefinition>("Assets/SO/Facility/Processing.asset");
var window = MobileGame.Foundation.Editor.BalanceEditorWindow.OpenDefinition(asset);
window.position = new UnityEngine.Rect(80, 80, 1100, 670);
window.Focus();
var fields = UnityEngine.UIElements.UQueryExtensions.Query<UnityEngine.UIElements.TextField>(window.rootVisualElement).ToList();
return new { id = window.SelectedId, fields = fields.Select(f => new { f.label, f.value, enabled=f.enabledInHierarchy }).ToArray() };
