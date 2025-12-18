using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;
using TerrariaInGameWorldEditor.UI.UIElements.DropDown;
using TerrariaInGameWorldEditor.UI.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class BrushTool : Tool
    {
        private Dictionary<Point, TileCopy> _currentDrawPreTilesPlaced = new Dictionary<Point, TileCopy>(); // the state of the tiles before we draw on them (for undo)
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
        private TIGWEDropDown _modeDropDown;

        public BrushTool() : base("TerrariaInGameWorldEditor/UI/UIImages/BrushTool", "Brush")
        {
            // settings
            // mode
            _modeDropDown = new TIGWEDropDown(["Selected Tile", "Clipboard"]);
            _modeDropDown.ShowDropDownButton = true;
            _modeDropDown.SetDefaultOption("Selected Tile");
            _modeDropDown.OnOptionChanged += (string optionText) =>
            {
                if (optionText.Equals("Selected Tile"))
                {
                    // reset reference of clipboard
                    _brush = new TileCollection();
                    _mode = BrushMode.SelectedTile;
                }
                else if (optionText.Equals("Clipboard"))
                {
                    // set to reference of clipboard
                    _brush = EditorSystem.Local.Clipboard;
                    _mode = BrushMode.Clipboard;
                }
            };
            _modeDropDown.Height.Set(26, 0f);
            _modeDropDown.Width.Set(140, 0f);
            Settings.Add(("Mode:", _modeDropDown));

            // size
            _sizeField = new TIGWENumberField(4, 200, 1);
            _d = 4;
            _sizeField.OnValueChanged += (int newValue) =>
            {
                _d = _sizeField.GetValue();
            };
            _sizeField.Width.Set(60, 0);
            _sizeField.Height.Set(26, 0);
            _sizeField.ShowButtons = true;
            Settings.Add(("Size:", _sizeField));
        }

        public override void Update()
        {
            // update brush if in selected tile mode and either size changed or selected tile changed
            if (_mode == BrushMode.SelectedTile && (_d != _brush.GetWidth() + 1 || _brush.AsDictionary().ToList()[0].Value != EditorSystem.Local.SelectedTile))
            {
                _brush.Clear();
                _brush.TryAddTiles(ToolUtils.GetEllipseFilledTileCollection(_d, _d, EditorSystem.Local.SelectedTile).AsDictionary());
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // get width and height of brush
            int width = (int)Math.Floor(_brush.GetWidth() / 2f);
            int height = (int)Math.Floor(_brush.GetHeight() / 2f);
            Point point = new Point(Player.tileTargetX - width, Player.tileTargetY - height);
            DrawUtils.DrawTileCollection(_brush, point);
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
                    TileCollection tileColl = new TileCollection();
                    tileColl.TryAddTiles(_currentDrawPreTilesPlaced);
                    EditorSystem.Local.UndoHistory.Add(tileColl);
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

                // get brush as list for easier iteration
                var brushList = _brush.ToNormalized().AsDictionary().ToList();

                foreach (Point point in pointsToDrawAt) {

                    // go over all the tiles and set coordinates
                    foreach (var tile in brushList)
                    {
                        int x = tile.Key.X + point.X - width;
                        int y = tile.Key.Y + point.Y - height;

                        _currentDrawPreTilesPlaced.TryAdd(new Point(x, y), new TileCopy(Main.tile[x, y]));

                        // we also need to add the tiles around it since those also get affected if we have update tiles on
                        if (TIGWESettings.ShouldUpdateDrawnTiles)
                        {
                            _currentDrawPreTilesPlaced.TryAdd(new Point(x + 1, y), new TileCopy(Main.tile[x + 1, y]));
                            _currentDrawPreTilesPlaced.TryAdd(new Point(x - 1, y), new TileCopy(Main.tile[x - 1, y]));
                            _currentDrawPreTilesPlaced.TryAdd(new Point(x, y + 1), new TileCopy(Main.tile[x, y + 1]));
                            _currentDrawPreTilesPlaced.TryAdd(new Point(x, y - 1), new TileCopy(Main.tile[x, y - 1]));
                        }
                    }

                    // paste
                    ToolUtils.Paste(_brush, new Point(point.X - width, point.Y - height), false, TIGWESettings.ShouldUpdateDrawnTiles);
                }
            }

            // change brush size with mouse wheel
            if (Keybinds.Key1MK.Current || Keybinds.Key1MK.GetAssignedKeys().Count < 1)
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
