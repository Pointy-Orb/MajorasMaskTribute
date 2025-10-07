using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using MajorasMaskTribute.Content.Items;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ID;

namespace MajorasMaskTribute.Common;

public class WandOfSparkingModePlayer : ModPlayer
{
    private bool Brutal
    {
        get
        {
            return ModContent.GetInstance<ServerConfig>().WandOfSparkingMode == WandOfSparkingMode.Brutal;
        }
    }

    public static readonly List<short> masks = new()
    {
        ItemID.KingSlimeMask,
        ItemID.EyeMask,
        ItemID.EaterMask,
        ItemID.BrainMask,
        ItemID.SkeletronMask,
        ItemID.BeeMask,
        ItemID.DeerclopsMask,
        ItemID.FleshMask,
        ItemID.QueenSlimeMask,
        ItemID.TwinMask,
        ItemID.DestroyerMask,
        ItemID.SkeletronPrimeMask,
        ItemID.PlanteraMask,
        ItemID.GolemMask,
        ItemID.DukeFishronMask,
        ItemID.FairyQueenMask,
        ItemID.BossMaskCultist,
        ItemID.BossMaskMoonlord
    };

    public void ResetInventory()
    {
        ResetInventoryInner(Player.inventory);
        ResetInventoryInner(Player.bank2.item, true);
        ResetInventoryInner(Player.bank3.item, true);
        ResetInventoryInner(Player.bank4.item, true);
        if (Brutal)
        {
            ResetInventoryInner(Player.bank.item, true);
        }
        else
        {
            ResetBank();
        }
        ResetInventoryInner(Player.armor, true, true);
        ResetInventoryInner(Player.miscEquips, true);
        Player.trashItem.TurnToAir();
        //Accomodate for modded accessory slots
        deleteAccessoriesNextTick = true;
        ConvertSpirits();
    }

    public bool deleteAccessoriesNextTick = false;
    public override void PostUpdateEquips()
    {
        deleteAccessoriesNextTick = false;
    }

    private void ConvertSpirits()
    {
        for (int i = 0; i < 3; i++)
        {
            if (Player.inventory[i].ModItem is ItemSpirit spirit)
            {
                var newItem = new Item();
                newItem.SetDefaults(spirit.targetItem);
                Player.inventory[i] = newItem;
            }
        }
    }

    private void ResetInventoryInner(Item[] inventory, bool fullReset = false, bool keepVanity = false)
    {
        int resetNumber = fullReset ? 0 : 3;
        if (!inventory.IndexInRange(resetNumber))
            return;
        for (int i = resetNumber; i < inventory.Length; i++)
        {
            if (!Brutal)
            {
                if (inventory[i].pick > 0) continue;
                if (inventory[i].axe > 0) continue;
                if (inventory[i].hammer > 0) continue;
                if (masks.Contains((short)inventory[i].type)) continue;
            }
            if (inventory[i].type == ModContent.ItemType<OcarinaOfTime>()) continue;
            if (inventory[i].type == ItemID.GoldWatch) continue;
            if (inventory[i].type == ModContent.ItemType<TerminianWatch>()) continue;
            if (inventory[i].type == ModContent.ItemType<TerminianWatch3D>()) continue;
            if (keepVanity && inventory[i].vanity) continue;
            inventory[i].headSlot = 0;
            inventory[i].bodySlot = 0;
            inventory[i].legSlot = 0;
            inventory[i].TurnToAir();
        }
    }

    private void ResetBank()
    {
        for (int i = 0; i < Player.bank.item.Length; i++)
        {
            if (Player.bank.item[i].IsACoin) continue;
            Player.bank.item[i].TurnToAir();
        }
    }

    public void RegisterBossDeathsByMask()
    {
        foreach (Item item in Player.inventory)
        {
            RegisterBossDeathsByMaskInner(item.type);
        }
        foreach (Item item in Player.bank2.item)
        {
            RegisterBossDeathsByMaskInner(item.type);
        }
        foreach (Item item in Player.bank3.item)
        {
            RegisterBossDeathsByMaskInner(item.type);
        }
        foreach (Item item in Player.bank4.item)
        {
            RegisterBossDeathsByMaskInner(item.type);
        }
        CheckIfBulbsSpawn();
    }

