using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UI.UIElements.Scrollbar
{
    internal class TIGWEScrollbar : UIScrollbar
    {
        public bool IsDragging => (bool)_isDraggingField.GetValue(this);

        // this class is just for custom scrollbar textures
        private Asset<Texture2D> _texture;
        private Asset<Texture2D> _innerTexture;
        private FieldInfo _isDraggingField;
        private FieldInfo _dragYOffsetField;
        private FieldInfo _viewPositionField;

        public TIGWEScrollbar()
        {
            // texture is the background and innerTexture is the scrollbar
            _texture = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture");
            _innerTexture = ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Scrollbar");

            _isDraggingField = typeof(UIScrollbar).GetField("_isDragging", BindingFlags.Instance | BindingFlags.NonPublic);
            _dragYOffsetField = typeof(UIScrollbar).GetField("_dragYOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            _viewPositionField = typeof(UIScrollbar).GetField("_viewPosition", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle innerDimensions = GetInnerDimensions();
            if ((bool)_isDraggingField.GetValue(this))
            {
                float num = UserInterface.ActiveInstance.MousePosition.Y - innerDimensions.Y - (float)_dragYOffsetField.GetValue(this);
                _viewPositionField.SetValue(this, MathHelper.Clamp(num / innerDimensions.Height * MaxViewSize, 0f, MaxViewSize - ViewSize));
            }

            CalculatedStyle dimensionsRectangle = new CalculatedStyle(GetDimensions().X, GetDimensions().Y, GetDimensions().Width, GetDimensions().Height);
            UIElementsUtils.DrawTexture2DWithDimensions(_texture.Value, dimensionsRectangle.ToRectangle());

            CalculatedStyle handleRectangle = new CalculatedStyle((int)GetInnerDimensions().X + 6, (int)(GetInnerDimensions().Y + 1 + GetInnerDimensions().Height * (ViewPosition / MaxViewSize)), 8, (int)(GetInnerDimensions().Height * (ViewSize / MaxViewSize)) - 1);
            UIElementsUtils.DrawTexture2DWithDimensions(_innerTexture.Value, handleRectangle.ToRectangle(), default, 4, 8);
        }

        public override void Recalculate()
        {
            base.Recalculate();

        }
    }
}
