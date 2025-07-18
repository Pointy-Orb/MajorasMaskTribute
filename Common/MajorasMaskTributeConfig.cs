using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Terraria.ModLoader.Config;

namespace MajorasMaskTribute.Common;

public enum WandOfSparkingMode
{
    Off,
    On,
    Brutal
}

public class ServerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(WandOfSparkingMode.Off)]
    [DrawTicks]
    public WandOfSparkingMode WandOfSparkingMode { get; set; }

    [DefaultValue(true)]
    public bool PauseGameDuringDayTransitions { get; set; }

    public bool VanillaBloodMoonLogic { get; set; }

    public bool SaveWorldAfterHardmodeStarts { get; set; }

    public bool OldManDoesntAppearOnFirstDay { get; set; }

    public bool NoPlanteraToSummonGolem { get; set; }

    public bool VanillaTimeRate { get; set; }
}

public class ClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(1f)]
    public float ScreenShakeStrength { get; set; }

    public bool NoScaryTextures { get; set; }

    [ReloadRequired]
    public bool RealisticPhaseShading { get; set; }

    [DefaultValue(true)]
    public bool GreenBackgroundDuringFinalDay { get; set; }

    public bool SupersizedMoon { get; set; }
    public bool SupersizedMoon2 { get; set; }
}
