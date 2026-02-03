using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor.Windows.Settings;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class BoxSelectionTool : SelectionTool
    {
        // hovering
        public bool HoveringAny => _hoveringLeft || _hoveringRight || _hoveringTop || _hoveringBottom;
        private bool _hoveringRight = false;
        private bool _hoveringLeft = false;
        private bool _hoveringTop = false;
        private bool _hoveringBottom = false;

        // points
        private Point _point1;
        private bool _point1placed = false;
        private Point _point2;
        private bool _point2placed = false;
        private bool _canChangePoint2X = true;
        private bool _canChangePoint2Y = true;
        private int _oldWidth = 0;
        private int _oldHeight = 0;

        public BoxSelectionTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/SelectTool"));
            ToggleToolButton.HoverText = "Box Selection";
        }

        public override string GetInfoText()
        {
            Rectangle selection = new Rectangle(0, 0, 0, 0);

            if (_point1placed)
            {
                // temp point at the cursor
                if (!_point2placed)
                {
                    _point2 = new Point(_canChangePoint2X ? Player.tileTargetX : _point1.X + _oldWidth, _canChangePoint2Y ? Player.tileTargetY : _point1.Y + _oldHeight);
                }
                selection = ToolUtils.GetRectangleFromPoints(_point1, _point2);
            }

            return $"[c/EAD87A:Width:] {selection.Width}, [c/EAD87A:Height:] {selection.Height}";
        }

        public override void ResetSelection()
        {
            // unplace both points if you right click, results in selection going away
            _point1placed = false;
            _point2placed = false;
            _canChangePoint2X = true;
            _canChangePoint2Y = true;
            _hoveringTop = false;
            _hoveringBottom = false;
            _hoveringLeft = false;
            _hoveringRight = false;
            _selection.Clear();
        }

        public override TileCollection GetSelection()
        {
            // no selection
            if (!_point1placed || !_point2placed)
            {
                return null;
            }            
            return _selection;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_point2placed)
            {
                _point2 = new Point(_canChangePoint2X ? Player.tileTargetX : _point1.X + _oldWidth, _canChangePoint2Y ? Player.tileTargetY : _point1.Y + _oldHeight);
            }
            Rectangle selection = ToolUtils.GetRectangleFromPoints(_point1, _point2);

            // draw a rectangle outline while we dont have anything actually selected
            if (_point1placed && !_point2placed)
            {
                DrawUtils.DrawRectangleOutline(selection, TIGWESettings.ToolColor);
            }

            // draw hovering side highlight
            if (HoveringAny && _point1placed && _point2placed)
            {
                spriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
                if (_hoveringTop)
                {
                    spriteBatch.Draw(DrawUtils.BlankTexture2D.Value, new Rectangle(selection.X * 16 - 2 - (int)Main.screenPosition.X, selection.Y * 16 - 2 - (int)Main.screenPosition.Y, selection.Width * 16 + 4, 6), Color.White * 0.8f);
                }
                if (_hoveringLeft)
                {
                    spriteBatch.Draw(DrawUtils.BlankTexture2D.Value, new Rectangle(selection.X * 16 - (int)Main.screenPosition.X - 2, selection.Y * 16 - 2 - (int)Main.screenPosition.Y, 6, selection.Height * 16 + 4), Color.White * 0.8f);
                }
                if (_hoveringRight)
                {
                    spriteBatch.Draw(DrawUtils.BlankTexture2D.Value, new Rectangle(selection.X * 16 + selection.Width * 16 - 4 - (int)Main.screenPosition.X, selection.Y * 16 - 2 - (int)Main.screenPosition.Y, 6, selection.Height * 16 + 4), Color.White * 0.8f);
                }
                if (_hoveringBottom)
                {
                    spriteBatch.Draw(DrawUtils.BlankTexture2D.Value, new Rectangle(selection.X * 16 - 2 - (int)Main.screenPosition.X, selection.Y * 16 + selection.Height * 16 - 4 - (int)Main.screenPosition.Y, selection.Width * 16 + 4, 6), Color.White * 0.8f);
                }
                spriteBatch.End();
            }
        }

        public override void Update()
        {
            if (!_point1placed)
            {
                return;
            }

            if (_point2placed)
            {
                // get what side we're hovering over
                // working with zoom makes this a bit more complicated
                Rectangle selection = ToolUtils.GetRectangleFromPoints(_point1, _point2);
                selection = new Rectangle((int)(selection.X * 16 - (int)Main.screenPosition.X), (int)(selection.Y * 16 - (int)Main.screenPosition.Y), (int)(selection.Width * 16), (int)(selection.Height * 16));
                Vector2 mouse = new Vector2((int)(Main.mouseX), (int)(Main.mouseY)); // mouse position
                _hoveringLeft = Math.Abs(mouse.X - selection.X) < 8f && (mouse.Y > selection.Y) && (mouse.Y < selection.Y + selection.Height);
                _hoveringRight = Math.Abs(mouse.X - (selection.X + selection.Width)) < 8f && (mouse.Y > selection.Y) && (mouse.Y < selection.Y + selection.Height);
                _hoveringTop = Math.Abs(mouse.Y - selection.Y) < 8f && (mouse.X > selection.X) && (mouse.X < selection.X + selection.Width);
                _hoveringBottom = Math.Abs(mouse.Y - (selection.Y + selection.Height)) < 8f && (mouse.X > selection.X) && (mouse.X < selection.X + selection.Width);
            } 
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;

            // left click
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
            {
                if (HoveringAny)
                {
                    // we are hovering over a side, allow changing only that axis
                    _canChangePoint2X = _hoveringLeft || _hoveringRight;
                    _canChangePoint2Y = _hoveringTop || _hoveringBottom;

                    // save old width and height to maintain size when only changing one axis
                    _oldWidth = _hoveringRight ? Math.Abs(_point2.X - _point1.X) : -Math.Abs(_point2.X - _point1.X);
                    _oldHeight = _hoveringBottom ? Math.Abs(_point2.Y - _point1.Y) : -Math.Abs(_point2.Y - _point1.Y);

                    // figure out where to place new point 1
                    Rectangle selection = ToolUtils.GetRectangleFromPoints(_point1, _point2);
                    int newPoint1X = _hoveringRight ? selection.X : selection.X + selection.Width - 1;
                    int newPoint1Y = _hoveringBottom ? selection.Y : selection.Y + selection.Height - 1;
                    _point1 = new Point(newPoint1X, newPoint1Y);

                    // set to false so the tool places point 2 again
                    _point2placed = false;

                    _hoveringTop = false;
                    _hoveringBottom = false;
                    _hoveringLeft = false;
                    _hoveringRight = false;
                } 
                else
                {
                    if (!_point1placed || (_point1placed && _point2placed)) // if both points have aleady been placed, reset them and place point 1 again
                    {
                        _point1 = new Point(Player.tileTargetX, Player.tileTargetY); // get mouse coordinates in the world and save them to point1
                        _point2placed = false;
                        _point1placed = true;
                    }
                    else
                    {
                        if (!_point2placed)
                        {
                            _point2 = new Point(_canChangePoint2X ? Player.tileTargetX : _point1.X + _oldWidth, _canChangePoint2Y ? Player.tileTargetY : _point1.Y + _oldHeight);
                            _point2placed = true;

                            // update selection
                            _selection.Clear();
                            Rectangle selection = ToolUtils.GetRectangleFromPoints(_point1, _point2);
                            int width = selection.Width;
                            int height = selection.Height;
                            for (int x = 0; x < width; x++)
                            {
                                for (int y = 0; y < height; y++)
                                {
                                    int newX = x + selection.X;
                                    int newY = y + selection.Y;
                                    _selection.TryAddTile(new Point16(newX, newY), new TileCopy(Main.tile[newX, newY], newX, newY));
                                }
                            }
                        }
                    }
                }
            }

            // right click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                ResetSelection();
            }
        }
    }
}