using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using Terraria.ID;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace MajorasMaskTribute.Common;

public class EclipseSystem : ModSystem
{
    public static bool PhonyDownedMechs
    {
        get
        {
            if (_downedMechsOverride)
            {
                return true;
            }
            return NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
        }
        set
        {
            _downedMechsOverride = value;
        }
    }

    public static bool PhonyDownedPlantera
    {
        get
        {
            if (_downedPlanteraOverride)
            {
                return true;
            }
            return NPC.downedPlantBoss;
        }
        set
        {
            _downedPlanteraOverride = value;
        }
    }

    private static bool _downedMechsOverride = false;
    private static bool _downedPlanteraOverride = false;

    public override void Load()
    {
        IL_Main.UpdateTime_StartDay += IL_StopEclipse;
    }

    public override void PostUpdateTime()
    {
        if (!Main.dayTime)
        {
            _downedMechsOverride = false;
            _downedPlanteraOverride = false;
        }
    }

    private static void IL_StopEclipse(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            var jumpLabel = il.DefineLabel();
            c.GotoNext(i => i.MatchStsfld(typeof(Main).GetField(nameof(Main.eclipse))));
            c.GotoPrev(MoveType.After, i => i.MatchBrtrue(out jumpLabel));
            c.EmitDelegate<Func<bool>>(() =>
            {
                return ModContent.GetInstance<ServerConfig>().VanillaEclipseLogic || !ApocalypseSystem.cycleActive;
            });
            c.Emit(Brfalse_S, jumpLabel);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (_downedPlanteraOverride)
        {
            tag["_downedPlanteraOverride"] = true;
        }
        if (_downedMechsOverride)
        {
            tag["_downedMechsOverride"] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        _downedPlanteraOverride = tag.GetBool("_downedPlanteraOverride");
        _downedMechsOverride = tag.GetBool("_downedMechsOverride");
    }
}

public class EclipseSpawning : GlobalNPC
{
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (!Main.eclipse || !Main.dayTime)
        {
            return;
        }
        if ((double)spawnInfo.PlayerFloorY > Main.worldSurface && !Main.remixWorld)
        {
            return;
        }
        if (Main.remixWorld && (double)spawnInfo.PlayerFloorY <= Main.rockLayer)
        {
            return;
        }
        if (!NPC.downedMechBoss1 || !NPC.downedMechBoss2 || !NPC.downedMechBoss3)
        {
            pool[NPCID.Reaper] = EclipseSystem.PhonyDownedMechs && Main.rand.NextBool(13) ? 1f : 0f;
        }
        if (!NPC.downedPlantBoss)
        {
            pool[NPCID.Butcher] = EclipseSystem.PhonyDownedPlantera && Main.rand.NextBool(5) ? 1f : 0f;
            pool[NPCID.DeadlySphere] = EclipseSystem.PhonyDownedPlantera && NPC.CountNPCS(NPCID.DeadlySphere) < 2 && Main.rand.NextBool(20) ? 1f : 0f;
            pool[NPCID.DrManFly] = EclipseSystem.PhonyDownedPlantera && Main.rand.NextBool(7) ? 1f : 0f;
            pool[NPCID.Nailhead] = EclipseSystem.PhonyDownedPlantera && !NPC.AnyNPCs(NPCID.Nailhead) && Main.rand.NextBool(20) ? 1f : 0f;
            pool[NPCID.Psycho] = EclipseSystem.PhonyDownedPlantera && !NPC.AnyNPCs(NPCID.Psycho) && Main.rand.NextBool(5) ? 1f : 0f;
            pool[NPCID.Mothron] = EclipseSystem.PhonyDownedPlantera && !NPC.AnyNPCs(NPCID.Mothron) && Main.rand.NextBool(80) ? 1f : 0f;
        }
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (eclipseEnemies.Contains((short)npc.type))
        {
            foreach (IItemDropRule rule in npcLoot.Get())
            {
                if (rule is LeadingConditionRule lConRule)
                {
                    foreach (IItemDropRuleChainAttempt chain in lConRule.ChainedRules)
                    {
                        if (chain.RuleToChain is LeadingConditionRule lConRule2)
                        {
                            var info2 = new DropAttemptInfo();
                            if (lConRule2.condition.CanDrop(info2) == new Conditions.DownedPlantera().CanDrop(info2))
                            {
                                lConRule2.condition = new PhonyDownedPlanteraCondition();
                            }
                            if (lConRule2.condition.CanDrop(info2) == new Conditions.DownedAllMechBosses().CanDrop(info2))
                            {
                                lConRule2.condition = new PhonyDownedAllMechsCondition();
                            }
                        }
                    }
                    var info = new DropAttemptInfo();
                    if (lConRule.condition.CanDrop(info) == new Conditions.DownedPlantera().CanDrop(info))
                    {
                        lConRule.condition = new PhonyDownedPlanteraCondition();
                    }
                    if (lConRule.condition.CanDrop(info) == new Conditions.DownedAllMechBosses().CanDrop(info))
                    {
                        lConRule.condition = new PhonyDownedAllMechsCondition();
                    }
                }
            }
        }
    }

    private static readonly List<short> eclipseEnemies = new() { NPCID.Mothron, NPCID.Reaper, NPCID.Butcher, NPCID.DrManFly, NPCID.Psycho, NPCID.DeadlySphere, NPCID.Nailhead };
}

public class PhonyDownedPlanteraCondition : IItemDropRuleCondition
{
    public bool CanDrop(DropAttemptInfo info)
    {
        return EclipseSystem.PhonyDownedPlantera;
    }

    public bool CanShowItemDropInUI()
    {
        return true;
    }

    public string GetConditionDescription()
    {
        return "";
    }
}
public class PhonyDownedAllMechsCondition : IItemDropRuleCondition
{
    public bool CanDrop(DropAttemptInfo info)
    {
        return EclipseSystem.PhonyDownedMechs;
    }

    public bool CanShowItemDropInUI()
    {
        return true;
    }

    public string GetConditionDescription()
    {
        return "";
    }
}
