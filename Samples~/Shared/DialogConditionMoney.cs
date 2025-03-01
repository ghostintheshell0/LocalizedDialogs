using UnityEngine;

namespace LocalizedDialogs.Samples
{
    [CreateAssetMenu(menuName = "Game/Localized Dialogs/Sample/Money condition")]
    public class DialogConditionMoney : DialogCondition
    {
        public int Amount;
        public override bool Check(Player player, NPC npc)
        {
            return player.Money >= Amount;
        }
    }
}