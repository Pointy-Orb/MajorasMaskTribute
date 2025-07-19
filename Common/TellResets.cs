using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Common;

public class TellResets : ModCommand
{
    public override string Command => "cycles";

    public override CommandType Type => CommandType.Chat;

    private LocalizedText resetsText;

    public override void SetStaticDefaults()
    {
        resetsText = Language.GetText("Mods.MajorasMaskTribute.ResetXTimes");
    }

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        Main.NewText(resetsText.Format(ApocalypseSystem.resets));
    }
}
