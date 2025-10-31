using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UI.Editor
{
    internal class EditorPalette : UIElement
    {
        private TIGWEImageResizeable _border;
        private UIGrid _paletteGrid;

        public EditorPalette()
        {
            // a border and the grid to hold the items
            _border = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenInnerBorder"), 6, 4);
            Append(_border);
            _paletteGrid = new UIGrid();
            _paletteGrid.Left.Set(8, 0f);
            _paletteGrid.Top.Set(8, 0f);
            _paletteGrid.ListPadding = 2f;
            _paletteGrid.OverflowHidden = false;
            _paletteGrid.ManualSortMethod = (List<UIElement> list) => {
                list.Sort((a, b) =>
                {
                    PaletteItem item1 = (PaletteItem)a;
                    PaletteItem item2 = (PaletteItem)b;
                    if (item1.TileCopy.HasTile)
                    {
                        if (item2.TileCopy.HasTile)
                        {
                            return item1.TileCopy.TileType.CompareTo(item2.TileCopy.TileType);
                        }
                        return -1;
                    }
                    return item1.TileCopy.WallType.CompareTo(item2.TileCopy.WallType);
                });
            };
            Append(_paletteGrid);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // update sizes
            _border.Width.Set(Width.Pixels, 0f);
            _border.Height.Set(Height.Pixels, 0f);
            _paletteGrid.Width.Set(_border.Width.Pixels - 16, 0f);
            _paletteGrid.Height.Set(_border.Height.Pixels - 16, 0f);
            Height.Set(_paletteGrid.Count > 0 ? _paletteGrid._items[^1].Top.Pixels + _paletteGrid._items[^1].Height.Pixels + _paletteGrid.Top.Pixels + 8 : 16, 0f);
            Recalculate();
            base.Draw(spriteBatch);
        }

        public void AddItem(PaletteItem item)
        {
            _paletteGrid.Add(item);
            Recalculate();
        }
    }
}
