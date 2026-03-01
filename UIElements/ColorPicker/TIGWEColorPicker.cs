using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements.ButtonResizable;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.NumberField;
using TerrariaInGameWorldEditor.UIElements.Slider;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UIElements.ColorPicker
{
    internal class TIGWEColorPicker : UIElement
    {
        public delegate void ColorChangedEventHandler(Color color);
        public event ColorChangedEventHandler OnColorChanged;

        private bool _isShowingColorPicker = false;
        private bool _isSelectingColor = false;
        private TIGWEImageResizeable _colorPicker;
        private UIElement _colorArea;
        private Asset<Texture2D> _color;
        private Asset<Texture2D> _gradient;
        private UIImage _colorDot;
        private TIGWETextField _hexTextField;
        private TIGWENumberField _rNumberField;
        private TIGWENumberField _gNumberField;
        private TIGWENumberField _bNumberField;
        private TIGWESlider _hueSlider;
        private TIGWESlider _alphaSlider;

        public TIGWEColorPicker()
        {
            Width.Set(26, 0);
            Height.Set(26, 0);

            // main button
            TIGWEImageButtonResizeable colorPicker = new TIGWEImageButtonResizeable(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture"));
            colorPicker.TextureHover = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/TextureHover");
            colorPicker.Height.Set(26, 0);
            colorPicker.Width.Set(26, 0);
            Append(colorPicker);
            colorPicker.OnLeftClick += (_, _) =>
            {
                ShowColorPicker();
            };

            // element/border to be the main color picker area
            _colorPicker = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Border"), 6, 4);
            _colorPicker.Width.Set(256, 0);
            _colorPicker.Height.Set(170, 0);

            // gradient and color
            _color = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/ColorPicker/Color");
            _gradient = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/ColorPicker/Gradient");
            _colorArea = new UIElement();
            _colorArea.Left.Set(8, 0);
            _colorArea.Top.Set(8, 0);
            _colorArea.Width.Set(110, 0);
            _colorArea.Height.Set(110, 0);
            _colorArea.OnDraw += DrawColorArea;
            _colorArea.OnLeftMouseDown += (_, _) =>
            {
                _isSelectingColor = true;
            };
            _colorArea.OnLeftMouseUp += (_, _) =>
            {
                _isSelectingColor = false;
            };
            _colorPicker.Append(_colorArea);

            // color picker dot
            _colorDot = new UIImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/ColorPicker/ColorPickerDot"));
            _colorDot.Left.Set(-4, 0);
            _colorDot.Top.Set(-4, 0);
            _colorArea.Append(_colorDot);

            // hue slider
            UIText hue = new UIText("H:");
            hue.Top.Set(122, 0);
            hue.Left.Set(12, 0);
            _colorPicker.Append(hue);
            _hueSlider = new TIGWESlider();
            _hueSlider.Height.Set(18, 0);
            _hueSlider.Width.Set(208, 0);
            _hueSlider.Top.Set(122, 0);
            _hueSlider.Left.Set(40, 0);
            _hueSlider.Texture = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/ColorPicker/HueSlider");
            _hueSlider.ShouldResize = false;
            _colorPicker.Append(_hueSlider);

            // alpha slider
            UIText alpha = new UIText("A:");
            alpha.Top.Set(144, 0);
            alpha.Left.Set(12, 0);
            _colorPicker.Append(alpha);
            _alphaSlider = new TIGWESlider();
            _alphaSlider.Height.Set(18, 0);
            _alphaSlider.Width.Set(208, 0);
            _alphaSlider.Top.Set(144, 0);
            _alphaSlider.Left.Set(40, 0);
            _alphaSlider.SetValue(100);
            _colorPicker.Append(_alphaSlider);

            // hex textfield
            UIText hex = new UIText("#:");
            hex.Top.Set(12, 0);
            hex.Left.Set(122, 0);
            _colorPicker.Append(hex);
            _hexTextField = new TIGWETextField("", 6);
            _hexTextField.OnTextChanged += (newText) =>
            {
                // validate input and make uppercase
                char[] validatedChars = newText.Where((c, b) =>
                {
                    return Uri.IsHexDigit(c);
                }).ToArray();
                _hexTextField.SetText(new string(validatedChars).ToUpper(), false);
                SetColor(HexToColor(newText));
            };
            _hexTextField.Width.Set(98, 0);
            _hexTextField.Height.Set(26, 0);
            _hexTextField.Top.Set(8, 0);
            _hexTextField.Left.Set(150, 0);
            _colorPicker.Append(_hexTextField);

            // rgb
            UIText r = new UIText("R:");
            r.Top.Set(40, 0);
            r.Left.Set(122, 0);
            _colorPicker.Append(r);
            _rNumberField = new TIGWENumberField(0);
            _rNumberField.OnValueChanged += (newValue) =>
            {
                SetColor(new Color(newValue, _gNumberField.GetValue(), _bNumberField.GetValue(), (int)(GetAlpha() * 255f)));
            };
            _rNumberField.Top.Set(36, 0);
            _rNumberField.Left.Set(150, 0);
            _rNumberField.Width.Set(98, 0);
            _rNumberField.Height.Set(26, 0);
            _colorPicker.Append(_rNumberField);
            UIText g = new UIText("G:");
            g.Top.Set(68, 0);
            g.Left.Set(122, 0);
            _colorPicker.Append(g);
            _gNumberField = new TIGWENumberField(0);
            _gNumberField.OnValueChanged += (newValue) =>
            {
                SetColor(new Color(_rNumberField.GetValue(), newValue, _bNumberField.GetValue(), (int)(GetAlpha() * 255f)));
            };
            _gNumberField.Top.Set(64, 0);
            _gNumberField.Left.Set(150, 0);
            _gNumberField.Width.Set(98, 0);
            _gNumberField.Height.Set(26, 0);
            _colorPicker.Append(_gNumberField);
            UIText b = new UIText("B:");
            b.Top.Set(96, 0);
            b.Left.Set(122, 0);
            _colorPicker.Append(b);
            _bNumberField = new TIGWENumberField(0);
            _bNumberField.OnValueChanged += (newValue) =>
            {
                SetColor(new Color(_rNumberField.GetValue(), _gNumberField.GetValue(), newValue, (int)(GetAlpha() * 255f)));
            };
            _bNumberField.Top.Set(92, 0);
            _bNumberField.Left.Set(150, 0);
            _bNumberField.Width.Set(98, 0);
            _bNumberField.Height.Set(26, 0);
            _colorPicker.Append(_bNumberField);
        }

        private void DrawColorArea(UIElement element)
        {
            // set up spritebatch
            SpriteBatch spriteBatch = new SpriteBatch(Main.graphics.GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, default, Main.UIScaleMatrix);

            // draw the color and gradient
            Rectangle bounds = element.GetViewCullingArea();
            spriteBatch.Draw(_color.Value, bounds, HsvToColor(GetHue(), 100, 100));
            spriteBatch.Draw(_gradient.Value, bounds, Color.White);
            spriteBatch.End();
            spriteBatch.Dispose();
        }

        private void ShowColorPicker()
        {
            if (!_isShowingColorPicker)
            {
                _isShowingColorPicker = true;

                // get the topmost element so we can append _colorPicker there causing it to render and handle input on top of everything else
                UIElement top = this;
                while (top.Parent != null)
                {
                    top = top.Parent;
                }
                top.Append(_colorPicker);
                top.OnUpdate += CheckIfShouldHide;
            }
        }

        private void CheckIfShouldHide(UIElement element)
        {
            Vector2 mouse = new Vector2(Main.mouseX, Main.mouseY);
            if (!_isSelectingColor && !_hueSlider.IsDragging && !_alphaSlider.IsDragging && !ContainsPoint(mouse) && !_colorPicker.ContainsPoint(mouse))
            {
                HideColorPicker();

                // set to a default value if we exit without typing anything
                if (string.IsNullOrEmpty(_hexTextField.GetText()) && !_hexTextField.IsFocused)
                {
                    SetColor(new Color(255, 255, 255));
                }
            }
        }

        private void HideColorPicker()
        {
            if (_isShowingColorPicker && _colorPicker.Parent != null)
            {
                _isShowingColorPicker = false;
                _hexTextField.IsFocused = false;
                _rNumberField.IsFocused = false;
                _gNumberField.IsFocused = false;
                _bNumberField.IsFocused = false;
                _colorPicker.Parent.OnUpdate -= CheckIfShouldHide;
                _colorPicker.Remove();
            }
        }

        public float GetHue()
        {
            // returns hue (0 - 360)
            return _hueSlider.GetValue() * 3.6f;
        }

        public float GetSaturation()
        {
            // returns saturation (0 - 100)
            return (_colorDot.Left.Pixels + 4) / (_colorArea.Width.Pixels - 2) * 100;
        }

        public float GetValue()
        {
            // returns value (0 - 100)
            return 100 - (_colorDot.Top.Pixels + 4) / (_colorArea.Height.Pixels - 2) * 100;
        }

        public float GetAlpha()
        {
            // returns alpha (0 - 1)
            return _alphaSlider.GetValue() / 100f;
        }

        public Color GetColor()
        {
            return HsvToColor(GetHue(), GetSaturation(), GetValue()) * GetAlpha();
        }

        public string GetHex()
        {
            Color color = GetColor();

            // converts to hex values
            string rHex = _rNumberField.GetValue().ToString("X");
            rHex = rHex.Length == 1 ? "0" + rHex : rHex;
            string gHex = _gNumberField.GetValue().ToString("X");
            gHex = gHex.Length == 1 ? "0" + gHex : gHex;
            string bHex = _bNumberField.GetValue().ToString("X");
            bHex = bHex.Length == 1 ? "0" + bHex : bHex;

            return rHex + gHex + bHex;
        }

        private Color HsvToColor(double h, double s, double v)
        {
            s = s / 100;
            v = v / 100;

            double c = v * s;
            double x = c * (1 - Math.Abs(h / 60 % 2 - 1));
            double m = v - c;

            double r = 0;
            double g = 0;
            double b = 0;
            switch (h)
            {
                case >= 300: r = c; g = 0; b = x; break;
                case >= 240: r = x; g = 0; b = c; break;
                case >= 180: r = 0; g = x; b = c; break;
                case >= 120: r = 0; g = c; b = x; break;
                case >= 60: r = x; g = c; b = 0; break;
                case >= 0: r = c; g = x; c = 0; break;
            }
            return new Color((int)Math.Round((r + m) * 255), (int)Math.Round((g + m) * 255), (int)Math.Round((b + m) * 255));
        }

        private Color HexToColor(string hex)
        {
            if (hex.Length == 6)
            {
                int.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, null, out int r);
                int.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, null, out int g);
                int.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, null, out int b);
                return new Color(r, g, b);
            }
            return new Color(255, 255, 255);
        }

        public void SetColor(Color color)
        {
            float R = (float)color.R / 255;
            float G = (float)color.G / 255;
            float B = (float)color.B / 255;

            float cMax = Math.Max(R, Math.Max(G, B));
            float cMin = Math.Min(R, Math.Min(G, B));

            float deltaC = cMax - cMin;

            float h = 0;
            float s = 0;
            float v = 0;

            // calculate hue
            if (deltaC == 0)
            {
                h = 0;
            }
            else
            {
                if (cMax == R)
                {
                    h = 60 * ((G - B) / deltaC % 6);
                }
                if (cMax == G)
                {
                    h = 60 * ((B - R) / deltaC + 2);
                }
                if (cMax == B)
                {
                    h = 60 * ((R - G) / deltaC + 4);
                }
                if (h < 0)
                {
                    h += 360;
                }
            }

            // calculate saturation
            if (cMax == 0)
            {
                s = 0;
            }
            else
            {
                s = deltaC / cMax;
            }

            // calculate value
            v = cMax;

            // update
            if (!_hueSlider.IsDragging && !_alphaSlider.IsDragging && !_isSelectingColor)
            {
                _hueSlider.SetValue(h / 3.6f);
                _alphaSlider.SetValue((color.A / 255f) * 100);
                _colorDot.Left.Set(s * (_colorArea.Width.Pixels - 2) - 4, 0);
                _colorDot.Top.Set(100 - v * (_colorArea.Height.Pixels - 2) + 4, 0);
            }
            _rNumberField.SetValue(color.R, false);
            _gNumberField.SetValue(color.G, false);
            _bNumberField.SetValue(color.B, false);
            if (!_hexTextField.IsFocused)
            {
                _hexTextField.SetText(GetHex(), false);
            }
            OnColorChanged?.Invoke(GetColor());
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // adjust color dot offset
            if (_isSelectingColor)
            {
                int offsetX = Main.mouseX - _colorArea.GetViewCullingArea().X;
                int offsetY = Main.mouseY - _colorArea.GetViewCullingArea().Y;
                _colorDot.Top.Set(Math.Clamp(offsetY - 7, -4, _colorArea.Height.Pixels - 6), 0);
                _colorDot.Left.Set(Math.Clamp(offsetX - 7, -4, _colorArea.Width.Pixels - 6), 0);
            }

            // update color when changing color with the slider and color area
            if (_hueSlider.IsDragging || _alphaSlider.IsDragging || _isSelectingColor)
            {
                SetColor(HsvToColor(GetHue(), GetSaturation(), GetValue()));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            // draw the color and gradient
            Rectangle bounds = GetViewCullingArea();
            bounds.X += 6;
            bounds.Y += 6;
            bounds.Width -= 12;
            bounds.Height -= 12;
            spriteBatch.Draw(_color.Value, bounds, GetColor());
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            if (_colorPicker.Parent != null)
            {
                _colorPicker.Top.Set(GetDimensions().Y - _colorPicker.Parent.Top.Pixels + 24, 0);
                _colorPicker.Left.Set(GetDimensions().X - _colorPicker.Parent.Left.Pixels, 0);
            }
        }
    }
}
