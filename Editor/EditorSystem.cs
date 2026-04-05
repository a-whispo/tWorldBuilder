using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Chat;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Content;
using TerrariaInGameWorldEditor.Content.Tools;
using TerrariaInGameWorldEditor.Editor.Windows;
using TerrariaInGameWorldEditor.Editor.Windows.Blueprints;
using TerrariaInGameWorldEditor.Editor.Windows.Masks;
using TerrariaInGameWorldEditor.Editor.Windows.Save;
using TerrariaInGameWorldEditor.Editor.Windows.Settings;
using TerrariaInGameWorldEditor.Editor.Windows.TileSelector;

namespace TerrariaInGameWorldEditor.Editor
{
    internal class EditorSystem : ModSystem
    {
        // local instance
        public static EditorSystem Local { get; private set; }

        // main ui
        public float Scale
        {
            get => _scale;
            set
            {
                if (UseCustomScale)
                {
                    Main.UIScale = value;
                    _scale = value;
                }
            }
        }
        private float _scale = 1f;
        private float _actualScale = Main.UIScale;
        public bool UseCustomScale
        {
            get => _useCustomScale;
            set
            {
                if (_useCustomScale != value)
                {
                    if (!_useCustomScale)
                    {
                        _actualScale = Main.UIScale;
                    }
                    Main.UIScale = value ? Scale : _actualScale;
                    _useCustomScale = value;
                }
            }
        }
        private bool _useCustomScale = false;
        public bool IsEditorVisible => _mainUIState.Visible;
        private EditorUI _mainUIState;
        private UserInterface _mainUserInterface;

        // windows
        private TileSelectorUI _selectTileUIState;
        private SettingsUI _settingsUIState;
        private BlueprintsUI _blueprintsUIState;
        private SaveUI _saveUIState;
        private MaskUI _maskUIState;

        // tools
        public List<Tool> Tools { get; private set; }
        private PasteTool _pasteTool;
        private Tool _currentTool;
        public Tool CurrentTool
        { // current selected tool
            get => _currentTool;
            set
            {
                _currentTool = value;
                _mainUIState.RecalculateToolSettings();
            }
        }

        // editing
        public TileCollection Clipboard // current clipboard
        {
            get => _clipboard;
            set
            {
                if (_clipboard != null)
                {
                    _clipboard.OnChanged -= ClipboardChanged;
                }
                _clipboard = value;
                if (_clipboard != null)
                {
                    _clipboard.OnChanged += ClipboardChanged;
                }
                OnClipboardChanged?.Invoke(this, EventArgs.Empty);
            }

        }
        public event EventHandler OnClipboardChanged;
        private TileCollection _clipboard = new TileCollection();

        public TileCollection CurrentSelection // current selection
        {
            get => _currentSelection;
            set
            {
                if (_currentSelection != null)
                {
                    _currentSelection.OnChanged -= SelectionChanged;
                }
                _currentSelection = value;
                if (_currentSelection != null)
                {
                    _currentSelection.OnChanged += SelectionChanged;
                }
                OnSelectionChanged?.Invoke(this, EventArgs.Empty);
            }

        }
        public event EventHandler OnSelectionChanged;
        private TileCollection _currentSelection = new TileCollection();

