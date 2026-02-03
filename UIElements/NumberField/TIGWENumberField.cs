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
using TerrariaInGameWorldEditor.UIElements;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UIElements.NumberField
{
    public class TIGWENumberField : UIElement
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
            get => (int)_tfText.Left.Pixels;
            set => _tfText.PaddingLeft = value;
        }
        public int TextOffsetTop 
        { 
            get => (int)_tfText.Top.Pixels;
            set => _tfText.PaddingTop = value;
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
        private UIText _tfText;
        private TIGWEImageResizeable _background;
        private int _textBlink;
        private UIImageButton _incrementButton;
        private UIImageButton _decrementButton;


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
                RemoveChild(_tfText);
                Append(_tfText);
            };
            _background.OnMouseOut += (_, _) =>
            {
                RemoveChild(_incrementButton);
                Append(_incrementButton);
                RemoveChild(_decrementButton);
                Append(_decrementButton);
            };
            Append(_background);

            // actual text
            _tfText = new UIText("");
            _tfText.IgnoresMouseInteraction = true;
            Append(_tfText);

            // increment button
            _incrementButton = new UIImageButton(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/NumberField/IncrementButton"));
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
                    RemoveChild(_tfText);
                    Append(_tfText);
                }
            };
            Append(_incrementButton);

            // decrement button
            _decrementButton = new UIImageButton(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/NumberField/DecrementButton"));
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
                    RemoveChild(_tfText);
                    Append(_tfText);
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
        }

        public void Decrement()
        {
            SetValue(_currentValue - Step);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // set text to placeholder text if we havent written anything
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
                string newText = Main.GetInputText(_tfText.Text.Replace("|", ""));
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
            _tfText.SetText(text);

            // this is kinda weird but ok
            _background.Texture = IsFocused ? _background.TextureHover : ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture");
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

        public int GetValue()
        {
            return _currentValue;
        }

        public void SetValue(int value)
        {
            _currentValue = Math.Clamp(value, MinValue, MaxValue);
            _tfText.SetText(_currentValue.ToString());
            OnValueChanged?.Invoke(value);
        }
    }
}