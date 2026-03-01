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

        private Func<UIElement, string, bool> _searchFunc;

        public TIGWESearchGrid(Func<UIElement, string, bool> search)
        {
            _searchFunc = search;
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
        }

        public void RemoveSearchBar(TIGWETextField searchBar)
        {
            searchBar.OnTextChanged -= SearchFor;
        }

        public void SearchFor(string searchTerm)
        {
            if (searchTerm.Equals(""))
            {
                base.Clear();
                IsSearching = false;
                base.AddRange(AllItems);
            }
            else
            {
                IsSearching = true;
                List<UIElement> matchingItems = new List<UIElement>();
                foreach (UIElement item in AllItems)
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
