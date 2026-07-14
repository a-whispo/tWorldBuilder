using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.DirectoryGrid;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.Scrollbar;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.Editor.Windows.Save
{
    // this is pretty much the exact same as BlueprintsUI, so might make a better system later
    internal class SelectFolderUI : TIGWEUI
    {
        public delegate void SelectFolderEventHandler(TIGWEDirectoryFolder folder);
        public event SelectFolderEventHandler OnSelectFolder;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Width.Set(700, 0);
            Height.Set(440, 0);
            _defaultTitle = LocalizationUtils.GetTextValue("Windows.SelectFolder.Title");

            // grid
            TIGWEDirectoryGrid grid = new TIGWEDirectoryGrid();
            grid.Height.Set(354, 0);
            grid.Width.Set(650, 0);
            grid.Left.Set(14, 0);
            grid.Top.Set(74, 0);
            grid.ListPadding = 2;
            grid.PaddingTop = 2;
            grid.SetDirectory(Path.Combine(Path.GetDirectoryName(ModLoader.ModPath), TerrariaInGameWorldEditor.MODNAME, "saves"));
            grid.CanSelectFiles = false;
            grid.ShouldShowFiles = false;
            grid.ShouldFilesAppearInSearch = false;
            grid.ShouldFoldersAppearInSearch = true;
            grid.CanSelectFolders = true;
            grid.OnSelectFolder += FolderSelected;
            grid.RefreshContent();
            Append(grid);
            TIGWEScrollbar scrollbar = new TIGWEScrollbar();
            scrollbar.Height.Set(grid.Height.Pixels + 10, 0);
            scrollbar.Width.Set(20, 0);
            scrollbar.Top.Set(grid.Top.Pixels - 4, 0);
            scrollbar.Left.Set(grid.Left.Pixels + grid.Width.Pixels + 10, 0);
            Append(scrollbar);
            grid.SetScrollbar(scrollbar);
            TIGWEImageResizeable border = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Border"), 6, 4);
            border.IgnoresMouseInteraction = true;
            border.Top.Set(grid.Top.Pixels - 4, 0);
            border.Left.Set(grid.Left.Pixels - 8, 0);
            border.Width.Set(grid.Width.Pixels + 16, 0);
            border.Height.Set(grid.Height.Pixels + 10, 0);
            Append(border);

            // open folder
            TIGWEButton openFolder = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/OpenFolderButton", AssetRequestMode.ImmediateLoad));
            openFolder.Top.Set(42, 0);
            openFolder.Left.Set(6, 0);
            openFolder.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.SelectFolder.HoverText.OpenFolder");
            openFolder.OnLeftClick += (_, _) =>
            {
                Utils.OpenFolder(Path.Combine(Path.GetDirectoryName(ModLoader.ModPath), TerrariaInGameWorldEditor.MODNAME, "saves"));
            };
            Append(openFolder);

            // create folder
            TIGWEButton createFolder = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/CreateFolderButton", AssetRequestMode.ImmediateLoad));
            createFolder.Top.Set(42, 0);
            createFolder.Left.Set(openFolder.Left.Pixels + openFolder.Width.Pixels + 2, 0);
            createFolder.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.SelectFolder.HoverText.CreateFolder");
            createFolder.OnLeftClick += (_, _) => grid.CreateNewDirectory();
            Append(createFolder);

            // refresh
            TIGWEButton refresh = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/RefreshButton", AssetRequestMode.ImmediateLoad));
            refresh.Top.Set(42, 0);
            refresh.Left.Set(createFolder.Left.Pixels + createFolder.Width.Pixels + 2, 0);
            refresh.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.SelectFolder.HoverText.Refresh");
            Append(refresh);

            // search bar
            TIGWETextField searchBar = new TIGWETextField(LocalizationUtils.GetTextValue("Windows.SelectFolder.FieldText.Search", grid.FileCount), 100);
            searchBar.ShowSearchIcon = true;
            searchBar.Width.Set(250, 0);
            searchBar.Height.Set(26, 0);
            searchBar.Top.Set(42, 0);
            searchBar.Left.Set(refresh.Left.Pixels + refresh.Width.Pixels + 2, 0);
            grid.SetSearchBar(searchBar);
            Append(searchBar);

            refresh.OnLeftClick += (_, _) =>
            {
                grid.RefreshContent();
                searchBar.PlaceholderText = LocalizationUtils.GetTextValue("Windows.SelectFolder.FieldText.Search", grid.FileCount);
            };
        }

        private void FolderSelected(TIGWEDirectoryFolder folder)
        {
            OnSelectFolder?.Invoke(folder);
        }
    }
}
