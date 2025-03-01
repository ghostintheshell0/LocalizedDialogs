using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocalizedDialogs.Editor
{
    public class LocalizedDialogNode : Node
    {   
        public event Action<LocalizedDialogNode> Changed;
        public event Action<LocalizedDialogNode, Rect> PositionChanged;
        private LocalizedDialogs _dialog;
        public int EntryGuid {get; private set;}
        public Port Input {get; private set;}
        public DialogEntryElement _dialogView;
        private VisualElement _answerPortsContainer;
        public List<Port> AnswerPorts;

        public LocalizedDialogNode(VisualTreeAsset treeAsset, VisualTreeAsset answerUxml) : base()
        {
            _dialogView = new DialogEntryElement(treeAsset.CloneTree(), answerUxml);
            _dialogView.AnswersChanged += OnAnswersChanged;
            _dialogView.style.width = 550;
            topContainer.Insert(1, _dialogView);
            AnswerPorts = new();
            _answerPortsContainer = new VisualElement();
            
            Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            Input.portName = "";
            inputContainer.Add(Input);
            
            outputContainer.style.flexDirection = FlexDirection.ColumnReverse;
            _answerPortsContainer.style.paddingBottom = 37f;
            _answerPortsContainer.style.flexGrow = 0;
            outputContainer.Add(_answerPortsContainer);
        }

        public void SetDialog(LocalizedDialogs dialogs, int guid)
        {
            EntryGuid = guid;
            _dialog = dialogs;
            _dialogView.Show(_dialog, guid, new SerializedObject(dialogs));
            title = $"Guid:{guid}; Array id:{dialogs.GetEntryId(guid)}";
            CreatePorts();
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            _dialogView.PickedAnswer = default;
        }

        public void CreatePorts()
        {
            var dialogEntry = _dialogView.DialogEntry;
            var missingPorts = _answerPortsContainer.childCount - dialogEntry.Answers.Count;
            DisconnectAllPorts();
            if(missingPorts == 0) return;
            if(missingPorts > 0)
            {
                for(var i = 0; i < missingPorts; i++)
                {
                    _answerPortsContainer.RemoveAt(AnswerPorts.Count -1);
                    AnswerPorts.RemoveAt(AnswerPorts.Count -1);
                }
            }
            else
            {
                missingPorts *= -1;
                for(var i = 0; i < missingPorts; i++)
                {
                    AnswerPorts.Add(CreateOutputPort());
                }
            }
        }

        private void DisconnectAllPorts()
        {
            foreach(var port in AnswerPorts)
            {
                foreach(var c in port.connections)
                {
                    c.parent.Remove(c);
                }
                port.DisconnectAll();
            }
        }

        private Port CreateOutputPort()
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            port.style.height = 34;
            port.portName = string.Empty;
            _answerPortsContainer.Add(port);
            return port;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            PositionChanged?.Invoke(this, newPos);
        }

        private void OnAnswersChanged(DialogEntryElement dialogView)
        {
            Changed?.Invoke(this);
        }

        public int GetAnswerIdByPort(Port port)
        {
            for(var i = 0; i < AnswerPorts.Count; i++)
            {
                if(AnswerPorts[i] == port)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}