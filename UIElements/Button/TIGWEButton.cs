using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace TerrariaInGameWorldEditor.UIElements.Button
{
    public class TIGWEButton : UIImageButton
    {
        public string HoverText { get; set; }
        public string Text
        {
            get => _btnText.Text;
            set => _btnText.SetText(value);
        }
        public int TextOffsetLeft
        {
            get => (int)_btnText.Left.Pixels;
            set => _btnText.PaddingLeft = value;
        }
        public int TextOffsetTop
        {
            get => (int)_btnText.Top.Pixels;
            set => _btnText.PaddingTop = value;
        }

        private UIText _btnText = new UIText("");

        public TIGWEButton(Asset<Texture2D> texture) : base(texture)
        {
            _btnText.IgnoresMouseInteraction = true;
            Append(_btnText);
            TextOffsetLeft = 10;
            TextOffsetTop = 5;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            if (HoverText != null && IsMouseHovering)
            {
                Main.instance.MouseText(HoverText);
            }
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }
    }
}
