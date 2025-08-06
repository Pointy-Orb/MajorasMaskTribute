using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Common;

public class MiscPlayer : ModPlayer
{
    public override bool CanUseItem(Item item)
    {
        if (item.type != ItemID.BloodMoonStarter)
        {
            return true;
        }
        if (!ApocalypseSystem.cycleActive)
        {
            return true;
        }
        return ModContent.GetInstance<ServerConfig>().VanillaBloodMoonLogic;
    }
}
