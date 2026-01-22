using Microsoft.Xna.Framework;
using TerrariaInGameWorldEditor.Common;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.Settings
{
    public static class TIGWESettings
    {
        public static Color ToolColor { get; set; }
        public static bool ShowCenterLines { get; set; }
        public static bool ShowMeasureLines { get; set; }
        public static bool ShouldPasteTiles { get; set; }
        public static bool ShouldPasteWalls { get; set; }
        public static bool ShouldPasteLiquid { get; set; }
        public static bool ShouldPasteWires { get; set; }
        public static bool ShouldPasteEmpty { get; set; }
        public static Mask ShouldPasteOnTiles { get; set; }
        public static Mask ShouldPasteOnWalls { get; set; }
        public static Mask ShouldPasteOnLiquid { get; set; }
        public static Mask ShouldPasteOnWires { get; set; }
        public static bool ShouldUpdateDrawnTiles { get; set; }
        public static bool ShouldTeleportOnEditorClosed { get; set; } = true;
    }
}
