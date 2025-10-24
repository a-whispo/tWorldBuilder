using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.TileSelector
{
    internal class SelectTileItem : UIElement
    {
        public int ItemId;
        public int CreateTile;
        public int CreateWall;
        public int PlaceStyle;
        public string Name;
        public int TileWidth;
        public int TileHeight;
        public bool HasItem;

        private TIGWEImageResizeable _texture;
        private Texture2D _itemTexture;

        public SelectTileItem(int? itemId = null)
        {
            // if an item id was passed in load it and get the texture and stuff
            if (itemId != null)
            {
                Main.instance.LoadItem((int)itemId);
                Item item = new Item((int)itemId);
                Name = item.Name;
                this.ItemId = (int)itemId;
                CreateTile = item.createTile;
                CreateWall = item.createWall;
                PlaceStyle = item.placeStyle;
                _itemTexture = TextureAssets.Item[(int)itemId].Value;
                HasItem = true;
            }

            // set width and height
            Width.Set(44, 0);
            Height.Set(44, 0);

            // texture
            _texture = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"));
            _texture.Width.Set(44, 0);
            _texture.Height.Set(44, 0);
            _texture.IgnoresMouseInteraction = true;
            Append(_texture);
            PaddingTop = 2;
            PaddingBottom = 2;

            OnMouseOver += (evt, element) =>
            {
                SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
                _texture.Texture = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TextureHover");
            };

            OnMouseOut += (evt, element) =>
            {
                _texture.Texture = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture");
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_itemTexture != null)
            {
                Rectangle tileButtonRect = GetDimensions().ToRectangle();
                // get texture from spritesheet with help from frameX and frameY and draw it
                double scale = 1;
                if (_itemTexture.Width > Width.Pixels - 12 || _itemTexture.Height > Width.Pixels - 12)
                {
                    scale = Math.Min((Width.Pixels - 12) / (double)_itemTexture.Width, (Width.Pixels - 12) / (double)_itemTexture.Height); // get the smallest scale
                }
                // calculate dimensions
                int width = (int)(_itemTexture.Width * scale);
                int height = (int)(_itemTexture.Height * scale);
                int x = ((int)Width.Pixels - width) / 2;
                int y = ((int)Height.Pixels - height) / 2;

                // draw texture
                spriteBatch.Draw(_itemTexture, new Rectangle(tileButtonRect.X + x, tileButtonRect.Y + y, width, height), Color.White);
            }

        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            if (IsMouseHovering)
            {
                Main.instance.MouseText($"{Name}");
            }
        }

        public void SetItem(int itemId)
        {
            try
            {
                Main.instance.LoadItem(itemId);
                Item item = new Item(itemId);
                Name = item.Name;
                this.ItemId = itemId;
                CreateTile = item.createTile;
                CreateWall = item.createWall;
                PlaceStyle = item.placeStyle;
                _itemTexture = TextureAssets.Item[itemId].Value;
                HasItem = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting item: {ex}");
            }
        }

        public TileCopy GetAsTileCopy()
        {
            if (HasItem)
            {
                try
                {
                    Tile tile = new Tile();
                    if (CreateTile != -1)
                    {
                        // important
                        tile.TileType = (ushort)CreateTile;
                        tile.HasTile = true;
                        tile.WallType = 0;

                        var tileObjectData = (TileObjectData.GetTileData(CreateTile, PlaceStyle, 0) ?? TileObjectData.GetTileData(CreateTile, 0, 0)) ?? null;
                        if (tileObjectData != null)
                        {
                            // calculate tileframex and tileframey
                            if (tileObjectData.StyleHorizontal)
                            {
                                // get values
                                int x = (short)(tileObjectData.CoordinateFullWidth * PlaceStyle * tileObjectData.StyleMultiplier);
                                int y = 0;

                                if (tileObjectData.StyleWrapLimit != 0)
                                {
                                    // calculate pixels per row
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
                                int y = (short)(tileObjectData.CoordinateFullHeight * PlaceStyle * tileObjectData.StyleMultiplier);

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
                            // place a temporary tile
                            int tempX = Main.maxTilesX - 10;
                            int tempY = Main.maxTilesY - 10;

                            // place temp tile
                            WorldGen.PlaceTile(tempX, tempY, CreateTile, true, false, -1, PlaceStyle);

                            // get tileframey and tileframex
                            TileCopy tc = new TileCopy(Main.tile[tempX, tempY]);
                            tile.TileFrameX = tc.TileFrameX;
                            tile.TileFrameY = tc.TileFrameY;

                            // remove tile
                            WorldGen.KillTile(tempX, tempY, false, false, true);
                        }
                    }
                    else
                    {
                        tile.WallType = (ushort)CreateWall;
                        Main.instance.LoadWall(tile.WallType);
                        tile.WallColor = 0;
                        tile.TileType = 0;
                        tile.HasTile = false;

                        // default wall to wallframex = 36 and wallframey = 36
                        // this is the wallframex and wallframey when the wall is 16x16
                        tile.WallFrameX = 36;
                        tile.WallFrameY = 36;
                    }

                    return new TileCopy(tile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting to TileCopy: {ex}");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}