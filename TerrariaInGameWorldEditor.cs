using log4net;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.UIElements;

namespace TerrariaInGameWorldEditor
{
    public class TerrariaInGameWorldEditor : Mod
	{
        public const string MODNAME = "TIGWE";
        public const string ASSET_PATH = $"TerrariaInGameWorldEditor";
        public static ILog ModLogger;

        public override void Load()
        {
            base.Load();
            UIElementUtils.Path = $"TerrariaInGameWorldEditor";
            ModLogger = Logger;
        }

        public override void Unload()
        {
            base.Unload();
            ModLogger = null;
        }
    }
}
