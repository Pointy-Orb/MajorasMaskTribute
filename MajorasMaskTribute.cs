using System;
using Terraria.Chat;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.Audio;
using Terraria.ID;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.Graphics.Effects;
using MajorasMaskTribute.Common;
using MajorasMaskTribute.Content.Items;
using System.IO;

namespace MajorasMaskTribute
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class MajorasMaskTribute : Mod
    {
        internal static Mod mod;

        internal enum MessageType : byte
        {
            SyncAnimationTimer,
            DisplayDayOf,
            BlowUpClient,
            TurnOffBlowUpShader,
            ResetPlayers,
            SavePlayerBackups,
            StartEclipse,
            RemoveEclipseDisc,
            PlayBell,
            DisintegrateNPC,
            DisintegrateNPCEffects,
            GetCycleCount
        }

        public override void Load()
        {
            mod = this;
            if (!Main.dedServ)
            {
                Filters.Scene["MajorasMaskTribute:FinalNightShader"] = new Filter(new FinalNightScreenShaderData("FilterBloodMoon"), EffectPriority.High);
                Filters.Scene["MajorasMaskTribute:BigScaryFlashShader"] = new Filter(new BigScaryFlashShader("FilterBloodMoon"), EffectPriority.VeryHigh);
                SkyManager.Instance["MajorasMaskTribute:FinalNightSky"] = new FinalNightSky();
            }
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MessageType messageType = (MessageType)reader.ReadByte();
            switch (messageType)
            {
                case MessageType.SyncAnimationTimer:
                    byte playerNumber = reader.ReadByte();
                    var ocarinaPlayer = Main.player[playerNumber].GetModPlayer<OcarinaOfTimePlayer>();
                    ocarinaPlayer.RecievePlayerSync(reader);
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ocarinaPlayer.SyncPlayer(-1, whoAmI, false);
                    }
                    break;
                case MessageType.DisplayDayOf:
                    bool overridePause = reader.ReadBoolean();
                    byte day = reader.ReadByte();
                    ApocalypseSystem.dayOfText.DisplayDayOf(overridePause, day);
                    if (Main.dayTime)
                    {
                        MiniatureClockTowerPlayer.PlayRooster();
                    }
                    break;
                case MessageType.BlowUpClient:
                    string name = reader.ReadString();
                    Main.instance.CameraModifiers.Add(new ApocalypseScreenShake(80, name));
                    SoundStyle explooood = new SoundStyle("MajorasMaskTribute/Assets/impact");
                    Main.LocalPlayer.ManageSpecialBiomeVisuals("MajorasMaskTribute:BigScaryFlashShader", true);
                    SoundEngine.PlaySound(explooood);
                    break;
                case MessageType.TurnOffBlowUpShader:
                    Main.LocalPlayer.ManageSpecialBiomeVisuals("MajorasMaskTribute:BigScaryFlashShader", false);
                    break;
                case MessageType.ResetPlayers:
                    ApocalypseSystem.ResetLocalPlayer();
                    break;
                case MessageType.SavePlayerBackups:
                    Player.SavePlayer(Main.ActivePlayerFileData);
                    FileUtilities.Copy(Main.ActivePlayerFileData.Path, Main.ActivePlayerFileData.Path + ".dayone", Main.ActivePlayerFileData.IsCloudSave);
                    if (FileUtilities.Exists(Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr"), Main.ActivePlayerFileData.IsCloudSave))
                    {
                        FileUtilities.Copy(Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr"), Path.ChangeExtension(Main.ActivePlayerFileData.Path, ".tplr") + ".dayone", Main.ActivePlayerFileData.IsCloudSave);
                    }
                    break;
                case MessageType.StartEclipse:
                    Main.eclipse = true;
                    var key = Main.remixWorld ? "LegacyMisc.106" : "LegacyMisc.20";
                    Main.NewText(Language.GetTextValue(key), 50, byte.MaxValue, 130);
                    break;
                case MessageType.RemoveEclipseDisc:
                    var player = reader.ReadByte();
                    if (player != Main.myPlayer)
                    {
                        return;
                    }
                    int disc = Main.player[player].FindItem(ModContent.ItemType<EclipseDisc>(), Main.player[player].inventory);
                    if (disc < 0)
                        return;
                    var discItem = Main.player[player].inventory[disc];
                    discItem.stack--;
                    if (discItem.stack <= 0)
                    {
                        discItem.TurnToAir();
                    }
                    break;
                case MessageType.PlayBell:
                    Vector2 position = reader.ReadVector2();
                    bool waveGore = reader.ReadBoolean();
                    SoundEngine.PlaySound(new SoundStyle("MajorasMaskTribute/Assets/bell"), position);
                    if (!waveGore)
                    {
                        break;
                    }
                    Vector2 SpawnPosition = new Vector2(position.X + 0.016f, position.Y - 0.16f);
                    Vector2 WaveMovement = new Vector2(0, -10f);
                    var wave = Gore.NewGorePerfect(new EntitySource_TileUpdate((int)position.X, (int)position.Y), SpawnPosition, WaveMovement, ModContent.GoreType<Content.Tiles.ClockTowerWave>(), 1f);
                    break;
                case MessageType.DisintegrateNPC:
                    List<int> victims = new();
                    byte victim = 0;
                    var effectPacket = this.GetPacket();
                    effectPacket.Write((byte)MessageType.DisintegrateNPCEffects);
                    while ((victim = reader.ReadByte()) < byte.MaxValue)
                    {
                        victims.Add(victim);
                        effectPacket.Write(victim);
                    }
                    effectPacket.Write(byte.MaxValue);
                    effectPacket.Send();
                    foreach (int npcIndex in victims)
                    {
                        var npc = Main.npc[npcIndex];
                        if (!NPCMaskDrops.maskNPCs.ContainsKey(npc.type))
                        {
                            continue;
                        }
                        if (Main.dedServ)
                        {
                            HomunculusNPC.NPCToMaskInner(npc);
                        }
                    }
                    break;
                case MessageType.DisintegrateNPCEffects:
                    List<int> effectVictims = new();
                    byte effectVictim = 0;
                    while ((effectVictim = reader.ReadByte()) < byte.MaxValue)
                    {
                        effectVictims.Add(effectVictim);
                    }
                    foreach (int effectVictimIndex in effectVictims)
                    {
                        var npc = Main.npc[effectVictimIndex];
                        Gore.NewGorePerfect(npc.GetSource_FromThis(), npc.position, new Vector2(0.5f, 0.7f), Main.rand.Next(11, 14));
                        Gore.NewGorePerfect(npc.GetSource_FromThis(), npc.position, new Vector2(-0.5f, 0.7f), Main.rand.Next(11, 14));
                        Gore.NewGorePerfect(npc.GetSource_FromThis(), npc.position, new Vector2(0.5f, -0.7f), Main.rand.Next(11, 14));
                        Gore.NewGorePerfect(npc.GetSource_FromThis(), npc.position, new Vector2(-0.5f, -0.7f), Main.rand.Next(11, 14));
                        SoundEngine.PlaySound(SoundID.NPCDeath6, npc.Center);
                    }
                    break;
                case MessageType.GetCycleCount:
                    var message = ShowCycles.ResetXTimesMessage.WithFormatArgs(CycleCounter.cycles);
                    byte messageTarget = reader.ReadByte();
                    ChatHelper.SendChatMessageToClient(message.ToNetworkText(), Color.White, messageTarget);
                    break;
            }
        }

        public class NetData : ModSystem
        {
            public static void NetDisplayDayOf(bool overridePause, byte day)
            {
                var alertPacket = MajorasMaskTribute.mod.GetPacket();
                alertPacket.Write((byte)MajorasMaskTribute.MessageType.DisplayDayOf);
                alertPacket.Write(overridePause);
                alertPacket.Write(day);
                alertPacket.Send();
            }

            public static void BlowUpClient(string name)
            {
                var explodePacket = MajorasMaskTribute.mod.GetPacket();
                explodePacket.Write((byte)MajorasMaskTribute.MessageType.BlowUpClient);
                explodePacket.Write(name);
                explodePacket.Send();
            }

            public static void TurnOffBlowUpShader()
            {
                var shaderPacket = MajorasMaskTribute.mod.GetPacket();
                shaderPacket.Write((byte)MajorasMaskTribute.MessageType.TurnOffBlowUpShader);
                shaderPacket.Send();
            }

            public static void ResetPlayers()
            {
                var resetPacket = MajorasMaskTribute.mod.GetPacket();
                resetPacket.Write((byte)MajorasMaskTribute.MessageType.ResetPlayers);
                resetPacket.Send();
            }

            public static void SavePlayerBackups()
            {
                var backupPacket = MajorasMaskTribute.mod.GetPacket();
                backupPacket.Write((byte)MajorasMaskTribute.MessageType.SavePlayerBackups);
                backupPacket.Send();
            }

            public static void StartEclipse()
            {
                var eclipsePacket = MajorasMaskTribute.mod.GetPacket();
                eclipsePacket.Write((byte)MajorasMaskTribute.MessageType.StartEclipse);
                eclipsePacket.Send();
            }

            public static void RemoveEclipseDisc(byte player)
            {
                var rmPacket = MajorasMaskTribute.mod.GetPacket();
                rmPacket.Write((byte)MajorasMaskTribute.MessageType.RemoveEclipseDisc);
                rmPacket.Write(player);
                rmPacket.Send();
            }

            public static void PlayBell(Vector2 position, bool doWave)
            {
                var bellPacket = MajorasMaskTribute.mod.GetPacket();
                bellPacket.Write((byte)MessageType.PlayBell);
                bellPacket.WriteVector2(position);
                bellPacket.Write(doWave);
                bellPacket.Send();
            }

            public static void SendCycleCount(byte targetPlayer)
            {
                var countPacket = MajorasMaskTribute.mod.GetPacket();
                countPacket.Write((byte)MessageType.GetCycleCount);
                countPacket.Write(targetPlayer);
                countPacket.Send();
            }

            public static void DisintegrateNPC(IEnumerable<byte> victims)
            {
                var packet = MajorasMaskTribute.mod.GetPacket();
                packet.Write((byte)MajorasMaskTribute.MessageType.DisintegrateNPC);
                foreach (byte victim in victims)
                {
                    packet.Write(victim);
                }
                packet.Write(byte.MaxValue);
                packet.Send();
            }
        }
    }
}
