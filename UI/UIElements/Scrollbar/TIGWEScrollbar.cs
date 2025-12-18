using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UI.UIElements.Scrollbar
{
    internal class TIGWEScrollbar : UIScrollbar
    {
        // this class is just for custom scrollbar textures
        private readonly Asset<Texture2D> _texture;
        private readonly Asset<Texture2D> _innerTexture;

        public TIGWEScrollbar(Asset<Texture2D> texture, Asset<Texture2D> innerTexture)
        {
            // texture is the background and innerTexture is the scrollbar
            this._texture = texture;
            this._innerTexture = innerTexture;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // do all base calculations
            base.DrawSelf(spriteBatch);

            // add custom textures (technically the original textures are still underneath but lets just ignore that)
            CalculatedStyle dimensionsRectangle = new CalculatedStyle(GetDimensions().X, GetDimensions().Y - 6, GetDimensions().Width, GetDimensions().Height + 12);
            UIElementsUtils.DrawTexture2DWithDimensions(_texture.Value, dimensionsRectangle.ToRectangle(), spriteBatch);

            CalculatedStyle handleRectangle = new CalculatedStyle((int)GetInnerDimensions().X + 6, (int)(GetInnerDimensions().Y + GetInnerDimensions().Height * (ViewPosition / MaxViewSize)) - 5, 8, (int)(GetInnerDimensions().Height * (ViewSize / MaxViewSize)) + 11);
            UIElementsUtils.DrawTexture2DWithDimensions(_innerTexture.Value, handleRectangle.ToRectangle(), spriteBatch, default, 4, 8);
        }
    }
}
