using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.CheckBox;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class MagicWandTool : FillTool, ISelectionTool
    {
        private TileCollection _selection;
        private TIGWECheckBox _quickSelectCheckBox;
        private bool _failedLastFill = false;

        public MagicWandTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/MagicWandTool", AssetRequestMode.ImmediateLoad));
            ToggleToolButton.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Tools.MagicWandTool.HoverText");
            _selection = new TileCollection();

            // hold down left mouse to quick select
            _quickSelectCheckBox = new TIGWECheckBox(false);
            Settings.Add((LocalizationUtils.GetTextValue("Tools.MagicWandTool.Settings.QuickSelect"), _quickSelectCheckBox));
            _selectCondition = () =>
            {
                if (_quickSelectCheckBox.IsChecked)
                {
                    Point16 coord = new Point16(Player.tileTargetX, Player.tileTargetY);
                    return (PlayerInput.Triggers.Current.MouseLeft && (!_selection.ContainsCoord(coord) || (PlayerInput.GetPressedKeys().Contains(Keys.LeftShift) && _selection.ContainsCoord(coord))));
                }
                return PlayerInput.Triggers.JustPressed.MouseLeft;
            };
        }

        public TileCollection GetSelection()
        {
            return _selection;
        }

        public void SetSelection(TileCollection selection)
        {
            _selection = selection ?? _selection;
        }

        public void ResetSelection()
        {
            _selection.Clear();
        }

        public override string GetInfoText()
        {
            return LocalizationUtils.GetTextValue("Tools.MagicWandTool.InfoText", _selection.Count);
        }

        protected override void OnFill(TileCollection tiles)
        {
            base.OnFill(tiles);
            if (PlayerInput.GetPressedKeys().Contains(Keys.LeftShift) && _selection.Count > 0)
            {
                _selection.TryRemoveTiles(tiles);
            }
            else
            {
                _selection.TryAddTiles(tiles);
            }
            _failedLastFill = false;
        }

        protected override void OnFillFailed()
        {
            base.OnFillFailed();
            // only show the message if we failed with a different amount of tiles than last time, this way if we hit the tile cap it wont spam the message every frame
            if (!(_quickSelectCheckBox.IsChecked && _failedLastFill))
            {
                TerrariaInGameWorldEditor.NewText(LocalizationUtils.GetTextValue("Tools.MagicWandTool.Messages.AreaTooBig"));
            }
            _failedLastFill = true;
        }

        public override void PostUpdateInput()
        {
            // left click
            if (PlayerInput.Triggers.JustPressed.MouseLeft)
            {
                // if ctrl is pressed down, keep adding tiles to our selection, if shift is pressed down remove tile from our selection
                if (!PlayerInput.GetPressedKeys().Contains(Keys.LeftControl) && !PlayerInput.GetPressedKeys().Contains(Keys.LeftShift))
                {
                    _selection.Clear();
                }
            }

            base.PostUpdateInput();

            // right click
            if (PlayerInput.Triggers.JustPressed.MouseRight)
            {
                _selection.Clear();
            }
        }
    }
}
