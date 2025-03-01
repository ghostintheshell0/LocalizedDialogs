using UnityEngine;

namespace LocalizedDialogs.Samples.Canvas
{
    [CreateAssetMenu(menuName = "Localized Dialogs Sample/Canvas/Add money")]
    public class DialogActionAddMoney : DialogAction
    {
        public int Amount;
        public override void Do(Player player, NPC npc)
        {
            player.Money += Amount;
        }
    }

}
