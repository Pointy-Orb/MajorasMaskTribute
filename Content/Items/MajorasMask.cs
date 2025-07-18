using Terraria;
using Terraria.Utilities;
using Terraria.IO;
using Terraria.Audio;
using MajorasMaskTribute.Common;
using Terraria.Localization;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using Terraria.ID;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MajorasMaskTribute.Content.Items;

public class MajorasMask : ModItem
{
    private static Asset<Texture2D> glow;

    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.width = 28;
        Item.height = 24;
        Item.rare = ItemRarityID.Red;
        Item.UseSound = ApocalypseSystem.cycleActive ? new SoundStyle("MajorasMaskTribute/Assets/daydoodledoo") : SoundID.NPCDeath62;
        Item.value = Item.sellPrice(2, 50);
    }

    public override void SetStaticDefaults()
    {
        glow = ModContent.Request<Texture2D>(Texture + "Glow");
    }

    public override void UpdateInventory(Player player)
    {
        Item.UseSound = ApocalypseSystem.cycleActive && ApocalypseSystem.cycleNeverDisabled ? new SoundStyle("MajorasMaskTribute/Assets/daydoodledoo") : SoundID.NPCDeath62;
    }

    public override bool? UseItem(Player player)
    {
        if (ApocalypseSystem.cycleActive)
        {
            if (ApocalypseSystem.cycleNeverDisabled)
            {
                Main.time = 0;
                Main.dayTime = true;
                ApocalypseSystem.dayOfText.BroadcastNewDay();
            }
            ApocalypseSystem.DisableCycle();
        }
        else
        {
            ApocalypseSystem.ResetCounter();
            Main.instance.CameraModifiers.Add(new ApocalypseScreenShake(20, FullName));
        }
        return true;
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);
        spriteBatch.Draw(glow.Value, drawPosition, itemFrame, Color.White, rotation, drawOrigin, scale, SpriteEffects.None, 0);
    }
}

public class MoonlordStuff : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (!ModContent.GetInstance<MajorasMaskTributeConfig>().SaveWorldAfterHardmodeStarts)
            return;
        if (NPC.downedMoonlord)
            return;
        if (npc.type != NPCID.MoonLordCore)
            return;

        ApocalypseSystem.ResetCounter();
        Main.time = 0;
        Main.dayTime = true;
        WorldFile.SaveWorld();
        if (FileUtilities.Exists(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave))
        {
            FileUtilities.Copy(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave);
        }
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type != NPCID.MoonLordCore)
        {
            return;
        }
        npcLoot.Add(ItemDropRule.ByCondition(new DoesntHaveMaskAlreadyCondition(), ModContent.ItemType<MajorasMask>()));
    }
}

public class DoesntHaveMaskAlreadyCondition : IItemDropRuleCondition
{
    private static LocalizedText Descripition;

    public DoesntHaveMaskAlreadyCondition()
    {
        Descripition ??= Language.GetOrRegister("Mods.MajorasMaskTribute.Conditions.MajorasMask");
    }

    public bool CanDrop(DropAttemptInfo info)
    {
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.HasItem(ModContent.ItemType<MajorasMask>()))
            {
                return false;
            }
        }
        return true;
    }

    public bool CanShowItemDropInUI()
    {
        return true;
    }

    public string GetConditionDescription()
    {
        return Descripition.Value;
    }
}
