
using Terraria;
using MajorasMaskTribute.Content.Items;
using System.Collections.Generic;
using ReLogic.Content;
using Terraria.GameContent;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MajorasMaskTribute.Common;

public class MMClockUIThreeD : UIElement
{
    public static Asset<Texture2D> texture;

    public bool active = false;

    public bool started = false;

    public override void OnInitialize()
    {
        HAlign = 0.5f;
        VAlign = 0.9f;
    }

    public override void OnActivate()
    {
        active = true;
    }

    public override void OnDeactivate()
    {
        active = false;
    }

    public Vector2 position
    {
        get
        {
            return new Vector2((float)Main.screenWidth * HAlign, (float)Main.screenHeight * VAlign);
        }
    }

    public Vector2 MarkerPosition(float scale)
    {
        var time = Main.time;
        if (!Main.dayTime)
        {
            time = DoubleLerp(0, Main.dayLength, Utils.GetLerpValue(0, Main.nightLength, time)) + Main.dayLength;
        }
        time += (Main.dayLength * ApocalypseSystem.apocalypseDay * 2);
        var pos = new Vector2();
        pos.X = position.X + Utils.Remap((float)time, 0, (float)Main.dayLength * 6, 36, frameRect.Width - 36) * scale - (frameRect.Width / 2) * scale;
        if (!ApocalypseSystem.cycleActive)
        {
            pos.X = position.X;
        }
        pos.Y = position.Y - 8 * scale;
        return pos;
    }

    public Vector2 ApocalypseTextPosition(float scale)
    {
        return new Vector2(position.X, position.Y - (34 + 36 * apocalypseAnimProgress) * scale);
    }

    private float apocalypseAnimProgress = 0f;
    private bool apocalypseTextActive = false;

    internal Rectangle markerRect => new Rectangle(144, 0, 70, 34);
    internal Rectangle clockBackRect => new Rectangle(72, 0, 70, 34);

    internal Rectangle sunMoonRect
    {
        get
        {
            var width = 70;
            var height = 34;
            var y = 36;
            var frame = 0;
            if (Main.dayTime)
            {
                if (Main.eclipse)
                {
                    frame = 2;
                }
                else if (Main.LocalPlayer.head == ArmorIDs.Head.Sunglasses)
                {
                    frame = 1;
                }
            }
            else
            {
                frame = 3;
                if (Main.snowMoon)
                {
                    frame = 4;
                }
                if (Main.pumpkinMoon)
                {
                    frame = 5;
                }
            }
            return new Rectangle((width + 2) * frame, y, width, height);
        }
    }

    internal Rectangle colonRect
    {
        get
        {
            var srect = sunMoonRect;
            srect.X = 432;
            srect.Y = 70;
            return srect;
        }
    }

    private double DoubleLerp(double value1, double value2, double amount)
    {
        return value1 + (value2 - value1) * amount;
    }

    public Rectangle frameRect
    {
        get
        {
            if (!ApocalypseSystem.cycleActive)
            {
                return new Rectangle(150, 96, 118, 16);
            }
            return new Rectangle(0, 82, 418, 30);
        }
    }

    private bool invertedSongActive
    {
        get
        {
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.GetModPlayer<Content.Items.InvertedSongOfTimePlayer>().invertedSongEquipped && !player.dead)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static Rectangle ApocalypseNumberRectFromNumber(int num)
    {
        int width = 16;
        int height = 26;
        int yPos = 0;
        int xOffset = 330;
        int frame = Int32.Clamp(num, 0, 9);
        return new Rectangle(xOffset + ((width + 2) * frame), yPos, width, height);
    }

    public static Rectangle NormalNumberRectFromNumber(int num)
    {
        int width = 6;
        int height = 12;
        int yPos = 0;
        int xOffset = 250;
        int frame = Int32.Clamp(num, 0, 9);
        return new Rectangle(xOffset + ((width + 2) * frame), yPos, width, height);
    }

    float scale => ModContent.GetInstance<ClientConfig>().HUDClockSize;

    private Rectangle GetDayFrame(int tDay)
    {
        int day = Int32.Clamp(tDay, 0, 2);
        int width = 114;
        if (tDay < 0)
        {
            return new Rectangle(420, 106, width, 8);
        }
        return new Rectangle((width + 2) * day, 72, width, 8);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!active || !started || ApocalypseSystem.apocalypseDay > 2)
        {
            return;
        }
        if (ApocalypseSystem.cycleActive)
        {
            DrawWhenCycleActive(spriteBatch);
        }
        else
        {
            DrawWhenCycleInactive(spriteBatch);
        }
    }

