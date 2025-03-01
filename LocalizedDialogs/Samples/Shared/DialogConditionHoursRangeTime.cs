using System;
using UnityEngine;

namespace LocalizedDialogs.Samples
{
    [CreateAssetMenu(menuName = "Game/Localized Dialogs/Sample/Hours range condition")]
    public class DialogConditionHoursRangeTime : DialogCondition
    {
        public int MinHours;
        public int MaxHours;
        public override bool Check(Player player, NPC npc)
        {
            var hours = DateTime.Now.Hour;
            return hours >= MinHours && hours < MaxHours;
        }
    }

}

