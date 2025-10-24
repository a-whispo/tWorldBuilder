using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.TileSelector
{
    internal class SelectTileGrid : UIGrid
    {
        public bool isSearching = false;
        public List<UIElement> allItems = new List<UIElement>();

        public override void Add(UIElement item)
        {
            base.Add(item);
            if (!allItems.Contains(item))
            {
                allItems.Add(item);
            }
        }

        public override void AddRange(IEnumerable<UIElement> items)
        {
            base.AddRange(items);
            foreach (UIElement item in items)
            {
                if (!allItems.Contains(item))
                {
                    allItems.Add(item);
                }
            }
            SortByTilePlacedTileType();
        }

        public void SearchFor(string searchTerm)
        {
            isSearching = true;

            // list of matching items
            List<SelectTileItem> matchingItems = new List<SelectTileItem>(allItems.Count);

            // go over all our possible items
            foreach (SelectTileItem item in allItems)
            {
                // check if the item matches the search term and if its a file (we dont want to display folders when searching)
                if (item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) // if it contains the seatch term add it to items that should show up
                {
                    matchingItems.Add(item);
                }
            }

            // clear and add matching items
            Clear();
            AddRange(matchingItems);

            // recalculate and stuff
            Recalculate();
            RecalculateChildren();
            SortByTilePlacedTileType();
        }

        public void ExitSearch()
        {
            // clear all our current items so we can add the ones that should be visible
            Clear();
            isSearching = false;

            AddRange(allItems);

            // recalculate
            Recalculate();
            RecalculateChildren();
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
