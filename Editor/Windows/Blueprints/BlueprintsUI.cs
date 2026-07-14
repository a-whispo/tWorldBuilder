using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor.Windows.Settings;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.DirectoryGrid;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.Scrollbar;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.Editor.Windows.Blueprints
{
    internal class BlueprintsUI : TIGWEUI
    {
        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Width.Set(700, 0);
            Height.Set(440, 0);
            _defaultTitle = LocalizationUtils.GetTextValue("Windows.Blueprints.Title");

            // grid
            TIGWEDirectoryGrid grid = new TIGWEDirectoryGrid();
            grid.Height.Set(354, 0);
            grid.Width.Set(650, 0);
            grid.Left.Set(14, 0);
            grid.Top.Set(74, 0);
            grid.ListPadding = 2;
            grid.PaddingTop = 2;
            grid.SetDirectory(Path.Combine(Path.GetDirectoryName(ModLoader.ModPath), TerrariaInGameWorldEditor.MODNAME, "saves"));
            grid.OnSelectFile += (file) =>
            {
                try
                {
                    EditorSystem.Local.Clipboard = ReadTwbFile(file.FullPath, out HashSet<string> missingMods);
                    if (missingMods?.Count > 0)
                    {
                        EditorSystem.Local.Clipboard = null;
                        string msg = LocalizationUtils.GetTextValue("Windows.Blueprints.Exceptions.MissingMods");
                        foreach (string mod in missingMods)
                        {
                            msg += $"\n{mod}";
                        }
                        TerrariaInGameWorldEditor.Warn(msg);
                    }
                    else
                    {
                        TerrariaInGameWorldEditor.NewText(LocalizationUtils.GetTextValue("Windows.Blueprints.Messages.Loaded", file.Name));
                    }
                    if (!Path.HasExtension(file.FullPath) && !File.Exists($"{file.FullPath}.twb"))
                    {
                        File.Move(file.FullPath, $"{file.FullPath}.twb");
                        file.FullPath += ".twb";
                    }
                }
                catch (Exception ex)
                {
                    TerrariaInGameWorldEditor.Warn(LocalizationUtils.GetTextValue("Windows.Blueprints.Exceptions.LoadFailed"), ex);
                    EditorSystem.Local.Clipboard = null;
                }
            };
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
            openFolder.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.Blueprints.HoverText.OpenFolder");
            openFolder.OnLeftClick += (_, _) =>
            {
                Utils.OpenFolder(Path.Combine(Path.GetDirectoryName(ModLoader.ModPath), TerrariaInGameWorldEditor.MODNAME, "saves"));
            };
            Append(openFolder);

            // create folder
            TIGWEButton createFolder = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/CreateFolderButton", AssetRequestMode.ImmediateLoad));
            createFolder.Top.Set(42, 0);
            createFolder.Left.Set(openFolder.Left.Pixels + openFolder.Width.Pixels + 2, 0);
            createFolder.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.Blueprints.HoverText.CreateFolder");
            createFolder.OnLeftClick += (_, _) => grid.CreateNewDirectory();
            Append(createFolder);

            // refresh
            TIGWEButton refresh = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/RefreshButton", AssetRequestMode.ImmediateLoad));
            refresh.Top.Set(42, 0);
            refresh.Left.Set(createFolder.Left.Pixels + createFolder.Width.Pixels + 2, 0);
            refresh.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.Blueprints.HoverText.Refresh");
            Append(refresh);

            // search bar
            TIGWETextField searchBar = new TIGWETextField(LocalizationUtils.GetTextValue("Windows.Blueprints.FieldText.Search", grid.FileCount), 100);
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
                searchBar.PlaceholderText = LocalizationUtils.GetTextValue("Windows.Blueprints.FieldText.Search", grid.FileCount);
            };
        }

        private static TileCollection ReadTwbFile(string path, out HashSet<string> missingMods)
        {
            missingMods = new HashSet<string>();
            Stream fileStream = File.OpenRead(path);
            Stream dataStream;
            byte version = 0;

            // read header
            using (BinaryReader headerReader = new BinaryReader(fileStream, System.Text.Encoding.UTF8, true))
            {
                if (headerReader.ReadString().Equals("twb"))
                {
                    version = headerReader.ReadByte();
                    dataStream = new BufferedStream(new DeflateStream(fileStream, CompressionMode.Decompress), 10000);
                }
                else
                {
                    fileStream.Position = 0;
                    dataStream = fileStream;
                }
            }

            // read the right version
            using (BinaryReader collectionReader = new BinaryReader(dataStream, System.Text.Encoding.UTF8, false))
            {
                switch (version)
                {
                    case 0:
                        return TileCollection.ReadV0TileCollection(collectionReader, out missingMods);

                    case 1:
                        return TileCollection.ReadV1TileCollection(collectionReader, out missingMods);

                    case 2:
                        return TileCollection.ReadV2TileCollection(collectionReader, out missingMods);

                    case 3:
                        return TileCollection.ReadV3TileCollection(collectionReader, out missingMods);

                    default:
                        throw new Exception($"Unknown twb file verson: {version}.");
                }
            }
        }
    }
}
