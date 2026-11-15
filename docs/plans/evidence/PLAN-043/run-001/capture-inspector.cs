var window=UnityEngine.Resources.FindObjectsOfTypeAll<UnityEditor.EditorWindow>().First(w=>w.GetType().Name=="InspectorWindow");
var rect=window.position;
var texture=new UnityEngine.Texture2D((int)rect.width,(int)rect.height,UnityEngine.TextureFormat.RGB24,false);
texture.SetPixels(UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(rect.position,(int)rect.width,(int)rect.height));texture.Apply();
System.IO.File.WriteAllBytes("C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-043/run-001/inspector-entry.png",texture.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(texture);
return new {rect.width,rect.height};