    private void RegisterBossDeathsByMaskInner(int type)
    {
        switch (type)
        {
            case ItemID.KingSlimeMask:
                NPC.downedSlimeKing = true;
                break;
            case ItemID.EyeMask:
                NPC.downedBoss1 = true;
                break;
            case ItemID.EaterMask:
            case ItemID.BrainMask:
                NPC.downedBoss2 = true;
                break;
            case ItemID.SkeletronMask:
                NPC.downedBoss3 = true;
                break;
            case ItemID.BeeMask:
                NPC.downedQueenBee = true;
                break;
            case ItemID.DeerclopsMask:
                NPC.downedDeerclops = true;
                break;
            case ItemID.FleshMask:
                if (!Main.hardMode && !WorldGen.IsGeneratingHardMode)
                {
                    WorldGen.StartHardmode();
                }
                break;
            case ItemID.QueenSlimeMask:
                NPC.downedQueenSlime = true;
                break;
            case ItemID.DestroyerMask:
                NPC.downedMechBoss1 = true;
                NPC.downedMechBossAny = true;
                break;
            case ItemID.TwinMask:
                NPC.downedMechBoss2 = true;
                NPC.downedMechBossAny = true;
                break;
            case ItemID.SkeletronPrimeMask:
                NPC.downedMechBoss3 = true;
                NPC.downedMechBossAny = true;
                break;
            case ItemID.PlanteraMask:
                NPC.downedPlantBoss = true;
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(Language.GetTextValue("LegacyMisc.33"), 50, byte.MaxValue, 130);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.33"), new Color(50, 255, 130));
                }
                break;
            case ItemID.GolemMask:
                NPC.downedGolemBoss = true;
                break;
            case ItemID.DukeFishronMask:
                NPC.downedFishron = true;
                break;
            case ItemID.FairyQueenMask:
                NPC.downedEmpressOfLight = true;
                break;
            case ItemID.BossMaskCultist:
                NPC.downedAncientCultist = true;
                break;
            case ItemID.BossMaskMoonlord:
                NPC.downedMoonlord = true;
                break;
        }
    }

    private void CheckIfBulbsSpawn()
    {
        if (!Main.hardMode)
            return;
        if (!NPC.downedMechBoss1 || !NPC.downedMechBoss2 || !NPC.downedMechBoss3)
            return;
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            Main.NewText(Language.GetTextValue("LegacyMisc.32"), 50, byte.MaxValue, 130);
        }
        else if (Main.netMode == NetmodeID.Server)
        {
            ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.32"), new Color(50, 255, 130));
        }
    }

    private bool HasItemAnywhere(int id)
    {
        if (Player.HasItemInInventoryOrOpenVoidBag(id))
        {
            return true;
        }
        if (Player.HasItem(id, Player.bank.item))
        {
            return true;
        }
        if (Player.HasItem(id, Player.bank2.item))
        {
            return true;
        }
        if (Player.HasItem(id, Player.bank3.item))
        {
            return true;
        }
        if (Player.HasItem(id, Player.bank4.item))
        {
            return true;
        }
        return false;
    }
}

public class PurgeAccessoriesAddMaskTooltip : GlobalItem
{
    public override void UpdateVanity(Item item, Player player)
    {
        if (player.GetModPlayer<WandOfSparkingModePlayer>().deleteAccessoriesNextTick && !item.vanity && item.type != ItemID.GoldWatch)
        {
            item.TurnToAir();
        }
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        if (player.GetModPlayer<WandOfSparkingModePlayer>().deleteAccessoriesNextTick && !item.vanity && item.type != ItemID.GoldWatch)
        {
            item.TurnToAir();
        }
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (ModContent.GetInstance<ServerConfig>().WandOfSparkingMode != WandOfSparkingMode.On)
        {
            return;
        }
        if (!WandOfSparkingModePlayer.masks.Contains((short)item.type))
            return;
        var bossName = item.Name.Replace(Language.GetTextValue("Mods.MajorasMaskTribute.Items.ItemSpirit.Mask"), "");
        if (item.type == ItemID.TwinMask)
        {
            bossName = Language.GetTextValue("Mods.MajorasMaskTribute.TwinsBossName");
        }
        bool useThe = false;
        switch (item.type)
        {
            case ItemID.EyeMask:
            case ItemID.EaterMask:
            case ItemID.BrainMask:
            case ItemID.TwinMask:
            case ItemID.FleshMask:
            case ItemID.DestroyerMask:
            case ItemID.BossMaskMoonlord:
                useThe = true;
                break;
        }
        string key = $"Mods.MajorasMaskTribute.Items.{(useThe ? "The" : "")}WoSMaskTooltip";
        int index = tooltips.FindIndex(0, tooltips.Count, i => Language.GetTextValue("ItemTooltip." + ItemID.Search.GetName(item.type)).Contains(i.Text)) + 1;
        if (!tooltips.IndexInRange(index))
        {
            index = 0;
        }
        tooltips.Insert(index, new TooltipLine(Mod, "WoSMask", Language.GetTextValue(key, bossName)));
    }
}
