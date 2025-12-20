using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.UIElements.Scrollbar;
using TerrariaInGameWorldEditor.UI.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.TileSelector
{
    internal class SelectTileUI : TIGWEUI
    {
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
            Title = "Tile Selector";

            // show only 16x16 (pixels)
            UIText only16x16Text = new UIText("[c/EAD87A:Note:] Will default to the top left part on \ntiles bigger than 1x1. Might not always work.");
            only16x16Text.Left.Set(330, 0);
            only16x16Text.Top.Set(35, 0);
            only16x16Text.IgnoresMouseInteraction = true;
            Append(only16x16Text);

            // search bar
            TIGWETextField _searchBar = new TIGWETextField($"Search for tiles... [c/60ABE7:({_selectableTiles.Count})]", 100);
            _searchBar.Width.Set(250, 0);
            _searchBar.Height.Set(26, 0);
            _searchBar.Top.Set(51, 0);
            _searchBar.Left.Set(36, 0);
            Append(_searchBar);
            UIImageButton searchIcon = new UIImageButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Search"));
            searchIcon.SetHoverImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/SearchHover"));
            searchIcon.Width.Set(26, 0);
            searchIcon.Height.Set(26, 0);
            searchIcon.Top.Set(51, 0);
            searchIcon.Left.Set(_searchBar.Left.Pixels + _searchBar.Width.Pixels + 2, 0);
            searchIcon.SetVisibility(0.7f, 1);
            Append(searchIcon);

            // scrollbar
            TIGWEScrollbar sb = new TIGWEScrollbar(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"), ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Scrollbar"));
            sb.Height.Set(336, 0);
            sb.Left.Set(12, 0);
            sb.Top.Set(88, 0);
            Append(sb);

            // grid
            SelectTileGrid _grid = new SelectTileGrid();
            _grid.Height.Set(332, 0);
            _grid.Width.Set(650, 0);
            _grid.Left.Set(46, 0);
            _grid.Top.Set(90, 0);
            _grid.SetScrollbar(sb);
            _grid.SetSearchBar(_searchBar);
            Append(_grid);

            Task task = new Task(() => {
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
            });
            task.Start();
        }
    }
}
