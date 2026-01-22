using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.ButtonResizable;

namespace TerrariaInGameWorldEditor.UI.UIElements.DropDown
{
    internal class TIGWEDropDownItem : TIGWEImageButtonResizeable
    {
        public string Text => _dropDownText.Text;
        public int TextOffsetLeft { get { return (int)_dropDownText.Left.Pixels; } set { _dropDownText.Left.Set(value, 0f); } }
        public int TextOffsetTop { get { return (int)_dropDownText.Top.Pixels; } set { _dropDownText.Top.Set(value, 0f); } }
        public TIGWEDropDown DropDownParent { get; set; }

        private static Asset<Texture2D> texture = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture");
        private UIText _dropDownText;

        public TIGWEDropDownItem(string text) : base(texture)
        {
            TextureHover = (ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TextureHover"));
            _dropDownText = new UIText(text);
            _dropDownText.IgnoresMouseInteraction = true;
            Append(_dropDownText);
            TextOffsetLeft = 10;
            TextOffsetTop = 5;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            UIElement parent = Parent;
            Remove();
            parent.Append(this);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            if (!ContainsPoint(new Vector2(Main.mouseX, Main.mouseY)) || Parent.Parent == null)
            {
                base.MouseOut(evt);
            }
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            DropDownParent?.SetSelectedOption(Text);
        }
    }
}
