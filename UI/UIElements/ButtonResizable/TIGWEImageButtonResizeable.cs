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

        public TIGWEImageButtonResizeable(Asset<Texture2D> texture) : base(texture)
        {
            
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
    }
}
