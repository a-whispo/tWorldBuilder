using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements;
using TerrariaInGameWorldEditor.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UIElements.ColorPicker;
using TerrariaInGameWorldEditor.UIElements.DropDown;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.Scrollbar;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.Editor.Windows.Settings
{
    internal class SettingsUI : TIGWEUI
    {
        private SettingsCategory _selectedCategory;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Width.Set(700, 0);
            Height.Set(368, 0);
            _defaultTitle = "Settings";

            // ui stuff
            
            // categories
            TIGWEImageResizeable categoriesBorder = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Border"), 6, 4);
            categoriesBorder.IgnoresMouseInteraction = true;
            categoriesBorder.Left.Set(6, 0);
            categoriesBorder.Top.Set(42, 0);
            categoriesBorder.Height.Set(320, 0);
            categoriesBorder.Width.Set(248, 0);
            Append(categoriesBorder);
            UIGrid settingCategories = new UIGrid();
            settingCategories.OnLeftClick += (_, element) =>
            {
                if (element.GetElementAt(new Vector2(Main.mouseX, Main.mouseY)) is SettingsCategory category)
                {
                    _selectedCategory?.SetNotSelected();
                    _selectedCategory = category;
                    _selectedCategory?.SetSelected();
                }
            };
            settingCategories.ListPadding = 2;
            settingCategories.MarginLeft = 2;
            settingCategories.Left.Set(categoriesBorder.Left.Pixels + 6, 0);
            settingCategories.Top.Set(categoriesBorder.Top.Pixels + 6, 0);
            settingCategories.Width.Set(categoriesBorder.Width.Pixels - 12, 0);
            settingCategories.Height.Set(categoriesBorder.Height.Pixels - 12, 0);
            Append(settingCategories);
            TIGWEScrollbar categoriesScrollBar = new TIGWEScrollbar();
            categoriesScrollBar.Left.Set(categoriesBorder.Left.Pixels + categoriesBorder.Width.Pixels + 2, 0);
            categoriesScrollBar.Top.Set(categoriesBorder.Top.Pixels, 0);
            categoriesScrollBar.Height.Set(categoriesBorder.Height.Pixels, 0);
            categoriesScrollBar.Width.Set(20, 0);
            Append(categoriesScrollBar);
            settingCategories.SetScrollbar(categoriesScrollBar);

            // options
            TIGWEImageResizeable optionsBorder = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Border"), 6, 4);
            optionsBorder.IgnoresMouseInteraction = true;
            optionsBorder.Left.Set(categoriesScrollBar.Left.Pixels + categoriesScrollBar.Width.Pixels + 2, 0);
            optionsBorder.Top.Set(70, 0);
            optionsBorder.Height.Set(292, 0);
            optionsBorder.Width.Set(Width.Pixels - optionsBorder.Left.Pixels - 28, 0);
            Append(optionsBorder);
            UIGrid optionsGrid = new UIGrid();
            optionsGrid.ListPadding = 2;
            optionsGrid.MarginLeft = 2;
            optionsGrid.Left.Set(optionsBorder.Left.Pixels + 6, 0);
            optionsGrid.Top.Set(optionsBorder.Top.Pixels + 6, 0);
            optionsGrid.Width.Set(optionsBorder.Width.Pixels - 12, 0);
            optionsGrid.Height.Set(optionsBorder.Height.Pixels - 12, 0);
            Append(optionsGrid);
            TIGWETextField optionsSearchBar = new TIGWETextField($"Search for options...", 100);
            optionsSearchBar.ShowSearchIcon = true;
            optionsSearchBar.Width.Set(optionsBorder.Width.Pixels, 0);
            optionsSearchBar.Height.Set(26, 0);
            optionsSearchBar.Top.Set(optionsBorder.Top.Pixels - optionsSearchBar.Height.Pixels - 2, 0);
            optionsSearchBar.Left.Set(optionsBorder.Left.Pixels, 0);
            Append(optionsSearchBar);
            TIGWEScrollbar optionsScrollBar = new TIGWEScrollbar();
            optionsScrollBar.Left.Set(optionsBorder.Left.Pixels + optionsBorder.Width.Pixels + 2, 0);
            optionsScrollBar.Top.Set(optionsBorder.Top.Pixels, 0);
            optionsScrollBar.Height.Set(optionsBorder.Height.Pixels, 0);
            optionsScrollBar.Width.Set(20, 0);
            Append(optionsScrollBar);
            optionsGrid.SetScrollbar(optionsScrollBar);

            // general
            SettingsCategory general = new SettingsCategory("General");
            general.SetOptionsGrid(optionsGrid);
            SettingsOption<TIGWECheckBox> updateDraw = new SettingsOption<TIGWECheckBox>("Update drawn/pasted tiles:", new TIGWECheckBox(TIGWESettings.ShouldUpdateDrawnTiles));
            updateDraw.OptionElement.OnCheckedChanged += (check) =>
            {
                TIGWESettings.ShouldUpdateDrawnTiles = check;
            };
            general.AddOption(updateDraw);
            SettingsOption<TIGWECheckBox> teleport = new SettingsOption<TIGWECheckBox>("Teleport to editor location when closing editor:", new TIGWECheckBox(TIGWESettings.ShouldTeleportOnEditorClosed));
            teleport.OptionElement.OnCheckedChanged += (check) =>
            {
                TIGWESettings.ShouldTeleportOnEditorClosed = check;
            };
            general.AddOption(teleport);
            settingCategories.Add(general);

            // visual
            SettingsCategory visual = new SettingsCategory("Visual");
            visual.SetOptionsGrid(optionsGrid);

            SettingsOption<TIGWECheckBox> centerLines = new SettingsOption<TIGWECheckBox>("Add center lines to selection:", new TIGWECheckBox(TIGWESettings.ShowCenterLines));
            centerLines.OptionElement.OnCheckedChanged += (check) =>
            {
                TIGWESettings.ShowCenterLines = check;
            };
            visual.AddOption(centerLines);

            SettingsOption<TIGWECheckBox> measuringLines = new SettingsOption<TIGWECheckBox>("Show measuring lines:", new TIGWECheckBox(TIGWESettings.ShowMeasureLines));
            measuringLines.OptionElement.OnCheckedChanged += (check) =>
            {
                TIGWESettings.ShowMeasureLines = check;
            };
            visual.AddOption(measuringLines);

            SettingsOption<TIGWECheckBox> fullbright = new SettingsOption<TIGWECheckBox>("Fullbright enabled:", new TIGWECheckBox(TIGWESettings.FullbrightEnabled));
            fullbright.OptionElement.OnCheckedChanged += (check) =>
            {
                TIGWESettings.FullbrightEnabled = check;
            };
            visual.AddOption(fullbright);

            SettingsOption<TIGWEColorPicker> toolColorPrimary = new SettingsOption<TIGWEColorPicker>("Tool color:", new TIGWEColorPicker());
            toolColorPrimary.OptionElement.SetColor(TIGWESettings.ToolColor);
            toolColorPrimary.OptionElement.OnColorChanged += (color) =>
            {
                TIGWESettings.ToolColor = color;
            };
            visual.AddOption(toolColorPrimary);

            SettingsGroup themeOptions = new SettingsGroup();
            SettingsOption<TIGWEDropDown<Theme>> theme = new SettingsOption<TIGWEDropDown<Theme>>("Theme:", new TIGWEDropDown<Theme>());
            theme.OptionElement.AddOption(Theme.Default, "Default");
            theme.OptionElement.AddOption(Theme.TexturePack, "Texture pack");
            theme.OptionElement.AddOption(Theme.Custom, "Custom");
            theme.OptionElement.SetSelectedValue(TIGWESettings.CurrentTheme);
            theme.OptionElement.Height.Set(26, 0);
            theme.OptionElement.Width.Set(150, 0);
            themeOptions.AddNode(theme);
            SettingsOption<TIGWEColorPicker> primaryColor = new SettingsOption<TIGWEColorPicker>("Primary color:", new TIGWEColorPicker());
            primaryColor.Enabled = false;
            themeOptions.AddNode(primaryColor);
            primaryColor.OptionElement.OnColorChanged += (color) =>
            {
                UIElementUtils.PrimaryColor = color;
                TIGWESettings.MainPrimaryColor = color;
            };
            primaryColor.OptionElement.SetColor(TIGWESettings.MainPrimaryColor);
            SettingsOption<TIGWEColorPicker> secondaryColor = new SettingsOption<TIGWEColorPicker>("Secondary color:", new TIGWEColorPicker());
            secondaryColor.Enabled = false;
            themeOptions.AddNode(secondaryColor);
            secondaryColor.OptionElement.OnColorChanged += (color) =>
            {
                UIElementUtils.SecondaryColor = color;
                TIGWESettings.MainSecondaryColor = color;
            };
            secondaryColor.OptionElement.SetColor(TIGWESettings.MainSecondaryColor);
            theme.OptionElement.OnOptionChanged += (selectedTheme) =>
            {
                primaryColor.Enabled = selectedTheme.Value == Theme.Custom;
                secondaryColor.Enabled = selectedTheme.Value == Theme.Custom;
                switch (selectedTheme.Value)
                {
                    case Theme.Default:
                        primaryColor.OptionElement.SetColor(new Color(43, 56, 101));
                        secondaryColor.OptionElement.SetColor(new Color(72, 92, 168));
                        break;
                    case Theme.TexturePack:
                        // fix logic
                        break;
                }
                TIGWESettings.CurrentTheme = selectedTheme.Value;
            };
            visual.AddOption(themeOptions);
            settingCategories.Add(visual);

            // default to general
            general.SetSelected();
            _selectedCategory = general;
        }
    }
}
