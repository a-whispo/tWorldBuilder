using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace TerrariaInGameWorldEditor.Common
{
    public class TileEntityData
    {
        public int EntityType { get; set; }
        public TagCompound EntityTag { get; set; } = new TagCompound();

        public TileEntityData()
        {

        }

        public void Place(int x, int y)
        {
            TileEntity.ByPosition.Remove(new Point16(x, y));
            TileEntity.PlaceEntityNet(x, y, EntityType);
            if (TileEntity.ByPosition.ContainsKey(new Point16(x, y)))
            {
                TileEntity.ByPosition[new Point16(x, y)].LoadData(EntityTag);
            }
        }

        public static TileEntityData CopyTileEntityData(int x, int y)
        {
            if (TileEntity.ByPosition.TryGetValue(new Point16(x, y), out TileEntity value))
            {
                TileEntityData ted = new TileEntityData();
                value.SaveData(ted.EntityTag);
                ted.EntityType = value.type;
                return ted;
            }
            return null;
        }

        public static void Write(BinaryWriter bw, TileEntityData ted)
        {
            bw.Write(ted != null);
            if (ted != null)
            {
                bw.Write(ted.EntityType);
                TagIO.Write(ted.EntityTag, bw);
            }
        }

        public static TileEntityData Read(BinaryReader br, HashSet<string> missingMods)
        {
            bool hasTileEntityData = br.ReadBoolean();
            if (!hasTileEntityData)
            {
                return null;
            }

            try
            {
                TileEntityData ted = new TileEntityData();
                ted.EntityType = br.ReadInt32();
                ted.EntityTag = TagIO.Read(br);
                return ted;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
