using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.TIGWEUI;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class MagicWandTool : SelectionTool
    {
        private Dictionary<Point, TileCopy> _tilesCloseToOutline = new Dictionary<Point, TileCopy>();
        private int _tileCap = 10000;

        public MagicWandTool() : base("TerrariaInGameWorldEditor/UI/UIImages/MagicWandTool", "Magic Wand")
        {
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw outline while we have a selection
            if (Selection.Count > 0)
            {
                DrawUtils.DrawTileCollectionOutline(Selection, new Point(Selection.GetMinX(), Selection.GetMinY()), TIGWEUISystem.Settings.ToolColor);
            }
        }

        public override void Update()
        {
            base.Update();
            InfoText = $"Count: {Selection.Count}";
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;
            // left click
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
                // flood fill kinda
                // will add some settings so you can choose to only fill tiles and/or walls and/or liquid if you want to

                if (!Keybinds.Key1MK.Current || Keybinds.Key1MK.GetAssignedKeys().Count < 1) // if key 1 is pressed down, keep adding tiles to our selection
                {
                    Selection.Clear();
                    _tilesCloseToOutline.Clear();
                }

                // get point and clicked tile
                Point point = new Point(Player.tileTargetX, Player.tileTargetY);
                TileCopy clickedTile = new TileCopy(Main.tile[point.X, point.Y]);

                // dictionary to store tiles we want to add
                Dictionary<Point, TileCopy> tilesToAdd = new Dictionary<Point, TileCopy>();

                // queue to store tiles we want check
                Queue<Point> queue = new Queue<Point>();
                queue.Enqueue(new Point(point.X, point.Y));

                // counter to see when we reach the tilecap
                int count = 0;

                // new action so we can undo this later if we want to
                TileCollection tileColl = new TileCollection();
                try
                {
                    // go until we hit the tilecap or cant find any more tiles that we want to select
                    while (queue.Count > 0 && count < _tileCap)
                    {
                        // get the coordinates at the tile in the first index
                        Point coords = queue.Dequeue();

                        // if we clicked a wall
                        if ((Main.tile[coords.X, coords.Y].WallType == clickedTile.WallType) && (!Main.tile[coords.X, coords.Y].HasTile && !clickedTile.HasTile))
                        {
                            // if we dont already have it added, add it
                            if (tileColl.TryAddTile(coords, new TileCopy(Main.tile[coords.X, coords.Y])))
                            {
                                tilesToAdd.TryAdd(coords, new TileCopy(Main.tile[coords.X, coords.Y]));
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
                            if (Main.tile[coords.X, coords.Y].TileType == clickedTile.TileType && Main.tile[coords.X, coords.Y].HasTile == clickedTile.HasTile)
                            {
                                // if we dont already have it added, add it
                                if (tileColl.TryAddTile(coords, new TileCopy(Main.tile[coords.X, coords.Y])))
                                {
                                    tilesToAdd.TryAdd(coords, new TileCopy(Main.tile[coords.X, coords.Y]));
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

                    // if we reach the tile cap just clear the array so we dont draw anything
                    if (count >= _tileCap)
                    {
                        tilesToAdd.Clear(); // just remove the tiles we wanted to add
                        // Main.NewText($"[c/D95763:({TerrariaInGameWorldEditor.MODNAME})] Area too big to select. Current cap is: {tileCap}");
                    }
                }
                catch (Exception ex)
                {
                    tilesToAdd.Clear(); // just remove the tiles we wanted to add
                    // Main.NewText($"[c/D95763:({TerrariaInGameWorldEditor.MODNAME})] Unknown error, area is probably too big to select. Current cap is: {tileCap}");
                }
                try
                {
                    Selection.TryAddTiles(tilesToAdd);
                }
                catch (Exception ex)
                {
                    // Main.NewText($"[c/D95763:({TerrariaInGameWorldEditor.MODNAME})] Selected tile is already in selection");
                }
            }

            // right click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                // undo selection
                Selection.Clear();
                _tilesCloseToOutline.Clear();
            }
        }
    }
}
