using Terraria;
using Terraria.GameContent;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MajorasMaskTribute.Common;

public class DayOfText : UIText
{
    public int time { get; private set; } = 0;
    public bool visible = true;
    public HourText hourText;
    public UIText Dawn;
    public BlackScreen blackScreen;

    private Color defaultShadow;
    private float defaultScale;
    private float defaultHourScale => defaultScale * hourScaleRelative;
    private float defaultDawnScale => defaultScale * 0.7f;
    private float hourScaleRelative = 0.5f;
    public static bool newDay { get; private set; } = false;

    public DayOfText(float textScale = 1f, float hourScaleRelative = 0.5f) : base("", textScale, true)
    {
        HAlign = 0.5f;
        VAlign = 0.4f;
        defaultScale = textScale;
        Dawn = new("", defaultDawnScale, true);
        Dawn.HAlign = 0.5f;
        Dawn.VAlign = 0.35f * textScale;
        defaultShadow = ShadowColor;
        this.hourScaleRelative = hourScaleRelative;
        hourText = new HourText(defaultHourScale);
    }

    public void DisplayDayOf(bool overridePause = false)
    {
        BlackScreen.overridePauseForEffect = overridePause;

        if (BlackScreen.PauseGameDuringDayTransitions && Main.dayTime)
        {
            defaultScale = 1.4f;
        }
        else
        {
            defaultScale = 1;
        }

        var message = (BlackScreen.PauseGameDuringDayTransitions && Main.dayTime ? "" : Language.GetTextValue($"Mods.MajorasMaskTribute.Announcements.{(Main.dayTime ? "DawnOf" : "NightOf")}")) + Language.GetTextValue($"Mods.MajorasMaskTribute.Announcements.Day{ApocalypseSystem.apocalypseDay}");
        SetText(message, Main.dayTime ? (BlackScreen.PauseGameDuringDayTransitions ? defaultScale * (Utils.Remap(ApocalypseSystem.apocalypseDay, 0, 2, 1, 1.5f)) : defaultScale) : defaultScale, true);

        int hours = 28 - (int)Utils.GetDayTimeAs24FloatStartingFromMidnight() + ((2 - ApocalypseSystem.apocalypseDay) * 24);
        var hourMessage = Language.GetTextValue("Mods.MajorasMaskTribute.Announcements.HoursRemaining", hours);
        hourText?.SetText(hourMessage,
                Main.dayTime ? (BlackScreen.PauseGameDuringDayTransitions ? defaultHourScale * (Utils.Remap(ApocalypseSystem.apocalypseDay, 0, 2, 1, 1.5f)) : defaultHourScale) : defaultHourScale, true);
        time = 300;

        if (!BlackScreen.PauseGameDuringDayTransitions || !Main.dayTime)
        {
            hourText.VAlign = HourText.defaultVAlign;
            return;
        }
        Dawn.VAlign = 0.35f - (Utils.Remap(ApocalypseSystem.apocalypseDay, 0, 2, 0, 0.03f)) - 0.02f;
        hourText.TextColor = Color.Black;
        hourText.VAlign = (HourText.defaultVAlign + (Utils.Remap(ApocalypseSystem.apocalypseDay, 0, 2, 0, 0.03f))) + 0.03f;
        Dawn.SetText(Language.GetTextValue("Mods.MajorasMaskTribute.Announcements.DawnOf"), defaultDawnScale * (Utils.Remap(ApocalypseSystem.apocalypseDay, 0, 2, 1, 1.5f)), true);
    }

    public void BroadcastNewDay()
    {
        newDay = true;
        var message = Language.GetTextValue($"Mods.MajorasMaskTribute.Announcements.NewDayMessage");
        SetText(message, 1.5f, true);
        time = 300;
    }

    public override void Update(GameTime gameTime)
    {
        hourText.TextColor = Color.White;
        if (newDay)
        {
            TextColor = Color.Black;
        }
        else
        {
            TextColor = Color.White;
        }
        ShadowColor = defaultShadow;
        Dawn.ShadowColor = defaultShadow;
        if (time > 0)
        {
            time--;
            blackScreen.display = (Main.dayTime && !BlackScreen.overridePauseForEffect) || newDay;
            if (blackScreen.display && BlackScreen.PauseGameDuringDayTransitions)
            {
                hourText.TextColor *= Utils.Remap(time, 100, 200, 1, 0);
                if (ApocalypseSystem.apocalypseDay >= 2)
                {
                    ShadowColor = Color.White;
                    Dawn.ShadowColor = Color.White;
                }
            }
            else
            {
                TextColor *= 0.4f;
                hourText.TextColor *= 0.4f;
            }
        }
        else
        {
            SetText("");
            hourText.SetText("");
            Dawn.SetText("");
            blackScreen.display = false;
            BlackScreen.overridePauseForEffect = false;
            newDay = false;
        }
        BlackScreen.newDay = newDay;
    }

    public class HourText : UIText
    {
        public const float defaultVAlign = 0.45f;

        public HourText(float textScale) : base("", textScale)
        {
            HAlign = 0.5f;
            VAlign = defaultVAlign;
        }
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

public class BlackScreen : UIElement
{
    public bool display = false;

    public static bool newDay = false;

    public static bool pausedForEffect { get; private set; } = false;

    public static bool overridePauseForEffect = false;

    private static readonly List<short> nocturnalBosses = new() { NPCID.EyeofCthulhu, NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail, NPCID.SkeletronPrime, NPCID.SkeletronHead };

