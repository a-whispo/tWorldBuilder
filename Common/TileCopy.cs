using System;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace TerrariaInGameWorldEditor.Common
{
    public class TileCopy : TagSerializable
    {
        // deserializer for the tagcompound
        public static Func<TagCompound, TileCopy> DESERIALIZER = s => DeserializeData(s);

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

        public TileCopy(Tile tile, int x = default, int y = default)
        {
            CopyTileData(tile);
            CopyWallData(tile);
            CopyWireData(tile);
            if (x != default && y != default)
            {
                CopyTreeData(x, y);
            }
        }

        public void CopyTreeData(int x, int y) // copy tree data from a specific tile
        {
            // do some palm tree checks
            if (this.TileType == 323) // check for palm trees
            {
                int tileX = x;
                int tileY = y;
                while (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == 323)
                {
                    tileY++;
                }

                // get variant number
                int num = -1;
                if (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == 53)
                {
                    num = 0;
                }
                if (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == 234)
                {
                    num = 1;
                }
                if (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == 116)
                {
                    num = 2;
                }
                if (Main.tile[tileX, tileY].HasTile && Main.tile[tileX, tileY].TileType == 112)
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

        public TagCompound SerializeData()
        {
            // create a tagcompound with all the fields
            string tileTypeName = TileID.Search.GetName(TileType);
            string wallTypeName = WallID.Search.GetName(WallType);
            var tag = new TagCompound()
            {
                ["HasTile"] = HasTile,
                ["TileTypeName"] = tileTypeName,
                ["WallTypeName"] = wallTypeName,
                ["LiquidType"] = LiquidType,
                ["IsHalfBlock"] = IsHalfBlock,
                ["TileColor"] = TileColor,
                ["WallFrameNumber"] = WallFrameNumber,
                ["BlueWire"] = BlueWire,
                ["GreenWire"] = GreenWire,
                ["RedWire"] = RedWire,
                ["YellowWire"] = YellowWire,
                ["IsTileInvisible"] = IsTileInvisible,
                ["IsWallInvisible"] = IsWallInvisible,
                ["CheckingLiquid"] = CheckingLiquid,
                ["LiquidAmount"] = LiquidAmount,
                ["SkipLiquid"] = SkipLiquid,
                ["TileFrameNumber"] = TileFrameNumber,
                ["TileFrameX"] = TileFrameX,
                ["TileFrameY"] = TileFrameY,
                ["WallColor"] = WallColor,
                ["IsWallFullbright"] = IsWallFullbright,
                ["WallFrameX"] = WallFrameX,
                ["WallFrameY"] = WallFrameY,
                ["IsTreeTop"] = IsTreeTop,
                ["IsTreeBranch"] = IsTreeBranch,
                ["IsTreeTrunk"] = IsTreeTrunk,
                ["IsFlipped"] = IsFlipped,
                ["TreeVariant"] = TreeVariant,
                ["TreeFrame"] = TreeFrame,
                ["TreeFrameWidth"] = TreeFrameWidth,
                ["TreeFrameHeight"] = TreeFrameHeight,
                ["TreeStyle"] = TreeStyle,
                ["y2"] = y2,
                ["TreeBiome"] = TreeBiome,
                ["HasActuator"] = HasActuator,
                ["IsActuated"] = IsActuated,
                ["Slope"] = (int)Slope
            };

            return tag;
        }

        public static TileCopy DeserializeData(TagCompound tag)
        {
            // create a tilecopy with all the fields that was in the tag
            TileCopy tc = new TileCopy(new Tile());
            tc.HasTile = tag.GetBool("HasTile");
            try
            {
                tc.TileType = (ushort)TileID.Search.GetId(tag.GetString("TileTypeName"));
            } catch (Exception ex)
            {
                TerrariaInGameWorldEditor.ModLogger.Warn("Failed to find TileType when deserializing TileCopy.", ex);
                tc.TileType = 697; // unloaded tile
            }
            try
            {
                tc.WallType = (ushort)WallID.Search.GetId(tag.GetString("WallTypeName"));
            }
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.ModLogger.Warn("Failed to find WallType when deserializing TileCopy.", ex);
                tc.WallType = 347; // unloaded wall
            }
            tc.LiquidType = tag.GetInt("LiquidType");
            tc.IsHalfBlock = tag.GetBool("IsHalfBlock");
            tc.TileColor = tag.GetByte("TileColor");
            tc.WallFrameNumber = tag.GetInt("WallFrameNumber");
            tc.GreenWire = tag.GetBool("GreenWire");
            tc.RedWire = tag.GetBool("RedWire");
            tc.YellowWire = tag.GetBool("YellowWire");
            tc.IsTileInvisible = tag.GetBool("IsTileInvisible");
            tc.IsWallInvisible = tag.GetBool("IsWallInvisible");
            tc.CheckingLiquid = tag.GetBool("CheckingLiquid");
            tc.LiquidAmount = tag.GetByte("LiquidAmount");
            tc.SkipLiquid = tag.GetBool("SkipLiquid");
            tc.TileFrameNumber = tag.GetInt("TileFrameNumber");
            tc.TileFrameX = tag.GetShort("TileFrameX");
            tc.TileFrameY = tag.GetShort("TileFrameY");
            tc.WallColor = tag.GetByte("WallColor");
            tc.IsWallFullbright = tag.GetBool("IsWallFullbright");
            tc.WallFrameX = tag.GetInt("WallFrameX");
            tc.WallFrameY = tag.GetInt("WallFrameY");
            tc.IsTreeTop = tag.GetBool("IsTreeTop");
            tc.IsTreeBranch = tag.GetBool("IsTreeBranch");
            tc.IsTreeTrunk = tag.GetBool("IsTreeTrunk");
            tc.IsFlipped = tag.GetBool("IsFlipped");
            tc.TreeVariant = tag.GetInt("TreeVariant");
            tc.TreeFrame = tag.GetInt("TreeFrame");
            tc.TreeFrameWidth = tag.GetInt("TreeFrameWidth");
            tc.TreeFrameHeight = tag.GetInt("TreeFrameHeight");
            tc.TreeStyle = tag.GetInt("TreeStyle");
            tc.y2 = tag.GetInt("y2");
            tc.TreeBiome = tag.GetInt("TreeBiome");
            tc.HasActuator = tag.GetBool("HasActuator");
            tc.IsActuated = tag.GetBool("IsActuated");
            tc.Slope = (SlopeType)tag.GetInt("Slope");
            return tc;
        }
    }
}
