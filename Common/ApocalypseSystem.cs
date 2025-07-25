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

    public static bool cycleNeverDisabled { get; private set; } = true;

    public static DayOfText dayOfText;

    public static bool FinalHours => apocalypseDay >= 2 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 25f;

    public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
    {
        bool vanillaTimeRate = ModContent.GetInstance<ServerConfig>().VanillaTimeRate;
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.GetModPlayer<InvertedSongOfTimePlayer>().invertedSongEquipped && !player.dead)
            {
                timeRate /= vanillaTimeRate ? 1.5 : 2.0;
                tileUpdateRate /= 1.5;
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
        if (Main.dedServ)
        {
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    static bool wasDay = true;

    public static Asset<Texture2D> scaryMoon;

    public override void Load()
    {
        try
        {
            scaryMoon = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/Moon_scary" + (ModContent.GetInstance<ClientConfig>().RealisticPhaseShading ? "_realistic" : ""));
            if (!ModContent.GetInstance<ClientConfig>().NoScaryTextures)
            {
                TextureAssets.PumpkinMoon = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/Moon_Pumpkin_scary" + (ModContent.GetInstance<ClientConfig>().RealisticPhaseShading ? "_realistic" : ""));
                TextureAssets.SnowMoon = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/Moon_Snow_scary" + (ModContent.GetInstance<ClientConfig>().RealisticPhaseShading ? "_realistic" : ""));
            }
            else
            {
                TextureAssets.PumpkinMoon = Main.Assets.Request<Texture2D>("Images/Moon_Pumpkin");
                TextureAssets.SnowMoon = Main.Assets.Request<Texture2D>("Images/Moon_Snow");
            }
        }
        catch (Exception e)
        {
            Mod.Logger.Error($"Error loading textures: {e}");
            throw;
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

    /*
    private static void IL_StopEclipse(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            var jumpLabel = il.DefineLabel();
            c.GotoNext(i => i.MatchStsfld(typeof(Main).GetField(nameof(Main.eclipse))));
            c.GotoPrev(MoveType.After, i => i.MatchBrtrue(out jumpLabel));
            c.EmitDelegate<Func<bool>>(() =>
            {
                return ModContent.GetInstance<ServerConfig>().VanillaEclipseLogic || !cycleActive;
            });
            c.Emit(Brfalse_S, jumpLabel);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
    }
	*/

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
                return ModContent.GetInstance<ServerConfig>().VanillaBloodMoonLogic || !cycleActive;
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
                return ModContent.GetInstance<ClientConfig>().NoScaryTextures || apocalypseDay < 0;
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
                if (ModContent.GetInstance<ClientConfig>().SupersizedMoon)
                {
                    startSize *= 2;
                    endSize *= 2;
                }
                if (ModContent.GetInstance<ClientConfig>().SupersizedMoon2)
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

            //Change the size of the sun during an eclipse
            c.GotoPrev(i => i.MatchLdsfld(typeof(TextureAssets).GetField(nameof(TextureAssets.Sun3))));
            c.GotoPrev(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.cloudAlpha))));
            c.GotoPrev(i => i.MatchLdarg(out _));
            var sunSizeIndex = 0;
            c.GotoPrev(MoveType.After, i => i.MatchStloc(out sunSizeIndex));
            c.Emit(Ldloc, sunSizeIndex);
            c.EmitDelegate<Func<float>>(() =>
            {
                if (!Main.eclipse)
                {
                    return 1f;
                }
                var multiplier = 1f;
                if (apocalypseDay >= 2)
                {
                    multiplier = Utils.Remap((float)Main.time, 0, (float)Main.dayLength, 7f, 10f);
                }
                else if (apocalypseDay == 1)
                {
                    multiplier = 3f;
                }
                if (ModContent.GetInstance<ClientConfig>().SupersizedMoon)
                {
                    multiplier *= 2;
                }
                if (ModContent.GetInstance<ClientConfig>().SupersizedMoon2)
                {
                    multiplier *= 3;
                }
                return multiplier;
            });
            c.Emit(Mul);
            c.Emit(Stloc, sunSizeIndex);
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
        var tPath = Path.ChangeExtension(path, ".tplr");
        if (!FileUtilities.Exists(tPath + ".dayone", cloud))
        {
            return;
        }
        if (cloud && SocialAPI.Cloud != null)
        {
            SocialAPI.Cloud.Delete(tPath + ".dayone");
        }
        else if (forceDeleteFile)
        {
            File.Delete(tPath + ".dayone");
        }
        else
        {
            Platform.Get<IPathService>().MoveToRecycleBin(tPath + ".dayone");
        }
    }

    public override void PostUpdateTime()
    {
        if (!Main.dayTime && wasDay && startChat)
        {
            if (Main.dedServ)
            {
                MajorasMaskTribute.NetData.NetDisplayDayOf(false, (byte)apocalypseDay);
            }
            else
            {
                dayOfText?.DisplayDayOf();
            }
            //MoonPhase.Full
            Main.moonPhase = 0;
        }
        if (ModContent.GetInstance<ServerConfig>().VanillaBloodMoonLogic)
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
        startChat = false;
        if (!FileUtilities.Exists(Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave) && FileUtilities.Exists(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave))
        {
            FileUtilities.Copy(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave);
        }
        if (!FileUtilities.Exists(Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld") + ".dayone", Main.ActiveWorldFileData.IsCloudSave) && FileUtilities.Exists(Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld"), Main.ActiveWorldFileData.IsCloudSave))
        {
            FileUtilities.Copy(Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld"), Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld") + ".dayone", Main.ActiveWorldFileData.IsCloudSave);
        }

        if (!FileUtilities.Exists(Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave) && FileUtilities.Exists(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.IsCloudSave))
        {
            FileUtilities.Copy(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave);
        }
        if (!FileUtilities.Exists(Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr") + ".dayone", Main.ActivePlayerFileData.IsCloudSave) && FileUtilities.Exists(Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr"), Main.ActivePlayerFileData.IsCloudSave))
        {
            FileUtilities.Copy(Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr"), Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr") + ".dayone", Main.ActivePlayerFileData.IsCloudSave);
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
        if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 4.5)
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
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }
        ResetLocalPlayer();
    }

    public static void ResetLocalPlayer()
    {
        if (!FileUtilities.Exists(Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave))
            return;
        FileUtilities.Copy(Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.IsCloudSave);
        if (FileUtilities.Exists(Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr") + ".dayone", Main.ActivePlayerFileData.IsCloudSave))
            FileUtilities.Copy(Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr") + ".dayone", Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr"), Main.ActivePlayerFileData.IsCloudSave);
        var newPlayer = Player.LoadPlayer(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.IsCloudSave);
        //Main.player[Main.myPlayer] = newPlayer.Player;
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
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 11.8f)
            {
                Main.maxRaining = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 5.1f, 9.7f, 0.24f, 0.61f);
            }
            else if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 14.1f)
            {
                Main.maxRaining = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 11.8f, 12.3f, 0.61f, 0.97f);
            }
            else if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 18.4f)
            {
                Main.maxRaining = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 14.1f, 16.6f, 0.97f, 0.31f);
            }
            else if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 22.3f)
            {
                Main.maxRaining = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 18.4f, 21.9f, 0.31f, 0.64f);
            }
            else
            {
                Main.maxRaining = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 22.3f, 26.1f, 0.64f, 0.34f);
            }
        }
        else if (Main.raining)
        {
            Main.StopRain();
        }
    }

    private void ManageWind()
    {
        if (apocalypseDay >= 2)
        {
            float speed;
            speed = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 4.5f, 8.2f, -0.1f, 0.8f);
            Main.windSpeedTarget = speed;
            Main.windSpeedCurrent = speed;
            Sandstorm.Happening = Utils.GetDayTimeAs24FloatStartingFromMidnight() > 11.7f;
            Sandstorm.Severity = speed;
        }
        if (apocalypseDay == 0)
        {
            float speed;
            speed = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 4.5f, 16f, 0, -0.3f);
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 16)
            {
                speed = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 16, 28, -0.3f, 0);
            }
            Main.windSpeedTarget = speed;
            Main.windSpeedCurrent = speed;
        }
        if (apocalypseDay == 1)
        {
            float speed;
            if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 13.1)
            {
                speed = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 4.5f, 11.1f, 0, -0.62f);
            }
            else
            {
                speed = Utils.Remap(Utils.GetDayTimeAs24FloatStartingFromMidnight(), 13.1f, 17.5f, -0.62f, -0.1f);
            }
            Main.windSpeedTarget = speed;
            Main.windSpeedCurrent = speed;
            Sandstorm.Happening = MathF.Abs(speed) >= 0.62f;
            Sandstorm.Severity = 0.41f;
        }
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!ModContent.GetInstance<ClientConfig>().GreenBackgroundDuringFinalDay)
        {
            return;
        }
        if (apocalypseDay >= 2 && Main.dayTime)
        {
            backgroundColor = backgroundColor.MultiplyRGB(new Color(0.7f, 1f, 0.7f));
        }
        if (((apocalypseDay >= 2 && Utils.GetDayTimeAs24FloatStartingFromMidnight() > 25) || Main.LocalPlayer.GetModPlayer<Content.Tiles.DoomMonolithPlayer>().doomMonolithActive || Content.Tiles.DoomMonolithSystem.nearDoomMonolith) && !ModContent.GetInstance<ServerConfig>().VanillaBloodMoonLogic && (Main.bloodMoon || Main.SceneMetrics.BloodMoonMonolith || Main.LocalPlayer.bloodMoonMonolithShader))
        {
            //Counteract blood moon brightening by doing our own darkening
            backgroundColor = Color.Black;
            tileColor = new Color(0.1f, 0.1f, 0.1f);
        }
    }

    public override void PostUpdateWorld()
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
            if (Main.dedServ)
            {
                MajorasMaskTribute.NetData.ResetPlayers();
            }
            DestroyEverything();
            apocalypseDay = 0;
            doApocalypseTimer = false;
            NetMessage.SendData(MessageID.WorldData);
            if (Main.dedServ)
            {
                MajorasMaskTribute.NetData.TurnOffBlowUpShader();
            }
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
                if (Main.dedServ)
                {
                    MajorasMaskTribute.NetData.NetDisplayDayOf(false, (byte)apocalypseDay);
                }
                else
                {
                    dayOfText?.DisplayDayOf();
                }
                //MoonPhase.Empty
                Main.moonPhase = 4;
                MiniatureClockTowerPlayer.PlayRooster();
            }
            else
            {
                apocalypseTimer = 30;
                doApocalypseTimer = true;
                if (Main.dedServ)
                {
                    MajorasMaskTribute.NetData.BlowUpClient(FullName);
                }
                else
                {
                    Main.instance.CameraModifiers.Add(new ApocalypseScreenShake(80, FullName));
                    SoundStyle explooood = new SoundStyle("MajorasMaskTribute/Assets/impact");
                    SoundEngine.PlaySound(explooood);
                }
            }
            if (Main.dedServ)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
        if (!Main.dedServ)
        {
            Main.LocalPlayer.ManageSpecialBiomeVisuals("MajorasMaskTribute:BigScaryFlashShader", doApocalypseTimer);
        }
        wasDay = Main.dayTime;
        if (!startChat)
        {
            startChat = true;
            BroadcastCurrentDay();
        }
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
    }

    public override void LoadWorldData(TagCompound tag)
    {
        apocalypseDay = tag.GetInt("apocalypseDay");
    }

    public class SaveDayOnePass : GenPass
    {
        public SaveDayOnePass(string name, float loadWeight) : base(name, loadWeight) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            if (!ModContent.GetInstance<ServerConfig>().SaveWorldAfterHardmodeStarts)
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
                if (ModContent.GetInstance<ServerConfig>().WandOfSparkingMode != WandOfSparkingMode.Off)
                {
                    player.GetModPlayer<WandOfSparkingModePlayer>().ResetInventory();
                }
                player.KillMe(deathReason, 99999, 0);
                if (Main.dedServ)
                {
                    NetMessage.SendPlayerDeath(player.whoAmI, deathReason, 99999, 0, false);
                }
            }
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.boss) npc.Transform(NPCID.Bunny);
                npc.StrikeInstantKill();
            }
            ResetWorldInner();
        }
    }

    public static void ResetWorldInner()
    {
        Rain.ClearRain();
        WorldGen.clearWorld();
        FileUtilities.Copy(Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave);
        FileUtilities.Copy(Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld") + ".dayone", Path.ChangeExtension(Main.ActiveWorldFileData.Path, ".twld"), Main.ActiveWorldFileData.IsCloudSave);
        WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
        startChat = true;
        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY; j++)
            {
                if (!WorldGen.InWorld(i, j)) continue;
                WorldGen.Reframe(i, j, true);
            }
        }
        if (Main.dedServ)
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
        bool sparedOldMan = ModContent.GetInstance<ServerConfig>().OldManDoesntAppearOnFirstDay;
        for (int i = 0; i < Main.npc.Length; i++)
        {
            if (!Main.npc[i].active)
                continue;
            var npc = Main.npc[i];
            if (!npc.HasGivenName && (npc.type != NPCID.OldMan || sparedOldMan))
            {
                npc.Transform(NPCID.Bunny);
                npc.position = Vector2.Zero;
                npc.GetGlobalNPC<HomunculusNPC>().isHomunculus = false;
                npc.StrikeInstantKill();
            }
            else if (npc.type == NPCID.OldMan)
            {
                sparedOldMan = true;
            }
            npc.netUpdate = true;
        }
        ResetApocalypseVariables();
    }

    public static void ResetApocalypseVariables()
    {
        Main.StopRain();
        Main.moonPhase = 0;
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
        Main.StopSlimeRain(false);
        if (ModContent.GetInstance<ServerConfig>().WandOfSparkingMode == WandOfSparkingMode.On)
        {
            foreach (Player player in Main.ActivePlayers)
            {
                player.GetModPlayer<WandOfSparkingModePlayer>().RegisterBossDeathsByMask();
            }
        }
        ResetCounter();
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(apocalypseDay);
        writer.Write(apocalypseTimer);
        writer.Write(doApocalypseTimer);
    }

    public override void NetReceive(BinaryReader reader)
    {
        apocalypseDay = reader.ReadInt32();
        apocalypseTimer = reader.ReadInt32();
        doApocalypseTimer = reader.ReadBoolean();
    }
}
