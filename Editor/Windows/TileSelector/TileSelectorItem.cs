using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.Editor.Windows.TileSelector
{
    internal class TileSelectorItem : UIElement
    {
        public string Name { get; set; }
        public int ItemId { get; set; }
        public string HoverText { get; set; }
        public int CreateTile { get; set; }
        public int CreateWall { get; set; }

        private int _placeStyle;

        public TileSelectorItem(int itemId)
        {
            // load item
            Item item = ContentSamples.ItemsByType[itemId];
            string[] name = ItemID.Search.GetName(itemId).Split('/');
            Name = name.Length > 1 ? name[1] : name[0];
            ItemId = itemId;
            CreateTile = item.createTile;
            CreateWall = item.createWall;
            _placeStyle = item.placeStyle;

            // ui and events
            TIGWEImageResizeable body = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));
            body.TextureHover = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/TextureHover");
            body.Width.Set(0, 1);
            body.Height.Set(0, 1);
            Append(body);
            Width.Set(44, 0);
            Height.Set(44, 0);
        }

        public string GetResolvedName()
        {
            string localized = Lang.GetItemName(ItemId).Value;
            return string.IsNullOrEmpty(localized) ? Name : localized;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            HoverText = LocalizationUtils.GetTextValue("Windows.TileSelector.HoverText.TileItem", GetResolvedName());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Main.instance.LoadItem(ItemId);
            Texture2D tex = TextureAssets.Item[ItemId].Value;
            CalculatedStyle dimensions = GetDimensions();
            float scale = 1;
            if (tex.Width > (Width.Pixels - 12) || tex.Height > (Width.Pixels - 12))
            {
                scale = Math.Min((Width.Pixels - 12) / tex.Width, (Width.Pixels - 12) / tex.Height); // get the smallest scale
            }
            spriteBatch.Draw(tex, new Rectangle((int)(dimensions.X + dimensions.Width / 2 - tex.Width * scale / 2), (int)(dimensions.Y + dimensions.Height / 2 - tex.Height * scale / 2), (int)(tex.Width * scale), (int)(tex.Height * scale)), Color.White);
            if (IsMouseHovering)
            {
                Main.instance.MouseText(HoverText);
            }
        }

        public TileCopy GetAsTileCopy()
        {
            Tile tile = new Tile();
            if (CreateTile != -1)
            {
                tile.TileType = (ushort)CreateTile;
                tile.HasTile = true;
                tile.WallType = WallID.None;
                tile.Slope = 0;
                tile.IsHalfBlock = false;
                tile.TileColor = PaintID.None;
                tile.IsActuated = false;
                tile.RedWire = false;
                tile.BlueWire = false;
                tile.YellowWire = false;
                tile.GreenWire = false;
                tile.IsWallFullbright = false;
                tile.SkipLiquid = false;
                tile.IsTileInvisible = false;
                tile.IsWallInvisible = false;
                tile.CheckingLiquid = false;
                tile.HasActuator = false;
                tile.TileFrameNumber = 0;
                tile.LiquidAmount = 0;
                tile.IsTileFullbright = false;

                TileObjectData tileObjectData = TileObjectData.GetTileData(CreateTile, _placeStyle, 0) ?? TileObjectData.GetTileData(CreateTile, 0, 0);
                if (tileObjectData != null)
                {
                    // calculate TileFrameX and TileFrameY
                    if (tileObjectData.StyleHorizontal)
                    {
                        int x = (short)(tileObjectData.CoordinateFullWidth * _placeStyle * tileObjectData.StyleMultiplier);
                        int y = 0;

                        if (tileObjectData.StyleWrapLimit != 0)
                        {
                            int pixelsPerRow = tileObjectData.CoordinateFullWidth * tileObjectData.StyleWrapLimit;

                            // if we exceed the pixels per row that means we have to wrap around to the next row
                            if (x >= pixelsPerRow)
                            {
                                // calculate new coordinates
                                int row = x / pixelsPerRow;
                                y = row * tileObjectData.CoordinateFullHeight;
                                x = x % pixelsPerRow;
                            }
                        }

                        tile.TileFrameX = (short)x;
                        tile.TileFrameY = (short)y;
                    }
                    else
                    {
                        int x = 0;
                        int y = (short)(tileObjectData.CoordinateFullHeight * _placeStyle * tileObjectData.StyleMultiplier);

                        if (tileObjectData.StyleWrapLimit != 0)
                        {
                            int pixelsPerRow = tileObjectData.CoordinateFullHeight * tileObjectData.StyleWrapLimit;

                            if (y >= pixelsPerRow)
                            {
                                int row = y / pixelsPerRow;
                                x = row * tileObjectData.CoordinateFullHeight;
                                y = y % pixelsPerRow;
                            }
                        }

                        tile.TileFrameX = (short)x;
                        tile.TileFrameY = (short)y;
                    }
                }
                else
                {
                    // place temp tile
                    int tempX = 10;
                    int tempY = 10;

                    // kill the ones beside it to make sure they dont blend
                    WorldGen.KillTile(tempX + 1, tempY, false, false, true);
                    WorldGen.KillTile(tempX - 1, tempY, false, false, true);
                    WorldGen.KillTile(tempX, tempY + 1, false, false, true);
                    WorldGen.KillTile(tempX, tempY - 1, false, false, true);
                    WorldGen.PlaceTile(tempX, tempY, CreateTile, true, false, -1, _placeStyle);

                    // get TileFrameY and TileFrameX
                    tile.TileFrameX = Main.tile[tempX, tempY].TileFrameX;
                    tile.TileFrameY = Main.tile[tempX, tempY].TileFrameY;

                    // remove tile
                    WorldGen.KillTile(tempX, tempY, false, false, true);
                }
                tile.WallFrameX = 0;
                tile.WallFrameY = 0;
            }
            else
            {
                tile.WallType = (ushort)CreateWall;
                Main.instance.LoadWall(tile.WallType);
                tile.WallColor = PaintID.None;
                tile.TileType = TileID.Dirt;
                tile.HasTile = false;
                tile.TileFrameY = 0;
                tile.TileFrameX = 0;

                // default wall to WallFrameX = 36 and WallFrameY = 36
                // this is the WallFrameX and WallFrameY when the wall is 16x16
                tile.WallFrameX = 36;
                tile.WallFrameY = 36;
            }

            return new TileCopy(tile);
        }
    }
}