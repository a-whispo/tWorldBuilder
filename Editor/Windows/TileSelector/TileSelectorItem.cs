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
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.Editor.Windows.TileSelector
{
    internal class TileSelectorItem : UIElement
    {
        public string Name { get; set; }
        public int ItemId { get; set; }
        public string HoverText { get; set; }

        private int _createTile;
        private int _createWall;
        private int _placeStyle;

        public TileSelectorItem(int itemId)
        {
            // load item
            Item item = ContentSamples.ItemsByType[itemId];
            string[] name = ItemID.Search.GetName(itemId).Split('/');
            Name = name.Length > 1 ? name[1] : name[0];
            ItemId = itemId;
            _createTile = item.createTile;
            _createWall = item.createWall;
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
            if (_createTile != -1)
            {
                tile.TileType = (ushort)_createTile;
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

                var tileObjectData = (TileObjectData.GetTileData(_createTile, _placeStyle, 0) ?? TileObjectData.GetTileData(_createTile, 0, 0)) ?? null;
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
                    WorldGen.PlaceTile(tempX, tempY, _createTile, true, false, -1, _placeStyle);

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
                tile.WallType = (ushort)_createWall;
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