using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocalizedDialogs.Editor
{
    public class LocalizedDialogsEditorWindow : EditorWindow
    {
        public VisualTreeAsset DialogUXML;
        public VisualTreeAsset AnswerUXML;

        private LocalizedDialogs _dialog;
        private LocalizedDialogsEditorGraph _dialogsGraph;

        [MenuItem("Game/Dialogs Editor")]
        public static void ShowExample()
        {
            var window = GetWindow<LocalizedDialogsEditorWindow>();
            if(ScriptableObjectExtensions.TryLoadSO<LocalizedDialogs>(out var dialog))
            {
                window.SetDialog(dialog);
            }
            window.minSize = new Vector2(850, 500);
        }

        public void SetDialog(LocalizedDialogs dialog)
        {
            _dialog = dialog;
            if(_dialog == default)
            {
                titleContent = new GUIContent($"Dialogs editor");
                _dialogsGraph.DeleteElements(_dialogsGraph.nodes);
            }
            else
            {
                titleContent = new GUIContent($"Dialogs editor [{_dialog.name}]");
                _dialogsGraph.Show(_dialog);
            }
        }
        

        public void CreateGUI()
        {
            var root = rootVisualElement;

            _dialogsGraph = new LocalizedDialogsEditorGraph(DialogUXML, AnswerUXML);
            root.Add(_dialogsGraph);
            if(_dialog != default)
            {
                SetDialog(_dialog);
            }
        }
        
        private void OnSelectionChange()
        {
            var guids = Selection.assetGUIDs;
            if(guids.Length > 0)
            {
                var guid = guids[0];
                if(ScriptableObjectExtensions.TryLoadSO<LocalizedDialogs>(guid, out var data))
                {
                    SetDialog(data);
                }
            }
        }
    }
}