using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.GameInput;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class ShapesTool : Tool
    {
        // points
        private Point _point1;
        private bool _point1placed = false;
        private Point _point2;
        private bool _point2placed = false;

        private int _d = 4;
        private KeyboardState _oldState;
        private enum Mode
        {
            Rectangle,
            RectangleFilled,
            Circle,
            CircleFilled
        }
        private Mode mode = Mode.CircleFilled;

        public ShapesTool() : base("TerrariaInGameWorldEditor/UI/UIImages/ShapesTool", "Shapes")
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Main.blockMouse = true;
            Rectangle selection = new Rectangle(0, 0, 0, 0);
            // selection bounds preview
            if (_point1placed)
            {
                if (!_point2placed) // if point2 hasnt been placed yet, put a temporary point2 that follows the mouse so you can see your selection
                {
                    _point2 = new Point(Player.tileTargetX, Player.tileTargetY);
                }
                selection = ToolUtils.GetRectangleFromPoints(_point1, _point2);
                selection = new Rectangle(selection.X, selection.Y, selection.Width, selection.Height);
                Color color = TIGWEUISystem.Settings.ToolColor;

                TileCollection tiles;
                switch (mode)
                {
                    case Mode.Rectangle:
                        tiles = GetRectangleTileCollection(selection.Width, selection.Height, _d);
                        break;
                    case Mode.RectangleFilled:
                        tiles = GetRectangleFilledTileCollection(selection.Width, selection.Height);
                        break;
                    case Mode.CircleFilled:
                        tiles = GetEllipseFilledTileCollection(selection.Width, selection.Height);
                        break;
                    case Mode.Circle:
                        tiles = GetEllipseTileCollection(selection.Width, selection.Height, _d);
                        break;
                    default:
                        tiles = GetRectangleTileCollection(selection.Width, selection.Height, _d);
                        break;
                }
                DrawUtils.DrawTileCollection(tiles, new Point(selection.X, selection.Y));
                DrawUtils.DrawTileCollectionOutline(tiles, new Point(selection.X, selection.Y), color);
                DrawUtils.DrawMiscOptions(selection, TIGWEUISystem.Settings.ShowCenterLines, TIGWEUISystem.Settings.ShowMeasureLines);
                if (_point2placed)
                {
                    ToolUtils.Paste(tiles, new Point(selection.X, selection.Y), true, TIGWEUISystem.Settings.ShouldUpdateDrawnTiles);
                    _point1placed = false;
                    _point2placed = false;
                }
            }
            InfoText = $"[c/EAD87A:Shape type:] {mode}, [c/EAD87A:Size:] {_d}, [c/EAD87A:Width:] {selection.Width}, [c/EAD87A:Height:] {selection.Height}";
        }

        public override void PostUpdateInput()
        {
            // left click
            if (Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface)
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
                        _point2 = new Point(Player.tileTargetX, Player.tileTargetY);
                        _point2placed = true;
                    }
                }
            }

            // right click
            if (Main.mouseRight && Main.mouseRightRelease && !Main.LocalPlayer.mouseInterface)
            {
                // unplace both points if you right click, results in selection going away
                _point1placed = false;
                _point2placed = false;
            }

            // check arrow keys input // swap mode with left/right
            KeyboardState currentState = Keyboard.GetState();
            if (currentState.IsKeyDown(Keys.Left) && _oldState.IsKeyUp(Keys.Left))
            {
                switch (mode)
                {
                    case Mode.Rectangle:
                        mode = Mode.RectangleFilled;
                        break;
                    case Mode.RectangleFilled:
                        mode = Mode.Circle;
                        break;
                    case Mode.Circle:
                        mode = Mode.CircleFilled;
                        break;
                    case Mode.CircleFilled:
                        mode = Mode.Rectangle;
                        break;
                }
            }
            if (currentState.IsKeyDown(Keys.Right) && _oldState.IsKeyUp(Keys.Right))
            {
                switch (mode)
                {
                    case Mode.Rectangle:
                        mode = Mode.CircleFilled;
                        break;
                    case Mode.RectangleFilled:
                        mode = Mode.Rectangle;
                        break;
                    case Mode.Circle:
                        mode = Mode.RectangleFilled;
                        break;
                    case Mode.CircleFilled:
                        mode = Mode.Circle;
                        break;
                }
            }
            _oldState = Keyboard.GetState();

            // change size with mouse wheel
            if (Keybinds.Key1MK.Current || Keybinds.Key1MK.GetAssignedKeys().Count < 1)
            {
                PlayerInput.LockVanillaMouseScroll($"{TerrariaInGameWorldEditor.MODNAME}/Shapes");
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
                _d = Math.Max(_d, 1);
            }
        }

        private TileCollection GetEllipseFilledTileCollection(int width, int height)
        {
            // mid point algo pretty much
            void PlotQuadrants(TileCollection tileColl, int x, int y, int xOffset, int yOffset, int width, int height)
            {
                while (y >= 0)
                {
                    int mx = (width % 2 == 0) ? -x - 1 : -x;
                    int my = (height % 2 == 0) ? -y - 1 : -y;

                    PlotTile(tileColl, x + xOffset, y + yOffset);
                    PlotTile(tileColl, mx + xOffset, y + yOffset);
                    PlotTile(tileColl, x + xOffset, my + yOffset);
                    PlotTile(tileColl, mx + xOffset, my + yOffset);
                    y--;
                }
            }

            void PlotTile(TileCollection tileColl, int x, int y)
            {
                Point coord = new Point(x, y);
                if (!tileColl.ContainsCoord(coord))
                {
                    tileColl.TryAddTile(coord, EditorSystem.Local.SelectedTile);
                }
            }

            // edge case for when height is 1 or 2
            if (height == 1 || height == 2 || width == 1 || width == 2)
            {
                return GetRectangleFilledTileCollection(width, height);
            }

            bool shouldRotate = false;
            if (width > height)
            {
                shouldRotate = true;
                int temp = width;
                width = height;
                height = temp;
            }

            TileCollection tileColl = new TileCollection();

            // determine center offsets
            int xOffset = (width % 2 == 0) ? 1 : 0;
            int yOffset = (height % 2 == 0) ? 1 : 0;

            width = (width % 2 == 0) ? width - 2 : width;
            height = (height % 2 == 0) ? height - 2 : height;

            // calculate radius and squares
            int a = width / 2;
            int b = height / 2;
            int a2 = a * a;
            int b2 = b * b;

            // starting position
            int x = 0;
            int y = b;

            int twoA2 = 2 * a2;
            int twoB2 = 2 * b2;
            int dx = twoB2 * x;
            int dy = twoA2 * y;

            // slope < 1
            int p = (int)(b2 - a2 * b + 0.25 * a2);
            while (dx < dy)
            {
                PlotQuadrants(tileColl, x, y, xOffset, yOffset, width, height);
                x++;
                dx += twoB2;
                if (p < 0)
                {
                    p += b2 + dx;
                }
                else
                {
                    y--;
                    dy -= twoA2;
                    p += b2 + dx - dy;
                }
            }

            // slope >= 1
            p = (int)(b2 * (x + 0.5) * (x + 0.5) + a2 * (y - 1) * (y - 1) - a2 * b2);
            while (y >= 0)
            {
                PlotQuadrants(tileColl, x, y, xOffset, yOffset, width, height);
                y--;
                dy -= twoA2;
                if (p > 0)
                {
                    p += a2 - dy;
                }
                else
                {
                    x++;
                    dx += twoB2;
                    p += a2 - dy + dx;
                }
            }

            if (shouldRotate)
            {
                return tileColl.To90DegAntiClockwise();
            }
            else
            {
                return tileColl;
            }
        }

        private TileCollection GetRectangleTileCollection(int width, int height, int size)
        {
            TileCollection tileColl = new TileCollection();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((0 <= x && x <= size - 1) || ((width - size) <= x && x <= (width - 1)))
                    {
                        tileColl.TryAddTile(new Point(x, y), EditorSystem.Local.SelectedTile);
                    }
                    else
                    {
                        if ((0 <= y && y <= size - 1) || ((height - size) <= y && y <= (height - 1)))
                        {
                            tileColl.TryAddTile(new Point(x, y), EditorSystem.Local.SelectedTile);
                        }
                    }
                }
            }

            return tileColl;
        }

        private TileCollection GetRectangleFilledTileCollection(int width, int height)
        {
            TileCollection tileColl = new TileCollection();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tileColl.TryAddTile(new Point(x, y), EditorSystem.Local.SelectedTile);
                }
            }

            return tileColl;
        }

        private TileCollection GetEllipseTileCollection(int width, int height, int size)
        {
            // mid point algo pretty much
            void PlotQuadrants(TileCollection tileColl, int x, int y, int xOffset, int yOffset, int width, int height, int size)
            {
                int tempy = y;
                while (tempy >= y + 1 - size)
                {
                    if (tempy < 0)
                    {
                        break;
                    }
                    int mx = (width % 2 == 0) ? -x - 1 : -x;
                    int my = (height % 2 == 0) ? -tempy - 1 : -tempy;

                    PlotTile(tileColl, x + xOffset, tempy + yOffset);
                    PlotTile(tileColl, mx + xOffset, tempy + yOffset);
                    PlotTile(tileColl, x + xOffset, my + yOffset);
                    PlotTile(tileColl, mx + xOffset, my + yOffset);
                    tempy--;
                }

                int tempx = x;
                while (tempx >= x + 1 - size)
                {
                    if (tempx < 0)
                    {
                        break;
                    }
                    int mx = (width % 2 == 0) ? -tempx - 1 : -tempx;
                    int my = (height % 2 == 0) ? -y - 1 : -y;

                    PlotTile(tileColl, tempx + xOffset, y + yOffset);
                    PlotTile(tileColl, mx + xOffset, y + yOffset);
                    PlotTile(tileColl, tempx + xOffset, my + yOffset);
                    PlotTile(tileColl, mx + xOffset, my + yOffset);
                    tempx--;
                }
            }

            void PlotTile(TileCollection tileColl, int x, int y)
            {
                Point coord = new Point(x, y);
                if (!tileColl.ContainsCoord(coord))
                {
                    tileColl.TryAddTile(coord, EditorSystem.Local.SelectedTile);
                }
            }

            // edge case for when height is 1 or 2
            if (height == 1 || height == 2 || width == 1 || width == 2)
            {
                return GetRectangleFilledTileCollection(width, height);
            }

            bool shouldRotate = false;
            if (width > height)
            {
                shouldRotate = true;
                int temp = width;
                width = height;
                height = temp;
            }

            TileCollection tileColl = new TileCollection();

            // determine center offsets
            int xOffset = (width % 2 == 0) ? 1 : 0;
            int yOffset = (height % 2 == 0) ? 1 : 0;

            width = (width % 2 == 0) ? width - 2 : width;
            height = (height % 2 == 0) ? height - 2 : height;

            // calculate radius and squares
            int a = width / 2;
            int b = height / 2;
            int a2 = a * a;
            int b2 = b * b;

            // starting position
            int x = 0;
            int y = b;

            int twoA2 = 2 * a2;
            int twoB2 = 2 * b2;
            int dx = twoB2 * x;
            int dy = twoA2 * y;

            // slope < 1
            int p = (int)(b2 - a2 * b + 0.25 * a2);
            while (dx < dy)
            {
                PlotQuadrants(tileColl, x, y, xOffset, yOffset, width, height, size);
                x++;
                dx += twoB2;
                if (p < 0)
                {
                    p += b2 + dx;
                }
                else
                {
                    y--;
                    dy -= twoA2;
                    p += b2 + dx - dy;
                }
            }

            // slope >= 1
            p = (int)(b2 * (x + 0.5) * (x + 0.5) + a2 * (y - 1) * (y - 1) - a2 * b2);
            while (y >= 0)
            {
                PlotQuadrants(tileColl, x, y, xOffset, yOffset, width, height, size);
                y--;
                dy -= twoA2;
                if (p > 0)
                {
                    p += a2 - dy;
                }
                else
                {
                    x++;
                    dx += twoB2;
                    p += a2 - dy + dx;
                }
            }

            if (shouldRotate)
            {
                return tileColl.To90DegAntiClockwise();
            }
            else
            {
                return tileColl;
            }
        }
    }
}
