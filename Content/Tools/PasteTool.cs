using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class PasteTool : Tool
    {
        private static int _pastingCorner = 0;
        private KeyboardState _oldState;

        public PasteTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/PasteTool"));
            ToggleToolButton.HoverText = "Paste";
        }

        public override string GetInfoText()
        {
            return $"[c/EAD87A:Count:] {EditorSystem.Local.Clipboard.Count}";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // dont draw anything if we dont have a clipboard
            if (EditorSystem.Local.Clipboard.Count != 0)
            {
                // paste preview
                Rectangle selection = GetCurrentSelectionRectangle();
                Color color = EditorSystem.Local.Settings.ToolColor;
                DrawUtils.DrawTileCollection(EditorSystem.Local.Clipboard, new Point(selection.X, selection.Y), EditorSystem.Local.Settings.ShouldPasteTiles, EditorSystem.Local.Settings.ShouldPasteWalls, EditorSystem.Local.Settings.ShouldPasteLiquid, EditorSystem.Local.Settings.ShouldPasteWires);
                DrawUtils.DrawTileCollectionOutline(EditorSystem.Local.Clipboard, new Point(selection.X, selection.Y), color);
                DrawUtils.DrawMiscOptions(selection, EditorSystem.Local.Settings.ShowCenterLines, EditorSystem.Local.Settings.ShowMeasureLines);
            }  
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;

            // left click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                EditorSystem.Local.CurrentTool = null;
            }

            // keybinds that need ctrl
            if (PlayerInput.GetPressedKeys().Contains(Keys.LeftControl))
            {
                // paste
                if (Keybinds.PasteMK.JustPressed && EditorSystem.Local.Clipboard != null)
                {
                    Rectangle bounds = GetCurrentSelectionRectangle();
                    ToolUtils.Paste(EditorSystem.Local.Clipboard, new Point16(bounds.X, bounds.Y), true, EditorSystem.Local.Settings.ShouldUpdateDrawnTiles);
                    TerrariaInGameWorldEditor.NewText($"Pasted.");
                }

                // rotate
                if (Keybinds.RotateMK.JustPressed)
                {
                    EditorSystem.Local.Clipboard = EditorSystem.Local.Clipboard.To90DegAntiClockwise();
                }

                // mirror
                if (Keybinds.MirrorMK.JustPressed)
                {
                    EditorSystem.Local.Clipboard = EditorSystem.Local.Clipboard.ToMirrored();
                }

                // change what corner you're pasting from
                if (Keybinds.ChangeCornerOnPasteMK.JustPressed)
                {
                    _pastingCorner = (_pastingCorner == 8 ? 0 : _pastingCorner + 1);
                }
            }

            // check arrow keys input
            // improve this at some point
            KeyboardState currentState = Keyboard.GetState();

            if (currentState.IsKeyDown(Keys.Up) && _oldState.IsKeyUp(Keys.Up))
            {
                Mouse.SetPosition(Mouse.GetState().X, Mouse.GetState().Y - 16);
            }

            if (currentState.IsKeyDown(Keys.Down) && _oldState.IsKeyUp(Keys.Down))
            {
                Mouse.SetPosition(Mouse.GetState().X, Mouse.GetState().Y + 16);
            }

            if (currentState.IsKeyDown(Keys.Left) && _oldState.IsKeyUp(Keys.Left))
            {
                Mouse.SetPosition(Mouse.GetState().X - 16, Mouse.GetState().Y);
            }

            if (currentState.IsKeyDown(Keys.Right) && _oldState.IsKeyUp(Keys.Right))
            {
                Mouse.SetPosition(Mouse.GetState().X + 16, Mouse.GetState().Y);
            }
            _oldState = Keyboard.GetState();
        }

        private Rectangle GetCurrentSelectionRectangle()
        {
            Rectangle bounds;
            Point p1 = new Point(Player.tileTargetX, Player.tileTargetY);
            Point p2 = new Point(Player.tileTargetX + EditorSystem.Local.Clipboard.GetWidth() - 1, Player.tileTargetY + EditorSystem.Local.Clipboard.GetHeight() - 1);
            bounds = ToolUtils.GetRectangleFromPoints(p1, p2);
            int widthOffset = bounds.Width / 2;
            int heightOffset = bounds.Height / 2;
            switch (_pastingCorner)
            {
                case 0: // 0 is top left
                    break;
                case 1: // 1 is top middle
                    bounds.X = bounds.X - widthOffset;
                    break;
                case 2: // 2 is top right
                    bounds.X = bounds.X - bounds.Width;
                    break;
                case 3: // 3 is middle left
                    bounds.Y = bounds.Y - heightOffset;
                    break;
                case 4: // 4 is middle middle
                    bounds.X = bounds.X - widthOffset;
                    bounds.Y = bounds.Y - heightOffset;
                    break;
                case 5: // 5 is middle right
                    bounds.X = bounds.X - bounds.Width;
                    bounds.Y = bounds.Y - heightOffset;
                    break;
                case 6: // 6 is bottom left
                    bounds.Y = bounds.Y - bounds.Height;
                    break;
                case 7: // 7 is bottom middle
                    bounds.X = bounds.X - widthOffset;
                    bounds.Y = bounds.Y - bounds.Height;
                    break;
                case 8: // 8 is bottom right
                    bounds.X = bounds.X - bounds.Width;
                    bounds.Y = bounds.Y - bounds.Height;
                    break;
                default: // default at top left if something goes wrong
                    break;
            }
            return bounds;
        }
    }
}