using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.Editor.Windows.Settings
{
    internal abstract class SettingsNode : UIElement
    {
        public bool ShowBody
        {
            get => _body.Parent != null;
            set
            {
                if (value)
                {
                    Append(_body);
                }
                else
                {
                    RemoveChild(_body);
                }
            }
        }

        private TIGWEImageResizeable _body;

        public SettingsNode()
        {
            _body = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));
            _body.Width.Set(0, 1);
            _body.Height.Set(0, 1);
            Append(_body);
        }
    }
}
