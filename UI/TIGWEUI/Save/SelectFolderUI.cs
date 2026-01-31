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

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.Save
{
    // this is pretty much the exact same as BlueprintsUI, so might make a better system later
    internal class SelectFolderUI : TIGWEUI
    {
        public delegate void SelectFolderEventHandler(UIDirectoryFolder folder);
        public event SelectFolderEventHandler OnSelectFolder;

        private UIDirectoryGrid _grid;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Width.Set(700, 0);
            Height.Set(440, 0);
            Left.Set(750, 0);
            Top.Set(150, 0);
            _defaultTitle = "(Save) Select Folder";

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
            searchBar.ShowSearchIcon = true;
            searchBar.Width.Set(250, 0);
            searchBar.Height.Set(26, 0);
            searchBar.Top.Set(42, 0);
            searchBar.Left.Set(refresh.Left.Pixels + refresh.Width.Pixels + 2, 0);
            Append(searchBar);

            // grid
            _grid = new UIDirectoryGrid();
            _grid.Height.Set(354, 0);
            _grid.Width.Set(650, 0);
            _grid.Left.Set(36, 0);
            _grid.Top.Set(74, 0);
            _grid.ListPadding = 2;
            _grid.PaddingTop = 2;
            _grid.SetDirectory(ModLoader.ModPath.Replace("\\Mods", "") + "\\TIGWE\\saves\\");
            _grid.SetSearchBar(searchBar);
            searchBar.PlaceholderText = $"Search for folders... [c/60ABE7:({_grid.FolderCount})]";
            _grid.CanSelectFiles = false;
            _grid.ShouldShowFiles = false;
            _grid.ShouldFilesAppearInSearch = false;
            _grid.ShouldFoldersAppearInSearch = true;
            _grid.CanSelectFolders = true;
            _grid.OnSelectFolder += FolderSelected;
            _grid.RefreshContent();
            Append(_grid);
            TIGWEScrollbar sb = new TIGWEScrollbar();
            sb.Height.Set(_grid.Height.Pixels + 10, 0);
            sb.Width.Set(20, 0);
            sb.Top.Set(_grid.Top.Pixels - 4, 0);
            sb.Left.Set(_grid.Left.Pixels - sb.Width.Pixels - 10, 0);
            Append(sb);
            _grid.SetScrollbar(sb);
            TIGWEImageResizeable border = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenInnerBorder"), 6, 4);
            border.IgnoresMouseInteraction = true;
            border.Top.Set(_grid.Top.Pixels - 4, 0);
            border.Left.Set(_grid.Left.Pixels - 8, 0);
            border.Width.Set(_grid.Width.Pixels + 16, 0);
            border.Height.Set(_grid.Height.Pixels + 10, 0);
            Append(border);
        }

        private void FolderSelected(UIDirectoryFolder folder)
        {
            OnSelectFolder?.Invoke(folder);
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
            folder.CanSelect = true;

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
