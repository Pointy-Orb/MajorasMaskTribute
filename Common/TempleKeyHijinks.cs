using Terraria;
using Terraria.GameContent.Events;
using Terraria.Localization;
using Terraria.GameContent.ItemDropRules;
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
        IL_CultistRitual.CheckRitual += IL_PrehardmodeCultists;
        //On_CultistRitual.CheckRitual += On_CultistCheck;
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
            c.GotoPrev(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.hardMode))));
            c.Remove();
            c.EmitDelegate<Func<bool>>(() =>
            {
                return ModContent.GetInstance<ServerConfig>().NoPlanteraToSummonGolem || Main.hardMode;
            });

        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
    }

    private static void IL_PrehardmodeCultists(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            var skipHardLabel = il.DefineLabel();
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.hardMode))));
            c.EmitDelegate<Func<bool>>(() => ModContent.GetInstance<ServerConfig>().NoPlanteraToSummonGolem);
            c.Emit(Brtrue_S, skipHardLabel);
            c.GotoNext(MoveType.After, i => i.MatchBrfalse(out _));
            c.MarkLabel(skipHardLabel);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
    }

    private static bool On_CultistCheck(On_CultistRitual.orig_CheckRitual orig, int x, int y)
    {
        var statement = orig(x, y);
        MajorasMaskTribute.mod.Logger.Info(statement);
        MajorasMaskTribute.mod.Logger.Info(Main.hardMode);
        MajorasMaskTribute.mod.Logger.Info(NPC.downedGolemBoss);
        MajorasMaskTribute.mod.Logger.Info(NPC.downedBoss3);
        return statement;
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
    public override bool ConsumeItem(Item item, Player player)
    {
        if (item.type != ItemID.TempleKey)
        {
            return true;
        }
        TempleKeySystem.anybodyUsedTempleKey = true;
        return ModContent.GetInstance<ServerConfig>().EatTempleKey;
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

public class DropHellShell : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.WallofFlesh)
        {
            LeadingConditionRule rule = new(new HellShellConfigEnabledCondition());
            rule.OnSuccess(ItemDropRule.ByCondition(new Conditions.NotExpert(), ItemID.DemonConch));
            npcLoot.Add(rule);
        }
    }
}

public class DropHellShellFromBag : GlobalItem
{
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (item.type == ItemID.WallOfFleshBossBag)
        {
            LeadingConditionRule rule = new(new HellShellConfigEnabledCondition());
            rule.OnSuccess(ItemDropRule.Common(ItemID.DemonConch));
            itemLoot.Add(rule);
        }
    }
}

public class HellShellConfigEnabledCondition : IItemDropRuleCondition
{
    public bool CanDrop(DropAttemptInfo info)
    {
        return ModContent.GetInstance<ServerConfig>().WOFDropsDemonConch;
    }

    public bool CanShowItemDropInUI()
    {
        return ModContent.GetInstance<ServerConfig>().WOFDropsDemonConch;
    }

    public string GetConditionDescription()
    {
        return "";
    }
}
