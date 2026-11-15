var window = UnityEditor.EditorWindow.GetWindow<MobileGame.Foundation.Editor.BalanceEditorWindow>();
var rect = window.position;
var pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(rect.position, (int)rect.width, (int)rect.height);
var texture = new UnityEngine.Texture2D((int)rect.width, (int)rect.height, UnityEngine.TextureFormat.RGB24, false);
texture.SetPixels(pixels); texture.Apply();
System.IO.File.WriteAllBytes("C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-043/run-001/validation-error.png", texture.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(texture);
return new {rect.width,rect.height};

