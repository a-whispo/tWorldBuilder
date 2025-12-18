using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
            TIGWEDropDown parent = (TIGWEDropDown)Parent;
            parent.RemoveChild(this);
            parent.Append(this);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            TIGWEDropDown parent = (TIGWEDropDown)Parent;
            parent.SetSelectedOption(Text);
        }
    }
}
