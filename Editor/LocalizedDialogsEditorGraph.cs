using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace LocalizedDialogs.Editor
{
    public class LocalizedDialogsEditorGraph : GraphView
    {
        public VisualTreeAsset DialogUXML;
        public VisualTreeAsset AnswerUXML;

        private LocalizedDialogs _dialog;
        private List<LocalizedDialogNode> _nodes;


        public LocalizedDialogsEditorGraph(VisualTreeAsset dialogUxml, VisualTreeAsset answerUxml)
        {
            DialogUXML = dialogUxml;
            AnswerUXML = answerUxml;
            AddManipulators();
            AddGridBackground();
            SetGraphStyle();
            _nodes = new();

            this.graphViewChanged += OnGraphViewChanged;
        }
        
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if(graphViewChange.edgesToCreate != default)
            {
                foreach(var edge in graphViewChange.edgesToCreate)
                {
                    var input = edge.input.node as LocalizedDialogNode;
                    var output = edge.output.node as LocalizedDialogNode;

                    var answerId = output.GetAnswerIdByPort(edge.output);
                    var inputPartId = _dialog.GetEntryId(input._dialogView.DialogEntry.Guid);
                    var outputPartId = _dialog.GetEntryId(output._dialogView.DialogEntry.Guid);
                    var part = _dialog.Entries[outputPartId];
                    var answer = part.Answers[answerId];
                    answer.NextDialogGuid = input._dialogView.DialogEntry.Guid;
                    part.Answers[answerId] = answer;
                    _dialog.Entries[outputPartId] = part;
                }
            }

            if(graphViewChange.elementsToRemove != default)
            {
                foreach(var el in graphViewChange.elementsToRemove)
                {
                    if(el is Edge edge)
                    {
                        RemoveEdge(edge);
                    }
                    else if(el is LocalizedDialogNode node)
                    {
                        RemoveNode(node);
                    }
                }
            }

            return graphViewChange;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 actualGraphPosition = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);
            evt.menu.AppendAction("Add", menu => CreateNewNode(actualGraphPosition));
        }

        public void Show(LocalizedDialogs dialogs)
        {
            RemoveAll();
            _dialog = dialogs;
            CreateNodes();
            CreateConnections();
        }


        private void CreateNodes()
        {
            foreach(var entry in _dialog.Entries)
            {
                var node = CreateNode(entry, entry.PositionInEditor);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort => endPort.direction != startPort.direction).ToList();
        }

        private LocalizedDialogNode CreateNode(LocalizedDialogsEntry entry, Vector2 position)
        {
            var node = new LocalizedDialogNode(DialogUXML, AnswerUXML);
            var pos = new Rect(position, new Vector2(999,999));
            node.SetPosition(pos);
            AddElement(node);
            node.SetDialog(_dialog, entry.Guid);
            _nodes.Add(node);
            node.PositionChanged += OnNodePositionChanged;
            node.Changed += NodeChangeListener;
            return node;
        }

        private void CreateConnections()
        {
            foreach(var node in _nodes)
            {
                for(var i = 0; i < node.AnswerPorts.Count; i++)
                {
                    var answerView = node._dialogView.AnswerElements[i];
                    var entryId = _dialog.GetEntryId(node._dialogView.Guid);
                    var answer = _dialog.Entries[entryId].Answers[answerView.Id];
                    var targetNode = GetDialogNode(answer.NextDialogGuid);
                    if(targetNode != default)
                    {
                        var edge = node.AnswerPorts[i].ConnectTo(targetNode.Input);
                        AddElement(edge);
                    }
                }
            }
        }

        private void CreateNewNode(Vector2 actualGraphPosition)
        {
            var entry = new LocalizedDialogsEntry();
            entry.PositionInEditor = actualGraphPosition;
            entry.Answers = new();
            entry.Audio = new LocalizedAudioClip();
            entry.Text = new LocalizedString();
            entry.Guid = _dialog.Entries.Count == 0 ? 1 :_dialog.Entries.Max(e => e.Guid) + 1;

            _dialog.Entries.Add(entry);
            var node = CreateNode(entry, entry.PositionInEditor);
            EditorUtility.SetDirty(_dialog);
        }

        private void NodeChangeListener(LocalizedDialogNode node)
        {
            node.CreatePorts();
            ConnectAnswers(node);
        }

        private void ConnectAnswers(LocalizedDialogNode node)
        {
            var entry = node._dialogView.DialogEntry;
            for(var i = 0; i < entry.Answers.Count; i++)
            {
                var targetNode = GetDialogNode(entry.Answers[i].NextDialogGuid);
                if(targetNode != default)
                {
                    var output = node.AnswerPorts[i];
                    var input = targetNode.Input;
                    var connection = output.ConnectTo(input);
                    AddElement(connection);
                }
            }
        }

        private void RemoveEdge(Edge edge)
        {
            var from = edge.output.node as LocalizedDialogNode;
            var answerId = from.GetAnswerIdByPort(edge.output);
            var dialogEntry = from._dialogView.DialogEntry;
            dialogEntry.Answers[answerId].NextDialogGuid = -1;
            RemoveElement(edge);
        }

        private void RemoveNode(Node node)
        {
            var dialogNode = node as LocalizedDialogNode;
            _nodes.Remove(dialogNode);
            
            var entryId = _dialog.GetEntryId(dialogNode._dialogView.Guid);
            _dialog.Entries.RemoveAt(entryId);

            foreach(var n in _nodes)
            {
                var entry = n._dialogView.DialogEntry;
                for(var i = 0; i < entry.Answers.Count; i++)
                {
                    if(entry.Answers[i].NextDialogGuid == entry.Guid)
                    {
                        ConnectAnswers(n);
                    }
                }
            }
        }

        private void OnNodePositionChanged(LocalizedDialogNode node, Rect rect)
        {
            var entryId = _dialog.GetEntryId(node.EntryGuid);
            var entry = _dialog.Entries[entryId];
            entry.PositionInEditor = rect.position;
            _dialog.Entries[entryId] = entry;
            EditorUtility.SetDirty(_dialog);
        }

        private void RemoveAll()
        {
            var allEdges = edges;
            foreach(var edge in allEdges)
            {
                RemoveElement(edge);
            }

            foreach(var node in _nodes)
            {
                if(node.parent != default)
                {
                    node.parent.Remove(node);
                }
            }
            _nodes.Clear();
        }

        private LocalizedDialogNode GetNodeByPort(Port port)
        {
            foreach(var node in _nodes)
            {
                if(node.Input == port)
                {
                    return node;
                }
            }

            return default;
        }

        private LocalizedDialogNode GetDialogNode(int guid)
        {
            foreach(var node in _nodes)
            {
                if(node.EntryGuid == guid)
                {
                    return node;
                }
            }

            return default;
        }

        private void SetGraphStyle()
        {
            var h = style.height;
            var v = h.value;
            v.unit = LengthUnit.Percent;
            v.value = 100;
            h.value = v;
            style.height = h;
        }

        private void AddGridBackground()
        {
            var grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);
        }

        private void AddManipulators()
        {
            this.AddManipulator(new ContentZoomer()
            {
                minScale = 0.1f,
                maxScale = 1f
            });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }
    }
}