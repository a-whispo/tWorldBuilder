using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class TilePickerTool : Tool
    {
        public TilePickerTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/TilePickerTool"));
            ToggleToolButton.HoverText = "Tile Picker";
        }

        public override string GetInfoText()
        {
            return $"[c/EAD87A:Target Tile Type:] {(Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile ? TileID.Search.GetName(Main.tile[Player.tileTargetX, Player.tileTargetY].TileType) : "Air")} ([c/EAD87A:ID:] {Main.tile[Player.tileTargetX, Player.tileTargetY].TileType}), [c/EAD87A:Target Wall Type:] {(Main.tile[Player.tileTargetX, Player.tileTargetY].WallType != WallID.None ? WallID.Search.GetName(Main.tile[Player.tileTargetX, Player.tileTargetY].WallType) : "None")} ([c/EAD87A:ID:] {Main.tile[Player.tileTargetX, Player.tileTargetY].WallType})";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Point16 point = new Point16(Player.tileTargetX, Player.tileTargetY);
            TileCollection tc = new TileCollection();
            tc.TryAddTile(point, new TileCopy(Main.tile[point.X, point.Y]));
            DrawUtils.DrawTileCollectionOutline(tc, point.ToPoint(), EditorSystem.Local.Settings.ToolColor);
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
