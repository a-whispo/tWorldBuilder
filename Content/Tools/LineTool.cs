using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class LineTool : Tool
    {
        // points
        private static Point _point1;
        private static bool _point1placed = false;
        private static Point _point2;
        private static bool _point2placed = false;

        private static TileCollection _tilesInLine = new TileCollection();
        private Point _lastPos;
        private int _lastd = 4;
        private int _d = 4;
        private int _length = 0;
        private int _yDiff = 0;
        private int _xDiff = 0;

        public LineTool() : base("TerrariaInGameWorldEditor/UI/UIImages/LineTool", "Line")
        {

        }

        public override void Update()
        {
            // have the mouse act as point 1 while point 1 isnt placed
            if (!_point1placed)
            {
                _point1 = new Point(Player.tileTargetX, Player.tileTargetY);
            }
            // have the mouse act as point 2 while point 2 isnt placed
            if (!_point2placed)
            {
                _point2 = new Point(Player.tileTargetX, Player.tileTargetY);
            }

            // only update line when needed
            if (_point2placed || _lastPos != Utils.ToTileCoordinates(Main.MouseWorld.ToVector2D()) || _lastd != _d || _tilesInLine.Count == 0 || _tilesInLine.AsDictionary().ToList()[0].Value != EditorSystem.Local.SelectedTile)
            {
                _lastPos = Utils.ToTileCoordinates(Main.MouseWorld.ToVector2D());
                _lastd = _d;
                _tilesInLine.Clear(); // clear so we can calculate what the line would look like
                CalculateTilesInLine(_point1, _point2, _d);
            }

            if (_point2placed)
            {
                _point1placed = false;
                _point2placed = false;
                ToolUtils.Paste(_tilesInLine, new Point(_tilesInLine.GetMinX(), _tilesInLine.GetMinY()), true, TIGWEUISystem.Settings.ShouldUpdateDrawnTiles);
                _tilesInLine.Clear();
            }

            InfoText = $"Size: {_d}, Length: {_length + _d - 1}, Y difference: {_yDiff}, X difference: {_xDiff}";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                // if point 2 isnt placed yet, show a preview of what it would look like if it was placed
                DrawUtils.DrawTileCollection(_tilesInLine, new Point(_tilesInLine.GetMinX(), _tilesInLine.GetMinY()));
                DrawUtils.DrawTileCollectionOutline(_tilesInLine, new Point(_tilesInLine.GetMinX(), _tilesInLine.GetMinY()), TIGWEUISystem.Settings.ToolColor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{TerrariaInGameWorldEditor.MODNAME}] Error drawing line tool: {ex}");
            }
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;
            // left click
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
                if (!_point1placed || (_point1placed && _point2placed)) // if both points have aleady been placed, reset them and place point 1 again
                {
                    _point1 = new Point(Player.tileTargetX, Player.tileTargetY); // get mouse coordinates in the world and save them to point1
                    _point2placed = false;
                    _point1placed = true;
                }
                else
                {
                    if (!_point2placed)
                    {
                        _point2 = new Point(Player.tileTargetX, Player.tileTargetY);
                        _point2placed = true;
                    }
                }
            }

            // right click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                if (_point1placed)
                {
                    // unplace points
                    _point1placed = false;

                    // clear tiles in line
                    _tilesInLine.Clear();
                }
            }
            if (Keybinds.Key1MK.Current || Keybinds.Key1MK.GetAssignedKeys().Count < 1)
            {
                PlayerInput.LockVanillaMouseScroll($"{TerrariaInGameWorldEditor.MODNAME}/Line");
                if (PlayerInput.ScrollWheelDelta > 0)
                {
                    _d++;
                }
                if (PlayerInput.ScrollWheelDelta < 0)
                {
                    if (_d >= 2)
                    {
                        _d--;
                    }
                }
            }
        }

        private void CalculateTilesInLine(Point origin, Point endpoint, int width)
        {
            // algorithm from https://rosettacode.org/wiki/Bitmap/Bresenham%27s_line_algorithm#C#
            var tiles = ToolUtils.GetEllipseFilledTileCollection(width, width, EditorSystem.Local.SelectedTile).AsDictionary().ToList();

            _length = 0;
            int x0 = origin.X;
            int y0 = origin.Y;
            int x1 = endpoint.X;
            int y1 = endpoint.Y;
            _yDiff = y0 - y1;
            _xDiff = x0 - x1;

            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            while (true)
            {
                _length++;
                // check if its worth doing calculations, if we havnt placed point 2 yet, if we have placed point 2 we obviously want to check where we should place all the tiles
                if (_point2placed || !((((x0 * 16) + 64) < (Main.screenPosition.X - 32) || ((x0 * 16) - 64) > (Main.screenPosition.X + Main.screenWidth) || ((y0 * 16) + 64) < (Main.screenPosition.Y) || ((y0 * 16) - 64) > (Main.screenPosition.Y + Main.screenHeight))))
                {
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        _tilesInLine.TryAddTile(new Point(x0 + tiles[i].Key.X, y0 + tiles[i].Key.Y), tiles[i].Value);
                    }
                }
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }
                e2 = err;
                if (e2 > -dx)
                {
                    err -= dy; x0 += sx;
                }
                if (e2 < dy)
                {
                    err += dx; y0 += sy;
                }
            }
        }
    }
}
