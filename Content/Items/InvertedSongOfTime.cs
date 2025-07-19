using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Items;

public class InvertedSongOfTime : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemNoGravity[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 22;
        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<InvertedSongOfTimePlayer>().invertedSongEquipped = true;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type];
        Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);
        spriteBatch.Draw(texture.Value, drawPosition, itemFrame, Color.White * 0.7f, rotation, drawOrigin, scale, SpriteEffects.None, 0);
        return false;
    }
}

public class InvertedSongOfTimePlayer : ModPlayer
{
    public bool invertedSongEquipped = false;

    public override void ResetEffects()
    {
        invertedSongEquipped = false;
    }
}
