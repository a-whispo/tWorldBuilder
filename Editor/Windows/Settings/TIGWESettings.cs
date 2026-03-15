using Microsoft.Xna.Framework;
using TerrariaInGameWorldEditor.Common;

namespace TerrariaInGameWorldEditor.Editor.Windows.Settings
{
    public static class TIGWESettings
    {
        // main settings
        public static Color ToolColor { get; set; } = Color.White;
        public static Color MainPrimaryColor { get; set; } = new Color(43, 56, 101);
        public static Color MainSecondaryColor { get; set; } = new Color(72, 92, 168);
        public static Theme CurrentTheme { get; set; } = Theme.Default;
        public static bool ShowCenterLines { get; set; } = false;
        public static bool ShowMeasureLines { get; set; } = false;
        public static bool ShouldUpdateDrawnTiles { get; set; } = false;
        public static bool ShouldTeleportOnEditorClosed { get; set; } = true;
        public static bool ForceScaleUI { get; set; } = false;
        public static bool FullbrightEnabled { get; set; } = false;
        public static int HistoryLimit { get; set; } = 1000;
        public static bool ShouldShowMessages { get; set; } = false;
        public static bool ShouldShowErrors { get; set; } = true;
        public static bool ShouldShowFatalErrors { get; set; } = false;

        // mask settings
        public static bool ShouldPasteTiles { get; set; }
        public static bool ShouldPasteWalls { get; set; }
        public static bool ShouldPasteLiquid { get; set; }
        public static bool ShouldPasteWires { get; set; }
        public static bool ShouldPasteEmpty { get; set; }
        public static Mask ShouldPasteOnTiles { get; set; }
        public static Mask ShouldPasteOnWalls { get; set; }
        public static Mask ShouldPasteOnLiquid { get; set; }
        public static Mask ShouldPasteOnWires { get; set; }
    }
}
