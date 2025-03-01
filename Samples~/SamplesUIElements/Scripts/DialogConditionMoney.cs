using UnityEngine;

namespace LocalizedDialogs.Samples.UIElements
{
    [CreateAssetMenu(menuName = "Localized Dialogs Sample/UI Elements/Money condition")]
    public class DialogConditionMoney : DialogCondition
    {
        public int Amount;
        public override bool Check(Player player, NPC npc)
        {
            return player.Money >= Amount;
        }
    }
}