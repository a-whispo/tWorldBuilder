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
            On_Lighting.GetColor_int_int += ApplyFullbright2;
        }

        public override void Unload()
        {
            base.Unload();
            On_TileLightScanner.ApplyTileLight -= ApplyFullbright1;
            On_Lighting.GetColor_int_int -= ApplyFullbright2;
        }

        private void ApplyFullbright1(On_TileLightScanner.orig_ApplyTileLight orig, TileLightScanner self, Tile tile, int x, int y, ref FastRandom localRandom, ref Vector3 lightColor)
        {
            if (TIGWESettings.FullbrightEnabled && EditorSystem.Local.IsEditorVisible)
            {
                lightColor = new Vector3(255, 255, 255);
            }
        }

        private Color ApplyFullbright2(On_Lighting.orig_GetColor_int_int orig, int x, int y)
        {
            if (TIGWESettings.FullbrightEnabled && EditorSystem.Local.IsEditorVisible)
            {
                return new Color(255, 255, 255);
            }
            return orig(x, y);
        }
    }
}
