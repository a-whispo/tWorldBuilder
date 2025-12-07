using Microsoft.Xna.Framework;
using ReLogic.Graphics;
using System;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace TerrariaInGameWorldEditor.UI.Editor
{
    internal class ToolSetting : UIElement
    {
        private int _textMarginRight = 8;
        private int _textMarginTop = 5;

        public ToolSetting(string name, UIElement element)
        {
            DynamicSpriteFont spriteFont = FontAssets.MouseText.Value;
            Vector2 size = ChatManager.GetStringSize(spriteFont, name, new Vector2(1));
            UIText text = new UIText(name);
            text.Top.Set(_textMarginTop, 0f);
            Append(text);
            element.Left.Set(size.X + _textMarginRight, 0f);
            Append(element);
            Width.Set(size.X + _textMarginRight + element.Width.Pixels, 0f);
            Height.Set(0, 1f);
        }
    }
}
