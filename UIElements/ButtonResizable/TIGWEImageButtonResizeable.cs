using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UIElements.ButtonResizable
{
    internal class TIGWEImageButtonResizeable : TIGWEImageResizeable
    {
        public string HoverText { get; set; }
        public int TextOffsetLeft 
        { 
            get => (int)_btnText.Left.Pixels;
            set => _btnText.PaddingLeft = value;
        }
        public int TextOffsetTop 
        { 
            get => (int)_btnText.Top.Pixels;
            set => _btnText.PaddingTop = value;
        }
        public bool FitWidthToText {
            get => _fitWidthToText;
            set
            {
                _fitWidthToText = value;
                if (_fitWidthToText)
                {
                    FitWidth();
                }
            }
        }

        private bool _fitWidthToText = false;
        private UIText _btnText;
        private float _visibilityActive = 1f;
        private float _visibilityInactive = 1f;

        public TIGWEImageButtonResizeable(Asset<Texture2D> texture) : base(texture)
        {
            _btnText = new UIText("");
            _btnText.IgnoresMouseInteraction = true;
            Append(_btnText);
            TextOffsetLeft = 10;
            TextOffsetTop = 5;
        }

        public string GetText()
        {
            return _btnText.Text;
        }

        public void SetText(string text)
        {
            _btnText.SetText(text);
        }

        public void SetText(LocalizedText text)
        {
            _btnText.SetText(text);
        }

        public void SetVisibility(float whenActive, float whenInactive)
        {
            _visibilityActive = MathHelper.Clamp(whenActive, 0f, 1f);
            _visibilityInactive = MathHelper.Clamp(whenInactive, 0f, 1f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (FitWidthToText)
            {
                FitWidth();
            }
        }

        public void FitWidth()
        {
            Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, GetText(), new Vector2(1));
            if (Width.Pixels != size.X + TextOffsetLeft * 2 + 20)
            {
                Width.Set(size.X + TextOffsetLeft * 2 + 20, 0);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            Color = Color.White * (IsMouseHovering ? _visibilityActive : _visibilityInactive);
            if (HoverText != null && IsMouseHovering)
            {
                Main.instance.MouseText(HoverText);
            }
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
        }
    }
}
