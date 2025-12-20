using TerrariaInGameWorldEditor.Common;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal abstract class SelectionTool : Tool
    {
        protected TileCollection _selection = new TileCollection();

        public SelectionTool(string iconPath, string hoverText) : base(iconPath, hoverText)
        {

        }

        public virtual TileCollection GetSelection()
        {
            return _selection;
        }

        public virtual void ResetSelection()
        {
            _selection.Clear();
        }
    }
}
