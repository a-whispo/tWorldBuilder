using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TerrariaInGameWorldEditor.Editor;

namespace TerrariaInGameWorldEditor.Common.Utils
{
    internal static class ToolUtils
    {
        public static void Paste(TileCollection tilesToPaste, Point16 point, bool saveToUndo = true, bool placeTileWithTileFraming = false) // takes point in terraria coordinates
        {
            TileCollection undoColl = new TileCollection();

            // if we want to update the tiles we're pasting we need to make sure to grab a copy of what we're pasting over before we do the paste
            // as well as the tiles around it since those textures will also update
            if (placeTileWithTileFraming && saveToUndo)
            {
                foreach (var tile in tilesToPaste)
                {
                    int x = tile.Key.X + point.X - tilesToPaste.GetMinX();
                    int y = tile.Key.Y + point.Y - tilesToPaste.GetMinY();
                    if (Math.Abs(x - Main.maxTilesX) <= 1 || x - 1 < 0 || Math.Abs(y - Main.maxTilesY) <= 1 || y - 1 < 0)
                    {
                        continue;
                    }
                    if ((!EditorSystem.Local.CurrentSelection?.ContainsCoord(new Point16(x, y)) ?? false) && (EditorSystem.Local.CurrentSelection?.Count > 0))
                    {
                        continue;
                    }
                    undoColl.TryAddTile(new Point16(x, y), () => new TileCopy(Main.tile[x, y]));
                    undoColl.TryAddTile(new Point16(x + 1, y), () => new TileCopy(Main.tile[x + 1, y]));
                    undoColl.TryAddTile(new Point16(x - 1, y), () => new TileCopy(Main.tile[x - 1, y]));
                    undoColl.TryAddTile(new Point16(x, y + 1), () => new TileCopy(Main.tile[x, y + 1]));
                    undoColl.TryAddTile(new Point16(x, y - 1), () => new TileCopy(Main.tile[x, y - 1]));
                }
            }

            foreach (var item in tilesToPaste)
            {
                int x = item.Key.X + point.X - tilesToPaste.GetMinX();
                int y = item.Key.Y + point.Y - tilesToPaste.GetMinY();
                TileCopy tile = item.Value;
                if (Math.Abs(x - Main.maxTilesX) <= 1 || x - 1 < 0 || Math.Abs(y - Main.maxTilesY) <= 1 || y - 1 < 0)
                {
                    continue;
                }
                if ((!EditorSystem.Local.CurrentSelection?.ContainsCoord(new Point16(x, y)) ?? false) && (EditorSystem.Local.CurrentSelection?.Count > 0))
                {
                    continue;
                }

                // if we dont want the pasted tiles to update just add to history as we paste the tiles
                if (!placeTileWithTileFraming && saveToUndo)
                {
                    undoColl.TryAddTile(new Point16(x, y), () => new TileCopy(Main.tile[x, y]));
                }

                if (CompareTileToDrawOnMask(Main.tile[x, y]))
                {
                    if (!EditorSystem.Local.Settings.ShouldPasteEmpty && WouldTileCopyBeEmpty(tile))
                    {
                        continue;
                    }

                    // paste liquid
                    if (EditorSystem.Local.Settings.ShouldPasteLiquid)
                    {
                        ((Tile)(Main.tile[x, y])).LiquidType = tile.LiquidType;
                        Main.tile[x, y].LiquidAmount = tile.LiquidAmount; // set liquid
                    }

                    // paste walls
                    if (EditorSystem.Local.Settings.ShouldPasteWalls)
                    {
                        // set and update walls
                        Main.tile[x, y].CopyWallData(tile);
                    }

                    // paste tiles
                    if (EditorSystem.Local.Settings.ShouldPasteTiles)
                    {
                        // get some temporary values and copy data
                        byte liquidAmount = Main.tile[x, y].LiquidAmount;
                        int liquidType = Main.tile[x, y].LiquidType;

                        // set tiles and update
                        Main.tile[x, y].CopyTileData(tile);
                        ((Tile)(Main.tile[x, y])).LiquidType = liquidType;
                        Main.tile[x, y].LiquidAmount = liquidAmount;
                    }

                    // paste wire stuff
                    if (EditorSystem.Local.Settings.ShouldPasteWires)
                    {
                        Main.tile[x, y].CopyWireData(tile);
                    }
                }
            }

            // update tiles and those around it
            if (placeTileWithTileFraming)
            {
                void CheckIfShouldUpdate(Point16 coord)
                {
                    if (!tilesToPaste.ContainsCoord(coord))
                    {
                        int px = coord.X - tilesToPaste.GetMinX() + point.X;
                        int py = coord.Y - tilesToPaste.GetMinY() + point.Y;
                        WorldGen.TileFrame(px, py, true, true);
                        WorldGen.SquareWallFrame(px, py, true);
                    }
                }
                foreach (var tile in tilesToPaste)
                {
                    int px = tile.Key.X - tilesToPaste.GetMinX() + point.X;
                    int py = tile.Key.Y - tilesToPaste.GetMinY() + point.Y;
                    bool imporant = Main.tileFrameImportant[tile.Value.TileType];
                    WorldGen.TileFrame(px, py, true, !imporant);
                    WorldGen.SquareWallFrame(px, py, true);
                    CheckIfShouldUpdate(new Point16(tile.Key.X + 1, tile.Key.Y));
                    CheckIfShouldUpdate(new Point16(tile.Key.X - 1, tile.Key.Y));
                    CheckIfShouldUpdate(new Point16(tile.Key.X, tile.Key.Y + 1));
                    CheckIfShouldUpdate(new Point16(tile.Key.X, tile.Key.Y - 1));
                }
            }

            if (saveToUndo)
            {
                EditorSystem.Local.AddToUndoHistory(undoColl);
            }
        }

