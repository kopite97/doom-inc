UnityEditor.EditorSettings.enterPlayModeOptionsEnabled=true;
UnityEditor.EditorSettings.enterPlayModeOptions=UnityEditor.EnterPlayModeOptions.DisableDomainReload;
var window=MobileGame.Foundation.Editor.BalanceEditorWindow.OpenDefinition(UnityEditor.AssetDatabase.LoadAssetAtPath<MobileGame.Foundation.FacilityDefinition>("Assets/SO/Facility/Gathering.asset"));
var field=UnityEngine.UIElements.UQueryExtensions.Query<UnityEngine.UIElements.TextField>(window.rootVisualElement).ToList().Single(f=>f.label=="건설 시간 (초)");
field.value="3";
window.SaveDraft();
return new {seconds=MobileGame.Foundation.Editor.GameDataAssets.LoadCatalog().Selected<MobileGame.Foundation.FacilityData>("gatheringId").BuildSeconds,dirty=window.Session.Dirty,playing=UnityEditor.EditorApplication.isPlaying};
