using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UIElements.SearchGrid
{
    internal class TIGWESearchGrid : UIGrid
    {
        public bool IsSearching { get; private set; } = false;
        public List<UIElement> ShownItems => _items;
        public List<UIElement> AllItems { get; private set; } = new List<UIElement>();

        private Func<UIElement, bool> _filter;
        private Func<UIElement, string, bool> _searchFunc;
        private TIGWETextField _searchBar;

        public TIGWESearchGrid(Func<UIElement, string, bool> search)
        {
            _searchFunc = search;
            ClearFilter();
        }

        public void ClearFilter()
        {
            _filter = (_) => { return true; };
        }

        public void SetFilter(Func<UIElement, bool> filter)
        {
            _filter = filter;
            SearchFor(_searchBar == null ? "" : _searchBar.GetText());
        }

        private List<UIElement> ApplyFilter(List<UIElement> items)
        {
            List<UIElement> filteredItems = new List<UIElement>();
            foreach (UIElement item in items)
            {
                if (_filter(item))
                {
                    filteredItems.Add(item);
                }
            }
            return filteredItems;
        }

        public override void Clear()
        {
            base.Clear();
            AllItems.Clear();
        }

        public override bool Remove(UIElement item)
        {
            if (base.Remove(item))
            {
                AllItems.Remove(item);
                return true;
            }
            return false;
        }

        public override void Add(UIElement item)
        {
            base.Add(item);
            AllItems.Add(item);
        }

        public override void AddRange(IEnumerable<UIElement> items)
        {
            base.AddRange(items);
            AllItems.AddRange(items);
        }

        public void SetSearchBar(TIGWETextField searchBar)
        {
            searchBar.OnTextChanged -= SearchFor;
            searchBar.OnTextChanged += SearchFor;
            _searchBar = searchBar;
        }

        public void RemoveSearchBar(TIGWETextField searchBar)
        {
            searchBar.OnTextChanged -= SearchFor;
            _searchBar = null;
        }

        public void SearchFor(string searchTerm)
        {
            if (searchTerm.Equals(""))
            {
                base.Clear();
                IsSearching = false;
                base.AddRange(ApplyFilter(AllItems));
            }
            else
            {
                IsSearching = true;
                List<UIElement> matchingItems = new List<UIElement>();
                foreach (UIElement item in ApplyFilter(AllItems))
                {
                    if (_searchFunc.Invoke(item, searchTerm))
                    {
                        matchingItems.Add(item);
                    }
                }
                base.Clear();
                base.AddRange(matchingItems);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }
    }
}
