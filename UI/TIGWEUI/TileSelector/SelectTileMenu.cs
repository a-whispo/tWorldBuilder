using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.UIElements.Scrollbar;
using TerrariaInGameWorldEditor.UI.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.TileSelector
{
    internal class SelectTileMenu : TIGWEUI
    {
        private string _searchTerm = "";
        private TIGWETextField _searchBar;
        private SelectTileGrid _grid = null;
        private List<int> _selectableTiles = new List<int>();

        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            if (Left.Pixels == default && Top.Pixels == default)
            {
                Left.Set(750, 0);
                Top.Set(150, 0);
            }
            Width.Set(700, 0);
            Height.Set(440, 0);

            // text
            UIText _menuText = new UIText("Tile Browser");
            _menuText.Left.Set(15, 0);
            _menuText.Top.Set(15, 0);
            _menuText.IgnoresMouseInteraction = true;
            Append(_menuText);

            // search bar
            _searchBar = new TIGWETextField($"Search for tiles... [c/60ABE7:({_selectableTiles.Count})]", 100, 8, 3);
            _searchBar.Width.Set(250, 0);
            _searchBar.Height.Set(26, 0);
            _searchBar.Top.Set(51, 0);
            _searchBar.Left.Set(36, 0);
            Append(_searchBar);
            UIImageButton searchIcon = new UIImageButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/TIGWEUI/TileSelector/Search"));
            searchIcon.SetHoverImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/TIGWEUI/TileSelector/SearchHover"));
            searchIcon.Width.Set(26, 0);
            searchIcon.Height.Set(26, 0);
            searchIcon.Top.Set(51, 0);
            searchIcon.Left.Set(289, 0);
            searchIcon.SetVisibility(0.7f, 1);
            Append(searchIcon);

            // show only 16x16 (pixels)
            UIText only16x16Text = new UIText("[c/EAD87A:Note:] Will default to the top left part on \ntiles bigger than 1x1. Might not always work.");
            only16x16Text.Left.Set(330, 0);
            only16x16Text.Top.Set(35, 0);
            only16x16Text.IgnoresMouseInteraction = true;
            Append(only16x16Text);

            // scrollbar
            TIGWEScrollbar sb = new TIGWEScrollbar(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/TIGWEUI/TileSelector/Texture"), ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/TIGWEUI/TileSelector/Scrollbar"));
            sb.Height.Set(336, 0);
            sb.Left.Set(12, 0);
            sb.Top.Set(88, 0);
            Append(sb);

            // grid
            _grid = new SelectTileGrid();
            _grid.Height.Set(332, 0);
            _grid.Width.Set(650, 0);
            _grid.Left.Set(46, 0);
            _grid.Top.Set(90, 0);
            _grid.SetScrollbar(sb);
            Append(_grid);

            // get all tiles from mods
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                try
                {
                    Item item = new Item(i);
                    if (item.createTile != -1 || item.createWall != -1)
                    {
                        // add to selectable tiles
                        _selectableTiles.Add(i);

                        // add to the grid and add an event
                        SelectTileItem tileItem = new SelectTileItem(i);
                        _grid.Add(tileItem);
                        tileItem.OnLeftClick += (evt, listeningElement) =>
                        {
                            EditorSystem.Local.SelectedTile = tileItem.GetAsTileCopy();
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex}");
                }
            }
            _grid.SortByTilePlacedTileType();
            _searchBar.PlaceholderText = $"Search for tiles... [c/60ABE7:({_selectableTiles.Count})]";
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // searchbar
            if (_searchBar != null)
            {
                if (_searchBar.HasText())
                {
                    if (!_searchTerm.Equals(_searchBar.GetText()))
                    {
                        _grid.SearchFor(_searchBar.GetText());
                        _searchTerm = _searchBar.GetText();
                    }
                }
                else
                {
                    if (_grid.isSearching)
                    {
                        _grid.ExitSearch();
                    }
                }
            }
        }

        private void CloseMenu(UIMouseEvent evt, UIElement listeningElement)
        {
            Visible = false;
        }
    }
}
