using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.UI.Editor;
using TerrariaInGameWorldEditor.UI.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UI.UIElements.ColorPicker;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI.Settings
{
    internal class SettingsUI : TIGWEUI
    {
        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Width.Set(700, 0);
            Height.Set(432, 0);

            // settings text
            UIText optionsText = new UIText("Settings");
            optionsText.Left.Set(15, 0);
            optionsText.Top.Set(15, 0);
            optionsText.IgnoresMouseInteraction = true;
            Append(optionsText);

            // color picker
            UIText colorText = new UIText("Tool color: ");
            colorText.Left.Set(15, 0);
            colorText.Top.Set(52, 0);
            Append(colorText);
            TIGWEColorPicker toolColorPicker = new TIGWEColorPicker();
            TIGWEUISystem.Settings.ToolColor = toolColorPicker.GetColor();
            toolColorPicker.OnColorChanged += (color) =>
            {
                TIGWEUISystem.Settings.ToolColor = color;
            };
            toolColorPicker.Top.Set(72, 0);
            toolColorPicker.Left.Set(12, 0);
            Append(toolColorPicker);

            // delete/cut/copy options
            UIText wallsAndTilesText = new UIText("Only paste/delete/cut/draw:");
            wallsAndTilesText.Left.Set(368, 0);
            wallsAndTilesText.Top.Set(52, 0);
            Append(wallsAndTilesText);
            TIGWEImageResizeable pasteOptions = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"));
            pasteOptions.Left.Set(368, 0);
            pasteOptions.Top.Set(72, 0);
            pasteOptions.Width.Set(320, 0);
            pasteOptions.Height.Set(120, 0);
            Append(pasteOptions);

            // paste options tiles
            UIText pasteTilesText = new UIText("Tiles");
            pasteTilesText.Left.Set(40, 0);
            pasteTilesText.Top.Set(15, 0);
            pasteOptions.Append(pasteTilesText);
            TIGWECheckBox pasteTilesCheckBox = new TIGWECheckBox(true);
            TIGWEUISystem.Settings.ShouldPasteTiles = pasteTilesCheckBox.IsChecked;
            pasteTilesCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldPasteTiles = isChecked;
            };
            pasteTilesCheckBox.Left.Set(10, 0);
            pasteTilesCheckBox.Top.Set(11, 0);
            pasteOptions.Append(pasteTilesCheckBox);

            // paste options walls
            UIText pasteWallsText = new UIText("Walls");
            pasteWallsText.Left.Set(40, 0);
            pasteWallsText.Top.Set(51, 0);
            pasteOptions.Append(pasteWallsText);
            TIGWECheckBox pasteWallsCheckBox = new TIGWECheckBox(true);
            TIGWEUISystem.Settings.ShouldPasteWalls = pasteWallsCheckBox.IsChecked;
            pasteWallsCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldPasteWalls = isChecked;
            };
            pasteWallsCheckBox.Left.Set(10, 0);
            pasteWallsCheckBox.Top.Set(47, 0);
            pasteOptions.Append(pasteWallsCheckBox);

            // paste options liquid
            UIText pasteLiquidText = new UIText("Liquid");
            pasteLiquidText.Left.Set(40, 0);
            pasteLiquidText.Top.Set(87, 0);
            pasteOptions.Append(pasteLiquidText);
            TIGWECheckBox pasteLiquidCheckBox = new TIGWECheckBox(true);
            TIGWEUISystem.Settings.ShouldPasteLiquid = pasteLiquidCheckBox.IsChecked;
            pasteLiquidCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldPasteLiquid = isChecked;
            };
            pasteLiquidCheckBox.Left.Set(10, 0);
            pasteLiquidCheckBox.Top.Set(83, 0);
            pasteOptions.Append(pasteLiquidCheckBox);

            // paste options wire
            UIText pasteWireText = new UIText("Wires/Actuators");
            pasteWireText.Left.Set(140, 0);
            pasteWireText.Top.Set(15, 0);
            pasteOptions.Append(pasteWireText);
            TIGWECheckBox pasteWireCheckBox = new TIGWECheckBox(true);
            TIGWEUISystem.Settings.ShouldPasteWires = pasteWireCheckBox.IsChecked;
            pasteWireCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldPasteWires = isChecked;
            };
            pasteWireCheckBox.Left.Set(110, 0);
            pasteWireCheckBox.Top.Set(11, 0);
            pasteOptions.Append(pasteWireCheckBox);

            // other options
            UIText otherOptionsText = new UIText("Other options:");
            otherOptionsText.Left.Set(15, 0);
            otherOptionsText.Top.Set(214, 0);
            Append(otherOptionsText);
            TIGWEImageResizeable otherOptions = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"));
            otherOptions.Left.Set(12, 0);
            otherOptions.Top.Set(234, 0);
            otherOptions.Width.Set(350, 0);
            otherOptions.Height.Set(186, 0);
            Append(otherOptions);

            // center lines
            UIText linesText = new UIText("Add center lines to selection?");
            linesText.Left.Set(40, 0);
            linesText.Top.Set(15, 0);
            otherOptions.Append(linesText);
            TIGWECheckBox linesCheckBox = new TIGWECheckBox();
            TIGWEUISystem.Settings.ShowCenterLines = linesCheckBox.IsChecked;
            linesCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShowCenterLines = isChecked;
            };
            linesCheckBox.Left.Set(10, 0);
            linesCheckBox.Top.Set(10, 0);
            otherOptions.Append(linesCheckBox);

            // measuring lines
            UIText measureText = new UIText("Show measuring lines?");
            measureText.Left.Set(40, 0);
            measureText.Top.Set(50, 0);
            otherOptions.Append(measureText);
            TIGWECheckBox measureCheckBox = new TIGWECheckBox();
            TIGWEUISystem.Settings.ShowMeasureLines = measureCheckBox.IsChecked;
            measureCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShowMeasureLines = isChecked;
            };
            measureCheckBox.Left.Set(10, 0);
            measureCheckBox.Top.Set(45, 0);
            otherOptions.Append(measureCheckBox);

            // only paste on air option
            UIText pasteOnAirText = new UIText("Only paste/draw on fully empty tiles?");
            pasteOnAirText.Left.Set(40, 0);
            pasteOnAirText.Top.Set(85, 0);
            otherOptions.Append(pasteOnAirText);
            TIGWECheckBox pasteOnAirCheckBox = new TIGWECheckBox();
            TIGWEUISystem.Settings.ShouldOnlyPasteOnAir = pasteOnAirCheckBox.IsChecked;
            pasteOnAirCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldOnlyPasteOnAir = isChecked;
            };
            pasteOnAirCheckBox.Left.Set(10, 0);
            pasteOnAirCheckBox.Top.Set(80, 0);
            otherOptions.Append(pasteOnAirCheckBox);

            // paste air option
            UIText pasteAirText = new UIText("Paste air/empty walls?");
            pasteAirText.Left.Set(40, 0);
            pasteAirText.Top.Set(120, 0);
            otherOptions.Append(pasteAirText);
            TIGWECheckBox pasteAirCheckBox = new TIGWECheckBox(true);
            TIGWEUISystem.Settings.ShouldPasteAir = pasteAirCheckBox.IsChecked;
            pasteAirCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldPasteAir = isChecked;
            };
            pasteAirCheckBox.Left.Set(10, 0);
            pasteAirCheckBox.Top.Set(115, 0);
            otherOptions.Append(pasteAirCheckBox);

            // update draw
            UIText updateDrawnTilesText = new UIText("Update drawn tiles texture?");
            updateDrawnTilesText.Left.Set(40, 0);
            updateDrawnTilesText.Top.Set(155, 0);
            otherOptions.Append(updateDrawnTilesText);
            TIGWECheckBox updateDrawnTilesCheckBox = new TIGWECheckBox();
            TIGWEUISystem.Settings.ShouldUpdateDrawnTiles = updateDrawnTilesCheckBox.IsChecked;
            updateDrawnTilesCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldUpdateDrawnTiles = isChecked;
            };
            updateDrawnTilesCheckBox.HoverText = "[c/EAD87A:Note:] Also updates tiles around where you draw.\nTiles larger than 1x1 often break from this, be careful.";
            updateDrawnTilesCheckBox.Left.Set(10, 0);
            updateDrawnTilesCheckBox.Top.Set(150, 0);
            otherOptions.Append(updateDrawnTilesCheckBox);

            // paste on options
            UIText pasteOnText = new UIText("Only paste/draw on:");
            pasteOnText.Left.Set(2, 0);
            pasteOnText.Top.Set(-20, 0);
            TIGWEImageResizeable pasteOnOptions = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Texture"));
            pasteOnOptions.Left.Set(368, 0);
            pasteOnOptions.Top.Set(234, 0);
            pasteOnOptions.Width.Set(320, 0);
            pasteOnOptions.Height.Set(186, 0);
            Append(pasteOnOptions);
            pasteOnOptions.Append(pasteOnText);

            // paste on tiles
            UIText onTilesText = new UIText("Tiles");
            onTilesText.Left.Set(40, 0);
            onTilesText.Top.Set(15, 0);
            pasteOnOptions.Append(onTilesText);
            TIGWECheckBox onTilesCheckBox = new TIGWECheckBox(true);
            TIGWEUISystem.Settings.ShouldPasteOnTiles = onTilesCheckBox.IsChecked;
            onTilesCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldPasteOnTiles = isChecked;
            };
            onTilesCheckBox.Left.Set(10, 0);
            onTilesCheckBox.Top.Set(10, 0);
            pasteOnOptions.Append(onTilesCheckBox);

            // paste on walls
            UIText onWallsText = new UIText("Walls");
            onWallsText.Left.Set(40, 0);
            onWallsText.Top.Set(50, 0);
            pasteOnOptions.Append(onWallsText);
            TIGWECheckBox onWallsCheckBox = new TIGWECheckBox(true);
            TIGWEUISystem.Settings.ShouldPasteOnWalls = onWallsCheckBox.IsChecked;
            onWallsCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldPasteOnWalls = isChecked;
            };
            onWallsCheckBox.Left.Set(10, 0);
            onWallsCheckBox.Top.Set(46, 0);
            pasteOnOptions.Append(onWallsCheckBox);

            // paste on liquid
            UIText onLiquidText = new UIText("Liquid");
            onLiquidText.Left.Set(40, 0);
            onLiquidText.Top.Set(86, 0);
            pasteOnOptions.Append(onLiquidText);
            TIGWECheckBox onLiquidCheckBox = new TIGWECheckBox(true);
            TIGWEUISystem.Settings.ShouldPasteOnLiquid = onLiquidCheckBox.IsChecked;
            onLiquidCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldPasteOnLiquid = isChecked;
            };
            onLiquidCheckBox.Left.Set(10, 0);
            onLiquidCheckBox.Top.Set(82, 0);
            pasteOnOptions.Append(onLiquidCheckBox);

            // paste on air
            UIText onAirText = new UIText("Air");
            onAirText.Left.Set(40, 0);
            onAirText.Top.Set(122, 0);
            pasteOnOptions.Append(onAirText);
            TIGWECheckBox OnAirCheckBox = new TIGWECheckBox(true);
            TIGWEUISystem.Settings.ShouldPasteOnAir = OnAirCheckBox.IsChecked;
            OnAirCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWEUISystem.Settings.ShouldPasteOnAir = isChecked;
            };
            OnAirCheckBox.Left.Set(10, 0);
            OnAirCheckBox.Top.Set(118, 0);
            pasteOnOptions.Append(OnAirCheckBox);
        }
    }
}
