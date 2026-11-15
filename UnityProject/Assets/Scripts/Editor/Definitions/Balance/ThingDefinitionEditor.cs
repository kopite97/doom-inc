using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MobileGame.Foundation.Editor
{
    [CustomEditor(typeof(ThingDefinition), true), CanEditMultipleObjects]
    public sealed class ThingDefinitionEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement(); InspectorElement.FillDefaultInspector(root, serializedObject, this);
            var info = new HelpBox(targets.Length == 1 ? "게임 수치는 연결된 JSON에서 편집합니다." : "SO 하나를 선택해야 해당 밸런스 항목을 열 수 있습니다.", HelpBoxMessageType.Info);
            root.Add(info);
            var open = new Button(() => { try { BalanceEditorWindow.OpenDefinition((ThingDefinition)target); } catch (Exception e) { info.text = e.Message; info.messageType = HelpBoxMessageType.Error; } })
                { text = "밸런스 편집창에서 열기" };
            open.SetEnabled(targets.Length == 1); root.Add(open); return root;
        }
    }
}
