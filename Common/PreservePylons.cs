using Terraria;
using Terraria.Localization;
using Terraria.GameContent;
using Terraria.GameContent.Tile_Entities;
using Terraria.DataStructures;
using Terraria.ObjectData;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ID;

namespace MajorasMaskTribute.Common;

public class PreservePylons : ModSystem
{
    public override void Load()
    {
        On_TeleportPylonsSystem.DoesPylonHaveEnoughNPCsAroundIt += NoNPCsforPylons;
    }

    private static bool NoNPCsforPylons(On_TeleportPylonsSystem.orig_DoesPylonHaveEnoughNPCsAroundIt orig, TeleportPylonsSystem self, TeleportPylonInfo info, int NPCCount)
    {
        return true;
    }

    public static List<Pylon> GetPylons()
    {
        List<Pylon> pylons = new();
        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY; j++)
            {
                Tile tile = Main.tile[i, j];
                if (!TileID.Sets.CountsAsPylon.Contains(tile.TileType))
                {
                    continue;
                }
                var pylonStyle = TileObjectData.GetTileStyle(tile);
                var pylonData = TileObjectData.GetTileData(tile.TileType, pylonStyle);
                int textureWidth = pylonData.CoordinateWidth * pylonData.Width + pylonData.CoordinatePadding * pylonData.Width;
                int relativeFrameX = tile.TileFrameX - textureWidth * (pylonData.StyleHorizontal ? pylonStyle : 0);
                int relativeFrameY = tile.TileFrameY - (pylonData.CoordinateHeights.Sum() + pylonData.CoordinatePadding * pylonData.Height) * (pylonData.StyleHorizontal ? 0 : pylonStyle);
                int tilePosX = relativeFrameX / (pylonData.CoordinateWidth + pylonData.CoordinatePadding);
                int tilePosY = 0;
                int sumHeights = pylonData.CoordinateHeights[tilePosY];
                while (pylonData.CoordinateHeights.IndexInRange(tilePosY + 1) && relativeFrameY - sumHeights > 0)
                {
                    tilePosY++;
                    sumHeights += pylonData.CoordinateHeights[tilePosY];
                }
                if (pylonData.Origin.X != tilePosX || pylonData.Origin.Y != tilePosY)
                {
                    continue;
                }
                pylons.Add(new Pylon
                {
                    location = new Point(i, j),
                    origin = pylonData.Origin.ToPoint(),
                    style = pylonStyle,
                    width = pylonData.Width,
                    height = pylonData.Height,
                    type = tile.TileType
                });
            }
        }
        return pylons;
    }

    public static void ReplacePylons(List<Pylon> pylons)
    {
        foreach (Pylon pylon in pylons)
        {
            int x = pylon.location.X;
            int y = pylon.location.Y;
            for (int i = x - pylon.origin.X; i < x - pylon.origin.X + pylon.width; i++)
            {
                for (int j = y - pylon.origin.Y; j < y - pylon.origin.Y + pylon.height; j++)
                {
                    WorldGen.KillTile(i, j, noItem: true);
                }
            }
            TileObject.CanPlace(x, y, pylon.type, pylon.style, -1, out var pylonObject);
            WorldGen.PlaceObject(x, y, pylon.type, true, pylon.style);
            if (TileLoader.GetTile(pylon.type) == null)
            {
                new TETeleportationPylon().NetPlaceEntityAttempt(x - pylon.origin.X, y - pylon.origin.Y);
                continue;
            }
            TileObjectData.CallPostPlacementPlayerHook(x, y, pylon.type, pylon.style, -1, 0, pylonObject);
        }
    }
}

public struct Pylon
{
    public required Point location;
    public required Point origin;
    public required int style;
    public required int width;
    public required int height;
    public required int type;
}

/*
public class PylonCheck : ModCommand
{
    public override CommandType Type => CommandType.World;

    public override string Command => "pylonCheck";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        var pylons = PreservePylons.GetPylons();
        foreach (Pylon pylon in pylons)
        {
            int x = pylon.location.X;
            int y = pylon.location.Y;
            for (int i = x - pylon.origin.X; i < x - pylon.origin.X + pylon.width; i++)
            {
                for (int j = y - pylon.origin.Y; j < y - pylon.origin.Y + pylon.height; j++)
                {
                    var dust = Dust.NewDustPerfect(new Point(i, j).ToWorldCoordinates(), DustID.AncientLight, Vector2.Zero);
                    dust.noGravity = true;
                }
            }
            Main.NewText(pylon.style);
            Main.NewText(pylon.type);
        }
    }
}
*/

public class ModifyPylonTooltip : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!TileID.Sets.CountsAsPylon.Contains(item.createTile))
        {
            return;
        }
        foreach (TooltipLine line in tooltips)
        {
            if (line.Text.Contains(Language.GetTextValue("Mods.MajorasMaskTribute.PylonCutoff")))
            {
                line.Text = Language.GetTextValue("Mods.MajorasMaskTribute.ProperPylonMessage");
            }
        }
    }
}
