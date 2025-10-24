using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UI.UIElements.TextField
{
    public class TIGWETextField : UIElement
    {
        public bool ShouldResize { get; set; } = true;
        public bool IsFocused { get; set; } = false;
        public bool CanFocus { get; set; } = true;
        public string PlaceholderText { get; set; }
        public Asset<Texture2D> Texture { get; set; }
        public Asset<Texture2D> TextureHover { get; set; }

        private bool _isPlaceholderTextActive;
        private int _maxTextLength;
        private UIText _tfText;
        private int _textBlink;
        private bool _isMouseHovering;
        private string _currentText = "";

        public TIGWETextField(string placeholderText = "Enter text...", int maxTextLength = 30, int textOffsetLeft = 10, int textOffsetDown = 0)
        {
            this.PlaceholderText = placeholderText;
            this._maxTextLength = maxTextLength;
            Texture = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture");
            TextureHover = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TextureHover");
            _tfText = new UIText("");
            _tfText.IgnoresMouseInteraction = true;
            _tfText.Left.Set(textOffsetLeft, 0);
            _tfText.Top.Set(8 - textOffsetDown, 0);
            Append(_tfText);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (_isMouseHovering || IsFocused) // draw textures
            {
                if (ShouldResize)
                {
                    UIElementsUtils.DrawTexture2DWithDimensions(TextureHover.Value, this.GetDimensions().ToRectangle(), spriteBatch);
                }
                else
                {
                    UIElementsUtils.DrawTexture(TextureHover.Value, (int)Width.Pixels, (int)Height.Pixels, this, spriteBatch);
                }
            }
            else
            {
                if (ShouldResize)
                {
                    UIElementsUtils.DrawTexture2DWithDimensions(Texture.Value, this.GetDimensions().ToRectangle(), spriteBatch);
                }
                else
                {
                    UIElementsUtils.DrawTexture(Texture.Value, (int)Width.Pixels, (int)Height.Pixels, this, spriteBatch);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsFocused)
            {
                // unfocus if enter or escape is pressed
                // check where player clicked, if its outside the textfield, unfocus
                if (Main.keyState.IsKeyDown(Keys.Enter) || Main.keyState.IsKeyDown(Keys.Escape) || Mouse.GetState().LeftButton == ButtonState.Pressed && !_isMouseHovering)
                {
                    IsFocused = false;
                }

                // check if visualName should be placeholder visualName or the typed string
                PlayerInput.WritingText = true;
                Main.instance.HandleIME();
                string newText = Main.GetInputText(_currentText);
                if (newText != _currentText && newText.Length < _maxTextLength + 1)
                {
                    _currentText = newText;
                }
            }

            // set visualName to placeholder visualName if we havent written anything
            string text = _currentText.Length == 0 && !IsFocused ? PlaceholderText : _currentText;
            _isPlaceholderTextActive = (_currentText.Length == 0 && !IsFocused);

            // visualName blinker thing
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
                    }
                }
            }
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            _isMouseHovering = true;
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
            base.MouseOver(evt);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            _isMouseHovering = false;
            base.MouseOver(evt);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            if (CanFocus)
            {
                IsFocused = true;
            }
            base.LeftClick(evt);
        }

        public string GetText()
        {
            if (_isPlaceholderTextActive)
            {
                return "";
            }
            else
            {
                if (_textBlink / 30 % 2 == 0 && IsFocused) // remove the visualName blinker if its there
                {
                    return _tfText.Text.Substring(0, _tfText.Text.Length - 1);
                }
                else
                {
                    return _tfText.Text;
                }
            }
        }

        public bool HasText()
        {
            if (GetText().Length > 0 && !_isPlaceholderTextActive)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public void SetText(string text)
        {
            _currentText = text;
        }
    }
}