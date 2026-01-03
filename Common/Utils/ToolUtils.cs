using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;

namespace TerrariaInGameWorldEditor.Common.Utils
{
    internal static class ToolUtils
    {
        public static void Paste(TileCollection tilesToPaste, Point point, bool saveToUndo = true, bool placeTileWithTileFraming = false) // takes point in terraria coordinates
        {
            // point is in tile coordinates

            // for undo history
            TileCollection tileColl = new TileCollection();

            // faster to get index with list
            List<KeyValuePair<Point, TileCopy>> tiles = tilesToPaste.ToNormalized().AsDictionary().ToList();

            // if we want to update the tiles we're pasting we need to make sure to grab a copy of what we're pasting over before we do the paste
            // as well as the tiles around it since those textures will also update
            if (placeTileWithTileFraming)
            {
                foreach (var tile in tiles)
                {
                    int x = tile.Key.X + point.X;
                    int y = tile.Key.Y + point.Y;

                    // check if location is in selected area
                    if ((!EditorSystem.Local.CurrentSelection?.ContainsCoord(new Point(x, y)) ?? false) && (EditorSystem.Local.CurrentSelection?.Count > 0))
                    {
                        continue;
                    }

                    // for undo history
                    tileColl.TryAddTile(new Point(x, y), new TileCopy(Main.tile[x, y]));
                    tileColl.TryAddTile(new Point(x + 1, y), new TileCopy(Main.tile[x + 1, y]));
                    tileColl.TryAddTile(new Point(x - 1, y), new TileCopy(Main.tile[x - 1, y]));
                    tileColl.TryAddTile(new Point(x, y + 1), new TileCopy(Main.tile[x, y + 1]));
                    tileColl.TryAddTile(new Point(x, y - 1), new TileCopy(Main.tile[x, y - 1]));
                }
            }

            foreach (var tile in tiles)
            {
                int x = tile.Key.X + point.X;
                int y = tile.Key.Y + point.Y;

                // check if location is in selected area
                if ((!EditorSystem.Local.CurrentSelection?.ContainsCoord(new Point(x, y)) ?? false) && (EditorSystem.Local.CurrentSelection?.Count > 0))
                {
                    continue;
                }

                // if we dont want the pasted tiles to update just add to history as we paste the tiles
                if (!placeTileWithTileFraming)
                {
                    // for undo history
                    tileColl.TryAddTile(new Point(x, y), new TileCopy(Main.tile[x, y]));
                }

                // temporary copy of the tile we want to paste
                TileCopy temp = new TileCopy(tile.Value.GetAsTile());
                if ((TIGWESettings.ShouldOnlyPasteOnAir && Main.tile[x, y].LiquidAmount == 0 && Main.tile[x, y].WallType == 0 && !Main.tile[x, y].HasTile) || !TIGWESettings.ShouldOnlyPasteOnAir)
                {
                    // check what we should paste on
                    if ((TIGWESettings.ShouldPasteOnTiles && Main.tile[x, y].HasTile) || (TIGWESettings.ShouldPasteOnWalls && Main.tile[x, y].WallType != 0) || (TIGWESettings.ShouldPasteOnLiquid && Main.tile[x, y].LiquidAmount > 0) || (TIGWESettings.ShouldPasteOnAir && (!Main.tile[x, y].HasTile && Main.tile[x, y].TileType == 0 && Main.tile[x, y].WallType == 0 && Main.tile[x, y].LiquidAmount == 0)))
                    {
                        // paste liquid
                        if (TIGWESettings.ShouldPasteLiquid)
                        {
                            if ((!TIGWESettings.ShouldPasteAir && tile.Value.LiquidAmount != 0) || TIGWESettings.ShouldPasteAir)
                            {
                                ((Tile)(Main.tile[x, y])).LiquidType = temp.LiquidType;
                                Main.tile[x, y].LiquidAmount = temp.LiquidAmount; // set liquid
                            }
                        }

                        // paste walls
                        if (TIGWESettings.ShouldPasteWalls)
                        {
                            if ((!TIGWESettings.ShouldPasteAir && tile.Value.WallType != 0) || TIGWESettings.ShouldPasteAir)
                            {
                                // get some temporary values
                                temp.CopyWallData(tile.Value.GetAsTile());
                                temp.CopyTileData(Main.tile[x, y]); // copy tile data of tiles at the location we're pasting to so we dont replace them
                                temp.CopyWireData(Main.tile[x, y]); // copy tile data of wires at the location we're pasting to so we dont replace them

                                // set walls
                                Main.tile[x, y].CopyFrom(temp.GetAsTile());

                                if (placeTileWithTileFraming)
                                {
                                    // update walls
                                    WorldGen.SquareWallFrame(x, y, true);
                                }
                            }
                        }

                        // paste tiles
                        if (TIGWESettings.ShouldPasteTiles)
                        {
                            if ((!TIGWESettings.ShouldPasteAir && tile.Value.HasTile) || TIGWESettings.ShouldPasteAir)
                            {
                                // get some temporary values
                                byte liquidAmount = Main.tile[x, y].LiquidAmount;
                                int liquidType = Main.tile[x, y].LiquidType;
                                temp.CopyTileData(tile.Value.GetAsTile()); // copy original tile data in case we replaced before when pasting walls
                                temp.CopyWallData(Main.tile[x, y]);
                                temp.CopyWireData(Main.tile[x, y]);

                                // set tiles
                                Main.tile[x, y].CopyFrom(temp.GetAsTile());
                                ((Tile)(Main.tile[x, y])).LiquidType = liquidType;
                                Main.tile[x, y].LiquidAmount = liquidAmount;

                                if (placeTileWithTileFraming)
                                {
                                    // squareframe but with noBreak
                                    // update tiles
                                    bool isTileFrameImportant = Main.tileFrameImportant[temp.TileType];
                                    WorldGen.TileFrame(x, y, true, !isTileFrameImportant);
                                    WorldGen.TileFrame(x + 1, y, true, !isTileFrameImportant);
                                    WorldGen.TileFrame(x - 1, y, true, !isTileFrameImportant);
                                    WorldGen.TileFrame(x, y + 1, true, !isTileFrameImportant);
                                    WorldGen.TileFrame(x, y - 1, true, !isTileFrameImportant);
                                    WorldGen.TileFrame(x + 1, y + 1, true, !isTileFrameImportant);
                                    WorldGen.TileFrame(x - 1, y + 1, true, !isTileFrameImportant);
                                    WorldGen.TileFrame(x - 1, y - 1, true, !isTileFrameImportant);
                                    WorldGen.TileFrame(x + 1, y - 1, true, !isTileFrameImportant);
                                }
                            }
                        }

                        // paste wire stuff
                        if (TIGWESettings.ShouldPasteWires)
                        {
                            if ((!TIGWESettings.ShouldPasteAir && (tile.Value.HasWire || tile.Value.HasActuator)) || TIGWESettings.ShouldPasteAir)
                            {
                                // get some temporary values
                                temp.CopyWireData(tile.Value.GetAsTile());
                                temp.CopyTileData(Main.tile[x, y]);
                                temp.CopyWallData(Main.tile[x, y]);
                                Main.tile[x, y].CopyFrom(temp.GetAsTile()); // set wires
                            }
                        }
                    }
                }
            }

            // add to undo history
            if (saveToUndo)
            {
                EditorSystem.Local.UndoHistory.Add(tileColl);
            }
        }

