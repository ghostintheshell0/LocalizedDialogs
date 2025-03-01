using UnityEngine;

namespace LocalizedDialogs.Samples.UIElements
{
    public abstract class DialogCondition : ScriptableObject
    {
        public abstract bool Check(Player player, NPC npc);
    }
}