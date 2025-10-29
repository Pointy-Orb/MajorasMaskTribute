using System.ComponentModel;
using Terraria.GameContent;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Terraria.ModLoader.Config;

namespace MajorasMaskTribute.Common;

public enum PauseDuringTransition
{
    Off,
    OnlyWithMiniClock,
    On
}

public enum WandOfSparkingMode
{
    Off,
    On,
    Brutal
}

public enum SigilSettings
{
    EarlyUse,
    Vanilla,
    Uncraftable
}

public class ServerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Header("General")]
    [DefaultValue(WandOfSparkingMode.Off)]
    [DrawTicks]
    public WandOfSparkingMode WandOfSparkingMode { get; set; }

    [DefaultValue(PauseDuringTransition.On)]
    [DrawTicks]
    public PauseDuringTransition PauseGameDuringDayTransitions { get; set; }

    public bool VanillaBloodMoonLogic { get; set; }

    public bool VanillaEclipseLogic { get; set; }

    public bool VanillaTimeRate { get; set; }

    public bool MurderForMasks { get; set; }

    [Header("Progression")]
    public bool SaveWorldAfterHardmodeStarts { get; set; }

    [DefaultValue(true)]
    public bool WOFDropsDemonConch { get; set; }

    public bool OldManDoesntAppearOnFirstDay { get; set; }

    public bool NoPlanteraToSummonGolem { get; set; }

    [DefaultValue(true)]
    public bool EatTempleKey { get; set; }

    [DefaultValue(SigilSettings.Vanilla)]
    [DrawTicks]
    [ReloadRequired]
    public SigilSettings SigilSettings { get; set; }
}

public enum CurrentDayDisplay
{
    Off,
    LastResort,
    On
}

public class ClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(1f)]
    public float ScreenShakeStrength { get; set; }

    public bool NoScaryTextures { get; set; }

    public bool RealisticPhaseShading { get; set; }

    [DefaultValue(true)]
    public bool GreenBackgroundDuringFinalDay { get; set; }

    public bool SupersizedMoon { get; set; }
    public bool SupersizedMoon2 { get; set; }

    public override void OnChanged()
    {
        if (Main.gameMenu)
        {
            return;
        }
        ModContent.RequestIfExists<Texture2D>("MajorasMaskTribute/Assets/Moon_scary" + (RealisticPhaseShading ? "_realistic" : ""), out ApocalypseSystem.scaryMoon);
        if (!NoScaryTextures)
        {
            ModContent.RequestIfExists<Texture2D>("MajorasMaskTribute/Assets/Moon_Pumpkin_scary" + (RealisticPhaseShading ? "_realistic" : ""), out TextureAssets.PumpkinMoon);
            ModContent.RequestIfExists<Texture2D>("MajorasMaskTribute/Assets/Moon_Snow_scary" + (RealisticPhaseShading ? "_realistic" : ""), out TextureAssets.SnowMoon);
        }
        else
        {
            TextureAssets.PumpkinMoon = Main.Assets.Request<Texture2D>("Images/Moon_Pumpkin");
            TextureAssets.SnowMoon = Main.Assets.Request<Texture2D>("Images/Moon_Snow");
        }
    }
}

public class UIConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(true)]
    public bool PreserveMapBetweenResets { get; set; }

    [DefaultValue(CurrentDayDisplay.LastResort)]
    [DrawTicks]
    public CurrentDayDisplay DisplayCurrentDay { get; set; }

    [DefaultValue(1.5f)]
    [Range(0.5f, 2.5f)]
    public float HUDClockSize { get; set; }
}
