using UnityEngine;

namespace LocalizedDialogs.Samples
{
    public abstract class DialogAction : ScriptableObject
    {
        public abstract void Do(Player player, NPC npc);
    }

}