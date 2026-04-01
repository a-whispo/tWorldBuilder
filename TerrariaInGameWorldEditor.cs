using log4net;
using System;
using Terraria;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.UIElements;

namespace TerrariaInGameWorldEditor
{
    public class TerrariaInGameWorldEditor : Mod
	{
        public const string MODNAME = "tWB";
        public const string ASSET_PATH = $"TerrariaInGameWorldEditor";
        private static ILog _modLogger;

        public override void Load()
        {
            base.Load();
            UIElementUtils.Path = $"TerrariaInGameWorldEditor";
            _modLogger = Logger;
        }

        public override void Unload()
        {
            base.Unload();
            _modLogger = null;
        }

        public static void NewText(string text)
        {
            if (EditorSystem.Local.Settings.ShouldShowMessages)
            {
                Main.NewText($"[c/606fA4:({MODNAME})] {text}");
            }
        }

        public static void Warn(string text, Exception ex)
        {
            if (EditorSystem.Local.Settings.ShouldShowErrors)
            {
                _modLogger.Warn(text, ex);
                Main.NewText($"[c/FF9900:({MODNAME} Warning)] {text}");
            }
        }

        public static void Warn(string text)
        {
            if (EditorSystem.Local.Settings.ShouldShowErrors)
            {
                _modLogger.Warn(text);
                Main.NewText($"[c/FF9900:({MODNAME} Warning)] {text}");
            }
        }

        public static void Error(string text, Exception ex)
        {
            if (EditorSystem.Local.Settings.ShouldShowFatalErrors)
            {
                _modLogger.Error(text, ex);
                Main.NewText($"[c/CC3300:({MODNAME} Error)] {text}");
            }
        }

        public static void Error(string text)
        {
            if (EditorSystem.Local.Settings.ShouldShowFatalErrors)
            {
                _modLogger.Error(text);
                Main.NewText($"[c/CC3300:({MODNAME} Error)] {text}");
            }
        }
    }
}
