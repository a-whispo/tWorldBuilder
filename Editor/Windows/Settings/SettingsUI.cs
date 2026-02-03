using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UIElements.ColorPicker;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.Editor.Windows.Settings
{
    internal class SettingsUI : TIGWEUI
    {
        public override void OnInitialize()
        {
            base.OnInitialize();

            // main area
            Width.Set(700, 0);
            Height.Set(432, 0);
            _defaultTitle = "Settings";

            // color picker
            UIText colorText = new UIText("Tool color: ");
            colorText.Left.Set(15, 0);
            colorText.Top.Set(52, 0);
            Append(colorText);
            TIGWEColorPicker toolColorPicker = new TIGWEColorPicker();
            toolColorPicker.DrawScale = 1f;
            TIGWESettings.ToolColor = toolColorPicker.GetColor();
            toolColorPicker.OnColorChanged += (color) =>
            {
                TIGWESettings.ToolColor = color;
            };
            toolColorPicker.Top.Set(72, 0);
            toolColorPicker.Left.Set(12, 0);
            Append(toolColorPicker);

            // other options
            UIText otherOptionsText = new UIText("Other options:");
            otherOptionsText.Left.Set(15, 0);
            otherOptionsText.Top.Set(214, 0);
            Append(otherOptionsText);
            TIGWEImageResizeable otherOptions = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));
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
            TIGWESettings.ShowCenterLines = linesCheckBox.IsChecked;
            linesCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWESettings.ShowCenterLines = isChecked;
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
            TIGWESettings.ShowMeasureLines = measureCheckBox.IsChecked;
            measureCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWESettings.ShowMeasureLines = isChecked;
            };
            measureCheckBox.Left.Set(10, 0);
            measureCheckBox.Top.Set(45, 0);
            otherOptions.Append(measureCheckBox);

            // update draw
            UIText updateDrawnTilesText = new UIText("Update drawn/pasted tiles?");
            updateDrawnTilesText.Left.Set(40, 0);
            updateDrawnTilesText.Top.Set(155, 0);
            otherOptions.Append(updateDrawnTilesText);
            TIGWECheckBox updateDrawnTilesCheckBox = new TIGWECheckBox();
            TIGWESettings.ShouldUpdateDrawnTiles = updateDrawnTilesCheckBox.IsChecked;
            updateDrawnTilesCheckBox.OnCheckedChanged += (isChecked) =>
            {
                TIGWESettings.ShouldUpdateDrawnTiles = isChecked;
            };
            updateDrawnTilesCheckBox.HoverText = "[c/EAD87A:Note:] Also updates tiles around where you draw.\nTiles larger than 1x1 often break from this, be careful.";
            updateDrawnTilesCheckBox.Left.Set(10, 0);
            updateDrawnTilesCheckBox.Top.Set(150, 0);
            otherOptions.Append(updateDrawnTilesCheckBox);
        }
    }
}
