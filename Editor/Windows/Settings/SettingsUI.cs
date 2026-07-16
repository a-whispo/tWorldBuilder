using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using TerrariaInGameWorldEditor.Common.Utils;
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
            SetTitle(LocalizationUtils.GetText("Windows.Settings.Title"));
            
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
            SettingsCategory editorSettings = new SettingsCategory(LocalizationUtils.GetText("Windows.Settings.Categories.EditorSettings"));
            editorSettings.SetOptionsGrid(optionsGrid);
            SettingsGroup uiScaleOptions = new SettingsGroup();
            SettingsOption<TIGWECheckBox> shouldForceScale = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.ForceScale"), new TIGWECheckBox());
            SettingsOption<TIGWEDropDown<float>> forceScale = new SettingsOption<TIGWEDropDown<float>>(LocalizationUtils.GetText("Windows.Settings.Settings.Scale"), new TIGWEDropDown<float>());
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
            SettingsOption<TIGWEDropDown<Theme>> theme = new SettingsOption<TIGWEDropDown<Theme>>(LocalizationUtils.GetText("Windows.Settings.Settings.Theme"), new TIGWEDropDown<Theme>());
            SettingsOption<TIGWEColorPicker> primaryColor = new SettingsOption<TIGWEColorPicker>(LocalizationUtils.GetText("Windows.Settings.Settings.PrimaryColor"), new TIGWEColorPicker());
            SettingsOption<TIGWEColorPicker> secondaryColor = new SettingsOption<TIGWEColorPicker>(LocalizationUtils.GetText("Windows.Settings.Settings.SecondaryColor"), new TIGWEColorPicker());
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
            theme.OptionElement.AddOption(Theme.Default, LocalizationUtils.GetTextValue("Windows.Settings.Options.Themes.Default"));
            theme.OptionElement.AddOption(Theme.Custom, LocalizationUtils.GetTextValue("Windows.Settings.Options.Themes.Custom"));
            theme.OptionElement.Height.Set(26, 0);
            theme.OptionElement.Width.Set(150, 0);
            editorSettings.AddOption(themeOptions);
            SettingsOption<TIGWECheckBox> godMode = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.GodMode"), new TIGWECheckBox());
            editorSettings.AddOption(godMode);
            godMode.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldEnableGodMode = check;
            };
            SettingsOption<TIGWECheckBox> teleport = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.TeleportToEditor"), new TIGWECheckBox());
            editorSettings.AddOption(teleport);
            teleport.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldTeleportOnEditorClosed = check;
            };
            SettingsOption<TIGWECheckBox> fullbright = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.Fullbright"), new TIGWECheckBox());
            editorSettings.AddOption(fullbright);
            fullbright.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.FullbrightEnabled = check;
            };
            SettingsOption<TIGWENumberField> editorBaseSpeed = new SettingsOption<TIGWENumberField>(LocalizationUtils.GetText("Windows.Settings.Settings.EditorBaseSpeed"), new TIGWENumberField(10, 100, 0));
            editorSettings.AddOption(editorBaseSpeed);
            editorBaseSpeed.OptionElement.OnValueChanged += (newValue) =>
            {
                EditorSystem.Local.Settings.EditorBaseSpeed = newValue;
            };
            editorBaseSpeed.OptionElement.Width.Set(100, 0);
            editorBaseSpeed.OptionElement.Height.Set(26, 0);
            SettingsOption<TIGWECheckBox> selectionActiveText = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.SelectionActiveText"), new TIGWECheckBox());
            editorSettings.AddOption(selectionActiveText);
            selectionActiveText.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldShowActiveSelectionText = check;
            };
            SettingsOption<TIGWENumberField> historyLimit = new SettingsOption<TIGWENumberField>(LocalizationUtils.GetText("Windows.Settings.Settings.MaxUndoRedo"), new TIGWENumberField(1000, ushort.MaxValue, 0));
            editorSettings.AddOption(historyLimit);
            historyLimit.OptionElement.OnValueChanged += (newValue) =>
            {
                EditorSystem.Local.Settings.HistoryLimit = newValue;
            };
            historyLimit.OptionElement.Width.Set(100, 0);
            historyLimit.OptionElement.Height.Set(26, 0);
            settingCategories.Add(editorSettings);

            // tool settings
            SettingsCategory toolSettings = new SettingsCategory(LocalizationUtils.GetText("Windows.Settings.Categories.ToolSettings"));
            toolSettings.SetOptionsGrid(optionsGrid);
            SettingsOption<TIGWECheckBox> updateDraw = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.UpdateTiles"), new TIGWECheckBox());
            toolSettings.AddOption(updateDraw);
            updateDraw.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldUpdateDrawnTiles = check;
            };
            SettingsOption<TIGWEDropDown<ToolInputMode>> inputMode = new SettingsOption<TIGWEDropDown<ToolInputMode>>(LocalizationUtils.GetText("Windows.Settings.Settings.InputMode"), new TIGWEDropDown<ToolInputMode>());
            toolSettings.AddOption(inputMode);
            inputMode.OptionElement.AddOption(ToolInputMode.Click, LocalizationUtils.GetTextValue("Windows.Settings.Options.InputModes.Click"));
            inputMode.OptionElement.AddOption(ToolInputMode.Drag, LocalizationUtils.GetTextValue("Windows.Settings.Options.InputModes.Drag"));
            inputMode.OptionElement.Height.Set(26, 0);
            inputMode.OptionElement.Width.Set(120, 0);
            inputMode.OptionElement.OnOptionChanged += (option) =>
            {
                EditorSystem.Local.Settings.InputMode = option.Value;
            };
            SettingsOption<TIGWEColorPicker> toolColor = new SettingsOption<TIGWEColorPicker>(LocalizationUtils.GetText("Windows.Settings.Settings.ToolColor"), new TIGWEColorPicker());
            toolSettings.AddOption(toolColor);
            toolColor.OptionElement.OnColorChanged += (color) =>
            {
                EditorSystem.Local.Settings.ToolColor = color;
            };
            SettingsOption<TIGWECheckBox> centerLines = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.CenterLines"), new TIGWECheckBox());
            toolSettings.AddOption(centerLines);
            centerLines.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShowCenterLines = check;
            };
            SettingsOption<TIGWECheckBox> measuringLines = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.MeasuringLines"), new TIGWECheckBox());
            toolSettings.AddOption(measuringLines);
            measuringLines.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShowMeasureLines = check;
            };
            settingCategories.Add(toolSettings);

            // messages
            SettingsCategory messages = new SettingsCategory(LocalizationUtils.GetText("Windows.Settings.Categories.Messages"));
            messages.SetOptionsGrid(optionsGrid);
            SettingsOption<TIGWECheckBox> showMessages = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.EditorMessages"), new TIGWECheckBox());
            messages.AddOption(showMessages);
            showMessages.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldShowMessages = check;
            };
            SettingsOption<TIGWECheckBox> showErrorMessages = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.WarnMessages"), new TIGWECheckBox());
            messages.AddOption(showErrorMessages);
            showErrorMessages.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldShowErrors = check;
            };
            SettingsOption<TIGWECheckBox> showFatalErrorMessages = new SettingsOption<TIGWECheckBox>(LocalizationUtils.GetText("Windows.Settings.Settings.ErrorMessages"), new TIGWECheckBox());
            messages.AddOption(showFatalErrorMessages);
            showFatalErrorMessages.OptionElement.OnCheckedChanged += (check) =>
            {
                EditorSystem.Local.Settings.ShouldShowFatalErrors = check;
            };
            settingCategories.Add(messages);

            // default to editor settings
            editorSettings.SetSelected();
            _selectedCategory = editorSettings;

            // set everything to current settings
            updateDraw.OptionElement.IsChecked = EditorSystem.Local.Settings.ShouldUpdateDrawnTiles;
            teleport.OptionElement.IsChecked = EditorSystem.Local.Settings.ShouldTeleportOnEditorClosed;
            selectionActiveText.OptionElement.IsChecked = EditorSystem.Local.Settings.ShouldShowActiveSelectionText;
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
            inputMode.OptionElement.SetSelectedValue(EditorSystem.Local.Settings.InputMode);
            godMode.OptionElement.IsChecked = EditorSystem.Local.Settings.ShouldEnableGodMode;
            editorBaseSpeed.OptionElement.SetValue(EditorSystem.Local.Settings.EditorBaseSpeed);
        }
    }
}
