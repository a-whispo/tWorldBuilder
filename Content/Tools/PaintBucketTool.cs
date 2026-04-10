using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class PaintBucketTool : FillTool
    {
        public PaintBucketTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/PaintBucketTool"));
            ToggleToolButton.HoverText = "Paint Bucket";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Point16 point = new Point16(Player.tileTargetX, Player.tileTargetY);
            TileCollection tc = new TileCollection();
            tc.TryAddTile(point, new TileCopy(point.X, point.Y));
            DrawUtils.DrawTileCollectionOutline(tc, point.ToPoint(), EditorSystem.Local.Settings.ToolColor);
        }

        protected override void OnFill(TileCollection tiles)
        {
            base.OnFill(tiles);
            TileCollection toPaste = new TileCollection();
            foreach (var tile in tiles)
            {
                toPaste.TryAddTile(tile.Key, EditorSystem.Local.SelectedTile);
            }
            ToolUtils.Paste(toPaste, new Point16(toPaste.GetMinX(), toPaste.GetMinY()), true, EditorSystem.Local.Settings.ShouldUpdateDrawnTiles);
        }

        protected override bool IsMatch(Point16 coords, TileCopy clickedTile)
        {
            if (!(((EditorSystem.Local.CurrentSelection?.ContainsCoord(coords)) ?? false) || EditorSystem.Local.CurrentSelection?.Count == 0))
            {
                return false;
            }
            return base.IsMatch(coords, clickedTile);
        }
    }
}