using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.Editor.Windows.Settings;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class PaintBucketTool : Tool
    {
        private int _tileCap = 10000;
        private TIGWENumberField _tileCapField;
        private TIGWECheckBox _fillCornersCheckBox;

        public PaintBucketTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/PaintBucketTool"));
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
            Point16 point = new Point16(Player.tileTargetX, Player.tileTargetY);
            TileCollection tc = new TileCollection();
            tc.TryAddTile(point, new TileCopy(Main.tile[point.X, point.Y]));
            DrawUtils.DrawTileCollectionOutline(tc, point.ToPoint(), TIGWESettings.ToolColor);
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
                Point16 point = new Point16(Player.tileTargetX, Player.tileTargetY);
                TileCopy clickedTile = new TileCopy(Main.tile[point.X, point.Y]);

                int count = 0;
                TileCollection tilesToFill = new TileCollection();

                Queue<Point16> queue = new Queue<Point16>();
                queue.Enqueue(new Point16(point.X, point.Y));

                bool IsMatch(Point16 coords)
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
                    Point16 coords = queue.Dequeue();

                    if (IsMatch(coords))
                    {
                        // if we dont already have it added, add it
                        if (tilesToFill.TryAddTile(coords, EditorSystem.Local.SelectedTile))
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
                        if (_fillCornersCheckBox.IsChecked)
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
                            if (!tilesToFill.ContainsCoord(direction) && !queue.Contains(direction))
                            {
                                queue.Enqueue(direction);
                            }
                        }
                    }
                }

                // if we didnt hit the cap, fill all the tiles with the selected tile
                if (count < _tileCap)
                {
                    ToolUtils.Paste(tilesToFill, new Point(tilesToFill.GetMinX(), tilesToFill.GetMinY()), true, TIGWESettings.ShouldUpdateDrawnTiles);
                }
                else
                {
                    TerrariaInGameWorldEditor.NewText($"Area too big to fill.");
                }
            }
        }
    }
}