using TerrariaInGameWorldEditor.Common;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class SelectionTool : Tool
    {
        public TileCollection Selection { get; } = new TileCollection();

        public SelectionTool(string iconPath, string hoverText) : base(iconPath, hoverText)
        {

        }
    }
}
