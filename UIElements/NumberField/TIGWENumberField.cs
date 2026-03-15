using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UIElements.NumberField
{
    internal class TIGWENumberField : UIElement
    {
        public delegate void TextChangedEventHandler(int newValue);
        public event TextChangedEventHandler OnValueChanged;
        public int MinValue { get; set; } = int.MinValue;
        public int MaxValue { get; set; } = int.MaxValue;
        public int Step { get; set; } = 1;
        public bool IsFocused { get; set; } = false;
        public bool CanFocus { get; set; } = true;
        public int TextOffsetLeft 
        { 
            get => (int)_numText.Left.Pixels;
            set => _numText.PaddingLeft = value;
        }
        public int TextOffsetTop 
        { 
            get => (int)_numText.Top.Pixels;
            set => _numText.PaddingTop = value;
        }
        public bool ShowButtons {
            get => _showButtons;
            set 
            { 
                if (!ShowButtons) 
                { 
                    RemoveChild(_incrementButton); 
                    RemoveChild(_decrementButton);
                } 
                else
                {
                    Append(_decrementButton);
                    Append(_incrementButton);
                }
                _showButtons = value;
            } 
        } 

        private bool _showButtons = true; // only really looks good if the height is 26
        private int _currentValue = 0;
        private int _initialValue;
        private UIText _numText;
        private TIGWEImageResizeable _background;
        private int _textBlink;
        private TIGWEButton _incrementButton;
        private TIGWEButton _decrementButton;


        public TIGWENumberField(int initialValue, int maxValue = int.MaxValue, int minValue = int.MinValue)
        {
            MaxValue = maxValue;
            MinValue = minValue;
            _currentValue = initialValue;
            _initialValue = initialValue;

            // background of the field
            _background = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture"));
            _background.TextureHover = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/TextureHover");
            _background.OnLeftClick += (_, _) =>
            {
                if (CanFocus)
                {
                    IsFocused = true;
                }
            };
            _background.OnMouseOver += (_, _) =>
            {
                RemoveChild(_background);
                Append(_background);
                RemoveChild(_numText);
                Append(_numText);
            };
            Append(_background);

            // actual text
            _numText = new UIText("");
            _numText.IgnoresMouseInteraction = true;
            Append(_numText);

            // increment button
            _incrementButton = new TIGWEButton(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/NumberField/IncrementButton"));
            _incrementButton.SetHoverImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/NumberField/IncrementButtonHover"));
            _incrementButton.SetVisibility(1f, 1f);
            _incrementButton.Width.Set(14, 0f);
            _incrementButton.Height.Set(14, 0f);
            _incrementButton.OnLeftClick += (_, _) => 
            { 
                Increment();
                RemoveChild(_incrementButton);
                Append(_incrementButton);
            };
            _incrementButton.OnMouseOver += (_, _) =>
            {
                // bring to front so the hover effect isnt cut off by the other button
                RemoveChild(_incrementButton);
                Append(_incrementButton);
                if (IsFocused)
                {
                    // bring number area to front
                    RemoveChild(_background);
                    Append(_background);
                    RemoveChild(_numText);
                    Append(_numText);
                }
            };
            Append(_incrementButton);

            // decrement button
            _decrementButton = new TIGWEButton(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/NumberField/DecrementButton"));
            _decrementButton.SetHoverImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/NumberField/DecrementButtonHover"));
            _decrementButton.SetVisibility(1f, 1f);
            _decrementButton.Width.Set(14, 0f);
            _decrementButton.Height.Set(14, 0f);
            _decrementButton.OnLeftClick += (_, _) => 
            { 
                Decrement();
                RemoveChild(_decrementButton);
                Append(_decrementButton);
            };
            _decrementButton.OnMouseOver += (_, _) =>
            {
                // bring to front so the hover effect isnt cut off by the other button
                RemoveChild(_decrementButton);
                Append(_decrementButton);
                if (IsFocused)
                {
                    // bring number area to front
                    RemoveChild(_background);
                    Append(_background);
                    RemoveChild(_numText);
                    Append(_numText);
                }
            };
            Append(_decrementButton);

            // default offsets
            TextOffsetLeft = 10;
            TextOffsetTop = 5;
        }

        public void Increment()
        {
            SetValue(_currentValue + Step);
            OnValueChanged?.Invoke(_currentValue);
        }

        public void Decrement()
        {
            SetValue(_currentValue - Step);
            OnValueChanged?.Invoke(_currentValue);
        }

        public override void Recalculate()
        {
            base.Recalculate();

            // update offsets
            _incrementButton.Left.Set(Width.Pixels - 14, 0f);
            _incrementButton.Top.Set(0, 0f);
            _decrementButton.Left.Set(Width.Pixels - 14, 0f);
            _decrementButton.Top.Set(12, 0f);
            _background.Width.Set(ShowButtons ? Width.Pixels - 12 : Width.Pixels, 0f);
            _background.Height.Set(Height.Pixels, 0f);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
            base.MouseOver(evt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            string text;
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
                string newText = Main.GetInputText(_numText.Text.Replace("|", ""));
                int result = _initialValue;
                if (newText.Equals("") || newText.Equals("-") && IsFocused && MinValue < 0 || int.TryParse(newText, out result))
                {
                    text = newText;
                    result = Math.Clamp(result, MinValue, MaxValue);
                    _currentValue = result;
                    OnValueChanged?.Invoke(result);
                }
                else
                {
                    text = _currentValue.ToString();
                }
            }
            else
            {
                text = _currentValue.ToString();
            }

            // text blinker thing
            if (++_textBlink / 30 % 2 == 0 && IsFocused)
            {
                text += "|";
            }

            if (IsFocused)
            {
                if ((Main.inputText.IsKeyDown(Keys.LeftControl) || Main.inputText.IsKeyDown(Keys.RightControl)) && !(Main.inputText.IsKeyDown(Keys.LeftAlt) || Main.inputText.IsKeyDown(Keys.RightAlt)))
                {
                    if (Main.inputText.IsKeyDown(Keys.Back) && !Main.oldInputText.IsKeyDown(Keys.Back))
                    {
                        _currentValue = _initialValue;
                        OnValueChanged?.Invoke(_initialValue);
                        text = "";
                    }
                }
            }
            _numText.SetText(text);

            // this is kinda weird but ok
            _background.Texture = IsFocused ? _background.TextureHover : ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture");
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.Draw(spriteBatch);
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }

        public int GetValue()
        {
            return _currentValue;
        }

        public void SetValue(int value, bool raiseEvent = true)
        {
            value = Math.Clamp(value, MinValue, MaxValue);
            if (_currentValue != value)
            {
                _currentValue = value;
                _numText.SetText(_currentValue.ToString());
                if (raiseEvent)
                {
                    OnValueChanged?.Invoke(value);
                }
            }
        }
    }
}