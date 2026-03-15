using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class MagicWandTool : SelectionTool
    {
        private int _tileCap = 10000;
        private TIGWENumberField _tileCapField;
        private TIGWECheckBox _selectCornersCheckBox;

        public MagicWandTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/MagicWandTool"));
            ToggleToolButton.HoverText = "Magic Wand";

            // settings
            // tile cap
            _tileCapField = new TIGWENumberField(_tileCap, minValue: 1);
            _tileCapField.OnValueChanged += (int newValue) =>
            {
                _tileCap = _tileCapField.GetValue();
            };
            _tileCapField.Width.Set(120, 0);
            _tileCapField.Height.Set(26, 0);
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
                // if ctrl is pressed down, keep adding tiles to our selection
                if (!PlayerInput.GetPressedKeys().Contains(Keys.LeftControl)) 
                {
                    _selection.Clear();
                }

                Point16 point = new Point16(Player.tileTargetX, Player.tileTargetY);
                TileCopy clickedTile = new TileCopy(Main.tile[point.X, point.Y]);
                Dictionary<Point16, TileCopy> tilesToAdd = new Dictionary<Point16, TileCopy>();
                
                int count = 0;
                TileCollection undoColl = new TileCollection();

                Queue<Point16> queue = new Queue<Point16>();
                queue.Enqueue(new Point16(point.X, point.Y));

                bool IsMatch(Point16 coords)
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
                    Point16 coords = queue.Dequeue();

                    if (IsMatch(coords))
                    {
                        // if we dont already have it added, add it
                        if (undoColl.TryAddTile(coords, new TileCopy(Main.tile[coords.X, coords.Y])))
                        {
                            tilesToAdd.TryAdd(coords, new TileCopy(Main.tile[coords.X, coords.Y]));
                            count++;
                        }

                        // tiles to check
                        List<Point16> directions = [
                            new Point16(coords.X + 1, coords.Y),
                            new Point16(coords.X - 1, coords.Y),
                            new Point16(coords.X, coords.Y + 1),
                            new Point16(coords.X, coords.Y - 1)
                        ];
                        if (_selectCornersCheckBox.IsChecked)
                        {
                            directions.AddRange(new List<Point16>
                            {
                                new Point16(coords.X + 1, coords.Y + 1),
                                new Point16(coords.X - 1, coords.Y + 1),
                                new Point16(coords.X - 1, coords.Y - 1),
                                new Point16(coords.X + 1, coords.Y - 1)
                            });
                        }

                        foreach (Point16 direction in directions)
                        {
                            if (!undoColl.ContainsCoord(direction) && !queue.Contains(direction))
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
                    TerrariaInGameWorldEditor.NewText($"Area too big to select.");
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
