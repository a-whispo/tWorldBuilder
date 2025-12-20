using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;
using TerrariaInGameWorldEditor.UI.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UI.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class MagicWandTool : SelectionTool
    {
        private int _tileCap = 10000;
        private TIGWENumberField _tileCapField;
        private TIGWECheckBox _selectCornersCheckBox;

        public MagicWandTool() : base("TerrariaInGameWorldEditor/UI/UIImages/MagicWandTool", "Magic Wand")
        {
            // settings
            // tile cap
            _tileCapField = new TIGWENumberField(_tileCap, minValue: 1);
            _tileCapField.OnValueChanged += (int newValue) =>
            {
                _tileCap = _tileCapField.GetValue();
            };
            _tileCapField.Width.Set(120, 0);
            _tileCapField.Height.Set(26, 0);
            _tileCapField.ShowButtons = true;
            Settings.Add(("Select tile cap:", _tileCapField));

            // fill tiles connected at corners
            _selectCornersCheckBox = new TIGWECheckBox(false);
            Settings.Add(("Select tiles connected at corners:", _selectCornersCheckBox));
        }

        public override string GetInfoText()
        {
            return $"[c/EAD87A:Count:] {_selection.Count}";
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
                    _selection.Clear();
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

                bool IsMatch(Point coords)
                {
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
                            tilesToAdd.TryAdd(coords, new TileCopy(Main.tile[coords.X, coords.Y]));
                            count++;
                        }

                        // tiles to check
                        List<Point> directions = [
                            new Point(coords.X + 1, coords.Y),
                            new Point(coords.X - 1, coords.Y),
                            new Point(coords.X, coords.Y + 1),
                            new Point(coords.X, coords.Y - 1)
                        ];
                        if (_selectCornersCheckBox.IsChecked)
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

                // if we reach the tile cap just clear the array so we dont draw anything
                if (count <= _tileCap)
                {
                    _selection.TryAddTiles(tilesToAdd);
                } else
                {
                    tilesToAdd.Clear(); // just remove the tiles we wanted to add
                    // Main.NewText($"[c/D95763:({TerrariaInGameWorldEditor.MODNAME})] Area too big to select. Current cap is: {tileCap}");
                }
            }

            // right click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                // undo selection
                _selection.Clear();
            }
        }
    }
}
