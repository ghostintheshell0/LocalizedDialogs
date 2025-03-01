using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

namespace LocalizedDialogs
{
    [CreateAssetMenu(menuName = "Game/LocalizationEditor")]   
    public class LocalizedDialogs : ScriptableObject
    {
        public List<LocalizedDialogsEntry> Entries;

        public int GetFirstId()
        {
            if(Entries.Count == 0)
            {
                return -1;
            }

            return Entries.Min(entry => entry.Guid);
        }

        public int GetEntryId(int guid)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                if(Entries[i].Guid == guid)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    [System.Serializable]
    public class LocalizedDialogsEntry
    {
        public int Guid;
        #if UNITY_EDITOR
        public Vector2 PositionInEditor;
        #endif
        public LocalizedString Text;
        public LocalizedAudioClip Audio;

        public List<LocalizedDialogAnswer> Answers;
    }

    [System.Serializable]
    public class LocalizedDialogAnswer
    {
        public LocalizedString Text;
        public int NextDialogGuid;
        public ScriptableObject Interactable;
        public ScriptableObject Visible;
        public ScriptableObject Action;
    }
}