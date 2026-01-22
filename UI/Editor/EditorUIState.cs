using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Content.Tools;
using TerrariaInGameWorldEditor.UI.UIElements.Button;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UI.Editor
{
    public class EditorUIState : UIState
    {
        public bool Visible { get; set; } = false;

        private TIGWEButton _undoButton;
        private TIGWEButton _redoButton;
        private UIText _toolInfoText;
        private EditorPalette _palette;
        private TIGWEButton _paletteDeleteButton;
        private UIElement _toolSettings;

        #region Side Dimensions
        private int _leftWidth;
        private int _rightWidth;
        private int _topHeight;
        private int _bottomHeight;

        public int LeftWidth
        {
            get => _leftWidth;
            set
            {
                _leftWidth = Math.Clamp(value, 34, Main.screenWidth / 8);
                RecalculateSideDimensions();
            }
        }

        public int RightWidth
        {
            get => _rightWidth;
            set
            {
                _rightWidth = Math.Clamp(value, 50, Main.screenWidth / 8);
                RecalculateSideDimensions();
            }
        }

        public int TopHeight
        {
            get => _topHeight;
            set
            {
                _topHeight = Math.Clamp(value, 34, 34);
                RecalculateSideDimensions();
            }
        }

        public int BottomHeight
        {
            get => _bottomHeight;
            set
            {
                _bottomHeight = Math.Clamp(value, 34, Main.screenHeight / 8);
                RecalculateSideDimensions();
            }
        }
        #endregion

        #region Sides
        private TIGWEImageResizeable _bottom;
        private TIGWEImageResizeable _left;
        private TIGWEImageResizeable _right;
        private TIGWEImageResizeable _titleBar;
        private TIGWEImageResizeable _top;
        private TIGWEImageResizeable _innerBorder;

        // resizing
        private bool _isDraggingSide = false;
        private bool _hoveringRight = false;
        private bool _hoveringLeft = false;
        private bool _hoveringTop = false;
        private bool _hoveringBottom = false;
        private event EventHandler OnRecalculateSides;
        #endregion

        public override void OnInitialize()
        {
            base.OnInitialize();
            Left.Set(0, 0f);
            Top.Set(0, 0f);
            Width.Set(Main.screenWidth * Main.UIScale, 1f);
            Height.Set(Main.screenHeight * Main.UIScale, 1f);

            // main body
            _bottom = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenColor"), 1, 1);
            Append(_bottom);
            _left = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenColor"), 1, 1);
            Append(_left);
            _right = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenColor"), 1, 1);
            Append(_right);
            _titleBar = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenTitleBar"), 4, 2);
            Append(_titleBar);
            _top = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenColor"), 1, 1);
            Append(_top);
            _innerBorder = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenInnerBorder"), 6, 4);
            Append(_innerBorder);

            // some default bounds
            BottomHeight = 32;
            LeftWidth = 34;
            RightWidth = 50;
            TopHeight = 34;

            // x button
            TIGWEButton xButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/XButton"));
            xButton.SetVisibility(0.8f, 1f);
            xButton.Width.Set(30, 0f);
            xButton.Height.Set(30, 0f);
            xButton.Left.Set(Width.Pixels - xButton.Width.Pixels, 0f);
            xButton.Top.Set(6, 0f);
            xButton.OnLeftClick += (_, _) =>
            {
                EditorSystem.Local.ToggleWindow(EditorWindow.Main);
            };
            xButton.HoverText = "Close";
            Append(xButton);

            // settings button
            TIGWEButton settingsButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/SettingsButton"));
            settingsButton.SetVisibility(0.8f, 1f);
            settingsButton.Width.Set(30, 0f);
            settingsButton.Height.Set(30, 0f);
            settingsButton.Left.Set(2, 0f);
            settingsButton.Top.Set(4, 0f);
            settingsButton.OnLeftClick += (_, _) =>
            {
                EditorSystem.Local.ToggleWindow(EditorWindow.Settings);
            };
            settingsButton.HoverText = "Settings";
            Append(settingsButton);

            // blueprints button
            TIGWEButton blueprintsButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/BlueprintsButton"));
            blueprintsButton.SetVisibility(0.8f, 1f);
            blueprintsButton.Width.Set(30, 0f);
            blueprintsButton.Height.Set(30, 0f);
            blueprintsButton.Left.Set(settingsButton.Left.Pixels + settingsButton.Width.Pixels + 2, 0f);
            blueprintsButton.Top.Set(4, 0f);
            blueprintsButton.OnLeftClick += (_, _) =>
            {
                EditorSystem.Local.ToggleWindow(EditorWindow.Blueprints);
            };
            blueprintsButton.HoverText = "Blueprints";
            Append(blueprintsButton);

            // save button
            TIGWEButton saveButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/SaveButton"));
            saveButton.SetVisibility(0.8f, 1f);
            saveButton.Width.Set(30, 0f);
            saveButton.Height.Set(30, 0f);
            saveButton.Left.Set(blueprintsButton.Left.Pixels + blueprintsButton.Width.Pixels + 2, 0f);
            saveButton.Top.Set(4, 0f);
            saveButton.OnLeftClick += (_, _) =>
            {
                EditorSystem.Local.ToggleWindow(EditorWindow.Save);
            };
            saveButton.HoverText = "Save";
            Append(saveButton);

            // current selected tile (opens tile browser on click)
            TIGWEButton tileButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TileButton"));
            tileButton.SetVisibility(0.8f, 1f);
            tileButton.Width.Set(30, 0f);
            tileButton.Height.Set(30, 0f);
            tileButton.Left.Set(2, 0f);
            tileButton.Top.Set(42, 0f);
            tileButton.HoverText = "Tile Browser";
            tileButton.OnLeftClick += (_, _) =>
            {
                EditorSystem.Local.ToggleWindow(EditorWindow.SelectTile);
            };
            // draw the selected tile
            tileButton.OnPreDrawSelf += (SpriteBatch spriteBatch) =>
            {
                if (EditorSystem.Local.SelectedTile != null)
                {
                    TileCopy selectedTile = EditorSystem.Local.SelectedTile;
                    TilePaintSystemV2 ps = Main.instance.TilePaintSystem;
                    Rectangle tileButtonRect = tileButton.GetDimensions().ToRectangle();
                    int x = tileButtonRect.X + tileButtonRect.Width / 2 - 16 / 2;
                    int y = tileButtonRect.Y + tileButtonRect.Height / 2 - 16 / 2;

                    // wall part
                    if (selectedTile.WallType != 0)
                    {

                        // get wall texture with tilepaintsystem to get the texture with paint if it has any
                        Texture2D tileWallTex = ps.TryGetWallAndRequestIfNotReady(selectedTile.WallType, selectedTile.WallColor);
                        if (tileWallTex != null)
                        {
                            // get texture from spritesheet with help from frameX and frameY and draw it
                            spriteBatch.Draw(tileWallTex, new Rectangle(x - 8, y - 8, 32, 32), new Rectangle(selectedTile.WallFrameX, selectedTile.WallFrameY, 32, 32), Color.White * (tileButton.IsMouseHovering ? 0.8f : 1f));
                        }
                    }

                    // tile part
                    if (selectedTile.HasTile)
                    {
                        // get tile texture with tilepaintsystem to get the texture with paint if it has any
                        Texture2D tileTex = ps.TryGetTileAndRequestIfNotReady(selectedTile.TileType, selectedTile.TileFrameNumber, selectedTile.TileColor);

                        if (tileTex != null)
                        {
                            // get texture from spritesheet with help from frameX and frameY and draw it
                            spriteBatch.Draw(tileTex, new Rectangle(x, y, 16, 16), new Rectangle(selectedTile.TileFrameX, selectedTile.TileFrameY, 16, 16), Color.White * (tileButton.IsMouseHovering ? 0.8f : 1f));

                        }
                    }

                    // liquid
                    if (selectedTile.LiquidAmount > 0)
                    {
                        int num = 0;
                        int height = (int)(((float)selectedTile.LiquidAmount / 255) * 16);
                        if (height < 6)
                        {
                            height = 6;
                        }
                        spriteBatch.Draw(TextureAssets.Liquid[selectedTile.LiquidType].Value, new Rectangle(x, y + (16 - height), 16, height), new Rectangle(0, num, 16, height), Color.White * 0.6f * (tileButton.IsMouseHovering ? 0.8f : 1f));
                    }

                    // wire
                    if ((selectedTile.GreenWire || selectedTile.RedWire || selectedTile.YellowWire || selectedTile.BlueWire))
                    {
                        Rectangle boundsTile = new Rectangle(x, y, 16, 16);

                        void DrawWire(string wireType, float t)
                        {
                            Texture2D texture = TextureAssets.Wire.Value;
                            switch (wireType)
                            {
                                case "Red":
                                    texture = TextureAssets.Wire.Value;
                                    break;
                                case "Blue":
                                    texture = TextureAssets.Wire2.Value;
                                    break;
                                case "Green":
                                    texture = TextureAssets.Wire3.Value;
                                    break;
                                case "Yellow":
                                    texture = TextureAssets.Wire4.Value;
                                    break;
                            }

                            // none
                            spriteBatch.Draw(texture, boundsTile, new Rectangle(0, 54, 16, 16), Color.White * t * (tileButton.IsMouseHovering ? 0.8f : 1f));
                        }

                        // draw the wires
                        if (selectedTile.RedWire)
                        {
                            DrawWire("Red", 0.65f);
                        }
                        if (selectedTile.BlueWire)
                        {
                            DrawWire("Blue", 0.4f);
                        }
                        if (selectedTile.GreenWire)
                        {
                            DrawWire("Green", 0.4f);
                        }
                        if (selectedTile.YellowWire)
                        {
                            DrawWire("Yellow", 0.4f);
                        }
                    }

                    // actuator
                    if (selectedTile.HasActuator)
                    {
                        Rectangle boundsTile = new Rectangle(x, y, 16, 16);

                        Texture2D texture = TextureAssets.Actuator.Value;
                        spriteBatch.Draw(texture, boundsTile, Color.White * 0.6f * (tileButton.IsMouseHovering ? 0.8f : 1f));
                    }
                }
            };
            Append(tileButton);

            // mask button
            TIGWEButton maskButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MaskButton"));
            maskButton.SetVisibility(0.8f, 1f);
            maskButton.Width.Set(30, 0f);
            maskButton.Height.Set(30, 0f);
            maskButton.Left.Set(saveButton.Left.Pixels + saveButton.Width.Pixels + 2, 0f);
            maskButton.Top.Set(4, 0f);
            maskButton.OnLeftClick += (_, _) =>
            {
                EditorSystem.Local.ToggleWindow(EditorWindow.Masks);
            };
            maskButton.HoverText = "Masks";
            Append(maskButton);

            // save tile to palette button
            TIGWEButton saveTileButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/AddButton"));
            saveTileButton.SetVisibility(0.8f, 1f);
            saveTileButton.Width.Set(30, 0f);
            saveTileButton.Height.Set(30, 0f);
            saveTileButton.Left.Set(tileButton.Left.Pixels + tileButton.Width.Pixels + 2, 0f);
            saveTileButton.Top.Set(42, 0f);
            saveTileButton.HoverText = "Add to current palette";
            saveTileButton.OnLeftClick += (_, _) =>
            {
                PaletteItem item = new PaletteItem(EditorSystem.Local.SelectedTile);
                item.OnLeftClick += (_, _) =>   
                {
                    EditorSystem.Local.SelectedTile = item.TileCopy;
                };
                _palette.AddItem(item);
                _palette.Recalculate();
                RecalculateSideDimensions();
                SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
            };
            Append(saveTileButton);

            // undo button
            _undoButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Undo"));
            _undoButton.SetVisibility(0.8f, 1f);
            _undoButton.Width.Set(30, 0f);
            _undoButton.Height.Set(30, 0f);
            _undoButton.Left.Set(saveTileButton.Left.Pixels + saveTileButton.Width.Pixels + 2, 0f);
            _undoButton.Top.Set(42, 0f);
            _undoButton.HoverText = "Undo (Ctrl + Z)";
            _undoButton.OnLeftClick += (_, _) =>
            {
                EditorSystem.Local.Undo();
            };
            Append(_undoButton);

            // redo button
            _redoButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Redo"));
            _redoButton.SetVisibility(0.8f, 1f);
            _redoButton.Width.Set(30, 0f);
            _redoButton.Height.Set(30, 0f);
            _redoButton.Left.Set(_undoButton.Left.Pixels + _undoButton.Width.Pixels + 2, 0f);
            _redoButton.Top.Set(42, 0f);
            _redoButton.HoverText = "Redo (Ctrl + Y)";
            _redoButton.OnLeftClick += (_, _) =>
            {
                EditorSystem.Local.Redo();
            };
            Append(_redoButton);

            // tool settings
            _toolSettings = new UIElement();
            _toolSettings.Top.Set(42, 0f);
            _toolSettings.Left.Set(_redoButton.Left.Pixels + _redoButton.Width.Pixels + 30, 0f);
            _toolSettings.Width.Set(0, 1f);
            _toolSettings.Height.Set(0, 1f);
            Append(_toolSettings);

            UIGrid toolGrid = new UIGrid();
            // loop through all the tools and add everything
            for (int i = 0; i < EditorSystem.Local.Tools.Count; i++)
            {
                Tool tool = EditorSystem.Local.Tools[i];
                int toolWidth = 30;
                int toolHeight = 30;
                tool.ToggleToolButton.OnLeftClick += (_, _) =>
                {
                    // reset selection if we clicked another selection tool
                    if (tool is SelectionTool selectionTool && selectionTool != EditorSystem.Local.CurrentTool)
                    {
                        selectionTool.ResetSelection();
                    }

                    // toggle tool
                    EditorSystem.Local.CurrentTool = EditorSystem.Local.CurrentTool == tool ? null : tool;
                    SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
                };
                tool.ToggleToolButton.SetVisibility(0.8f, 1f);
                tool.ToggleToolButton.Width.Set(toolWidth, 0f);
                tool.ToggleToolButton.Height.Set(toolHeight, 0f);
                toolGrid.Add(tool.ToggleToolButton);
            }
            toolGrid.ListPadding = 2f;
            toolGrid.Top.Set(_top.Top.Pixels + _top.Height.Pixels, 0f);
            toolGrid.Left.Set(2, 0f);
            toolGrid.Width.Set(LeftWidth - 4, 0f);
            toolGrid.Height.Set(0, 0.5f);
            OnRecalculateSides += (_, _) =>
            {
                toolGrid.Width.Set(LeftWidth - 4, 0f);
            };
            Append(toolGrid);

            // palette (wow this sucks)
            _palette = new EditorPalette();
            _palette.Top.Set(_top.Top.Pixels + _top.Height.Pixels, 0f);
            _palette.Left.Set(_right.Left.Pixels + 2, 0f);
            _palette.Width.Set(RightWidth - 4, 0f);
            _palette.AutoResizeHeight = true;
            OnRecalculateSides += (_, _) =>
            {
                _palette.Width.Set(RightWidth - 4, 0f);
                _palette.Left.Set(_right.Left.Pixels + 2, 0f);
            };
            Append(_palette);
            // buttons for palette
            _paletteDeleteButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/DeleteButton"));
            _paletteDeleteButton.HoverText = "Delete";
            _paletteDeleteButton.Width.Set(30, 0f);
            _paletteDeleteButton.Height.Set(30, 0f);
            _paletteDeleteButton.SetVisibility(0.8f, 1f);
            _paletteDeleteButton.OnLeftClick += (_, _) =>
            {
                _palette.IsDeletingItems = !_palette.IsDeletingItems;
                _paletteDeleteButton.HoverText = _palette.IsDeletingItems ? "Finish Deleting" : "Delete";                
            };
            TIGWEButton paletteClearButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/ClearButton"));
            paletteClearButton.HoverText = "Clear";
            paletteClearButton.Width.Set(30, 0f);
            paletteClearButton.Height.Set(30, 0f);
            paletteClearButton.SetVisibility(0.8f, 1f);
            paletteClearButton.OnLeftClick += (_, _) =>
            {
                if (!_palette.IsDeletingItems)
                {
                    _palette.ClearItems();
                }
            };
            UIGrid paletteButtonGrid = new UIGrid();
            paletteButtonGrid.Height.Set(200, 0f);
            paletteButtonGrid.Left.Set(10, 0f);
            paletteButtonGrid.ListPadding = 2f;
            paletteButtonGrid.Add(_paletteDeleteButton);
            paletteButtonGrid.Add(paletteClearButton);
            OnRecalculateSides += (_, _) =>
            {
                paletteButtonGrid.Top.Set(_palette.Top.Pixels + _palette.Height.Pixels + 2, 0f);
                paletteButtonGrid.Width.Set(RightWidth - 20, 0f);
            };
            _right.Append(paletteButtonGrid);

            // text at the bottom with info about stuff
            _toolInfoText = new UIText("");
            _toolInfoText.Left.Set(_left.Width.Pixels + 4, 0f);
            _toolInfoText.Top.Set(8, 0f);
            _bottom.Append(_toolInfoText);

            // update everything
            RecalculateSideDimensions();
        }

        public void PostUpdateInput()
        {
            // check if we're hovering the ui
            Main.LocalPlayer.mouseInterface = !(_innerBorder.GetViewCullingArea().Contains(new Point(Main.mouseX, Main.mouseY))) || _isDraggingSide || Main.LocalPlayer.mouseInterface;

            // hovering sides
            var dimensions = _innerBorder.GetDimensions();
            if (!_isDraggingSide)
            {
                _hoveringRight = Math.Abs(Main.mouseX - dimensions.X - dimensions.Width) < 5f && Main.mouseY > dimensions.Y && Main.mouseY < dimensions.Y + dimensions.Height;
                _hoveringLeft = Math.Abs(Main.mouseX - dimensions.X) < 5f && Main.mouseY > dimensions.Y && Main.mouseY < dimensions.Y + dimensions.Height;
                _hoveringTop = Math.Abs(Main.mouseY - dimensions.Y) < 5f && Main.mouseX > dimensions.X && Main.mouseX < dimensions.X + dimensions.Width;
                _hoveringBottom = Math.Abs(Main.mouseY - dimensions.Y - dimensions.Height) < 5f && Main.mouseX > dimensions.X && Main.mouseX < dimensions.X + dimensions.Width;
            }
            if (Main.mouseLeft && Main.mouseLeftRelease && (_hoveringRight || _hoveringLeft || _hoveringTop || _hoveringBottom) || _isDraggingSide)
            {
                _isDraggingSide = true;
                if (_hoveringRight)
                {
                    RightWidth = (int)(Width.Pixels - Main.mouseX);
                }
                if (_hoveringLeft)
                {
                    LeftWidth = (int)(Main.mouseX);
                }
                if (_hoveringTop)
                {
                    TopHeight = (int)(Main.mouseY - _titleBar.Height.Pixels);
                }
                if (_hoveringBottom)
                {
                    BottomHeight = (int)(Height.Pixels - Main.mouseY);
                }
            }
            if (!Main.mouseLeft)
            {
                _isDraggingSide = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // update undo and redo buttons to match if we can undo or redo
            if (EditorSystem.Local.RedoHistory.Count > 0)
            {
                _redoButton.IgnoresMouseInteraction = false;
                _redoButton.SetVisibility(0.8f, 1f);
            }
            else
            {
                _redoButton.IgnoresMouseInteraction = true;
                _redoButton.SetVisibility(0.6f, 0.6f);
            }
            if (EditorSystem.Local.UndoHistory.Count > 0)
            {
                _undoButton.IgnoresMouseInteraction = false;
                _undoButton.SetVisibility(0.8f, 1f);
            } else
            {
                _undoButton.IgnoresMouseInteraction = true;
                _undoButton.SetVisibility(0.6f, 0.6f);
            }
            _toolInfoText.SetText($"([c/EAD87A:X:] {Player.tileTargetX}, [c/EAD87A:Y:] {Player.tileTargetY}) {EditorSystem.Local.CurrentTool?.GetInfoText()}");
            _toolInfoText.Left.Set(LeftWidth + 4, 0f);
            _toolInfoText.Recalculate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // should maybe not do this every frame but whatever
            RecalculateSideDimensions();

            // draw the ui on top
            base.Draw(spriteBatch);

            // draw gold outline on selected tool button
            if (EditorSystem.Local.CurrentTool != null)
            {
                Rectangle dimensions = EditorSystem.Local.CurrentTool.ToggleToolButton.GetViewCullingArea();
                spriteBatch.Draw(DrawUtils.BlankTexture2D, new Rectangle(dimensions.X + 2, dimensions.Y, dimensions.Width - 4, 2), new Color(166, 105, 22));
                spriteBatch.Draw(DrawUtils.BlankTexture2D, new Rectangle(dimensions.X + +dimensions.Width - 2, dimensions.Y + 2, 2, dimensions.Height - 4), new Color(166, 105, 22));
                spriteBatch.Draw(DrawUtils.BlankTexture2D, new Rectangle(dimensions.X + 2, dimensions.Y + dimensions.Height - 2, dimensions.Width - 4, 2), new Color(227, 167, 43));
                spriteBatch.Draw(DrawUtils.BlankTexture2D, new Rectangle(dimensions.X, dimensions.Y + 2, 2, dimensions.Height - 4), new Color(227, 167, 43));
            }
            // draw gold outline if we're deleting palette items
            if (_palette.IsDeletingItems)
            {
                Rectangle dimensions = _paletteDeleteButton.GetViewCullingArea();
                spriteBatch.Draw(DrawUtils.BlankTexture2D, new Rectangle(dimensions.X + 2, dimensions.Y, dimensions.Width - 4, 2), new Color(166, 105, 22));
                spriteBatch.Draw(DrawUtils.BlankTexture2D, new Rectangle(dimensions.X + +dimensions.Width - 2, dimensions.Y + 2, 2, dimensions.Height - 4), new Color(166, 105, 22));
                spriteBatch.Draw(DrawUtils.BlankTexture2D, new Rectangle(dimensions.X + 2, dimensions.Y + dimensions.Height - 2, dimensions.Width - 4, 2), new Color(227, 167, 43));
                spriteBatch.Draw(DrawUtils.BlankTexture2D, new Rectangle(dimensions.X, dimensions.Y + 2, 2, dimensions.Height - 4), new Color(227, 167, 43));
            }
        }

        public void RecalculateToolSettings()
        {
            _toolSettings.RemoveAllChildren();
            if (EditorSystem.Local.CurrentTool != null)
            {
                ToolSetting previousSetting = null;
                foreach ((string, UIElement) setting in EditorSystem.Local.CurrentTool.Settings)
                {
                    ToolSetting toolSetting = new ToolSetting(setting.Item1, setting.Item2);
                    toolSetting.Top.Set(2, 0f);
                    toolSetting.Left.Set(previousSetting != null ? previousSetting.Width.Pixels + previousSetting.Left.Pixels + 30 : 0, 0f);
                    previousSetting = toolSetting;
                    _toolSettings.Append(toolSetting);
                }
            }
        }

        public void RecalculateSideDimensions()
        {
            // resize sides
            _bottom.Width.Set(0, 1f);
            _bottom.Height.Set(BottomHeight, 0f);
            _bottom.Left.Set(0, 0f);
            _bottom.Top.Set(Height.Pixels - _bottom.Height.Pixels, 0f);

            _left.Width.Set(LeftWidth, 0f);
            _left.Height.Set(0, 1f);
            _left.Left.Set(0, 0f);
            _left.Top.Set(0, 0f);

            _right.Width.Set(RightWidth, 0f);
            _right.Height.Set(0, 1f);
            _right.Left.Set(Width.Pixels - _right.Width.Pixels, 0f);
            _right.Top.Set(0, 0f);

            _titleBar.Width.Set(0, 1f);
            _titleBar.Height.Set(40, 0f);
            _titleBar.Left.Set(0, 0f);
            _titleBar.Top.Set(0, 0f);

            _top.Width.Set(0, 1f);
            _top.Height.Set(TopHeight, 0f);
            _top.Left.Set(0, 0f);
            _top.Top.Set(_titleBar.Height.Pixels, 0f);

            _innerBorder.Width.Set(Width.Pixels - _left.Width.Pixels - _right.Width.Pixels, 0f);
            _innerBorder.Height.Set(Height.Pixels - _top.Top.Pixels - _top.Height.Pixels - _bottom.Height.Pixels, 0f);
            _innerBorder.Left.Set(_left.Width.Pixels, 0f);
            _innerBorder.Top.Set(_top.Top.Pixels + _top.Height.Pixels, 0f);

            OnRecalculateSides?.Invoke(this, EventArgs.Empty);
            Recalculate();
        }
    }
}