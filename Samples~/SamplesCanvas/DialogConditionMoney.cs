using UnityEngine;

namespace LocalizedDialogs.Samples.Canvas
{
    [CreateAssetMenu(menuName = "Localized Dialogs Sample/Canvas/Money condition")]
    public class DialogConditionMoney : DialogCondition
    {
        public int Amount;
        public override bool Check(Player player, NPC npc)
        {
            return player.Money >= Amount;
        }
    }
}
