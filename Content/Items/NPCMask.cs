using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Items;

public abstract class NPCMask : ModItem
{
    public abstract int targetNPC { get; }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.boss) continue;
            if (npc.Hitbox.Contains(Item.position.ToPoint()))
            {
                npc.GetGlobalNPC<HomunculusNPC>().TransformByMask(npc, targetNPC);
                Item.active = false;
            }
            break;
        }
    }
}

public class HomunculusNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public bool isHomunculus { get; private set; } = false;
    public int originalType { get; private set; }

    public void TransformByMask(NPC target, int maskNPCType)
    {
    }
}
