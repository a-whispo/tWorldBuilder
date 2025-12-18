using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UI.UIElements.TextField
{
    public class TIGWETextField : UIElement
    {
        public delegate void TextChangedEventHandler(string newText);
        public event TextChangedEventHandler OnTextChanged;
        public bool IsFocused { get; set; } = false;
        public bool CanFocus { get; set; } = true;
        public string PlaceholderText { get; set; }
        public int TextOffsetLeft { get { return (int)_tfText.Left.Pixels; } set { _tfText.Left.Set(value, 0f); } }
        public int TextOffsetTop { get { return (int)_tfText.Top.Pixels; } set { _tfText.Top.Set(value, 0f); } }

        private bool _isPlaceholderTextActive;
        private int _maxTextLength;
        private UIText _tfText;
        private TIGWEImageResizeable _background;
        private int _textBlink;
        private string _currentText = "";

        public TIGWETextField(string placeholderText = "Enter text...", int maxTextLength = 30)
        {
            this.PlaceholderText = placeholderText;
            this._maxTextLength = maxTextLength;

            // background of the field
            _background = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"));
            _background.TextureHover = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TextureHover");
            _background.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                if (CanFocus)
                {
                    IsFocused = true;
                }
            };
            Append(_background);

            // actual text
            _tfText = new UIText("");
            _tfText.IgnoresMouseInteraction = true;
            Append(_tfText);

            // default offsets
            TextOffsetLeft = 10;
            TextOffsetTop = 5;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsFocused)
            {
                // unfocus if enter or escape is pressed
                // check where player clicked, if its outside the textfield, unfocus
                if (Main.keyState.IsKeyDown(Keys.Enter) || Main.keyState.IsKeyDown(Keys.Escape) || Mouse.GetState().LeftButton == ButtonState.Pressed && !_background.IsMouseHovering)
                {
                    IsFocused = false;
                }

                // check if text should be placeholder text or the typed string
                PlayerInput.WritingText = true;
                Main.instance.HandleIME();
                string newText = Main.GetInputText(_currentText);
                if (newText != _currentText && newText.Length < _maxTextLength + 1)
                {
                    _currentText = newText;
                    OnTextChanged?.Invoke(_currentText);
                }
            }

            // set text to placeholder text if we havent written anything
            string text = _currentText.Length == 0 && !IsFocused ? PlaceholderText : _currentText;
            _isPlaceholderTextActive = (_currentText.Length == 0 && !IsFocused);

            // text blinker thing
            if (++_textBlink / 30 % 2 == 0 && IsFocused)
            {
                text += "|";
            }
            _tfText.SetText(text);

            if (IsFocused)
            {
                if ((Main.inputText.IsKeyDown(Keys.LeftControl) || Main.inputText.IsKeyDown(Keys.RightControl)) && !(Main.inputText.IsKeyDown(Keys.LeftAlt) || Main.inputText.IsKeyDown(Keys.RightAlt)))
                {
                    if (Main.inputText.IsKeyDown(Keys.Back) && !Main.oldInputText.IsKeyDown(Keys.Back))
                    {
                        string[] words;
                        words = _currentText.Split(" ", System.StringSplitOptions.None);
                        _currentText = "";
                        for (int i = 0; i < words.Length - 1; i++)
                        {
                            _currentText = _currentText + words[i] + " ";
                        }
                        OnTextChanged?.Invoke(_currentText);
                    }
                }
            }

            // update offset
            _background.Width.Set(Width.Pixels, 0f);
            _background.Height.Set(Height.Pixels, 0f);

            // this is kinda weird but ok
            _background.Texture = IsFocused ? _background.TextureHover : ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture");
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
            base.MouseOver(evt);
        }

        public virtual string GetText()
        {
            if (_isPlaceholderTextActive)
            {
                return "";
            }
            else
            {
                if (_textBlink / 30 % 2 == 0 && IsFocused) // remove the blinker if its there
                {
                    return _tfText.Text.Substring(0, _tfText.Text.Length - 1);
                }
                else
                {
                    return _tfText.Text;
                }
            }
        }

        public virtual void SetText(string text)
        {
            _currentText = text;
            OnTextChanged?.Invoke(_currentText);
        }
    }
}