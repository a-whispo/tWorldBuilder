using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Reflection;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.UIElements.CheckBox;
using TerrariaInGameWorldEditor.UIElements.DropDown;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.NumberField;

namespace TerrariaInGameWorldEditor.Editor.Windows.TileSelector
{
    internal class TileSelectorProperty : UIElement
    {
        public string Name => _propertyText.Text;
        public PropertyInfo Property { get; private set; }
        public UIElement PropertyUIElement { get; private set; }
        public TileCopy Tile { get; private set; }

        private TIGWEImageResizeable _body;
        private int _textMarginTop = 10;
        private int _textMarginRight = 8;
        private UIText _propertyText;
        private Vector2 _propertyTextSize;
        private bool _shouldFit;

        // this is a bit messy
        public TileSelectorProperty(PropertyInfo property, TileCopy tile)
        {
            Property = property;
            Tile = tile;

            // property assignment
            switch (Property.Name)
            {
                // special cases
                case "LiquidType":
                    PropertyUIElement = new TIGWEDropDown<int>();
                    for (int i = 0; i < LiquidID.Search.Count; i++)
                    {
                        if (LiquidID.Search.TryGetName(i, out string name))
                        {
                            ((TIGWEDropDown<int>)PropertyUIElement).AddOption(i, $"[c/60ABE7:({i})] " + name);
                        }
                    }
                    break;

                case "WallType":
                    PropertyUIElement = new TIGWEDropDown<ushort>();
                    for (ushort i = 0; i < WallLoader.WallCount; i++)
                    {
                        if (WallID.Search.TryGetName(i, out string name))
                        {
                            ((TIGWEDropDown<ushort>)PropertyUIElement).AddOption(i, $"[c/60ABE7:({i})] " + name);
                        }
                    }
                    break;

                case "TileType":
                    PropertyUIElement = new TIGWEDropDown<ushort>();
                    for (ushort i = 0; i < TileLoader.TileCount; i++)
                    {
                        if (TileID.Search.TryGetName(i, out string name))
                        {
                            ((TIGWEDropDown<ushort>)PropertyUIElement).AddOption(i, $"[c/60ABE7:({i})] " + name);
                        }   
                    }
                    break;

                case "TileColor":
                case "WallColor":
                    PropertyUIElement = new TIGWEDropDown<byte>();
                    for (byte i = 0; i < byte.MaxValue; i++)
                    {
                        if (PaintID.Search.TryGetName(i, out string name))
                        {
                            ((TIGWEDropDown<byte>)PropertyUIElement).AddOption(i, $"[c/60ABE7:({i})] " + name);
                        }
                    }
                    break;

                case "Slope":
                    PropertyUIElement = new TIGWEDropDown<SlopeType>();
                    ((TIGWEDropDown<SlopeType>)PropertyUIElement).AddOption(SlopeType.Solid, $"[c/60ABE7:(0)] " + SlopeType.Solid.ToString());
                    ((TIGWEDropDown<SlopeType>)PropertyUIElement).AddOption(SlopeType.SlopeDownLeft, $"[c/60ABE7:(1)] " + SlopeType.SlopeDownLeft.ToString());
                    ((TIGWEDropDown<SlopeType>)PropertyUIElement).AddOption(SlopeType.SlopeDownRight, $"[c/60ABE7:(2)] " + SlopeType.SlopeDownRight.ToString());
                    ((TIGWEDropDown<SlopeType>)PropertyUIElement).AddOption(SlopeType.SlopeUpLeft, $"[c/60ABE7:(3)] " + SlopeType.SlopeUpLeft.ToString());
                    ((TIGWEDropDown<SlopeType>)PropertyUIElement).AddOption(SlopeType.SlopeUpRight, $"[c/60ABE7:(4)] " + SlopeType.SlopeUpRight.ToString());
                    break;

                // general cases
                default:
                    if (Property.PropertyType == typeof(bool))
                    {
                        PropertyUIElement = new TIGWECheckBox();
                    }
                    else if (Property.PropertyType == typeof(int))
                    {
                        PropertyUIElement = new TIGWENumberField(0, int.MaxValue, int.MinValue);
                    }
                    else if (Property.PropertyType == typeof(ushort))
                    {
                        PropertyUIElement = new TIGWENumberField(0, ushort.MaxValue, ushort.MinValue);
                    }
                    else if (Property.PropertyType == typeof(short))
                    {
                        PropertyUIElement = new TIGWENumberField(0, short.MaxValue, short.MinValue);
                    }
                    else if (Property.PropertyType == typeof(byte))
                    {
                        PropertyUIElement = new TIGWENumberField(0, byte.MaxValue, byte.MinValue);
                    }
                    break;
            }

            // property config
            // make it a bit easier to change frames
            switch (Property.Name)
            {
                case "TileFrameY":
                case "TileFrameX":
                    if (PropertyUIElement is TIGWENumberField tileFrameField)
                    {
                        tileFrameField.Step = 18;
                    }
                    break;

                case "WallFrameY":
                case "WallFrameX":
                    if (PropertyUIElement is TIGWENumberField wallFrameField)
                    {
                        wallFrameField.Step = 36;
                    }
                    break;
            }
            _shouldFit = PropertyUIElement is not TIGWECheckBox;

            // property events
            if (PropertyUIElement is TIGWENumberField numField)
            {
                numField.OnValueChanged += (val) => Property.SetValue(Tile, Convert.ChangeType(val, Property.PropertyType));
            }
            else if (PropertyUIElement is TIGWECheckBox checkBox)
            {
                checkBox.OnCheckedChanged += (val) => Property.SetValue(Tile, Convert.ChangeType(val, Property.PropertyType));
            }
            else if (PropertyUIElement is TIGWEDropDown<int> intDropDown)
            {
                intDropDown.OnOptionChanged += (val) => Property.SetValue(Tile, Convert.ChangeType(val.Value, Property.PropertyType));
            }
            else if (PropertyUIElement is TIGWEDropDown<ushort> ushortDropDown)
            {
                ushortDropDown.OnOptionChanged += (val) => Property.SetValue(Tile, Convert.ChangeType(val.Value, Property.PropertyType));
            }
            else if (PropertyUIElement is TIGWEDropDown<byte> byteDropDown)
            {
                byteDropDown.OnOptionChanged += (val) => Property.SetValue(Tile, Convert.ChangeType(val.Value, Property.PropertyType));
            }
            else if (PropertyUIElement is TIGWEDropDown<SlopeType> slopeTypeDropDown)
            {
                slopeTypeDropDown.OnOptionChanged += (val) => Property.SetValue(Tile, Convert.ChangeType(val.Value, Property.PropertyType));
            }

            // ui stuff
            Width.Set(-4, 1);
            Height.Set(38, 0);
            _body = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Texture"));
            _body.Width.Set(0, 1);
            _body.Height.Set(0, 1);
            Append(_body);
            DynamicSpriteFont spriteFont = FontAssets.MouseText.Value;
            _propertyTextSize = ChatManager.GetStringSize(spriteFont, $"{property.Name}:", new Vector2(1));
            _propertyText = new UIText($"{property.Name}:");
            _propertyText.Top.Set(_textMarginTop, 0);
            _propertyText.Left.Set(6, 0);
            _body.Append(_propertyText);
            _body.Append(PropertyUIElement);
            if (_shouldFit)
            {
                PropertyUIElement.Width.Set(_body.GetDimensions().Width - _propertyTextSize.X - _textMarginRight - _propertyText.Left.Pixels - 4, 0);
                PropertyUIElement.Height.Set(26, 0);
            }
            PropertyUIElement.Left.Set(_body.GetDimensions().Width - PropertyUIElement.Width.Pixels - 6, 0);
            PropertyUIElement.Top.Set(6, 0);
        }