        public TileCopy SelectedTile // current selected tile
        {
            get => _selectedTile;
            set
            {
                _selectedTile = value;
                OnSelectedTileChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler OnSelectedTileChanged;
        private TileCopy _selectedTile;

        // screen offsets
        private Point _mouseMiddleClickedPoint;
        private Vector2 _screenPositionOffset;

        // redo undo
        public int UndoCount => _undoHistory.Count;
        private List<TileCollection> _undoHistory = new List<TileCollection>();
        public int RedoCount => _redoHistory.Count;
        private List<TileCollection> _redoHistory = new List<TileCollection>();

        // settings
        public TIGWESettings Settings { get; private set; }

        public override void OnModLoad()
        {
            base.OnModLoad();
            if (!Main.dedServ)
            {
                Local = this;
                _pasteTool = new PasteTool();
                Tools = new List<Tool> { new BrushTool(), new EraseTool(), new LineTool(), new ShapesTool(), new PaintBucketTool(), new TilePickerTool(), new BoxSelectionTool(), new MagicWandTool(), new LassoTool() };
                Settings = TIGWESettings.Load($"{ModLoader.ModPath.Replace("\\Mods", "")}\\{TerrariaInGameWorldEditor.MODNAME}\\settings");
            }
        }

        public override void OnModUnload()
        {
            base.OnModUnload();
            if (!Main.dedServ)
            {
                Local = null;
                TIGWESettings.Save($"{ModLoader.ModPath.Replace("\\Mods", "")}\\{TerrariaInGameWorldEditor.MODNAME}\\settings", Settings);
            }
        }

        public override void OnWorldUnload()
        {
            base.OnWorldUnload();
            if (!Main.dedServ)
            {
                TIGWESettings.Save($"{ModLoader.ModPath.Replace("\\Mods", "")}\\{TerrariaInGameWorldEditor.MODNAME}\\settings", Settings);
            }
        }

        public override void PostSetupContent()
        {
            base.PostSetupContent();

            if (!Main.dedServ)
            {
                // ui stuff
                _mainUIState = new EditorUI();
                _mainUIState.Activate();
                _mainUserInterface = new UserInterface();
                _selectTileUIState = new TileSelectorUI();
                _selectTileUIState.Title = "(Selected Tile) Tile Selector";
                _selectTileUIState.OnTileConfirmed += (_, tileCopy) =>
                {
                    SelectedTile = tileCopy;
                };
                _settingsUIState = new SettingsUI();
                _blueprintsUIState = new BlueprintsUI();
                _saveUIState = new SaveUI();
                _maskUIState = new MaskUI();
                TIGWEUISystem.Local.RegisterUI(_selectTileUIState);
                TIGWEUISystem.Local.RegisterUI(_settingsUIState);
                TIGWEUISystem.Local.RegisterUI(_blueprintsUIState);
                TIGWEUISystem.Local.RegisterUI(_saveUIState);
                TIGWEUISystem.Local.RegisterUI(_maskUIState);
                IL_RemadeChatMonitor.DrawChat += DrawChat;
                IL_Main.DrawPlayerChat += DrawPlayerChat;

                // smart cursor kinda messes with the drawing/pasting since the tools use tileTargetX/Y
                // so we want to make sure its always off when the editor is open
                On_Player.TryToToggleSmartCursor += DisableSmartCursor;
            }

            // default
            Tile tile = new Tile();
            tile.HasTile = true;
            tile.TileType = TileID.Dirt;
            tile.WallType = WallID.None;
            tile.TileFrameX = 18;
            tile.TileFrameY = 18;
            SelectedTile = new TileCopy(tile);
        }

        private void DisableSmartCursor(On_Player.orig_TryToToggleSmartCursor orig, Player self, ref bool smartCursorWanted)
        {
            if (_mainUIState.Visible)
            {
                smartCursorWanted = false;
            }
            orig(self, ref smartCursorWanted);
        }

        private void DrawPlayerChat(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                for (int i = 0; i < 5; i++)
                {
                    c.GotoNext(i => i.MatchNewobj<Vector2>());
                    c.Index++;
                    c.EmitDelegate<Func<Vector2, Vector2>>((position) =>
                    {
                        return _mainUIState.Visible ? new Vector2(position.X + _mainUIState.LeftWidth - 70, position.Y - 2 - _mainUIState.BottomHeight) : position;
                    });
                }
            }
            catch (Exception ex)
            {
                MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaInGameWorldEditor>(), il);
                TerrariaInGameWorldEditor.Error("Error IL editing DrawPlayerChat.", ex);
            }
        }

