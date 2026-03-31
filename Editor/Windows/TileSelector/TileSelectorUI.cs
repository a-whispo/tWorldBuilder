using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.ButtonResizable;
using TerrariaInGameWorldEditor.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.Scrollbar;
using TerrariaInGameWorldEditor.UIElements.SearchGrid;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.Editor.Windows.TileSelector
{
    internal class TileSelectorUI : TIGWEUI
    {
        public event EventHandler<TileCopy> OnTileConfirmed;

        private TileCopy _currentTileCopy;
        private List<TileSelectorProperty> _properties;

        private TIGWESearchGrid propertiesGrid;

        public override void OnInitialize()
        {
            base.OnInitialize();
            Width.Set(700, 0);
            Height.Set(440, 0);
            _defaultTitle = "Tile Selector";

            UIText preview = new UIText("Preview:");
            preview.Top.Set(45, 0);
            preview.Left.Set(6, 0);
            Append(preview);
            TIGWEImageButtonResizeable confirmBtn = new TIGWEImageButtonResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));
            confirmBtn.TextureHover = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/TextureHover");
            confirmBtn.OnLeftClick += (_, _) =>
            {
                OnTileConfirmed?.Invoke(this, _currentTileCopy);
                Visible = false;
            };
            confirmBtn.Text = "Confirm";
            confirmBtn.Left.Set(6, 0);
            confirmBtn.Top.Set(64, 0);
            confirmBtn.Height.Set(26, 0);
            confirmBtn.Width.Set(86, 0);
            Append(confirmBtn);
            TIGWEImageResizeable previewBox = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"), 8, 16);
            previewBox.Width.Set(48, 0);
            previewBox.Height.Set(48, 0);
            previewBox.Top.Set(42, 0);
            previewBox.Left.Set(confirmBtn.Left.Pixels + confirmBtn.Width.Pixels + 2, 0);
            UIElement tileCopy = new UIElement();
            previewBox.Append(tileCopy);
            tileCopy.OnDraw += (_) =>
            {
                CalculatedStyle previewBoxDimensions = previewBox.GetDimensions();
                Rectangle dimensions = new Rectangle((int)(previewBoxDimensions.X + previewBoxDimensions.Width / 2 - 32 / 2), (int)(previewBoxDimensions.Y + previewBoxDimensions.Height / 2 - 32 / 2), 32, 32);
                DrawUtils.DrawTileCopyInUI(_currentTileCopy, dimensions);
            };
            Append(previewBox);
            TIGWEImageResizeable border = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"), 8, 16);
            border.Top.Set(confirmBtn.Top.Pixels + confirmBtn.Height.Pixels + 2, 0);
            border.Left.Set(6, 0);
            border.Height.Set(Height.Pixels - border.Top.Pixels - 6, 0);
            border.Width.Set(Width.Pixels - 12, 0);
            Append(border);

            // tile properties grid
            TIGWEImageResizeable propertiesGridBorder = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Border"), 6, 4);
            propertiesGridBorder.IgnoresMouseInteraction = true;
            propertiesGridBorder.Left.Set(6, 0);
            propertiesGridBorder.Top.Set(34, 0);
            propertiesGridBorder.Height.Set(300, 0);
            propertiesGridBorder.Width.Set(248, 0);
            border.Append(propertiesGridBorder);
            propertiesGrid = new TIGWESearchGrid((item, searchTerm) =>
            {
                return ((TileSelectorProperty)item).Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            });
            propertiesGrid.ManualSortMethod = (elements) =>
            {
                // sorting by the element type make it easier to find what you want in the grid
                // most of the bool properties (checkboxes) are not that important tho so just put those last
                elements.Sort((item1, item2) =>
                {
                    UIElement item1Element = ((TileSelectorProperty)item1).PropertyUIElement;
                    UIElement item2Element = ((TileSelectorProperty)item2).PropertyUIElement;
                    int checkBoxComparison = (item1Element is TIGWECheckBox).CompareTo(item2Element is TIGWECheckBox);
                    if (checkBoxComparison != 0)
                    {
                        return checkBoxComparison;
                    }
                    return string.Compare(((TileSelectorProperty)item1).PropertyUIElement.GetType().Name, ((TileSelectorProperty)item2).PropertyUIElement.GetType().Name);
                });
            };
            propertiesGrid.ListPadding = 2;
            propertiesGrid.MarginLeft = 2;
            propertiesGrid.Left.Set(propertiesGridBorder.Left.Pixels + 6, 0);
            propertiesGrid.Top.Set(propertiesGridBorder.Top.Pixels + 6, 0);
            propertiesGrid.Width.Set(propertiesGridBorder.Width.Pixels - 12, 0);
            propertiesGrid.Height.Set(propertiesGridBorder.Height.Pixels - 12, 0);
            border.Append(propertiesGrid);
            TIGWETextField tilePropertiesSearchBar = new TIGWETextField($"Search for properties...", 100);
            tilePropertiesSearchBar.ShowSearchIcon = true;
            tilePropertiesSearchBar.Width.Set(propertiesGridBorder.Width.Pixels - 28, 0);
            tilePropertiesSearchBar.Height.Set(26, 0);
            tilePropertiesSearchBar.Top.Set(propertiesGridBorder.Top.Pixels - tilePropertiesSearchBar.Height.Pixels - 2, 0);
            tilePropertiesSearchBar.Left.Set(propertiesGridBorder.Left.Pixels, 0);
            border.Append(tilePropertiesSearchBar);
            TIGWEScrollbar propertiesScrollbar = new TIGWEScrollbar();
            propertiesScrollbar.Left.Set(propertiesGridBorder.Left.Pixels + propertiesGridBorder.Width.Pixels + 2, 0);
            propertiesScrollbar.Top.Set(propertiesGridBorder.Top.Pixels, 0);
            propertiesScrollbar.Height.Set(propertiesGridBorder.Height.Pixels, 0);
            propertiesScrollbar.Width.Set(20, 0);
            border.Append(propertiesScrollbar);
            TIGWEButton tilePropertiesInfoButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/InfoButton"));
            tilePropertiesInfoButton.HoverText = "[c/EAD87A:Note:] Modifying some values may \n cause problems. Be careful.";
            tilePropertiesInfoButton.SetVisibility(0.7f, 1);
            tilePropertiesInfoButton.SetHoverImage(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/InfoButtonHover"));
            tilePropertiesInfoButton.Width.Set(26, 0);
            tilePropertiesInfoButton.Height.Set(26, 0);
            tilePropertiesInfoButton.Top.Set(tilePropertiesSearchBar.Top.Pixels, 0);
            tilePropertiesInfoButton.Left.Set(tilePropertiesSearchBar.Left.Pixels + tilePropertiesSearchBar.Width.Pixels + 2, 0);
            border.Append(tilePropertiesInfoButton);
            propertiesGrid.SetScrollbar(propertiesScrollbar);
            propertiesGrid.SetSearchBar(tilePropertiesSearchBar);

            // select tile grid
            TIGWEImageResizeable tileGridBorder = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Border"), 6, 4);
            tileGridBorder.IgnoresMouseInteraction = true;
            tileGridBorder.Left.Set(propertiesScrollbar.Left.Pixels + propertiesScrollbar.Width.Pixels + 2, 0);
            tileGridBorder.Top.Set(34, 0);
            tileGridBorder.Height.Set(300, 0);
            tileGridBorder.Width.Set(border.Width.Pixels - tileGridBorder.Left.Pixels - 28, 0);
            border.Append(tileGridBorder);
            TIGWESearchGrid tileGrid = new TIGWESearchGrid((item, searchTerm) => 
            {
                return ((TileSelectorItem)item).Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            });
            tileGrid.ListPadding = 2;
            tileGrid.MarginLeft = 2;
            tileGrid.ManualSortMethod = (items) =>
            {
                // compare by id
                items.Sort((element1, element2) =>
                {
                    TileSelectorItem item1 = (TileSelectorItem)element1;
                    TileSelectorItem item2 = (TileSelectorItem)element2;
                    return item1.ItemId.CompareTo(item2.ItemId);
                });
            };
            tileGrid.Left.Set(tileGridBorder.Left.Pixels + 6, 0);
            tileGrid.Top.Set(tileGridBorder.Top.Pixels + 6, 0);
            tileGrid.Width.Set(tileGridBorder.Width.Pixels - 12, 0);
            tileGrid.Height.Set(tileGridBorder.Height.Pixels - 12, 0);
            border.Append(tileGrid);
            TIGWETextField tileSearchBar = new TIGWETextField($"Search for tiles...", 100);
            tileSearchBar.ShowSearchIcon = true;
            tileSearchBar.Width.Set(tileGridBorder.Width.Pixels - 28, 0);
            tileSearchBar.Height.Set(26, 0);
            tileSearchBar.Top.Set(tileGridBorder.Top.Pixels - tileSearchBar.Height.Pixels - 2, 0);
            tileSearchBar.Left.Set(tileGridBorder.Left.Pixels, 0);
            border.Append(tileSearchBar);
            TIGWEScrollbar tileScrollbar = new TIGWEScrollbar();
            tileScrollbar.Left.Set(tileGridBorder.Left.Pixels + tileGridBorder.Width.Pixels + 2, 0);
            tileScrollbar.Top.Set(tileGridBorder.Top.Pixels, 0);
            tileScrollbar.Height.Set(tileGridBorder.Height.Pixels, 0);
            tileScrollbar.Width.Set(20, 0);
            border.Append(tileScrollbar);
            TIGWEButton tileInfoButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/InfoButton"));
            tileInfoButton.HoverText = "[c/EAD87A:Note:] Will default to the top left part on \ntiles bigger than 1x1. Might not always work.";
            tileInfoButton.SetVisibility(0.7f, 1);
            tileInfoButton.SetHoverImage(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/InfoButtonHover"));
            tileInfoButton.Width.Set(26, 0);
            tileInfoButton.Height.Set(26, 0);
            tileInfoButton.Top.Set(tileSearchBar.Top.Pixels, 0);
            tileInfoButton.Left.Set(tileSearchBar.Left.Pixels + tileSearchBar.Width.Pixels + 2, 0);
            border.Append(tileInfoButton);
            tileGrid.SetScrollbar(tileScrollbar);
            tileGrid.SetSearchBar(tileSearchBar);

            // get all tiles from mods
            List<TileSelectorItem> items = new List<TileSelectorItem>();
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                Item item = ContentSamples.ItemsByType[i];
                if (item.createTile != -1 || item.createWall != -1)
                {
                    TileSelectorItem tileItem = new TileSelectorItem(i);
                    items.Add(tileItem);
                    tileItem.OnLeftClick += (_, _) =>
                    {
                        SetCurrentTileCopy(tileItem.GetAsTileCopy());
                    };
                    tileItem.OnRightClick += (_, _) =>
                    {
                        TileCopy tc = tileItem.GetAsTileCopy();
                        _currentTileCopy.TileType = tc.TileType;
                        _currentTileCopy.WallType = tc.WallType;
                        _currentTileCopy.HasTile = tc.HasTile;
                        _currentTileCopy.TileFrameX = tc.TileFrameX;
                        _currentTileCopy.TileFrameY = tc.TileFrameY;
                        _currentTileCopy.WallFrameX = tc.WallFrameX;
                        _currentTileCopy.WallFrameY = tc.WallFrameY;
                        SetCurrentTileCopy(_currentTileCopy);
                    };
                    tileItem.HoverText = $"{tileItem.Name}\n[c/EAD87A:Left click] to update and reset properties.\n[c/EAD87A:Right click] to only update tile/wall type.";
                }
            }
            tileGrid.AddRange(items);
            tileSearchBar.PlaceholderText = $"Search for tiles... [c/60ABE7:({tileGrid.AllItems.Count})]";

            // add tile properties
            _properties = new List<TileSelectorProperty>();
            foreach (PropertyInfo property in typeof(TileCopy).GetProperties())
            {
                // exclude things that are not in the actual tile class
                if (property.Name.Equals("HasWire") || property.Name.Equals("IsTreeTop") || property.Name.Equals("IsTreeBranch") || property.Name.Equals("IsTreeTrunk") || property.Name.Equals("IsFlipped") || property.Name.Equals("TreeVariant") || property.Name.Equals("TreeFrame") || property.Name.Equals("TreeFrameWidth") || property.Name.Equals("TreeFrameHeight") || property.Name.Equals("TreeStyle") || property.Name.Equals("y2") || property.Name.Equals("TreeBiome"))
                {
                    continue;
                }
                _properties.Add(new TileSelectorProperty(property, _currentTileCopy));
            }
            propertiesGrid.AddRange(_properties);
        }

        public void SetCurrentTileCopy(TileCopy tileCopy)
        {
            // make sure we dont copy by reference
            _currentTileCopy = new TileCopy(tileCopy.GetAsTile());
            foreach (TileSelectorProperty property in _properties)
            {
                property.UpdateProperty(_currentTileCopy);
            }
        }
    }
}
