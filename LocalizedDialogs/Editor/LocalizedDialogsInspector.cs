using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LocalizedDialogs.Editor
{
    [CustomEditor(typeof(LocalizedDialogs))]
    public class LocalizedDialogsInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var result = new VisualElement();
            InspectorElement.FillDefaultInspector(result, serializedObject, this);
            var button = new Button();
            button.text = "Show in editor";
            button.clicked += OnShow;
            result.Add(button);
            return result;
        }

        void OnShow()
        {
            var window = EditorWindow.GetWindow<LocalizedDialogsEditorWindow>();
            window.SetDialog((LocalizedDialogs)target);
        }
    }
}