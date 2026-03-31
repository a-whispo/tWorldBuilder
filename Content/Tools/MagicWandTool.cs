using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class MagicWandTool : FillTool, ISelectionTool
    {
        private TileCollection _selection;

        public MagicWandTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/MagicWandTool"));
            ToggleToolButton.HoverText = "Magic Wand";
            _selection = new TileCollection();
        }

        public TileCollection GetSelection()
        {
            return _selection;
        }

        public void ResetSelection()
        {
            _selection.Clear();
        }

        public override string GetInfoText()
        {
            return $"[c/EAD87A:Count:] {_selection.Count}";
        }

        protected override void OnFill(TileCollection tiles)
        {
            base.OnFill(tiles);
            _selection.TryAddTiles(tiles);
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;

            // left click
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
                // if ctrl is pressed down, keep adding tiles to our selection
                if (!PlayerInput.GetPressedKeys().Contains(Keys.LeftControl))
                {
                    _selection.Clear();
                }
            }

            base.PostUpdateInput();

            // right click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                // undo selection
                _selection.Clear();
            }
        }
    }
}
