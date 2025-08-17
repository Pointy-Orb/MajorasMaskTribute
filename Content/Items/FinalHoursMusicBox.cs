using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Items;

public class FinalHoursMusicBox : ModItem
{
    public override void SetStaticDefaults()
    {
        MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(Mod, "Assets/Music/finalhours"), ModContent.ItemType<FinalHoursMusicBox>(), ModContent.TileType<Tiles.FinalHoursMusicBox>());
    }

    public override void SetDefaults()
    {
        Item.DefaultToMusicBox(ModContent.TileType<Tiles.FinalHoursMusicBox>());
        Item.width = 32;
        Item.height = 26;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.MusicBox)
            .AddIngredient(ModContent.ItemType<MajorasMask>())
            .AddConsumeIngredientCallback((Recipe recipe, int type, ref int amount, bool isDecrafting) =>
            {
                if (type == ModContent.ItemType<MajorasMask>())
                {
                    amount = 0;
                }
            })
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
