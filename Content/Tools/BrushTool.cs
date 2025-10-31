using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.TIGWEUI;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class BrushTool : Tool
    {
        private Dictionary<Point, TileCopy> _currentDrawPreTilesPlaced = new Dictionary<Point, TileCopy>(); // the state of the tiles before we draw on them (for undo)
        private List<Point> _currentDrawLinePoints = new List<Point>(); // list of all the points the cursor passed over when we're drawing (used to make lines complete without gaps caused by 60 fps)
        private TileCollection _brush = new TileCollection(); // the brush itself, a collection of tiles in the shape of an ellipse
        private int _d = 4; // diameter of the brush

        public BrushTool() : base("TerrariaInGameWorldEditor/UI/UIImages/BrushTool", "Brush")
        {

        }

        public override void Update()
        {
            // update brush when needed
            if (_d != _brush.GetWidth() + 1 || _brush.AsDictionary().ToList()[0].Value != EditorSystem.Local.SelectedTile)
            {
                _brush.Clear();
                _brush.TryAddTiles(ToolUtils.GetEllipseFilledTileCollection(_d, _d, EditorSystem.Local.SelectedTile).AsDictionary());
            }
            InfoText = $"Size: {_d}";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                int width = _d - 2;
                int height = _d - 2;
                Point point = new Point(Player.tileTargetX - width / 2, Player.tileTargetY - height / 2);
                DrawUtils.DrawTileCollection(_brush, point);
                DrawUtils.DrawTileCollectionOutline(_brush, point, TIGWEUISystem.Settings.ToolColor);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{TerrariaInGameWorldEditor.MODNAME}] Error drawing brush: {ex}");
            }
        }

        public override void PostUpdateInput()
        {
            Main.blockMouse = true;

            // on release (stop drawing)
            if (Main.mouseLeftRelease)
            {
                // if we placed tiles
                if (_currentDrawPreTilesPlaced.Count > 0)
                {
                    // create a tile collection with all the affected tiles and add it to undo history
                    TileCollection tileColl = new TileCollection();
                    tileColl.TryAddTiles(_currentDrawPreTilesPlaced);
                    EditorSystem.Local.UndoHistory.Add(tileColl);
                    _currentDrawPreTilesPlaced.Clear();
                    _currentDrawLinePoints.Clear();
                }
            }

            // on hold (draw) and not hovering over UI
            if (Main.mouseLeft && !Main.LocalPlayer.mouseInterface)
            {
                List<Point> pointsToDrawAt = new List<Point>();
                pointsToDrawAt.Add(new Point(Player.tileTargetX, Player.tileTargetY));
                _currentDrawLinePoints.Add(new Point(Player.tileTargetX, Player.tileTargetY));
                if (_currentDrawLinePoints.Count > 1)
                {
                    // get the points in a line between the last point and the current point to avoid gaps
                    pointsToDrawAt.AddRange(ToolUtils.CalculatePointsInLine(_currentDrawLinePoints[_currentDrawLinePoints.Count - 2], _currentDrawLinePoints[_currentDrawLinePoints.Count - 1]));
                }

                // get width and height of brush
                int width = (int)(_brush.GetWidth() / 2);
                int height = (int)(_brush.GetHeight() / 2);

                // get brush as list for easier iteration
                var brushList = _brush.ToNormalized().AsDictionary().ToList();

                foreach (Point point in pointsToDrawAt) {

                    // go over all the tiles and set coordinates
                    foreach (var tile in brushList)
                    {
                        int x = tile.Key.X + point.X - width;
                        int y = tile.Key.Y + point.Y - height;

                        _currentDrawPreTilesPlaced.TryAdd(new Point(x, y), new TileCopy(Main.tile[x, y]));

                        // we also need to add the tiles around it since those also get affected if we have update tiles on
                        if (TIGWEUISystem.Settings.ShouldUpdateDrawnTiles)
                        {
                            _currentDrawPreTilesPlaced.TryAdd(new Point(x + 1, y), new TileCopy(Main.tile[x + 1, y]));
                            _currentDrawPreTilesPlaced.TryAdd(new Point(x - 1, y), new TileCopy(Main.tile[x - 1, y]));
                            _currentDrawPreTilesPlaced.TryAdd(new Point(x, y + 1), new TileCopy(Main.tile[x, y + 1]));
                            _currentDrawPreTilesPlaced.TryAdd(new Point(x, y - 1), new TileCopy(Main.tile[x, y - 1]));
                        }
                    }

                    // paste
                    ToolUtils.Paste(_brush, new Point(point.X - width, point.Y - height), false, TIGWEUISystem.Settings.ShouldUpdateDrawnTiles);
                }
            }
        }
    }
}
