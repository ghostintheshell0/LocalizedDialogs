using UnityEngine;

namespace LocalizedDialogs.Samples
{
    public abstract class DialogCondition : ScriptableObject
    {
        public abstract bool Check(Player player, NPC npc);
    }
}