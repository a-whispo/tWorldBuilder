using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using System.IO.Compression;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.ButtonResizable;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.Editor.Windows.Save
{
    internal class SaveUI : TIGWEUI
    {
        private string _selectedPath;
        private TIGWETextField _pathField;
        private TIGWETextField _saveAsField;
        private TIGWEImageButtonResizeable _saveButton;
        private SelectFolderUI _selectFolderUI;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Height.Set(168, 0);
            Width.Set(330, 0);
            _defaultTitle = LocalizationUtils.GetTextValue("Windows.Save.Title");

            // save as
            UIText saveAsText = new UIText(LocalizationUtils.GetTextValue("Windows.Save.LabelText.SaveAs"));
            saveAsText.Top.Set(44, 0);
            saveAsText.Left.Set(6, 0);
            Append(saveAsText);
            _saveAsField = new TIGWETextField(LocalizationUtils.GetTextValue("Windows.Save.FieldText.SaveAs"), 50);
            _saveAsField.Height.Set(26, 0);
            _saveAsField.Width.Set(250, 0);
            _saveAsField.Top.Set(saveAsText.Top.Pixels + 18, 0);
            _saveAsField.Left.Set(saveAsText.Left.Pixels, 0);
            Append(_saveAsField);

            // path
            UIText pathText = new UIText(LocalizationUtils.GetTextValue("Windows.Save.LabelText.Path"));
            pathText.Top.Set(_saveAsField.Top.Pixels + _saveAsField.Height.Pixels + 6, 0);
            pathText.Left.Set(6, 0);
            Append(pathText);
            _pathField = new TIGWETextField($"", 50);
            ResetPath();
            _pathField.CanFocus = false;
            _pathField.Height.Set(26, 0);
            _pathField.Width.Set(250, 0);
            _pathField.Top.Set(pathText.Top.Pixels + 18, 0);
            _pathField.Left.Set(pathText.Left.Pixels, 0);
            Append(_pathField);

            // path select button
            TIGWEButton pathSelect = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/OpenFolderButton", AssetRequestMode.ImmediateLoad));
            pathSelect.Left.Set(_pathField.Left.Pixels + _pathField.Width.Pixels + 2, 0);
            pathSelect.Top.Set(_pathField.Top.Pixels, 0);
            pathSelect.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.Save.HoverText.SelectPath");
            pathSelect.OnLeftClick += (_, _) => SelectPath();
            Append(pathSelect);

            // path reset button
            TIGWEButton pathReset = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/RefreshButton", AssetRequestMode.ImmediateLoad));
            pathReset.Left.Set(pathSelect.Left.Pixels + pathSelect.Width.Pixels + 2, 0);
            pathReset.Top.Set(_pathField.Top.Pixels, 0);
            pathReset.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.Save.HoverText.ResetPath");
            pathReset.OnLeftClick += (_, _) => ResetPath();
            Append(pathReset);

            // save button
            _saveButton = new TIGWEImageButtonResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));
            _saveButton.Text = LocalizationUtils.GetTextValue("Windows.Save.LabelText.Save");
            _saveButton.TextureHover = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/TextureHover");
            _saveButton.Left.Set(6, 0);
            _saveButton.Top.Set(_pathField.Top.Pixels + _pathField.Height.Pixels + 2, 0);
            _saveButton.Width.Set(64, 0);
            _saveButton.Height.Set(26, 0);
            _saveButton.OnLeftClick += (_, _) => SaveToFile();
            Append(_saveButton);

            // make sure to hide the select folder UI if this one is closed
            OnHide += (_, _) =>
            {
                if (_selectFolderUI != null)
                {
                    _selectFolderUI.Visible = false;
                }
            };

            // update save button state based on selection
            EditorSystem.Local.OnSelectionChanged += (_, _) =>
            {
                if (EditorSystem.Local.CurrentSelection?.Count > 0)
                {
                    _saveButton.IgnoresMouseInteraction = false;
                    _saveButton.SetVisibility(1f, 1f);
                }
                else
                {
                    _saveButton.IgnoresMouseInteraction = true;
                    _saveButton.SetVisibility(0.6f, 0.6f);
                }
            };

            Height.Set(_saveButton.Top.Pixels + _saveButton.Height.Pixels + _saveButton.Left.Pixels, 0);
            Width.Set(pathReset.Left.Pixels + pathReset .Width.Pixels + 6, 0);
        }

        private void SelectPath()
        {
            // make sure its registered
            if (_selectFolderUI == null)
            {
                _selectFolderUI = new SelectFolderUI();
                _selectFolderUI.OnSelectFolder += (folder) =>
                {
                    _selectedPath = folder.FullPath;
                    _pathField?.SetText(LocalizationUtils.GetTextValue("Windows.Save.FieldText.CustomPath", folder.Name));
                    _selectFolderUI.Visible = false;
                };
                TIGWEUISystem.Local.RegisterUI(_selectFolderUI);
            }
            _selectFolderUI.Visible = !_selectFolderUI.Visible;
        }

        private void ResetPath()
        {
            _selectedPath = Path.Combine(Path.GetDirectoryName(ModLoader.ModPath), TerrariaInGameWorldEditor.MODNAME, "saves");
            _pathField?.SetText(LocalizationUtils.GetTextValue("Windows.Save.FieldText.DefaultPath"));
        }

        private void SaveToFile()
        {
            try
            {
                // get path where we should save to and create the saves directory if it doesnt exist
                string name = _saveAsField.GetText();
                string modsPath = Path.GetDirectoryName(ModLoader.ModPath);
                Directory.CreateDirectory(Path.Combine(modsPath, TerrariaInGameWorldEditor.MODNAME, "saves"));
                string path = Path.Combine(_selectedPath, $"{name}.twb");
                if (!Path.Exists(_selectedPath))
                {
                    TerrariaInGameWorldEditor.Warn(LocalizationUtils.GetTextValue("Windows.Save.Exceptions.FolderMissing"));
                    return;
                }
                if (File.Exists(path))
                {
                    TerrariaInGameWorldEditor.Warn(LocalizationUtils.GetTextValue("Windows.Save.Exceptions.FileExists"));
                    return;
                }
                WriteTwbFile(path, EditorSystem.Local.CurrentSelection);

                // reset UI
                _saveAsField.SetText("");
                Visible = false;
            }
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.Warn(LocalizationUtils.GetTextValue("Windows.Save.Exceptions.SaveFailed"), ex);
            }
        }

        public static void WriteTwbFile(string path, TileCollection tc)
        {
            if (File.Exists(path))
            {
                return;
            }
            byte version = 3;
            Stream fileStream = File.Create(path);

            // write header
            using (BinaryWriter headerWriter = new BinaryWriter(fileStream, System.Text.Encoding.UTF8, true))
            {
                headerWriter.Write("twb");
                headerWriter.Write(version);
            }

            // write data
            using (BinaryWriter collectionWriter = new BinaryWriter(new BufferedStream(new DeflateStream(fileStream, CompressionLevel.Optimal), 10000)))
            {
                TileCollection.WriteTileCollection(collectionWriter, tc);
            }
        }
    }
}
