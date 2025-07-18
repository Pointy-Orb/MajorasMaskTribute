using Terraria;
using System;
using static Mono.Cecil.Cil.OpCodes;
using MonoMod.Cil;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace MajorasMaskTribute.Common;

/*
 * By default, Golem cannot be summoned unless Plantera is defeated.
 * This is because hoiks can be used to break into the jungle temple much earlier than intended.
 * Since hoiks are too cool to patch out, this was the alternative solution.
 *
 * However, this class exists to replace that requirement with a new one that checks if a Temple Key has been consumed or if a player at one point had
 * an altar in their inventory,  so that players can feel that they have big brains by saving the keys from previous cycles to use in new ones.
*/

public class TempleKeyPlayer : ModPlayer
{
    public override void PostUpdate()
    {
        if (HasItemAnywhere(ItemID.LihzahrdAltar))
            TempleKeySystem.anybodyUsedTempleKey = true;
    }

    private bool HasItemAnywhere(int id)
    {
        if (Player.HasItemInInventoryOrOpenVoidBag(id))
        {
            return true;
        }
        if (Player.HasItem(id, Player.bank.item))
        {
            return true;
        }
        if (Player.HasItem(id, Player.bank2.item))
        {
            return true;
        }
        if (Player.HasItem(id, Player.bank3.item))
        {
            return true;
        }
        if (Player.HasItem(id, Player.bank4.item))
        {
            return true;
        }
        return false;
    }
}

public class TempleKeySystem : ModSystem
{
    public static bool anybodyUsedTempleKey = false;

    public override void Load()
    {
        IL_Player.TileInteractionsUse += IL_ChangeSummonGolemCondition;
    }

    private static void IL_ChangeSummonGolemCondition(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            c.GotoNext(i => i.MatchLdsfld(typeof(NPC).GetField(nameof(NPC.downedPlantBoss))));
            c.Remove();
            c.EmitDelegate<Func<bool>>(() =>
            {
                if (ModContent.GetInstance<ServerConfig>().NoPlanteraToSummonGolem)
                {
                    return anybodyUsedTempleKey;
                }
                else
                {
                    return NPC.downedPlantBoss;
                }
            });
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
    }

    public override void ClearWorld()
    {
        anybodyUsedTempleKey = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (anybodyUsedTempleKey)
        {
            tag["anybodyUsedTempleKey"] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        anybodyUsedTempleKey = tag.GetBool("anybodyUsedTempleKey");
    }
}

public class TempleKeyCheck : GlobalItem
{
    public override void OnConsumeItem(Item item, Player player)
    {
        if (item.type == ItemID.TempleKey)
        {
            TempleKeySystem.anybodyUsedTempleKey = true;
        }
    }
}

public class KillOldGeezer : GlobalNPC
{
    public override void PostAI(NPC npc)
    {
        if (npc.type == NPCID.OldMan && ModContent.GetInstance<ServerConfig>().OldManDoesntAppearOnFirstDay)
        {
            npc.Transform(NPCID.Bunny);
            npc.position = Microsoft.Xna.Framework.Vector2.Zero;
            npc.StrikeInstantKill();
        }
    }
}
