using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.ScrollText;

namespace TerrariaInGameWorldEditor.UIElements.TextField
{
    public class TIGWETextField : UIElement
    {
        public delegate void TextChangedEventHandler(string newText);
        public event TextChangedEventHandler OnTextChanged;
        public bool IsFocused { get; set; } = false;
        public bool CanFocus { get; set; } = true;
        public bool ShowSearchIcon { get; set; } = false;
        public string PlaceholderText { get; set; }

        private bool _isPlaceholderTextActive;
        private int _maxTextLength;
        private TIGWEScrollText _tfText;
        private TIGWEImageResizeable _background;
        private int _textBlink;
        private string _currentText = "";
        private Asset<Texture2D> _searchIcon;

        public TIGWETextField(string placeholderText = null, int maxTextLength = 30)
        {
            PlaceholderText = placeholderText ?? LocalizationUtils.GetTextValue("UIElements.TextField.PlaceholderText");
            _maxTextLength = maxTextLength;

            // textures
            _searchIcon = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/TextField/SearchIcon");
            _background = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture"));
            _background.TextureHover = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/TextureHover");
            _background.OnLeftClick += (_, _) =>
            {
                if (CanFocus)
                {
                    IsFocused = true;
                }
            };
            Append(_background);

            // actual text
            _tfText = new TIGWEScrollText("");
            _tfText.TextOffsetTop = 5;
            Append(_tfText);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
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
                    TextChanged(_currentText);
                }
            }

            // this is kinda weird but ok
            _background.Texture = IsFocused ? _background.TextureHover : ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture");
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _tfText.ShouldScroll = !IsFocused;

            // set text to placeholder text if we havent written anything
            string text = _currentText;
            if (_currentText.Length == 0 && !IsFocused)
            {
                if (CanFocus)
                {
                    string[] parts = Regex.Split(PlaceholderText, @"(\[[^\]]*\])");
                    foreach (string part in parts)
                    {
                        if (!part.Equals(""))
                        {
                            text += !part.StartsWith("[c/") ? $"[c/AAAAAA:{part}]" : part;
                        }
                    }
                }
            }
            _isPlaceholderTextActive = _currentText.Length == 0 && !IsFocused;

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
                        TextChanged(_currentText);
                    }
                }
            }
            Recalculate();
        }

        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            base.DrawChildren(spriteBatch);
            if (ShowSearchIcon)
            {
                Rectangle dimensions = new Rectangle((int)GetDimensions().X + (int)Width.Pixels - 20, (int)GetDimensions().Y + 4, _searchIcon.Value.Width, _searchIcon.Value.Height);
                spriteBatch.Draw(_searchIcon.Value, dimensions, Color.White);
            }
        }

        public override void Recalculate()
        {
            base.Recalculate();

            // update offset
            _background.Width.Set(Width.Pixels, 0);
            _background.Height.Set(Height.Pixels, 0);
            _tfText.Left.Set(4, 0);
            _tfText.Width.Set(Width.Pixels - 8 - (ShowSearchIcon ? 18 : 0), 0);
            _tfText.Height.Set(Height.Pixels, 0);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            Main.GetInputText(_currentText);
        }

        void TextChanged(string text)
        {
            OnTextChanged?.Invoke(text);
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
                    return _tfText.GetText().Substring(0, _tfText.GetText().Length - 1);
                }
                else
                {
                    return _tfText.GetText();
                }
            }
        }

        public virtual void SetText(string text, bool raiseEvent = true)
        {
            if (!_currentText.Equals(text))
            {
                _currentText = text;
                if (raiseEvent)
                {
                    TextChanged(text);
                }
            }
        }
    }
}