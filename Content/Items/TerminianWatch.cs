using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;

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
        player.GetModPlayer<MMTimePlayer>().terminaWatch = MMTimePlayer.TerminaWatch.Classic;
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

        CreateRecipe()
            .AddIngredient(ModContent.ItemType<TerminianWatch3D>())
            .Register();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (ModContent.GetInstance<Common.ServerConfig>().WandOfSparkingMode == Common.WandOfSparkingMode.Off)
        {
            return;
        }
        int tooltipIndex = tooltips.FindIndex(0, tooltips.Count, i => i.Text == Tooltip.Value);
        if (!tooltips.IndexInRange(tooltipIndex) || !tooltips[tooltipIndex].Visible)
        {
            return;
        }
        tooltips.Insert(tooltipIndex + 1, new TooltipLine(Mod, "WoSModeNote", this.GetLocalizedValue("WoSModeNote")));
    }
}

public class TerminianWatch3D : ModItem
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
        player.GetModPlayer<MMTimePlayer>().terminaWatch = MMTimePlayer.TerminaWatch.ThreeDeeEss;
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

        CreateRecipe()
            .AddIngredient(ModContent.ItemType<TerminianWatch>())
            .Register();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (ModContent.GetInstance<Common.ServerConfig>().WandOfSparkingMode == Common.WandOfSparkingMode.Off)
        {
            return;
        }
        int tooltipIndex = tooltips.FindIndex(0, tooltips.Count, i => i.Text == Tooltip.Value);
        if (!tooltips.IndexInRange(tooltipIndex) || !tooltips[tooltipIndex].Visible)
        {
            return;
        }
        tooltips.Insert(tooltipIndex + 1, new TooltipLine(Mod, "WoSModeNote", this.GetLocalizedValue("WoSModeNote")));
    }
}

public class MMTimePlayer : ModPlayer
{
    public enum TerminaWatch
    {
        Off,
        Classic,
        ThreeDeeEss
    }

    public TerminaWatch terminaWatch = TerminaWatch.Off;

    public override void ResetInfoAccessories()
    {
        terminaWatch = TerminaWatch.Off;
    }
}
