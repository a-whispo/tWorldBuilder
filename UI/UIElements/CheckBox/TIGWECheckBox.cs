using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UI.UIElements.CheckBox
{
    internal class TIGWECheckBox : UIImageButton
    {
        public delegate void OnCheckedChangedHandler(bool isChecked);
        public event OnCheckedChangedHandler OnCheckedChanged;

        public string HoverText = null;
        public bool IsChecked;
        private static Asset<Texture2D> _texture = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/CheckBox/CheckBoxUnchecked");

        public TIGWECheckBox(bool isChecked = false) : base(_texture)
        {
            IsChecked = isChecked;
            SetVisibility(1f, 1f);
            if (isChecked)
            {
                SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/CheckBox/CheckBoxChecked"));
            }
            else
            {
                SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/CheckBox/CheckBoxUnchecked"));
            }
            Width.Set(26, 0);
            Height.Set(26, 0);
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
            if (IsChecked)
            {
                SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/CheckBox/CheckBoxCheckedHover"));
            }
            else
            {
                SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/CheckBox/CheckBoxUncheckedHover"));
            }
            base.MouseOver(evt);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            if (IsChecked)
            {
                SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/CheckBox/CheckBoxChecked"));
            }
            else
            {
                SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/CheckBox/CheckBoxUnchecked"));
            }
            base.MouseOut(evt);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            IsChecked = !IsChecked;
            OnCheckedChanged.Invoke(IsChecked);
            MouseOver(evt); // update texture
        }
    }
}
