using UnityEngine;

namespace LocalizedDialogs.Samples.UIElements
{
    public abstract class DialogAction : ScriptableObject
    {
        public abstract void Do(Player player, NPC npc);
    }

}