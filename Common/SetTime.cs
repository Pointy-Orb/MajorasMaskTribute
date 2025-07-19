using Terraria;
using System;
using System.Reflection;
using Mono.Cecil;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.IO;
using System.Collections.Generic;

namespace MajorasMaskTribute.Common;

public class SetTime : ModSystem
{
    public override void Load()
    {
        IL_WorldFile.ResetTempsToDayTime += IL_ChangeDefaultTime;
    }

    private static void IL_ChangeDefaultTime(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(i => i.MatchLdcR8(out _));
        FieldReference tempTime = null;
        c.GotoNext(MoveType.After, i => i.MatchStsfld(out tempTime));
        if (tempTime == null)
        {
            return;
        }
        c.EmitDelegate<Func<double>>(() =>
        {
            return (double)0;
        });
        c.Emit(Stsfld, tempTime);
    }
}
