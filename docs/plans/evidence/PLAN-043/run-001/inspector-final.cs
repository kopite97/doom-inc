var w=UnityEditor.EditorWindow.GetWindow<MobileGame.Foundation.Editor.BalanceEditorWindow>();
w.DiscardChanges();
var missing=UnityEngine.ScriptableObject.CreateInstance<MobileGame.Foundation.FacilityDefinition>();
var rejected=false;
try { MobileGame.Foundation.Editor.BalanceEditorWindow.OpenDefinition(missing); }
catch(System.InvalidOperationException) { rejected=true; }
finally { UnityEngine.Object.DestroyImmediate(missing); }
if(!rejected) throw new System.Exception("Missing ID accepted");
w.Close();
UnityEditor.Selection.activeObject=UnityEditor.AssetDatabase.LoadAssetAtPath<MobileGame.Foundation.FacilityDefinition>("Assets/SO/Facility/Gathering.asset");
return new { missingIdRejected=rejected, playing=UnityEditor.EditorApplication.isPlaying };
