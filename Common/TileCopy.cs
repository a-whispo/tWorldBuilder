using System;
using System.Collections.Generic;
using System.CommandLine.Help;
using System.IO;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;

namespace TerrariaInGameWorldEditor.Common
{
    public class TileCopy
    {
        // general tile stuff
        public bool HasTile { get; set; }
        public ushort TileType { get; set; }
        public ushort WallType { get; set; }
        public int LiquidType { get; set; }
        public SlopeType Slope { get; set; }
        public bool IsHalfBlock { get; set; }
        public Byte TileColor { get; set; }
        public int WallFrameNumber { get; set; }
        public bool BlueWire { get; set; }
        public bool GreenWire { get; set; }
        public bool RedWire { get; set; }
        public bool YellowWire { get; set; }
        public bool IsTileInvisible { get; set; }
        public bool IsWallInvisible { get; set; }
        public bool CheckingLiquid { get; set; }
        public Byte LiquidAmount { get; set; }
        public bool SkipLiquid { get; set; }
        public int TileFrameNumber { get; set; }
        public short TileFrameX { get; set; }
        public short TileFrameY { get; set; }
        public Byte WallColor { get; set; }
        public bool IsWallFullbright { get; set; }
        public int WallFrameX { get; set; }
        public int WallFrameY { get; set; }
        public bool IsActuated { get; set; }
        public bool HasActuator { get; set; }
        public bool HasWire { get { return (RedWire || GreenWire || BlueWire || YellowWire); } }

        // tree variables
        public bool IsTreeTop { get; set; } = false;
        public bool IsTreeBranch { get; set; } = false;
        public bool IsTreeTrunk { get; set; } = false;
        public bool IsFlipped { get; set; } = false; // used to know if treebranch textures should be flipped
        public int TreeVariant { get; set; } // used to get the right tree trunk texture
        public int TreeFrame { get; set; } // used to get treetop and tree branch textures
        public int TreeFrameWidth { get; set; } // used to get treetop and tree branch textures
        public int TreeFrameHeight { get; set; } // used to get treetop and tree branch textures
        public int TreeStyle { get; set; } // used to get treetop and tree branch textures
        public int y2 { get; set; }
        public int TreeBiome { get; set; } = 0; // mostly used for help with drawing palm trees

        public TileCopy(Tile tile)
        {
            CopyTileData(tile);
            CopyWallData(tile);
            CopyWireData(tile);
        }

        public TileCopy(Tile tile, int x, int y)
        {
            CopyTileData(tile);
            CopyWallData(tile);
            CopyWireData(tile);
            CopyTreeData(x, y);
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
            if (this.TileType == TileID.PalmTree) // check for palm trees
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
                this.TreeBiome = num;
            }

            // check if tile is a treetrunk
            this.IsTreeTrunk = TileID.Sets.IsATreeTrunk[this.TileType];

            // check if tile is a treetop, tiletype 72 and tileframex 36 is shroomcaps
            if (WorldGen.IsTileALeafyTreeTop(x, y) || (this.TileType == 72 && this.TileFrameX >= 36))
            {
                this.IsTreeTop = true;
            }

            // checks if its a treebranch, i dont really like checking it this way but it works for all vanilla trees so i'll let it be for now
            if ((this.TileFrameX == 66 || this.TileFrameX == 44) && this.TileFrameY >= 198)
            {
                this.IsTreeBranch = true;
                WorldGen.GetTreeBottom(x, y, out int bottomX, out int bottomY);
                this.IsFlipped = !(x > bottomX); // check what side of the tree the branch is so we know if we should flip it
            }

            // update tree data in the tilecopy
            if (this.IsTreeTop || this.IsTreeBranch || this.IsTreeTrunk) // update relevant data if its a treetop or branch
            {
                WorldGen.GetTreeBottom(x, y, out int bottomX, out int bottomY);

                // get tree variant (used to get the right tree trunk texture)
                this.TreeVariant = TileDrawing.GetTreeVariant(bottomX, bottomY) + 1;

                // get info about treetop and hande some special tree cases
                WorldGen.GetTreeFoliageDataMethod foliageDataMethod;

                if (this.TileType == 588) // gem trees
                {
                    foliageDataMethod = new WorldGen.GetTreeFoliageDataMethod(WorldGen.GetGemTreeFoliageData);
                }
                else if (this.TileType == 616) // vanity trees like sakura and yellow willow
                {
                    foliageDataMethod = new WorldGen.GetTreeFoliageDataMethod(WorldGen.GetVanityTreeFoliageData);
                }
                else if (this.TileType == 634) // ash trees
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

                // set variables
                this.TreeFrameHeight = topTextureFrameHeight1;
                this.TreeFrameWidth = topTextureFrameWidth1;
                this.TreeStyle = treeStyle;
                this.TreeFrame = treeFrame;
            }
        }

        public void CopyWallData(Tile original)
        {
            this.WallType = original.WallType;
            this.WallFrameNumber = original.WallFrameNumber;
            this.IsWallInvisible = original.IsWallInvisible;
            this.WallColor = original.WallColor;
            this.IsWallFullbright = original.IsWallFullbright;
            this.WallFrameX = original.WallFrameX;
            this.WallFrameY = original.WallFrameY;
        }