        public void UpdateProperty(TileCopy tile)
        {
            Tile = tile;
            if (PropertyUIElement is TIGWENumberField numField)
            {
                numField.SetValue(Convert.ToInt32(Property.GetValue(Tile)));
            }
            else if (PropertyUIElement is TIGWECheckBox checkBox)
            {
                checkBox.IsChecked = (bool)Property.GetValue(Tile);
            }
            else if (PropertyUIElement is TIGWEDropDown<int> intDropDown)
            {
                intDropDown.SetSelectedValue((int)Property.GetValue(Tile));
            }
            else if (PropertyUIElement is TIGWEDropDown<ushort> ushortDropDown)
            {
                ushortDropDown.SetSelectedValue((ushort)Property.GetValue(Tile));
            }
            else if (PropertyUIElement is TIGWEDropDown<byte> byteDropDown)
            {
                byteDropDown.SetSelectedValue((byte)Property.GetValue(Tile));
            }
            else if (PropertyUIElement is TIGWEDropDown<SlopeType> slopeTypeDropDown)
            {
                slopeTypeDropDown.SetSelectedValue((SlopeType)Property.GetValue(Tile));
            }
        }

        public override void Recalculate()
        {
            base.Recalculate();
            if (_shouldFit)
            {
                PropertyUIElement.Width.Set(_body.GetDimensions().Width - _propertyTextSize.X - _textMarginRight - _propertyText.Left.Pixels - 4, 0);
            }
            PropertyUIElement.Left.Set(_body.GetDimensions().Width - PropertyUIElement.Width.Pixels - 6, 0);
            PropertyUIElement.Top.Set(6, 0);
        }
    }
}
