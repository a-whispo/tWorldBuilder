using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace TerrariaInGameWorldEditor.UI.UIElements.Button
{
    public class TIGWEButton : UIImageButton
    {
        // events
        public delegate void DrawSelfEventHandler(SpriteBatch spriteBatch);
        public event DrawSelfEventHandler PostDrawingSelf;
        public string HoverText { get; set; } = null;

        public TIGWEButton(Asset<Texture2D> texture) : base(texture)
        {
            // just a version of UIImageButton that allows for hover text
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            PostDrawingSelf?.Invoke(spriteBatch);
            base.DrawSelf(spriteBatch);
            if (HoverText != null && IsMouseHovering)
            {
                Main.instance.MouseText(HoverText);
            }
        }
    }
}
