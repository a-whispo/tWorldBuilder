using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UIElements.DirectoryGrid
{
    internal abstract class TIGWEDirectoryItem : UIElement
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool HasParentFolder => _parentFolder != null;
        public bool CanSelect 
        { 
            get 
            { 
                return _canSelect;
            } 
            set 
            {
                _canSelect = value;
                _selectButton.SetVisibility(value ? 0.8f : 0f, value ? 1f : 0f);
                _selectButton.IgnoresMouseInteraction = !value;
            } 
        }

        protected bool _isHoveringOverButtons => IsMouseHovering && !_texture.IsMouseHovering;
        protected TIGWEDirectoryFolder _parentFolder;
        protected TIGWEDirectoryGrid _parentGrid;
        protected UIImage _icon;
        protected TIGWEButton _deleteButton;
        protected TIGWEButton _renameButton;
        protected TIGWEButton _selectButton;

        private bool _canSelect = true;
        private UIText _nameText;
        private TIGWEImageResizeable _texture;
        private bool _isDeleting = false;
        private bool _isRenaming = false;
        private TIGWEButton _confirmRenameButton;
        private TIGWETextField _renameTextField;


        public TIGWEDirectoryItem(string fullPath)
        {
            FullPath = fullPath;
            Name = Path.GetFileNameWithoutExtension(@FullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            // texture
            _texture = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture"));
            _texture.TextureHover = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/TextureHover");
            Append(_texture);

            // item name
            _nameText = new UIText(Name);
            _nameText.IgnoresMouseInteraction = true;
            _nameText.Left.Set(6, 0f);
            _nameText.Top.Set(10, 0f);
            Append(_nameText);

            // delete
            _deleteButton = new TIGWEButton(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/Delete", AssetRequestMode.ImmediateLoad));
            ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/DeleteConfirm");
            _deleteButton.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.UIElements.DirectoryItem.HoverText.Delete");
            _deleteButton.OnLeftClick += (_, _) => StartDelete();
            _deleteButton.OnMouseOut += (_, _) => CancelDelete();
            Append(_deleteButton);

            // rename
            _renameButton = new TIGWEButton(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/Rename", AssetRequestMode.ImmediateLoad));
            _renameButton.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.UIElements.DirectoryItem.HoverText.Rename");
            _renameButton.OnLeftClick += (_, _) => StartRename();
            Append(_renameButton);
            _renameTextField = new TIGWETextField("", 100);
            _renameTextField.Height.Set(26, 0f);
            _renameTextField.Width.Set(150, 0f);
            _confirmRenameButton = new TIGWEButton(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/Confirm", AssetRequestMode.ImmediateLoad));
            _confirmRenameButton.HoverText = LocalizationUtils.GetTextValue("UIElements.DirectoryItem.HoverText.ConfirmRename");
            _confirmRenameButton.OnLeftClick += (_, _) => ConfirmRename();

            // select
            _selectButton = new TIGWEButton(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/Confirm", AssetRequestMode.ImmediateLoad));
            _selectButton.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.UIElements.DirectoryItem.HoverText.Select");
            _selectButton.OnLeftClick += (_, _) => _parentGrid.OnSelectItem(this);
            Append(_selectButton);

            // icon
            _icon = new UIImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/File")); // just a placeholder texture, overridden in child classes
            _icon.IgnoresMouseInteraction = true;
            _icon.AllowResizingDimensions = false;
            Append(_icon);
        }

        public void AssignParentFolder(TIGWEDirectoryFolder folder)
        {
            _parentFolder = folder;
        }

        public void AssignParentGrid(TIGWEDirectoryGrid grid)
        {
            _parentGrid = grid;
        }

        public void StartRename()
        {
            if (!_isRenaming)
            {
                RemoveChild(_nameText);
                _renameTextField.Left.Set(_icon.Left.Pixels + _icon.Width.Pixels + 2, 0f);
                _renameTextField.Top.Set(6, 0f);
                _renameTextField.IsFocused = true;
                Append(_renameTextField);
                _confirmRenameButton.Left.Set(_renameTextField.Left.Pixels + _renameTextField.Width.Pixels + 2, 0f);
                _confirmRenameButton.Top.Set(6, 0f);
                Append(_confirmRenameButton);
                _isRenaming = true;
            } else
            {
                Append(_nameText);
                RemoveChild(_renameTextField);
                RemoveChild(_confirmRenameButton);
                _renameTextField.SetText("");
                _isRenaming = false;
            }
        }

        public void StartDelete()
        {
            try
            {
                if (!_isDeleting)
                {
                    _deleteButton.SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/DeleteConfirm"));
                    _deleteButton.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.UIElements.DirectoryItem.HoverText.ConfirmDelete");
                    _isDeleting = true;
                }
                else
                {
                    if (this is TIGWEDirectoryFile) // if its a file
                    {
                        File.Delete(FullPath);
                    }
                    else if (this is TIGWEDirectoryFolder folder) // if it's a directory
                    {
                        // remove all its children from the grid before deleting
                        folder.Close();
                        Directory.Delete(FullPath, true);
                    }

                    // remove from the grid
                    _parentFolder?.RemoveContentChild(this);
                    _parentGrid.Remove(this);
                }
            } 
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.Error(LocalizationUtils.GetTextValue("UIElements.DirectoryItem.Exceptions.DeleteFail"), ex);
            }
        }

        private void ConfirmRename()
        {
            // make sure we have an actual name
            if (!string.IsNullOrWhiteSpace(_renameTextField.GetText()))
            {
                string newPath = FullPath.Substring(0, FullPath.Length - Name.Length) + _renameTextField.GetText();
                try
                {
                    Directory.Move(FullPath, newPath); // rename the file
                    _nameText.SetText(_renameTextField.GetText()); // set new name visually
                    Name = _renameTextField.GetText();
                    FullPath = newPath;
                    if (this is TIGWEDirectoryFolder folder) // recalculate the paths of all the folders children so they are still shown in order
                    {
                        folder.RecalculateChildrenPaths();
                    }
                }
                catch (Exception ex)
                {
                    TerrariaInGameWorldEditor.Warn(LocalizationUtils.GetTextValue("UIElements.DirectoryItem.Exceptions.RenameFail"), ex);
                    return;
                }
            }

            // remove elements and stuff
            RemoveChild(_renameTextField);
            RemoveChild(_confirmRenameButton);
            _renameTextField.SetText("");
            _isRenaming = false;
            _parentGrid.UpdateOrder();
            Append(_nameText);
        }

        private void CancelDelete()
        {
            _deleteButton.SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/Delete"));
            _isDeleting = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // rename
            // stop renaming under these conditions
            if (Main.keyState.IsKeyDown(Keys.Escape) || Mouse.GetState().LeftButton == ButtonState.Pressed && !_isHoveringOverButtons && !_renameTextField.IsMouseHovering) // if we click if the textfield or whatever else just add it as is
            {
                RemoveChild(_confirmRenameButton);
                RemoveChild(_renameTextField);
                _renameTextField.SetText("");
                _isRenaming = false;
                Append(_nameText);
            }

            // delete
            if (Main.keyState.IsKeyDown(Keys.Escape) || Mouse.GetState().LeftButton == ButtonState.Pressed && !_deleteButton.IsMouseHovering)
            {
                _isDeleting = false;
                _deleteButton.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.UIElements.DirectoryItem.HoverText.Delete");
                _deleteButton.SetImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DirectoryGrid/Delete"));
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }

        public override void Recalculate()
        {
            if (Parent == null)
            {
                return;
            }

            // if we dont have a parent folder that means we are in the root folder and we have a directorygrid as a parent so we match that width
            Width.Set(_parentFolder == null || _parentGrid.IsSearching ? _parentGrid.Width.Pixels : _parentFolder.GetInnerDimensions().Width - 14, 0f);
            Height.Set(38, 0f);
            _texture.Width.Set(Width.Pixels, 0f);
            _texture.Height.Set(Height.Pixels, 0f);
            Left.Set(Parent.Parent.Width.Pixels - Width.Pixels, 0f);

            // delete, rename, select
            _deleteButton.Left.Set(_texture.Width.Pixels - _deleteButton.Width.Pixels - 6, 0f);
            _deleteButton.Top.Set(6, 0f);
            _renameButton.Left.Set(_deleteButton.Left.Pixels - _renameButton.Width.Pixels - 2, 0f);
            _renameButton.Top.Set(6, 0f);
            _selectButton.Left.Set(_renameButton.Left.Pixels - _renameButton.Width.Pixels - 2, 0f);
            _selectButton.Top.Set(6, 0f);

            // name
            _nameText.Left.Set(_icon.Width.Pixels + 20, 0f);

            base.Recalculate();
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            _texture.Texture = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/TextureHover");
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            _texture.Texture = ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Texture");
        }
    }
}
