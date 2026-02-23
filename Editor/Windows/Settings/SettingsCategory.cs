using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements.ButtonResizable;

namespace TerrariaInGameWorldEditor.Editor.Windows.Settings
{
    internal class SettingsCategory : UIElement
    {
        private List<UIElement> _options = new List<UIElement>();
        private UIGrid _gridToPopulate;
        private TIGWEImageButtonResizeable _body;
        private bool _isSelected = false;

        public SettingsCategory(string category)
        {
            _body = new TIGWEImageButtonResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));
            _body.Text = category;
            _body.TextOffsetTop = 10;
            _body.IgnoresMouseInteraction = true;
            _body.Width.Set(0, 1);
            _body.Height.Set(0, 1);
            Append(_body);
            Width.Set(-4, 1);
            Height.Set(38, 0);
        }

        public void SetOptionsGrid(UIGrid grid)
        {
            _gridToPopulate = grid;
        }

        public void AddOption(UIElement option)
        {
            _options.Add(option);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            _body.Texture = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/TextureHover");
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            if (!_isSelected)
            {
                _body.Texture = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture");
            }
        }

        public void SetSelected()
        {
            if (_gridToPopulate != null)
            {
                _body.Texture = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/TextureHover");
                _gridToPopulate.Clear();
                _gridToPopulate.AddRange(_options);
                _isSelected = true;
            }
        }

        public void SetNotSelected()
        {
            if (_gridToPopulate != null)
            {
                _body.Texture = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture");
                _isSelected = false;
            }
        }
    }
}
