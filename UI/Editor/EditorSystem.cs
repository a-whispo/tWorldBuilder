using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Content;
using TerrariaInGameWorldEditor.Content.Tools;
using TerrariaInGameWorldEditor.UI.TIGWEUI;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Blueprints;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Masks;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Save;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;
using TerrariaInGameWorldEditor.UI.TIGWEUI.TileSelector;

namespace TerrariaInGameWorldEditor.UI.Editor
{
    internal class EditorSystem : ModSystem
    {
        // local instance
        public static EditorSystem Local { get; private set; }

        // main ui
        private EditorUIState _mainUIState;
        private UserInterface _mainUserInterface;
        private SpriteBatch _spriteBatch;

        // windows
        private SelectTileUI _selectTileUIState;
        private SettingsUI _settingsUIState;
        private BlueprintsUI _blueprintsUIState;
        private SaveUI _saveUIState;
        private MaskUI _maskUIState;

        // tools
        public List<Tool> Tools { get; private set; } 
        private PasteTool _pasteTool = new PasteTool();
        private Tool _currentTool;
        public Tool CurrentTool { // current selected tool
            get {
                return _currentTool;
            }
            set {
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
        public List<TileCollection> UndoHistory { get; set; } = new List<TileCollection>();
        public List<TileCollection> RedoHistory { get; set; } = new List<TileCollection>(); 

        public override void OnModLoad()
        {
            base.OnModLoad();
            Local = this;
            Tools = new List<Tool> { new BrushTool(), new EraseTool(), new LineTool(), new ShapesTool(), new PaintBucketTool(), new TilePickerTool(), new BoxSelectionTool(), new MagicWandTool(), new LassoTool() };
        }

        public override void PostSetupContent()
        {
            base.PostSetupContent();

            // ui stuff
            _mainUIState = new EditorUIState();
            _mainUIState.Activate();
            _mainUserInterface = new UserInterface();
            _selectTileUIState = new SelectTileUI();
            _settingsUIState = new SettingsUI();
            _blueprintsUIState = new BlueprintsUI();
            _saveUIState = new SaveUI();
            _maskUIState = new MaskUI();
            TIGWEUISystem.Local.RegisterUI(_selectTileUIState);
            TIGWEUISystem.Local.RegisterUI(_settingsUIState);
            TIGWEUISystem.Local.RegisterUI(_blueprintsUIState);
            TIGWEUISystem.Local.RegisterUI(_saveUIState);
            TIGWEUISystem.Local.RegisterUI(_maskUIState);

            // for testing
            Tile tile = new Tile();
            tile.HasTile = true;
            tile.TileType = 0;
            tile.WallType = 0;
            tile.TileFrameX = 18;
            tile.TileFrameY = 18;
            SelectedTile = new TileCopy(tile);

            // smart cursor kinda messes with the drawing/pasting since the tools use tileTargetX/Y
            // so we want to make sure its always off when the editor is open
            On_Player.TryToToggleSmartCursor += DisableSmartCursor;

            /*
            Main.OnPreDraw += (_) =>
            {
                return;
                if (!_mainUIState.Visible)
                {
                    return;
                }

                // make sure to remove the local player from players that are to be drawn
                FieldInfo _playersThatDrawBehindNPCs = typeof(Main).GetField("_playersThatDrawBehindNPCs", BindingFlags.NonPublic | BindingFlags.Instance);
                ((List<Player>)_playersThatDrawBehindNPCs.GetValue(Main.instance)).Remove(Main.LocalPlayer);
                FieldInfo _playersThatDrawAfterProjectiles = typeof(Main).GetField("_playersThatDrawAfterProjectiles", BindingFlags.NonPublic | BindingFlags.Instance);
                ((List<Player>)_playersThatDrawAfterProjectiles.GetValue(Main.instance)).Remove(Main.LocalPlayer);
            };
            Main.OnPostDraw += (_) =>
            {
                if (!_mainUIState.Visible)
                {
                    return;
                }

                // make sure to re add the player
                MethodInfo RefreshPlayerDrawOrder = typeof(Main).GetMethod("RefreshPlayerDrawOrder", BindingFlags.NonPublic | BindingFlags.Instance);
                RefreshPlayerDrawOrder.Invoke(Main.instance, null);
            };
            */
        }

        private void DisableSmartCursor(On_Player.orig_TryToToggleSmartCursor orig, Player self, ref bool smartCursorWanted)
        {
            if (_mainUIState.Visible)
            {
                smartCursorWanted = false;
            }
            orig(self, ref smartCursorWanted);
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

                // temporarily adjust to make everything act as if the UI scale is 1f
                Main.mouseX = (int)(Main.mouseX * Main.UIScale);
                Main.mouseY = (int)(Main.mouseY * Main.UIScale);
                int tempWidth = Main.screenWidth;
                int tempHeight = Main.screenHeight;
                float tempUIScale = Main.UIScale;
                Main.screenWidth = (int)(Main.screenWidth * Main.UIScale);
                Main.screenHeight = (int)(Main.screenHeight * Main.UIScale);
                Main.UIScale = 1f;

                // do thing
                _mainUserInterface.Update(gameTime);

                // restore originals
                Main.UIScale = tempUIScale;
                Main.screenWidth = tempWidth;
                Main.screenHeight = tempHeight;
            }
            else
            {
                _mainUserInterface.SetState(null);
                TIGWEUISystem.Local.ShouldRenderUI = false;
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) // layer stuff
        {
            // dont render if not visible
            if (!_mainUIState.Visible)
            {
                return;
            }

            // setup spritebatch if we havent yet
            _spriteBatch ??= new SpriteBatch(Main.graphics.GraphicsDevice);

            // draw tools before the main ui
            if (Local.CurrentSelection?.Count > 0)
            {
                // draw selection outline
                DrawUtils.DrawTileCollectionOutline(Local.CurrentSelection, new Point(Local.CurrentSelection.GetMinX(), Local.CurrentSelection.GetMinY()), TIGWESettings.ToolColor);
                DrawUtils.DrawMiscOptions(new Rectangle(Local.CurrentSelection.GetMinX(), Local.CurrentSelection.GetMinY(), Local.CurrentSelection.GetWidth(), Local.CurrentSelection.GetHeight()), TIGWESettings.ShowCenterLines, TIGWESettings.ShowMeasureLines);
            }

            // draw current tool
            Local.CurrentTool?.Draw(_spriteBatch);

            // removes all layers except the cursor layer
            layers.RemoveAll(layer =>
            {
                // Vanilla: Interface Logic 4 is the mouse text, idk why
                return !(layer.Name.Equals("Vanilla: Cursor") || layer.Name.Equals($"{TerrariaInGameWorldEditor.MODNAME}: UI") || layer.Name.Equals("Vanilla: Interface Logic 4") || layer.Name.Equals("Vanilla: Tile Grid Option"));
            });

            // insert our layer before the cursor layer but after the tile grid option layer so the tile grid isnt drawn over our UI
            layers.Insert(layers.FindIndex(0, layers.Count, layer => layer.Name.Equals("Vanilla: Tile Grid Option")) + 1, new LegacyGameInterfaceLayer(
                $"{TerrariaInGameWorldEditor.MODNAME}: MainScreen",
                delegate
                {
                    if (_mainUserInterface.CurrentState != null)
                    {
                        // temporarily adjust to make everything act as if the UI scale is 1f
                        Main.mouseX = (int)(Main.mouseX * Main.UIScale);
                        Main.mouseY = (int)(Main.mouseY * Main.UIScale);
                        int tempWidth = Main.screenWidth;
                        int tempHeight = Main.screenHeight;
                        float tempUIScale = Main.UIScale;
                        Main.screenWidth = (int)(Main.screenWidth * Main.UIScale);
                        Main.screenHeight = (int)(Main.screenHeight * Main.UIScale);
                        Main.UIScale = 1f;

                        // start spritebatch with SamplerState.PointClamp and no UIScaleMatrix to 1f since the normal one is kinda ugly with UI scaling
                        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, default, Main.UIScaleMatrix);
                        _mainUserInterface.Draw(_spriteBatch, Main.gameTimeCache);
                        _spriteBatch.End();

                        // restore originals
                        Main.UIScale = tempUIScale;
                        Main.screenWidth = tempWidth;
                        Main.screenHeight = tempHeight;
                    }
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }

        public override void PostUpdateInput()
        {
            base.PostUpdateInput();

            // open
            if (Keybinds.OpenEditorMK.JustPressed)
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
                if (CurrentTool is SelectionTool selectionTool)
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
                    if (UndoHistory.Count > 0)
                    {
                        Undo();
                    }
                }

                // redo
                if (Keybinds.RedoMK.JustPressed)
                {
                    if (RedoHistory.Count > 0)
                    {
                        Redo();
                    }
                }

                // copy
                if (Keybinds.CopyMK.JustPressed)
                {
                    if (CurrentSelection?.Count > 0)
                    {
                        CopyToClipboard(CurrentSelection); // copy
                    }
                }

                // paste
                if (Keybinds.PasteMK.JustPressed)
                {
                    if (Clipboard?.Count > 0) // if we have something in our clipboard
                    {
                        CurrentTool = _pasteTool;
                    }
                }

                // cut
                if (Keybinds.CutMK.JustPressed)
                {
                    if (CurrentSelection != null)
                    {
                        CopyToClipboard(CurrentSelection);
                        Delete(CurrentSelection, true);
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
                        Delete(CurrentSelection, true);
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
                        _mouseMiddleClickedPoint = new Point((int)((float)Main.mouseX / Main.GameZoomTarget) + (int)_screenPositionOffset.X, (int)((float)Main.mouseY / Main.GameZoomTarget) + (int)_screenPositionOffset.Y);
                    }
                    _screenPositionOffset.X = _mouseMiddleClickedPoint.X - Main.mouseX / Main.GameZoomTarget;
                    _screenPositionOffset.Y = _mouseMiddleClickedPoint.Y - Main.mouseY / Main.GameZoomTarget;
                }
                int mult = 1;
                if (PlayerInput.GetPressedKeys().Contains(Keys.LeftShift))
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
            }
        }

        public override void ModifyScreenPosition()
        {
            base.ModifyScreenPosition();

            if (_mainUIState.Visible)
            {
                Main.screenPosition += _screenPositionOffset;
            }
        }

        public override void PostUpdatePlayers()
        {
            base.PostUpdatePlayers();

            // update tools
            Local.CurrentTool?.Update();
            if (Local.CurrentTool is SelectionTool selectionTool)
            {
                Local.CurrentSelection = selectionTool.GetSelection();
            }
        }

        private void ToggleEditor()
        {
            _mainUIState.Visible = !_mainUIState.Visible;
            
            if (!_mainUIState.Visible)
            {
                CreativePowerManager.Instance.GetPower<CreativePowers.GodmodePower>().SetEnabledState(Main.LocalPlayer.whoAmI, false);
                CurrentTool = null;
                if (TIGWESettings.ShouldTeleportOnEditorClosed)
                {
                    Vector2 screenPos = new Vector2(Main.screenPosition.X + Main.screenWidth / 2f - Main.LocalPlayer.width / 2f, Main.screenPosition.Y + Main.screenHeight / 2f - Main.LocalPlayer.height / 2f);
                    Main.LocalPlayer.Teleport(screenPos);
                    Main.blockInput = false;
                }
            }
            else
            {
                CreativePowerManager.Instance.GetPower<CreativePowers.GodmodePower>().SetEnabledState(Main.LocalPlayer.whoAmI, true);
                Main.ingameOptionsWindow = false;
                _screenPositionOffset = Vector2.Zero;
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
            Clipboard.TryAddTiles(tilesToCopy.AsDictionary());
        }

        public void Redo()
        {
            // same as undo pretty much
            UndoHistory.Add(new TileCollection());
            foreach (var tile in RedoHistory[RedoHistory.Count - 1].AsDictionary())
            {
                UndoHistory[UndoHistory.Count - 1].TryAddTile(new Point(tile.Key.X, tile.Key.Y), new TileCopy(Main.tile[tile.Key.X, tile.Key.Y]));
                Main.tile[tile.Key.X, tile.Key.Y].CopyFrom(tile.Value.GetAsTile());
            }
            RedoHistory.RemoveAt(RedoHistory.Count - 1);
        }

        public void Undo()
        {
            // so we can redo
            RedoHistory.Add(new TileCollection());

            // go over all the tiles in the most recently added tile collection to undo
            foreach (var tile in UndoHistory[UndoHistory.Count - 1].AsDictionary())
            {
                // add the tile to redo
                RedoHistory[RedoHistory.Count - 1].TryAddTile(new Point(tile.Key.X, tile.Key.Y), new TileCopy(Main.tile[tile.Key.X, tile.Key.Y]));

                // set the tile at the coordinates to the tile data we stored
                Main.tile[tile.Key.X, tile.Key.Y].CopyFrom(tile.Value.GetAsTile());
            }

            // remove the most recent tile collection since we just undid it
            UndoHistory.RemoveAt(UndoHistory.Count - 1);
        }

        public void Delete(TileCollection tilesToDelete, bool save = false) // delete area
        {
            // for undo history
            TileCollection tileColl = new TileCollection();

            // faster to get index with list
            List<KeyValuePair<Point, TileCopy>> tiles = tilesToDelete.AsDictionary().ToList();

            // go over all the tiles in the most recently added tile collection to undo
            for (int i = 0; i < tiles.Count; i++)
            {
                int x = tiles[i].Key.X;
                int y = tiles[i].Key.Y;

                // add tile to history
                tileColl.TryAddTile(new Point(x, y), new TileCopy(Main.tile[x, y]));

                // will only delete walls
                if (TIGWESettings.ShouldPasteWalls)
                {
                    Main.tile[x, y].WallType = 0;
                }

                // will only delete tiles
                if (TIGWESettings.ShouldPasteTiles)
                {
                    ((Tile)Main.tile[x, y]).HasTile = false;
                    Main.tile[x, y].TileType = 0;
                }

                // deletes liquid
                if (TIGWESettings.ShouldPasteLiquid)
                {
                    Main.tile[x, y].LiquidAmount = 0;
                }
            }

            if (save == true)
            {
                UndoHistory.Add(tileColl);
            }
        }

        public void Replace(TileCollection tilesToChange, TileCopy tileFrom, TileCopy tileTo, bool save = true)
        {
            // for undo history
            TileCollection tileColl = new TileCollection();

            // since we update the tiles as we replace we take a copy of the selected area for history before we replace anything
            foreach (var tile in tilesToChange.AsDictionary())
            {
                int x = tile.Key.X;
                int y = tile.Key.Y;

                // add tile to history
                tileColl.TryAddTile(new Point(x, y), new TileCopy(Main.tile[x, y]));
            }
            if (save == true)
            {
                UndoHistory.Add(tileColl);
            }

            // go over the collection
            foreach (var tile in tilesToChange.AsDictionary())
            {
                int x = tile.Key.X;
                int y = tile.Key.Y;
                TileCopy temp = new TileCopy(Main.tile[x, y]);

                if (tile.Value.WallType == tileFrom.WallType)
                {
                    // get some temporary values
                    temp.CopyWallData(tileTo.GetAsTile());
                    temp.CopyTileData(Main.tile[x, y]); // copy tile data of tiles at the location we're pasting to so we dont replace them
                    temp.CopyWireData(Main.tile[x, y]); // copy tile data of wires at the location we're pasting to so we dont replace them

                    // set and update walls
                    Main.tile[x, y].CopyFrom(temp.GetAsTile());
                    WorldGen.SquareWallFrame(x, y, true);
                }

                if (tile.Value.TileType == tileFrom.TileType && tile.Value.HasTile == tileFrom.HasTile)
                {
                    // get some temporary values
                    temp.CopyTileData(tileTo.GetAsTile()); // copy original tile data in case we replaced before when pasting walls
                    temp.CopyWallData(Main.tile[x, y]);
                    temp.CopyWireData(Main.tile[x, y]);

                    // replace tile
                    Main.tile[x, y].CopyFrom(temp.GetAsTile());

                    // squareframe but with noBreak
                    // update tiles
                    bool isTileFrameImportant = Main.tileFrameImportant[tileTo.TileType];
                    WorldGen.TileFrame(x, y, true, !isTileFrameImportant);
                    WorldGen.TileFrame(x + 1, y, true, !isTileFrameImportant);
                    WorldGen.TileFrame(x - 1, y, true, !isTileFrameImportant);
                    WorldGen.TileFrame(x, y + 1, true, !isTileFrameImportant);
                    WorldGen.TileFrame(x, y - 1, true, !isTileFrameImportant);
                    WorldGen.TileFrame(x + 1, y + 1, true, !isTileFrameImportant);
                    WorldGen.TileFrame(x - 1, y + 1, true, !isTileFrameImportant);
                    WorldGen.TileFrame(x - 1, y - 1, true, !isTileFrameImportant);
                    WorldGen.TileFrame(x + 1, y - 1, true, !isTileFrameImportant);
                }
            }
        }
    }
}
