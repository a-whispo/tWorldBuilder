using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;

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
            DrawUtils.DrawTileCollectionOutline(tc, point, TIGWESettings.ToolColor);
        }

        public override void Update()
        {
            base.Update();
            InfoText = $"[c/EAD87A:Target Tile Type:] {(Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile ? TileID.Search.GetName(Main.tile[Player.tileTargetX, Player.tileTargetY].TileType) : "Air")} ([c/EAD87A:ID:] {Main.tile[Player.tileTargetX, Player.tileTargetY].TileType}), [c/EAD87A:Target Wall Type:] {(Main.tile[Player.tileTargetX, Player.tileTargetY].WallType != 0 ? WallID.Search.GetName(Main.tile[Player.tileTargetX, Player.tileTargetY].WallType) : "None")} ([c/EAD87A:ID:] {Main.tile[Player.tileTargetX, Player.tileTargetY].WallType})";
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
