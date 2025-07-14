using Terraria;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace MajorasMaskTribute.Common;

public class DayOfText : UIText
{
    int time = 0;
    public bool visible = true;
    public HourText hourText;

    public DayOfText(float textScale = 1f, bool large = true) : base("", textScale, large)
    {
        HAlign = 0.5f;
        VAlign = 0.4f;
    }

    public void DisplayDayOf()
    {
        SetText(Language.GetTextValue($"Mods.MajorasMaskTribute.Announcements.{(Main.dayTime ? "Dawn" : "Night")}{ApocalypseSystem.apocalypseDay}.Message"));
        hourText?.SetText(Language.GetTextValue($"Mods.MajorasMaskTribute.Announcements.{(Main.dayTime ? "Dawn" : "Night")}{ApocalypseSystem.apocalypseDay}.Hours"));
        time = 300;
    }

    public override void Update(GameTime gameTime)
    {
        if (time > 0)
        {
            time--;
        }
        else
        {
            SetText("");
            hourText?.SetText("");
        }
    }
}

public class HourText : UIText
{
    public HourText(float textScale = 0.5f, bool large = true) : base("", textScale, large)
    {
        HAlign = 0.5f;
        VAlign = 0.45f;
    }
}

public class ApocalypseText : UIText
{
    public ApocalypseText(float textScale = 1.3f, bool large = true) : base("", textScale, large)
    {
        HAlign = 0.5f;
        VAlign = 0.9f;
    }

    public override void Update(GameTime gameTime)
    {
        if (Utils.GetDayTimeAs24FloatStartingFromMidnight() < 25 || ApocalypseSystem.apocalypseDay < 2)
        {
            SetText("");
            return;
        }
        var timeLeft = 28.5 - Utils.GetDayTimeAs24FloatStartingFromMidnight();
        var hoursLeft = (int)Math.Truncate(timeLeft);
        double minutesLeft = (timeLeft - hoursLeft) * 60;
        SetText($"{hoursLeft.ToString("D1")}:{(minutesLeft < 10 ? "0" : "")}{minutesLeft.ToString("F2")}");
    }
}

public class DayDisplay : UIState
{
    public DayOfText dayOfText;
    public HourText hourText;
    public ApocalypseText apocalypseText;

    public override void OnInitialize()
    {
        dayOfText = new();
        hourText = new();
        apocalypseText = new();
        dayOfText.hourText = hourText;
        ApocalypseSystem.dayOfText = dayOfText;
        Append(dayOfText);
        Append(hourText);
        Append(apocalypseText);
    }
}

[Autoload(Side = ModSide.Client)]
public class DaySystem : ModSystem
{
    internal DayDisplay dayDisplay;

    private UserInterface _dayDisplay;

    public override void Load()
    {
        dayDisplay = new();
        dayDisplay.Activate();
        _dayDisplay = new UserInterface();
        _dayDisplay.SetState(dayDisplay);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        _dayDisplay?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "MajorasMaskTribute: Day Of Display",
                delegate
                {
                    _dayDisplay.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}


