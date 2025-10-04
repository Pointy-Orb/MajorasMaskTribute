
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

    public Vector2 markerPosition(float scale)
    {
        var time = Main.time;
        if (!Main.dayTime)
        {
            time = DoubleLerp(0, Main.dayLength, Utils.GetLerpValue(0, Main.nightLength, time)) + Main.dayLength;
        }
        time += (Main.dayLength * ApocalypseSystem.apocalypseDay * 2);
        var pos = new Vector2();
        pos.X = position.X + Utils.Remap((float)time, 0, (float)Main.dayLength * 6, 36, frameRect.Width - 36) * scale - (frameRect.Width / 2) * scale;
        pos.Y = position.Y - 8 * scale;
        return pos;
    }

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
                if (Main.snowMoon)
                {
                    frame = 4;
                }
                if (Main.pumpkinMoon)
                {
                    frame = 5;
                }
                frame = 3;
            }
            return new Rectangle((width + 2) * frame, y, width, height);
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
        int width = 12;
        int height = 20;
        int yPos = 14;
        int xOffset = 250;
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
        return new Rectangle((width + 2) * day, 72, width, 8);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!active || !started)
        {
            return;
        }
        DrawFrame(spriteBatch, scale);
        if (!ApocalypseSystem.FinalHours && ApocalypseSystem.apocalypseDay < 3)
        {
            DrawClock(spriteBatch, scale);
        }
        DrawMarker(spriteBatch, scale);
    }

    private void DrawClock(SpriteBatch spriteBatch, float scale)
    {
        Vector2 origin = new(markerRect.Width / 2, markerRect.Height);
        spriteBatch.Draw(texture.Value, markerPosition(scale), clockBackRect, Color.White * 0.6f, 0f, origin, scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture.Value, markerPosition(scale), sunMoonRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
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
            spriteBatch.Draw(texture.Value, markerPosition(scale), NormalNumberRectFromNumber(normalDigits[i]), Color.White, 0f, adjustedOrigin, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawMarker(SpriteBatch spriteBatch, float scale)
    {
        Vector2 origin = new(markerRect.Width / 2, markerRect.Height);
        spriteBatch.Draw(texture.Value, markerPosition(scale), markerRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
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
        var timeString = $"{hoursLeft.ToString("D1")}{(minutesLeft < 10 ? "0" : "")}{minutesLeft.ToString("F2")}";
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
