using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.Button;

namespace TerrariaInGameWorldEditor.UI.UIElements.DirectoryGrid
{
    internal class UIDirectoryFolder : UIDirectoryItem
    {
        public bool IsOpen { get; private set; } = false;
        public HashSet<UIDirectoryItem> FolderContent { get; } = new HashSet<UIDirectoryItem>();

        private TIGWEButton _createSubFolderButton;

        public UIDirectoryFolder(string pathFromSaves) : base(pathFromSaves)
        {
            // icon
            _icon.SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/FolderClosed"));
            _icon.Top.Set(10, 0);
            _icon.Left.Set(10, 0);
            _icon.Width.Set(38, 0f);
            _icon.Height.Set(18, 0f);

            // create folder
            _createSubFolderButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/CreateFolder"));
            _createSubFolderButton.SetVisibility(0.8f, 1f);
            _createSubFolderButton.Width.Set(26, 0);
            _createSubFolderButton.Height.Set(26, 0);
            _createSubFolderButton.HoverText = "Create new folder";
            _createSubFolderButton.OnLeftClick += (_, _) => CreateDirectory();
            Append(_createSubFolderButton);
        }

        public void AddContentChild(UIDirectoryItem item)
        {
            FolderContent.Add(item);
            item.AssignParentFolder(this);
        }

        public void RemoveContentChild(UIDirectoryItem item)
        {
            FolderContent.Remove(item);
            item.AssignParentFolder(null);
        }

        private void CreateDirectory()
        {
            // make sure we dont try to create a file with the same name as another one
            int num = 1;
            while (Directory.Exists($"{FullPath}\\New Folder ({num})"))
            {
                num++;
            }

            // create the directory and UIDirectoryFolder
            Directory.CreateDirectory($"{FullPath}\\New Folder ({num})");
            UIDirectoryFolder folder = new UIDirectoryFolder($"{FullPath}\\New Folder ({num})");
            folder.CanSelect = _canSelect;
            folder.AssignParentGrid(_parentGrid);

            // add item and open to show the new folder
            AddContentChild(folder);
            Open();

            // goto that element in the grid so we can see what we're naming it
            _parentGrid.Goto((UIElement element) => {
                return ((UIDirectoryItem)element).FullPath.Equals(folder.FullPath);
            }, true);

            // initialize the renaming
            folder.StartRename();
        }

        public void RecalculateChildrenPaths()
        {
            // go through each item and replace the first part of the full path
            foreach (UIDirectoryItem item in FolderContent)
            {
                item.FullPath = FullPath + item.FullPath.Substring(item.FullPath.Length - 1 - item.Name.Length);
                if (item is UIDirectoryFolder folder)
                {
                    folder.RecalculateChildrenPaths();
                }
            }
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            if (!_isHoveringOverButtons && !_parentGrid.IsSearching)
            {
                IsOpen = !IsOpen; // swap between open and closed
                if (IsOpen)
                {
                    Open();
                }
                else
                {
                    Close();
                }
            }
        }

        public void Open()
        {
            if (_parentGrid == null || _parentGrid.IsSearching)
            {
                return;
            }
            foreach (UIDirectoryItem item in FolderContent)
            {
                // add the item
                _parentGrid.Add(item);
                item.Recalculate();

                // if the item is a folder and it was open before opening this folder we want to also open it again
                if (item is UIDirectoryFolder folder && folder.IsOpen)
                {
                    folder.Open();
                }
            }
            IsOpen = true;
            _icon.SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/FolderOpen"));
            _parentGrid.UpdateOrder();
        }

        public void Close()
        {
            if (_parentGrid == null || _parentGrid.IsSearching)
            {
                return;
            }
            foreach (UIDirectoryItem item in FolderContent) // iterate over all the children
            {
                if (item is UIDirectoryFolder folder) // check if current item is a folder, we want to remove all that folders children if it is
                {
                    folder.Close(); // call itself recursivly with the child folder to also remove all children of that folder from the parent grid
                }
                _parentGrid.RemoveVisually(item);
            }
            IsOpen = false;
            _icon.SetImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/FolderClosed"));
            _parentGrid.UpdateOrder();
        }

        public override void Recalculate()
        {
            base.Recalculate();

            _createSubFolderButton.Left.Set(_canSelect ? _selectButton.Left.Pixels - _selectButton.Width.Pixels - 2 : _renameButton.Left.Pixels - _renameButton.Width.Pixels - 2, 0f);
            _createSubFolderButton.Top.Set(6, 0f);
        }
    }
}