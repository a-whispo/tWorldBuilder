using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    public abstract class Tool
    {
        public TIGWEButton ToggleToolButton { get; protected set; }
        public List<(string, UIElement)> Settings { get; protected set; } = new List<(string, UIElement)>();

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
