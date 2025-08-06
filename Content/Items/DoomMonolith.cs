using Terraria;
using MajorasMaskTribute.Content.Tiles;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;

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
        Item.SetShopValues(ItemRarityColor.Cyan9, Item.sellPrice(2, 60));
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<MoonTear>())
            .AddIngredient(ModContent.ItemType<MajorasMask>())
            .AddTile(TileID.MythrilAnvil)
            .DisableDecraft()
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
