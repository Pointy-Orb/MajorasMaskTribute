using System;
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
            RemoveEclipseDisc
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
        }
    }
}
