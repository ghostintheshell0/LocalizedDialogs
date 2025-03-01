using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace LocalizedDialogs.Editor
{
    public class AnswerElement : VisualElement
    {
        public static string DefaultAnswerText = "-=NOTHING=-";
        public event Action<AnswerElement> Changed;
        public event Action<AnswerElement> MoveUpClicked;
        public event Action<AnswerElement> MoveDownClicked;
        public int Id;
        public VisualElement Header;
        public VisualElement PopupOpenButton;
        public VisualElement PopupCloseButton;
        public VisualElement Popup;
        public LocalizedStringElement LocalizedStringElement;
        public Label AnswerId;
        public Label AnswerLabel;
        public VisualElement InteractableIcon;
        public VisualElement VisibleIcon;
        public VisualElement ActionIcon;
        private bool _popupShowed;
        public ObjectField ActionField;
        public ObjectField VisibleConditionField;
        public ObjectField InteractableConditionField;
        public VisualElement IconsContainer;
        private LocalizedDialogsEntry _entry;
        public VisualElement MoveUpButton;
        public VisualElement MoveDownButton;

        public AnswerElement(VisualElement tree)
        {
            Add(tree);
            AnswerLabel = tree.Q<Label>("AnswerLabel");
            AnswerId = tree.Q<Label>("Id");
            Header = tree.Q<VisualElement>("Header");
            Popup = tree.Q<VisualElement>("Popup");
            PopupOpenButton = tree.Q<VisualElement>("PopupOpenButton");
            PopupCloseButton = tree.Q<VisualElement>("PopupCloseButton");
            VisibleConditionField = tree.Q<ObjectField>("Visible");
            VisibleConditionField.RegisterValueChangedCallback(PropertyChangeListener);
            InteractableConditionField = tree.Q<ObjectField>("Interactable");
            InteractableConditionField.RegisterValueChangedCallback(PropertyChangeListener);
            ActionField = tree.Q<ObjectField>("Action");
            ActionField.RegisterValueChangedCallback(PropertyChangeListener);
            VisibleIcon = tree.Q<VisualElement>("VisibleIcon");
            InteractableIcon = tree.Q<VisualElement>("InteractableIcon");
            ActionIcon = tree.Q<VisualElement>("ActionIcon");
            IconsContainer = tree.Q<VisualElement>("IconsContainer");
            MoveUpButton = tree.Q<VisualElement>("MoveUp");
            MoveUpButton.RegisterCallback<PointerDownEvent>(MoveUpListener);
            MoveDownButton = tree.Q<VisualElement>("MoveDown");
            MoveDownButton.RegisterCallback<PointerDownEvent>(MoveDownListener);
        }

        public void Show(LocalizedDialogs dialog, LocalizedDialogsEntry data, SerializedObject serializedTarget, SerializedProperty serializedAnswer)
        {
            _entry = data;
            AnswerId.text = $"{Id}.";
            var answerData = data.Answers[Id];
            if(answerData == default)
            {
               return;
            }

            if(LocalizedStringElement != default)
            {
                LocalizedStringElement.Changed -= LocalizedStringChangeListener;
                if(LocalizedStringElement.parent != default)
                {
                    LocalizedStringElement.parent.Remove(LocalizedStringElement);
                }
            }

            var serializedAnswerText = serializedAnswer.FindPropertyRelative(nameof(LocalizedDialogAnswer.Text));
            LocalizedStringElement = LocalizedStringElement.Create(serializedTarget, serializedAnswerText);
            Popup.Insert(0, LocalizedStringElement);
            LocalizedStringElement.Changed += LocalizedStringChangeListener;
            
            var answerText = answerData.Text.GetLocalizedEditorString();
            AnswerLabel.text = answerText != default ? answerText : DefaultAnswerText;
            InteractableConditionField.SetValueWithoutNotify(answerData.Interactable);
            VisibleConditionField.SetValueWithoutNotify(answerData.Visible);
            ActionField.SetValueWithoutNotify(answerData.Action);
            RefreshIcons(answerData);
        }

        public void OpenPopup()
        {
            _popupShowed = true;
            Popup.style.display = DisplayStyle.Flex;
            PopupCloseButton.style.display = DisplayStyle.Flex;
            PopupOpenButton.style.display = DisplayStyle.None;
        }

        public void RefreshIcons(LocalizedDialogAnswer answerData)
        {
            InteractableIcon.style.visibility = answerData.Interactable == default ? Visibility.Hidden : Visibility.Visible;
            VisibleIcon.style.visibility = answerData.Visible == default ? Visibility.Hidden : Visibility.Visible;
            ActionIcon.style.visibility = answerData.Action == default ? Visibility.Hidden : Visibility.Visible;
        }

        public void ClosePopup()
        {
            _popupShowed = false;
            Popup.style.display = DisplayStyle.None;
            PopupCloseButton.style.display = DisplayStyle.None;
            PopupOpenButton.style.display = DisplayStyle.Flex;
        }

        private void MoveUpListener(PointerDownEvent evt)
        {
            MoveUpClicked?.Invoke(this);
        }

        private void MoveDownListener(PointerDownEvent evt)
        {
            MoveDownClicked?.Invoke(this);
        }

        public void PropertyChangeListener(ChangeEvent<UnityEngine.Object> evt)
        {
            Changed?.Invoke(this);
        }
        public void LocalizedStringChangeListener(LocalizedStringElement evt)
        {
            var answerText = _entry.Answers[Id].Text.GetLocalizedEditorString();
            AnswerLabel.text = answerText != default ? answerText : DefaultAnswerText;
            Changed?.Invoke(this);
        }

        public void OnDestroy()
        {
            if(LocalizedStringElement != default)
            {
                LocalizedStringElement.Changed -= LocalizedStringChangeListener;
                LocalizedStringElement.OnDestroy();
            }
        }

        public bool IsPopupShowed
        {
            get => _popupShowed;
            set
            {
                if(_popupShowed == value) return;
                _popupShowed = value;
                if(_popupShowed)
                {
                    OpenPopup();
                }
                else
                {
                    ClosePopup();
                }
            }
        }
    }
}