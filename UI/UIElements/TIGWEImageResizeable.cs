using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UI.UIElements
{
    internal class TIGWEImageResizeable : UIElement
    {
        // public
        public int CornerSize { get; set; }
        public int BarSize { get; set; }
        public bool ShouldResize { get; set; }
        public Color Color { get; set; } = Color.White;

        // private
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
                    UIUtils.DrawTexture2DWithDimensions(TextureHover.Value, this.GetDimensions().ToRectangle(), spriteBatch, Color, CornerSize, BarSize);
                }
                else
                {
                    UIUtils.DrawTexture(TextureHover.Value, (int)Width.Pixels, (int)Height.Pixels, this, spriteBatch);
                }
            }
            else
            {
                if (ShouldResize)
                {
                    UIUtils.DrawTexture2DWithDimensions(Texture.Value, this.GetDimensions().ToRectangle(), spriteBatch, Color, CornerSize, BarSize);
                }
                else
                {
                    UIUtils.DrawTexture(Texture.Value, (int)Width.Pixels, (int)Height.Pixels, this, spriteBatch);
                }
            }
        }
    }
}
