using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace TerrariaInGameWorldEditor.Common
{
    public class TileCollection : TagSerializable
    {
        // deserializer for the tagcompound
        public static Func<TagCompound, TileCollection> DESERIALIZER { get; set; } = s => DeserializeData(s);

        // tiles stored
        private Dictionary<Point, TileCopy> _tiles = new Dictionary<Point, TileCopy>();

        // min values
        private int _minX = int.MaxValue;
        private int _minY = int.MaxValue;
        private int _maxX = int.MinValue;
        private int _maxY = int.MinValue;
        private bool _boundsDirty = false;

        public int Count
        {
            get => _tiles.Count;
        }

        public TileCollection ToMirrored()
        {
            // recalculate if needed
            if (_boundsDirty)
            {
                RecalculateBounds();
            }

            // new mirrored tile collection
            TileCollection newTileColl = new TileCollection();
            int maxX = GetWidth() - 1;

            // mirror
            foreach (var tile in _tiles)
            {
                newTileColl.TryAddTile(new Point((maxX - tile.Key.X), tile.Key.Y), tile.Value);
            }

            return newTileColl;
        }

        public TileCollection To90DegAntiClockwise()
        {
            // recalculate if needed
            if (_boundsDirty)
            {
                RecalculateBounds();
            }

            // new rotated tile collection
            TileCollection newTileColl = new TileCollection();
            int maxX = GetWidth() - 1;
            int minX = GetMinX();

            // rotate
            foreach (var tile in _tiles)
            {
                newTileColl.TryAddTile(new Point(tile.Key.Y, (maxX + minX - tile.Key.X)), tile.Value);
            }

            return newTileColl;
        }

        public Dictionary<Point, TileCopy> AsDictionary()
        {
            return _tiles;
        }

        public List<Point> ToListOfPoints()
        {
            List<Point> points = new List<Point>();
            foreach (var tile in _tiles)
            {
                points.Add(tile.Key);
            }
            return points;
        }

        public TileCollection ToNormalized()
        {
            // recalculate if needed
            if (_boundsDirty)
            {
                RecalculateBounds();
            }

            // create a tilecollection with updated values
            TileCollection normalizedCollection = new TileCollection();
            foreach (var entry in _tiles)
            {
                normalizedCollection.TryAddTile(new Point(entry.Key.X - _minX, entry.Key.Y - _minY), entry.Value);
            }

            return normalizedCollection;
        }

        private void UpdateBounds(Point point)
        {
            // just compare and update
            if (point.X < _minX)
            {
                _minX = point.X;
            }
            if (point.X > _maxX)
            {
                _maxX = point.X;
            }
            if (point.Y < _minY)
            {
                _minY = point.Y;
            }
            if (point.Y > _maxY)
            {
                _maxY = point.Y;
            }
        }

        private void RecalculateBounds()
        {
            // set to extreme values so we can update them correctly
            _minX = int.MaxValue;
            _minY = int.MaxValue;
            _maxX = int.MinValue;
            _maxY = int.MinValue;

            // iterate and compare
            foreach (var tile in _tiles)
            {
                UpdateBounds(tile.Key);
            }

            _boundsDirty = false;
        }

        public void TryAddTiles(Dictionary<Point, TileCopy> tilesToAdd)
        {
            foreach (var tile in tilesToAdd)
            {
                TryAddTile(tile.Key, tile.Value);
            }
        }

        public bool TryAddTile(Point coords, TileCopy tileToAdd)
        {
            // check if we can add

            // update bounds if we added
            if (_tiles.TryAdd(coords, tileToAdd))
            {
                if (!_boundsDirty)
                {
                    UpdateBounds(coords);
                }
                return true;
            }
            return false;
        }

        public bool RemoveTile(Point coords)
        {
            if (_tiles.Remove(coords))
            {
                // check if the tile we remove was on the bounds
                if (coords.X == _minX || coords.X == _maxX || coords.Y == _minY || coords.Y == _maxY)
                {
                    // if it was we need to recalculate the bounds
                    _boundsDirty = true;
                }
                return true;
            }
            return false;
        }

        public bool TryGetTile(Point coords, out TileCopy tile)
        {
            return _tiles.TryGetValue(coords, out tile);
        }

        public void Clear()
        {
            _tiles.Clear();
            _minX = int.MaxValue;
            _minY = int.MaxValue;
            _maxX = int.MinValue;
            _maxY = int.MinValue;
            _boundsDirty = false;
        }

        public bool ContainsCoord(Point coords)
        {
            return _tiles.ContainsKey(coords);
        }

        public int GetMinX()
        {
            if (_boundsDirty)
            {
                RecalculateBounds();
            }
            return _minX;
        }

        public int GetMinY()
        {
            if (_boundsDirty)
            {
                RecalculateBounds();
            }
            return _minY;
        }

        public int GetMaxX()
        {
            if (_boundsDirty)
            {
                RecalculateBounds();
            }
            return _maxX;
        }

        public int GetMaxY()
        {
            if (_boundsDirty)
            {
                RecalculateBounds();
            }
            return _maxY;
        }

        public int GetWidth()
        {
            if (_boundsDirty)
            {
                RecalculateBounds();
            }
            return _maxX - _minX;
        }

        public int GetHeight()
        {
            if (_boundsDirty)
            {
                RecalculateBounds();
            }
            return _maxY - _minY;
        }

        public TagCompound SerializeData()
        {
            // serialize the dict with help of a list
            var tag = new TagCompound();
            var list = new List<TagCompound>();

            foreach (var item in _tiles)
            {
                list.Add(new TagCompound() {
                    { "X", item.Key.X },
                    { "Y", item.Key.Y },
                    { "TileCopy", item.Value },
                });
            }
            tag["Tiles"] = list;
            return tag;
        }

        public static TileCollection DeserializeData(TagCompound tag)
        {
            // deserialize the dict
            TileCollection tileColl = new TileCollection();

            var list = tag.GetList<TagCompound>("Tiles");
            foreach (var item in list)
            {
                int x = item.GetInt("X");
                int y = item.GetInt("Y");
                TileCopy tileCopy = item.Get<TileCopy>("TileCopy");
                tileColl._tiles.Add(new Point(x, y), tileCopy);
            }
            return tileColl;
        }
    }
}