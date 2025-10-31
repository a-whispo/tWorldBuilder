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
        public string HoverText = null;
        private UIText _text = new UIText("");

        public TIGWEImageButtonResizeable(Asset<Texture2D> texture) : base(texture)
        {
            OverrideSamplerState = SamplerState.PointClamp;
            _text.Left.Set(10, 0);
            _text.Top.Set(7, 0);
            _text.IgnoresMouseInteraction = true;
            Append(_text);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            if (HoverText != null && IsMouseHovering)
            {
                Main.instance.MouseText(HoverText);
            }
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            if (IsMouseHovering)
            {
                SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
            }
        }

        public void SetText(string text)
        {
            _text.SetText(text);
        }
    }
}
