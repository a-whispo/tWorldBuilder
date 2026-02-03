using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements;
using TerrariaInGameWorldEditor.UIElements.ButtonResizable;

namespace TerrariaInGameWorldEditor.UIElements.DropDown
{
    internal class TIGWEDropDownItem<T> : TIGWEImageButtonResizeable
    {
        public T Value { get; set; }

        public TIGWEDropDownItem(T value, string text) : base(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture"))
        {
            TextureHover = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/TextureHover");
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
