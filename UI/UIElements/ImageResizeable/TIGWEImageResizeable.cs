using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable
{
    internal class TIGWEImageResizeable : UIElement
    {
        public int CornerSize;
        public int BarSize;
        public bool ShouldResize;
        public Color Color = Color.White;
        public Asset<Texture2D> Texture;
        public Asset<Texture2D> TextureHover;

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
            // draw textures
            if (ShouldResize)
            {
                UIElementsUtils.DrawTexture2DWithDimensions(IsMouseHovering ? TextureHover.Value : Texture.Value, GetDimensions().ToRectangle(), spriteBatch, Color, CornerSize, BarSize);
            }
            else
            {
                UIElementsUtils.DrawTexture(IsMouseHovering ? TextureHover.Value : Texture.Value, (int)Width.Pixels, (int)Height.Pixels, this, spriteBatch);
            }
        }
    }
}
