using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.Editor.Windows.Settings;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.DropDown;
using TerrariaInGameWorldEditor.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class BrushTool : Tool
    {
        private Dictionary<Point16, TileCopy> _currentDrawPreTilesPlaced = new Dictionary<Point16, TileCopy>(); // the state of the tiles before we draw on them (for undo)
        private List<Point> _currentDrawLinePoints = new List<Point>(); // list of all the points the cursor passed over when we're drawing (used to make lines complete without gaps caused by 60 fps)
        private TileCollection _brush = new TileCollection(); // the brush itself, a collection of tiles in the shape of an ellipse
        private int _d; // diameter of the brush
        private enum BrushMode
        {
            SelectedTile,
            Clipboard
        }
        private BrushMode _mode;
        private TIGWENumberField _sizeField;
        private TIGWEDropDown<BrushMode> _modeDropDown;

        public BrushTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/BrushTool"));
            ToggleToolButton.HoverText = "Brush";

            // settings
            // mode
            _modeDropDown = new TIGWEDropDown<BrushMode>();
            _modeDropDown.AddOption(BrushMode.SelectedTile, "Selected Tile");
            _modeDropDown.AddOption(BrushMode.Clipboard, "Clipboard");
            _modeDropDown.OnOptionChanged += (option) =>
            {
                _mode = option.Value;
                UpdateBrush();
            };
            _modeDropDown.Height.Set(26, 0f);
            _modeDropDown.Width.Set(140, 0f);
            Settings.Add(("Mode:", _modeDropDown));

            // size
            _sizeField = new TIGWENumberField(4, 200, 1);
            _d = 4;
            _sizeField.OnValueChanged += (int newValue) =>
            {
                _d = newValue;
                UpdateBrush();
            };
            _sizeField.Width.Set(60, 0);
            _sizeField.Height.Set(26, 0);
            Settings.Add(("Size:", _sizeField));

            // when to update brush
            EditorSystem.Local.OnSelectedTileChanged += (_, _) =>
            {
                UpdateBrush();
            };
            EditorSystem.Local.OnClipboardChanged += (_, _) =>
            {
                UpdateBrush();
            };

            // make sure brush is set at the start
            UpdateBrush();
        }

        protected virtual void UpdateBrush()
        {
            switch(_mode)
            {
                case BrushMode.SelectedTile:
                    if (_brush == EditorSystem.Local.Clipboard)
                    {
                        _brush = new TileCollection(); // make sure to reset brush if its a reference to clipboard
                    }
                    _brush.Clear();
                    _brush.TryAddTiles(ToolUtils.GetEllipseFilledTileCollection(_d, _d, EditorSystem.Local.SelectedTile));
                    break;

                case BrushMode.Clipboard:
                    _brush = EditorSystem.Local.Clipboard;
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // get width and height of brush
            int width = (int)Math.Floor(_brush.GetWidth() / 2f);
            int height = (int)Math.Floor(_brush.GetHeight() / 2f);
            Point point = new Point(Player.tileTargetX - width, Player.tileTargetY - height);
            DrawUtils.DrawTileCollection(_brush, point, TIGWESettings.ShouldPasteTiles, TIGWESettings.ShouldPasteWalls, TIGWESettings.ShouldPasteLiquid, TIGWESettings.ShouldPasteWires);
            DrawUtils.DrawTileCollectionOutline(_brush, point, TIGWESettings.ToolColor);
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;

            // on release (stop drawing)
            if (Main.mouseLeftRelease)
            {
                // if we placed tiles
                if (_currentDrawPreTilesPlaced.Count > 0)
                {
                    // create a tile collection with all the affected tiles and add it to undo history
                    TileCollection undoColl = new TileCollection();
                    undoColl.TryAddTiles(_currentDrawPreTilesPlaced);
                    EditorSystem.Local.UndoHistory.Add(undoColl);
                    _currentDrawPreTilesPlaced.Clear();
                    _currentDrawLinePoints.Clear();
                }
            }

            // on hold (draw) and not hovering over UI
            if (Main.mouseLeft && !Main.LocalPlayer.mouseInterface && _brush.Count > 0)
            {
                List<Point> pointsToDrawAt = new List<Point>();
                pointsToDrawAt.Add(new Point(Player.tileTargetX, Player.tileTargetY));
                _currentDrawLinePoints.Add(new Point(Player.tileTargetX, Player.tileTargetY));
                if (_currentDrawLinePoints.Count > 1)
                {
                    // get the points in a line between the last point and the current point to avoid gaps
                    pointsToDrawAt.AddRange(ToolUtils.CalculatePointsInLine(_currentDrawLinePoints[_currentDrawLinePoints.Count - 2], _currentDrawLinePoints[_currentDrawLinePoints.Count - 1]));
                }

                // get width and height of brush
                int width = (int)Math.Floor(_brush.GetWidth() / 2f);
                int height = (int)Math.Floor(_brush.GetHeight() / 2f);

                foreach (Point point in pointsToDrawAt) {

                    // go over all the tiles and set coordinates
                    foreach (var tile in _brush.ToNormalized())
                    {
                        int x = tile.Key.X + point.X - width;
                        int y = tile.Key.Y + point.Y - height;

                        _currentDrawPreTilesPlaced.TryAdd(new Point16(x, y), new TileCopy(Main.tile[x, y]));

                        // we also need to add the tiles around it since those also get affected if we have update tiles on
                        if (TIGWESettings.ShouldUpdateDrawnTiles)
                        {
                            _currentDrawPreTilesPlaced.TryAdd(new Point16(x + 1, y), new TileCopy(Main.tile[x + 1, y]));
                            _currentDrawPreTilesPlaced.TryAdd(new Point16(x - 1, y), new TileCopy(Main.tile[x - 1, y]));
                            _currentDrawPreTilesPlaced.TryAdd(new Point16(x, y + 1), new TileCopy(Main.tile[x, y + 1]));
                            _currentDrawPreTilesPlaced.TryAdd(new Point16(x, y - 1), new TileCopy(Main.tile[x, y - 1]));
                        }
                    }

                    // paste
                    ToolUtils.Paste(_brush, new Point(point.X - width, point.Y - height), false, TIGWESettings.ShouldUpdateDrawnTiles);
                }
            }

            // change brush size with mouse wheel
            if (PlayerInput.GetPressedKeys().Contains(Keys.LeftControl))
            {
                PlayerInput.LockVanillaMouseScroll($"{TerrariaInGameWorldEditor.MODNAME}/Brush");
                if (PlayerInput.ScrollWheelDelta > 0)
                {
                    _d += (PlayerInput.GetPressedKeys().Contains(Keys.LeftShift) ? 10 : 1);
                }
                if (PlayerInput.ScrollWheelDelta < 0)
                {
                    if (_d >= 2)
                    {
                        _d -= (PlayerInput.GetPressedKeys().Contains(Keys.LeftShift) ? 10 : 1);
                    }
                }
                if (PlayerInput.ScrollWheelDelta != 0)
                {
                    _d = Math.Max(_d, 1);
                    _sizeField.SetValue(_d);
                }
            }
        }
    }
}
