using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UI.UIElements.ButtonResizable
{
    internal class TIGWEImageButtonResizeable : TIGWEImageResizeable
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

        private UIText _btnText;
        private float _visibilityActive = 1f;
        private float _visibilityInactive = 1f;

        public TIGWEImageButtonResizeable(Asset<Texture2D> texture) : base(texture)
        {
            _btnText = new UIText("");
            _btnText.IgnoresMouseInteraction = true;
            Append(_btnText);
            TextOffsetLeft = 10;
            TextOffsetTop = 5;
        }

        public void SetVisibility(float whenActive, float whenInactive)
        {
            _visibilityActive = MathHelper.Clamp(whenActive, 0f, 1f);
            _visibilityInactive = MathHelper.Clamp(whenInactive, 0f, 1f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            Color = Color.White * (base.IsMouseHovering ? _visibilityActive : _visibilityInactive);
            if (HoverText != null && IsMouseHovering)
            {
                Main.instance.MouseText(HoverText);
            }
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
        }
    }
}
