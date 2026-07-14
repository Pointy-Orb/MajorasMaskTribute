using MajorasMaskTribute.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MajorasMaskTribute.Content.Items;

public class EclipseDisc : ModItem
{
    private static Asset<Texture2D> glow;

    public bool downedPlantBoss { get; private set; }
    public bool downedAllMechs { get; private set; }

    public override void SetStaticDefaults()
    {
        glow = ModContent.Request<Texture2D>(Texture + "Glow");
        Item.ResearchUnlockCount = 3;
    }

    public override void SetDefaults()
    {
        Item.maxStack = Item.CommonMaxStack;
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(gold: 2, silver: 46));
        Item.width = 40;
        Item.height = 42;
    }

    public override void UpdateInventory(Player player)
    {
        downedPlantBoss = NPC.downedPlantBoss;
        downedAllMechs = NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);
        spriteBatch.Draw(glow.Value, drawPosition, itemFrame, Color.White, rotation, drawOrigin, scale, SpriteEffects.None, 0);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 4)
            .AddIngredient(ModContent.ItemType<MoonTear>())
            .AddTile(TileID.MythrilAnvil)
            .AddCondition(Language.GetOrRegister("Mods.MajorasMaskTribute.VanillaEclipseLogicDisabled"), () => !ModContent.GetInstance<ServerConfig>().VanillaEclipseLogic)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 4)
            .AddIngredient(ItemID.BloodMoonStarter)
            .AddTile(TileID.MythrilAnvil)
            .AddCondition(Language.GetOrRegister("Mods.MajorasMaskTribute.VanillaEclipseLogicDisabled"), () => !ModContent.GetInstance<ServerConfig>().VanillaEclipseLogic)
            .Register();
    }
}

public class EclipseDiscPlayer : ModPlayer
{
    public bool CheckForEclipseDisc(bool remove = false)
    {
        var eclipseDiscSlot = Player.FindItem(ModContent.ItemType<EclipseDisc>(), Player.inventory);
        if (!remove || eclipseDiscSlot < 0)
        {
            return eclipseDiscSlot >= 0;
        }
        Player.inventory[eclipseDiscSlot].stack--;
        if (Player.inventory[eclipseDiscSlot].stack <= 0)
        {
            Player.inventory[eclipseDiscSlot].TurnToAir();
        }
        return true;
        /*
        if (eclipseDiscSlot < 0)
            return false;
        if (Player.inventory[eclipseDiscSlot].ModItem is EclipseDisc disc)
        {
            EclipseSystem.PhonyDownedMechs = disc.downedAllMechs;
            EclipseSystem.PhonyDownedPlantera = disc.downedPlantBoss;
        }
        hasEclipseDisc = true;
        return true;
         */
    }
}
