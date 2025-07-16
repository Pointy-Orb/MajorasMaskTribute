using Terraria;
using Terraria.GameContent.Events;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Terraria.GameContent.Bestiary;
using System.Collections.Generic;
using MajorasMaskTribute.Common;
using Terraria.IO;
using Terraria.Utilities;
using System;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace MajorasMaskTribute.Content.Items;

public enum SongPlaying
{
    None,
    SongOfTime,
    SongOfHealing,
    InvertedSongOfTime
}

public class OcarinaOfTime : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.width = 26;
        Item.height = 22;
        Item.useTime = 1;
        Item.useAnimation = 5;
        Item.autoReuse = true;
        Item.rare = ItemRarityID.Expert;
        Item.useTurn = true;
        Item.holdStyle = ItemHoldStyleID.HoldFront;
    }

    public override bool? UseItem(Player player)
    {
        var desiredSongPlaying = player.GetModPlayer<OcarinaOfTimePlayer>().songPlaying;
        if (player.altFunctionUse == 2)
        {
            if (player.gravDir < 0)
            {
                desiredSongPlaying = SongPlaying.InvertedSongOfTime;
            }
            else
            {
                desiredSongPlaying = SongPlaying.SongOfHealing;
            }
        }
        else
        {
            desiredSongPlaying = SongPlaying.SongOfTime;
        }

        if (player.GetModPlayer<OcarinaOfTimePlayer>().songPlaying != desiredSongPlaying && player.GetModPlayer<OcarinaOfTimePlayer>().songPlaying != SongPlaying.None)
        {
            player.GetModPlayer<OcarinaOfTimePlayer>().animationTimer = 0;
        }
        player.GetModPlayer<OcarinaOfTimePlayer>().songPlaying = desiredSongPlaying;
        player.GetModPlayer<OcarinaOfTimePlayer>().animationTimer += 2;
        return null;
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override void UpdateInventory(Player player)
    {
        if (player.itemAnimation <= 0 && player.HeldItem.type == Type)
        {
            player.GetModPlayer<OcarinaOfTimePlayer>().songPlaying = SongPlaying.None;
        }
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        player.itemLocation += new Vector2(-5 * player.direction, 13 * player.gravDir);
    }

    public override void HoldStyle(Player player, Rectangle heldItemFrame)
    {
        player.itemLocation += new Vector2(-10 * player.direction, 7 * player.gravDir);
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var ocarinaTimer = Main.LocalPlayer.GetModPlayer<OcarinaOfTimePlayer>().animationTimer;
        foreach (Player player in Main.ActivePlayers)
        {
            if (Main.LocalPlayer.GetModPlayer<OcarinaOfTimePlayer>().animationTimer > ocarinaTimer)
            {
                ocarinaTimer = Main.LocalPlayer.GetModPlayer<OcarinaOfTimePlayer>().animationTimer;
            }
        }
        whiteScreen.ocarinaTimer = ocarinaTimer;
    }

    public static WhiteScreen whiteScreen;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.ClayBlock, 5)
            .AddIngredient(ItemID.FallenStar, 10)
            .AddTile(TileID.Furnaces)
            .Register();
    }
}

public class ShutUpImPlayingTheHealingSong : ModSceneEffect
{
    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/songofhealing");

    public override bool IsSceneEffectActive(Player player)
    {
        foreach (Player activePlayer in Main.ActivePlayers)
        {
            if (activePlayer.HeldItem.type == ModContent.ItemType<OcarinaOfTime>() && activePlayer.GetModPlayer<OcarinaOfTimePlayer>().songPlaying == SongPlaying.SongOfHealing)
            {
                return true;
            }
        }
        return false;
    }
}

public class ShutUpImPlayingTheOcarina : ModSceneEffect
{
    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/zelda-song-of-time");

