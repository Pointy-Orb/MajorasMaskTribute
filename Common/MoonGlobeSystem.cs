using Terraria;
using MajorasMaskTribute.Content.Items;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Common;

public class MoonGlobeSystem : ModSystem
{
    public override void AddRecipes()
    {
        Recipe.Create(ItemID.MoonGlobe)
            .AddIngredient(ModContent.ItemType<MajorasMask>())
            .AddConsumeIngredientCallback((Recipe recipe, int type, ref int amount, bool isDecrafting) =>
            {
                if (type == ModContent.ItemType<MajorasMask>())
                {
                    amount = 0;
                }
            })
            .AddIngredient(ItemID.GoldCoin, 4)
            .Register();
    }
}
