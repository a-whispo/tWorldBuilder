using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace TerrariaInGameWorldEditor.Editor.Windows.Settings
{
    internal class FullbrightSystem : ModSystem
    {

        public override void Load()
        {
            base.Load();
            On_TileLightScanner.ApplyTileLight += ApplyFullbright1;
            On_TileLightScanner.ApplyWallLight += ApplyFullbright2;
            On_TileLightScanner.ApplyLiquidLight += ApplyFullbright3;
            On_TileLightScanner.ApplyHellLight += ApplyFullbright4;
            On_TileLightScanner.ApplySurfaceLight += ApplyFullbright5;
        }

        public override void Unload()
        {
            base.Unload();
            On_TileLightScanner.ApplyTileLight -= ApplyFullbright1;
            On_TileLightScanner.ApplyWallLight -= ApplyFullbright2;
            On_TileLightScanner.ApplyLiquidLight -= ApplyFullbright3;
            On_TileLightScanner.ApplyHellLight -= ApplyFullbright4;
            On_TileLightScanner.ApplySurfaceLight -= ApplyFullbright5;
        }

        private void ApplyFullbright1(On_TileLightScanner.orig_ApplyTileLight orig, TileLightScanner self, Tile tile, int x, int y, ref FastRandom localRandom, ref Vector3 lightColor)
        {
            if (EditorSystem.Local.Settings.FullbrightEnabled && EditorSystem.Local.IsEditorVisible)
            {
                lightColor = new Vector3(255, 255, 255);
            }
            else
            {
                orig(self, tile, x, y, ref localRandom, ref lightColor);
            }
        }

        private void ApplyFullbright2(On_TileLightScanner.orig_ApplyWallLight orig, TileLightScanner self, Tile tile, int x, int y, ref FastRandom localRandom, ref Vector3 lightColor)
        {
            if (EditorSystem.Local.Settings.FullbrightEnabled && EditorSystem.Local.IsEditorVisible)
            {
                lightColor = new Vector3(255, 255, 255);
            }
            else
            {
                orig(self, tile, x, y, ref localRandom, ref lightColor);
            }
        }

        private void ApplyFullbright3(On_TileLightScanner.orig_ApplyLiquidLight orig, TileLightScanner self, Tile tile, ref Vector3 lightColor)
        {
            if (EditorSystem.Local.Settings.FullbrightEnabled && EditorSystem.Local.IsEditorVisible)
            {
                lightColor = new Vector3(255, 255, 255);
            }
            else
            {
                orig(self, tile, ref lightColor);
            }
        }

        private void ApplyFullbright4(On_TileLightScanner.orig_ApplyHellLight orig, TileLightScanner self, Tile tile, int x, int y, ref Vector3 lightColor)
        {
            if (EditorSystem.Local.Settings.FullbrightEnabled && EditorSystem.Local.IsEditorVisible)
            {
                lightColor = new Vector3(255, 255, 255);
            }
            else
            {
                orig(self, tile, x, y, ref lightColor);
            }
        }

        private void ApplyFullbright5(On_TileLightScanner.orig_ApplySurfaceLight orig, TileLightScanner self, Tile tile, int x, int y, ref Vector3 lightColor)
        {
            if (EditorSystem.Local.Settings.FullbrightEnabled && EditorSystem.Local.IsEditorVisible)
            {
                lightColor = new Vector3(255, 255, 255);
            }
            else
            {
                orig(self, tile, x, y, ref lightColor);
            }
        }
    }
}
