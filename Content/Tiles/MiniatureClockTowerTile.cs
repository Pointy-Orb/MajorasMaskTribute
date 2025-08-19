using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.GameContent;
using Terraria.DataStructures;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.ObjectData;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Tiles;

public class MiniatureClockTowerTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<MiniatureClockTowerTileEntity>().Generic_HookPostPlaceMyPlayer;
        TileObjectData.addTile(Type);

        AddMapEntry(Color.Brown, Language.GetText("Mods.MajorasMaskTribute.Items.MiniatureClockTower.DisplayName"));
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override bool RightClick(int i, int j)
    {
        SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
        Toggle(i, j);
        return true;
    }

    public override void HitWire(int i, int j)
    {
        Toggle(i, j);
    }

    private void Toggle(int i, int j)
    {
        int leftX = i - (Main.tile[i, j].TileFrameX - (IsTowerActive(i, j) ? 36 : 0)) / 18;
        int topY = j - Main.tile[i, j].TileFrameY / 18;
        short frameAdjust = (short)(IsTowerActive(i, j) ? -36 : 36);
        for (int k = 0; k < 2; k++)
        {
            for (int l = 0; l < 3; l++)
            {
                Tile tile = Main.tile[leftX + k, topY + l];
                tile.TileFrameX += frameAdjust;
                if (Wiring.running)
                {
                    Wiring.SkipWire(leftX + k, topY + l);
                }
            }
        }
        if (IsTowerActive(i, j))
        {
            SoundEngine.PlaySound(new SoundStyle("MajorasMaskTribute/Assets/bell"), new Vector2(i, j) * 16);
            Vector2 SpawnPosition = new Vector2((float)leftX + 0.1f, (float)topY - 0.1f) * 16;
            Vector2 WaveMovement = new Vector2(0, -10f);
            if (TileDrawing.IsVisible(Main.tile[i, j]))
            {
                var wave = Gore.NewGorePerfect(new EntitySource_TileUpdate(leftX, topY), SpawnPosition, WaveMovement, ModContent.GoreType<ClockTowerWave>(), 1f);
            }
        }
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            NetMessage.SendTileSquare(-1, leftX, topY, 2, 4, TileChangeType.None);
        }
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.cursorItemIconEnabled = true;
        player.noThrow = 2;

        player.cursorItemIconID = ModContent.ItemType<Items.MiniatureClockTower>();
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        yield return new Item(ModContent.ItemType<Items.MiniatureClockTower>());
    }

    public static bool IsTowerActive(int i, int j)
    {
        return Main.tile[i, j].TileFrameX >= 36;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (!closer)
        {
            return;
        }
        if (IsTowerActive(i, j))
        {
            TowerTileSystem.nearClockTower = true;
        }
    }
}

public class MiniatureClockTowerTileEntity : ModTileEntity
{
    private int bellsPlayed = 0;

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<MiniatureClockTowerTile>();
    }

    public override void Update()
    {
        var tile = Main.tile[Position.X, Position.Y];
        if (tile.TileFrameX < 36)
        {
            return;
        }
        Items.MiniatureClockTower.TryPlayBells(ref bellsPlayed, Position.ToVector2() * 16, 2f, () =>
        {
            if (!TileDrawing.IsVisible(Main.tile[Position.X, Position.Y]))
            {
                return;
            }
            Vector2 SpawnPosition = new Vector2(Position.X + 0.1f, Position.Y - 0.1f) * 16;
            Vector2 WaveMovement = new Vector2(0, -10f);
            var wave = Gore.NewGorePerfect(new EntitySource_TileUpdate(Position.X, Position.Y), SpawnPosition, WaveMovement, ModContent.GoreType<ClockTowerWave>(), 1f);
        }, true);
    }
}

public class ClockTowerWave : ModGore
{
    public override void SetStaticDefaults()
    {
        ChildSafety.SafeGore[Type] = true;
    }

    public override void OnSpawn(Gore gore, IEntitySource source)
    {
        gore.timeLeft = 60;
    }

    public override bool Update(Gore gore)
    {
        gore.scale += 0.05f;
        gore.position += gore.velocity;
        if (gore.timeLeft <= 0)
        {
            gore.active = false;
        }
        return false;
    }
}

public class TowerTileSystem : ModSystem
{
    public static bool nearClockTower = false;

    public override void ResetNearbyTileEffects()
    {
        nearClockTower = false;
    }
}
