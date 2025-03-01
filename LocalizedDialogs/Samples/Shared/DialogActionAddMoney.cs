using UnityEngine;

namespace LocalizedDialogs.Samples
{
    [CreateAssetMenu(menuName = "Game/Localized Dialogs/Sample/Add money")]
    public class DialogActionAddMoney : DialogAction
    {
        public int Amount;
        public override void Do(Player player, NPC npc)
        {
            player.Money += Amount;
        }
    }

}