    public static bool PauseGameDuringDayTransitions
    {
        get
        {
            if (newDay)
            {
                return true;
            }
            if (overridePauseForEffect)
            {
                return false;
            }
            if (Main.invasionType != 0)
            {
                return false;
            }
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.boss && !nocturnalBosses.Contains((short)npc.type))
                {
                    return false;
                }
            }
            return ModContent.GetInstance<GameplayConfig>().PauseGameDuringDayTransitions;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (display && PauseGameDuringDayTransitions)
        {
            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), newDay ? Color.White : Color.Black);
            pausedForEffect = true;
        }
        else
        {
            pausedForEffect = false;
        }
    }
}

public class PauseGame : ModSystem
{
    public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
    {
        if (!BlackScreen.pausedForEffect)
        {
            return;
        }
        timeRate = 0;
        tileUpdateRate = 0;
        eventUpdateRate = 0;
    }

    private bool wasPaused = false;

    Dictionary<int, Vector2> playerVelocities = new();
    Dictionary<int, Vector2> npcVelocities = new();
    Dictionary<int, Vector2> projectileVelocities = new();
    Dictionary<int, Vector2> itemVelocities = new();
    Dictionary<int, Vector2> goreVelocities = new();

    public override void PostUpdatePlayers()
    {
        if (!BlackScreen.pausedForEffect)
        {
            if (wasPaused)
            {
                //Store velocities like this so that players don't get massive momentum shifts when the day starts
                foreach (Player player in Main.ActivePlayers)
                {
                    if (!playerVelocities.ContainsKey(player.whoAmI))
                        continue;
                    player.velocity = playerVelocities[player.whoAmI];
                    player.frozen = false;
                }
            }
            return;
        }
        if (!wasPaused)
        {
            playerVelocities.Clear();
            foreach (Player player in Main.ActivePlayers)
            {
                playerVelocities.Add(player.whoAmI, player.velocity);
            }
        }
        foreach (Player player in Main.ActivePlayers)
        {
            player.position = player.oldPosition;
            player.frozen = true;
            for (int i = 0; i < player.buffTime.Length; i++)
            {
                if (player.buffTime[i] > 0)
                {
                    player.buffTime[i]++;
                }
            }
        }
    }

    public override void PostUpdateNPCs()
    {
        if (!BlackScreen.pausedForEffect)
        {
            if (wasPaused)
            {
                //Store velocities like this so that npcs don't get massive momentum shifts when the day starts
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npcVelocities.ContainsKey(npc.whoAmI))
                        continue;
                    npc.velocity = npcVelocities[npc.whoAmI];
                }
            }
            return;
        }
        if (!wasPaused)
        {
            npcVelocities.Clear();
            foreach (NPC npc in Main.ActiveNPCs)
            {
                npcVelocities.Add(npc.whoAmI, npc.velocity);
            }
        }
        foreach (NPC npc in Main.ActiveNPCs)
        {
            npc.position = npc.oldPosition;
            for (int i = 0; i < npc.buffTime.Length; i++)
            {
                if (npc.buffTime[i] > 0)
                {
                    npc.buffTime[i]++;
                }
            }
        }
    }

    public override void PostUpdateProjectiles()
    {
        if (!BlackScreen.pausedForEffect)
        {
            if (wasPaused)
            {
                //Store velocities like this so that projectiles don't get massive momentum shifts when the day starts
                foreach (Projectile projectile in Main.ActiveProjectiles)
                {
                    if (!projectileVelocities.ContainsKey(projectile.whoAmI))
                        continue;
                    projectile.velocity = projectileVelocities[projectile.whoAmI];
                }
            }
            return;
        }
        if (!wasPaused)
        {
            projectileVelocities.Clear();
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                projectileVelocities.Add(projectile.whoAmI, projectile.velocity);
            }
        }
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            projectile.position = projectile.oldPosition;
        }
    }

    public override void PostUpdateItems()
    {
        if (!BlackScreen.pausedForEffect)
        {
            if (wasPaused)
            {
                //Store velocities like this so that items don't get massive momentum shifts when the day starts
                foreach (Item item in Main.ActiveItems)
                {
                    if (!itemVelocities.ContainsKey(item.whoAmI))
                        continue;
                    item.velocity = itemVelocities[item.whoAmI];
                }
            }
            return;
        }
        if (!wasPaused)
        {
            itemVelocities.Clear();
            foreach (Item item in Main.ActiveItems)
            {
                itemVelocities.Add(item.whoAmI, item.velocity);
            }
        }
        foreach (Item item in Main.ActiveItems)
        {
            item.velocity = Vector2.Zero;
        }
    }

    public override void PostUpdateEverything()
    {
        wasPaused = BlackScreen.pausedForEffect;
    }
}

public class WhiteScreen : UIElement
{
    public int ocarinaTimer;

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * Utils.Remap(ocarinaTimer, 660, 470, 1, 0));
    }
}

public class DayDisplay : UIState
{
    public DayOfText dayOfText;
    public ApocalypseText apocalypseText;
    public WhiteScreen whiteScreen;
    public BlackScreen blackScreen;

    public override void OnInitialize()
    {
        dayOfText = new();
        blackScreen = new();
        apocalypseText = new();
        whiteScreen = new();

        dayOfText.blackScreen = blackScreen;
        ApocalypseSystem.dayOfText = dayOfText;
        Content.Items.OcarinaOfTime.whiteScreen = whiteScreen;

        Append(blackScreen);
        Append(dayOfText);
        Append(dayOfText.Dawn);
        Append(dayOfText.hourText);
        Append(apocalypseText);
        Append(whiteScreen);
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


