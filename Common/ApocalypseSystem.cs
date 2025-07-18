using Terraria;
using System.IO;
using Terraria.Social;
using ReLogic.OS;
using System;
using Terraria.GameContent.Events;
using MajorasMaskTribute.Content.Items;
using Terraria.ID;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using Terraria.Utilities;
using Terraria.IO;
using Terraria.Chat;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace MajorasMaskTribute.Common;

public class ApocalypseSystem : ModSystem
{
    public static bool cycleActive => apocalypseDay >= 0;

    //Off-by-one because I couldn't help myself
    public static int apocalypseDay { get; private set; } = 0;

    public static int resets { get; set; } = 0;

    public static bool cycleNeverDisabled { get; private set; } = true;

    public static DayOfText dayOfText;

    public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
    {
        bool vanillaTimeRate = ModContent.GetInstance<MajorasMaskTributeConfig>().VanillaTimeRate;
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.GetModPlayer<InvertedSongOfTimePlayer>().invertedSongEquipped)
            {
                timeRate /= vanillaTimeRate ? 1.5 : 2.0;
                return;
            }
        }
        if (vanillaTimeRate)
        {
            return;
        }
        timeRate /= 1.5;
    }

    public static void DisableCycle()
    {
        apocalypseDay = -1;
        cycleNeverDisabled = false;
    }

    public static void ResetCounter()
    {
        apocalypseDay = 0;
        wasDay = Main.dayTime;
    }

    static bool wasDay = true;

    public static Asset<Texture2D> scaryMoon;

    public override void Load()
    {
        scaryMoon = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/Moon_scary" + (ModContent.GetInstance<MajorasMaskTributeConfig>().RealisticPhaseShading ? "_realistic" : ""));
        if (!ModContent.GetInstance<MajorasMaskTributeConfig>().NoScaryTextures)
        {
            TextureAssets.PumpkinMoon = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/Moon_Pumpkin_scary" + (ModContent.GetInstance<MajorasMaskTributeConfig>().RealisticPhaseShading ? "_realistic" : ""));
            TextureAssets.SnowMoon = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/Moon_Snow_scary" + (ModContent.GetInstance<MajorasMaskTributeConfig>().RealisticPhaseShading ? "_realistic" : ""));
        }
        else
        {
            TextureAssets.PumpkinMoon = Main.Assets.Request<Texture2D>("Images/Moon_Pumpkin");
            TextureAssets.SnowMoon = Main.Assets.Request<Texture2D>("Images/Moon_Snow");
        }
        IL_Main.DrawSunAndMoon += IL_BiggerMoon;
        IL_Main.UpdateTime_StartNight += IL_StopBloodMoon;
        On_Main.EraseWorld += On_EraseWorld;
        On_FileUtilities.Delete += On_Delete;
    }

    public override void Unload()
    {
        TextureAssets.PumpkinMoon = Main.Assets.Request<Texture2D>("Images/Moon_Pumpkin");
        TextureAssets.SnowMoon = Main.Assets.Request<Texture2D>("Images/Moon_Snow");
    }

    private static void IL_StopBloodMoon(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            var jumpLabel = il.DefineLabel();
            c.GotoNext(i => i.MatchStsfld(typeof(Main).GetField(nameof(Main.bloodMoon))));
            c.GotoPrev(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.netMode))));
            c.GotoNext(MoveType.After, i => i.MatchBeq(out jumpLabel));
            c.EmitDelegate<Func<bool>>(() =>
            {
                return ModContent.GetInstance<MajorasMaskTributeConfig>().VanillaBloodMoonLogic || !cycleActive;
            });
            c.Emit(Brfalse_S, jumpLabel);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
    }

    private static void IL_BiggerMoon(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            int scaleIndex = 0;
            int moonIndex = 0;

            //Change the moon texture
            var skipTextureSetLabel = il.DefineLabel();
            c.GotoNext(i => i.MatchBrtrue(out _));
            c.GotoNext(i => i.MatchLdsfld(typeof(TextureAssets).GetField(nameof(TextureAssets.Moon))));
            c.GotoNext(MoveType.After, i => i.MatchStloc(out moonIndex));
            c.EmitDelegate<Func<bool>>(() =>
            {
                return ModContent.GetInstance<MajorasMaskTributeConfig>().NoScaryTextures || apocalypseDay < 0;
            });
            c.Emit(Brtrue_S, skipTextureSetLabel);
            c.EmitDelegate<Func<Texture2D>>(() =>
            {
                return scaryMoon.Value;
            });
            c.Emit(Stloc, moonIndex);
            c.MarkLabel(skipTextureSetLabel);

            //Change the moon size depending on the current day and hour
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.ForcedMinimumZoom))));
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.ForcedMinimumZoom))));
            c.GotoNext(MoveType.After, i => i.MatchStloc(out scaleIndex));
            c.Emit(Ldloc, scaleIndex);
            c.EmitDelegate<Func<float>>(() =>
            {
                if (apocalypseDay < 0)
                {
                    return 1f;
                }
                var startSize = 1;
                var endSize = 3;
                switch (apocalypseDay)
                {
                    case 1:
                        startSize = 3;
                        endSize = 7;
                        break;
                    case 2:
                        startSize = 10;
                        endSize = 20;
                        break;
                }
                if (ModContent.GetInstance<MajorasMaskTributeConfig>().SupersizedMoon)
                {
                    startSize *= 2;
                    endSize *= 2;
                }
                if (ModContent.GetInstance<MajorasMaskTributeConfig>().SupersizedMoon2)
                {
                    startSize *= 3;
                    endSize *= 3;
                }
                float hoursFloat = Utils.Remap((float)Main.time, 0, (float)Main.nightLength, startSize, endSize);
                return hoursFloat;
            });
            c.Emit(Mul);
            c.Emit(Stloc, scaleIndex);

            //Remove the moon shrinking as night closes
            int sizeMultIndex = 0;
            c.GotoPrev(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.time))));
            c.GotoNext(MoveType.After, i => i.MatchStloc(out sizeMultIndex));
            //The next variable is the moon's vertical position on the sky, which uses the variable we want to modify as a base.
            //Going to after that variable is set ensures that the size is changed without the posiiton.
            c.GotoNext(MoveType.After, i => i.MatchStloc(out _));
            c.Emit(Ldloc, sizeMultIndex);
            c.EmitDelegate<Func<double, double>>((double input) =>
            {
                if (!cycleActive)
                {
                    return input;
                }
                var midnightValue = (float)Math.Pow(1.0 - 0.5 * 2.0, 2.0);
                return (double)Utils.Remap((float)Main.time, (float)Main.nightLength / 2f, (float)Main.nightLength, midnightValue, 0);
            });
            c.Emit(Stloc, sizeMultIndex);

            //Give the normal moon phases, since it is not considered a regular moon texture the phases have to be added manually
            c.GotoNext(i => i.MatchLdsfld(typeof(TextureAssets).GetField(nameof(TextureAssets.SnowMoon))));
            c.GotoNext(i => i.MatchLdsfld(typeof(TextureAssets).GetField(nameof(TextureAssets.Moon))));
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.spriteBatch))));
            c.GotoNext(i => i.MatchLdcI4(0));
            c.GotoNext(MoveType.After, i => i.MatchLdcI4(0));
            c.Emit(Pop);
            c.EmitDelegate<Func<int>>(() =>
            {
                return scaryMoon.Value.Bounds.Width * Main.moonPhase;
            });

            //Make the moon fully opaque, even during a storm.
            c.GotoPrev(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.atmo))));
            c.GotoNext(i => i.MatchStloc(out _));
            c.Emit(Pop);
            c.Emit(Ldc_R4, 1f);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
    }

    private static void On_EraseWorld(On_Main.orig_EraseWorld orig, int i)
    {
        var path = Main.WorldList[i].Path + ".dayone";
        var tWldPath = Path.ChangeExtension(Main.WorldList[i].Path, ".twld") + ".dayone";
        bool isCloud = Main.WorldList[i].IsCloudSave;
        orig(i);
        try
        {
            if (!isCloud)
            {
                Platform.Get<IPathService>().MoveToRecycleBin(path);
                Platform.Get<IPathService>().MoveToRecycleBin(tWldPath);
            }
            else if (SocialAPI.Cloud != null)
            {
                SocialAPI.Cloud.Delete(path);
                SocialAPI.Cloud.Delete(tWldPath);
            }
        }
        catch
        {

        }
    }

    private static void On_Delete(On_FileUtilities.orig_Delete orig, string path, bool cloud, bool forceDeleteFile = false)
    {
        orig(path, cloud, forceDeleteFile);
        if (!FileUtilities.Exists(path + ".dayone", cloud))
        {
            return;
        }
        if (cloud && SocialAPI.Cloud != null)
        {
            SocialAPI.Cloud.Delete(path + ".dayone");
        }
        else if (forceDeleteFile)
        {
            File.Delete(path + ".dayone");
        }
        else
        {
            Platform.Get<IPathService>().MoveToRecycleBin(path + ".dayone");
        }
    }

    public override void PostUpdateTime()
    {
        if (!Main.dayTime && wasDay && startChat)
        {
            dayOfText?.DisplayDayOf();
            //MoonPhase.Full
            Main.moonPhase = 0;
        }
        if (ModContent.GetInstance<MajorasMaskTributeConfig>().VanillaBloodMoonLogic)
        {
            return;
        }
        if (apocalypseDay >= 2 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 25 && !Main.snowMoon && !Main.pumpkinMoon)
        {
            if (!Main.bloodMoon)
            {
                Main.bloodMoon = true;
                SendChatMessage(Language.GetText("Mods.MajorasMaskTribute.Announcements.EndIsNear"));
            }
        }
    }

    public override void PostWorldGen()
    {
        Main.time = 0;
        Main.dayTime = true;
    }

    public override void ClearWorld()
    {
        apocalypseDay = 0;
        cycleNeverDisabled = true;
        resets = 0;
        startChat = false;
        if (!FileUtilities.Exists(Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave) && FileUtilities.Exists(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave))
        {
            FileUtilities.Copy(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave);
        }
        if (!FileUtilities.Exists(Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave) && FileUtilities.Exists(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.IsCloudSave))
        {
            FileUtilities.Copy(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave);
        }
        if (!FileUtilities.Exists(Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld") + ".dayone", Main.ActiveWorldFileData.IsCloudSave) && FileUtilities.Exists(Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld"), Main.ActiveWorldFileData.IsCloudSave))
        {
            FileUtilities.Copy(Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld"), Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld") + ".dayone", Main.ActiveWorldFileData.IsCloudSave);
        }
    }

    private void BroadcastCurrentDay()
    {
        if (Utils.GetDayTimeAs24FloatStartingFromMidnight() >= 4.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() < 5 && Main.dayTime)
        {
            dayOfText?.DisplayDayOf();
            MiniatureClockTowerPlayer.PlayRooster();
        }
        else
        {
            int hours = 28 - (int)Utils.GetDayTimeAs24FloatStartingFromMidnight();
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 4.5)
            {
                hours = 4 - (int)Utils.GetDayTimeAs24FloatStartingFromMidnight();
            }
            LocalizedText dayMisc = Language.GetText("Mods.MajorasMaskTribute.Announcements.DayMisc").WithFormatArgs(new object[] { apocalypseDay + 1, hours + ((2 - apocalypseDay) * 24) });
            SendChatMessage(dayMisc);
        }
    }

    private void MaybeScreenShake()
    {
        int hours = (int)Utils.GetDayTimeAs24FloatStartingFromMidnight() - 4;
        if ((int)Utils.GetDayTimeAs24FloatStartingFromMidnight() < 4.5)
        {
            hours = 20 + (int)Utils.GetDayTimeAs24FloatStartingFromMidnight();
        }
        if (!Main.rand.NextBool(2000 - (hours * 20)))
        {
            return;
        }
        Main.instance.CameraModifiers.Add(new ApocalypseScreenShake(hours * 3, FullName));
    }

    public override void PostUpdatePlayers()
    {
        if (apocalypseTimer != 0 || !doApocalypseTimer)
        {
            return;
        }
        if (!FileUtilities.Exists(Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave))
            return;
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }
        FileUtilities.Copy(Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.IsCloudSave);
        var newPlayer = Player.LoadPlayer(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.IsCloudSave);
        newPlayer.SetAsActive();
        SoundEngine.PlaySound(SoundID.PlayerKilled);
        if (Main.LocalPlayer.Male)
        {
            SoundEngine.PlaySound(SoundID.PlayerHit);
        }
        else
        {
            SoundEngine.PlaySound(SoundID.FemaleHit);
        }
        Main.LocalPlayer.Spawn(PlayerSpawnContext.SpawningIntoWorld);
        Main.LocalPlayer.position = Vector2.Zero;
    }

    public static bool startChat { get; private set; } = false;
    bool doApocalypseTimer = false;
    int apocalypseTimer = 0;

    private void ManageWeather()
    {
        if (!CreativePowerManager.Instance.GetPower<CreativePowers.FreezeRainPower>().Enabled)
        {
            ManageRainAndClouds();
        }
        if (!CreativePowerManager.Instance.GetPower<CreativePowers.FreezeWindDirectionAndStrength>().Enabled)
        {
            ManageWind();
        }
    }

    private void ManageRainAndClouds()
    {
        if (apocalypseDay == 1 && Utils.GetDayTimeAs24FloatStartingFromMidnight() < 26.2f)
        {
            if (!Main.raining)
            {
                Main.StartRain();
            }
        }
        else if (Main.raining)
        {
            Main.StopRain();
        }
    }

    private void ManageWind()
    {
        if (!Main.dayTime && apocalypseDay >= 2)
        {
            Main.windSpeedTarget = 0.8f;
            Main.windSpeedCurrent = 0.8f;
        }
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!ModContent.GetInstance<MajorasMaskTributeConfig>().GreenBackgroundDuringFinalDay)
        {
            return;
        }
        if (apocalypseDay >= 2 && Main.dayTime)
        {
            backgroundColor = backgroundColor.MultiplyRGB(new Color(0.7f, 1f, 0.7f));
        }
        if (apocalypseDay >= 2 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 25 && !ModContent.GetInstance<MajorasMaskTributeConfig>().VanillaBloodMoonLogic && Main.bloodMoon)
        {
            //Counteract blood moon brightening by doing our own darkening
            backgroundColor = Color.Black;
            tileColor = new Color(0.1f, 0.1f, 0.1f);
        }
    }

    public override void PostUpdateEverything()
    {
        if (apocalypseDay < 0)
        {
            cycleNeverDisabled = false;
            return;
        }
        ManageWeather();
        if (apocalypseDay >= 2)
        {
            MaybeScreenShake();
        }
        if (apocalypseTimer <= 0 && doApocalypseTimer)
        {
            DestroyEverything();
            apocalypseDay = 0;
            doApocalypseTimer = false;
        }
        if (apocalypseTimer > 0)
        {
            apocalypseTimer--;
        }
        if (Main.dayTime && !wasDay && startChat)
        {
            apocalypseDay++;
            if (apocalypseDay < 3)
            {
                dayOfText?.DisplayDayOf();
                //MoonPhase.Empty
                Main.moonPhase = 4;
                MiniatureClockTowerPlayer.PlayRooster();
            }
            else
            {
                apocalypseTimer = 30;
                doApocalypseTimer = true;
                Main.instance.CameraModifiers.Add(new ApocalypseScreenShake(80, FullName));
                SoundStyle explooood = new SoundStyle("MajorasMaskTribute/Assets/impact");
                SoundEngine.PlaySound(explooood);
            }
        }
        foreach (Player player in Main.ActivePlayers)
        {
            player.ManageSpecialBiomeVisuals("MajorasMaskTribute:BigScaryFlashShader", doApocalypseTimer);
        }
        if (!startChat)
        {
            startChat = true;
            BroadcastCurrentDay();
        }
        wasDay = Main.dayTime;
    }

    private static void SendChatMessage(LocalizedText input)
    {
        if (Main.netMode == 0)
        {
            Main.NewText(input.Value, 50, byte.MaxValue, 130);
        }
        else if (Main.netMode == 2)
        {
            ChatHelper.BroadcastChatMessage(input.ToNetworkText(), new Color(50, 255, 130));
        }
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (apocalypseDay != 0)
        {
            tag["apocalypseDay"] = apocalypseDay;
        }
        if (resets > 0)
        {
            tag["resets"] = resets;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        apocalypseDay = tag.GetInt("apocalypseDay");
        resets = tag.GetInt("resets");
    }

    public class SaveDayOnePass : GenPass
    {
        public SaveDayOnePass(string name, float loadWeight) : base(name, loadWeight) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            if (!ModContent.GetInstance<MajorasMaskTributeConfig>().SaveWorldAfterHardmodeStarts)
                return;
            WorldGen.IsGeneratingHardMode = false;
            apocalypseDay = 0;
            Main.time = 0;
            Main.dayTime = true;
            WorldFile.SaveWorld();
            if (FileUtilities.Exists(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave))
            {
                FileUtilities.Copy(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave);
            }
            LocalizedText hardTimeReset = Language.GetOrRegister("Mods.MajorasMaskTribute.Announcements.HardmodeReset");
            SendChatMessage(hardTimeReset);
        }
    }

    public override void ModifyHardmodeTasks(List<GenPass> list)
    {
        list.Add(new SaveDayOnePass("Save New Day One", 800f));
    }

    private void DestroyEverything()
    {
        if (FileUtilities.Exists(Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave) && FileUtilities.Exists(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave))
        {
            foreach (Player player in Main.ActivePlayers)
            {
                var deathReason = new PlayerDeathReason();
                var messageNumber = Main.rand.Next(1, 6);
                deathReason.CustomReason = Language.GetText($"Mods.MajorasMaskTribute.DeathMessages.Moon{messageNumber}").WithFormatArgs(player.name).ToNetworkText();
                if (ModContent.GetInstance<MajorasMaskTributeConfig>().WandOfSparkingMode != WandOfSparkingMode.Off)
                {
                    player.GetModPlayer<WandOfSparkingModePlayer>().ResetInventory();
                }
                player.KillMe(deathReason, 99999, 0);
            }
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.boss) npc.Transform(NPCID.Bunny);
                npc.StrikeInstantKill();
            }
            ResetWorldInner();
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                foreach (Player player in Main.ActivePlayers)
                {
                    NetMessage.BootPlayer(player.whoAmI, NetworkText.FromKey("Mods.MajorasMaskTribute.MultiplayerResetMessage.Violent"));
                }
                Netplay.Disconnect = true;
            }
        }
    }

    public static void ResetWorldInner()
    {
        int tempResets = resets;
        WorldGen.clearWorld();
        FileUtilities.Copy(Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave);
        FileUtilities.Copy(Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld") + ".dayone", Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld"), Main.ActiveWorldFileData.IsCloudSave);
        WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
        resets = tempResets;
        startChat = true;
        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY; j++)
            {
                if (!WorldGen.InWorld(i, j)) continue;
                WorldGen.Reframe(i, j, true);
            }
        }
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            var chunkSize = 50;
            for (int i = 0; i < Main.maxTilesX; i += chunkSize)
            {
                for (int j = 0; j < Main.maxTilesY; j += chunkSize)
                {
                    var rangeX = chunkSize;
                    var rangeY = chunkSize;
                    if (!WorldGen.InWorld(i + rangeX, j))
                    {
                        rangeX = Main.maxTilesX - i;
                    }
                    if (!WorldGen.InWorld(i, j + rangeY))
                    {
                        rangeY = Main.maxTilesY - j;
                    }
                    NetMessage.SendTileSquare(-1, i, j, rangeX, rangeY);
                }
            }
            NetMessage.SendData(MessageID.WorldData);
        }
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            projectile.Kill();
        }
        foreach (Item item in Main.ActiveItems)
        {
            item.active = false;
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI, 1f);
            }
        }
        for (int i = 0; i < Main.npc.Length; i++)
        {
            if (!Main.npc[i].active)
                return;
            var npc = Main.npc[i];
            if (!npc.HasGivenName && (npc.type != NPCID.OldMan || ModContent.GetInstance<MajorasMaskTributeConfig>().OldManDoesntAppearOnFirstDay))
            {
                npc.Transform(NPCID.Bunny);
                npc.position = Vector2.Zero;
                npc.GetGlobalNPC<HomunculusNPC>().isHomunculus = false;
                npc.StrikeInstantKill();
            }
            npc.netUpdate = true;
        }
        ResetApocalypseVariables();
    }

    public static void ResetApocalypseVariables()
    {
        Main.StopRain();
        Main.raining = false;
        Main.maxRain = 0;
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
        Main.fastForwardTimeToDawn = false;
        Main.fastForwardTimeToDusk = false;
        TempleKeySystem.anybodyUsedTempleKey = false;
        LanternNight.NextNightIsLanternNight = false;
        if (ModContent.GetInstance<MajorasMaskTributeConfig>().WandOfSparkingMode == WandOfSparkingMode.On)
        {
            foreach (Player player in Main.ActivePlayers)
            {
                player.GetModPlayer<WandOfSparkingModePlayer>().RegisterBossDeathsByMask();
            }
        }
        resets++;
        ResetCounter();
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            NetMessage.SendData(MessageID.WorldData);
        }
    }
}
