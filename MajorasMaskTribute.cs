using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.Graphics.Effects;
using MajorasMaskTribute.Common;

namespace MajorasMaskTribute
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class MajorasMaskTribute : Mod
    {
        internal static Mod mod;

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
    }
}
