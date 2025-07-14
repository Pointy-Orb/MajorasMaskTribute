using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace MajorasMaskTribute.Content.Items;

public class InvertedSongOfTime : ModItem
{
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
}

public class InvertedSongOfTimePlayer : ModPlayer
{
    public bool invertedSongEquipped = false;

    public override void ResetEffects()
    {
        invertedSongEquipped = false;
    }
}
