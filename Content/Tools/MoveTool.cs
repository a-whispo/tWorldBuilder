using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class MoveTool : Tool
    {
        private TileCollection _selectionCopy = new TileCollection();
        private Vector2 _offset = new Vector2();
        private Point _originPos = new Point();
        private bool _isDragging => _isDraggingRight || _isDraggingLeft;
        private bool _isDraggingRight = false;
        private bool _isDraggingLeft = false;

        public MoveTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/MoveTool", AssetRequestMode.ImmediateLoad));
            ToggleToolButton.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Tools.MoveTool.HoverText");
        }

        public override string GetInfoText()
        {
            if (_selectionCopy == null)
            {
                return "";
            }
            return LocalizationUtils.GetTextValue("Tools.MoveTool.InfoText", _selectionCopy.Count);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_isDragging && _selectionCopy != null)
            {
                Point point = new Point(Player.tileTargetX - (int)_offset.X, Player.tileTargetY - (int)_offset.Y);
                if (_isDraggingLeft)
                {
                    DrawUtils.DrawTileCollection(_selectionCopy, point);
                }
                DrawUtils.DrawTileCollectionOutline(_selectionCopy, point, EditorSystem.Local.Settings.ToolColor);
            }
        }

        public override void PostUpdateInput()
        {
            base.PostUpdateInput();
            if (EditorSystem.Local.CurrentSelection?.Count == 0 && PlayerInput.Triggers.JustPressed.MouseLeft)
            {
                TerrariaInGameWorldEditor.NewText(LocalizationUtils.GetTextValue("Tools.MoveTool.Messages.NoSelection"));
            }

            if (EditorSystem.Local.CurrentSelection?.Count > 0 && !_isDragging)
            {
                if (PlayerInput.Triggers.JustPressed.MouseLeft || PlayerInput.Triggers.JustPressed.MouseRight)
                {
                    _selectionCopy = ToolUtils.Copy(EditorSystem.Local.CurrentSelection);
                    _offset = new Vector2(Player.tileTargetX - _selectionCopy.GetMinX(), Player.tileTargetY - _selectionCopy.GetMinY());
                    _originPos = new Point(Player.tileTargetX, Player.tileTargetY);
                    EditorSystem.Local.CurrentSelection = null;
                    EditorSystem.Local.CanPaste = false;

                    if (PlayerInput.Triggers.JustPressed.MouseLeft)
                    {
                        ToolUtils.Delete(_selectionCopy, false);
                        _isDraggingLeft = true;
                    }
                    else
                    {
                        _isDraggingRight = true;
                    }
                }
            }

            if (PlayerInput.Triggers.JustReleased.MouseLeft && _isDraggingLeft)
            {
                if (_selectionCopy != null)
                {
                    // see what tile are affected
                    Point16 point = new Point16(Player.tileTargetX - (int)_offset.X, Player.tileTargetY - (int)_offset.Y);
                    TileCollection allTilesAffected = new TileCollection();
                    foreach (var tile in _selectionCopy)
                    {
                        int x = tile.Key.X + point.X - _selectionCopy.GetMinX();
                        int y = tile.Key.Y + point.Y - _selectionCopy.GetMinY();
                        allTilesAffected.TryAddTile(new Point16(x, y), new TileCopy(x, y));
                    }
                    ToolUtils.Paste(_selectionCopy, point, false);

                    // move selection
                    TileCollection newSelection = new TileCollection();
                    int offsetX = (_originPos.X - Player.tileTargetX);
                    int offsetY = (_originPos.Y - Player.tileTargetY);
                    foreach (var tile in _selectionCopy)
                    {
                        int x = tile.Key.X - offsetX;
                        int y = tile.Key.Y - offsetY;
                        newSelection.TryAddTile(new Point16(x, y), tile.Value);
                    }
                    EditorSystem.Local.CurrentSelection = newSelection;

                    // add everything to undo so the delete and paste can both be undone at the same time
                    _selectionCopy.TryAddTiles(allTilesAffected);
                    EditorSystem.Local.AddToUndoHistory(_selectionCopy);

                    _isDraggingLeft = false;
                    _selectionCopy = null;
                    EditorSystem.Local.CanPaste = true;
                }
            }

            if (PlayerInput.Triggers.JustReleased.MouseRight && _isDraggingRight)
            {
                if (_selectionCopy != null)
                {
                    // move selection
                    TileCollection _newSelection = new TileCollection();
                    int offsetX = (_originPos.X - Player.tileTargetX);
                    int offsetY = (_originPos.Y - Player.tileTargetY);
                    foreach (var tile in _selectionCopy)
                    {
                        int x = tile.Key.X - offsetX;
                        int y = tile.Key.Y - offsetY;
                        _newSelection.TryAddTile(new Point16(x, y), tile.Value);
                    }
                    EditorSystem.Local.CurrentSelection = _newSelection;

                    _isDraggingRight = false;
                    _selectionCopy = null;
                    EditorSystem.Local.CanPaste = true;
                }
            }
        }
    }
}
