using UnityEngine;

namespace LocalizedDialogs.Samples.Canvas
{
    public abstract class DialogAction : ScriptableObject
    {
        public abstract void Do(Player player, NPC npc);
    }

}
