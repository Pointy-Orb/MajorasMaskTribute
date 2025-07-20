using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using System;

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
        IL_NPC.SpawnNPC += IL_PhonySpawnConditions;
    }

    public override void PostUpdateTime()
    {
        if (!Main.dayTime)
        {
            _downedMechsOverride = false;
        }
    }

    private static void IL_PhonySpawnConditions(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            var skipLabel = il.DefineLabel();
            c.GotoNext(i => i.MatchLdsfld(typeof(NPC).GetField(nameof(NPC.downedMechBoss1))));
            c.GotoNext(i => i.MatchBrfalse(out skipLabel));
            c.GotoNext(i => i.MatchLdcI4(1));
            c.GotoPrev(i => i.MatchLdsfld(typeof(NPC).GetField(nameof(NPC.downedMechBoss1))));
            c.Remove();
            c.Remove();
            if (c.Next.OpCode != Ldsfld)
            {
                c.GotoNext(i => i.MatchLdsfld(out _));
            }
            c.Remove();
            c.Remove();
            if (c.Next.OpCode != Ldsfld)
            {
                c.GotoNext(i => i.MatchLdsfld(out _));
            }
            c.Remove();
            c.Remove();
            c.EmitDelegate<Func<bool>>(() => PhonyDownedMechs);
            c.Emit(Brfalse_S, skipLabel);
            while (c.Next != null && c.Next.OpCode != Br)
            {
                if (c.Next.MatchLdsfld(typeof(NPC).GetField(nameof(NPC.downedPlantBoss))))
                {
                    c.Index++;
                    c.EmitDelegate<Func<bool>>(() => PhonyDownedPlantera);
                    c.GotoPrev(i => i.MatchLdsfld(typeof(NPC).GetField(nameof(NPC.downedPlantBoss))));
                    c.Remove();
                }
                c.Index++;
            }
            c.GotoPrev(MoveType.After, i => i.MatchStloc(out _));
            c.MarkLabel(skipLabel);
            //MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
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
}
