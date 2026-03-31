using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UIElements.DropDown;
using TerrariaInGameWorldEditor.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal abstract class FillTool : Tool
    {
        protected int _tileCap = 10000;
        protected TIGWENumberField _tileCapField;
        protected TIGWECheckBox _includeCornersCheckBox;
        protected enum Target
        {
            Auto,
            Tiles,
            Walls,
            Liquid
        }
        protected Target _mode = Target.Auto;
        protected TIGWEDropDown<Target> _targetDropDown;

        public FillTool()
        {
            // settings
            // tile cap
            _tileCapField = new TIGWENumberField(_tileCap, minValue: 1);
            _tileCapField.OnValueChanged += (newValue) => _tileCap = _tileCapField.GetValue();
            _tileCapField.Width.Set(120, 0);
            _tileCapField.Height.Set(26, 0);
            Settings.Add(("Tile cap:", _tileCapField));

            // fill tiles connected at corners
            _includeCornersCheckBox = new TIGWECheckBox(false);
            Settings.Add(("Include tiles connected at corners:", _includeCornersCheckBox));

            // target
            _targetDropDown = new TIGWEDropDown<Target>();
            _targetDropDown.AddOption(Target.Auto, "Auto");
            _targetDropDown.AddOption(Target.Tiles, "Tiles/Air");
            _targetDropDown.AddOption(Target.Walls, "Walls");
            _targetDropDown.AddOption(Target.Liquid, "Liquid");
            _targetDropDown.Height.Set(26, 0f);
            _targetDropDown.Width.Set(140, 0f);
            _targetDropDown.OnOptionChanged += (option) => _mode = option.Value;
            Settings.Add(("Target:", _targetDropDown));
        }

        protected virtual void OnFill(TileCollection tiles)
        {

        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;

            // left click
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
                Point16 point = new Point16(Player.tileTargetX, Player.tileTargetY);
                TileCopy clickedTile = new TileCopy(Main.tile[point.X, point.Y]);

                int count = 0;
                TileCollection tilesToAdd = new TileCollection();

                Queue<Point16> queue = new Queue<Point16>();
                queue.Enqueue(new Point16(point.X, point.Y));

                bool IsMatch(Point16 coords)
                {
                    Tile tile = Main.tile[coords.X, coords.Y];

                    switch (_mode)
                    {
                        case Target.Auto:
                            if (tile.HasTile || clickedTile.HasTile)
                            {
                                return tile.TileType == clickedTile.TileType && tile.HasTile == clickedTile.HasTile;
                            }
                            if (tile.WallType != WallID.None || clickedTile.WallType != WallID.None)
                            {
                                return tile.WallType != WallID.None && tile.WallType == clickedTile.WallType;
                            }
                            if (tile.LiquidAmount != 0 || clickedTile.LiquidAmount != 0)
                            {
                                return tile.LiquidAmount != 0 && tile.LiquidType == clickedTile.LiquidType;
                            }
                            return tile.TileType == clickedTile.TileType;

                        case Target.Tiles:
                            return tile.TileType == clickedTile.TileType && tile.HasTile == clickedTile.HasTile;

                        case Target.Walls:
                            if (tile.WallType != WallID.None || clickedTile.WallType != WallID.None)
                            {
                                return tile.WallType != WallID.None && tile.WallType == clickedTile.WallType;
                            }
                            return false;

                        case Target.Liquid:
                            if (tile.LiquidAmount != 0 || clickedTile.LiquidAmount != 0)
                            {
                                return tile.LiquidAmount != 0 && tile.LiquidType == clickedTile.LiquidType;
                            }
                            return false;
                    }
                    return false;
                }

                // go until we hit the tilecap or cant find any more tiles that we think match
                while (queue.Count > 0 && count <= _tileCap)
                {
                    Point16 coords = queue.Dequeue();

                    if (IsMatch(coords))
                    {
                        // if we dont already have it added, add it
                        if (tilesToAdd.TryAddTile(coords, new TileCopy(Main.tile[coords.X, coords.Y])))
                        {
                            count++;
                        }

                        // tiles to check
                        List<Point16> directions = [
                            new Point16(coords.X + 1, coords.Y),
                            new Point16(coords.X - 1, coords.Y),
                            new Point16(coords.X, coords.Y + 1),
                            new Point16(coords.X, coords.Y - 1)
                        ];
                        if (_includeCornersCheckBox.IsChecked)
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
                            if (!tilesToAdd.ContainsCoord(direction) && !queue.Contains(direction))
                            {
                                queue.Enqueue(direction);
                            }
                        }
                    }
                }
                if (count > _tileCap)
                {
                    tilesToAdd.Clear(); // just remove the tiles we wanted to add
                    TerrariaInGameWorldEditor.NewText($"Area too big to select.");
                }
                else
                {
                    OnFill(tilesToAdd);
                }
            }
        }
    }
}
