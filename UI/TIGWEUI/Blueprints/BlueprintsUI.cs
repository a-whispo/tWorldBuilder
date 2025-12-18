using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.Button;
using TerrariaInGameWorldEditor.UI.UIElements.DirectoryGrid;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UI.UIElements.Scrollbar;
using TerrariaInGameWorldEditor.UI.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.Blueprints
{
    internal class BlueprintsUI : TIGWEUI
    {
        private UIDirectoryGrid _grid;

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
            Title = "Blueprints";

            // open folder
            TIGWEButton openFolder = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/OpenFolder"));
            openFolder.Width.Set(26, 0);
            openFolder.Height.Set(26, 0);
            openFolder.Top.Set(42, 0);
            openFolder.Left.Set(28, 0);
            openFolder.SetVisibility(0.7f, 1);
            openFolder.HoverText = "Open save folder";
            openFolder.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                Utils.OpenFolder(ModLoader.ModPath.Replace("\\Mods", "") + "\\TIGWE\\saves\\");
            };
            Append(openFolder);

            // create folder
            TIGWEButton createFolder = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/CreateFolder"));
            createFolder.Width.Set(26, 0);
            createFolder.Height.Set(26, 0);
            createFolder.Top.Set(42, 0);
            createFolder.Left.Set(openFolder.Left.Pixels + openFolder.Width.Pixels + 2, 0);
            createFolder.SetVisibility(0.7f, 1);
            createFolder.HoverText = "Create new folder";
            createFolder.OnLeftClick += CreateDirectory;
            Append(createFolder);

            // refresh
            TIGWEButton refresh = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Refresh"));
            refresh.Width.Set(26, 0);
            refresh.Height.Set(26, 0);
            refresh.Top.Set(42, 0);
            refresh.Left.Set(createFolder.Left.Pixels + createFolder.Width.Pixels + 2, 0);
            refresh.SetVisibility(0.7f, 1);
            refresh.HoverText = "Refresh";
            refresh.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                _grid.RefreshContent();
            };
            Append(refresh);

            // search bar
            TIGWETextField searchBar = new TIGWETextField("Search for files...", 100);
            searchBar.Width.Set(250, 0);
            searchBar.Height.Set(26, 0);
            searchBar.Top.Set(42, 0);
            searchBar.Left.Set(refresh.Left.Pixels + refresh.Width.Pixels + 2, 0);
            Append(searchBar);
            UIImageButton searchIcon = new UIImageButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Search"));
            searchIcon.SetHoverImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/SearchHover"));
            searchIcon.Width.Set(26, 0);
            searchIcon.Height.Set(26, 0);
            searchIcon.Top.Set(42, 0);
            searchIcon.Left.Set(373, 0);
            searchIcon.SetVisibility(0.7f, 1);
            Append(searchIcon);

            // scrollbar
            TIGWEScrollbar sb = new TIGWEScrollbar(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"), ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Scrollbar"));
            sb.Height.Set(352, 0);
            sb.Left.Set(6, 0);
            sb.Top.Set(76, 0);
            Append(sb);

            // grid
            _grid = new UIDirectoryGrid();
            _grid.Height.Set(354, 0);
            _grid.Width.Set(650, 0);
            _grid.Left.Set(sb.Left.Pixels + sb.Width.Pixels + 10, 0);
            _grid.Top.Set(74, 0);
            _grid.SetScrollbar(sb);
            _grid.SetSearchBar(searchBar);
            _grid.ListPadding = 2;
            _grid.PaddingTop = 2;
            _grid.SetDirectory(ModLoader.ModPath.Replace("\\Mods", "") + "\\TIGWE\\saves\\");
            _grid.CanSelectFolders = false;
            _grid.RefreshContent();
            Append(_grid);
            TIGWEImageResizeable border = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenInnerBorder"), 6, 4);
            border.IgnoresMouseInteraction = true;
            border.Top.Set(_grid.Top.Pixels - 4, 0);
            border.Left.Set(_grid.Left.Pixels - 8, 0);
            border.Width.Set(_grid.Width.Pixels + 16, 0f);
            border.Height.Set(_grid.Height.Pixels + 10, 0f);
            Append(border);
        }

        private void CreateDirectory(UIMouseEvent evt, UIElement listeningElement)
        {
            if (_grid.IsSearching)
            {
                return;
            }

            // make sure we dont try to create a file with the same name as another one
            int num = 1;
            while (Directory.Exists($"{ModLoader.ModPath.Replace("\\Mods", "")}\\TIGWE\\saves\\New Folder ({num})"))
            {
                num++;
            }

            string fullPath = $"{ModLoader.ModPath.Replace("\\Mods", "")}\\TIGWE\\saves\\New Folder ({num})";
            // create the directory and UIBlueprintItem
            Directory.CreateDirectory(fullPath);
            UIDirectoryFolder folder = new UIDirectoryFolder(fullPath);
            folder.CanSelect = false;

            // add the new folder and do some recalculations
            _grid.Add(folder);

            // goto that element in the grid so we can see what we're naming it
            _grid.Goto((UIElement element) => {
                return ((UIDirectoryItem)element).FullPath.Equals(fullPath);
            }, true);

            // initialize the renaming
            folder.StartRename();
        }
    }
}
