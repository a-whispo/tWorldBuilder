using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.TIGWEUI;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class LassoTool : SelectionTool
    {
        private bool _isSelecting = false;

        public LassoTool() : base("TerrariaInGameWorldEditor/UI/UIImages/BrushTool", "Lasso Tool")
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw a line from the first to the last point if we're selecting so you can see what the selection would be like
            if (_isSelecting)
            {
                var selectionArray = Selection.AsDictionary().ToArray();
                Vector2 pointLast = new Vector2(selectionArray[selectionArray.Count() - 1].Key.X * 16 - Main.screenPosition.X + 8, selectionArray[selectionArray.Count() - 1].Key.Y * 16 - Main.screenPosition.Y + 8);
                Vector2 pointFirst = new Vector2(selectionArray[0].Key.X * 16 - Main.screenPosition.X + 8, selectionArray[0].Key.Y * 16 - Main.screenPosition.Y + 8);
                DrawUtils.DrawLine(pointLast, pointFirst, color: TIGWEUISystem.Settings.ToolColor);
            }

            // draw the selection
            if (Selection.Count > 0)
            {
                DrawUtils.DrawTileCollectionOutline(Selection, new Point(Selection.GetMinX(), Selection.GetMinY()), TIGWEUISystem.Settings.ToolColor);
            }
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
                tc.TryAddTile(new Point(x0, y0), new TileCopy(Main.tile[x0, y0]));
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
                var selectionArray = Selection.AsDictionary().ToArray();
                Selection.TryAddTiles(CalculateTilesInLine(new Point(selectionArray[selectionArray.Length - 1].Key.X, selectionArray[selectionArray.Length - 1].Key.Y), new Point(selectionArray[0].Key.X, selectionArray[0].Key.Y)).AsDictionary());
                
                // get 
                var polygon = Selection.AsDictionary().ToList();
                int width = Selection.GetWidth();
                int height = Selection.GetHeight();
                int offsetX = Selection.GetMinX();
                int offsetY = Selection.GetMinY();

                for (int x = 0; x <= width; x++)
                {
                    for (int y = 0; y <= height; y++)
                    {
                        // test if tile is inside the polygon
                        Point testPoint = new Point(x + offsetX, y + offsetY);
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
                            Selection.TryAddTile(testPoint, new TileCopy(Main.tile[testPoint.X, testPoint.Y]));
                        }
                    }
                }
            }

            if (Main.mouseLeft && !Main.LocalPlayer.mouseInterface)
            {
                if (!_isSelecting)
                {
                    Selection.Clear();
                    _isSelecting = true;
                }

                // add tile to selection and drag a line between it and the last point to make sure we have a complete outline
                if (Selection.TryAddTile(new Point(Player.tileTargetX, Player.tileTargetY), new TileCopy(Main.tile[Player.tileTargetX, Player.tileTargetY])))
                {
                    // dont bother if we dont have at least 2 points in the selection
                    if (Selection.Count >= 2)
                    {
                        // fix all the gaps in the outline
                        var selectionArray = Selection.AsDictionary().ToArray();

                        // remove so the order of all the tiles arent messed up since this should be after the tiles in the line that we're adding
                        Selection.RemoveTile(new Point(Player.tileTargetX, Player.tileTargetY));

                        // draw line from the 2nd to last added tile and the tile that was just added and add the resulting tiles to the selection
                        Selection.TryAddTiles(CalculateTilesInLine(new Point(selectionArray[selectionArray.Length - 2].Key.X, selectionArray[selectionArray.Length - 2].Key.Y), new Point(selectionArray[selectionArray.Length - 1].Key.X, selectionArray[selectionArray.Length - 1].Key.Y)).AsDictionary());
                    }
                }
            }

            // clear selection bounds with right click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                Selection.Clear();
            }
        }
    }
}
