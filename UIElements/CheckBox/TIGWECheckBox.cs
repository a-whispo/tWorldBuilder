using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

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
                SetImage(IsChecked ? _textureChecked : _textureUnchecked);
            }
        }

        private bool _isChecked;
        private static Asset<Texture2D> _textureUnchecked = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/CheckBoxUnchecked");
        private static Asset<Texture2D> _textureUncheckedHover = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/CheckBoxUncheckedHover");
        private static Asset<Texture2D> _textureChecked = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/CheckBoxChecked");
        private static Asset<Texture2D> _textureCheckedHover = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/CheckBox/CheckBoxCheckedHover");

        public TIGWECheckBox(bool isChecked = false) : base(_textureUnchecked)
        {
            IsChecked = isChecked;
            SetVisibility(1f, 1f);
            SetImage(IsChecked ? _textureChecked : _textureUnchecked);
            Width.Set(26, 0);
            Height.Set(26, 0);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            if (HoverText != null && IsMouseHovering)
            {
                Main.instance.MouseText(HoverText);
            }
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            SetImage(IsChecked ? _textureCheckedHover : _textureUncheckedHover);
            base.MouseOver(evt);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            SetImage(IsChecked ? _textureChecked : _textureUnchecked);
            base.MouseOut(evt);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            IsChecked = !IsChecked;
            OnCheckedChanged?.Invoke(IsChecked);
            SetImage(IsChecked ? _textureCheckedHover : _textureUncheckedHover);
        }
    }
}
