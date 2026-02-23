using System.Collections.Generic;

namespace TerrariaInGameWorldEditor.Editor.Windows.Settings
{
    internal class SettingsGroup : SettingsNode
    {
        private List<SettingsNode> _nodes = new List<SettingsNode>();
        private float _currentTop = 0;

        public SettingsGroup()
        {
            Width.Set(-4, 1);
        }

        public void AddNode(SettingsNode node)
        {
            _nodes.Add(node);
            node.Top.Set(_currentTop, 0);
            node.Width.Set(0, 1);
            node.ShowBody = false;
            _currentTop += node.Height.Pixels - 10;
            Append(node);
        }

        public void RemoveNode(SettingsNode node)
        {
            _nodes.Remove(node);
            _currentTop -= node.Height.Pixels + 10;
            RemoveChild(node);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            Height.Set(_currentTop + 10, 0);
        }
    }
}
