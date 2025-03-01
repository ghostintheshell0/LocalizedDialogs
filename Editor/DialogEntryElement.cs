using System;
using UnityEditor;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;

namespace LocalizedDialogs.Editor
{
    public class DialogEntryElement : VisualElement
    {
        public static string PickedAnswerClass = "picked-answer";
        public event Action<DialogEntryElement> AnswersChanged;
        public int Guid;
        public VisualElement Root;
        public VisualElement MessagesContainer;
        public LocalizedStringElement LocalizedMessage;
        public Label Message;
        public VisualElement AnswersContainer;
        public LocalizedElement<LocalizedAudioClip> LocalizedAudioClip;
        private LocalizedDialogs _data = default;
        public VisualTreeAsset AnswerUXML;
        private SerializedObject _serializedObject;
        private SerializedProperty _serializedEntries;
        private SerializedProperty _serializedAnswers;
        private AnswerElement _pickedAnswer;
        public VisualElement AddAnswer;
        public VisualElement RemoveAnswer;

        private List<AnswerElement> _answerElements = new();

        public DialogEntryElement(VisualElement tree, VisualTreeAsset answerUxml)
        {
            AnswerUXML = answerUxml;
            Root = tree.Q<VisualElement>("Window");
            MessagesContainer = Root.Q<VisualElement>("MessageContainer");
            AnswersContainer = Root.Q<VisualElement>("AnswersContainer");
            Message = Root.Q<Label>("DialogMessage");
            AddAnswer = Root.Q<VisualElement>("AddAnswer");
            AddAnswer.RegisterCallback<PointerDownEvent>(AddAnswerListener);
            RemoveAnswer = Root.Q<VisualElement>("RemoveAnswer");
            RemoveAnswer.RegisterCallback<PointerDownEvent>(RemoveAnswerListener);
            Add(tree);
        }

        public void Show(LocalizedDialogs data, int guid, SerializedObject serializedObject)
        {
            Guid = guid;
            _data = data;
            _serializedObject = serializedObject;
            var arrayId = _data.GetEntryId(guid);
            _serializedEntries = serializedObject.FindProperty(nameof(LocalizedDialogs.Entries));
            var entry = _serializedEntries.GetArrayElementAtIndex(arrayId);
            _serializedAnswers = entry.FindPropertyRelative(nameof(LocalizedDialogsEntry.Answers));
            
            CreateLocalizedMessage();
            CreateLocalizedAudioClip();

            var dialogEntry = DialogEntry;
            Message.text = dialogEntry.Text.GetLocalizedEditorString();
            CreateAnswers(dialogEntry);
            UpdateSortingButtons();
        }

        private void CreateLocalizedMessage()
        {
            if(LocalizedMessage != default)
            {
                LocalizedMessage.Changed -= DialogStringChanged;
                LocalizedMessage.parent.Remove(LocalizedMessage);
            }

            var arrayId = _data.GetEntryId(Guid);
            var dialogEntry = _serializedEntries.GetArrayElementAtIndex(arrayId);
            var dialogMessageProperty = dialogEntry.FindPropertyRelative(nameof(LocalizedDialogsEntry.Text));
            LocalizedMessage = LocalizedStringElement.Create(_serializedObject, dialogMessageProperty);
            LocalizedMessage.Changed += DialogStringChanged;
            MessagesContainer.Insert(0, LocalizedMessage);
        }


        private void CreateLocalizedAudioClip()
        {
            if(LocalizedAudioClip != default)
            {
                LocalizedAudioClip.parent.Remove(LocalizedAudioClip);
            }

            var arrayId = _data.GetEntryId(Guid);
            var dialogEntry = _serializedEntries.GetArrayElementAtIndex(arrayId);
            var dialogAudioProperty = dialogEntry.FindPropertyRelative(nameof(LocalizedDialogsEntry.Audio));

            LocalizedAudioClip = LocalizedElement<LocalizedAudioClip>.Create(_serializedObject, dialogAudioProperty);
            MessagesContainer.Add(LocalizedAudioClip);
        }

        private void CreateAnswers(in LocalizedDialogsEntry dialogsEntry)
        {
            for(var i = 0; i < dialogsEntry.Answers.Count; i++)
            {
                var answerElement = CreateAnswer();
                answerElement.Id = i;
                AnswersContainer.Add(answerElement);
                var serializedAnswer = _serializedAnswers.GetArrayElementAtIndex(i);
                answerElement.Show(_data, DialogEntry, _serializedObject, serializedAnswer);
                answerElement.Changed += AnswerChangeListener;
           }
        }

        private void AddAnswerListener(PointerDownEvent evt)
        {
            var dialogEntry = DialogEntry;
            var id = dialogEntry.Answers.Count;

            var answer = new LocalizedDialogAnswer();
            answer.Text = new LocalizedString();
            dialogEntry.Answers.Add(answer);
            var answerElement = CreateAnswer();
            answerElement.Id = id;
            var serializedAnswers = GetSerializedAnswersProperty();
            EditorUtility.SetDirty(_data);
            _serializedObject.Update();
            var serializedAnswer = serializedAnswers.GetArrayElementAtIndex(id);
            answerElement.Show(_data, DialogEntry, _serializedObject, serializedAnswer);
            AnswersContainer.Add(answerElement);
            AnswersChanged?.Invoke(this);
            UpdateSortingButtons();
        }

