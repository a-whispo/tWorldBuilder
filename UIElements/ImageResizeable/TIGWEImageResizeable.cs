using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UIElements.ImageResizeable
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
            OverrideSamplerState = SamplerState.PointClamp;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            // draw textures
            if (ShouldResize)
            {
                UIElementUtils.DrawTexture2DWithDimensions(IsMouseHovering ? TextureHover.Value : Texture.Value, GetDimensions().ToRectangle(), Color, CornerSize, BarSize);
            }
            else
            {
                UIElementUtils.DrawTexture(IsMouseHovering ? TextureHover.Value : Texture.Value, (int)Width.Pixels, (int)Height.Pixels, this);
            }
        }
    }
}
