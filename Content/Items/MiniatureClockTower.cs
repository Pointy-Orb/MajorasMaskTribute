using Terraria;
using System.Collections.Generic;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Graphics;

namespace MajorasMaskTribute.Content.Items;

public class MiniatureClockTower : ModItem
{
    public override void Load()
    {
        bellSound = new SoundStyle("MajorasMaskTribute/Assets/bell");
    }

    public override void SetDefaults()
    {
        bellSound = new SoundStyle("MajorasMaskTribute/Assets/bell");
        Item.width = 26;
        Item.height = 22;
        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
    }

    SoundStyle bellSound;

    public override void UpdateVanity(Player player)
    {
        UpdateAccessory(player, false);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var towerPlayer = player.GetModPlayer<MiniatureClockTowerPlayer>();
        if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 19.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.25)
        {
            if (towerPlayer.bellsPlayed < 1)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.3 && towerPlayer.bellsPlayed < 2)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.35 && towerPlayer.bellsPlayed < 3)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.4 && towerPlayer.bellsPlayed < 4)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.45 && towerPlayer.bellsPlayed < 5)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
        }
        else if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 28.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.25 && (Common.ApocalypseSystem.apocalypseDay < 2 && !Main.dayTime))
        {
            if (towerPlayer.bellsPlayed < 1)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.3 && towerPlayer.bellsPlayed < 2)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.35 && towerPlayer.bellsPlayed < 3)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.4 && towerPlayer.bellsPlayed < 4)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.45 && towerPlayer.bellsPlayed < 5)
            {
                SoundEngine.PlaySound(bellSound);
                towerPlayer.bellsPlayed++;
            }
        }
        else if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 28.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 25 && (Common.ApocalypseSystem.apocalypseDay >= 2 && !Main.dayTime))
        {
            List<double> bellTimes = new();
            for (int i = 0; i < 30; i++)
            {
                bellTimes.Add((double)(i * 5) / 60 + 25);
            }
            for (int i = 0; i < 12; i++)
            {
                bellTimes.Add((double)(i * 2.5) / 60 + 27.5);
            }
            for (int i = 0; i < 20; i++)
            {
                bellTimes.Add((double)(i * 1.5) / 60 + 28);
            }
            for (int i = 0; i < bellTimes.Count; i++)
            {
                if (towerPlayer.bellsPlayed > i) continue;
                if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > bellTimes[i])
                {
                    SoundEngine.PlaySound(bellSound);
                    towerPlayer.bellsPlayed++;
                }
            }
        }
        else
        {
            towerPlayer.bellsPlayed = 0;
        }
        towerPlayer.miniClockEquipped = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup("MajorasMaskTribute:AnyWatch")
            .AddIngredient(ItemID.StoneBlock, 10)
            .AddIngredient(ItemID.Wood, 5)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class AnyWatchSystem : ModSystem
{
    public override void AddRecipeGroups()
    {
        RecipeGroup group = new RecipeGroup(() => $"{Language.GetTextValue("Mods.MajorasMaskTribute.AnyWatchGroup")}", ItemID.CopperWatch, ItemID.TinWatch, ItemID.SilverWatch, ItemID.TungstenWatch, ItemID.GoldWatch, ItemID.PlatinumWatch);
        RecipeGroup.RegisterGroup("MajorasMaskTribute:AnyWatch", group);
    }
}

public class BellRingingEffect : ModSceneEffect
{
    public override int Music => 0;

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override bool IsSceneEffectActive(Player player)
    {
        if (!Main.dayTime && Common.ApocalypseSystem.apocalypseDay >= 2)
            return false;
        if (Main.dayTime && Common.ApocalypseSystem.dayOfText.time > 0)
            return true;
        return player.GetModPlayer<MiniatureClockTowerPlayer>().miniClockEquipped && ((Utils.GetDayTimeAs24FloatStartingFromMidnight() < 19.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.2) || (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 28.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.2));
    }
}

public class MiniatureClockTowerPlayer : ModPlayer
{
    public bool miniClockEquipped = false;
    public int bellsPlayed = 0;

    public override void ResetEffects()
    {
        miniClockEquipped = false;
    }

    public override void OnEnterWorld()
    {
        wasDay = Main.dayTime;
        nightHowl = new SoundStyle("MajorasMaskTribute/Assets/nighthowl");
        dayDoodleDoo = new SoundStyle("MajorasMaskTribute/Assets/daydoodledoo");
    }

    static SoundStyle nightHowl;
    static SoundStyle dayDoodleDoo;

    bool wasDay = true;
    bool started = false;
    public override void PostUpdate()
    {
        if (!started)
        {
            started = true;
            wasDay = Main.dayTime;
            return;
        }
        if (Main.netMode == NetmodeID.Server)
            return;
        if (!Main.dayTime && wasDay)
        {
            SoundEngine.PlaySound(nightHowl);
        }
        if (Main.dayTime && !wasDay && Common.ApocalypseSystem.apocalypseDay <= 1)
        {
            SoundEngine.PlaySound(dayDoodleDoo);
        }
        wasDay = Main.dayTime;
    }

    public static void PlayRooster()
    {
        SoundEngine.PlaySound(dayDoodleDoo);
    }
}
