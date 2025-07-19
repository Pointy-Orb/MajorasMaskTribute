using System;
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
            DisplayDayOf
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
        }
    }
}
