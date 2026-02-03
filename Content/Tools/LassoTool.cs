using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor.Windows.Settings;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class LassoTool : SelectionTool
    {
        private bool _isSelecting = false;

        public LassoTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/LassoTool"));
            ToggleToolButton.HoverText = "Lasso";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw a line from the first to the last point if we're selecting so you can see what the selection would be like
            if (_isSelecting)
            {
                var selectionArray = _selection.ToArray();
                Vector2 pointLast = new Vector2(selectionArray[selectionArray.Count() - 1].Key.X * 16 - Main.screenPosition.X + 8, selectionArray[selectionArray.Count() - 1].Key.Y * 16 - Main.screenPosition.Y + 8);
                Vector2 pointFirst = new Vector2(selectionArray[0].Key.X * 16 - Main.screenPosition.X + 8, selectionArray[0].Key.Y * 16 - Main.screenPosition.Y + 8);
                DrawUtils.DrawLine(pointLast, pointFirst, color: TIGWESettings.ToolColor);
            }
        }

        public override string GetInfoText()
        {
            return $"[c/EAD87A:Count:] {_selection.Count}";
        }

        private TileCollection CalculateTilesInLine(Point origin, Point endpoint)
        {
            TileCollection tc = new TileCollection();

            // algorithm from https://rosettacode.org/wiki/Bitmap/Bresenham%27s_line_algorithm#C#

            int x0 = origin.X;
            int y0 = origin.Y;

            int x1 = endpoint.X;
            int y1 = endpoint.Y;

            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            for (; ; )
            {
                tc.TryAddTile(new Point16(x0, y0), new TileCopy(Main.tile[x0, y0]));
                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
            }

            return tc;
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;
            if (Main.mouseLeftRelease && _isSelecting)
            {
                _isSelecting = false;

                // add add a line from the last point to the first point to close the selection
                var selectionArray = _selection.ToArray();
                _selection.TryAddTiles(CalculateTilesInLine(new Point(selectionArray[selectionArray.Length - 1].Key.X, selectionArray[selectionArray.Length - 1].Key.Y), new Point(selectionArray[0].Key.X, selectionArray[0].Key.Y)));
                
                // get 
                var polygon = _selection.ToList();
                int width = _selection.GetWidth();
                int height = _selection.GetHeight();
                int offsetX = _selection.GetMinX();
                int offsetY = _selection.GetMinY();

                for (int x = 0; x <= width; x++)
                {
                    for (int y = 0; y <= height; y++)
                    {
                        // test if tile is inside the polygon
                        Point16 testPoint = new Point16(x + offsetX, y + offsetY);
                        bool inside = false;
                        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
                        {
                            var pi = polygon[i];
                            var pj = polygon[j];

                            bool intersect = ((pi.Key.Y > testPoint.Y) != (pj.Key.Y > testPoint.Y)) && (testPoint.X < (pj.Key.X - pi.Key.X) * (testPoint.Y - pi.Key.Y) / (pj.Key.Y - pi.Key.Y) + pi.Key.X);

                            if (intersect)
                            {
                                inside = !inside;
                            }
                        }
                        if (inside)
                        {
                            _selection.TryAddTile(testPoint, new TileCopy(Main.tile[testPoint.X, testPoint.Y]));
                        }
                    }
                }
            }

            if (Main.mouseLeft && !Main.LocalPlayer.mouseInterface)
            {
                if (!_isSelecting)
                {
                    _selection.Clear();
                    _isSelecting = true;
                }

                // add tile to selection and drag a line between it and the last point to make sure we have a complete outline
                if (_selection.TryAddTile(new Point16(Player.tileTargetX, Player.tileTargetY), new TileCopy(Main.tile[Player.tileTargetX, Player.tileTargetY])))
                {
                    if (_selection.Count >= 2)
                    {
                        var selectionArray = _selection.ToArray();
                        _selection.RemoveTile(new Point16(Player.tileTargetX, Player.tileTargetY)); // remove so the order of all the tiles arent messed up since this should be after the tiles in the line that we're adding
                        _selection.TryAddTiles(CalculateTilesInLine(new Point(selectionArray[selectionArray.Length - 2].Key.X, selectionArray[selectionArray.Length - 2].Key.Y), new Point(selectionArray[selectionArray.Length - 1].Key.X, selectionArray[selectionArray.Length - 1].Key.Y)));
                    }
                }
            }

            // clear selection bounds with right click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                _selection.Clear();
            }
        }
    }
}