        private SerializedProperty GetSerializedAnswersProperty()
        {
            var arrayId = _data.GetEntryId(Guid);
            var serializedEntries = _serializedObject.FindProperty(nameof(LocalizedDialogs.Entries));
            var entry = serializedEntries.GetArrayElementAtIndex(arrayId);
            var serializedAnswers = entry.FindPropertyRelative(nameof(LocalizedDialogsEntry.Answers));
            return serializedAnswers;
        }

        private void RemoveAnswerListener(PointerDownEvent evt)
        {
            if(_pickedAnswer == default)
            {
                return;
            }

            var dialogEntry = DialogEntry;
            _pickedAnswer.OnDestroy();
            _pickedAnswer.RemoveFromClassList(PickedAnswerClass);
            _pickedAnswer.IconsContainer.RemoveFromClassList(PickedAnswerClass);
            dialogEntry.Answers.RemoveAt(_pickedAnswer.Id);
            _answerElements.RemoveAt(_pickedAnswer.Id);
            AnswersContainer.Remove(_pickedAnswer);
            _serializedObject.Update();
            for(var i = 0; i < dialogEntry.Answers.Count-1; i++)
            {
                _answerElements[i].Id = i;
            }

            _pickedAnswer = default;
            AnswersChanged?.Invoke(this);
        }

        private void AnswerChangeListener(AnswerElement answerElement)
        {
            var id = answerElement.Id;
            var entry = DialogEntry;
            entry.Answers[id].Visible = answerElement.VisibleConditionField.value as ScriptableObject;
            entry.Answers[id].Interactable = answerElement.InteractableConditionField.value as ScriptableObject;
            entry.Answers[id].Action = answerElement.ActionField.value as ScriptableObject;
            answerElement.RefreshIcons(entry.Answers[id]);
            EditorUtility.SetDirty(_data);
        }

        private AnswerElement CreateAnswer()
        {
            var tree = AnswerUXML.CloneTree();
            var ve = tree.hierarchy.ElementAt(0);
            var answer = new AnswerElement(ve);
            answer.Header.RegisterCallback<PointerDownEvent>(AnswerClickListener);
            answer.MoveUpClicked += AnswerUpListener;
            answer.MoveDownClicked += AnswerDownListener;
            _answerElements.Add(answer);
            return answer;
        }

        private void AnswerClickListener(PointerDownEvent evt)
        {
            if(evt.button != 0) return;
            
            if((evt.currentTarget as VisualElement).parent.parent is AnswerElement answerElement)
            {
                if(evt.target != answerElement.Header) return;
                SetSelectedAnswer(answerElement);
            }
        }

        private void SetSelectedAnswer(AnswerElement answerView)
        {
            if(_pickedAnswer != default)
            {
                _pickedAnswer.RemoveFromClassList(PickedAnswerClass);
                _pickedAnswer.IconsContainer.RemoveFromClassList(PickedAnswerClass);
            }
            _pickedAnswer = answerView;
            if(_pickedAnswer != default)
            {
                _pickedAnswer.AddToClassList(PickedAnswerClass);
                _pickedAnswer.IconsContainer.AddToClassList(PickedAnswerClass);
                _pickedAnswer.IsPopupShowed = !_pickedAnswer.IsPopupShowed;
            }
        }

        private void AnswerUpListener(AnswerElement answerView)
        {
            SwapAnswers(answerView.Id, answerView.Id - 1);
            UpdateSortingButtons();
            AnswersChanged?.Invoke(this);
            EditorUtility.SetDirty(_data);
        }

        private void AnswerDownListener(AnswerElement answerView)
        {
            SwapAnswers(answerView.Id, answerView.Id + 1);
            UpdateSortingButtons();
            AnswersChanged?.Invoke(this);
            EditorUtility.SetDirty(_data);
        }

        private void SwapAnswers(int a, int b)
        {
            var entryId = _data.GetEntryId(Guid);
            var entry = _data.Entries[entryId];
            var tempAnswer = entry.Answers[a];
            entry.Answers[a] = entry.Answers[b];
            entry.Answers[b] = tempAnswer;
            var tempAnswerView = _answerElements[a];
            _answerElements[a] = _answerElements[b];
            _answerElements[b] = tempAnswerView;
            AnswersContainer.Clear();
            _answerElements[a].Id = a;
            _answerElements[b].Id = b;

            _answerElements[a].AnswerId.text = a.ToString();
            _answerElements[b].AnswerId.text = b.ToString();
            
            for(var i = 0; i < _answerElements.Count; i++)
            {
                AnswersContainer.Add(_answerElements[i]);
            }
        }

        private void UpdateSortingButtons()
        {
            for(int i = 0; i < _answerElements.Count; i++)
            {
                _answerElements[i].MoveUpButton.SetEnabled(i != 0);
                _answerElements[i].MoveDownButton.SetEnabled(i != _answerElements.Count -1);
            }
        }

        public void DialogStringChanged(LocalizedStringElement element)
        {
            Message.text = DialogEntry.Text.GetLocalizedEditorString();
        }

        public AnswerElement GetAnswerElement(int id)
        {
            return _answerElements[id];
        }

        public LocalizedDialogsEntry DialogEntry
        {
            get
            {
                var arrayId = _data.GetEntryId(Guid);
                return _data.Entries[arrayId];
            }
        }

        public AnswerElement PickedAnswer
        {
            get => _pickedAnswer;
            set
            {
                SetSelectedAnswer(value);
            }
        }

        public IReadOnlyList<AnswerElement> AnswerElements => _answerElements; 
    }
}