        public static bool WouldTileCopyBeEmpty(TileCopy tile)
        {
            return !(EditorSystem.Local.Settings.ShouldPasteTiles && tile.HasTile || EditorSystem.Local.Settings.ShouldPasteWalls && tile.WallType != WallID.None || EditorSystem.Local.Settings.ShouldPasteLiquid && tile.LiquidAmount != 0 || EditorSystem.Local.Settings.ShouldPasteWires && (tile.HasWire || tile.HasActuator));
        }

        public static bool CompareTileToDrawOnMask(Tile tile)
        {
            bool CompareMaskToBool(Mask mask, bool value)
            {
                switch (mask)
                {
                    case Mask.Yes:
                        return value;
                    case Mask.Any:
                        return true;
                    case Mask.No:
                        return !value;
                    default: 
                        return false;
                }
            }

            if (!CompareMaskToBool(EditorSystem.Local.Settings.ShouldPasteOnTiles, (tile.HasTile)))
            {
                return false;
            }
            if (!CompareMaskToBool(EditorSystem.Local.Settings.ShouldPasteOnWalls, (tile.WallType != WallID.None)))
            {
                return false;
            }
            if (!CompareMaskToBool(EditorSystem.Local.Settings.ShouldPasteOnLiquid, (tile.LiquidAmount > 0)))
            {
                return false;
            }
            if (!CompareMaskToBool(EditorSystem.Local.Settings.ShouldPasteOnWires, (tile.HasActuator || tile.RedWire || tile.GreenWire || tile.BlueWire || tile.YellowWire)))
            {
                return false;
            }
            return true;
        }

        public static void CopyWallData(this Tile original, TileCopy tile)
        {
            original.WallType = tile.WallType;
            original.WallFrameNumber = tile.WallFrameNumber;
            original.IsWallInvisible = tile.IsWallInvisible;
            original.WallColor = tile.WallColor;
            original.IsWallFullbright = tile.IsWallFullbright;
            original.WallFrameX = tile.WallFrameX;
            original.WallFrameY = tile.WallFrameY;
        }

        public static void CopyTileData(this Tile original, TileCopy tile)
        {
            original.HasTile = tile.HasTile;
            original.TileType = tile.TileType;
            original.LiquidType = tile.LiquidType;
            original.Slope = tile.Slope;
            original.IsHalfBlock = tile.IsHalfBlock;
            original.TileColor = tile.TileColor;
            original.IsTileInvisible = tile.IsTileInvisible;
            original.CheckingLiquid = tile.CheckingLiquid;
            original.LiquidAmount = tile.LiquidAmount;
            original.SkipLiquid = tile.SkipLiquid;
            original.TileFrameNumber = tile.TileFrameNumber;
            original.TileFrameX = tile.TileFrameX;
            original.TileFrameY = tile.TileFrameY;
            original.IsActuated = tile.IsActuated;
        }

        public static void CopyWireData(this Tile original, TileCopy tile)
        {
            original.BlueWire = tile.BlueWire;
            original.GreenWire = tile.GreenWire;
            original.RedWire = tile.RedWire;
            original.YellowWire = tile.YellowWire;
            original.HasActuator = tile.HasActuator;
        }

        public static void Delete(TileCollection tilesToDelete, bool saveToUndo = true)
        {
            TileCollection undoColl = new TileCollection();

            foreach (var tile in tilesToDelete)
            {
                int x = tile.Key.X;
                int y = tile.Key.Y;
                undoColl.TryAddTile(new Point16(x, y), () => new TileCopy(Main.tile[x, y]));

                if (CompareTileToDrawOnMask(Main.tile[x, y]))
                {
                    if (EditorSystem.Local.Settings.ShouldPasteTiles)
                    {
                        ((Tile)Main.tile[x, y]).HasTile = false;
                        Main.tile[x, y].TileType = TileID.Dirt;
                    }
                    if (EditorSystem.Local.Settings.ShouldPasteWalls)
                    {
                        Main.tile[x, y].WallType = WallID.None;
                    }
                    if (EditorSystem.Local.Settings.ShouldPasteLiquid)
                    {
                        Main.tile[x, y].LiquidAmount = 0;
                    }
                    if (EditorSystem.Local.Settings.ShouldPasteWires)
                    {
                        ((Tile)Main.tile[x, y]).GreenWire = false;
                        ((Tile)Main.tile[x, y]).RedWire = false;
                        ((Tile)Main.tile[x, y]).YellowWire = false;
                        ((Tile)Main.tile[x, y]).BlueWire = false;
                    }
                }
            }

            if (saveToUndo)
            {
                EditorSystem.Local.AddToUndoHistory(undoColl);
            }
        }

        public static List<Point16> CalculatePointsInLine(Point16 origin, Point16 endpoint)
        {
            List<Point16> points = new List<Point16>();

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
                points.Add(new Point16(x0, y0));
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
                Point16 key = new Point16(x, y);
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