    public override bool IsSceneEffectActive(Player player)
    {
        foreach (Player activePlayer in Main.ActivePlayers)
        {
            if (activePlayer.HeldItem.type == ModContent.ItemType<OcarinaOfTime>() && activePlayer.GetModPlayer<OcarinaOfTimePlayer>().songPlaying == SongPlaying.SongOfTime)
            {
                return true;
            }
        }
        return false;
    }
}

public class ShutUpImPlayingTheOcarinaBackwards : ModSceneEffect
{
    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/invertedsongoftime");

    public override bool IsSceneEffectActive(Player player)
    {
        if (player.HeldItem.type == ModContent.ItemType<OcarinaOfTime>() && player.GetModPlayer<OcarinaOfTimePlayer>().songPlaying == SongPlaying.InvertedSongOfTime)
        {
            return true;
        }
        return false;
    }
}

public class OcarinaOfTimePlayer : ModPlayer
{
    public SongPlaying songPlaying = SongPlaying.None;
    public int animationTimer = 0;

    public override bool CanUseItem(Item item)
    {
        foreach (Player player in Main.ActivePlayers)
        {
            if (player == Player) continue;
            if (player.GetModPlayer<OcarinaOfTimePlayer>().animationTimer > 0)
            {
                return false;
            }
        }
        return true;
    }

    public override void PostUpdate()
    {
        if (animationTimer >= 660 && songPlaying == SongPlaying.SongOfTime)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Player.SavePlayer(Main.ActivePlayerFileData);
                FileUtilities.Copy(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                ResetEverything();
            }
            animationTimer = 0;
        }
        if (animationTimer >= 300 && songPlaying == SongPlaying.InvertedSongOfTime)
        {
            Player.QuickSpawnItem(Player.GetSource_DropAsItem(), ModContent.ItemType<InvertedSongOfTime>());
            animationTimer = 0;
        }
        if (animationTimer >= 250 && songPlaying == SongPlaying.SongOfHealing)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                var homunculusNPC = npc.GetGlobalNPC<HomunculusNPC>();
                if (!homunculusNPC.isHomunculus) continue;
                if (new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight).Intersects(npc.Hitbox))
                {
                    //Some healing you got there.
                    npc.StrikeInstantKill();
                }
            }
            animationTimer = 0;
        }
        if (animationTimer > 0)
        {
            animationTimer--;
        }
    }

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        var ocarina = new Item();
        ocarina.SetDefaults(ModContent.ItemType<OcarinaOfTime>());
        yield return ocarina;
    }

    private static void ResetEverything()
    {
        if (!FileUtilities.Exists(Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave))
        {
            return;
        }
        Main.CheckForMoonEventsScoreDisplay();
        FileUtilities.Copy(Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave);
        foreach (NPC npc in Main.ActiveNPCs)
        {
            npc.Transform(NPCID.Bunny);
            npc.position = Vector2.Zero;
            npc.GetGlobalNPC<HomunculusNPC>().isHomunculus = false;
            npc.StrikeInstantKill();
        }
        WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY; j++)
            {
                if (!WorldGen.InWorld(i, j)) continue;
                WorldGen.Reframe(i, j, true);
            }
        }
        foreach (Player player in Main.ActivePlayers)
        {
            player.Teleport(new Vector2(Main.spawnTileX * 16, Main.spawnTileY * 16 - player.height), 6);
            player.velocity = Vector2.Zero;
        }
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.HasGivenName)
            {
                npc.Transform(NPCID.Bunny);
                npc.position = Vector2.Zero;
                npc.GetGlobalNPC<HomunculusNPC>().isHomunculus = false;
                npc.StrikeInstantKill();
            }
        }
        Main.StopRain();
        Main.windSpeedTarget = 0;
        Main.windSpeedCurrent = 0;
        Main.time = 0;
        Main.dayTime = true;
        if (Main.zenithWorld)
        {
            Main.afterPartyOfDoom = true;
            BirthdayParty.GenuineParty = true;
        }
        Main.forceHalloweenForToday = false;
        Main.forceXMasForToday = false;
        LanternNight.NextNightIsLanternNight = false;
        ApocalypseSystem.ResetCounter();
    }
}
