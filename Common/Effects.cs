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
        UseColor(1, 0, 0.5f);
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
        spriteBatch.Draw(white.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
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
            return Main.time >= Main.nightLength * 0.6 ? MusicLoader.GetMusicSlot(Mod, "Assets/Music/finalhours") : 0;
        }
    }

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override void SpecialVisuals(Player player, bool isActive)
    {
        player.ManageSpecialBiomeVisuals("MajorasMaskTribute:FinalNightShader", isActive);
        if (isActive && !SkyManager.Instance["MajorasMaskTribute:FinalNightSky"].IsActive())
        {
            SkyManager.Instance["MajorasMaskTribute:FinalNightSky"].Activate(player.position);
        }
        if (!isActive && SkyManager.Instance["MajorasMaskTribute:FinalNightSky"].IsActive())
        {
            SkyManager.Instance["MajorasMaskTribute:FinalNightSky"].Deactivate();
        }
        if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 4.45f && Utils.GetDayTimeAs24FloatStartingFromMidnight() < 4.5f)
        {
            player.ManageSpecialBiomeVisuals("MajorasMaskTribute:FinalNightShader", isActive);
        }
    }

    public override bool IsSceneEffectActive(Player player)
    {
        return !Main.dayTime && ApocalypseSystem.apocalypseDay >= 2 && !player.ZoneDirtLayerHeight && !player.ZoneRockLayerHeight && !player.ZoneUnderworldHeight;
    }
}
