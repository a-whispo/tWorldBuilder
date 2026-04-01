using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using TerrariaInGameWorldEditor.UIElements;
using TerrariaInGameWorldEditor.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UIElements.ColorPicker;
using TerrariaInGameWorldEditor.UIElements.DropDown;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.NumberField;
using TerrariaInGameWorldEditor.UIElements.Scrollbar;

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
            optionsBorder.Top.Set(42, 0);
            optionsBorder.Height.Set(320, 0);
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
            TIGWEScrollbar optionsScrollBar = new TIGWEScrollbar();
            optionsScrollBar.Left.Set(optionsBorder.Left.Pixels + optionsBorder.Width.Pixels + 2, 0);
            optionsScrollBar.Top.Set(optionsBorder.Top.Pixels, 0);
            optionsScrollBar.Height.Set(optionsBorder.Height.Pixels, 0);
            optionsScrollBar.Width.Set(20, 0);
            Append(optionsScrollBar);
            optionsGrid.SetScrollbar(optionsScrollBar);

            // this way of doing things is kind of awful

            // editor settings
            SettingsCategory editorSettings = new SettingsCategory("Editor settings");
            editorSettings.SetOptionsGrid(optionsGrid);
            SettingsGroup uiScaleOptions = new SettingsGroup();
            SettingsOption<TIGWECheckBox> shouldForceScale = new SettingsOption<TIGWECheckBox>("Force editor UI scale:", new TIGWECheckBox());
            SettingsOption<TIGWEDropDown<float>> forceScale = new SettingsOption<TIGWEDropDown<float>>("Scale:", new TIGWEDropDown<float>());
            uiScaleOptions.AddNode(shouldForceScale);
            uiScaleOptions.AddNode(forceScale);
            shouldForceScale.OptionElement.OnCheckedChanged += (check) =>
            {
                forceScale.Enabled = check;
                EditorSystem.Local.UseCustomScale = check;
                EditorSystem.Local.Settings.ForceScaleUI = check;
            };
            forceScale.OptionElement.OnOptionChanged += (option) =>
            {
                EditorSystem.Local.Scale = option.Value;
                EditorSystem.Local.Settings.UIScale = option.Value;
            };
            forceScale.Enabled = false;
            forceScale.OptionElement.AddOption(0.5f, "50%");
            forceScale.OptionElement.AddOption(1f, "100%");
            forceScale.OptionElement.AddOption(1.5f, "150%");
            forceScale.OptionElement.AddOption(2f, "200%");
            forceScale.OptionElement.Height.Set(26, 0);
            forceScale.OptionElement.Width.Set(150, 0);
            editorSettings.AddOption(uiScaleOptions);
            SettingsGroup themeOptions = new SettingsGroup();
            SettingsOption<TIGWEDropDown<Theme>> theme = new SettingsOption<TIGWEDropDown<Theme>>("Theme:", new TIGWEDropDown<Theme>());
            SettingsOption<TIGWEColorPicker> primaryColor = new SettingsOption<TIGWEColorPicker>("Primary color:", new TIGWEColorPicker());
            SettingsOption<TIGWEColorPicker> secondaryColor = new SettingsOption<TIGWEColorPicker>("Secondary color:", new TIGWEColorPicker());
            themeOptions.AddNode(theme);
            themeOptions.AddNode(primaryColor);
            themeOptions.AddNode(secondaryColor);
            theme.OptionElement.OnOptionChanged += (option) =>
            {
                primaryColor.Enabled = option.Value == Theme.Custom;
                secondaryColor.Enabled = option.Value == Theme.Custom;
                if (option.Value == Theme.Default)
                {
                    primaryColor.OptionElement.SetColor(new Color(11, 19, 66, 160));
                    secondaryColor.OptionElement.SetColor(new Color(62, 70, 113, 160));
                }
                EditorSystem.Local.Settings.CurrentTheme = option.Value;
            };
            primaryColor.OptionElement.OnColorChanged += (color) =>
            {
                UIElementUtils.PrimaryColor = color;
                EditorSystem.Local.Settings.PrimaryColor = color;
            };
            secondaryColor.OptionElement.OnColorChanged += (color) =>
            {
                UIElementUtils.SecondaryColor = color;
                EditorSystem.Local.Settings.SecondaryColor = color;
            };
            theme.OptionElement.AddOption(Theme.Default, "Default");
            theme.OptionElement.AddOption(Theme.Custom, "Custom");
            theme.OptionElement.Height.Set(26, 0);
            theme.OptionElement.Width.Set(150, 0);
            editorSettings.AddOption(themeOptions);
            settingCategories.Add(editorSettings);  
            SettingsOption<TIGWECheckBox> teleport = new SettingsOption<TIGWECheckBox>("Teleport to editor location when closing editor:", new TIGWECheckBox());
            editorSettings.AddOption(teleport);
            teleport.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldTeleportOnEditorClosed = check;
            };
            SettingsOption<TIGWECheckBox> fullbright = new SettingsOption<TIGWECheckBox>("Fullbright enabled:", new TIGWECheckBox());
            editorSettings.AddOption(fullbright);
            fullbright.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.FullbrightEnabled = check;
            };
            SettingsOption<TIGWENumberField> historyLimit = new SettingsOption<TIGWENumberField>("Max undo/redo actions to keep in memory:", new TIGWENumberField(1000, ushort.MaxValue, 0));
            editorSettings.AddOption(historyLimit);
            historyLimit.OptionElement.OnValueChanged += (newValue) =>
            {
                EditorSystem.Local.Settings.HistoryLimit = newValue;
            };
            historyLimit.OptionElement.Width.Set(100, 0);
            historyLimit.OptionElement.Height.Set(26, 0);

            // tool settings
            SettingsCategory toolSettings = new SettingsCategory("Tool settings");
            toolSettings.SetOptionsGrid(optionsGrid);
            SettingsOption<TIGWECheckBox> updateDraw = new SettingsOption<TIGWECheckBox>("Update drawn/pasted tiles:", new TIGWECheckBox());
            toolSettings.AddOption(updateDraw);
            updateDraw.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldUpdateDrawnTiles = check;
            };
            SettingsOption<TIGWEColorPicker> toolColor = new SettingsOption<TIGWEColorPicker>("Tool color:", new TIGWEColorPicker());
            toolSettings.AddOption(toolColor);
            toolColor.OptionElement.OnColorChanged += (color) =>
            {
                EditorSystem.Local.Settings.ToolColor = color;
            };
            SettingsOption<TIGWECheckBox> centerLines = new SettingsOption<TIGWECheckBox>("Add center lines to selection:", new TIGWECheckBox());
            toolSettings.AddOption(centerLines);
            centerLines.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShowCenterLines = check;
            };
            SettingsOption<TIGWECheckBox> measuringLines = new SettingsOption<TIGWECheckBox>("Show measuring lines:", new TIGWECheckBox());
            toolSettings.AddOption(measuringLines);
            measuringLines.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShowMeasureLines = check;
            };
            settingCategories.Add(toolSettings);

            // messages
            SettingsCategory messages = new SettingsCategory("Messages");
            messages.SetOptionsGrid(optionsGrid);
            SettingsOption<TIGWECheckBox> showMessages = new SettingsOption<TIGWECheckBox>("Should write editor messages in chat.", new TIGWECheckBox());
            messages.AddOption(showMessages);
            showMessages.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldShowMessages = check;
            };
            SettingsOption<TIGWECheckBox> showErrorMessages = new SettingsOption<TIGWECheckBox>("Should write warn messages in chat.", new TIGWECheckBox());
            messages.AddOption(showErrorMessages);
            showErrorMessages.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldShowErrors = check;
            };
            SettingsOption<TIGWECheckBox> showFatalErrorMessages = new SettingsOption<TIGWECheckBox>("Should write error messages in chat.", new TIGWECheckBox());
            messages.AddOption(showFatalErrorMessages);
            showFatalErrorMessages.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldShowFatalErrors = check;
            };
            settingCategories.Add(messages);

            // default to general
            editorSettings.SetSelected();
            _selectedCategory = editorSettings;

            // set everything to current settings
            updateDraw.OptionElement.IsChecked = EditorSystem.Local.Settings.ShouldUpdateDrawnTiles;
            teleport.OptionElement.IsChecked = EditorSystem.Local.Settings.ShouldTeleportOnEditorClosed;
            historyLimit.OptionElement.SetValue(EditorSystem.Local.Settings.HistoryLimit);
            showMessages.OptionElement.IsChecked = EditorSystem.Local.Settings.ShouldShowMessages;
            showErrorMessages.OptionElement.IsChecked = EditorSystem.Local.Settings.ShouldShowErrors;
            showFatalErrorMessages.OptionElement.IsChecked = EditorSystem.Local.Settings.ShouldShowFatalErrors;
            centerLines.OptionElement.IsChecked = EditorSystem.Local.Settings.ShowCenterLines;
            measuringLines.OptionElement.IsChecked = EditorSystem.Local.Settings.ShowMeasureLines;
            shouldForceScale.OptionElement.IsChecked = EditorSystem.Local.Settings.ForceScaleUI;
            forceScale.OptionElement.SetSelectedValue(EditorSystem.Local.Settings.UIScale);
            fullbright.OptionElement.IsChecked = EditorSystem.Local.Settings.FullbrightEnabled;
            toolColor.OptionElement.SetColorPremultipled(EditorSystem.Local.Settings.ToolColor);
            theme.OptionElement.SetSelectedValue(EditorSystem.Local.Settings.CurrentTheme);
            primaryColor.OptionElement.SetColorPremultipled(EditorSystem.Local.Settings.PrimaryColor);
            secondaryColor.OptionElement.SetColorPremultipled(EditorSystem.Local.Settings.SecondaryColor);
        }
    }
}
