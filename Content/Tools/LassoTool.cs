using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class LassoTool : Tool, ISelectionTool
    {
        private bool _isSelecting = false;
        private TileCollection _selection = new TileCollection();
        private TileCollection _selectionOutlinePreview = new TileCollection();

        public LassoTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/LassoTool"));
            ToggleToolButton.HoverText = "Lasso \n[c/EAD87A:Right Mouse:] Remove selection \n[c/EAD87A:Left Mouse:] New selection \n[c/EAD87A:Ctrl + Left Mouse:] Add to selection \n[c/EAD87A:Shift + Left Mouse:] Remove from selection";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw a line from the first to the last point if we're selecting so you can see what the selection would be like
            if (_isSelecting && _selectionOutlinePreview.Count > 0)
            {
                var selectionArray = _selectionOutlinePreview.ToArray();
                Vector2 pointLast = new Vector2(selectionArray[selectionArray.Count() - 1].Key.X * 16 - Main.screenPosition.X + 8, selectionArray[selectionArray.Count() - 1].Key.Y * 16 - Main.screenPosition.Y + 8);
                Vector2 pointFirst = new Vector2(selectionArray[0].Key.X * 16 - Main.screenPosition.X + 8, selectionArray[0].Key.Y * 16 - Main.screenPosition.Y + 8);
                Color color = EditorSystem.Local.Settings.ToolColor;
                if (_selection.Count > 0)
                {
                    color = PlayerInput.GetPressedKeys().Contains(Keys.LeftShift) ? Color.IndianRed : Color.ForestGreen;
                }
                DrawUtils.DrawLine(pointLast, pointFirst, 4, color);
                DrawUtils.DrawTileCollectionOutline(_selectionOutlinePreview, new Point(_selectionOutlinePreview.GetMinX(), _selectionOutlinePreview.GetMinY()), color);
            }
        }

        public TileCollection GetSelection()
        {
            return _selection;
        }

        public void ResetSelection()
        {
            _selection.Clear();
            _selectionOutlinePreview.Clear();
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
            for (;;)
            {
                tc.TryAddTile(new Point16(x0, y0), new TileCopy(x0, y0));
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
                var polygon = _selectionOutlinePreview.ToList();
                _selectionOutlinePreview.TryAddTiles(CalculateTilesInLine(new Point(polygon[polygon.Count - 1].Key.X, polygon[polygon.Count - 1].Key.Y), new Point(polygon[0].Key.X, polygon[0].Key.Y)));

                // scanline fill
                int minY = _selectionOutlinePreview.GetMinY();
                int maxY = minY + _selectionOutlinePreview.GetHeight();
                for (int y = minY; y <= maxY; y++)
                {
                    List<int> intersections = new List<int>();
                    for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
                    {
                        Point16 pointi = polygon[i].Key;
                        Point16 pointj = polygon[j].Key;

                        if ((pointi.Y > y) != (pointj.Y > y))
                        {
                            int x = pointi.X + (y - pointi.Y) * (pointj.X - pointi.X) / (pointj.Y - pointi.Y);
                            intersections.Add(x);
                        }
                    }

                    intersections.Sort();
                    for (int k = 0; k < intersections.Count; k += 2)
                    {
                        if (k + 1 >= intersections.Count)
                        {
                            break;
                        }

                        int xStart = intersections[k];
                        int xEnd = intersections[k + 1];
                        for (int x = xStart; x <= xEnd; x++)
                        {
                            Point16 point = new Point16(x, y);
                            _selectionOutlinePreview.TryAddTile(point, new TileCopy(x, y));
                        }
                    }
                }

                // if shift is pressed down remove tile from our selection
                if (PlayerInput.GetPressedKeys().Contains(Keys.LeftShift) && _selection.Count > 0)
                {
                    _selection.TryRemoveTiles(_selectionOutlinePreview);
                }
                else
                {
                    _selection.TryAddTiles(_selectionOutlinePreview);
                }
            }

            if (Main.mouseLeft && !Main.LocalPlayer.mouseInterface)
            {
                if (!_isSelecting)
                {
                    // if ctrl is pressed down, keep adding tiles to our selection, if shift is pressed down remove tile from our selection
                    if (!PlayerInput.GetPressedKeys().Contains(Keys.LeftControl) && !PlayerInput.GetPressedKeys().Contains(Keys.LeftShift))
                    {
                        _selection.Clear();
                    }
                    _selectionOutlinePreview.Clear();
                    _isSelecting = true;
                }

                // add tile to selection and drag a line between it and the last point to make sure we have a complete outline
                if (_selectionOutlinePreview.TryAddTile(new Point16(Player.tileTargetX, Player.tileTargetY), new TileCopy(Player.tileTargetX, Player.tileTargetY)))
                {
                    if (_selectionOutlinePreview.Count >= 2)
                    {
                        var selectionArray = _selectionOutlinePreview.ToArray();
                        _selectionOutlinePreview.TryRemoveTile(new Point16(Player.tileTargetX, Player.tileTargetY)); // remove so the order of all the tiles arent messed up since this should be after the tiles in the line that we're adding
                        _selectionOutlinePreview.TryAddTiles(CalculateTilesInLine(new Point(selectionArray[selectionArray.Length - 2].Key.X, selectionArray[selectionArray.Length - 2].Key.Y), new Point(selectionArray[selectionArray.Length - 1].Key.X, selectionArray[selectionArray.Length - 1].Key.Y)));
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
