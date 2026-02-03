using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.UIElements;

namespace TerrariaInGameWorldEditor.UIElements.DirectoryGrid
{
    internal class TIGWEDirectoryFile : TIGWEDirectoryItem
    {
        public TIGWEDirectoryFile(string pathFromSaves) : base(pathFromSaves)
        {
            // icon
            _icon.SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/File"));
            _icon.Top.Set(8, 0);
            _icon.Left.Set(10, 0);
            _icon.Width.Set(16, 0f);
            _icon.Height.Set(22, 0f);
        }
    }
}