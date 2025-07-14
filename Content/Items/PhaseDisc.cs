using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Items;

public class PhaseDisc : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.useAnimation = 1;
        Item.useTime = 1;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item4;
        Item.rare = ItemRarityID.Orange;
    }

    public override bool? UseItem(Player player)
    {
        Main.moonPhase++;
        if (Main.moonPhase >= 8)
        {
            Main.moonPhase = 0;
        }
        return null;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.FallenStar, 10)
            .AddIngredient(ItemID.TissueSample, 5)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.FallenStar, 10)
            .AddIngredient(ItemID.ShadowScale, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