    private void DrawWhenCycleActive(SpriteBatch spriteBatch)
    {
        DrawFrame(spriteBatch, scale);
        if (!ApocalypseSystem.FinalHours)
        {
            DrawClock(spriteBatch, scale);
            apocalypseTextActive = false;
        }
        else
        {
            if (!apocalypseTextActive)
            {
                apocalypseAnimProgress = 1f;
            }
            apocalypseTextActive = true;
            DrawApocalypseUI(spriteBatch, scale);
        }
        DrawMarker(spriteBatch, scale);
        if (apocalypseAnimProgress > 0)
        {
            apocalypseAnimProgress = Single.Clamp(apocalypseAnimProgress - Single.Clamp(0.1f * apocalypseAnimProgress, 0, 0.015f), 0, apocalypseAnimProgress);
        }
    }

    private void DrawWhenCycleInactive(SpriteBatch spriteBatch)
    {
        DrawPeacefulFrame(spriteBatch, scale);
        DrawClock(spriteBatch, scale);
        DrawMarker(spriteBatch, scale);
    }

    private void DrawApocalypseUI(SpriteBatch spriteBatch, float scale)
    {
        var color = Color.Lerp(new Color(255, 255, 0), new Color(190, 0, 0), Utils.GetLerpValue(25f, 28.5f, Utils.GetDayTimeAs24FloatStartingFromMidnight(), true));
        if (ApocalypseSystem.apocalypseDay > 2)
        {
            color = new Color(190, 0, 0);
        }
        var fadein = Utils.Remap(apocalypseAnimProgress, 0.8f, 1f, 1f, 0f);
        color *= fadein;
        var textOrigin = new Vector2(-1, ApocalypseNumberRectFromNumber(0).Height);
        for (int i = 0; i < deathDigits.Count; i++)
        {
            var numFrame = ApocalypseNumberRectFromNumber(deathDigits[i]);
            var colonFrame = new Rectangle(510, 0, 8, 26);
            var adjustedOrigin = textOrigin;
            var xModifier = 0f;
            int posMultiplier = i - deathDigits.Count / 2;
            posMultiplier *= -1;
            xModifier = (numFrame.Width + 2) * posMultiplier;
            if (i < 2)
            {
                xModifier += 10;
            }
            if (i > 3)
            {
                xModifier -= 10;
            }
            adjustedOrigin.X += xModifier;
            spriteBatch.Draw(texture.Value, ApocalypseTextPosition(scale), numFrame, color, 0f, adjustedOrigin, scale, SpriteEffects.None, 0f);
            var colonOrigin = textOrigin;
            xModifier = (numFrame.Width + 2) * 1;
            xModifier += 10;
            colonOrigin.X += xModifier;
            spriteBatch.Draw(texture.Value, ApocalypseTextPosition(scale), colonFrame, color, 0f, colonOrigin, scale, SpriteEffects.None, 0f);
            colonOrigin = textOrigin;
            colonOrigin.X -= 18;
            spriteBatch.Draw(texture.Value, ApocalypseTextPosition(scale), colonFrame, color, 0f, colonOrigin, scale, SpriteEffects.None, 0f);
        }
        var moonPos = ApocalypseTextPosition(scale);
        if (apocalypseAnimProgress > keyStart)
        {
            moonPos.X += Main.rand.Next(-4, 5);
        }
        moonPos.Y -= 52 * scale;
        float moonScale = 1f;
        var moonColor = Color.Lerp(new Color(255, 255, 0), new Color(190, 0, 0), Utils.GetLerpValue(25f, 27.5f, Utils.GetDayTimeAs24FloatStartingFromMidnight(), true));
        if (apocalypseAnimProgress >= keyMiddle && apocalypseAnimProgress <= keyStart)
        {
            moonColor = Color.Lerp(Color.White, new Color(190, 0, 0), Utils.GetLerpValue(keyStart, keyMiddle, apocalypseAnimProgress, true));
            moonScale = Utils.Remap(apocalypseAnimProgress, keyStart, keyMiddle, 1f, 1.5f);
        }
        if (apocalypseAnimProgress < keyMiddle)
        {
            moonColor = Color.Lerp(new Color(190, 0, 0), moonColor, Utils.GetLerpValue(keyMiddle, 0f, apocalypseAnimProgress, true));
            moonScale = Utils.Remap(apocalypseAnimProgress, keyMiddle, 0f, 1.5f, 1f);
        }
        moonColor *= fadein;
        spriteBatch.Draw(texture.Value, moonPos, moonRect, moonColor, 0f, new Vector2(moonRect.Width / 2, moonRect.Height / 2), moonScale * scale, SpriteEffects.None, 0f);
    }

