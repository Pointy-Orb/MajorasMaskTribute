using Terraria;
using Terraria.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.GameContent.ObjectInteractions;

namespace MajorasMaskTribute.Content.Tiles;

public class DoomMonolith : ModTile
{
    private Asset<Texture2D> glowTexture;

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
        TileObjectData.addTile(Type);

        DustType = DustID.Iron;

        AddMapEntry(
            Color.DimGray,
            Language.GetOrRegister("Mods.MajorasMaskTribute.Tiles.DoomMonolith.MapEntry")
        );

        glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        if (Main.tile[i, j].TileFrameX < 36)
        {
            return;
        }
        tileFrameX = (short)((Main.tileFrame[Type] * 36) + 36);
        if (Main.tile[i, j].TileFrameX > 48)
        {
            tileFrameX += 18;
        }
    }

    public override void AnimateTile(ref int frame, ref int frameCounter)
    {
        frameCounter++;
        if (frameCounter >= 15)
        {
            frameCounter = 0;
        }
        frame = frameCounter / 5;
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
        int leftX = i - (Main.tile[i, j].TileFrameX - (IsMonolithActive(i, j) ? 36 : 0)) / 18;
        int topY = j - Main.tile[i, j].TileFrameY / 18;
        short frameAdjust = (short)(IsMonolithActive(i, j) ? -36 : 36);
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
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            NetMessage.SendTileSquare(-1, leftX, topY, 2, 4, TileChangeType.None);
        }
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.cursorItemIconEnabled = true;

        player.cursorItemIconID = ModContent.ItemType<Items.DoomMonolith>();
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        yield return new Item(ModContent.ItemType<Content.Items.DoomMonolith>());
    }

    public static bool IsMonolithActive(int i, int j)
    {
        return Main.tile[i, j].TileFrameX >= 36;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        if (!TileDrawing.IsVisible(tile))
        {
            return;
        }
        Color paintColor = WorldGen.paintColor(tile.TileColor);

        int offset = 2;

        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

        var rect = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

        if (tile.TileFrameX >= 36)
        {
            rect.X = (Main.tileFrame[Type] * 36) + (tile.TileFrameX % 36 == 0 ? 0 : 18) + 36;
        }

        spriteBatch.Draw(
            glowTexture.Value,
            new Vector2(
                i * 16 - (int)Main.screenPosition.X,
                j * 16 - (int)Main.screenPosition.Y + offset
            ) + zero,
            rect,
            paintColor,
            0f,
            Vector2.Zero,
            1f,
            SpriteEffects.None,
            0f
        );
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (!closer)
        {
            return;
        }
        if (IsMonolithActive(i, j))
        {
            DoomMonolithSystem.nearDoomMonolith = true;
        }
    }
}

public class DoomMonolithPlayer : ModPlayer
{
    public bool doomMonolithActive = false;

    public override void ResetEffects()
    {
        doomMonolithActive = false;
    }
}

public class DoomMonolithSystem : ModSystem
{
    public static bool nearDoomMonolith = false;

    public override void ResetNearbyTileEffects()
    {
        nearDoomMonolith = false;
    }
}
