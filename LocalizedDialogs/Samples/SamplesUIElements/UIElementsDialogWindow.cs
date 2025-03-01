using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocalizedDialogs.Samples
{
    public class UIElementsDialogWindow : MonoBehaviour
    {
        public event Action Ended;
        public UIDocument Document;
        public string NpcNameSelector;
        public string NpcAvatarSelector;
        public string MessageSelector;
        public string AnswersContainerSelector;
        public string AnswerHoverClassName;
        public VisualTreeAsset AnswerUXML;
        public string AnswerMessageSelector;
        public AudioSource AudioSource;

        private Label _npcName;
        private VisualElement _npcAvatar;
        private Label _dialogMessage;
        private ScrollView _answersContainer;
        private List<UIElementsDialogAnswer> _answers = new();
        private List<UIElementsDialogAnswer> _pool = new();

        private LocalizedDialogs _dialogs;
        private int _currentGuid;
        private NPC _npc;
        private Player _player;
        private bool _inited;

        public void Show(Player player, NPC npc, LocalizedDialogs dialogs, int entryGuid)
        {
            _npc = npc;
            _player = player;
            if(_inited)
            {
                _npcName.text = npc.Name.GetLocalizedString();
                _npcAvatar.style.backgroundImage = new StyleBackground(npc.Avatar);
            }

            Show(dialogs, entryGuid);
        }

        public void Show(LocalizedDialogs dialogs, int entryGuid)
        {
            _dialogs = dialogs;
            _currentGuid = entryGuid;
            Refresh();
        }

        private void Refresh()
        {
            if(!_inited || TryEnd()) return;
            var id = _dialogs.GetEntryId(_currentGuid);
            var entry = _dialogs.Entries[id];
            
            _dialogMessage.text = FormatString(entry.Text.GetLocalizedString());
            
            if(!entry.Audio.IsEmpty)
            {
                var clip = entry.Audio.LoadAsset();
                AudioSource.clip = clip;
                AudioSource.Play();
            }

            RecycleAnswers();
            ShowAnswers(entry);
        }

        private void RecycleAnswers()
        {
            for(var i = 0; i < _answers.Count; i++)
            {
                _answersContainer.Remove(_answers[i]);
                _answers[i].RemoveFromClassList(_answers[i].HoverClassName);
                _pool.Add(_answers[i]);
            }

            _answers.Clear();
        }

        private void ShowAnswers(in LocalizedDialogsEntry entry)
        {
            for(var i = 0; i < entry.Answers.Count; i++)
            {
                if(entry.Answers[i].Visible is DialogCondition visibleCondition)
                {
                    if(visibleCondition != default && !visibleCondition.Check(_player, _npc))
                    {
                        continue;
                    }
                }
                
                var answerView = GetAnswer();
                _answersContainer.Add(answerView);
                ShowAnswer(answerView, entry.Answers[i], i);
                _answers.Add(answerView);
            }
        }

        private void ShowAnswer(UIElementsDialogAnswer answerView, LocalizedDialogAnswer answer, int id)
        {
            answerView.Id = id;
            if(answer.Interactable is DialogCondition interactableCondition)
            {
                answerView.Interactable = interactableCondition == default || interactableCondition.Check(_player, _npc);
            }
            else
            {
                answerView.Interactable = true;
            }

            answerView.Message.text = FormatString(answer.Text.GetLocalizedString());
        }

        private UIElementsDialogAnswer GetAnswer()
        {
            UIElementsDialogAnswer answer;
            if (_pool.Count > 0)
            {
                var last = _pool.Count - 1;
                answer = _pool[last];
                _pool.RemoveAt(last);
                answer.style.display = DisplayStyle.Flex;
            }
            else
            {
                answer = new UIElementsDialogAnswer(AnswerUXML, AnswerHoverClassName);
                answer.Clicked += AnswerClickListener;
                answer.Message = answer.Q<Label>(AnswerMessageSelector);
            }
            
            return answer;
        }

        private void AnswerClickListener(UIElementsDialogAnswer answerView)
        {
            var id = _dialogs.GetEntryId(_currentGuid);
            var answer = _dialogs.Entries[id].Answers[answerView.Id];
            if(answer.Action is DialogAction action && action != default)
            {
                action.Do(_player, _npc);
            }
            AudioSource.Stop();
            Show(_dialogs, answer.NextDialogGuid);
        }

        private bool TryEnd()
        {
            if(_dialogs == default)
            {
                End();
                return true;
            }

            var id = _dialogs.GetEntryId(_currentGuid);
            if(id == -1)
            {
                End();
                return true;
            }

            return false;
        }

        private string FormatString(string s)
        {
            return string.Format(s, _player.PlayerName, _player.Money);
        }

        public void End()
        {
            _dialogs = default;
            _currentGuid = -1;
            Ended?.Invoke();
        }

        private void CollectElements()
        {
            var root = Document.rootVisualElement;
            _npcName = root.Q<Label>(NpcNameSelector);
            _npcAvatar = root.Q<VisualElement>(NpcAvatarSelector);
            _dialogMessage = root.Q<Label>(MessageSelector);
            _answersContainer = root.Q<ScrollView>(AnswersContainerSelector);
            _inited = true;
        }

        private void OnEnable()
        {
            CollectElements();
            if(_dialogs != default)
            {
                Refresh();
            }
        }

        private void OnDisable()
        {
            _inited = false;
            _pool.Clear();
        }
    }
}