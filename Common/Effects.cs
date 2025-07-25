using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.GameContent;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MajorasMaskTribute.Common;

public class BigScaryFlashShader : ScreenShaderData
{
    public BigScaryFlashShader(string passName) : base(passName)
    { }

    public override void Update(GameTime gameTime)
    {
        UseColor(3000, 2000, 1000);
    }
}

public class FinalNightScreenShaderData : ScreenShaderData
{
    public FinalNightScreenShaderData(string passName) : base(passName)
    { }

    public override void Update(GameTime gameTime)
    {
        if ((Main.bloodMoon || Main.SceneMetrics.BloodMoonMonolith || Main.LocalPlayer.bloodMoonMonolithShader) && !ModContent.GetInstance<ServerConfig>().VanillaBloodMoonLogic)
        {
            UseColor(0f, 0, 0);
        }
        else
        {
            UseColor(1, 0, 0.5f);
        }
    }
}

public class FinalNightSky : CustomSky
{
    Asset<Texture2D> white;

    public override bool IsActive()
    {
        return true;
    }

    public override void OnLoad()
    {
        white = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/white");
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        //spriteBatch.Draw(white.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);
    }

    public override void Activate(Vector2 position, params object[] args)
    {
    }

    public override void Deactivate(params object[] args)
    {
    }

    public override void Reset()
    {
    }
}

public class FinalNightEffect : ModSceneEffect
{
    public override int Music
    {
        get
        {
            return Utils.GetDayTimeAs24FloatStartingFromMidnight() >= 25 ? MusicLoader.GetMusicSlot(Mod, "Assets/Music/finalhours") : 0;
        }
    }

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override void SpecialVisuals(Player player, bool isActive)
    {
        player.ManageSpecialBiomeVisuals("MajorasMaskTribute:FinalNightShader", (isActive && !player.ZoneDirtLayerHeight && ((!player.ZoneRockLayerHeight && !player.ZoneUnderworldHeight) || Main.remixWorld)) || player.GetModPlayer<Content.Tiles.DoomMonolithPlayer>().doomMonolithActive || Content.Tiles.DoomMonolithSystem.nearDoomMonolith);
        if (!SkyManager.Instance["MajorasMaskTribute:FinalNightSky"].IsActive())
        {
            SkyManager.Instance["MajorasMaskTribute:FinalNightSky"].Activate(player.position);
        }
        if (!isActive && SkyManager.Instance["MajorasMaskTribute:FinalNightSky"].IsActive())
        {
            SkyManager.Instance["MajorasMaskTribute:FinalNightSky"].Deactivate();
        }
        if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 4.45f && Utils.GetDayTimeAs24FloatStartingFromMidnight() < 4.5f)
        {
            player.ManageSpecialBiomeVisuals("MajorasMaskTribute:FinalNightShader", isActive || player.GetModPlayer<Content.Tiles.DoomMonolithPlayer>().doomMonolithActive || Content.Tiles.DoomMonolithSystem.nearDoomMonolith);
        }
    }

    public override bool IsSceneEffectActive(Player player)
    {
        if (ApocalypseSystem.apocalypseDay > 2)
        {
            return true;
        }
        if (Main.dayTime || ApocalypseSystem.apocalypseDay < 2)
        {
            return false;
        }
        if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 25)
        {
            return !player.ZoneDirtLayerHeight && ((!player.ZoneRockLayerHeight && !player.ZoneUnderworldHeight) || Main.remixWorld);
        }
        return true;
    }
}
