using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.Editor
{
    internal class EditorPalette : UIElement
    {
        public bool IsDeletingItems { get; set; } = false;

        private TIGWEImageResizeable _border;
        private UIGrid _paletteGrid;

        public EditorPalette()
        {
            // a border and the grid to hold the items
            _border = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Border"), 6, 4);
            _border.Width.Set(0, 1f);
            _border.Height.Set(0, 1f);
            Append(_border);
            _paletteGrid = new UIGrid();
            _paletteGrid.Left.Set(8, 0f);
            _paletteGrid.Top.Set(8, 0f);
            _paletteGrid.Width.Set(-16, 1f);
            _paletteGrid.Height.Set(-16, 1f);
            _paletteGrid.ListPadding = 2f;
            _paletteGrid.OverflowHidden = false;
            _paletteGrid.ManualSortMethod = (list) => {
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

        public override void Recalculate()
        {
            Height.Set(_paletteGrid.Count > 0 ? _paletteGrid._items[^1].Top.Pixels + _paletteGrid._items[^1].Height.Pixels + 16 : 12, 0f);
            base.Recalculate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Recalculate();
            base.Draw(spriteBatch);
        }

        public void AddItem(PaletteItem item)
        {
            _paletteGrid.Add(item);
            item.OnLeftClick += (_, _) =>
            {
                if (IsDeletingItems)
                {
                    RemoveItem(item);
                }
            };
            Recalculate();
        }

        public void RemoveItem(PaletteItem item)
        {
            if (_paletteGrid.Count > 0)
            {
                _paletteGrid.Remove(item);
                Recalculate();
            }
        }

        public void ClearItems()
        {
            _paletteGrid.Clear();
            Recalculate();
        }
    }
}