        private void DrawChat(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(i => i.MatchNewobj<Vector2>());
                c.Index++;
                c.EmitLdloc3();
                c.EmitDelegate<Func<Vector2, int, Vector2>>((position, num5) =>
                {
                    return _mainUIState.Visible ? new Vector2(_mainUIState.LeftWidth + 12, Main.screenHeight - _mainUIState.BottomHeight - (Main.drawingPlayerChat ? 60 : 30) - num5 * 21) : position;
                });
            }
            catch (Exception ex)
            {
                MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaInGameWorldEditor>(), il);
                TerrariaInGameWorldEditor.Error("Error IL editing DrawChat.", ex);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // update UI
            if (_mainUIState.Visible)
            {
                if (_mainUserInterface.CurrentState == null)
                {
                    _mainUserInterface.SetState(_mainUIState);
                    TIGWEUISystem.Local.ShouldRenderUI = true;
                    return;
                }
                _mainUserInterface.Update(gameTime);
            }
            else
            {
                _mainUserInterface.SetState(null);
                TIGWEUISystem.Local.ShouldRenderUI = false;
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) // layer stuff
        {
            if (!_mainUIState.Visible)
            {
                return;
            }

            // draw tools before the main ui
            if (Local.CurrentSelection?.Count > 0)
            {
                DrawUtils.DrawTileCollectionOutline(Local.CurrentSelection, new Point(Local.CurrentSelection.GetMinX(), Local.CurrentSelection.GetMinY()), EditorSystem.Local.Settings.ToolColor);
                DrawUtils.DrawMiscOptions(new Rectangle(Local.CurrentSelection.GetMinX(), Local.CurrentSelection.GetMinY(), Local.CurrentSelection.GetWidth(), Local.CurrentSelection.GetHeight()), EditorSystem.Local.Settings.ShowCenterLines, EditorSystem.Local.Settings.ShowMeasureLines);
            }
            Local.CurrentTool?.Draw(Main.spriteBatch);

            // removes all layers except the ones we want
            layers.RemoveAll(layer =>
            {
                return !(layer.Name.Equals("Vanilla: Player Chat") || layer.Name.Equals("Vanilla: Cursor") || layer.Name.Equals($"{TerrariaInGameWorldEditor.MODNAME}: UI") || layer.Name.Equals("Vanilla: Interface Logic 4") || layer.Name.Equals("Vanilla: Tile Grid Option"));
            });

            // insert our layer before the cursor layer but after the tile grid option layer so the tile grid isnt drawn over our UI
            layers.Insert(layers.FindIndex(0, layers.Count, layer => layer.Name.Equals("Vanilla: Tile Grid Option")) + 1, new LegacyGameInterfaceLayer(
                $"{TerrariaInGameWorldEditor.MODNAME}: MainScreen",
                delegate
                {
                    if (_mainUserInterface.CurrentState != null)
                    {
                        // weird case when ui scale is a multiple of 25 that pointclamp causes artifacts with 9 splicing, so just avoid those cases
                        // a ui scale of 1 is fine tho
                        if (Main.UIScale % 0.25 == 0 && Main.UIScale != 1f)
                        {
                            Main.UIScale += 0.01f;
                        }
                        _mainUserInterface.Draw(Main.spriteBatch, Main.gameTimeCache);
                    }
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }

        public override void PostUpdateInput()
        {
            base.PostUpdateInput();

            if (Keybinds.OpenEditorMK.JustPressed && !Main.mapFullscreen)
            {
                ToggleEditor();
            }
            if (!_mainUIState?.Visible ?? true)
            {
                return;
            }

            // update input for the ui and tool
            _mainUIState.PostUpdateInput();
            if (!Main.LocalPlayer.mouseInterface)
            {
                Local.CurrentTool?.PostUpdateInput();
            }

            // remove current selection if escape is pressed
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Escape))
            {
                CurrentSelection?.Clear();
                if (CurrentTool is ISelectionTool selectionTool)
                {
                    selectionTool.ResetSelection();
                }
            }

            // keybinds that need ctrl
            if (PlayerInput.GetPressedKeys().Contains(Keys.LeftControl))
            {
                // undo
                if (Keybinds.UndoMK.JustPressed)
                {
                    if (_undoHistory.Count > 0)
                    {
                        Undo();
                        TerrariaInGameWorldEditor.NewText($"Undid.");
                    }
                    else
                    {
                        TerrariaInGameWorldEditor.NewText($"Nothing left to undo.");
                    }
                }

                // redo
                if (Keybinds.RedoMK.JustPressed)
                {
                    if (_redoHistory.Count > 0)
                    {
                        Redo();
                        TerrariaInGameWorldEditor.NewText($"Redid.");
                    }
                    else
                    {
                        TerrariaInGameWorldEditor.NewText($"Nothing left to redo.");
                    }
                }

                // copy
                if (Keybinds.CopyMK.JustPressed)
                {
                    if (CurrentSelection?.Count > 0)
                    {
                        CopyToClipboard(CurrentSelection); // copy
                        TerrariaInGameWorldEditor.NewText($"Copied.");
                    }
                }

                // paste
                if (Keybinds.PasteMK.JustPressed)
                {
                    if (Clipboard?.Count > 0) // if we have something in our clipboard
                    {
                        if (CurrentTool != _pasteTool)
                        {
                            CurrentTool = _pasteTool;
                            TerrariaInGameWorldEditor.NewText($"Paste tool opened. Press Ctrl + {Keybinds.PasteMK.GetAssignedKeys()[0]} again to paste.");
                        }
                    }
                    else
                    {
                        TerrariaInGameWorldEditor.NewText($"Nothing in clipboard.");
                    }
                }

                // cut
                if (Keybinds.CutMK.JustPressed)
                {
                    if (CurrentSelection != null)
                    {
                        CopyToClipboard(CurrentSelection);
                        ToolUtils.Delete(CurrentSelection);
                        TerrariaInGameWorldEditor.NewText($"Cut.");
                    }
                }

                // de select current tool
                if (Main.mouseRight && Main.mouseRightRelease)
                {
                    CurrentTool = null;
                }

                // save menu
                if (Keybinds.SaveMK.JustPressed)
                {
                    _saveUIState.Visible = true;
                }

                // delete
                if (Keybinds.DeleteMK.JustPressed)
                {
                    if (CurrentSelection != null)
                    {
                        ToolUtils.Delete(CurrentSelection);
                        TerrariaInGameWorldEditor.NewText($"Deleted.");
                    }
                }
            }
            else // if key 1 is not pressed down
            {
                // zoom in/out with mouse
                PlayerInput.LockVanillaMouseScroll($"{TerrariaInGameWorldEditor.MODNAME}/Zoom");
                if (PlayerInput.ScrollWheelDelta > 0 && !Main.LocalPlayer.mouseInterface)
                {
                    Main.GameZoomTarget = Math.Clamp(Main.GameZoomTarget + 0.1f, 1f, 2f);
                }
                if (PlayerInput.ScrollWheelDelta < 0 && !Main.LocalPlayer.mouseInterface)
                {
                    Main.GameZoomTarget = Math.Clamp(Main.GameZoomTarget - 0.1f, 1f, 2f);
                }

                // moving screen with mouse and movement keys
                if (Main.mouseMiddle)
                {
                    if (Main.mouseMiddleRelease)
                    {
                        _mouseMiddleClickedPoint = new Point((int)(Main.mouseX / Main.GameZoomTarget) + (int)_screenPositionOffset.X, (int)(Main.mouseY / Main.GameZoomTarget) + (int)_screenPositionOffset.Y);
                    }
                    _screenPositionOffset.X = _mouseMiddleClickedPoint.X - Main.mouseX / Main.GameZoomTarget;
                    _screenPositionOffset.Y = _mouseMiddleClickedPoint.Y - Main.mouseY / Main.GameZoomTarget;
                }
                int mult = 1;
                if (Keybinds.FastMoveMK.Current)
                {
                    mult = 3;
                }
                if (PlayerInput.Triggers.Current.KeyStatus["Up"] || PlayerInput.Triggers.Current.KeyStatus["Jump"])
                {
                    _screenPositionOffset.Y -= 10 * mult;
                }
                if (PlayerInput.Triggers.Current.KeyStatus["Down"])
                {
                    _screenPositionOffset.Y += 10 * mult;
                }
                if (PlayerInput.Triggers.Current.KeyStatus["Left"])
                {
                    _screenPositionOffset.X -= 10 * mult;
                }
                if (PlayerInput.Triggers.Current.KeyStatus["Right"])
                {
                    _screenPositionOffset.X += 10 * mult;
                }
                _screenPositionOffset = Vector2.Clamp(_screenPositionOffset, Vector2.Zero, new Vector2(Main.maxTilesX * 16, Main.maxTilesY * 16));
            }
        }

        public override void ModifyScreenPosition()
        {
            base.ModifyScreenPosition();

            if (_mainUIState.Visible)
            {
                Main.screenPosition = _screenPositionOffset;
            }
        }

        public override void PostUpdatePlayers()
        {
            base.PostUpdatePlayers();

            // update tools
            Local?.CurrentTool?.Update();
            if (Local?.CurrentTool is ISelectionTool selectionTool)
            {
                Local.CurrentSelection = selectionTool.GetSelection();
            }
        }

        private void ToggleEditor()
        {
            _mainUIState.Visible = !_mainUIState.Visible;

            if (!_mainUIState.Visible)
            {
                if (UseCustomScale)
                {
                    Main.UIScale = _actualScale;
                }
                CreativePowerManager.Instance.GetPower<CreativePowers.GodmodePower>().SetEnabledState(Main.LocalPlayer.whoAmI, false);
                CurrentTool = null;
                if (Local.Settings.ShouldTeleportOnEditorClosed)
                {
                    Vector2 screenPos = new Vector2(Main.screenPosition.X + Main.screenWidth / 2f - Main.LocalPlayer.width / 2f, Main.screenPosition.Y + Main.screenHeight / 2f - Main.LocalPlayer.height / 2f);
                    Main.LocalPlayer.Teleport(screenPos);
                    Main.blockInput = false;
                }
            }
            else
            {
                if (UseCustomScale)
                {
                    _actualScale = Main.UIScale;
                    Main.UIScale = Scale;
                }
                CreativePowerManager.Instance.GetPower<CreativePowers.GodmodePower>().SetEnabledState(Main.LocalPlayer.whoAmI, true);
                Main.ingameOptionsWindow = false;
                _screenPositionOffset = Main.screenPosition;
                Main.blockInput = true;
                Main.SmartCursorWanted_Mouse = false;
            }
        }

        public void ToggleWindow(EditorWindow window)
        {
            switch (window)
            {
                case EditorWindow.Main:
                    ToggleEditor();
                    break;
                case EditorWindow.Settings:
                    _settingsUIState.Visible = !_settingsUIState.Visible;
                    break;
                case EditorWindow.Blueprints:
                    _blueprintsUIState.Visible = !_blueprintsUIState.Visible;
                    break;
                case EditorWindow.Save:
                    _saveUIState.Visible = !_saveUIState.Visible;
                    break;
                case EditorWindow.SelectTile:
                    _selectTileUIState.Visible = !_selectTileUIState.Visible;
                    _selectTileUIState.SetCurrentTileCopy(SelectedTile);
                    break;
                case EditorWindow.Masks:
                    _maskUIState.Visible = !_maskUIState.Visible;
                    break;
            }
        }

        private void ClipboardChanged(object sender, EventArgs e)
        {
            OnClipboardChanged?.Invoke(sender, e);
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            OnSelectionChanged?.Invoke(sender, e);
        }

        public void CopyToClipboard(TileCollection tilesToCopy) // copy area in gotten area
        {
            Clipboard.Clear();
            Clipboard.TryAddTiles(tilesToCopy);
        }

        public void Redo()
        {
            TileCollection undo = new TileCollection();
            TileCollection redo = _redoHistory[_redoHistory.Count - 1];

            // same as undo pretty much
            foreach (var tile in redo)
            {
                undo.TryAddTile(new Point16(tile.Key.X, tile.Key.Y), new TileCopy(Main.tile[tile.Key.X, tile.Key.Y]));
                Main.tile[tile.Key.X, tile.Key.Y].CopyFrom(tile.Value.GetAsTile());
            }
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ToolUtils.SendTileCollectionUpdates(new Point16(redo.GetMinX(), redo.GetMinY()), redo);
            }
            AddToUndoHistory(undo);
            _redoHistory.RemoveAt(_redoHistory.Count - 1);
        }

