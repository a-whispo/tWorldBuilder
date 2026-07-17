using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace TerrariaInGameWorldEditor.UIElements.ScrollText
{
    internal class TIGWEScrollText : UIElement
    {
        public bool ShouldScroll { get; set; } = true;
        public int TextOffsetLeft
        {
            get => (int)_text.PaddingLeft;
            set => _text.PaddingLeft = value;
        }
        public int TextOffsetTop
        {
            get => (int)_text.PaddingTop;
            set => _text.PaddingTop = value;
        }
        public float ScrollSpeed { get; set; } = 1f;
        public Color TextColor 
        {
            get => _text.TextColor;
            set => _text.TextColor = value;
        }

        private UIText _text;
        private UIElement _clipContainer;
        private float _textScroll = 0;
        private bool _scrollRight = false;

        public TIGWEScrollText(string text) : this(new UIText(text))
        {

        }

        public TIGWEScrollText(LocalizedText text) : this(new UIText(text))
        {

        }

        private TIGWEScrollText(UIText uiText)
        {
            _clipContainer = new UIElement();
            _clipContainer.OverflowHidden = true;
            IgnoresMouseInteraction = true;
            _text = uiText;
            _clipContainer.Append(_text);
            Append(_clipContainer);
            Height.Set(40, 0);
            TextOffsetLeft = 10;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float textWidth = ChatManager.GetStringSize(FontAssets.MouseText.Value, GetText(), new Vector2(1)).X;

            // auto adjust width if it hasnt been set
            if (Width.Pixels == 0f)
            {
                if (Parent != null)
                {
                    Width.Set(Parent.Width.Pixels - Left.Pixels - 6, 0);
                }
            }

            // text scroll thing
            if (textWidth > Width.Pixels && ShouldScroll)
            {
                _textScroll += _scrollRight ? 0.25f * ScrollSpeed : -0.25f * ScrollSpeed;
                if (textWidth + _textScroll < _clipContainer.Width.Pixels - 20)
                {
                    _scrollRight = true;
                }
                if (_textScroll >= 0)
                {
                    _scrollRight = false;
                }
                _text.Left.Set(_textScroll, 0);
            }
            else
            {
                _textScroll = 0;
                if (!ShouldScroll)
                {
                    _text.Left.Set(Math.Clamp(_clipContainer.Width.Pixels - textWidth - 20, int.MinValue, -6), 0);
                }
                else
                {
                    _text.Left.Set(-6, 0);
                }
            }
        }

        public override void Recalculate()
        {
            base.Recalculate();
            _clipContainer.Width.Set(Width.Pixels, 0);
            _clipContainer.Height.Set(Height.Pixels, 0);
        }

        public string GetText()
        {
            return _text.Text;
        }

        public void SetText(string text)
        {
            _text.SetText(text);
        }

        public void SetText(LocalizedText text)
        {
            _text.SetText(text);
        }
    }
}
