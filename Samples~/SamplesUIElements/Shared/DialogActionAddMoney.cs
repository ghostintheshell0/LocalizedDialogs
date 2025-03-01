using UnityEngine;

namespace LocalizedDialogs.Samples.UIElements
{
    [CreateAssetMenu(menuName = "Localized Dialogs Sample/UI Elements/Add money")]
    public class DialogActionAddMoney : DialogAction
    {
        public int Amount;
        public override void Do(Player player, NPC npc)
        {
            player.Money += Amount;
        }
    }

}