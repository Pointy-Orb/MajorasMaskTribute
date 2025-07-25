using Terraria;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;

namespace MajorasMaskTribute.Common;

public class SigilUse : GlobalItem
{
    public override void Load()
    {
        On_Player.ItemCheck_CheckCanUse += CanUseSigilAnyway;
        On_Player.ItemCheck_UseEventItems += UseSigilAnywayEvent;
    }

    public override bool CanUseItem(Item item, Player player)
    {
        if (item.type == ItemID.CelestialSigil && ModContent.GetInstance<ServerConfig>().SigilSettings == SigilSettings.Uncraftable)
        {
            return false;
        }
        return true;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (ModContent.GetInstance<ServerConfig>().SigilSettings != SigilSettings.Uncraftable)
        {
            return;
        }
        if (item.type != ItemID.CelestialSigil)
        {
            return;
        }
        foreach (TooltipLine line in tooltips)
        {
            if (line.Name != "ItemName")
            {
                line.Hide();
            }
        }
        tooltips.Add(new TooltipLine(Mod, "NoUse", Language.GetTextValue("CommonItemTooltip.MechdusaSummonNotDuringEverything")));
    }

    private static bool CanUseSigilAnyway(On_Player.orig_ItemCheck_CheckCanUse orig, Player self, Item item)
    {
        if (item.type != ItemID.CelestialSigil)
        {
            return orig(self, item);
        }
        if (ModContent.GetInstance<ServerConfig>().SigilSettings != SigilSettings.EarlyUse)
        {
            return orig(self, item);
        }
        return true;
    }

    private static void UseSigilAnywayEvent(On_Player.orig_ItemCheck_UseEventItems orig, Player self, Item item)
    {
        if (item.type != ItemID.CelestialSigil)
        {
            orig(self, item);
            return;
        }
        if (ModContent.GetInstance<ServerConfig>().SigilSettings != SigilSettings.EarlyUse)
        {
            orig(self, item);
            return;
        }
        if (self.ItemTimeIsZero && self.itemAnimation > 0 && !NPC.AnyDanger() && !NPC.AnyoneNearCultists())
        {
            SoundEngine.PlaySound(SoundID.Roar, new Microsoft.Xna.Framework.Vector2(self.position.X, self.position.Y));
            self.ApplyItemTime(item);
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                WorldGen.StartImpendingDoom(720);
            }
            else
            {
                NetMessage.SendData(61, -1, -1, null, self.whoAmI, -8f);
            }
        }
    }
}

public class SigilRecipe : ModSystem
{
    public override void PostAddRecipes()
    {
        if (ModContent.GetInstance<ServerConfig>().SigilSettings != SigilSettings.Uncraftable)
        {
            return;
        }
        for (int i = 0; i < Recipe.numRecipes; i++)
        {
            var recipe = Main.recipe[i];
            if (recipe.TryGetResult(ItemID.CelestialSigil, out _))
            {
                recipe.DisableRecipe();
            }
        }
    }
}
