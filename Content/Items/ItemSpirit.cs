using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Enums;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MajorasMaskTribute.Content.Items;

public class ItemSpirit : ModItem
{
    public readonly short targetItem;
    private readonly string _name;

    public override string Texture => $"Terraria/Images/Item_{targetItem}";

    protected override bool CloneNewInstances => true;

    public override string Name => _name;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemNoGravity[Item.type] = true;
    }

    public ItemSpirit(short targetItem)
    {
        _name = ItemID.Search.GetName(targetItem).Replace("Mask", "Spirit");
        this.targetItem = targetItem;
    }

    public override LocalizedText DisplayName
    {
        get
        {
            var item = new Item();
            item.SetDefaults(targetItem);
            return Language.GetText("Mods.MajorasMaskTribute.Items.ItemSpirit.DisplayName").WithFormatArgs(item.Name.Replace(Language.GetTextValue("Mods.MajorasMaskTribute.Items.ItemSpirit.Mask"), ""));
        }
    }

    public override LocalizedText Tooltip => Language.GetText("Mods.MajorasMaskTribute.Items.ItemSpirit.Tooltip");

    private readonly Color spiritColor = new Color(80, 180, byte.MaxValue);

    public override void SetDefaults()
    {
        var sourceItem = new Item();
        sourceItem.SetDefaults(targetItem);
        Item.width = sourceItem.width;
        Item.height = sourceItem.height;
        Item.color = spiritColor;
        Item.alpha = 165;
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(silver: 75));
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type];
        Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);
        spriteBatch.Draw(texture.Value, drawPosition, itemFrame, spiritColor * 0.65f, rotation, drawOrigin, scale, SpriteEffects.None, 0);
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (ModContent.GetInstance<Common.ServerConfig>().WandOfSparkingMode != Common.WandOfSparkingMode.Off)
        {
            return;
        }
        var tooltipNoDisclaimer = Language.GetTextValue("Mods.MajorasMaskTribute.Items.ItemSpirit.TooltipNoDisclaimer");
        foreach (TooltipLine tooltip in tooltips)
        {
            if (tooltipNoDisclaimer.Contains(tooltip.Text))
            {
                tooltip.Hide();
            }
        }
    }
}

public class ItemSpiritLoader : ILoadable
{
    public void Load(Mod mod)
    {
        foreach (short type in Common.WandOfSparkingModePlayer.masks)
        {
            mod.AddContent(new ItemSpirit(type));
        }
    }

    public void Unload() { }
}

public class ItemSpiritDrop : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        var npcName = NPCID.Search.GetName(npc.type);
        switch (npc.type)
        {
            case NPCID.EyeofCthulhu:
                npcName = "Eye";
                goto default;
            case NPCID.EaterofWorldsHead:
            case NPCID.EaterofWorldsBody:
            case NPCID.EaterofWorldsTail:
                LeadingConditionRule killedWholeEaterRule = new(new Conditions.LegacyHack_IsABoss());
                killedWholeEaterRule.OnSuccess(ItemDropRule.ByCondition(new WandOfSparkingOnCondition(), Mod.Find<ModItem>("EaterSpirit").Type));
                npcLoot.Add(killedWholeEaterRule);
                break;
            case NPCID.SkeletronHead:
                npcName = "Skeletron";
                goto default;
            case NPCID.WallofFlesh:
                npcName = "Flesh";
                goto default;
            case NPCID.QueenSlimeBoss:
                npcName = "QueenSlime";
                goto default;
            case NPCID.TheDestroyer:
                npcName = "Destroyer";
                goto default;
            case NPCID.Retinazer:
            case NPCID.Spazmatism:
                LeadingConditionRule killedBothTwinsRule = new(new Conditions.MissingTwin());
                killedBothTwinsRule.OnSuccess(ItemDropRule.ByCondition(new WandOfSparkingOnCondition(), Mod.Find<ModItem>("TwinSpirit").Type));
                npcLoot.Add(killedBothTwinsRule);
                break;
            case NPCID.HallowBoss:
                npcName = "FairyQueen";
                goto default;
            case NPCID.CultistBoss:
                npcLoot.Add(ItemDropRule.ByCondition(new WandOfSparkingOnCondition(), Mod.Find<ModItem>("BossSpiritCultist").Type));
                break;
            case NPCID.MoonLordCore:
                npcLoot.Add(ItemDropRule.ByCondition(new WandOfSparkingOnCondition(), Mod.Find<ModItem>("BossSpiritMoonlord").Type));
                break;
            case NPCID.QueenBee:
                npcName = "Bee";
                goto default;
            case NPCID.Bee:
                break;
            default:
                if (!Mod.TryFind<ModItem>(npcName + "Spirit", out var spirit))
                    break;
                npcLoot.Add(ItemDropRule.ByCondition(new WandOfSparkingOnCondition(), spirit.Type));
                break;
        }
    }
}

public class WandOfSparkingOnCondition : IItemDropRuleCondition
{
    private static LocalizedText Descripition;

    public WandOfSparkingOnCondition()
    {
        Descripition ??= Language.GetOrRegister("Mods.MajorasMaskTribute.DropConditions.WandOfSparkingOn");
    }

    public bool CanDrop(DropAttemptInfo info)
    {
        //Item spirits don't matter in Brutal anyway
        return ModContent.GetInstance<Common.ServerConfig>().WandOfSparkingMode == Common.WandOfSparkingMode.On;
    }

    public bool CanShowItemDropInUI()
    {
        return ModContent.GetInstance<Common.ServerConfig>().WandOfSparkingMode == Common.WandOfSparkingMode.On;
    }

    public string GetConditionDescription()
    {
        return Descripition.Value;
    }
}
