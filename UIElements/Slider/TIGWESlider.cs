using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UIElements.Slider
{
    internal class TIGWESlider : UIElement
    {
        public Asset<Texture2D> Texture 
        { 
            set => _body.Texture = value; 
        }
        public Asset<Texture2D> TextureHover 
        { 
            set => _body.TextureHover = value; 
        }
        public bool ShouldResize 
        { 
            set => _body.ShouldResize = value; 
        }

        private TIGWEImageResizeable _body;
        private UIImage _point;
        private bool _isPressing = false;

        public TIGWESlider(int width = 100)
        {
            Width.Set(width, 0);
            Height.Set(18, 0);

            _body = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture"));
            _body.Width.Set(width, 0);
            _body.Height.Set(14, 0);
            _body.Top.Set(2, 0);
            Append(_body);

            _point = new UIImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Slider/SliderPoint"));
            _point.Width.Set(8, 0);
            _point.Height.Set(18, 0);
            Append(_point);
        }

        public float GetValue()
        {
            return _point.Left.Pixels / (Width.Pixels - 7) * 100; // returns where the point is on the slider from 0 - 100 (0 being at the start and 100 being at the end)
        }

        public void SetValue(int value)
        {
            value = Math.Clamp(value, 0, 100);
            _point.Left.Set(value / 100 * (Width.Pixels - 7), 0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!(Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                _isPressing = false;
            }
            if (_isPressing)
            {
                // calculate offset depending on mouse location
                int offsetX = (int)(Main.MouseWorld.X - Main.screenPosition.X) - GetViewCullingArea().X;

                // mouse picker circle to mouse location
                _point.Left.Set(Math.Clamp(offsetX - 6, 0, Width.Pixels - _point.Width.Pixels), 0); // clamp is so it doesnt go too far to the left
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            _isPressing = true;
        }
    }
}
