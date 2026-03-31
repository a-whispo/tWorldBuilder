using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class EraseTool : BrushTool
    {
        private TileCopy _air;

        public EraseTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/EraseTool"));
            ToggleToolButton.HoverText = "Eraser";

            // remove mode setting
            Settings.RemoveAt(0);

            // make sure brush is set at the start
            Tile t = new Tile();
            t.TileType = TileID.Dirt;
            t.HasTile = false;
            _air = new TileCopy(t);
            _brush.Clear();
            _brush.TryAddTiles(ToolUtils.GetEllipseFilledTileCollection(_d, _d, _air));
        }

        protected override void UpdateBrush()
        {
            _brush.Clear();
            _brush.TryAddTiles(ToolUtils.GetEllipseFilledTileCollection(_d, _d, _air));
        }
    }
}
