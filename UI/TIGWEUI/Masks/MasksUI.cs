using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;
using TerrariaInGameWorldEditor.UI.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UI.UIElements.DropDown;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.Masks
{
    internal class MaskUI : TIGWEUI
    {
        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Title = "Masks";
            Height.Set(224, 0);
            Width.Set(424, 0);
            Left.Set(750, 0);
            Top.Set(150, 0);

            // what tiles to draw/paste
            TIGWEImageResizeable pasteTilesOptions = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"));
            pasteTilesOptions.Left.Set(6, 0);
            pasteTilesOptions.Top.Set(68, 0);
            pasteTilesOptions.Width.Set(180, 0);
            pasteTilesOptions.Height.Set(150, 0);
            Append(pasteTilesOptions);
            UIText modifyTilesText = new UIText("Draw/paste:");
            modifyTilesText.Top.Set(-22, 0);
            pasteTilesOptions.Append(modifyTilesText);

            // paste options tiles
            TIGWECheckBox pasteTilesCheckBox = new TIGWECheckBox(true);
            TIGWESettings.ShouldPasteTiles = pasteTilesCheckBox.IsChecked;
            pasteTilesCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWESettings.ShouldPasteTiles = isChecked;
            };
            pasteTilesCheckBox.Left.Set(6, 0);
            pasteTilesCheckBox.Top.Set(6, 0);
            pasteTilesOptions.Append(pasteTilesCheckBox);
            UIText drawTilesText = new UIText("Tiles");
            drawTilesText.Left.Set(pasteTilesCheckBox.Left.Pixels + pasteTilesCheckBox.Width.Pixels + 4, 0);
            drawTilesText.Top.Set(pasteTilesCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(drawTilesText);

            // paste options walls
            TIGWECheckBox pasteWallsCheckBox = new TIGWECheckBox(true);
            TIGWESettings.ShouldPasteWalls = pasteWallsCheckBox.IsChecked;
            pasteWallsCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWESettings.ShouldPasteWalls = isChecked;
            };
            pasteWallsCheckBox.Left.Set(6, 0);
            pasteWallsCheckBox.Top.Set(34, 0);
            pasteTilesOptions.Append(pasteWallsCheckBox);
            UIText drawWallsText = new UIText("Walls");
            drawWallsText.Left.Set(pasteWallsCheckBox.Left.Pixels + pasteWallsCheckBox.Width.Pixels + 4, 0);
            drawWallsText.Top.Set(pasteWallsCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(drawWallsText);

            // paste options liquid
            TIGWECheckBox pasteLiquidCheckBox = new TIGWECheckBox(true);
            TIGWESettings.ShouldPasteLiquid = pasteLiquidCheckBox.IsChecked;
            pasteLiquidCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWESettings.ShouldPasteLiquid = isChecked;
            };
            pasteLiquidCheckBox.Left.Set(6, 0);
            pasteLiquidCheckBox.Top.Set(62, 0);
            pasteTilesOptions.Append(pasteLiquidCheckBox);
            UIText pasteLiquidText = new UIText("Liquid");
            pasteLiquidText.Left.Set(pasteLiquidCheckBox.Left.Pixels + pasteLiquidCheckBox.Width.Pixels + 4, 0);
            pasteLiquidText.Top.Set(pasteLiquidCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(pasteLiquidText);

            // paste options wire
            TIGWECheckBox pasteWireCheckBox = new TIGWECheckBox(true);
            TIGWESettings.ShouldPasteWires = pasteWireCheckBox.IsChecked;
            pasteWireCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWESettings.ShouldPasteWires = isChecked;
            };
            pasteWireCheckBox.Left.Set(6, 0);
            pasteWireCheckBox.Top.Set(90, 0);
            pasteTilesOptions.Append(pasteWireCheckBox);
            UIText pasteWireText = new UIText("Wires/Actuators");
            pasteWireText.Left.Set(pasteWireCheckBox.Left.Pixels + pasteWireCheckBox.Width.Pixels + 4, 0);
            pasteWireText.Top.Set(pasteWireCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(pasteWireText);

            // paste empty tiles
            TIGWECheckBox pasteAirCheckBox = new TIGWECheckBox(true);
            TIGWESettings.ShouldPasteEmpty = pasteAirCheckBox.IsChecked;
            pasteAirCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWESettings.ShouldPasteEmpty = isChecked;
            };
            pasteAirCheckBox.Left.Set(6, 0);
            pasteAirCheckBox.Top.Set(118, 0);
            pasteTilesOptions.Append(pasteAirCheckBox);
            UIText pasteAirText = new UIText("Empty tiles");
            pasteAirText.Left.Set(pasteAirCheckBox.Left.Pixels + pasteAirCheckBox.Width.Pixels + 4, 0);
            pasteAirText.Top.Set(pasteAirCheckBox.Top.Pixels + 4, 0);
            pasteTilesOptions.Append(pasteAirText);


            // what tiles to draw/paste on
            TIGWEImageResizeable pasteOnTilesOptions = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"));
            pasteOnTilesOptions.Left.Set(pasteTilesOptions.Left.Pixels + pasteTilesOptions.Width.Pixels + 2, 0);
            pasteOnTilesOptions.Top.Set(pasteTilesOptions.Top.Pixels, 0);
            pasteOnTilesOptions.Width.Set(230, 0);
            pasteOnTilesOptions.Height.Set(150, 0);
            Append(pasteOnTilesOptions);
            UIText modifyTilesOnText = new UIText("Draw/paste on tiles with:");
            modifyTilesOnText.Top.Set(-22, 0);
            pasteOnTilesOptions.Append(modifyTilesOnText);

            // paste on tiles
            TIGWEDropDown pasteOnTilesDropDown = new TIGWEDropDown(["Yes", "Any", "No"]);
            TIGWESettings.ShouldPasteOnTiles = Mask.Any;
            pasteOnTilesDropDown.SetSelectedOption("Any");
            pasteOnTilesDropDown.ShowDropDownButton = true;
            pasteOnTilesDropDown.OnOptionChanged += (string option) =>
            {
                TIGWESettings.ShouldPasteOnTiles = (Mask)pasteOnTilesDropDown.SelectedOptionIndex;
            };
            pasteOnTilesDropDown.Height.Set(26, 0);
            pasteOnTilesDropDown.Width.Set(80, 0);
            pasteOnTilesDropDown.Top.Set(6, 0);
            pasteOnTilesDropDown.Left.Set(6, 0);
            pasteOnTilesOptions.Append(pasteOnTilesDropDown);
            UIText pasteOnTilesText = new UIText("Tiles");
            pasteOnTilesText.Left.Set(pasteOnTilesDropDown.Left.Pixels + pasteOnTilesDropDown.Width.Pixels + 4, 0);
            pasteOnTilesText.Top.Set(pasteOnTilesDropDown.Top.Pixels + 4, 0);
            pasteOnTilesOptions.Append(pasteOnTilesText);

            // paste on walls
            TIGWEDropDown pasteOnWallsDropDown = new TIGWEDropDown(["Yes", "Any", "No"]);
            TIGWESettings.ShouldPasteOnWalls = Mask.Any;
            pasteOnWallsDropDown.SetSelectedOption("Any");
            pasteOnWallsDropDown.ShowDropDownButton = true;
            pasteOnWallsDropDown.OnOptionChanged += (string option) =>
            {
                TIGWESettings.ShouldPasteOnWalls = (Mask)pasteOnWallsDropDown.SelectedOptionIndex;
            };
            pasteOnWallsDropDown.Height.Set(26, 0);
            pasteOnWallsDropDown.Width.Set(80, 0);
            pasteOnWallsDropDown.Top.Set(34, 0);
            pasteOnWallsDropDown.Left.Set(6, 0);
            pasteOnTilesOptions.Append(pasteOnWallsDropDown);
            UIText pasteOnWallsText = new UIText("Walls");
            pasteOnWallsText.Left.Set(pasteOnWallsDropDown.Left.Pixels + pasteOnWallsDropDown.Width.Pixels + 4, 0);
            pasteOnWallsText.Top.Set(pasteOnWallsDropDown.Top.Pixels + 4, 0);
            pasteOnTilesOptions.Append(pasteOnWallsText);

            // paste on liquid
            TIGWEDropDown pasteOnLiquidDropDown = new TIGWEDropDown(["Yes", "Any", "No"]);
            TIGWESettings.ShouldPasteOnLiquid = Mask.Any;
            pasteOnLiquidDropDown.SetSelectedOption("Any");
            pasteOnLiquidDropDown.ShowDropDownButton = true;
            pasteOnLiquidDropDown.OnOptionChanged += (string option) =>
            {
                TIGWESettings.ShouldPasteOnLiquid = (Mask)pasteOnLiquidDropDown.SelectedOptionIndex;
            };
            pasteOnLiquidDropDown.Height.Set(26, 0);
            pasteOnLiquidDropDown.Width.Set(80, 0);
            pasteOnLiquidDropDown.Top.Set(62, 0);
            pasteOnLiquidDropDown.Left.Set(6, 0);
            pasteOnTilesOptions.Append(pasteOnLiquidDropDown);
            UIText pasteOnLiquidText = new UIText("Liquid");
            pasteOnLiquidText.Left.Set(pasteOnLiquidDropDown.Left.Pixels + pasteOnLiquidDropDown.Width.Pixels + 4, 0);
            pasteOnLiquidText.Top.Set(pasteOnLiquidDropDown.Top.Pixels + 4, 0);
            pasteOnTilesOptions.Append(pasteOnLiquidText);

            // paste on wire
            TIGWEDropDown pasteOnWireDropDown = new TIGWEDropDown(["Yes", "Any", "No"]);
            TIGWESettings.ShouldPasteOnWires = Mask.Any;
            pasteOnWireDropDown.SetSelectedOption("Any");
            pasteOnWireDropDown.ShowDropDownButton = true;
            pasteOnWireDropDown.OnOptionChanged += (string option) =>
            {
                TIGWESettings.ShouldPasteOnWires = (Mask)pasteOnWireDropDown.SelectedOptionIndex;
            };
            pasteOnWireDropDown.Height.Set(26, 0);
            pasteOnWireDropDown.Width.Set(80, 0);
            pasteOnWireDropDown.Top.Set(90, 0);
            pasteOnWireDropDown.Left.Set(6, 0);
            pasteOnTilesOptions.Append(pasteOnWireDropDown);
            UIText pasteOnWireText = new UIText("Wires/Actuators");
            pasteOnWireText.Left.Set(pasteOnWireDropDown.Left.Pixels + pasteOnWireDropDown.Width.Pixels + 4, 0);
            pasteOnWireText.Top.Set(pasteOnWireDropDown.Top.Pixels + 4, 0);
            pasteOnTilesOptions.Append(pasteOnWireText);
        }
    }
}
