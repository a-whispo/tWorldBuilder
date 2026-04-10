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
    internal class BrushTool : Tool
    {
        private TileCollection _brushStroke = new TileCollection(); // the state of the tiles before we draw on them (for undo)
        private List<Point16> _brushStrokePoints = new List<Point16>(); // list of all the points the cursor passed over when we're drawing (used to make lines complete without gaps caused by 60 fps)
        private TileCollection _tilesToPaste = new TileCollection();
        private int _pasteCounter = 0;
        protected TileCollection _brush = new TileCollection(); // the brush itself, a collection of tiles in the shape of an ellipse
        protected int _d; // diameter of the brush
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
            DrawUtils.DrawTileCollection(_brush, point, EditorSystem.Local.Settings.ShouldPasteTiles, EditorSystem.Local.Settings.ShouldPasteWalls, EditorSystem.Local.Settings.ShouldPasteLiquid, EditorSystem.Local.Settings.ShouldPasteWires);
            DrawUtils.DrawTileCollectionOutline(_brush, point, EditorSystem.Local.Settings.ToolColor);
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;

            // on release (stop drawing)
            if (Main.mouseLeftRelease)
            {
                // if we placed tiles
                if (_brushStroke.Count > 0)
                {
                    TileCollection undoColl = new TileCollection();
                    undoColl.TryAddTiles(_brushStroke);
                    EditorSystem.Local.AddToUndoHistory(undoColl);
                    _brushStroke.Clear();
                    _brushStrokePoints.Clear();
                    _tilesToPaste.Clear();
                }
            }

            // on hold (draw) and not hovering over UI
            if (Main.mouseLeft && !Main.LocalPlayer.mouseInterface && _brush.Count > 0)
            {
                int halfWidth = (int)Math.Floor(_brush.GetWidth() / 2f);
                int halfHeight = (int)Math.Floor(_brush.GetHeight() / 2f);
                _brushStrokePoints.Add(new Point16(Player.tileTargetX, Player.tileTargetY));
                List<Point16> pointsToDrawAt = [ new Point16(Player.tileTargetX, Player.tileTargetY) ];
                if (_brushStrokePoints.Count >= 2)
                {
                    pointsToDrawAt = ToolUtils.CalculatePointsInLine(_brushStrokePoints[_brushStrokePoints.Count - 1], _brushStrokePoints[_brushStrokePoints.Count - 2]);
                }

                int spacing = (int)Math.Max(1, _brush.GetWidth() * 0.1); // dont bother putting the brush at every point when it gets bigger
                int count = 0;
                foreach (Point16 point in pointsToDrawAt)
                {
                    count++;
                    if (count != spacing && (point != pointsToDrawAt[0] && point != pointsToDrawAt[pointsToDrawAt.Count - 1]))
                    {
                        continue;
                    }
                    count = 0;

                    // go over all the tiles and set coordinates
                    foreach (var tile in _brush.ToNormalized())
                    {
                        int x = tile.Key.X + point.X - halfWidth;
                        int y = tile.Key.Y + point.Y - halfHeight;
                        if (_tilesToPaste.ContainsCoord(new Point16(x, y)))
                        {
                            continue;
                        }

                        _tilesToPaste.TryAddTile(new Point16(x, y), tile.Value);
                        _brushStroke.TryAddTile(new Point16(x, y), () => new TileCopy(x, y));
                        if (EditorSystem.Local.Settings.ShouldUpdateDrawnTiles)
                        {
                            // we also need to add the tiles around it since those also get affected if we have update tiles on
                            _brushStroke.TryAddTile(new Point16(x + 1, y), () => new TileCopy(x + 1, y));
                            _brushStroke.TryAddTile(new Point16(x - 1, y), () => new TileCopy(x - 1, y));
                            _brushStroke.TryAddTile(new Point16(x, y + 1), () => new TileCopy(x, y + 1));
                            _brushStroke.TryAddTile(new Point16(x, y - 1), () => new TileCopy(x, y - 1));
                        }
                    }
                }

                // dont paste every frame
                if (_pasteCounter == 2)
                {
                    ToolUtils.Paste(_tilesToPaste, new Point16(_tilesToPaste.GetMinX(), _tilesToPaste.GetMinY()), false, EditorSystem.Local.Settings.ShouldUpdateDrawnTiles);
                    _tilesToPaste.Clear();
                    _pasteCounter = 0;
                }
                else
                {
                    _pasteCounter++;
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
                    _d = Math.Clamp(_d, 1, 100);
                    _sizeField.SetValue(_d);
                }
            }
        }
    }
}
