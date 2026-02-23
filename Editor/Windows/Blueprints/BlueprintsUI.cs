using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.DirectoryGrid;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.Scrollbar;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.Editor.Windows.Blueprints
{
    internal class BlueprintsUI : TIGWEUI
    {
        private TIGWEDirectoryGrid _grid;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Width.Set(700, 0);
            Height.Set(440, 0);
            _defaultTitle = "Blueprints";

            // open folder
            TIGWEButton openFolder = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/OpenFolderButton"));
            openFolder.Width.Set(26, 0);
            openFolder.Height.Set(26, 0);
            openFolder.Top.Set(42, 0);
            openFolder.Left.Set(6, 0);
            openFolder.SetVisibility(0.7f, 1);
            openFolder.HoverText = "Open save folder";
            openFolder.OnLeftClick += (evt, listeningElement) =>
            {
                Utils.OpenFolder(ModLoader.ModPath.Replace("\\Mods", "") + "\\TIGWE\\saves\\");
            };
            Append(openFolder);

            // create folder
            TIGWEButton createFolder = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/CreateFolderButton"));
            createFolder.Width.Set(26, 0);
            createFolder.Height.Set(26, 0);
            createFolder.Top.Set(42, 0);
            createFolder.Left.Set(openFolder.Left.Pixels + openFolder.Width.Pixels + 2, 0);
            createFolder.SetVisibility(0.7f, 1);
            createFolder.HoverText = "Create new folder";
            createFolder.OnLeftClick += CreateDirectory;
            Append(createFolder);

            // refresh
            TIGWEButton refresh = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/RefreshButton"));
            refresh.Width.Set(26, 0);
            refresh.Height.Set(26, 0);
            refresh.Top.Set(42, 0);
            refresh.Left.Set(createFolder.Left.Pixels + createFolder.Width.Pixels + 2, 0);
            refresh.SetVisibility(0.7f, 1);
            refresh.HoverText = "Refresh";
            refresh.OnLeftClick += (evt, listeningElement) =>
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
            _grid = new TIGWEDirectoryGrid();
            _grid.Height.Set(354, 0);
            _grid.Width.Set(650, 0);
            _grid.Left.Set(14, 0);
            _grid.Top.Set(74, 0);
            _grid.ListPadding = 2;
            _grid.PaddingTop = 2;
            _grid.SetDirectory(ModLoader.ModPath.Replace("\\Mods", "") + "\\TIGWE\\saves\\");
            _grid.SetSearchBar(searchBar);
            searchBar.PlaceholderText = $"Search for files... [c/60ABE7:({_grid.FileCount})]";
            _grid.OnSelectFile += (file) =>
            {
                try
                {
                    TagCompound tag = TagIO.FromFile(file.FullPath);
                    EditorSystem.Local.Clipboard = tag.Get<TileCollection>("TileCollection");
                }
                catch (Exception ex)
                {
                    TerrariaInGameWorldEditor.ModLogger.Warn("Failed to load selected file.", ex);
                    Main.NewText("Failed to load selected file.", Color.Red);
                    EditorSystem.Local.Clipboard = null;
                }
            };
            _grid.RefreshContent();
            Append(_grid);
            TIGWEScrollbar scrollbar = new TIGWEScrollbar();
            scrollbar.Height.Set(_grid.Height.Pixels + 10, 0);
            scrollbar.Width.Set(20, 0);
            scrollbar.Top.Set(_grid.Top.Pixels - 4, 0);
            scrollbar.Left.Set(_grid.Left.Pixels + _grid.Width.Pixels + 10, 0);
            Append(scrollbar);
            _grid.SetScrollbar(scrollbar);
            TIGWEImageResizeable border = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Border"), 6, 4);
            border.IgnoresMouseInteraction = true;
            border.Top.Set(_grid.Top.Pixels - 4, 0);
            border.Left.Set(_grid.Left.Pixels - 8, 0);
            border.Width.Set(_grid.Width.Pixels + 16, 0);
            border.Height.Set(_grid.Height.Pixels + 10, 0);
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
            TIGWEDirectoryFolder folder = new TIGWEDirectoryFolder(fullPath);
            folder.CanSelect = false;

            // add the new folder and do some recalculations
            _grid.Add(folder);

            // goto that element in the grid so we can see what we're naming it
            _grid.Goto((element) => {
                return ((TIGWEDirectoryItem)element).FullPath.Equals(fullPath);
            }, true);

            // initialize the renaming
            folder.StartRename();
        }
    }
}