        public void Undo()
        {
            TileCollection redo = new TileCollection();
            TileCollection undo = _undoHistory[_undoHistory.Count - 1];

            // go over all the tiles in the most recently added tile collection to undo
            foreach (var tile in undo)
            {
                redo.TryAddTile(new Point16(tile.Key.X, tile.Key.Y), new TileCopy(Main.tile[tile.Key.X, tile.Key.Y]));
                Main.tile[tile.Key.X, tile.Key.Y].CopyFrom(tile.Value.GetAsTile());
            }
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ToolUtils.SendTileCollectionUpdates(new Point16(undo.GetMinX(), undo.GetMinY()), undo);
            }
            AddToRedoHistory(redo);
            _undoHistory.RemoveAt(_undoHistory.Count - 1);
        }

        public void AddToRedoHistory(TileCollection redo)
        {
            if (Local.Settings.HistoryLimit <= _redoHistory.Count)
            {
                for (int i = 0; i < _redoHistory.Count - 1; i++)
                {
                    _redoHistory[i] = _redoHistory[i + 1];
                }
                _redoHistory[_redoHistory.Count - 1] = redo;
            }
            else
            {
                _redoHistory.Add(redo);
            }
        }

        public void AddToUndoHistory(TileCollection undo)
        {
            if (Local.Settings.HistoryLimit <= _undoHistory.Count)
            {
                for (int i = 0; i < _undoHistory.Count - 1; i++)
                {
                    _undoHistory[i] = _undoHistory[i + 1];
                }
                _undoHistory[_undoHistory.Count - 1] = undo;
            }
            else
            {
                _undoHistory.Add(undo);
            }
        }
    }
}
