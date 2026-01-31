using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;
using TerrariaInGameWorldEditor.UI.UIElements.Button;
using TerrariaInGameWorldEditor.UI.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UI.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class PaintBucketTool : Tool
    {
        private int _tileCap = 10000;
        private TIGWENumberField _tileCapField;
        private TIGWECheckBox _fillCornersCheckBox;

        public PaintBucketTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/PaintBucketTool"));
            ToggleToolButton.HoverText = "Paint Bucket";

            // settings
            // tile cap
            _tileCapField = new TIGWENumberField(_tileCap, minValue: 1);
            _tileCapField.OnValueChanged += (int newValue) =>
            {
                _tileCap = _tileCapField.GetValue();
            };
            _tileCapField.Width.Set(120, 0);
            _tileCapField.Height.Set(26, 0);
            Settings.Add(("Fill tile cap:", _tileCapField));

            // fill tiles connected at corners
            _fillCornersCheckBox = new TIGWECheckBox(false);
            Settings.Add(("Fill tiles connected at corners:", _fillCornersCheckBox));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Point point = new Point(Player.tileTargetX, Player.tileTargetY);
            TileCollection tc = new TileCollection();
            tc.TryAddTile(point, new TileCopy(Main.tile[point.X, point.Y]));
            DrawUtils.DrawTileCollectionOutline(tc, point, TIGWESettings.ToolColor);
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
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

                bool IsMatch(Point coords)
                {
                    bool inSelection = ((EditorSystem.Local.CurrentSelection?.ContainsCoord(coords)) ?? false) || EditorSystem.Local.CurrentSelection?.Count == 0;
                    if (!inSelection)
                    {
                        return false;
                    }

                    Tile tile = Main.tile[coords.X, coords.Y];
                    // if we clicked a wall and the tile matches
                    if (tile.WallType == clickedTile.WallType && !tile.HasTile && !clickedTile.HasTile)
                    {
                        return true;
                    }
                    // if we clicked a tile and the tile matches
                    if (tile.TileType == clickedTile.TileType && tile.HasTile && clickedTile.HasTile)
                    {
                        return true;
                    }
                    return false;
                }

                // go until we hit the tilecap or cant find any more tiles that we want to replace
                while (queue.Count > 0 && count <= _tileCap)
                {
                    // get the coordinates at the tile in the first index
                    Point coords = queue.Dequeue();

                    if (IsMatch(coords))
                    {
                        // if we dont already have it added, add it
                        if (tileColl.TryAddTile(coords, new TileCopy(Main.tile[coords.X, coords.Y])))
                        {
                            count++;
                        }

                        // tiles to check
                        List<Point> directions = [
                            new Point(coords.X + 1, coords.Y),
                            new Point(coords.X - 1, coords.Y),
                            new Point(coords.X, coords.Y + 1),
                            new Point(coords.X, coords.Y - 1)
                        ];
                        if (_fillCornersCheckBox.IsChecked)
                        {
                            directions.AddRange(new List<Point>
                            {
                                new Point(coords.X + 1, coords.Y + 1),
                                new Point(coords.X - 1, coords.Y + 1),
                                new Point(coords.X - 1, coords.Y - 1),
                                new Point(coords.X + 1, coords.Y - 1)
                            });
                        }

                        foreach (Point direction in directions)
                        {
                            if (!tileColl.ContainsCoord(direction) && !queue.Contains(direction))
                            {
                                queue.Enqueue(direction);
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
                        if (TIGWESettings.ShouldUpdateDrawnTiles)
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
        }
    }
}