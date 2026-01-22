using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;
using TerrariaInGameWorldEditor.UI.UIElements.Button;
using TerrariaInGameWorldEditor.UI.UIElements.DropDown;
using TerrariaInGameWorldEditor.UI.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class LineTool : Tool
    {
        // points
        private Point _point1;
        private bool _point1placed = false;
        private Point _point2;
        private bool _point2placed = false;

        private TileCollection _brush = new TileCollection();
        private TileCollection _tilesInLine = new TileCollection();
        private int _d = 4;
        private int _length = 0;
        private int _yDiff = 0;
        private int _xDiff = 0;
        private enum LineMode
        {
            SelectedTile,
            Clipboard
        }
        private LineMode _mode;
        private TIGWENumberField _sizeField;
        private TIGWEDropDown _modeDropDown;

        public LineTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/LineTool"));
            ToggleToolButton.HoverText = "Line";

            // settings
            // mode
            _modeDropDown = new TIGWEDropDown(["Selected Tile", "Clipboard"]);
            _modeDropDown.ShowDropDownButton = true;
            _modeDropDown.SetDefaultOption("Selected Tile");
            _modeDropDown.OnOptionChanged += (string optionText) =>
            {
                switch (optionText)
                {
                    case "Selected Tile":
                        _mode = LineMode.SelectedTile;
                        break;

                    case "Clipboard":
                        _mode = LineMode.Clipboard;
                        break;
                }
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
            _sizeField.ShowButtons = true;
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
        }

        public override string GetInfoText()
        {
            return $"[c/EAD87A:Y Difference:] {_yDiff}, [c/EAD87A:X Difference:] {_xDiff}";
        }

        private void UpdateBrush()
        {
            switch (_mode)
            {
                case LineMode.SelectedTile:
                    if (_brush == EditorSystem.Local.Clipboard)
                    {
                        _brush = new TileCollection(); // make sure to reset brush if its a reference to clipboard
                    }
                    _brush.Clear();
                    _brush.TryAddTiles(ToolUtils.GetEllipseFilledTileCollection(_d, _d, EditorSystem.Local.SelectedTile).AsDictionary());
                    break;

                case LineMode.Clipboard:
                    _brush = EditorSystem.Local.Clipboard;
                    break;
            }
        }

        public override void Update()
        {
            // have the mouse act as point 1 while point 1 isnt placed
            if (!_point1placed)
            {
                _point1 = new Point(Player.tileTargetX, Player.tileTargetY);
            }
            // have the mouse act as point 2 while point 2 isnt placed
            if (!_point2placed)
            {
                _point2 = new Point(Player.tileTargetX, Player.tileTargetY);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // update line
            _tilesInLine = CalculateTilesInLine(_point1, _point2, _brush.ToNormalized());
            Point point = new Point(_tilesInLine.GetMinX() - _brush.GetWidth() / 2, _tilesInLine.GetMinY() - _brush.GetHeight() / 2);
            DrawUtils.DrawTileCollection(_tilesInLine, point);
            DrawUtils.DrawTileCollectionOutline(_tilesInLine, point, TIGWESettings.ToolColor);
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;
            // left click
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
                if (!_point1placed || (_point1placed && _point2placed)) // if both points have aleady been placed, reset them and place point 1 again
                {
                    _point1 = new Point(Player.tileTargetX, Player.tileTargetY); // get mouse coordinates in the world and save them to point1
                    _point2placed = false;
                    _point1placed = true;
                }
                else
                {
                    if (!_point2placed && _brush.Count > 0)
                    {
                        // update line and paste
                        _point2placed = true; // setting this to true makes the line algo calculate the full line
                        _tilesInLine = CalculateTilesInLine(_point1, new Point(Player.tileTargetX, Player.tileTargetY), _brush.ToNormalized());
                        ToolUtils.Paste(_tilesInLine, new Point(_tilesInLine.GetMinX() - _brush.GetWidth() / 2, _tilesInLine.GetMinY() - _brush.GetHeight() / 2), true, TIGWESettings.ShouldUpdateDrawnTiles);
                        _tilesInLine.Clear();
                        _point1placed = false;
                        _point2placed = false;
                    }
                }
            }

            // right click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                if (_point1placed)
                {
                    // unplace points
                    _point1placed = false;

                    // clear tiles in line
                    _tilesInLine.Clear();
                }
            }
            if (PlayerInput.GetPressedKeys().Contains(Keys.LeftControl))
            {
                PlayerInput.LockVanillaMouseScroll($"{TerrariaInGameWorldEditor.MODNAME}/Line");
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

        private TileCollection CalculateTilesInLine(Point origin, Point endpoint, TileCollection brush)
        {
            // algorithm from https://rosettacode.org/wiki/Bitmap/Bresenham%27s_line_algorithm#C#
            var brushList = brush.AsDictionary().ToList();
            TileCollection tc = new TileCollection();

            _length = 0;
            int x0 = origin.X;
            int y0 = origin.Y;
            int x1 = endpoint.X;
            int y1 = endpoint.Y;
            _yDiff = y0 - y1;
            _xDiff = x0 - x1;

            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            while (true)
            {
                _length++;
                // check if its worth doing calculations, if we havnt placed point 2 yet, if we have placed point 2 we obviously want to check where we should place all the tiles
                if (_point2placed || !((((x0 * 16) + 64) < (Main.screenPosition.X - 32) || ((x0 * 16) - 64) > (Main.screenPosition.X + Main.screenWidth) || ((y0 * 16) + 64) < (Main.screenPosition.Y) || ((y0 * 16) - 64) > (Main.screenPosition.Y + Main.screenHeight))))
                {
                    for (int i = 0; i < brushList.Count; i++)
                    {
                        tc.TryAddTile(new Point(x0 + brushList[i].Key.X, y0 + brushList[i].Key.Y), brushList[i].Value);
                    }
                }
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }
                e2 = err;
                if (e2 > -dx)
                {
                    err -= dy; x0 += sx;
                }
                if (e2 < dy)
                {
                    err += dx; y0 += sy;
                }
            }

            return tc;
        }
    }
}
