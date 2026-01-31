using log4net;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaInGameWorldEditor
{
    public class TerrariaInGameWorldEditor : Mod
	{
        public const string MODNAME = "TIGWE";
        public static ILog ModLogger;

        public override void Load()
        {
            base.Load();
            ModLogger = Logger;
        }

        public override void Unload()
        {
            base.Unload();
            ModLogger = null;
        }
    }
}
