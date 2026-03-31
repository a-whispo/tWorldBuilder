using TerrariaInGameWorldEditor.Common;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal interface ISelectionTool
    {
        public TileCollection GetSelection();

        public void ResetSelection();
    }
}
