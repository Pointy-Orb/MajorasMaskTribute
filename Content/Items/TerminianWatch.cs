using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Items;

public class TerminianWatch : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 48;
        Item.SetShopValues(ItemRarityColor.Blue1, Item.sellPrice(silver: 25));
        Item.accessory = true;
    }

    public override void UpdateInfoAccessory(Player player)
    {
        player.GetModPlayer<MMTimePlayer>().accTerminaWatch = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.GoldWatch)
            .AddRecipeGroup(RecipeGroupID.IronBar, 4)
            .AddTile(TileID.Tables)
            .AddTile(TileID.Chairs)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.PlatinumWatch)
            .AddRecipeGroup(RecipeGroupID.IronBar, 4)
            .AddTile(TileID.Tables)
            .AddTile(TileID.Chairs)
            .Register();
    }
}

public class MMTimePlayer : ModPlayer
{
    public bool accTerminaWatch = false;

    public override void ResetInfoAccessories()
    {
        accTerminaWatch = false;
    }
}
