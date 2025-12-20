using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    public abstract class Tool
    {
        public TIGWEButton ToggleToolButton { get; private set; }
        public List<(string, UIElement)> Settings { get; protected set; } = new List<(string, UIElement)>();

        public Tool(string iconPath, string hoverText)
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>(iconPath));
            ToggleToolButton.HoverText = hoverText;
        }

        public virtual string GetInfoText()
        {
            return "";
        }

        public virtual void PostUpdateInput()
        {

        }

        public virtual void Update()
        {
            
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
