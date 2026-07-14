using Terraria;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;

namespace MajorasMaskTribute.Common;

public class WerewolfSystem : GlobalNPC
{
    const double midnight = Main.nightLength / 2;

    public override void Load()
    {
        IL_NPC.SpawnNPC += EditWerewolfSpawning;
    }

    public override void Unload()
    {
        IL_NPC.SpawnNPC -= EditWerewolfSpawning;
    }

    private static void EditWerewolfSpawning(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            var skip = il.DefineLabel();
            c.GotoNext(i => i.MatchLdcI4(NPCID.Werewolf));
            c.GotoPrev(MoveType.After, i => i.MatchBrfalse(out skip));
            c.EmitDelegate<Func<bool>>(() =>
            {
                if (!ModContent.GetInstance<ServerConfig>().WerewolfSpawningChange)
                {
                    return false;
                }
                if (Main.bloodMoon || !ApocalypseSystem.cycleActive)
                {
                    return false;
                }
                return Math.Abs(Main.time - midnight) > 3600;
            });
            c.Emit(Brtrue_S, skip);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
    }
}
