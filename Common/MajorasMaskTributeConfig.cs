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

    public bool RealisticPhaseShading { get; set; }

    [DefaultValue(true)]
    public bool GreenBackgroundDuringFinalDay { get; set; }

    public bool SupersizedMoon { get; set; }
    public bool SupersizedMoon2 { get; set; }

    public override void OnChanged()
    {
        try
        {
            ApocalypseSystem.scaryMoon = Mod.Assets.Request<Texture2D>("Assets/Moon_scary" + (ModContent.GetInstance<ClientConfig>().RealisticPhaseShading ? "_realistic" : ""));
            if (!NoScaryTextures)
            {
                TextureAssets.PumpkinMoon = Mod.Assets.Request<Texture2D>("Assets/Moon_Pumpkin_scary" + (RealisticPhaseShading ? "_realistic" : ""));
                TextureAssets.SnowMoon = Mod.Assets.Request<Texture2D>("Assets/Moon_Snow_scary" + (RealisticPhaseShading ? "_realistic" : ""));
            }
            else
            {
                TextureAssets.PumpkinMoon = Main.Assets.Request<Texture2D>("Images/Moon_Pumpkin");
                TextureAssets.SnowMoon = Main.Assets.Request<Texture2D>("Images/Moon_Snow");
            }
        }
        catch { }
    }
}