    const float keyStart = 0.7f;
    const float keyMiddle = 0.35f;

    internal Rectangle moonRect
    {
        get
        {
            int width = 40;
            int height = 40;
            int y = 28;
            int x = 432;
            int frame = 0;
            if (apocalypseAnimProgress < (keyStart + keyMiddle) / 2f && apocalypseAnimProgress > keyMiddle / 2f)
            {
                frame = 1;
            }
            return new Rectangle(x + (width + 2) * frame, y, width, height);
        }
    }

    private void DrawClock(SpriteBatch spriteBatch, float scale)
    {
        Vector2 origin = new(markerRect.Width / 2, markerRect.Height);
        float alpha = 0.6f;
        if (invertedSongActive)
        {
            var time = Main.GlobalTimeWrappedHourly;
            var pulseRateSeconds = 3f;
            time %= pulseRateSeconds;
            if (time > pulseRateSeconds / 2f)
            {
                alpha = Utils.Remap(time, pulseRateSeconds / 2f, pulseRateSeconds, 0.9f, 0.2f);
            }
            else
            {
                alpha = Utils.Remap(time, 0, pulseRateSeconds / 2f, 0.2f, 0.9f);
            }
        }
        spriteBatch.Draw(texture.Value, MarkerPosition(scale), clockBackRect, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
        var color = Color.White;
        var bloodMoon = !Main.dayTime && Main.bloodMoon && (ModContent.GetInstance<ServerConfig>().VanillaBloodMoonLogic || !ApocalypseSystem.cycleActive);
        if (bloodMoon)
        {
            color = Color.PaleVioletRed;
        }
        spriteBatch.Draw(texture.Value, MarkerPosition(scale), sunMoonRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        if (bloodMoon)
        {
            spriteBatch.Draw(texture.Value, MarkerPosition(scale), colonRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        if (invertedSongActive)
        {
            var outline = clockBackRect;
            outline.X = 0;
            spriteBatch.Draw(texture.Value, MarkerPosition(scale), outline, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            var arrow = new Rectangle(216, 0, 32, 20);
            var arrowOrigin = new Vector2(origin.X, origin.Y);
            arrowOrigin.X -= outline.Width - 2;
            arrowOrigin.Y -= 4;
            spriteBatch.Draw(texture.Value, MarkerPosition(scale), arrow, Color.White * alpha, 0f, arrowOrigin, scale, SpriteEffects.None, 0f);
        }
        for (int i = 0; i < normalDigits.Count; i++)
        {
            if (normalDigits[i] == 0 && i == 0)
            {
                continue;
            }
            var adjustedOrigin = origin;
            adjustedOrigin.X -= 22 + (8 * i);
            if (i >= normalDigits.Count / 2)
            {
                adjustedOrigin.X -= 8;
            }
            adjustedOrigin.Y -= 8;
            spriteBatch.Draw(texture.Value, MarkerPosition(scale), NormalNumberRectFromNumber(normalDigits[i]), Color.White, 0f, adjustedOrigin, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawMarker(SpriteBatch spriteBatch, float scale)
    {
        Vector2 origin = new(markerRect.Width / 2, markerRect.Height);
        spriteBatch.Draw(texture.Value, MarkerPosition(scale), markerRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        if (invertedSongActive && ApocalypseSystem.FinalHours)
        {
            float alpha = 0.6f;
            var time = Main.GlobalTimeWrappedHourly;
            var pulseRateSeconds = 3f;
            time %= pulseRateSeconds;
            if (time > pulseRateSeconds / 2f)
            {
                alpha = Utils.Remap(time, pulseRateSeconds / 2f, pulseRateSeconds, 0.9f, 0.2f);
            }
            else
            {
                alpha = Utils.Remap(time, 0, pulseRateSeconds / 2f, 0.2f, 0.9f);
            }
            var arrow = new Rectangle(216, 0, 32, 20);
            var arrowOrigin = new Vector2(origin.X, origin.Y);
            arrowOrigin.X -= arrow.Width / 2 + 6;
            arrowOrigin.Y -= 4;
            spriteBatch.Draw(texture.Value, MarkerPosition(scale), arrow, Color.White * alpha, 0f, arrowOrigin, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawPeacefulFrame(SpriteBatch spriteBatch, float scale)
    {
        var time = Main.time;
        if (!Main.dayTime)
        {
            time = DoubleLerp(0, Main.dayLength, Utils.GetLerpValue(0, Main.nightLength, time)) + Main.dayLength;
        }
        Vector2 origin = new(0, frameRect.Height);
        origin.X += Utils.Remap((float)time, 0, (float)Main.dayLength * 2, 0, frameRect.Width - 2);
        Vector2 prevOrigin = origin;
        prevOrigin.X -= frameRect.Width - 2;
        Vector2 forwardOrigin = origin;
        forwardOrigin.X += frameRect.Width - 2;

        Vector2 fillOffset = new(-2, -4);
        spriteBatch.Draw(texture.Value, position, GetDayFrame(-1), Color.White, 0, origin + fillOffset, scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture.Value, position, frameRect, Color.White, 0, origin, scale, SpriteEffects.None, 0f);

        spriteBatch.Draw(texture.Value, position, GetDayFrame(-1), Color.White * Utils.Remap((float)time, 0, (float)Main.dayLength * 2, 0, 1) * 0.4f, 0, prevOrigin + fillOffset, scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture.Value, position, frameRect, Color.White * Utils.Remap((float)time, 0, (float)Main.dayLength * 2, 0, 1), 0, prevOrigin, scale, SpriteEffects.None, 0f);

        spriteBatch.Draw(texture.Value, position, GetDayFrame(-1), Color.White * Utils.Remap((float)time, 0, (float)Main.dayLength * 2, 1, 0) * 0.4f, 0, forwardOrigin + fillOffset, scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture.Value, position, frameRect, Color.White * Utils.Remap((float)time, 0, (float)Main.dayLength * 2, 1, 0), 0, forwardOrigin, scale, SpriteEffects.None, 0f);
    }

    private void DrawFrame(SpriteBatch spriteBatch, float scale)
    {
        Vector2 origin = new(frameRect.Width / 2, frameRect.Height);
        float inactiveAlpha = 0.4f;
        spriteBatch.Draw(texture.Value, position, GetDayFrame(0), Color.White * (ApocalypseSystem.apocalypseDay == 0 ? 1 : inactiveAlpha), 0, new Vector2(origin.X - 36, origin.Y - 18), scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture.Value, position, GetDayFrame(1), Color.White * (ApocalypseSystem.apocalypseDay == 1 ? 1 : inactiveAlpha), 0, new Vector2(origin.X - 152, origin.Y - 18), scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture.Value, position, GetDayFrame(2), Color.White * (ApocalypseSystem.apocalypseDay == 2 ? 1 : inactiveAlpha), 0, new Vector2(origin.X - 268, origin.Y - 18), scale, SpriteEffects.None, 0f);

        spriteBatch.Draw(texture.Value, position, frameRect, Color.White, 0, origin, scale, SpriteEffects.None, 0f);
    }

    List<int> deathDigits = new();
    List<int> normalDigits = new();

    public override void Update(GameTime gameTime)
    {
        if (!active || !started)
        {
            return;
        }
        deathDigits.Clear();
        normalDigits.Clear();
        var timeLeft = 28.5 - Utils.GetDayTimeAs24FloatStartingFromMidnight();
        var hoursLeft = (int)Math.Truncate(timeLeft);
        double minutesLeft = (timeLeft - hoursLeft) * 60;
        double secondsLeft = minutesLeft - Math.Truncate(minutesLeft);
        minutesLeft = Math.Truncate(minutesLeft);
        secondsLeft *= 60;
        var timeString = $"{hoursLeft.ToString("D1")}{(minutesLeft < 10 ? "0" : "")}{minutesLeft.ToString()}{(secondsLeft < 10 ? "0" : "")}{Math.Truncate(secondsLeft).ToString()}";
        deathDigits.Add(0);
        foreach (char digit in timeString)
        {
            if (Int32.TryParse(digit.ToString(), out var number))
            {
                deathDigits.Add(number);
            }
        }
        if (ApocalypseSystem.apocalypseDay > 2)
        {
            deathDigits.Clear();
            for (int i = 0; i < 5; i++)
            {
                deathDigits.Add(0);
            }
        }

        var time = Utils.GetDayTimeAs24FloatStartingFromMidnight();
        while ((int)time > 12)
        {
            time -= 12;
        }
        var hours = (int)Math.Truncate(time);
        int minutes = (int)Math.Truncate((time - hours) * 60);
        var normalTimeString = $"{(hours < 10 ? "0" : "")}{hours.ToString("D1")}{(minutes < 10 ? "0" : "")}{minutes.ToString()}";
        foreach (char digit in normalTimeString)
        {
            if (Int32.TryParse(digit.ToString(), out var number))
            {
                normalDigits.Add(number);
            }
        }

        base.Update(gameTime);
    }
}

public class ClockDisplayThreeD : UIState
{
    public MMClockUIThreeD clock;

    private bool addedClock = false;

    public override void OnInitialize()
    {
        clock = new();

        Append(clock);
    }
}

[Autoload(Side = ModSide.Client)]
public class ClockSystemThreeD : ModSystem
{
    internal ClockDisplayThreeD clockDisplay;

    private UserInterface _clockDisplay;

    public override void Load()
    {
        MMClockUIThreeD.texture = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/mm_clock_3d");
        clockDisplay = new();
        _clockDisplay = new UserInterface();
        _clockDisplay.SetState(clockDisplay);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (!clockDisplay.clock.started && !Main.gameMenu)
        {
            clockDisplay.clock.started = true;
        }
        if (Main.gameMenu)
        {
            clockDisplay.clock.started = false;
        }
        if (Main.LocalPlayer.GetModPlayer<MMTimePlayer>().terminaWatch == MMTimePlayer.TerminaWatch.ThreeDeeEss && !clockDisplay.clock.active)
        {
            clockDisplay.Activate();
        }
        if (Main.LocalPlayer.GetModPlayer<MMTimePlayer>().terminaWatch != MMTimePlayer.TerminaWatch.ThreeDeeEss && clockDisplay.clock.active)
        {
            clockDisplay.Deactivate();
        }
        _clockDisplay?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "MajorasMaskTribute: Clock Display (ThreeDS)",
                delegate
                {
                    _clockDisplay.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}
