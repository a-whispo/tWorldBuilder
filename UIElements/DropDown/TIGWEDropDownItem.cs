using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements.ButtonResizable;

namespace TerrariaInGameWorldEditor.UIElements.DropDown
{
    internal class TIGWEDropDownItem<T> : UIElement
    {
        public T Value { get; set; }
        public string Text => _body.Text;
        private TIGWEImageButtonResizeable _body;

        public TIGWEDropDownItem(T value, string text)
        {
            _body = new TIGWEImageButtonResizeable(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture"));
            _body.Width.Set(0, 1);
            _body.Height.Set(0, 1);
            _body.TextureHover = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/TextureHover");
            _body.Text = text;
            Append(_body);
            Value = value;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            UIElement parent = Parent;
            Remove();
            parent.Append(this);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }
    }
}
