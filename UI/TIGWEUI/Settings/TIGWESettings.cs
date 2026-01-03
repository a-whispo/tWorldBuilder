using Microsoft.Xna.Framework;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.Settings
{
    public static class TIGWESettings
    {
        public static Color ToolColor { get; set; }
        public static bool ShowCenterLines { get; set; }
        public static bool ShowMeasureLines { get; set; }
        public static bool ShouldPasteAir { get; set; }
        public static bool ShouldPasteTiles { get; set; }
        public static bool ShouldPasteWalls { get; set; }
        public static bool ShouldPasteLiquid { get; set; }
        public static bool ShouldPasteWires { get; set; }
        public static bool ShouldOnlyPasteOnAir { get; set; }
        public static bool ShouldPasteOnTiles { get; set; }
        public static bool ShouldPasteOnWalls { get; set; }
        public static bool ShouldPasteOnLiquid { get; set; }
        public static bool ShouldPasteOnAir { get; set; }
        public static bool ShouldUpdateDrawnTiles { get; set; }
        public static bool ShouldTeleportOnEditorClosed { get; set; } = true;
    }
}
