using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class TilePickerTool : Tool
    {
        public TilePickerTool() : base("TerrariaInGameWorldEditor/UI/UIImages/TilePickerTool", "Tile Picker")
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Point point = new Point(Player.tileTargetX, Player.tileTargetY);
            TileCollection tc = new TileCollection();
            tc.TryAddTile(point, new TileCopy(Main.tile[point.X, point.Y]));
            DrawUtils.DrawTileCollectionOutline(tc, point, TIGWEUISystem.Settings.ToolColor);
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;

            // left click
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
                Point point = new Point(Player.tileTargetX, Player.tileTargetY);
                EditorSystem.Local.SelectedTile = new TileCopy(Main.tile[point.X, point.Y]);
            }
        }
    }
}
