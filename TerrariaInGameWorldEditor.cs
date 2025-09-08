using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.UI;

namespace TerrariaInGameWorldEditor
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class TerrariaInGameWorldEditor : Mod
	{
        public const string MODNAME = "TIGWE";
        public MainScreen MainScreen;

        public override void PostSetupContent()
        {
            base.PostSetupContent();
            MainScreen = new MainScreen();
        }
    }
}
