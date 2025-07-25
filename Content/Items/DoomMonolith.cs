using Terraria;
using MajorasMaskTribute.Content.Tiles;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Items;

public class DoomMonolith : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.DoomMonolith>());
        Item.width = 24;
        Item.height = 36;
        Item.vanity = true;
        Item.accessory = true;
        Item.SetShopValues(ItemRarityColor.Orange3, Item.sellPrice(gold: 5));
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.BloodMoonMonolith)
            .AddIngredient(ModContent.ItemType<MoonTear>())
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override void UpdateVanity(Player player)
    {
        player.GetModPlayer<DoomMonolithPlayer>().doomMonolithActive = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        UpdateVanity(player);
    }
}