        public void CopyTileData(Tile original)
        {
            this.HasTile = original.HasTile;
            this.TileType = original.TileType;
            this.LiquidType = original.LiquidType;
            this.Slope = original.Slope;
            this.IsHalfBlock = original.IsHalfBlock;
            this.TileColor = original.TileColor;
            this.IsTileInvisible = original.IsTileInvisible;
            this.CheckingLiquid = original.CheckingLiquid;
            this.LiquidAmount = original.LiquidAmount;
            this.SkipLiquid = original.SkipLiquid;
            this.TileFrameNumber = original.TileFrameNumber;
            this.TileFrameX = original.TileFrameX;
            this.TileFrameY = original.TileFrameY;
            this.IsActuated = original.IsActuated;
        }

        public void CopyWireData(Tile original)
        {
            this.BlueWire = original.BlueWire;
            this.GreenWire = original.GreenWire;
            this.RedWire = original.RedWire;
            this.YellowWire = original.YellowWire;
            this.HasActuator = original.HasActuator;
        }

        public Tile GetAsTile()
        {
            Tile newTile = new Tile();
            newTile.HasTile = this.HasTile;
            newTile.TileType = this.TileType;
            newTile.WallType = this.WallType;
            newTile.LiquidType = this.LiquidType;
            newTile.Slope = this.Slope;
            newTile.IsHalfBlock = this.IsHalfBlock;
            newTile.TileColor = this.TileColor;
            newTile.WallFrameNumber = this.WallFrameNumber;
            newTile.BlueWire = this.BlueWire;
            newTile.GreenWire = this.GreenWire;
            newTile.RedWire = this.RedWire;
            newTile.YellowWire = this.YellowWire;
            newTile.IsTileInvisible = this.IsTileInvisible;
            newTile.IsWallInvisible = this.IsWallInvisible;
            newTile.CheckingLiquid = this.CheckingLiquid;
            newTile.LiquidAmount = this.LiquidAmount;
            newTile.SkipLiquid = this.SkipLiquid;
            newTile.TileFrameNumber = this.TileFrameNumber;
            newTile.TileFrameX = this.TileFrameX;
            newTile.TileFrameY = this.TileFrameY;
            newTile.WallColor = this.WallColor;
            newTile.IsWallFullbright = this.IsWallFullbright;
            newTile.WallFrameX = this.WallFrameX;
            newTile.WallFrameY = this.WallFrameY;
            newTile.IsActuated = this.IsActuated;
            newTile.HasActuator = this.HasActuator;
            newTile.Slope = this.Slope;
            return newTile;
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
            bw.Write(tc.IsTreeBranch);
            bw.Write(tc.IsTreeTrunk);
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
        }

        public static TileCopy ReadTileCopy(BinaryReader br, out HashSet<string> missingMods)
        {
            missingMods = new HashSet<string>();
            TileCopy tc = new TileCopy();
            tc.HasTile = br.ReadBoolean();
            string tileName = br.ReadString();
            if (TileID.Search.TryGetId(tileName, out int tileID))
            {
                tc.TileType = (ushort)tileID;
            }
            else
            {
                missingMods.Add(tileName.Split('/')[0]);
                tc.TileType = 697; // unloaded tile
            }
            string wallName = br.ReadString();
            if (WallID.Search.TryGetId(wallName, out int wallID))
            {
                tc.WallType = (ushort)wallID;
            }
            else
            {
                missingMods.Add(wallName.Split('/')[0]);
                tc.WallType = 347; // unloaded wall
            }
            tc.LiquidType = br.ReadInt32();
            tc.IsHalfBlock = br.ReadBoolean();
            tc.TileColor = br.ReadByte();
            tc.WallFrameNumber = br.ReadInt32();
            tc.BlueWire = br.ReadBoolean();
            tc.GreenWire = br.ReadBoolean();
            tc.RedWire = br.ReadBoolean();
            tc.YellowWire = br.ReadBoolean();
            tc.IsTileInvisible = br.ReadBoolean();
            tc.IsWallInvisible = br.ReadBoolean();
            tc.CheckingLiquid = br.ReadBoolean();
            tc.LiquidAmount = br.ReadByte();
            tc.SkipLiquid = br.ReadBoolean();
            tc.TileFrameNumber = br.ReadInt32();
            tc.TileFrameX = br.ReadInt16();
            tc.TileFrameY = br.ReadInt16();
            tc.WallColor = br.ReadByte();
            tc.IsWallFullbright = br.ReadBoolean();
            tc.WallFrameX = br.ReadInt32();
            tc.WallFrameY = br.ReadInt32();
            tc.IsTreeTop = br.ReadBoolean();
            tc.IsTreeBranch = br.ReadBoolean();
            tc.IsTreeTrunk = br.ReadBoolean();
            tc.IsFlipped = br.ReadBoolean();
            tc.TreeVariant = br.ReadInt32();
            tc.TreeFrame = br.ReadInt32();
            tc.TreeFrameWidth = br.ReadInt32();
            tc.TreeFrameHeight = br.ReadInt32();
            tc.TreeStyle = br.ReadInt32();
            tc.y2 = br.ReadInt32();
            tc.TreeBiome = br.ReadInt32();
            tc.HasActuator = br.ReadBoolean();
            tc.IsActuated = br.ReadBoolean();
            tc.Slope = (SlopeType)br.ReadByte();
            return tc;
        }
    }
}
