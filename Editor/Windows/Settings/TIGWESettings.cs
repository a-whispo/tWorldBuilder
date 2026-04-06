using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TerrariaInGameWorldEditor.Common;

namespace TerrariaInGameWorldEditor.Editor.Windows.Settings
{
    public class TIGWESettings
    {
        // main settings
        public Color ToolColor { get; set; } = Color.White;
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public Theme CurrentTheme { get; set; } = Theme.Default;
        public bool ShowCenterLines { get; set; } = false;
        public bool ShowMeasureLines { get; set; } = false;
        public bool ShouldUpdateDrawnTiles { get; set; } = true;
        public bool ShouldTeleportOnEditorClosed { get; set; } = true;
        public bool ForceScaleUI { get; set; } = false;
        public float UIScale { get; set; } = 1f;
        public bool FullbrightEnabled { get; set; } = false;
        public int HistoryLimit { get; set; } = 1000;
        public bool ShouldShowActiveSelectionText { get; set; } = true;
        public bool ShouldShowMessages { get; set; } = true;
        public bool ShouldShowErrors { get; set; } = true;
        public bool ShouldShowFatalErrors { get; set; } = true;

        // mask settings
        public bool ShouldPasteTiles { get; set; }
        public bool ShouldPasteWalls { get; set; }
        public bool ShouldPasteLiquid { get; set; }
        public bool ShouldPasteWires { get; set; }
        public bool ShouldPasteEmpty { get; set; }
        public Mask ShouldPasteOnTiles { get; set; }
        public Mask ShouldPasteOnWalls { get; set; }
        public Mask ShouldPasteOnLiquid { get; set; }
        public Mask ShouldPasteOnWires { get; set; }

        public static TIGWESettings Load(string path)
        {
            try
            {
                string p = path.Replace(".json", "") + ".json";
                if (File.Exists(p))
                {
                    return JsonSerializer.Deserialize<TIGWESettings>(File.ReadAllText(p), new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } });
                }
            }
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.Warn("Failed to load settings.", ex);
            }
            return new TIGWESettings();
        }

        public static void Save(string path, TIGWESettings settings)
        {
            try
            {
                string p = path.Replace(".json", "") + ".json";
                Directory.CreateDirectory(Path.GetDirectoryName(p));
                File.WriteAllText(p, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } }));
            } 
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.Warn("Failed to save settings.", ex);
            }
        }
    }
}
