using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UIElements.DropDown;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.Editor.Windows.Masks
{
    internal class MaskUI : TIGWEUI
    {
        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Height.Set(224, 0);
            Width.Set(424, 0);
            _defaultTitle = LocalizationUtils.GetTextValue("Windows.Masks.Title");

            // what tiles to draw/paste
            TIGWEImageResizeable pasteTilesOptions = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));
            pasteTilesOptions.Left.Set(6, 0);
            pasteTilesOptions.Top.Set(68, 0);
            pasteTilesOptions.Width.Set(180, 0);
            pasteTilesOptions.Height.Set(150, 0);
            Append(pasteTilesOptions);
            UIText modifyTilesText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.Sections.Modify"));
            modifyTilesText.Top.Set(-22, 0);
            pasteTilesOptions.Append(modifyTilesText);

            // paste options tiles
            TIGWECheckBox pasteTilesCheckBox = new TIGWECheckBox(true);
            EditorSystem.Local.Settings.ShouldPasteTiles = pasteTilesCheckBox.IsChecked;
            pasteTilesCheckBox.OnCheckedChanged += (isChecked) =>
            {
                EditorSystem.Local.Settings.ShouldPasteTiles = isChecked;
            };
            pasteTilesCheckBox.Left.Set(6, 0);
            pasteTilesCheckBox.Top.Set(6, 0);
            pasteTilesOptions.Append(pasteTilesCheckBox);
            UIText drawTilesText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.LabelText.Tiles"));
            drawTilesText.Left.Set(pasteTilesCheckBox.Left.Pixels + pasteTilesCheckBox.Width.Pixels + 4, 0);
            drawTilesText.Top.Set(pasteTilesCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(drawTilesText);

            // paste options walls
            TIGWECheckBox pasteWallsCheckBox = new TIGWECheckBox(true);
            EditorSystem.Local.Settings.ShouldPasteWalls = pasteWallsCheckBox.IsChecked;
            pasteWallsCheckBox.OnCheckedChanged += (isChecked) =>
            {
                EditorSystem.Local.Settings.ShouldPasteWalls = isChecked;
            };
            pasteWallsCheckBox.Left.Set(6, 0);
            pasteWallsCheckBox.Top.Set(34, 0);
            pasteTilesOptions.Append(pasteWallsCheckBox);
            UIText drawWallsText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.LabelText.Walls"));
            drawWallsText.Left.Set(pasteWallsCheckBox.Left.Pixels + pasteWallsCheckBox.Width.Pixels + 4, 0);
            drawWallsText.Top.Set(pasteWallsCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(drawWallsText);

            // paste options liquid
            TIGWECheckBox pasteLiquidCheckBox = new TIGWECheckBox(true);
            EditorSystem.Local.Settings.ShouldPasteLiquid = pasteLiquidCheckBox.IsChecked;
            pasteLiquidCheckBox.OnCheckedChanged += (isChecked) =>
            {
                EditorSystem.Local.Settings.ShouldPasteLiquid = isChecked;
            };
            pasteLiquidCheckBox.Left.Set(6, 0);
            pasteLiquidCheckBox.Top.Set(62, 0);
            pasteTilesOptions.Append(pasteLiquidCheckBox);
            UIText pasteLiquidText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.LabelText.Liquid"));
            pasteLiquidText.Left.Set(pasteLiquidCheckBox.Left.Pixels + pasteLiquidCheckBox.Width.Pixels + 4, 0);
            pasteLiquidText.Top.Set(pasteLiquidCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(pasteLiquidText);

            // paste options wire
            TIGWECheckBox pasteWireCheckBox = new TIGWECheckBox(true);
            EditorSystem.Local.Settings.ShouldPasteWires = pasteWireCheckBox.IsChecked;
            pasteWireCheckBox.OnCheckedChanged += (isChecked) =>
            {
                EditorSystem.Local.Settings.ShouldPasteWires = isChecked;
            };
            pasteWireCheckBox.Left.Set(6, 0);
            pasteWireCheckBox.Top.Set(90, 0);
            pasteTilesOptions.Append(pasteWireCheckBox);
            UIText pasteWireText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.LabelText.Wires"));
            pasteWireText.Left.Set(pasteWireCheckBox.Left.Pixels + pasteWireCheckBox.Width.Pixels + 4, 0);
            pasteWireText.Top.Set(pasteWireCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(pasteWireText);

            // paste empty tiles
            TIGWECheckBox pasteAirCheckBox = new TIGWECheckBox(true);
            EditorSystem.Local.Settings.ShouldPasteEmpty = pasteAirCheckBox.IsChecked;
            pasteAirCheckBox.OnCheckedChanged += (isChecked) =>
            {
                EditorSystem.Local.Settings.ShouldPasteEmpty = isChecked;
            };
            pasteAirCheckBox.Left.Set(6, 0);
            pasteAirCheckBox.Top.Set(118, 0);
            pasteTilesOptions.Append(pasteAirCheckBox);
            UIText pasteAirText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.LabelText.Empty"));
            pasteAirText.Left.Set(pasteAirCheckBox.Left.Pixels + pasteAirCheckBox.Width.Pixels + 4, 0);
            pasteAirText.Top.Set(pasteAirCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(pasteAirText);

            // reset button
            TIGWEButton pasteTilesReset = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/ResetMasks", AssetRequestMode.ImmediateLoad));
            pasteTilesReset.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.Masks.HoverText.Reset");
            pasteTilesReset.Left.Set(pasteTilesOptions.Width.Pixels - pasteTilesReset.Width.Pixels - 6, 0);
            pasteTilesReset.Top.Set(6, 0);
            pasteTilesReset.OnLeftClick += (_, _) =>
            {
                pasteTilesCheckBox.IsChecked = true;
                pasteWallsCheckBox.IsChecked = true;
                pasteLiquidCheckBox.IsChecked = true;
                pasteWireCheckBox.IsChecked = true;
                pasteAirCheckBox.IsChecked = true;
            };
            pasteTilesOptions.Append(pasteTilesReset);

            // what tiles to draw/paste on
            TIGWEImageResizeable pasteOnTilesOptions = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture", AssetRequestMode.ImmediateLoad));
            pasteOnTilesOptions.Left.Set(pasteTilesOptions.Left.Pixels + pasteTilesOptions.Width.Pixels + 2, 0);
            pasteOnTilesOptions.Top.Set(pasteTilesOptions.Top.Pixels, 0);
            pasteOnTilesOptions.Width.Set(230, 0);
            pasteOnTilesOptions.Height.Set(150, 0);
            Append(pasteOnTilesOptions);
            UIText modifyTilesOnText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.Sections.ModifyOn"));
            modifyTilesOnText.Top.Set(-22, 0);
            pasteOnTilesOptions.Append(modifyTilesOnText);

            string yes = LocalizationUtils.GetTextValue("Windows.Masks.Options.Yes");
            string any = LocalizationUtils.GetTextValue("Windows.Masks.Options.Any");
            string no = LocalizationUtils.GetTextValue("Windows.Masks.Options.No");

            // paste on tiles
            TIGWEDropDown<Mask> pasteOnTilesDropDown = new TIGWEDropDown<Mask>();
            pasteOnTilesDropDown.AddOption(Mask.Yes, yes);
            pasteOnTilesDropDown.AddOption(Mask.Any, any);
            pasteOnTilesDropDown.AddOption(Mask.No, no);
            pasteOnTilesDropDown.SetSelectedValue(Mask.Any);
            EditorSystem.Local.Settings.ShouldPasteOnTiles = Mask.Any;
            pasteOnTilesDropDown.OnOptionChanged += (option) =>
            {
                EditorSystem.Local.Settings.ShouldPasteOnTiles = option.Value;
            };
            pasteOnTilesDropDown.Height.Set(26, 0);
            pasteOnTilesDropDown.Width.Set(80, 0);
            pasteOnTilesDropDown.Top.Set(6, 0);
            pasteOnTilesDropDown.Left.Set(6, 0);
            pasteOnTilesOptions.Append(pasteOnTilesDropDown);
            UIText pasteOnTilesText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.LabelText.Tiles"));
            pasteOnTilesText.Left.Set(pasteOnTilesDropDown.Left.Pixels + pasteOnTilesDropDown.Width.Pixels + 4, 0);
            pasteOnTilesText.Top.Set(pasteOnTilesDropDown.Top.Pixels + 4, 0);
            pasteOnTilesOptions.Append(pasteOnTilesText);

            // paste on walls
            TIGWEDropDown<Mask> pasteOnWallsDropDown = new TIGWEDropDown<Mask>();
            pasteOnWallsDropDown.AddOption(Mask.Yes, yes);
            pasteOnWallsDropDown.AddOption(Mask.Any, any);
            pasteOnWallsDropDown.AddOption(Mask.No, no);
            pasteOnWallsDropDown.SetSelectedValue(Mask.Any);
            EditorSystem.Local.Settings.ShouldPasteOnWalls = Mask.Any;
            pasteOnWallsDropDown.OnOptionChanged += (option) =>
            {
                EditorSystem.Local.Settings.ShouldPasteOnWalls = option.Value;
            };
            pasteOnWallsDropDown.Height.Set(26, 0);
            pasteOnWallsDropDown.Width.Set(80, 0);
            pasteOnWallsDropDown.Top.Set(34, 0);
            pasteOnWallsDropDown.Left.Set(6, 0);
            pasteOnTilesOptions.Append(pasteOnWallsDropDown);
            UIText pasteOnWallsText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.LabelText.Walls"));
            pasteOnWallsText.Left.Set(pasteOnWallsDropDown.Left.Pixels + pasteOnWallsDropDown.Width.Pixels + 4, 0);
            pasteOnWallsText.Top.Set(pasteOnWallsDropDown.Top.Pixels + 4, 0);
            pasteOnTilesOptions.Append(pasteOnWallsText);

            // paste on liquid
            TIGWEDropDown<Mask> pasteOnLiquidDropDown = new TIGWEDropDown<Mask>();
            pasteOnLiquidDropDown.AddOption(Mask.Yes, yes);
            pasteOnLiquidDropDown.AddOption(Mask.Any, any);
            pasteOnLiquidDropDown.AddOption(Mask.No, no);
            pasteOnLiquidDropDown.SetSelectedValue(Mask.Any);
            EditorSystem.Local.Settings.ShouldPasteOnLiquid = Mask.Any;
            pasteOnLiquidDropDown.OnOptionChanged += (option) =>
            {
                EditorSystem.Local.Settings.ShouldPasteOnLiquid = option.Value;
            };
            pasteOnLiquidDropDown.Height.Set(26, 0);
            pasteOnLiquidDropDown.Width.Set(80, 0);
            pasteOnLiquidDropDown.Top.Set(62, 0);
            pasteOnLiquidDropDown.Left.Set(6, 0);
            pasteOnTilesOptions.Append(pasteOnLiquidDropDown);
            UIText pasteOnLiquidText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.LabelText.Liquid"));
            pasteOnLiquidText.Left.Set(pasteOnLiquidDropDown.Left.Pixels + pasteOnLiquidDropDown.Width.Pixels + 4, 0);
            pasteOnLiquidText.Top.Set(pasteOnLiquidDropDown.Top.Pixels + 4, 0);
            pasteOnTilesOptions.Append(pasteOnLiquidText);

            // paste on wire
            TIGWEDropDown<Mask> pasteOnWireDropDown = new TIGWEDropDown<Mask>();
            pasteOnWireDropDown.AddOption(Mask.Yes, yes);
            pasteOnWireDropDown.AddOption(Mask.Any, any);
            pasteOnWireDropDown.AddOption(Mask.No, no);
            pasteOnWireDropDown.SetSelectedValue(Mask.Any);
            EditorSystem.Local.Settings.ShouldPasteOnWires = Mask.Any;
            pasteOnWireDropDown.OnOptionChanged += (option) =>
            {
                EditorSystem.Local.Settings.ShouldPasteOnWires = option.Value;
            };
            pasteOnWireDropDown.Height.Set(26, 0);
            pasteOnWireDropDown.Width.Set(80, 0);
            pasteOnWireDropDown.Top.Set(90, 0);
            pasteOnWireDropDown.Left.Set(6, 0);
            pasteOnTilesOptions.Append(pasteOnWireDropDown);
            UIText pasteOnWireText = new UIText(LocalizationUtils.GetTextValue("Windows.Masks.LabelText.Wires"));
            pasteOnWireText.Left.Set(pasteOnWireDropDown.Left.Pixels + pasteOnWireDropDown.Width.Pixels + 4, 0);
            pasteOnWireText.Top.Set(pasteOnWireDropDown.Top.Pixels + 4, 0);
            pasteOnTilesOptions.Append(pasteOnWireText);

            // reset button
            TIGWEButton pasteOnTilesReset = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/ResetMasks"));
            pasteOnTilesReset.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Windows.Masks.HoverText.Reset");
            pasteOnTilesReset.Left.Set(pasteOnTilesOptions.Width.Pixels - pasteOnTilesReset.Width.Pixels - 6, 0);
            pasteOnTilesReset.Top.Set(6, 0);
            pasteOnTilesReset.OnLeftClick += (_, _) =>
            {
                pasteOnTilesDropDown.SetSelectedValue(Mask.Any);
                pasteOnWallsDropDown.SetSelectedValue(Mask.Any);
                pasteOnLiquidDropDown.SetSelectedValue(Mask.Any);
                pasteOnWireDropDown.SetSelectedValue(Mask.Any);
            };
            pasteOnTilesOptions.Append(pasteOnTilesReset);
        }
    }
}
