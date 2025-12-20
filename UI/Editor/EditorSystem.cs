using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Content;
using TerrariaInGameWorldEditor.Content.Tools;
using TerrariaInGameWorldEditor.UI.TIGWEUI;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;

namespace TerrariaInGameWorldEditor.UI.Editor
{
    internal class EditorSystem : ModSystem
    {
        // local instance
        public static EditorSystem Local { get; private set; }

        // ui
        private EditorUIState _mainScreen;
        private UserInterface _mainScreenUI;
        private SpriteBatch _spriteBatch;

        // tools
        public List<Tool> Tools { get; } = new List<Tool> { new BrushTool(), new LineTool(), new ShapesTool(), new PaintBucketTool(), new TilePickerTool(), new BoxSelectionTool(), new MagicWandTool(), new LassoTool() };
        private PasteTool _pasteTool = new PasteTool();
        private Tool _currentTool;
        public Tool CurrentTool { // current selected tool
            get {
                return _currentTool;
            }
            set {
                _currentTool = value;
                _mainScreen.RecalculateToolSettings();
            }
        }

        // editing
        public TileCollection Clipboard { get; set; } = new TileCollection(); // current clipboard
        public TileCollection CurrentSelection { get; set; } = new TileCollection(); // current selection
        public TileCopy SelectedTile { get; set; } // current selected tile

        // redo undo
        public List<TileCollection> UndoHistory { get; set; } = new List<TileCollection>();
        public List<TileCollection> RedoHistory { get; set; } = new List<TileCollection>();

        public override void OnModLoad()
        {
            base.OnModLoad();
            Local = this;
        }

        public override void PostSetupContent()
        {
            base.PostSetupContent();
            _mainScreen = new EditorUIState();
            _mainScreen.Activate();
            _mainScreenUI = new UserInterface();

            // for testing
            Tile tile = new Tile();
            tile.HasTile = true;
            tile.TileType = 0;
            tile.WallType = 0;
            tile.TileFrameX = 18;
            tile.TileFrameY = 18;
            SelectedTile = new TileCopy(tile);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // update UI
            if (_mainScreen.Visible)
            {
                if (_mainScreenUI.CurrentState == null)
                {
                    _mainScreenUI.SetState(_mainScreen);
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
                _mainScreenUI.Update(gameTime);

                // restore originals
                Main.UIScale = tempUIScale;
                Main.screenWidth = tempWidth;
                Main.screenHeight = tempHeight;
            }
            else
            {
                _mainScreenUI.SetState(null);
                TIGWEUISystem.Local.ShouldRenderUI = false;
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) // layer stuff
        {
            if (_mainScreen.Visible)
            {
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
                        if (_mainScreenUI.CurrentState != null)
                        {
                            // setup spritebatch if we havent yet
                            _spriteBatch ??= new SpriteBatch(Main.graphics.GraphicsDevice);

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
                            _mainScreenUI.Draw(_spriteBatch, Main.gameTimeCache);
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
            base.ModifyInterfaceLayers(layers);
        }

        public override void PostUpdateInput()
        {
            // update input for the ui
            if (_mainScreen.Visible)
            {
                _mainScreen.PostUpdateInput();
            }

            // toggle the main screen visibility if the keybind is pressed
            if (Keybinds.OpenEditorMK.JustPressed)
            {
                // close the ingame options window if its open
                Main.ingameOptionsWindow = false;
                _mainScreen.Visible = !_mainScreen.Visible;
            }

            // reset current tool if window is closed
            if (!_mainScreen.Visible)
            {
                CurrentTool = null;
            }

            // remove current selection if escape is pressed
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Escape) && _mainScreen.Visible)
            {
                CurrentSelection.Clear();
                if (CurrentTool is SelectionTool)
                {
                    ((SelectionTool)CurrentTool).Selection.Clear();
                }
            }

            // keybinds that need key 1 to be pressed down go in here. count is < 1 if theres no keybind set
            if (Keybinds.Key1MK.Current || Keybinds.Key1MK.GetAssignedKeys().Count < 1)
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
                        // copy and delete
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
                if (Keybinds.SaveMK.JustPressed && _mainScreen.Visible)
                {
                    TIGWEUISystem.Local.SaveUI.Visible = true;
                }
            }

            // delete
            if (Keybinds.DeleteMK.JustPressed)
            {
                if (CurrentSelection != null) // if we have a selection
                {
                    Delete(CurrentSelection, true); // delete
                }
            }

            base.PostUpdateInput();
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
                // x and y
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
                // get coords of tile
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
                // get coords of tile
                int x = tile.Key.X;
                int y = tile.Key.Y;

                // create a temporary copy
                TileCopy temp = new TileCopy(Main.tile[x, y]);

                if (tile.Value.WallType == tileFrom.WallType)
                {
                    // get some temporary values
                    temp.CopyWallData(tileTo.GetAsTile());
                    temp.CopyTileData(Main.tile[x, y]); // copy tile data of tiles at the location we're pasting to so we dont replace them
                    temp.CopyWireData(Main.tile[x, y]); // copy tile data of wires at the location we're pasting to so we dont replace them

                    // set walls
                    Main.tile[x, y].CopyFrom(temp.GetAsTile());

                    // update walls
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
