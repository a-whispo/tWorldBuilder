using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Globalization;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UI.UIElements.Slider;
using TerrariaInGameWorldEditor.UI.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UI.UIElements.ColorPicker
{
    internal class TIGWEColorPicker : UIElement
    {
        public delegate void ColorChangedEventHandler(Color color);
        public event ColorChangedEventHandler OnColorChanged;
        public float DrawScale { get; set; }

        private UIElement _colorPane;
        private UIImage _colorDot;
        private TIGWEImageResizeable _colorBorder;
        private TIGWEImageResizeable _previewBorder;
        private TIGWESlider _hueSlider;
        private TIGWETextField _hexTextField;
        private bool _isPressing = false;
        private TIGWETextField _rTextField;
        private TIGWETextField _gTextField;
        private TIGWETextField _bTextField;

        public TIGWEColorPicker()
        {
            DrawScale = Main.UIScale;

            // set width and height
            Width.Set(350, 0);
            Height.Set(120, 0);

            // element to hold the border and the color dot
            _colorPane = new UIElement();
            _colorPane.Width.Set(100, 0);
            _colorPane.Height.Set(100, 0);
            _colorPane.Top.Set(10, 0);
            _colorPane.Left.Set(10, 0);
            Append(_colorPane);

            // create the dot
            _colorDot = new UIImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/ColorPicker/ColorPickerDot"));
            // place dot in top left by default
            _colorDot.Left.Set(2, 0);
            _colorDot.Top.Set(2, 0);
            _colorPane.Append(_colorDot);

            // create a border around the color
            _colorBorder = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Border"));
            _colorBorder.Width.Set(100, 0);
            _colorBorder.Height.Set(100, 0);
            _colorBorder.IgnoresMouseInteraction = true;
            _colorPane.Append(_colorBorder);

            // create a border around the color
            _previewBorder = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Border"));
            _previewBorder.Width.Set(30, 0);
            _previewBorder.Height.Set(100, 0);
            _previewBorder.Top.Set(10, 0);
            _previewBorder.Left.Set(114, 0);
            _previewBorder.IgnoresMouseInteraction = true;
            Append(_previewBorder);

            // hue slider and visualName
            UIText hue = new UIText("Hue: ");
            hue.Top.Set(15, 0);
            hue.Left.Set(149, 0);
            Append(hue);
            _hueSlider = new TIGWESlider(150);
            _hueSlider.Top.Set(15, 0);
            _hueSlider.Left.Set(190, 0);
            _hueSlider.Texture = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/ColorPicker/HueSlider");
            _hueSlider.TextureHover = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/ColorPicker/HueSlider");
            _hueSlider.ShouldResize = false;
            Append(_hueSlider);

            // hex textfield and visualName
            UIText hex = new UIText("Hex: ");
            hex.Top.Set(50, 0);
            hex.Left.Set(149, 0);
            Append(hex);
            _hexTextField = new TIGWETextField("", 6);
            _hexTextField.TextOffsetLeft = 40;
            _hexTextField.CanFocus = false;
            _hexTextField.Width.Set(150, 0);
            _hexTextField.Height.Set(32, 0);
            _hexTextField.Top.Set(43, 0);
            _hexTextField.Left.Set(190, 0);
            Append(_hexTextField);
            UIText number = new UIText("#");
            number.Left.Set(10, 0);
            number.Top.Set(7, 0);
            number.IgnoresMouseInteraction = true;
            _hexTextField.Append(number);

            // rgb textfields and visualName
            UIText rgb = new UIText("Rgb: ");
            rgb.Top.Set(85, 0);
            rgb.Left.Set(149, 0);
            Append(rgb);
            
            // r
            _rTextField = new TIGWETextField("", 3);
            _rTextField.CanFocus = false;
            _rTextField.Width.Set(48, 0);
            _rTextField.Height.Set(32, 0);
            _rTextField.Top.Set(78, 0);
            _rTextField.Left.Set(190, 0);
            Append(_rTextField);

            // g
            _gTextField = new TIGWETextField("", 3);
            _gTextField.CanFocus = false;
            _gTextField.Width.Set(48, 0);
            _gTextField.Height.Set(32, 0);
            _gTextField.Top.Set(78, 0);
            _gTextField.Left.Set(241, 0);
            Append(_gTextField);

            // b
            _bTextField = new TIGWETextField("", 3);
            _bTextField.CanFocus = false;
            _bTextField.Width.Set(48, 0);
            _bTextField.Height.Set(32, 0);
            _bTextField.Top.Set(78, 0);
            _bTextField.Left.Set(292, 0);
            Append(_bTextField);
        }

        public float GetHue()
        {
            return _hueSlider.GetValue() * 3.6f; // returns hue (0 - 360)
        }

        public float GetSaturation()
        {
            return (_colorDot.Left.Pixels - 2) / 84 * 100; // returns saturation (0 - 100)
        }

        public float GetValue()
        {
            return 100 - ((_colorDot.Top.Pixels - 2) / 84 * 100); // returns value (0 - 100)
        }

        public Color GetColor()
        {
            int[] rgb = HsvToRgb(GetHue(), GetSaturation(), GetValue());
            return new Color(rgb[0], rgb[1], rgb[2]);
        }

        public string GetHex()
        {
            int[] rgb = HsvToRgb(GetHue(), GetSaturation(), GetValue());
            
            // converts to hex values
            string rHex = rgb[0].ToString("X");
            rHex = rHex.Length == 1 ? "0" + rHex : rHex;
            string gHex = rgb[1].ToString("X");
            gHex = gHex.Length == 1 ? "0" + gHex : gHex;
            string bHex = rgb[2].ToString("X");
            bHex = bHex.Length == 1 ? "0" + bHex : bHex;

            return (rHex + gHex + bHex);
        }

        private void SetSaturation(int saturation)
        {
            saturation = Math.Clamp(saturation, 0, 100);
            _colorDot.Left.Set(86 - (saturation / 84) * 100, 0); // sets saturation (0 - 100)
        }

        private void SetValue(int value)
        {
            value = Math.Clamp(value, 0, 100);
            _colorDot.Top.Set(2 + (value / 84) * 100, 0); // sets value (0 - 100)
        }

        private static int[] HsvToRgb(double h, double s, double v)
        {
            s = s / 100;
            v = v / 100;

            // using a formula i found on rapidtables
            double c = v * s;
            double x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
            double m = v - c;

            double r = 0;
            double g = 0;
            double b = 0;
            switch (h)
            {
                case >=300: r = c; g = 0; b = x; break;
                case >= 240: r = x; g = 0; b = c; break;
                case >= 180: r = 0; g = x; b = c; break;
                case >= 120: r = 0; g = c; b = x; break;
                case >= 60: r = x; g = c; b = 0; break;
                case >= 0: r = c; g = x; c = 0; break;
            }
            return [(int)Math.Round((r + m) * 255), (int)Math.Round((g + m) * 255), (int)Math.Round((b + m) * 255)];
        }

        private static int[] HexToRgb(string hex)
        {
            hex = hex.Replace("|", "");
            NumberStyles s = NumberStyles.HexNumber;

            int r = int.Parse(hex.Substring(0, 2), s);
            int g = int.Parse(hex.Substring(2, 2), s);
            int b = int.Parse(hex.Substring(4, 2), s);

            return [r, g, b];
        }

        private static int[] RgbToHsv(float r, float g, float b)
        {
            float R = r / 255;
            float G = g / 255;
            float B = b / 255;

            // using a formula i found on rapidtables
            float cMax = Math.Max(R, Math.Max(G, B));
            float cMin = Math.Min(R, Math.Min(G, B));

            float deltaC = cMax - cMin;

            float h = 0;
            float s = 0;
            float v = 0;

            // calculate h
            if (deltaC == 0)
            {
                h = 0;
            } else
            {
                if (cMax == R)
                {
                    h = 60 * (((G - B) / deltaC) % 6);
                }
                if (cMax == G)
                {
                    h = 60 * (((B - R) / deltaC) + 2);
                }
                if (cMax == B)
                {
                    h = 60 * (((R - G) / deltaC) + 4);
                }
            }

            // calculate s
            if (cMax == 0)
            {
                s = 0;
            } else
            {
                s = deltaC / cMax;
            }

            // calculate v
            v = cMax;

            return [(int)(h), (int)(s * 100), (int)(v * 100)];
        }

        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            // draw a background
            UIElementsUtils.DrawTexture2DWithDimensions((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"), this.GetDimensions().ToRectangle());

            // get bounds
            Rectangle bounds = _colorPane.GetViewCullingArea();

            // draw the color of selected hue
            int[] hue = HsvToRgb(GetHue(), 100, 100);
            spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/ColorPicker/Color"), new Rectangle(bounds.X + 6, bounds.Y + 6, bounds.Width - 10, bounds.Height - 10), new Color(hue[0], hue[1], hue[2])); // Set color to whatever hue we have selected

            // draw color preview
            spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/ColorPicker/Color"), new Rectangle(bounds.X + 110, bounds.Y + 4, 20, 96), GetColor()); // Set color to whatever we have selected

            // need to start a new spritebatch with nonpremultiplied blendstate to properly draw the gradient
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, default, Matrix.CreateScale(DrawScale));
            // draw gradient
            spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/ColorPicker/Gradient"), new Rectangle(bounds.X + 6, bounds.Y + 6, bounds.Width - 11, bounds.Height - 11), Color.White);

            // start normal spritebatch again
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, default, Matrix.CreateScale(DrawScale)); // make sure to go back to the normal spritebatch

            // draw all children like border and dot after so they appear on top
            base.DrawChildren(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!(Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                _isPressing = false;
            }
            if (_isPressing) // checks if lmb is pressed down while hovering above the color area
            {
                // calculate offset depending on mouse location
                int offsetX = (int)(Main.MouseWorld.X - Main.screenPosition.X) - _colorBorder.GetViewCullingArea().X;
                int offsetY = (int)(Main.MouseWorld.Y - Main.screenPosition.Y) - _colorBorder.GetViewCullingArea().Y;
                
                // mouse picker circle to mouse location
                _colorDot.Top.Set(Math.Clamp(offsetY - 6, 2, _colorPane.Height.Pixels - 14), 0);
                _colorDot.Left.Set(Math.Clamp(offsetX - 6, 2, _colorPane.Width.Pixels - 14), 0);
            }

            // TODO make it so you can change color with hex codes and rgb values as well
            // set all the visualName fields
            int[] rgb = HsvToRgb(GetHue(), GetSaturation(), GetValue());
            _rTextField.SetText(rgb[0].ToString());
            _gTextField.SetText(rgb[1].ToString());
            _bTextField.SetText(rgb[2].ToString());
            _hexTextField.SetText(GetHex());
            OnColorChanged.Invoke(GetColor());
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            // calculate bounds
            Rectangle bounds = _colorBorder.GetViewCullingArea();
            Rectangle newBounds = new Rectangle(bounds.X + 8, bounds.Y + 8, bounds.Width - 15, bounds.Height - 15); // same bounds as the color and gradient becuase we only want to move the dot when we click on those, not the border            

            // checks if lmb is pressed down while hovering
            if (newBounds.Contains((int)(Main.MouseWorld.X - Main.screenPosition.X), (int)(Main.MouseWorld.Y - Main.screenPosition.Y)) && Mouse.GetState().LeftButton == ButtonState.Pressed && !_isPressing) 
            {
                _isPressing = true;
            }
        }
    }
}
