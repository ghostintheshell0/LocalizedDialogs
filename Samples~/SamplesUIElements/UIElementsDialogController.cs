using System.Linq;
using UnityEngine;

namespace LocalizedDialogs.Samples
{
    public class UIElementsDialogController : MonoBehaviour
    {
        public NPC Npc;
        public Player Player;
        public UIElementsDialogWindow DialogWindow;

        private void Start()
        {
            DialogWindow.Ended += DialogEndListener;
            var firstEntry = Npc.Dialogs.GetFirstId();
            DialogWindow.Show(Player, Npc, Npc.Dialogs, firstEntry);
        }

        private void OnDestroy()
        {
            DialogWindow.Ended -= DialogEndListener;
        }

        private void DialogEndListener()
        {
            Debug.Log("Dialog completed. Start Again");
            var firstEntry = Npc.Dialogs.Entries.Min(entry => entry.Guid);
            DialogWindow.Show(Player, Npc, Npc.Dialogs, firstEntry);
        }
    }
}