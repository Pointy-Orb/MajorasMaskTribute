using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.Chat;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace MajorasMaskTribute.Common;

//[Autoload(Side = ModSide.Server)]
public class CycleCounter : ModSystem
{
    public static int cycles = 0;

    public override void ClearWorld()
    {
        cycles = 0;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (cycles > 0)
        {
            tag["cycles"] = cycles;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        cycles = tag.GetAsInt("cycles");
    }
}

public class ShowCycles : ModCommand
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "cycles";

    public static LocalizedText ResetXTimesMessage;

    public override void SetStaticDefaults()
    {
        ResetXTimesMessage = Language.GetText("Mods.MajorasMaskTribute.ResetXTimes");
    }

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        var message = ResetXTimesMessage.WithFormatArgs(CycleCounter.cycles);
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            Main.NewText(message);
        }
        else if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            MajorasMaskTribute.NetData.SendCycleCount((byte)caller.Player.whoAmI);
        }
    }
}
