using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common.Utils;

namespace TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable
{
    internal class TIGWEImageResizeable : UIElement
    {
        public int CornerSize { get; set; }
        public int BarSize { get; set; }
        public bool ShouldResize { get; set; }
        public Color Color { get; set; } = Color.White;
        public Asset<Texture2D> Texture { get; set; }
        public Asset<Texture2D> TextureHover { get; set; }

        public TIGWEImageResizeable(Asset<Texture2D> texture, int cornerSize = 8, int barSize = 16, bool shouldResize = true)
        {
            Texture = texture;
            TextureHover = texture;
            CornerSize = cornerSize;
            BarSize = barSize;
            ShouldResize = shouldResize;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (IsMouseHovering) // draw textures
            {
                if (ShouldResize)
                {
                    UIElementsUtils.DrawTexture2DWithDimensions(TextureHover.Value, GetDimensions().ToRectangle(), spriteBatch, Color, CornerSize, BarSize);
                }
                else
                {
                    UIElementsUtils.DrawTexture(TextureHover.Value, (int)Width.Pixels, (int)Height.Pixels, this, spriteBatch);
                }
            }
            else
            {
                if (ShouldResize)
                {
                    UIElementsUtils.DrawTexture2DWithDimensions(Texture.Value, GetDimensions().ToRectangle(), spriteBatch, Color, CornerSize, BarSize);
                }
                else
                {
                    UIElementsUtils.DrawTexture(Texture.Value, (int)Width.Pixels, (int)Height.Pixels, this, spriteBatch);
                }
            }
        }
    }
}
