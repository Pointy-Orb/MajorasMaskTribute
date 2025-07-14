using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace MajorasMaskTribute.Common;

public class EditWatch : GlobalInfoDisplay
{
    public override void ModifyDisplayParameters(InfoDisplay currentDisplay, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor)
    {
        if (currentDisplay != InfoDisplay.Watches)
        {
            return;
        }
        displayValue = Language.GetText("Mods.MajorasMaskTribute.WatchDisplay").WithFormatArgs(ApocalypseSystem.apocalypseDay + 1, displayValue).Value;
    }
}
