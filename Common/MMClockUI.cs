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

public class MMClockUI : UIElement
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

    public Vector2 diamondPosition => new Vector2((float)Main.screenWidth * HAlign, ((float)Main.screenHeight * VAlign) - (20 * scale));

    public Vector2 timerPosition => diamondPosition;

    public Rectangle frameRect
    {
        get
        {
            int width = 140;
            int height = 90;
            if (Main.dayTime)
            {
                return new Rectangle(0, 92, width, height);
            }
            else
            {
                return new Rectangle(0, 184, width, height);
            }
        }
    }

    public Rectangle diamondRect
    {
        get
        {
            int size = 52;
            if (invertedSongActive)
            {
                return new Rectangle(142, 54, size, size);
            }
            else
            {
                return new Rectangle(142, 0, size, size);
            }
        }
    }

    public Rectangle dayRect
    {
        get
        {
            int size = 52;
            int xPos = 142;
            switch (ApocalypseSystem.apocalypseDay)
            {
                case 0:
                    return new Rectangle(xPos, 108, size, size);
                case 1:
                    return new Rectangle(xPos, 162, size, size);
                case 2:
                    return new Rectangle(xPos, 216, size, size);
                default:
                    return new Rectangle(0, 0, 1, 1);
            }
        }
    }

    public Rectangle sunMoonRect
    {
        get
        {
            int xPos = 196;
            int size = 24;
            int frame = 0;
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
                if (Main.pumpkinMoon)
                {
                    frame = 5;
                }
                else if (Main.snowMoon)
                {
                    frame = 4;
                }
            }
            return new Rectangle(xPos, (size + 2) * frame, size, size);
        }
    }

    public Rectangle numberRect
    {
        get
        {
            var hours = (int)Utils.GetDayTimeAs24FloatStartingFromMidnight();
            var size = 18;
            while (hours > 12)
            {
                hours -= 12;
            }
            hours--;
            return new Rectangle(222, (size + 2) * hours, 20, size);
        }
    }

    public Rectangle apocalypseTimerRect => new Rectangle(0, 276, 90, 18);

    public Rectangle defaultApocalypseTimeRect => new Rectangle(0, 296, apocalypseTimerRect.Width, apocalypseTimerRect.Height);

    public readonly Rectangle littleSunRect = new(222, 240, 18, 18);

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
        int width = 10;
        int height = 14;
        int yPos = 276;
        int xOffset = 92;
        int frame = Int32.Clamp(num, 0, 9);
        return new Rectangle(xOffset + ((width + 2) * frame), yPos, width, height);
    }

    float scale => ModContent.GetInstance<ClientConfig>().HUDClockSize;

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!active || !started)
        {
            return;
        }
        DrawFrame(spriteBatch, scale);
        if (!ApocalypseSystem.FinalHours && ApocalypseSystem.apocalypseDay < 3)
        {
            DrawDiamond(spriteBatch, scale);
            DrawDay(spriteBatch, scale);
            DrawMiniSun(spriteBatch, scale);
        }
        else
        {
            DrawApocalypseTimer(spriteBatch, scale);
        }
        if (ApocalypseSystem.apocalypseDay < 3)
        {
            DrawSunMoon(spriteBatch, scale);
        }
    }

    private void DrawFrame(SpriteBatch spriteBatch, float scale)
    {
        Vector2 origin = new(frameRect.Width / 2, frameRect.Height);
        spriteBatch.Draw(texture.Value, position, frameRect, Color.White, 0, origin, scale, SpriteEffects.None, 0f);
    }

    private void DrawDiamond(SpriteBatch spriteBatch, float scale)
    {
        var color = Color.White;
        if (invertedSongActive)
        {
            var time = Main.GlobalTimeWrappedHourly;
            var pulseRateSeconds = 1.5f;
            time %= pulseRateSeconds;
            var tint = 1f;
            if (time > pulseRateSeconds / 2f)
            {
                tint = Utils.Remap(time, pulseRateSeconds / 2f, pulseRateSeconds, 1f, 0.5f);
            }
            else
            {
                tint = Utils.Remap(time, 0, pulseRateSeconds / 2f, 0.5f, 1);
            }
            color = new Color(tint, tint, tint + 0.4f);
        }
        spriteBatch.Draw(texture.Value, diamondPosition, diamondRect, color, 0, new Vector2(diamondRect.Width / 2, diamondRect.Height / 2), scale, SpriteEffects.None, 0f);
    }

    private void DrawDay(SpriteBatch spriteBatch, float scale)
    {
        spriteBatch.Draw(texture.Value, diamondPosition, dayRect, Color.White, 0, new Vector2(dayRect.Width / 2, dayRect.Height / 2), scale, SpriteEffects.None, 0f);
    }

    private void DrawMiniSun(SpriteBatch spriteBatch, float scale)
    {
        var timeInHour = Utils.GetDayTimeAs24FloatStartingFromMidnight() % 1;
        var rotation = Utils.Remap(timeInHour, 0, 1, 0, MathF.PI * 2);
        var origin = new Vector2(littleSunRect.Width / 2, littleSunRect.Height * 2);
        spriteBatch.Draw(texture.Value, diamondPosition, littleSunRect, Color.White, rotation, origin, scale, SpriteEffects.None, 0f);
    }

    private void DrawSunMoon(SpriteBatch spriteBatch, float scale)
    {
        var rotation = Utils.Remap((float)Main.time - (Main.dayTime ? 900 : 270), 0, Main.dayTime ? (float)Main.dayLength : (float)Main.nightLength, 0, MathF.PI) - MathHelper.PiOver2;
        var origin = new Vector2(sunMoonRect.Width / 2, sunMoonRect.Height * 3.3f);
        var numOrigin = new Vector2(numberRect.Width / 2, numberRect.Height * 5.6f);
        var color = Color.White;
        if (!Main.dayTime && Main.bloodMoon && ModContent.GetInstance<ServerConfig>().VanillaBloodMoonLogic)
        {
            color = Color.Red;
        }
        spriteBatch.Draw(texture.Value, position, sunMoonRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture.Value, position, numberRect, Color.White, rotation, numOrigin, scale, SpriteEffects.None, 0f);
    }

    List<int> digits = new();

    public override void Update(GameTime gameTime)
    {
        if (!active || !started)
        {
            return;
        }
        digits.Clear();
        var timeLeft = 28.5 - Utils.GetDayTimeAs24FloatStartingFromMidnight();
        var hoursLeft = (int)Math.Truncate(timeLeft);
        double minutesLeft = (timeLeft - hoursLeft) * 60;
        var timeString = $"{hoursLeft.ToString("D1")}{(minutesLeft < 10 ? "0" : "")}{minutesLeft.ToString("F2")}";
        foreach (char digit in timeString)
        {
            if (Int32.TryParse(digit.ToString(), out var number))
            {
                digits.Add(number);
            }
        }
        if (ApocalypseSystem.apocalypseDay > 2)
        {
            digits.Clear();
            for (int i = 0; i < 5; i++)
            {
                digits.Add(0);
            }
        }

        base.Update(gameTime);
    }

    private void DrawApocalypseTimer(SpriteBatch spriteBatch, float scale)
    {
        var origin = new Vector2(apocalypseTimerRect.Width / 2, apocalypseTimerRect.Height / 2);
        spriteBatch.Draw(texture.Value, timerPosition, apocalypseTimerRect, Color.White, 0, origin, scale, SpriteEffects.None, 0f);
        var timeColor = new Color(35, 33, 1);
        if (Utils.GetDayTimeAs24FloatStartingFromMidnight() > 27.5f || ApocalypseSystem.apocalypseDay > 2)
        {
            var time = Main.GlobalTimeWrappedHourly;
            var pulseRateSeconds = 0.65f;
            time %= pulseRateSeconds;
            var tint = 1f;
            if (time > pulseRateSeconds / 2f)
            {
                tint = Utils.Remap(time, pulseRateSeconds / 2f, pulseRateSeconds, 0.7f, 0);
            }
            else
            {
                tint = Utils.Remap(time, 0, pulseRateSeconds / 2f, 0, 0.7f);
            }
            timeColor = new Color(0.137f + tint, 0.129f, 0);
        }
        spriteBatch.Draw(texture.Value, timerPosition, defaultApocalypseTimeRect, timeColor, 0, origin, scale, SpriteEffects.None, 0f);
        for (int i = 0; i < digits.Count; i++)
        {
            var numFrame = ApocalypseNumberRectFromNumber(digits[i]);
            var adjustedOrigin = origin;
            adjustedOrigin.Y -= apocalypseTimerRect.Height * 0.11f;
            if (i < 1)
            {
                adjustedOrigin.X -= apocalypseTimerRect.Width * 0.155f;
            }
            else if (i < 2)
            {
                adjustedOrigin.X -= apocalypseTimerRect.Width * 0.375f;
            }
            else if (i < 3)
            {
                adjustedOrigin.X -= apocalypseTimerRect.Width * 0.51f;
            }
            else if (i < 4)
            {
                adjustedOrigin.X -= apocalypseTimerRect.Width * 0.734f;
            }
            else
            {
                adjustedOrigin.X -= apocalypseTimerRect.Width * 0.865f;
            }
            spriteBatch.Draw(texture.Value, timerPosition, numFrame, timeColor, 0, adjustedOrigin, scale, SpriteEffects.None, 0f);
        }
    }
}

public class ClockDisplay : UIState
{
    public MMClockUI clock;

    private bool addedClock = false;

    public override void OnInitialize()
    {
        clock = new();

        Append(clock);
    }
}

[Autoload(Side = ModSide.Client)]
public class ClockSystem : ModSystem
{
    internal ClockDisplay clockDisplay;

    private UserInterface _clockDisplay;

    public override void Load()
    {
        MMClockUI.texture = ModContent.Request<Texture2D>("MajorasMaskTribute/Assets/mm_clock");
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
        if (Main.LocalPlayer.GetModPlayer<MMTimePlayer>().accTerminaWatch && !clockDisplay.clock.active)
        {
            clockDisplay.Activate();
        }
        if (!Main.LocalPlayer.GetModPlayer<MMTimePlayer>().accTerminaWatch && clockDisplay.clock.active)
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
                "MajorasMaskTribute: Clock Display",
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
