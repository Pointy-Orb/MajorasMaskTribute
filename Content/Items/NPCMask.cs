using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.GameContent.ItemDropRules;
using System.Collections.Generic;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;
using System.IO;

namespace MajorasMaskTribute.Content.Items;

public abstract class NPCMask : ModItem
{
    public abstract int targetNPC { get; }

    public const string malePronounKey = "Mods.MajorasMaskTribute.Items.NPCMask.MalePronoun";
    public const string femalePronounKey = "Mods.MajorasMaskTribute.Items.NPCMask.FemalePronoun";

    public abstract bool male { get; }

    private string npcName;

    public sealed override LocalizedText DisplayName => Language.GetText("Mods.MajorasMaskTribute.Items.NPCMask.DisplayName").WithFormatArgs(npcName);

    public sealed override LocalizedText Tooltip => Language.GetText("Mods.MajorasMaskTribute.Items.NPCMask.Tooltip").WithFormatArgs(npcName, Language.GetTextValue(male ? malePronounKey : femalePronounKey));

    public override void Load()
    {
        NPCMaskDrops.maskNPCs.Add(targetNPC, this);
        var npc = new NPC();
        npc.SetDefaults(targetNPC);
        npcName = npc.TypeName;
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Orange;
        Item.width = 28;
        Item.height = 28;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        if (HomunculusNPC.maskCooldown > 0) return;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.boss) continue;
            if (npc.type == targetNPC) continue;
            if (npc.isLikeATownNPC) continue;
            if (NPCID.Sets.ProjectileNPC[npc.type]) continue; ;
            if (NPCID.Sets.BelongsToInvasionOldOnesArmy[npc.type]) continue; ;
            if (NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) continue; ;
            if (npc.GetGlobalNPC<HomunculusNPC>().isHomunculus) continue;
            if (Item.Hitbox.Intersects(npc.Hitbox))
            {
                string typeName = npc.TypeName;
                HomunculusNPC.TransformByMask(npc, targetNPC);
                npc.GivenName = typeName;
                Item.active = false;
                ParticleOrchestrator.BroadcastParticleSpawn(ParticleOrchestraType.TownSlimeTransform, new ParticleOrchestraSettings
                {
                    PositionInWorld = npc.Center,
                    MovementVector = Vector2.Zero,
                    UniqueInfoPiece = 1
                });
                if (npc.townNPC)
                {
                    if (Main.dedServ)
                    {
                        ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasArrived", npc.FullName), new Color(50, 125, byte.MaxValue));
                    }
                    else
                    {
                        Main.NewText(Language.GetTextValue("Announcement.HasArrived", npc.FullName), 50, 125, byte.MaxValue);
                    }
                }
                OnTransform();
                break;
            }
        }
    }

    protected virtual void OnTransform()
    { }
}

public class NPCMaskDrops : GlobalNPC
{
    public static Dictionary<int, NPCMask> maskNPCs = new();

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        foreach (int type in maskNPCs.Keys)
        {
            if (npc.type != type) continue;
            npcLoot.Add(ItemDropRule.ByCondition(new AfterPartyOfDoomCondition(), maskNPCs[type].Type));
        }
    }
}

public class AfterPartyOfDoomCondition : IItemDropRuleCondition
{
    private static LocalizedText Description;

    public AfterPartyOfDoomCondition()
    {
        Description ??= Language.GetOrRegister("Mods.MajorasMaskTribute.AfterPartyOfDoomCondition");
    }

    public bool CanDrop(DropAttemptInfo info)
    {
        return !Main.afterPartyOfDoom;
    }

    public bool CanShowItemDropInUI()
    {
        return true;
    }

    public string GetConditionDescription()
    {
        return Description.Value;
    }
}

public class GuideMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Guide;
}

public class MerchantMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Merchant;
    protected override void OnTransform()
    {
        NPC.unlockedMerchantSpawn = true;
    }
}

public class NurseMask : NPCMask
{
    public override bool male => false;
    public override int targetNPC => NPCID.Nurse;
    protected override void OnTransform()
    {
        NPC.unlockedNurseSpawn = true;
    }
}

public class DemolitionistMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Demolitionist;
    protected override void OnTransform()
    {
        NPC.unlockedDemolitionistSpawn = true;
    }
}

public class DyeTraderMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.DyeTrader;
    protected override void OnTransform()
    {
        NPC.unlockedDyeTraderSpawn = true;
    }
}

public class AnglerMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Angler;
    protected override void OnTransform()
    {
        NPC.savedAngler = true;
    }
}

public class ZoologistMask : NPCMask
{
    public override int targetNPC => NPCID.BestiaryGirl;
    public override bool male => false;
}

public class DryadMask : NPCMask
{
    public override int targetNPC => NPCID.Dryad;
    public override bool male => false;
    protected override void OnTransform()
    {
        NPC.downedBoss1 = true;
    }
}

public class PainterMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Painter;
}

public class GolferMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Golfer;
    protected override void OnTransform()
    {
        NPC.savedGolfer = true;
    }
}

