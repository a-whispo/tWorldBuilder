using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.ButtonResizable;

namespace TerrariaInGameWorldEditor.UI.UIElements.DropDown
{
    internal class TIGWEDropDownItem<T> : TIGWEImageButtonResizeable
    {
        public T Value { get; set; }

        public TIGWEDropDownItem(T value, string text) : base(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"))
        {
            TextureHover = (ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TextureHover"));
            Text = text;
            Value = value;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            UIElement parent = Parent;
            Remove();
            parent.Append(this);
        }
    }
}
