using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace TerrariaInGameWorldEditor.UIElements.Button
{
    public class TIGWEButton : UIImageButton
    {
        public LocalizedText HoverText { get; set; }

        public TIGWEButton(Asset<Texture2D> texture) : base(texture)
        {
            SetVisibility(0.8f, 1f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            if (HoverText != null && IsMouseHovering)
            {
                Main.instance.MouseText(HoverText.Value);
            }
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }
    }
}
