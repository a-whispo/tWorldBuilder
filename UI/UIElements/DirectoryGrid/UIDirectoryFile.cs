using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace TerrariaInGameWorldEditor.UI.UIElements.DirectoryGrid
{
    internal class UIDirectoryFile : UIDirectoryItem
    {
        public UIDirectoryFile(string pathFromSaves) : base(pathFromSaves)
        {
            // icon
            _icon.SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/File"));
            _icon.Top.Set(8, 0);
            _icon.Left.Set(10, 0);
            _icon.Width.Set(16, 0f);
            _icon.Height.Set(22, 0f);
        }
    }
}