public class ArmsDealerMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.ArmsDealer;
    protected override void OnTransform()
    {
        NPC.unlockedArmsDealerSpawn = true;
    }
}

public class TavernkeepMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.DD2Bartender;
    protected override void OnTransform()
    {
        NPC.savedBartender = true;
    }
}

public class StylistMask : NPCMask
{
    public override int targetNPC => NPCID.Stylist;
    public override bool male => false;
    protected override void OnTransform()
    {
        NPC.savedStylist = true;
    }
}

public class GoblinTinkererMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.GoblinTinkerer;
    protected override void OnTransform()
    {
        NPC.savedGoblin = true;
    }
}

public class WitchDoctorMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.WitchDoctor;
    protected override void OnTransform()
    {
        NPC.downedQueenBee = true;
    }
}

public class ClothierMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Clothier;
}

public class MechanicMask : NPCMask
{
    public override int targetNPC => NPCID.Mechanic;
    public override bool male => false;
    protected override void OnTransform()
    {
        NPC.savedMech = true;
    }
}

public class PartyGirlMask : NPCMask
{
    public override int targetNPC => NPCID.PartyGirl;
    public override bool male => false;
    protected override void OnTransform()
    {
        NPC.unlockedPartyGirlSpawn = true;
    }
}

public class WizardMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Wizard;
    protected override void OnTransform()
    {
        NPC.savedWizard = true;
    }
}

public class TaxCollectorMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.TaxCollector;
    protected override void OnTransform()
    {
        NPC.savedTaxCollector = true;
    }
}

public class TruffleMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Truffle;
    protected override void OnTransform()
    {
        NPC.unlockedTruffleSpawn = true;
    }
}

public class PirateMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Pirate;
    protected override void OnTransform()
    {
        NPC.downedPirates = true;
    }
}

public class SteampunkerMask : NPCMask
{
    public override int targetNPC => NPCID.Steampunker;
    public override bool male => false;
}

public class CyborgMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.Cyborg;
}

public class SantaMask : NPCMask
{
    public override bool male => true;
    public override int targetNPC => NPCID.SantaClaus;
    protected override void OnTransform()
    {
        NPC.downedFrost = true;
    }
}

public class PrincessMask : NPCMask
{
    public override int targetNPC => NPCID.Princess;
    public override bool male => false;
    protected override void OnTransform()
    {
        NPC.unlockedPrincessSpawn = true;
    }
}

public class HomunculusNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public bool isHomunculus { get; set; } = false;
    public int originalType { get; private set; }
    public static int maskCooldown = 0;

    public static void TransformByMask(NPC target, int maskNPCType)
    {
        var originalType = target.type;
        target.Transform(maskNPCType);
        var homunculusNPC = target.GetGlobalNPC<HomunculusNPC>();
        homunculusNPC.isHomunculus = true;
        homunculusNPC.originalType = originalType;
        target.netUpdate = true;
    }

    public override void PostAI(NPC npc)
    {
        if (maskCooldown > 0)
            maskCooldown--;
    }

    public override bool CheckDead(NPC npc)
    {
        var homunculusNPC = npc.GetGlobalNPC<HomunculusNPC>();
        if (homunculusNPC.isHomunculus)
        {
            maskCooldown = 2000;
            Item.NewItem(npc.GetSource_DropAsItem(), npc.Top, Vector2.One, NPCMaskDrops.maskNPCs[npc.type].Type);
            string oldForm = npc.TypeName;
            string newForm = npc.GivenName;
            npc.Transform(homunculusNPC.originalType);
            npc.velocity = Vector2.Zero;
            homunculusNPC.isHomunculus = false;
            npc.netUpdate = true;
            if (Main.dedServ)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Mods.MajorasMaskTribute.DroppedMask", npc.TypeName, oldForm), new Color(byte.MaxValue, 25, 25));
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(Language.GetTextValue("Mods.MajorasMaskTribute.DroppedMask", newForm, oldForm), byte.MaxValue, 25, 25);
            }
            return false;
        }
        return true;
    }

    public override void SaveData(NPC npc, TagCompound tag)
    {
        var homunculusNPC = npc.GetGlobalNPC<HomunculusNPC>();
        tag["isHomunculus"] = homunculusNPC.isHomunculus;
        if (homunculusNPC.isHomunculus)
        {
            tag["originalType"] = homunculusNPC.originalType;
        }
    }

    public override void LoadData(NPC npc, TagCompound tag)
    {
        var homunculusNPC = npc.GetGlobalNPC<HomunculusNPC>();
        homunculusNPC.isHomunculus = tag.GetBool("isHomunculus");
        homunculusNPC.originalType = tag.GetInt("originalType");
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        var homunculusNPC = npc.GetGlobalNPC<HomunculusNPC>();
        bitWriter.WriteBit(homunculusNPC.isHomunculus);
        binaryWriter.Write((short)homunculusNPC.originalType);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        var homunculusNPC = npc.GetGlobalNPC<HomunculusNPC>();
        homunculusNPC.isHomunculus = bitReader.ReadBit();
        homunculusNPC.originalType = binaryReader.ReadInt16();
    }
}
