using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UIElements.Slider
{
    internal class TIGWESlider : UIElement
    {
        public bool IsDragging { get; private set; } = false;
        public bool ShouldResize
        {
            get => _shouldResize;
            set
            {
                if (!value)
                {
                    RemoveChild(_body);
                }
                else
                {
                    Append(_body);
                    RemoveChild(_point);
                    Append(_point);
                }
                _shouldResize = value;
            }
        }
        public Asset<Texture2D> Texture 
        {
            get => _body.Texture;
            set => _body.Texture = value; 
        }

        private bool _shouldResize = true;
        private TIGWEImageResizeable _body;
        private UIImage _point;
        
        public TIGWESlider()
        {
            Height.Set(18, 0);
            _body = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture"));
            _body.Width.Set(0, 1);
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
            return (float)(_point.Left.Pixels / (float)(Width.Pixels - _point.Width.Pixels)) * 100f; // returns where the point is on the slider from 0 - 100 (0 being at the start and 100 being at the end)
        }

        public void SetValue(float value)
        {
            value = Math.Clamp(value, 0, 100);
            _point.Left.Set((value / 100) * (Width.Pixels - _point.Width.Pixels), 0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!(Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                IsDragging = false;
            }
            if (IsDragging)
            {
                int offsetX = Main.mouseX - GetViewCullingArea().X;
                _point.Left.Set(Math.Clamp(offsetX - 6, 0, Width.Pixels - _point.Width.Pixels), 0); // clamp is so it doesnt go too far to the left
            }
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            IsDragging = true;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            if (!ShouldResize)
            {
                CalculatedStyle dimensions = GetDimensions();
                spriteBatch.Draw(Texture.Value, new Rectangle((int)dimensions.X, (int)dimensions.Y + 2, (int)dimensions.Width, (int)dimensions.Height - 4), Color.White);
            }
            base.DrawSelf(spriteBatch);
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            base.DrawChildren(spriteBatch);
        }
    }
}
