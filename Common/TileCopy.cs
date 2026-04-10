using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace TerrariaInGameWorldEditor.Common
{
    public class TileCopy
    {
        public TileEntityData Entity { get; set; }
        public ChestData Container { get; set; }

        // general tile stuff
        public bool HasTile { get; set; }
        public ushort TileType { get; set; }
        public ushort WallType { get; set; }
        public Byte LiquidType { get; set; }
        public SlopeType Slope { get; set; }
        public bool IsHalfBlock { get; set; }
        public Byte TileColor { get; set; }
        public short WallFrameNumber { get; set; }
        public bool BlueWire { get; set; }
        public bool GreenWire { get; set; }
        public bool RedWire { get; set; }
        public bool YellowWire { get; set; }
        public bool IsTileInvisible { get; set; }
        public bool IsWallInvisible { get; set; }
        public bool CheckingLiquid { get; set; }
        public Byte LiquidAmount { get; set; }
        public bool SkipLiquid { get; set; }
        public short TileFrameNumber { get; set; }
        public short TileFrameX { get; set; }
        public short TileFrameY { get; set; }
        public Byte WallColor { get; set; }
        public bool IsWallFullbright { get; set; }
        public short WallFrameX { get; set; }
        public short WallFrameY { get; set; }
        public bool IsActuated { get; set; }
        public bool HasActuator { get; set; }
        public bool HasWire => (RedWire || GreenWire || BlueWire || YellowWire);

        // tree variables
        public bool IsTreeTop { get; set; }
        public bool IsTreeBranch => (TileFrameX == 66 || TileFrameX == 44) && TileFrameY >= 198;
        public bool IsTreeTrunk => TileID.Sets.IsATreeTrunk[TileType];
        public bool IsFlipped { get; set; } = false;
        public short TreeVariant { get; set; }
        public short TreeFrame { get; set; }
        public short TreeFrameWidth { get; set; }
        public short TreeFrameHeight { get; set; }
        public short TreeStyle { get; set; }
        public short y2 { get; set; }
        public short TreeBiome { get; set; } = 0;

        public TileCopy(Tile tile)
        {
            CopyTileData(tile);
            CopyWallData(tile);
            CopyWireData(tile);
        }

        public TileCopy(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            CopyTileData(tile);
            CopyWallData(tile);
            CopyWireData(tile);
            CopyTreeData(x, y);
            Entity = TileEntityData.CopyTileEntityData(x, y);
            Container = ChestData.CopyChestData(x, y);
        }

        public TileCopy()
        {
            Tile tile = new Tile();
            CopyTileData(tile);
            CopyWallData(tile);
            CopyWireData(tile);
        }

        public void CopyTreeData(int x, int y) // copy tree data from a specific tile
        {
            // do some palm tree checks
            if ( TileType == TileID.PalmTree) // check for palm trees
            {
                int tileX = x;
                int tileY = y;
                while (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == TileID.PalmTree)
                {
                    tileY++;
                }

                // get variant number
                int num = -1;
                if (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == TileID.Sand)
                {
                    num = 0;
                }
                if (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == TileID.Crimsand)
                {
                    num = 1;
                }
                if (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == TileID.Pearlsand)
                {
                    num = 2;
                }
                if (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == TileID.Ebonsand)
                {
                    num = 3;
                }
                if (WorldGen.IsPalmOasisTree(x))
                {
                    num += 4;
                }
                TreeBiome = (short)num;
            }

            // check if tile is a treetop, tiletype 72 and tileframex 36 is shroomcaps
            if (WorldGen.IsTileALeafyTreeTop(x, y) || (TileType == 72 && TileFrameX >= 36))
            {
                IsTreeTop = true;
            }

            // checks if its a treebranch, i dont really like checking it this way but it works for all vanilla trees so i'll let it be for now
            if ((TileFrameX == 66 || TileFrameX == 44) && TileFrameY >= 198)
            {
                WorldGen.GetTreeBottom(x, y, out int bottomX, out int bottomY);
                IsFlipped = !(x > bottomX); // check what side of the tree the branch is so we know if we should flip it
            }

            if (IsTreeTop || IsTreeBranch || IsTreeTrunk)
            {
                WorldGen.GetTreeBottom(x, y, out int bottomX, out int bottomY);
                TreeVariant = (short)(TileDrawing.GetTreeVariant(bottomX, bottomY) + 1);
                WorldGen.GetTreeFoliageDataMethod foliageDataMethod;

                if (TileType == 588) // gem trees
                {
                    foliageDataMethod = new WorldGen.GetTreeFoliageDataMethod(WorldGen.GetGemTreeFoliageData);
                }
                else if (TileType == 616) // vanity trees like sakura and yellow willow
                {
                    foliageDataMethod = new WorldGen.GetTreeFoliageDataMethod(WorldGen.GetVanityTreeFoliageData);
                }
                else if (TileType == 634) // ash trees
                {
                    foliageDataMethod = new WorldGen.GetTreeFoliageDataMethod(WorldGen.GetAshTreeFoliageData);
                }
                else // all other trees
                {
                    foliageDataMethod = new WorldGen.GetTreeFoliageDataMethod(WorldGen.GetCommonTreeFoliageData);
                }

                int treeFrame = WorldGen.GetTreeFrame(Main.tile[x, y]);
                int treeStyle = 0;
                foliageDataMethod(x, y, 0, ref treeFrame, ref treeStyle, out int y2, out int topTextureFrameWidth1, out int topTextureFrameHeight1);
                TreeFrameHeight = (short)topTextureFrameHeight1;
                TreeFrameWidth = (short)topTextureFrameWidth1;
                TreeStyle = (short)treeStyle;
                TreeFrame = (short)treeFrame;
            }
        }

        public void CopyWallData(Tile original)
        {
            WallType = original.WallType;
            WallFrameNumber = (short)original.WallFrameNumber;
            IsWallInvisible = original.IsWallInvisible;
            WallColor = original.WallColor;
            IsWallFullbright = original.IsWallFullbright;
            WallFrameX = (short)original.WallFrameX;
            WallFrameY = (short)original.WallFrameY;
        }

        public void CopyTileData(Tile original)
        {
            HasTile = original.HasTile;
            TileType = original.TileType;
            LiquidType = (Byte)original.LiquidType;
            Slope = original.Slope;
            IsHalfBlock = original.IsHalfBlock;
            TileColor = original.TileColor;
            IsTileInvisible = original.IsTileInvisible;
            CheckingLiquid = original.CheckingLiquid;
            LiquidAmount = original.LiquidAmount;
            SkipLiquid = original.SkipLiquid;
            TileFrameNumber = (short)original.TileFrameNumber;
            TileFrameX = original.TileFrameX;
            TileFrameY = original.TileFrameY;
            IsActuated = original.IsActuated;
        }

        public void CopyWireData(Tile original)
        {
            BlueWire = original.BlueWire;
            GreenWire = original.GreenWire;
            RedWire = original.RedWire;
            YellowWire = original.YellowWire;
            HasActuator = original.HasActuator;
        }

        public Tile GetAsTile()
        {
            Tile newTile = new Tile();
            newTile.HasTile = HasTile;
            newTile.TileType = TileType;
            newTile.WallType = WallType;
            newTile.LiquidType = LiquidType;
            newTile.Slope = Slope;
            newTile.IsHalfBlock = IsHalfBlock;
            newTile.TileColor = TileColor;
            newTile.WallFrameNumber = WallFrameNumber;
            newTile.BlueWire = BlueWire;
            newTile.GreenWire = GreenWire;
            newTile.RedWire = RedWire;
            newTile.YellowWire = YellowWire;
            newTile.IsTileInvisible = IsTileInvisible;
            newTile.IsWallInvisible = IsWallInvisible;
            newTile.CheckingLiquid = CheckingLiquid;
            newTile.LiquidAmount = LiquidAmount;
            newTile.SkipLiquid = SkipLiquid;
            newTile.TileFrameNumber = TileFrameNumber;
            newTile.TileFrameX = TileFrameX;
            newTile.TileFrameY = TileFrameY;
            newTile.WallColor = WallColor;
            newTile.IsWallFullbright = IsWallFullbright;
            newTile.WallFrameX = WallFrameX;
            newTile.WallFrameY = WallFrameY;
            newTile.IsActuated = IsActuated;
            newTile.HasActuator = HasActuator;
            newTile.Slope = Slope;
            return newTile;
        }

        private static ushort TryReadTileType(BinaryReader br, HashSet<string> missingMods)
        {
            string tileName = br.ReadString();
            if (TileID.Search.TryGetId(tileName, out int id))
            {
                return (ushort)id;
            }
            else
            {
                missingMods.Add(tileName.Split('/')[0]);
                return 697; // unloaded tile
            }
        }

        private static ushort TryReadWallType(BinaryReader br, HashSet<string> missingMods)
        {
            string wallName = br.ReadString();
            if (WallID.Search.TryGetId(wallName, out int id))
            {
                return (ushort)id;
            }
            else
            {
                missingMods.Add(wallName.Split('/')[0]);
                return 347; // unloaded wall
            }
        }

        public static void WriteTileCopy(BinaryWriter bw, TileCopy tc)
        {
            bw.Write(tc.HasTile);
            bw.Write(TileID.Search.GetName(tc.TileType));
            bw.Write(WallID.Search.GetName(tc.WallType));
            bw.Write(tc.LiquidType);
            bw.Write(tc.IsHalfBlock);
            bw.Write(tc.TileColor);
            bw.Write(tc.WallFrameNumber);
            bw.Write(tc.BlueWire);
            bw.Write(tc.GreenWire);
            bw.Write(tc.RedWire);
            bw.Write(tc.YellowWire);
            bw.Write(tc.IsTileInvisible);
            bw.Write(tc.IsWallInvisible);
            bw.Write(tc.CheckingLiquid);
            bw.Write(tc.LiquidAmount);
            bw.Write(tc.SkipLiquid);
            bw.Write(tc.TileFrameNumber);
            bw.Write(tc.TileFrameX);
            bw.Write(tc.TileFrameY);
            bw.Write(tc.WallColor);
            bw.Write(tc.IsWallFullbright);
            bw.Write(tc.WallFrameX);
            bw.Write(tc.WallFrameY);
            bw.Write(tc.IsTreeTop);
            bw.Write(tc.IsFlipped);
            bw.Write(tc.TreeVariant);
            bw.Write(tc.TreeFrame);
            bw.Write(tc.TreeFrameWidth);
            bw.Write(tc.TreeFrameHeight);
            bw.Write(tc.TreeStyle);
            bw.Write(tc.y2);
            bw.Write(tc.TreeBiome);
            bw.Write(tc.HasActuator);
            bw.Write(tc.IsActuated);
            bw.Write((byte)tc.Slope);
            ChestData.Write(bw, tc.Container);
            TileEntityData.Write(bw, tc.Entity);
        }

        public static TileCopy ReadV2TileCopy(BinaryReader br, HashSet<string> missingMods)
        {
            TileCopy tc = ReadV1TileCopy(br, missingMods);
            tc.Container = ChestData.Read(br, missingMods);
            tc.Entity = TileEntityData.Read(br, missingMods);
            return tc;
        }

        public static TileCopy ReadV1TileCopy(BinaryReader br, HashSet<string> missingMods)
        {
            TileCopy tc = new TileCopy();
            tc.HasTile = br.ReadBoolean();
            tc.TileType = TryReadTileType(br, missingMods);
            tc.WallType = TryReadWallType(br, missingMods);
            tc.LiquidType = br.ReadByte();
            tc.IsHalfBlock = br.ReadBoolean();
            tc.TileColor = br.ReadByte();
            tc.WallFrameNumber = br.ReadInt16();
            tc.BlueWire = br.ReadBoolean();
            tc.GreenWire = br.ReadBoolean();
            tc.RedWire = br.ReadBoolean();
            tc.YellowWire = br.ReadBoolean();
            tc.IsTileInvisible = br.ReadBoolean();
            tc.IsWallInvisible = br.ReadBoolean();
            tc.CheckingLiquid = br.ReadBoolean();
            tc.LiquidAmount = br.ReadByte();
            tc.SkipLiquid = br.ReadBoolean();
            tc.TileFrameNumber = br.ReadInt16();
            tc.TileFrameX = br.ReadInt16();
            tc.TileFrameY = br.ReadInt16();
            tc.WallColor = br.ReadByte();
            tc.IsWallFullbright = br.ReadBoolean();
            tc.WallFrameX = br.ReadInt16();
            tc.WallFrameY = br.ReadInt16();
            tc.IsTreeTop = br.ReadBoolean();
            tc.IsFlipped = br.ReadBoolean();
            tc.TreeVariant = br.ReadInt16();
            tc.TreeFrame = br.ReadInt16();
            tc.TreeFrameWidth = br.ReadInt16();
            tc.TreeFrameHeight = br.ReadInt16();
            tc.TreeStyle = br.ReadInt16();
            tc.y2 = br.ReadInt16();
            tc.TreeBiome = br.ReadInt16();
            tc.HasActuator = br.ReadBoolean();
            tc.IsActuated = br.ReadBoolean();
            tc.Slope = (SlopeType)br.ReadByte();
            return tc;
        }

        public static TileCopy ReadV0TileCopy(BinaryReader br, HashSet<string> missingMods)
        {
            TileCopy tc = new TileCopy();
            tc.HasTile = br.ReadBoolean();
            tc.TileType = TryReadTileType(br, missingMods);
            tc.WallType = TryReadWallType(br, missingMods);
            tc.LiquidType = (Byte)br.ReadInt32();
            tc.IsHalfBlock = br.ReadBoolean();
            tc.TileColor = br.ReadByte();
            tc.WallFrameNumber = (short)br.ReadInt32();
            tc.BlueWire = br.ReadBoolean();
            tc.GreenWire = br.ReadBoolean();
            tc.RedWire = br.ReadBoolean();
            tc.YellowWire = br.ReadBoolean();
            tc.IsTileInvisible = br.ReadBoolean();
            tc.IsWallInvisible = br.ReadBoolean();
            tc.CheckingLiquid = br.ReadBoolean();
            tc.LiquidAmount = br.ReadByte();
            tc.SkipLiquid = br.ReadBoolean();
            tc.TileFrameNumber = (short)br.ReadInt32();
            tc.TileFrameX = br.ReadInt16();
            tc.TileFrameY = br.ReadInt16();
            tc.WallColor = br.ReadByte();
            tc.IsWallFullbright = br.ReadBoolean();
            tc.WallFrameX = (short)br.ReadInt32();
            tc.WallFrameY = (short)br.ReadInt32();
            tc.IsTreeTop = br.ReadBoolean();
            _ = br.ReadBoolean();
            _ = br.ReadBoolean();
            tc.IsFlipped = br.ReadBoolean();
            tc.TreeVariant = (short)br.ReadInt32();
            tc.TreeFrame = (short)br.ReadInt32();
            tc.TreeFrameWidth = (short)br.ReadInt32();
            tc.TreeFrameHeight = (short)br.ReadInt32();
            tc.TreeStyle = (short)br.ReadInt32();
            tc.y2 = (short)br.ReadInt32();
            tc.TreeBiome = (short)br.ReadInt32();
            tc.HasActuator = br.ReadBoolean();
            tc.IsActuated = br.ReadBoolean();
            tc.Slope = (SlopeType)br.ReadByte();
            return tc;
        }
    }
}
