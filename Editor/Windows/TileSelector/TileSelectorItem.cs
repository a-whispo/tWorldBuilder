using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
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

        private int _createTile;
        private int _createWall;
        private int _placeStyle;
        private Item _item;
        private TIGWEImageResizeable _body = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));

        public TileSelectorItem(int itemId)
        {
            // load item
            _item = new Item(itemId);
            Name = _item.Name;
            ItemId = itemId;
            _createTile = _item.createTile;
            _createWall = _item.createWall;
            _placeStyle = _item.placeStyle;

            // ui and events
            Width.Set(44, 0);
            Height.Set(44, 0);
            _body.Width.Set(44, 0);
            _body.Height.Set(44, 0);
            _body.IgnoresMouseInteraction = true;
            Append(_body);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Main.inventoryScale = 1f;
            ItemSlot.Draw(spriteBatch, ref _item, 21, new Vector2(GetDimensions().X - 4, GetDimensions().Y - 4));
            if (IsMouseHovering)
            {
                Main.instance.MouseText($"{Name}");
            }
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
            _body.Texture = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/TextureHover");
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            _body.Texture = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture");
        }

        public TileCopy GetAsTileCopy()
        {
            Tile tile = new Tile();
            if (_createTile != -1)
            {
                // important
                tile.TileType = (ushort)_createTile;
                tile.HasTile = true;
                tile.WallType = WallID.None;

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
                    int tempX = Main.maxTilesX - 10;
                    int tempY = Main.maxTilesY - 10;
                    WorldGen.PlaceTile(tempX, tempY, _createTile, true, false, -1, _placeStyle);

                    // get TileFrameY and TileFrameX
                    TileCopy tc = new TileCopy(Main.tile[tempX, tempY]);
                    tile.TileFrameX = tc.TileFrameX;
                    tile.TileFrameY = tc.TileFrameY;

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