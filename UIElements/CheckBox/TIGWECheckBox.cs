using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements;

namespace TerrariaInGameWorldEditor.UIElements.CheckBox
{
    internal class TIGWECheckBox : UIImageButton
    {
        public delegate void CheckedChangedEventHandler(bool isChecked);
        public event CheckedChangedEventHandler OnCheckedChanged;
        public string HoverText { get; set; }
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/{(IsChecked ? "CheckBoxChecked" : "CheckBoxUnchecked")}"));
            }
        }

        private bool _isChecked;
        private static Asset<Texture2D> _texture = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/CheckBoxUnchecked");

        public TIGWECheckBox(bool isChecked = false) : base(_texture)
        {
            IsChecked = isChecked;
            SetVisibility(1f, 1f);
            if (isChecked)
            {
                SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/CheckBoxChecked"));
            }
            else
            {
                SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/CheckBoxUnchecked"));
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
            SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/{(IsChecked ? "CheckBoxCheckedHover" : "CheckBoxUncheckedHover")}"));
            base.MouseOver(evt);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/{(IsChecked ? "CheckBoxChecked" : "CheckBoxUnchecked")}"));
            base.MouseOut(evt);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            IsChecked = !IsChecked;
            OnCheckedChanged?.Invoke(IsChecked);
            SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/{(IsChecked ? "CheckBoxCheckedHover" : "CheckBoxUncheckedHover")}"));
        }
    }
}
