using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class PaintBucketTool : Tool
    {
        private int _tileCap = 10000;

        public PaintBucketTool() : base("TerrariaInGameWorldEditor/UI/UIImages/PaintBucketTool", "Paint Bucket")
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
                // flood fill kinda
                // will add some settings so you can choose to only fill tiles and/or walls and/or liquid if you want to

                // get point and clicked tile
                Point point = new Point(Player.tileTargetX, Player.tileTargetY);
                TileCopy clickedTile = new TileCopy(Main.tile[point.X, point.Y]);

                // queue to store tiles we want check
                Queue<Point> queue = new Queue<Point>();
                queue.Enqueue(new Point(point.X, point.Y));

                // counter to see when we reach the tilecap
                int count = 0;

                // new action so we can undo this later if we want to
                TileCollection tileColl = new TileCollection();
                try
                {
                    // go until we hit the tilecap or cant find any more tiles that we want to replace
                    while (queue.Count > 0 && count < _tileCap)
                    {
                        // get the coordinates at the tile in the first index
                        Point coords = queue.Dequeue();

                        // if we clicked a wall
                        if ((Main.tile[coords.X, coords.Y].WallType == clickedTile.WallType) && (!Main.tile[coords.X, coords.Y].HasTile && !clickedTile.HasTile) && (((EditorSystem.Local.CurrentSelection?.ContainsCoord(coords)) ?? false) || EditorSystem.Local.CurrentSelection?.Count == 0))
                        {
                            // if we dont already have it added, add it
                            if (tileColl.TryAddTile(coords, new TileCopy(Main.tile[coords.X, coords.Y])))
                            {
                                count++;
                            }

                            // add the tiles up, down, left and right to tiles we want to check
                            if (Main.tile[coords.X + 1, coords.Y].WallType == clickedTile.WallType && !tileColl.ContainsCoord(new Point(coords.X + 1, coords.Y)) && !queue.Contains(new Point(coords.X + 1, coords.Y)))
                            {
                                queue.Enqueue(new Point(coords.X + 1, coords.Y));
                            }
                            if (Main.tile[coords.X - 1, coords.Y].WallType == clickedTile.WallType && !tileColl.ContainsCoord(new Point(coords.X - 1, coords.Y)) && !queue.Contains(new Point(coords.X - 1, coords.Y)))
                            {
                                queue.Enqueue(new Point(coords.X - 1, coords.Y));
                            }
                            if (Main.tile[coords.X, coords.Y + 1].WallType == clickedTile.WallType && !tileColl.ContainsCoord(new Point(coords.X, coords.Y + 1)) && !queue.Contains(new Point(coords.X, coords.Y + 1)))
                            {
                                queue.Enqueue(new Point(coords.X, coords.Y + 1));
                            }
                            if (Main.tile[coords.X, coords.Y - 1].WallType == clickedTile.WallType && !tileColl.ContainsCoord(new Point(coords.X, coords.Y - 1)) && !queue.Contains(new Point(coords.X, coords.Y - 1)))
                            {
                                queue.Enqueue(new Point(coords.X, coords.Y - 1));
                            }
                        }
                        else
                        {
                            // if the tile type matches the tiletype of the tile we clicked at then that means we want to add it
                            if (Main.tile[coords.X, coords.Y].TileType == clickedTile.TileType && Main.tile[coords.X, coords.Y].HasTile == clickedTile.HasTile && (((EditorSystem.Local.CurrentSelection?.ContainsCoord(coords)) ?? false) || EditorSystem.Local.CurrentSelection?.Count == 0))
                            {
                                // if we dont already have it added, add it
                                if (tileColl.TryAddTile(coords, new TileCopy(Main.tile[coords.X, coords.Y])))
                                {
                                    count++;
                                }

                                // add the tiles up, down, left and right to tiles we want to check
                                if (Main.tile[coords.X + 1, coords.Y].TileType == clickedTile.TileType && !tileColl.ContainsCoord(new Point(coords.X + 1, coords.Y)) && !queue.Contains(new Point(coords.X + 1, coords.Y)))
                                {
                                    queue.Enqueue(new Point(coords.X + 1, coords.Y));
                                }
                                if (Main.tile[coords.X - 1, coords.Y].TileType == clickedTile.TileType && !tileColl.ContainsCoord(new Point(coords.X - 1, coords.Y)) && !queue.Contains(new Point(coords.X - 1, coords.Y)))
                                {
                                    queue.Enqueue(new Point(coords.X - 1, coords.Y));
                                }
                                if (Main.tile[coords.X, coords.Y + 1].TileType == clickedTile.TileType && !tileColl.ContainsCoord(new Point(coords.X, coords.Y + 1)) && !queue.Contains(new Point(coords.X, coords.Y + 1)))
                                {
                                    queue.Enqueue(new Point(coords.X, coords.Y + 1));
                                }
                                if (Main.tile[coords.X, coords.Y - 1].TileType == clickedTile.TileType && !tileColl.ContainsCoord(new Point(coords.X, coords.Y - 1)) && !queue.Contains(new Point(coords.X, coords.Y - 1)))
                                {
                                    queue.Enqueue(new Point(coords.X, coords.Y - 1));
                                }
                            }
                        }
                    }

                    // if we didnt hit the cap, fill all the tiles with the selected tile
                    if (count < _tileCap)
                    {
                        // go over all the tile we want to change
                        foreach (var tile in tileColl.AsDictionary())
                        {
                            int x = tile.Key.X;
                            int y = tile.Key.Y;

                            Main.tile[tile.Key.X, tile.Key.Y].CopyFrom(EditorSystem.Local.SelectedTile.GetAsTile());

                            // update tiles
                            if (TIGWEUISystem.Settings.ShouldUpdateDrawnTiles)
                            {
                                // squareframe but with noBreak
                                // update tiles
                                bool isTileFrameImportant = Main.tileFrameImportant[EditorSystem.Local.SelectedTile.TileType];
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

                        // add the information about the tiles we changed to our undo history so we can undo later
                        EditorSystem.Local.UndoHistory.Add(tileColl);
                    }
                    else
                    {
                        //Main.NewText($"[c/D95763:({TerrariaInGameWorldEditor.MODNAME})] Area too big to fill. Current cap is: {_tileCap}");
                    }
                }
                catch (Exception e)
                {
                    //Main.NewText($"[c/D95763:({TerrariaInGameWorldEditor.MODNAME})] Unknown error, area is probably too big to fill. Current cap is: {_tileCap}");
                }
            }
        }
    }
}