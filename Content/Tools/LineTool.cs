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
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.DropDown;
using TerrariaInGameWorldEditor.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class LineTool : Tool
    {
        // points
        private Point16 _point1;
        private bool _point1placed = false;
        private Point16 _point2;
        private bool _point2placed = false;

        private TileCollection _brush = new TileCollection();
        private TileCollection _cachedTilesInLine = new TileCollection();
        private TileCollection _tilesToDraw = new TileCollection();
        private int _d = 4;
        private int _yDiff = 0;
        private int _xDiff = 0;
        private enum LineMode
        {
            SelectedTile,
            Clipboard
        }
        private LineMode _mode;
        private TIGWENumberField _sizeField;
        private TIGWEDropDown<LineMode> _modeDropDown;
        
        public LineTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/LineTool"));
            ToggleToolButton.HoverText = "Line \n[c/EAD87A:Ctrl + Scroll:] Change size by 1 \n[c/EAD87A:Ctrl + Shift + Scroll:] Change size by 10";

            // settings
            // mode
            _modeDropDown = new TIGWEDropDown<LineMode>();
            _modeDropDown.AddOption(LineMode.SelectedTile, "Selected Tile");
            _modeDropDown.AddOption(LineMode.Clipboard, "Clipboard");
            _modeDropDown.OnOptionChanged += (option) =>
            {
                _mode = option.Value;
                UpdateBrush();
            };
            _modeDropDown.Height.Set(26, 0f);
            _modeDropDown.Width.Set(140, 0f);
            Settings.Add(("Mode:", _modeDropDown));

            // size
            _sizeField = new TIGWENumberField(4, 100, 1);
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
                    _brush.TryAddTiles(ToolUtils.GetEllipseFilledTileCollection(_d, _d, EditorSystem.Local.SelectedTile));
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
                _point1 = new Point16(Player.tileTargetX, Player.tileTargetY);
            }
            // have the mouse act as point 2 while point 2 isnt placed
            if (!_point2placed)
            {
                _point2 = new Point16(Player.tileTargetX, Player.tileTargetY);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // update line
            List<Point16> pointsToDrawAt = ToolUtils.CalculatePointsInLine(_point1, _point2);
            _yDiff = _point2.Y - _point1.Y;
            _xDiff = _point2.X - _point1.X;
            _tilesToDraw.Clear();
            int radius = (int)Math.Floor(_brush.GetWidth() / 2f);
            int spacing = (int)Math.Max(1, _brush.GetWidth() * 0.1); // dont bother putting the brush at every point when it gets bigger
            int count = 0;
            foreach (Point16 point in pointsToDrawAt)
            {
                count++;
                if (count != spacing && (point != _point1 && point != _point2))
                {
                    continue;
                }
                count = 0;
                if (point.X < (Main.screenPosition.X / 16) - radius || point.X > (Main.screenPosition.X / 16 + radius + Main.screenWidth / 16))
                {
                    continue;
                }
                if (point.Y < (Main.screenPosition.Y / 16) - radius || point.Y > (Main.screenPosition.Y / 16 + radius + Main.screenHeight / 16))
                {
                    continue;
                }

                // go over all the tiles and set coordinates
                foreach (var tile in _brush.ToNormalized())
                {
                    int x = tile.Key.X + point.X - radius;
                    int y = tile.Key.Y + point.Y - radius;
                    _tilesToDraw.TryAddTile(new Point16(x, y), tile.Value);
                }
            }

            // draw
            Point coord = new Point(_tilesToDraw.GetMinX(), _tilesToDraw.GetMinY());
            DrawUtils.DrawTileCollection(_tilesToDraw, coord);
            DrawUtils.DrawTileCollectionOutline(_tilesToDraw, coord, EditorSystem.Local.Settings.ToolColor);
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;

            // left click
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
                if (!_point1placed || (_point1placed && _point2placed)) // if both points have aleady been placed, reset them and place point 1 again
                {
                    _point1 = new Point16(Player.tileTargetX, Player.tileTargetY); // get mouse coordinates in the world and save them to point1
                    _point2placed = false;
                    _point1placed = true;
                }
                else
                {
                    if (!_point2placed && _brush.Count > 0)
                    {
                        // update line and paste
                        _point2placed = true; // setting this to true makes the line algo calculate the full line
                        List<Point16> pointsInLine = ToolUtils.CalculatePointsInLine(_point1, new Point16(Player.tileTargetX, Player.tileTargetY));
                        int spacing = (int)Math.Max(1, _brush.GetWidth() * 0.1); // dont bother putting the brush at every point when it gets bigger
                        int count = 0;
                        foreach (Point16 point in pointsInLine)
                        {
                            count++;
                            if (count != spacing && (point != _point1 && point != _point2))
                            {
                                continue;
                            }
                            count = 0;

                            // go over all the tiles and set coordinates
                            foreach (var tile in _brush.ToNormalized())
                            {
                                int x = tile.Key.X + point.X;
                                int y = tile.Key.Y + point.Y;
                                _cachedTilesInLine.TryAddTile(new Point16(x, y), tile.Value);
                            }
                        }
                        ToolUtils.Paste(_cachedTilesInLine, new Point16(_cachedTilesInLine.GetMinX() - _brush.GetWidth() / 2, _cachedTilesInLine.GetMinY() - _brush.GetHeight() / 2), true, EditorSystem.Local.Settings.ShouldUpdateDrawnTiles);
                        _cachedTilesInLine.Clear();
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
                    _cachedTilesInLine.Clear();
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
                    _d = Math.Clamp(_d, 1, 100);
                    _sizeField.SetValue(_d);
                }
            }
        }
    }
}
