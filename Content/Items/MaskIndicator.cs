using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Items;

public class MaskIndicator : ModItem
{
    public override void Load()
    {
        On_ContentSamples.CreativeHelper.ShouldRemoveFromList += NotForHumanEyes;
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
    }

    public override void SetDefaults()
    {
        //Item.TurnToAir();
    }

    private static bool NotForHumanEyes(On_ContentSamples.CreativeHelper.orig_ShouldRemoveFromList orig, Item item)
    {
        if (item.type == ModContent.ItemType<MaskIndicator>())
        {
            return true;
        }
        return orig(item);
    }
}

public class IndicateMask : GlobalNPC
{
    public override void Load()
    {
        On_ShopHelper.GetShoppingSettings += AddMaskIcon;
    }

    private static ShoppingSettings AddMaskIcon(On_ShopHelper.orig_GetShoppingSettings orig, ShopHelper self, Player player, NPC npc)
    {
        var report = orig(self, player, npc);
        bool happy = report.PriceAdjustment <= 0.8999999761581421;
        if (happy && !npc.GetGlobalNPC<HomunculusNPC>().isHomunculus)
        {
            report.HappinessReport += "[i:MajorasMaskTribute/MaskIndicator]";
        }
        return report;
    }
}

