using Terraria;
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
    //Off-by-one because I couldn't help myself
    public static int apocalypseDay = 0;

    public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
    {
        timeRate /= 1.5;
    }

    bool wasDay = true;

    public static Asset<Texture2D> scaryMoon;
    public static Asset<Texture2D> scaryFrostMoon;

    public override void Load()
    {
        scaryMoon = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/Moon_scary");
        TextureAssets.PumpkinMoon = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/Moon_Pumpkin_scary");
        TextureAssets.SnowMoon = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/Moon_Snow_scary");
        IL_Main.DrawSunAndMoon += IL_BiggerMoon;
    }

    public static Texture2D GetScaryMoon()
    {
        return scaryMoon.Value;
    }
    public static Texture2D GetScarySnowMoon()
    {
        return scaryMoon.Value;
    }
    public static Texture2D GetScaryPumpkinMoon()
    {
        return scaryMoon.Value;
    }

    private static void IL_BiggerMoon(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            int scaleIndex = 0;
            int moonIndex = 0;
            //Change the moon texture
            c.GotoNext(i => i.MatchBrtrue(out _));
            c.GotoNext(i => i.MatchLdsfld(typeof(TextureAssets).GetField(nameof(TextureAssets.Moon))));
            c.GotoNext(MoveType.After, i => i.MatchStloc(out moonIndex));
            c.Emit(Call, typeof(ApocalypseSystem).GetMethod("GetScaryMoon"));
            c.Emit(Stloc, moonIndex);

            //Change the moon size depending on the current day and hour
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.ForcedMinimumZoom))));
            c.GotoNext(i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.ForcedMinimumZoom))));
            c.GotoNext(MoveType.After, i => i.MatchStloc(out scaleIndex));
            c.Emit(Ldloc, scaleIndex);
            c.Emit(Call, typeof(ApocalypseSystem).GetMethod("TimePassedMultiplier"));
            c.Emit(Mul);
            c.Emit(Stloc, scaleIndex);
        }
        catch
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<MajorasMaskTribute>(), il);
        }
    }

    public static float TimePassedMultiplier()
    {
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
        float hoursFloat = Utils.Remap((float)Main.time, 0, (float)Main.nightLength, startSize, endSize);
        return hoursFloat;
    }

    public override void PostWorldGen()
    {
        Main.time = 0;
        Main.dayTime = true;
    }

    public override void ClearWorld()
    {
        apocalypseDay = 0;
        startChat = false;
        if (!FileUtilities.Exists(Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave) && FileUtilities.Exists(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave))
        {
            FileUtilities.Copy(Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.IsCloudSave);
        }
        if (!FileUtilities.Exists(Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave) && FileUtilities.Exists(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.IsCloudSave))
        {
            FileUtilities.Copy(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave);
        }
    }

    private void BroadcastCurrentDay()
    {
        if (Utils.GetDayTimeAs24FloatStartingFromMidnight() >= 4.5 && Utils.GetDayTimeAs24FloatStartingFromMidnight() < 5 && Main.dayTime)
        {
            LocalizedText dayOne = Language.GetOrRegister("Mods.MajorasMaskTribute.Announcements.DawnOne");
            SendChatMessage(dayOne);
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
        Main.LocalPlayer.Spawn(PlayerSpawnContext.SpawningIntoWorld);
        Main.LocalPlayer.position = Vector2.Zero;
        DestroyEverything();
        apocalypseDay = 0;
        doApocalypseTimer = false;
    }

    bool startChat = false;
    bool wasGeneratingHardmode = false;
    bool doApocalypseTimer = false;
    int apocalypseTimer = 0;
    public override void PostUpdateEverything()
    {
        if (wasGeneratingHardmode && !WorldGen.IsGeneratingHardMode)
        {
        }
        wasGeneratingHardmode = WorldGen.IsGeneratingHardMode;
        if (apocalypseDay >= 2)
        {
            MaybeScreenShake();
        }
        if (apocalypseTimer > 0)
        {
            apocalypseTimer--;
        }
        if (Main.dayTime && !wasDay)
        {
            apocalypseDay++;
            if (apocalypseDay < 3)
            {
                LocalizedText dayOf = Language.GetOrRegister("Mods.MajorasMaskTribute.Announcements.DawnOne");
                switch (apocalypseDay)
                {
                    case 1:
                        dayOf = Language.GetOrRegister("Mods.MajorasMaskTribute.Announcements.DawnTwo");
                        break;
                    case 2:
                        dayOf = Language.GetOrRegister("Mods.MajorasMaskTribute.Announcements.DawnThree");
                        break;
                }
                SendChatMessage(dayOf);
                //MoonPhase.Empty
                Main.moonPhase = 4;
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
        Main.LocalPlayer.ManageSpecialBiomeVisuals("MajorasMaskTribute:BigScaryFlashShader", doApocalypseTimer);
        if (!Main.dayTime && wasDay && startChat)
        {
            LocalizedText dayOf = Language.GetOrRegister("Mods.MajorasMaskTribute.Announcements.NightOne");
            switch (apocalypseDay)
            {
                case 1:
                    dayOf = Language.GetOrRegister("Mods.MajorasMaskTribute.Announcements.NightTwo");
                    break;
                case 2:
                    dayOf = Language.GetOrRegister("Mods.MajorasMaskTribute.Announcements.NightThree");
                    break;
            }
            //MoonPhase.Full
            Main.moonPhase = 0;
            SendChatMessage(dayOf);
        }
        if (!startChat)
        {
            startChat = true;
            BroadcastCurrentDay();
        }
        wasDay = Main.dayTime;
        if (!Main.dayTime && apocalypseDay >= 2)
            FinalNightSpook();
    }

    private void FinalNightSpook()
    {
        if (!CreativePowerManager.Instance.GetPower<CreativePowers.FreezeWindDirectionAndStrength>().Enabled)
        {
            Main.windSpeedTarget = 0.8f;
            Main.windSpeedCurrent = 0.8f;
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
        if (apocalypseDay > 0)
        {
            tag["apocalypseDay"] = apocalypseDay;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey("apocalypseDay"))
        {
            apocalypseDay = (int)tag["apocalypseDay"];
        }
    }

    public class SaveDayOnePass : GenPass
    {
        public SaveDayOnePass(string name, float loadWeight) : base(name, loadWeight) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
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
                player.KillMe(deathReason, 99999, 0);
            }
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.boss) continue;
                npc.StrikeInstantKill();
            }
            FileUtilities.Copy(Main.ActiveWorldFileData.Path + ".dayone", Main.ActiveWorldFileData.Path, Main.ActiveWorldFileData.IsCloudSave);
            WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    if (!WorldGen.InWorld(i, j)) continue;
                    WorldGen.Reframe(i, j, true);
                }
            }
        }
        Main.raining = false;
        Main.windSpeedTarget = 0;
        Main.windSpeedCurrent = 0;
    }
}
