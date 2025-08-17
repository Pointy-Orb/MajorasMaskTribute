using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using System.Collections.Generic;
using Terraria.GameContent.Drawing;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ObjectData;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.ObjectInteractions;

namespace MajorasMaskTribute.Content.Tiles;

public class FinalHoursMusicBox : ModTile
{
    private Asset<Texture2D> glowTexture;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
        TileObjectData.addTile(Type);

        DustType = DustID.Stone;

        AddMapEntry(
            Color.Gray, Language.GetText("ItemName.MusicBox")
        );

        glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    /*
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
        int leftX = i - (Main.tile[i, j].TileFrameX - (IsMusicBoxActive(i, j) ? 36 : 0)) / 18;
        int topY = j - Main.tile[i, j].TileFrameY / 18;
        short frameAdjust = (short)(IsMusicBoxActive(i, j) ? -36 : 36);
        for (int k = 0; k < 2; k++)
        {
            for (int l = 0; l < 2; l++)
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
	*/

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.cursorItemIconEnabled = true;

        player.cursorItemIconID = ModContent.ItemType<Items.FinalHoursMusicBox>();
        player.noThrow = 2;
    }

    /*
    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        yield return new Item(ModContent.ItemType<Content.Items.FinalHoursMusicBox>());
    }

    public static bool IsMusicBoxActive(int i, int j)
    {
        return Main.tile[i, j].TileFrameX >= 36;
    }
	*/

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

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        yield return new Item(ModContent.ItemType<Items.FinalHoursMusicBox>());
    }

    public override void EmitParticles(int i, int j, Tile tileCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
    {
        Tile tile = Main.tile[i, j];

        if (!visible || tile.TileFrameX != 36 || tile.TileFrameY % 36 != 0 || (int)Main.timeForVisualEffects % 7 != 0 || !Main.rand.NextBool(3))
        {
            return;
        }

        int MusicNote = Main.rand.Next(570, 573);
        Vector2 SpawnPosition = new Vector2(i * 16 + 8, j * 16 - 8);
        Vector2 NoteMovement = new Vector2(Main.WindForVisuals * 2f, -0.5f);
        NoteMovement.X *= Main.rand.NextFloat(0.5f, 1.5f);
        NoteMovement.Y *= Main.rand.NextFloat(0.5f, 1.5f);
        switch (MusicNote)
        {
            case 572:
                SpawnPosition.X -= 8f;
                break;
            case 571:
                SpawnPosition.X -= 4f;
                break;
        }

        Gore.NewGore(new EntitySource_TileUpdate(i, j), SpawnPosition, NoteMovement, MusicNote, 0.8f);
    }
}