        public static List<Point> CalculatePointsInLine(Point origin, Point endpoint)
        {
            List<Point> points = new List<Point>();

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
                points.Add(new Point(x0, y0));
                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
            }

            return points;
        }

        public static TileCollection GetEllipseFilledTileCollection(int width, int height, TileCopy tile)
        {
            // mid point algo pretty much
            void PlotQuadrants(TileCollection tileColl, int x, int y, int xOffset, int yOffset, int width, int height)
            {
                while (y >= 0)
                {
                    int mx = (width % 2 == 0) ? -x - 1 : -x;
                    int my = (height % 2 == 0) ? -y - 1 : -y;

                    PlotTile(tileColl, x + xOffset, y + yOffset);
                    PlotTile(tileColl, mx + xOffset, y + yOffset);
                    PlotTile(tileColl, x + xOffset, my + yOffset);
                    PlotTile(tileColl, mx + xOffset, my + yOffset);
                    y--;
                }
            }

            void PlotTile(TileCollection tileColl, int x, int y)
            {
                var key = new Point(x, y);
                if (!tileColl.ContainsCoord(key))
                {
                    tileColl.TryAddTile(key, tile);
                }
            }

            TileCollection tileColl = new TileCollection();

            // determine center offsets
            int xOffset = (width % 2 == 0) ? 1 : 0;
            int yOffset = (height % 2 == 0) ? 1 : 0;

            width = (width % 2 == 0) ? width - 2 : width;
            height = (height % 2 == 0) ? height - 2 : height;

            // calculate radius and squares
            int a = width / 2;
            int b = height / 2;
            int a2 = a * a;
            int b2 = b * b;

            // starting position
            int x = 0;
            int y = b;

            int twoA2 = 2 * a2;
            int twoB2 = 2 * b2;
            int dx = twoB2 * x;
            int dy = twoA2 * y;

            // slope < 1
            int p = (int)(b2 - a2 * b + 0.25 * a2);
            while (dx < dy)
            {
                PlotQuadrants(tileColl, x, y, xOffset, yOffset, width, height);
                x++;
                dx += twoB2;
                if (p < 0)
                {
                    p += b2 + dx;
                }
                else
                {
                    y--;
                    dy -= twoA2;
                    p += b2 + dx - dy;
                }
            }

            // slope >= 1
            p = (int)(b2 * (x + 0.5) * (x + 0.5) + a2 * (y - 1) * (y - 1) - a2 * b2);
            while (y >= 0)
            {
                PlotQuadrants(tileColl, x, y, xOffset, yOffset, width, height);
                y--;
                dy -= twoA2;
                if (p > 0)
                {
                    p += a2 - dy;
                }
                else
                {
                    x++;
                    dx += twoB2;
                    p += a2 - dy + dx;
                }
            }

            return tileColl;
        }

        public static Rectangle GetRectangleFromPoints(Point point1, Point point2)
        {
            int p1x = point1.X;
            int p2x = point2.X;
            int p1y = point1.Y;
            int p2y = point2.Y;

            // get leftmost and rightmost positions in map
            int posLeft = (p1x > p2x ? p2x : p1x);
            int posRight = (p1x > p2x ? p1x : p2x);

            // set width with the new values
            int width = posRight - posLeft;

            // get upmost and downmost positions in map (y is lower the higher up you are)
            int posUp = (p1y > p2y ? p2y : p1y);
            int posDown = (p1y > p2y ? p1y : p2y);

            // set height with the new values
            int height = posDown - posUp;

            // rectangle 
            Rectangle selection = new Rectangle(posLeft, posUp, width + 1, height + 1);
            return selection;
        }
    }
}
