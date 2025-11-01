using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.UI.UIElements.Button;

namespace TerrariaInGameWorldEditor.UI.Editor
{
    internal class PaletteItem : TIGWEButton
    {
        public TileCopy TileCopy;

        public PaletteItem(TileCopy tileCopy) : base(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/PaletteItem"))
        {
            TileCopy = tileCopy;
            Width.Set(30, 0f);
            Height.Set(30, 0f);
            SetVisibility(0.5f, 1f);
            OnLeftClick += (evt, listeningElement) =>
            {
                if (Parent.Parent.Parent is EditorPalette palette && palette.IsDeletingItems)
                {
                    palette.RemoveItem(this);
                }
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (TileCopy == null)
            {
                return;
            }

            // draw the tile on top
            TilePaintSystemV2 ps = Main.instance.TilePaintSystem;
            Rectangle rect = GetDimensions().ToRectangle();
            int x = rect.X + rect.Width / 2 - 16 / 2;
            int y = rect.Y + rect.Height / 2 - 16 / 2;

            // wall part
            if (TileCopy.WallType != 0)
            {

                // get wall texture with tilepaintsystem to get the texture with paint if it has any
                Texture2D tileWallTex = ps.TryGetWallAndRequestIfNotReady(TileCopy.WallType, TileCopy.WallColor);
                if (tileWallTex != null)
                {
                    // get texture from spritesheet with help from frameX and frameY and draw it
                    spriteBatch.Draw(tileWallTex, new Rectangle(x - 8, y - 8, 32, 32), new Rectangle(TileCopy.WallFrameX, TileCopy.WallFrameY, 32, 32), Color.White * (IsMouseHovering ? 0.8f : 1f));
                }
            }

            // tile part
            if (TileCopy   .HasTile)
            {
                // get tile texture with tilepaintsystem to get the texture with paint if it has any
                Texture2D tileTex = ps.TryGetTileAndRequestIfNotReady(TileCopy.TileType, TileCopy.TileFrameNumber, TileCopy.TileColor);

                if (tileTex != null)
                {
                    // get texture from spritesheet with help from frameX and frameY and draw it
                    spriteBatch.Draw(tileTex, new Rectangle(x, y, 16, 16), new Rectangle(TileCopy.TileFrameX, TileCopy.TileFrameY, 16, 16), Color.White * (IsMouseHovering ? 0.8f : 1f));

                }
            }

            // liquid
            if (TileCopy.LiquidAmount > 0)
            {
                int num = 0;
                int height = (int)(((float)TileCopy.LiquidAmount / 255) * 16);
                if (height < 6)
                {
                    height = 6;
                }
                spriteBatch.Draw(TextureAssets.Liquid[TileCopy.LiquidType].Value, new Rectangle(x, y + (16 - height), 16, height), new Rectangle(0, num, 16, height), Color.White * 0.6f * (IsMouseHovering ? 0.8f : 1f));
            }

            // wire
            if ((TileCopy.GreenWire || TileCopy.RedWire || TileCopy.YellowWire || TileCopy.BlueWire))
            {
                Rectangle boundsTile = new Rectangle(x, y, 16, 16);

                void DrawWire(string wireType, float t)
                {
                    Texture2D texture = TextureAssets.Wire.Value;
                    switch (wireType)
                    {
                        case "Red":
                            texture = TextureAssets.Wire.Value;
                            break;
                        case "Blue":
                            texture = TextureAssets.Wire2.Value;
                            break;
                        case "Green":
                            texture = TextureAssets.Wire3.Value;
                            break;
                        case "Yellow":
                            texture = TextureAssets.Wire4.Value;
                            break;
                    }

                    // none
                    spriteBatch.Draw(texture, boundsTile, new Rectangle(0, 54, 16, 16), Color.White * t * (IsMouseHovering ? 0.8f : 1f));
                }

                // draw the wires
                if (TileCopy.RedWire)
                {
                    DrawWire("Red", 0.65f);
                }
                if (TileCopy.BlueWire)
                {
                    DrawWire("Blue", 0.4f);
                }
                if (TileCopy.GreenWire)
                {
                    DrawWire("Green", 0.4f);
                }
                if (TileCopy.YellowWire)
                {
                    DrawWire("Yellow", 0.4f);
                }
            }

            // actuator
            if (TileCopy.HasActuator)
            {
                Rectangle boundsTile = new Rectangle(x, y, 16, 16);

                Texture2D texture = TextureAssets.Actuator.Value;
                spriteBatch.Draw(texture, boundsTile, Color.White * 0.6f * (IsMouseHovering ? 0.8f : 1f));
            }
            base.Draw(spriteBatch);

            // draw an x over it if we are deleting items
            if (Parent.Parent.Parent is EditorPalette palette && palette.IsDeletingItems)
            {
                Vector2 position = GetDimensions().Position();
                position.X += 7;
                position.Y += 7;
                spriteBatch.Draw(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/X").Value, position, new Color(215, 215, 215) * 0.9f * (IsMouseHovering ? 0.6f : 1f));
            }
        }
    }
}
