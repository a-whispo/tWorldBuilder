using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements;
using TerrariaInGameWorldEditor.UI.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.TileSelector
{
    internal class SelectTileGrid : UIGrid
    {
        public bool IsSearching { get; set; } = false;
        private List<UIElement> _allItems = new List<UIElement>();

        public override void Add(UIElement item)
        {
            base.Add(item);
            if (!_allItems.Contains(item))
            {
                _allItems.Add(item);
            }
        }

        public override void AddRange(IEnumerable<UIElement> items)
        {
            base.AddRange(items);
            foreach (UIElement item in items)
            {
                if (!_allItems.Contains(item))
                {
                    _allItems.Add(item);
                }
            }
            SortByTilePlacedTileType();
        }

        public void SetSearchBar(TIGWETextField searchBar)
        {
            searchBar.OnTextChanged += (string newText) => SearchFor(newText);
        }

        public void SearchFor(string searchTerm)
        {
            IsSearching = true;

            // list of matching items
            List<SelectTileItem> matchingItems = new List<SelectTileItem>(_allItems.Count);

            // go over all our possible items
            foreach (SelectTileItem item in _allItems)
            {
                // check if the item matches the search term and if its a file (we dont want to display folders when searching)
                if (item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) // if it contains the seatch term add it to items that should show up
                {
                    matchingItems.Add(item);
                }
            }

            // clear and add matching items
            Clear();
            if (!searchTerm.Equals(""))
            {
                AddRange(matchingItems);
            } else
            {
                AddRange(_allItems);
                IsSearching = false;
            }
            SortByTilePlacedTileType();
        }

        public void SortByTilePlacedTileType()
        {
            // sort
            _items.Sort((element1, element2) =>
            {
                SelectTileItem item1 = (SelectTileItem)element1;
                SelectTileItem item2 = (SelectTileItem)element2;
                return item1.ItemId.CompareTo(item2.ItemId);
            });
            Recalculate();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            Rectangle bounds = GetDimensions().ToRectangle();
            bounds.Y -= 8;
            bounds.X -= 10;
            bounds.Height += 16;
            bounds.Width += 2;

            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Border");
            UIElementsUtils.DrawTexture2DWithDimensions(texture, bounds, spriteBatch, cornerSize: 9, barSize: 14);
        }
    }
}
