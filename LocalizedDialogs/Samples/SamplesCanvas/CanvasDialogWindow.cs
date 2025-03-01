using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace LocalizedDialogs.Samples
{
    public class CanvasDialogWindow : MonoBehaviour
    {
        public event Action Ended;
        public Text MessageField;
        public Text NpcNameField;
        public Image NpcAvatar;
        public Transform AnswersContainer;
        public CanvasDialogAnswer AnswerPrefab;
        public AudioSource AudioSource;
        public List<CanvasDialogAnswer> _answers;
        public List<CanvasDialogAnswer> _pool;
        private LocalizedDialogs _dialogs;
        private int _currentGuid;
        private NPC _npc;
        private Player _player;

        public void Show(Player player, NPC npc, LocalizedDialogs dialogs, int entryGuid)
        {
            _npc = npc;
            _player = player;
            NpcAvatar.sprite = npc.Avatar;
            NpcNameField.text = npc.Name.GetLocalizedString();
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
            if(TryEnd()) return;
            var id = _dialogs.GetEntryId(_currentGuid);
            var entry = _dialogs.Entries[id];

            MessageField.text = FormatString(entry.Text.GetLocalizedString());
            if(!entry.Audio.IsEmpty)
            {
                var clip = entry.Audio.LoadAsset();
                AudioSource.clip = clip;
                AudioSource.Play();
            }
            RecycleAnswers();
            ShowAnswers(entry);
        }

        private void ShowAnswer(CanvasDialogAnswer answerView, LocalizedDialogAnswer answer, int id)
        {
            answerView.Show(answer, id);
            if(answer.Interactable is DialogCondition interactableCondition)
            {
                answerView.Interactable = interactableCondition == default || interactableCondition.Check(_player, _npc);
            }
            else
            {
                answerView.Interactable = true;
            }

            answerView.TextField.text = FormatString(answer.Text.GetLocalizedString());
        }

        private CanvasDialogAnswer GetAnswer()
        {
            CanvasDialogAnswer answer;
            if (_pool.Count > 0)
            {
                var last = _pool.Count - 1;
                answer = _pool[last];
                _pool.RemoveAt(last);
                answer.gameObject.SetActive(true);
            }
            else
            {
                answer = Instantiate(AnswerPrefab, AnswersContainer);
                answer.Clicked += AnswerClickListener;
            }
            
            return answer;
        }

        private void AnswerClickListener(CanvasDialogAnswer answerView)
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

        private void RecycleAnswers()
        {
            for(var i = 0; i < _answers.Count; i++)
            {
                _answers[i].gameObject.SetActive(false);
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
                answerView.transform.SetSiblingIndex(i);
                ShowAnswer(answerView, entry.Answers[i], i);
                _answers.Add(answerView);
            }
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

        public void End()
        {
            _dialogs = default;
            _currentGuid = -1;
            Ended?.Invoke();
        }

        private string FormatString(string s)
        {
            return string.Format(s, _player.PlayerName, _player.Money);
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocalizationChanged;
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocalizationChanged;
        }

        private void OnLocalizationChanged(Locale locale)
        {
            Refresh();
        }
    }
}