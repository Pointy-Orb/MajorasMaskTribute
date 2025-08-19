using Terraria;
using Terraria.GameContent.Drawing;
using System;
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
        defaultVolume = bellSound.Volume;
    }

    public override void SetDefaults()
    {
        bellSound = new SoundStyle("MajorasMaskTribute/Assets/bell");
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.MiniatureClockTowerTile>());
        Item.maxStack = 1;
        Item.width = 26;
        Item.height = 22;
        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
        Item.vanity = true;
    }

    static SoundStyle bellSound;
    private static float defaultVolume;

    public override void UpdateVanity(Player player)
    {
        UpdateAccessory(player, false);
    }

    public static void TryPlayBells(ref int bellsPlayed, Vector2? position = null, float? volume = null, Action onRing = null, bool broadcast = false)
    {
        bellSound.Volume = volume ?? 1;
        if (NPC.MoonLordCountdown > 0)
        {
            if (NPC.MoonLordCountdown % 540 == 0 && NPC.MoonLordCountdown > 540)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
            }
        }
        else if (Main.curMusic == MusicLoader.GetMusicSlot(MajorasMaskTribute.mod, "Assets/Music/finalhours") && !(Utils.GetDayTimeAs24FloatStartingFromMidnight() < 28.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 25 && (Common.ApocalypseSystem.apocalypseDay >= 2 && !Main.dayTime)))
        {
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() % (5f / 60f) < 0.001f)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
            }
        }
        else if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 19.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.25)
        {
            if (bellsPlayed < 1)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.3 && bellsPlayed < 2)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.35 && bellsPlayed < 3)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.4 && bellsPlayed < 4)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.45 && bellsPlayed < 5)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
            }
        }
        else if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 28.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.25 && (Common.ApocalypseSystem.apocalypseDay < 2 && !Main.dayTime))
        {
            if (bellsPlayed < 1)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.3 && bellsPlayed < 2)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.35 && bellsPlayed < 3)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.4 && bellsPlayed < 4)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
            }
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.45 && bellsPlayed < 5)
            {
                if (broadcast && Main.dedServ)
                {
                    var pos = position ?? Vector2.Zero;
                    MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                }
                SoundEngine.PlaySound(bellSound, position);
                onRing?.Invoke();
                bellsPlayed++;
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
                if (bellsPlayed > i) continue;
                if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > bellTimes[i])
                {
                    onRing?.Invoke();
                    if (broadcast && Main.dedServ)
                    {
                        var pos = position ?? Vector2.Zero;
                        MajorasMaskTribute.NetData.PlayBell(pos, TileDrawing.IsVisible(Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)]));
                    }
                    else
                    {
                        SoundEngine.PlaySound(bellSound, position);
                    }
                    bellsPlayed++;
                }
            }
        }
        else
        {
            bellsPlayed = 0;
        }
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var towerPlayer = player.GetModPlayer<MiniatureClockTowerPlayer>();
        TryPlayBells(ref towerPlayer.bellsPlayed, volume: 0.5f);
        towerPlayer.miniClockEquipped = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup("MajorasMaskTribute:AnyWatch")
            .AddIngredient(ItemID.StoneBlock, 10)
            .AddRecipeGroup(RecipeGroupID.Wood, 4)
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
    //Using actual silence (Music => 0) will cut the preceding music off abruptly instead of fading it out.
    public override int Music => Common.DayOfText.newDay ? 0 : MusicLoader.GetMusicSlot(Mod, "Assets/Music/silence");

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override float GetWeight(Player player)
    {
        return 1f;
    }

    public override bool IsSceneEffectActive(Player player)
    {
        if (!Main.dayTime && Common.ApocalypseSystem.apocalypseDay >= 2)
            return false;
        if (Main.eclipse && !ModContent.GetInstance<Common.ServerConfig>().VanillaEclipseLogic)
            return false;
        if (Main.dayTime && Common.ApocalypseSystem.dayOfText.time > 0)
            return true;
        return (player.GetModPlayer<MiniatureClockTowerPlayer>().miniClockEquipped || Tiles.TowerTileSystem.nearClockTower) && ((Utils.GetDayTimeAs24FloatStartingFromMidnight() < 19.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 19.2) || (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 28.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 28.2));
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
        dayRoosterReal = new SoundStyle("MajorasMaskTribute/Assets/daydoodledoo_old");
    }

    static SoundStyle nightHowl;
    static SoundStyle dayDoodleDoo;
    static SoundStyle dayRoosterReal;

    public static bool wasDay = true;
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
        if (Main.dayTime && !wasDay && Common.ApocalypseSystem.apocalypseDay <= 1 && !(Main.eclipse && !ModContent.GetInstance<Common.ServerConfig>().VanillaEclipseLogic))
        {
            if (Common.ApocalypseSystem.cycleActive)
            {
                SoundEngine.PlaySound(dayDoodleDoo);
            }
            else
            {
                SoundEngine.PlaySound(dayRoosterReal);
            }
        }
        wasDay = Main.dayTime;
    }

    public static void PlayRooster()
    {
        if (Main.eclipse && !ModContent.GetInstance<Common.ServerConfig>().VanillaEclipseLogic)
        {
            return;
        }
        SoundEngine.PlaySound(dayDoodleDoo);
    }
}
