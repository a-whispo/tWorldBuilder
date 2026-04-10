using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerrariaInGameWorldEditor.Common
{
    public class TileCollection : IEnumerable<KeyValuePair<Point16, TileCopy>>
    {
        public EventHandler OnChanged;
        public int Count => _tiles.Count;
        
        // tiles stored
        private Dictionary<Point16, TileCopy> _tiles = new Dictionary<Point16, TileCopy>();
        private TileCollection _cachedMirrored;
        private TileCollection _cached90Deg;
        private TileCollection _cachedNormalized;

        // min values
        private int _minX = int.MaxValue;
        private int _minY = int.MaxValue;
        private int _maxX = int.MinValue;
        private int _maxY = int.MinValue;
        private bool _boundsDirty = true;

        public TileCollection ToMirrored()
        {
            if (_cachedMirrored != null)
            {
                return _cachedMirrored;
            }
            if (_boundsDirty)
            {
                RecalculateBounds();
            }
            
            _cachedMirrored = new TileCollection();
            int maxX = GetWidth() - 1;
            foreach (var tile in _tiles)
            {
                _cachedMirrored.TryAddTile(new Point16((maxX - tile.Key.X), tile.Key.Y), tile.Value);
            }

            return _cachedMirrored;
        }

        public TileCollection To90DegAntiClockwise()
        {
            if (_cached90Deg != null)
            {
                return _cached90Deg;
            }
            if (_boundsDirty)
            {
                RecalculateBounds();
            }

            _cached90Deg = new TileCollection();
            int maxX = GetWidth() - 1;
            int minX = GetMinX();
            foreach (var tile in _tiles)
            {
                _cached90Deg.TryAddTile(new Point16(tile.Key.Y, (maxX + minX - tile.Key.X)), tile.Value);
            }

            return _cached90Deg;
        }

        public TileCollection ToNormalized()
        {
            if (_cachedNormalized != null)
            {
                return _cachedNormalized;
            }
            if (_boundsDirty)
            {
                RecalculateBounds();
            }

            _cachedNormalized = new TileCollection();
            foreach (var tile in _tiles)
            {
                _cachedNormalized.TryAddTile(new Point16(tile.Key.X - _minX, tile.Key.Y - _minY), tile.Value);
            }

            return _cachedNormalized;
        }

        public bool ContainsCoord(Point16 coord)
        {
            return _tiles.ContainsKey(coord);
        }

        public void TryAddTiles(IEnumerable<KeyValuePair<Point16, TileCopy>> tiles)
        {
            bool changed = false;
            foreach (var tile in tiles)
            {
                if (_tiles.TryAdd(tile.Key, tile.Value))
                {
                    if (!_boundsDirty)
                    {
                        UpdateBounds(tile.Key);
                    }
                    changed = true;

                }
            }
            if (changed)
            {
                InvalidateCaches();
                OnChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TryAddTile(Point16 coord, TileCopy tile)
        {
            if (_tiles.TryAdd(coord, tile))
            {
                if (!_boundsDirty)
                {
                    UpdateBounds(coord);
                }
                InvalidateCaches();
                OnChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public bool TryAddTile(Point16 coord, Func<TileCopy> func)
        {
            if (_tiles.ContainsKey(coord))
            {
                return false;
            }

            _tiles.Add(coord, func());
            if (!_boundsDirty)
            {
                UpdateBounds(coord);
            }
            InvalidateCaches();
            OnChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool TryGetTile(Point16 coord, out TileCopy tile)
        {
            return _tiles.TryGetValue(coord, out tile);
        }

        public bool RemoveTile(Point16 coord)
        {
            if (_tiles.Remove(coord))
            {
                // if the tile we removed was on the bounds we need to recalculate them
                if (coord.X == _minX || coord.X == _maxX || coord.Y == _minY || coord.Y == _maxY)
                {
                    _boundsDirty = true;
                }
                InvalidateCaches();
                OnChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _tiles.Clear();
            _minX = int.MaxValue;
            _minY = int.MaxValue;
            _maxX = int.MinValue;
            _maxY = int.MinValue;
            _boundsDirty = false;
            InvalidateCaches();
            OnChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateBounds(Point16 coord)
        {
            // just compare and update
            if (coord.X < _minX)
            {
                _minX = coord.X;
            }
            if (coord.X > _maxX)
            {
                _maxX = coord.X;
            }
            if (coord.Y < _minY)
            {
                _minY = coord.Y;
            }
            if (coord.Y > _maxY)
            {
                _maxY = coord.Y;
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

        private void InvalidateCaches()
        {
            _cachedMirrored = null;
            _cached90Deg = null;
            _cachedNormalized = null;
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

        public IEnumerator<KeyValuePair<Point16, TileCopy>> GetEnumerator()
        {
            return _tiles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tiles.GetEnumerator();
        }

        public static void WriteTileCollection(BinaryWriter bw, TileCollection tc)
        {
            // write tiles
            bw.Write(tc.Count);
            foreach (var item in tc)
            {
                bw.Write(item.Key.X);
                bw.Write(item.Key.Y);
                TileCopy.WriteTileCopy(bw, item.Value);
            }
        }

        public static TileCollection ReadV2TileCollection(BinaryReader br, out HashSet<string> missingMods)
        {
            missingMods = new HashSet<string>();
            TileCollection tc = new TileCollection();

            // read tiles
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                short x = br.ReadInt16();
                short y = br.ReadInt16();
                tc.TryAddTile(new Point16(x, y), TileCopy.ReadV2TileCopy(br, missingMods));
            }
            return tc;
        }

        public static TileCollection ReadV1TileCollection(BinaryReader br, out HashSet<string> missingMods)
        {
            missingMods = new HashSet<string>();
            TileCollection tc = new TileCollection();

            // read tiles
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                short x = br.ReadInt16();
                short y = br.ReadInt16();
                tc.TryAddTile(new Point16(x, y), TileCopy.ReadV1TileCopy(br, missingMods));
            }
            return tc;
        }

        public static TileCollection ReadV0TileCollection(BinaryReader br, out HashSet<string> missingMods)
        {
            missingMods = new HashSet<string>();
            TileCollection tc = new TileCollection();

            // read tiles
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                short x = br.ReadInt16();
                short y = br.ReadInt16();
                tc.TryAddTile(new Point16(x, y), TileCopy.ReadV0TileCopy(br, missingMods));
            }
            return tc;
        }
    }
}