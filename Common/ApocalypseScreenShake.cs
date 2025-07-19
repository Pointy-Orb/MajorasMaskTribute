using Terraria;
using Microsoft.Xna.Framework;
using System;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;

namespace MajorasMaskTribute.Common;

public class ApocalypseScreenShake : ICameraModifier
{
    private float _shakeStrength = 0;
    private int framesTotal;
    private int framesElapsed;
    const int durationMultiplier = 3;

    public string UniqueIdentity { get; private set; }
    public bool Finished { get; private set; }

    public void Update(ref CameraInfo cameraInfo)
    {
        if (framesElapsed >= framesTotal || ApocalypseSystem.apocalypseDay < 2)
        {
            Finished = true;
            return;
        }
        float progress = Utils.GetLerpValue(0, framesTotal, framesElapsed);
        progress -= (float)((int)(progress / 0.025f)) * 0.025f;
        float lerpAmount = Utils.Remap(progress, 0, 0.025f, -1, 1);
        var targetPos = new Vector2(cameraInfo.CameraPosition.X, cameraInfo.CameraPosition.Y + _shakeStrength);
        cameraInfo.CameraPosition = Vector2.Lerp(cameraInfo.CameraPosition, targetPos, lerpAmount * ModContent.GetInstance<ClientConfig>().ScreenShakeStrength);
        if (!Main.gameInactive && !Main.gamePaused)
        {
            framesElapsed++;
            if (framesElapsed % durationMultiplier == 0)
            {
                _shakeStrength--;
            }
        }
    }

    public ApocalypseScreenShake(int shakeStrength, string uniqueIdentity = null)
    {
        _shakeStrength = shakeStrength;
        framesTotal = shakeStrength * durationMultiplier;
        UniqueIdentity = uniqueIdentity;
    }
}
