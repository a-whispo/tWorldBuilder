using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UI.UIElements.DirectoryGrid
{
    internal class UIDirectoryGrid : UIGrid
    {
        public delegate void SelectFolderEventHandler(UIDirectoryFolder folder);
        public event SelectFolderEventHandler OnSelectFolder;
        public delegate void SelectFileEventHandler(UIDirectoryFile file);
        public event SelectFileEventHandler OnSelectFile;
        public bool IsSearching { get; private set; } = false;
        public bool ShouldShowFolders { get; set; } = true;
        public bool ShouldShowFiles { get; set; } = true;
        public bool CanSelectFolders { get; set; } = true;
        public bool CanSelectFiles { get; set; } = true;
        public string DirectoryPath { get; private set; }
        
        // all the items that are shown during a search (will only be files)
        private HashSet<UIDirectoryItem> _allItems = new HashSet<UIDirectoryItem>();
        
        public UIDirectoryGrid()
        {
            ManualSortMethod = (List<UIElement> items) =>
            {
                // compare by path
                items.Sort(new Comparison<UIElement>((UIElement element1, UIElement element2) =>
                {
                    UIDirectoryItem item1 = (UIDirectoryItem)element1;
                    UIDirectoryItem item2 = (UIDirectoryItem)element2;

                    // comepare paths
                    return (item1.FullPath + (item1 is UIDirectoryFile ? "." : "\\")).CompareTo(item2.FullPath + (item2 is UIDirectoryFile ? "." : "\\"));
                }));
                RecalculateChildren();
            };
        }

        public override void Clear()
        {
            base.Clear();
            _allItems.Clear();
            DirectoryPath = null;
        }

        public override bool Remove(UIElement item)
        {
            if (base.Remove(item))
            {
                // remove from the list with all our items
                _allItems.Remove((UIDirectoryItem)item);
                ((UIDirectoryItem)item).AssignParentGrid(null);
                return true;
            } else
            {
                return false;
            }
        }

        public override void Add(UIElement item)
        {
            if (item is UIDirectoryItem dirItem && !_items.Contains(dirItem))
            {
                dirItem.AssignParentGrid(this);
                base.Add(dirItem);
                _allItems.Add(dirItem);
            }
        }

        public override void AddRange(IEnumerable<UIElement> items)
        {
            foreach (UIElement item in items)
            {
                Add(item);
            }
        }

        public void SetSearchBar(TIGWETextField searchBar)
        {
            searchBar.OnTextChanged += SearchFor;
        }

        public void RemoveSearchBar(TIGWETextField searchBar)
        {
            searchBar.OnTextChanged -= SearchFor;
        }

        public void OnSelectItem(UIDirectoryItem item)
        {
            if (item is UIDirectoryFolder folder)
            {
                OnSelectFolder?.Invoke(folder);
            }
            else if (item is UIDirectoryFile file)
            {
                OnSelectFile?.Invoke(file);
            }
        }

        public void SearchFor(string searchTerm)
        {
            if (searchTerm.Equals(""))
            {
                // clear all our current items so we can add the ones that should be visible
                base.Clear();

                // list of items to add
                HashSet<UIDirectoryItem> items = new HashSet<UIDirectoryItem>();

                // go over all our possible items
                foreach (UIDirectoryItem item in _allItems)
                {
                    // only add items that should be visible
                    if (!item.HasParentFolder)
                    {
                        items.Add(item);

                        // make sure to open folders that were open before searching
                        if (item is UIDirectoryFolder folder && folder.IsOpen)
                        {
                            folder.Open();
                        }
                    }
                }
                AddRange(items);
                IsSearching = false;
            } else
            {
                // list of matching items
                HashSet<UIDirectoryItem> matchingItems = new HashSet<UIDirectoryItem>();

                // go over all our possible items
                foreach (UIDirectoryItem item in _allItems)
                {
                    // check if the item matches the search term and if its a file (we dont want to display folders when searching)
                    if (item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) && item is UIDirectoryFile) // if it contains the seatch term add it to items that should show up
                    {
                        matchingItems.Add(item);
                    }
                }

                // clear and add matching items
                base.Clear();
                AddRange(matchingItems);
                IsSearching = true;
            }
        }

        public void SetDirectory(string path)
        {
            DirectoryPath = path;

            void AddItemsToGrid(string path, UIDirectoryFolder parent)
            {
                // add folders
                if (ShouldShowFolders)
                {
                    foreach (string dirPath in Directory.EnumerateDirectories(path))
                    {
                        UIDirectoryFolder folder = new UIDirectoryFolder(dirPath);
                        parent?.AddContentChild(folder);
                        if (parent == null)
                        {
                            Add(folder);
                        }
                        else
                        {
                            _allItems.Add(folder);
                        }

                        // add items in folder
                        AddItemsToGrid(dirPath, folder);
                    }
                }

                if (ShouldShowFiles)
                {
                    // add files
                    foreach (string filePath in Directory.EnumerateFiles(path, "*"))
                    {
                        UIDirectoryFile file = new UIDirectoryFile(filePath);
                        parent?.AddContentChild(file);
                        if (parent == null)
                        {
                            Add(file);
                        }
                        else
                        {
                            _allItems.Add(file);
                        }
                    }
                }
            }
            AddItemsToGrid(path, null);
            UpdateOrder();
        }

        public void RefreshContent()
        {
            // re add the current directory
            if (string.IsNullOrWhiteSpace(DirectoryPath))
            {
                return;
            }
            string dirPath = DirectoryPath;
            Clear();
            SetDirectory(dirPath);

            // update selectable states
            foreach (UIDirectoryItem item in _allItems)
            {
                item.CanSelect = (item is UIDirectoryFolder && CanSelectFolders) || (item is UIDirectoryFile && CanSelectFiles);
            }
        }
    }
}