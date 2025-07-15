using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Terraria.ModLoader.Config;

namespace MajorasMaskTribute.Common;

public class MajorasMaskTributeConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(1f)]
    public float ScreenShakeStrength { get; set; }

    [ReloadRequired]
    public bool NoScaryTextures { get; set; }

    [ReloadRequired]
    public bool RealisticPhaseShading { get; set; }

    [ReloadRequired]
    public bool VanillaBloodMoonLogic { get; set; }

    public bool SupersizedMoon { get; set; }
    public bool SupersizedMoon2 { get; set; }
}
