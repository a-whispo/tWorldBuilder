using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
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
            _defaultTitle = "Save";

            // save as
            UIText saveAsText = new UIText("Save selection as:");
            saveAsText.Top.Set(44, 0);
            saveAsText.Left.Set(6, 0);
            Append(saveAsText);
            _saveAsField = new TIGWETextField("Enter file name...", 50);
            _saveAsField.Height.Set(26, 0);
            _saveAsField.Width.Set(250, 0);
            _saveAsField.Top.Set(saveAsText.Top.Pixels + 18, 0);
            _saveAsField.Left.Set(saveAsText.Left.Pixels, 0);
            Append(_saveAsField);

            // path
            UIText pathText = new UIText("Path:");
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
            TIGWEButton pathSelect = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/OpenFolderButton"));
            pathSelect.SetVisibility(0.8f, 1f);
            pathSelect.Left.Set(_pathField.Left.Pixels + _pathField.Width.Pixels + 2, 0);
            pathSelect.Top.Set(_pathField.Top.Pixels, 0);
            pathSelect.Width.Set(26, 0);
            pathSelect.Height.Set(26, 0);
            pathSelect.HoverText = "Select folder";
            pathSelect.OnLeftClick += (_, _) => SelectPath();
            Append(pathSelect);

            // path reset button
            TIGWEButton pathReset = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/RefreshButton"));
            pathReset.SetVisibility(0.8f, 1f);
            pathReset.Left.Set(pathSelect.Left.Pixels + pathSelect.Width.Pixels + 2, 0);
            pathReset.Top.Set(_pathField.Top.Pixels, 0);
            pathReset.Width.Set(26, 0);
            pathReset.Height.Set(26, 0);
            pathReset.HoverText = "Reset to default";
            pathReset.OnLeftClick += (_, _) => ResetPath();
            Append(pathReset);

            // save button
            _saveButton = new TIGWEImageButtonResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));
            _saveButton.Text = "Save";
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
                    _pathField?.SetText($"...\\{folder.Name} [c/F3CD5A:(custom)]");
                    _selectFolderUI.Visible = false;
                };
                TIGWEUISystem.Local.RegisterUI(_selectFolderUI);
            }
            _selectFolderUI.Visible = !_selectFolderUI.Visible;
        }

        private void ResetPath()
        {
            _selectedPath = $"{ModLoader.ModPath.Replace("\\Mods", "")}\\TIGWE\\saves";
            _pathField?.SetText($"...\\saves [c/60ABE7:(default)]");
        }

        private void SaveToFile()
        {
            try
            {
                // get path where we should save to and create the saves directory if it doesnt exist
                string name = _saveAsField.GetText();
                string modsPath = ModLoader.ModPath.Replace("\\Mods", "");
                Directory.CreateDirectory($"{modsPath}\\TIGWE\\saves");
                string path = $"{_selectedPath}\\{name}";
                if (File.Exists(path))
                {
                    Main.NewText("A file with that name already exists.", Color.Red);
                    return;
                }

                // create tag compound of current selection
                TagCompound tag = new TagCompound();
                tag["TileCollection"] = EditorSystem.Local.CurrentSelection;
                TagIO.ToFile(tag, path);

                // reset UI
                _saveAsField.SetText("");
                Visible = false;
            }
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.ModLogger.Warn("Failed to save current selection.", ex);
            }
        }
    }
}
