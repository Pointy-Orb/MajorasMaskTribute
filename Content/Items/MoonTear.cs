using Terraria;
using MajorasMaskTribute.Common;
using System;
using Terraria.Localization;
using Terraria.GameContent.ItemDropRules;
using Terraria.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Items;

public class MoonTear : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 24;
        Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(gold: 10));
        Item.maxStack = Item.CommonMaxStack;
    }

    public override void PostUpdate()
    {
        Lighting.AddLight((int)((Item.position.X + (float)Item.width) / 16f), (int)((Item.position.Y + (float)(Item.height / 2)) / 16f), 0.4f, 0.4f, 0.9f);
        if (Item.timeSinceItemSpawned % 12 == 0)
        {
            Dust dust2 = Dust.NewDustPerfect(Item.Center + new Vector2(0f, (float)Item.height * -0.1f) + Main.rand.NextVector2CircularEdge((float)Item.width * 0.6f, (float)Item.height * 0.6f) * (0.3f + Main.rand.NextFloat() * 0.5f), 279, new Vector2(0f, (0f - Main.rand.NextFloat()) * 0.3f - 1.5f), 127);
            dust2.scale = 0.5f;
            dust2.fadeIn = 1.1f;
            dust2.noGravity = true;
            dust2.noLight = true;
            dust2.alpha = 0;
        }
    }

    public override void AddRecipes()
    {
        Recipe.Create(ItemID.BloodMoonStarter)
            .AddIngredient(Type)
            .AddIngredient(ItemID.Lens, 2)
            .AddTile(TileID.WorkBenches)
            .AddCondition(Condition.InGraveyard)
            .DisableDecraft()
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.BloodMoonStarter)
            .AddIngredient(ItemID.Lens, 2)
            .AddTile(TileID.WorkBenches)
            .AddCondition(Condition.InGraveyard)
            .DisableDecraft()
            .Register();
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var texture = TextureAssets.Item[Type];

        float num7 = (float)Item.timeSinceItemSpawned / 240f + Main.GlobalTimeWrappedHourly * 0.04f;
        float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
        globalTimeWrappedHourly %= 4f;
        globalTimeWrappedHourly /= 2f;
        if (globalTimeWrappedHourly >= 1f)
        {
            globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;
        }
        globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
        //Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = position;

        for (float num8 = 0f; num8 < 1f; num8 += 0.25f)
        {
            spriteBatch.Draw(texture.Value, drawPosition + new Vector2(0f, 8f).RotatedBy((num8 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, frame, new Color(90, 70, 255, 50), 0, origin, scale, SpriteEffects.None, 0f);
        }
        for (float num9 = 0f; num9 < 1f; num9 += 0.34f)
        {
            spriteBatch.Draw(texture.Value, drawPosition + new Vector2(0f, 4f).RotatedBy((num9 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, frame, new Color(140, 120, 255, 77), 0, origin, scale, SpriteEffects.None, 0f);
        }
        return true;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type];

        float num7 = (float)Item.timeSinceItemSpawned / 240f + Main.GlobalTimeWrappedHourly * 0.04f;
        float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
        globalTimeWrappedHourly %= 4f;
        globalTimeWrappedHourly /= 2f;
        if (globalTimeWrappedHourly >= 1f)
        {
            globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;
        }
        globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
        Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);

        for (float num8 = 0f; num8 < 1f; num8 += 0.25f)
        {
            spriteBatch.Draw(texture.Value, drawPosition + new Vector2(0f, 8f).RotatedBy((num8 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, itemFrame, new Color(90, 70, 255, 50), rotation, drawOrigin, scale, SpriteEffects.None, 0f);
        }
        for (float num9 = 0f; num9 < 1f; num9 += 0.34f)
        {
            spriteBatch.Draw(texture.Value, drawPosition + new Vector2(0f, 4f).RotatedBy((num9 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, itemFrame, new Color(140, 120, 255, 77), rotation, drawOrigin, scale, SpriteEffects.None, 0f);
        }
        spriteBatch.Draw(texture.Value, drawPosition, itemFrame, Color.White, rotation, drawOrigin, scale, SpriteEffects.None, 0);
        return false;
    }
}

public class MoonTearDrop : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        foreach (IItemDropRule rule in npcLoot.Get())
        {
            if (rule is CommonDrop drop && drop.itemId == ItemID.BloodMoonStarter)
            {
                LeadingConditionRule vanillaBloodMoonRule = new(new VanillaBloodMoonOnCondition());
                vanillaBloodMoonRule.OnSuccess(rule);
                var numerator = drop.chanceNumerator * 2;
                numerator = Int32.Clamp(numerator, 1, drop.chanceDenominator);
                var moonTearDrop = ItemDropRule.ByCondition(new VanillaBloodMoonOffCondition(), ModContent.ItemType<MoonTear>(), drop.chanceDenominator, drop.amountDroppedMinimum, drop.amountDroppedMaximum, numerator);
                npcLoot.Remove(rule);
                npcLoot.Add(vanillaBloodMoonRule);
                npcLoot.Add(moonTearDrop);
            }
            else if (rule is ItemDropWithConditionRule conDrop && conDrop.itemId == ItemID.BloodMoonStarter)
            {
                LeadingConditionRule vanillaBloodMoonRule = new(new VanillaBloodMoonOnCondition());
                vanillaBloodMoonRule.OnSuccess(rule);
                var numerator = conDrop.chanceNumerator * 2;
                numerator = Int32.Clamp(numerator, 1, conDrop.chanceDenominator);
                var moonTearDrop = ItemDropRule.ByCondition(conDrop.condition, ModContent.ItemType<MoonTear>(), conDrop.chanceDenominator, conDrop.amountDroppedMinimum, conDrop.amountDroppedMaximum, numerator);
                LeadingConditionRule majoraBloodMoonRule = new(new VanillaBloodMoonOffCondition());
                majoraBloodMoonRule.OnSuccess(moonTearDrop);
                npcLoot.Remove(rule);
                npcLoot.Add(vanillaBloodMoonRule);
                npcLoot.Add(moonTearDrop);
            }
            else if (rule is DropBasedOnExpertMode expertDrop && expertDrop.ruleForNormalMode is CommonDrop commonNormalDrop && expertDrop.ruleForExpertMode is CommonDrop commonExpertDrop && commonNormalDrop.itemId == ItemID.BloodMoonStarter)
            {
                LeadingConditionRule vanillaBloodMoonRule = new(new VanillaBloodMoonOnCondition());
                vanillaBloodMoonRule.OnSuccess(rule);
                var moonTearDrop = ItemDropRule.NormalvsExpert(ModContent.ItemType<MoonTear>(), Int32.Clamp(commonNormalDrop.chanceDenominator / 2, 1, commonNormalDrop.chanceDenominator), Int32.Clamp(commonExpertDrop.chanceDenominator / 2, 1, commonExpertDrop.chanceDenominator));
                LeadingConditionRule majoraBloodMoonRule = new(new VanillaBloodMoonOffCondition());
                majoraBloodMoonRule.OnSuccess(moonTearDrop);
                npcLoot.Remove(rule);
                npcLoot.Add(vanillaBloodMoonRule);
                npcLoot.Add(moonTearDrop);
            }
        }
    }
}

public class VanillaBloodMoonOffCondition : IItemDropRuleCondition
{
    private static LocalizedText Descripition;

    public VanillaBloodMoonOffCondition()
    {
        Descripition ??= Language.GetOrRegister("Mods.MajorasMaskTribute.DropConditions.VanillaBloodMoonOff");
    }

    public bool CanDrop(DropAttemptInfo info)
    {
        return CanShowItemDropInUI();
    }

    public bool CanShowItemDropInUI()
    {
        if (!ApocalypseSystem.cycleActive)
        {
            return false;
        }
        return !ModContent.GetInstance<Common.ServerConfig>().VanillaBloodMoonLogic;
    }

    public string GetConditionDescription()
    {
        return Descripition.Value;
    }
}

public class VanillaBloodMoonOnCondition : IItemDropRuleCondition
{
    private static LocalizedText Descripition;

    public VanillaBloodMoonOnCondition()
    {
        Descripition ??= Language.GetOrRegister("Mods.MajorasMaskTribute.DropConditions.VanillaBloodMoonOn");
    }

    public bool CanDrop(DropAttemptInfo info)
    {
        return CanShowItemDropInUI();
    }

    public bool CanShowItemDropInUI()
    {
        if (!ApocalypseSystem.cycleActive)
        {
            return true;
        }
        return ModContent.GetInstance<Common.ServerConfig>().VanillaBloodMoonLogic || !ApocalypseSystem.cycleActive;
    }

    public string GetConditionDescription()
    {
        return Descripition.Value;
    }
}
