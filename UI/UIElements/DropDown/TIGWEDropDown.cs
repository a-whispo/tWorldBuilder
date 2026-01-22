using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UI.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UI.UIElements.DropDown
{
    internal class TIGWEDropDown : UIElement
    {
        public delegate void OptionChangedEventHandler(string optionText);
        public event OptionChangedEventHandler OnOptionChanged;
        public string SelectedOption { get; private set; }
        public int SelectedOptionIndex { get; private set; } // this is kinda bad
        public bool ShowDropDownButton { get { return HasChild(_dropDownButton); } set { Append(_dropDownButton); } } // just visual, only looks good with a height of 26 though

        private TIGWETextField _selectedOption;
        private List<TIGWEDropDownItem> _items = new List<TIGWEDropDownItem>();
        private TIGWEImageResizeable _border;
        private UIImageButton _dropDownButton;
        private bool _showingOptions = false;
        private UIElement _lastHover;
        private bool _lastMouseLeft = false;
        private bool _lastHoveredParent = false;

        public TIGWEDropDown(string[] items = null)
        {
            _selectedOption = new TIGWETextField("");
            _selectedOption.CanFocus = false;
            _selectedOption.OnMouseOver += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                // bring to front
                RemoveChild(_selectedOption);
                Append(_selectedOption);
            };
            _selectedOption.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                LeftMouseClick();
            };
            Append(_selectedOption);

            _dropDownButton = new UIImageButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/DropDown/DropDownButton"));
            _dropDownButton.SetHoverImage(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/DropDown/DropDownButtonHover"));
            _dropDownButton.SetVisibility(1f, 1f);
            _dropDownButton.HAlign = 1f;
            _dropDownButton.Width.Set(22, 0f);
            _dropDownButton.Height.Set(26, 0f);
            _dropDownButton.OnMouseOver += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                // bring to front
                RemoveChild(_dropDownButton);
                Append(_dropDownButton);
            };
            _dropDownButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                LeftMouseClick();
            };

            _border = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIElements/DropDown/DropDownBorder"), 6, 4);
            if (items != null)
            {
                foreach (string item in items)
                {
                    AddOption(item);
                }
                SetDefaultOption(items[0]);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // some basic event handling in cases where the dropdown menu goes outside the ui since
            // terraria doesnt handle those cases itself
            if (_showingOptions)
            {
                UIElement hoveredElement = null;
                foreach (TIGWEDropDownItem item in _items)
                {
                    if (item.ContainsPoint(new Vector2(Main.mouseX, Main.mouseY)))
                    {
                        hoveredElement = item;
                        break;
                    }
                }

                // only do events if we're outside the ui and need to do it ourselves
                if (!Parent.ContainsPoint(new Vector2(Main.mouseX, Main.mouseY)))
                {
                    if (hoveredElement != null)
                    {
                        if (_lastHover != hoveredElement)
                        {
                            hoveredElement.MouseOver(new UIMouseEvent(hoveredElement, new Vector2(Main.mouseX, Main.mouseY)));
                        }
                        if (!_lastMouseLeft && Main.mouseLeft)
                        {
                            hoveredElement.LeftMouseDown(new UIMouseEvent(hoveredElement, new Vector2(Main.mouseX, Main.mouseY)));
                        }
                    }
                }
                if (_lastHover != hoveredElement && _lastHover != null && ((!_lastHoveredParent && Parent.ContainsPoint(new Vector2(Main.mouseX, Main.mouseY))) || !Parent.ContainsPoint(new Vector2(Main.mouseX, Main.mouseY))))
                {
                    _lastHover.MouseOut(new UIMouseEvent(_lastHover, new Vector2(Main.mouseX, Main.mouseY)));
                }

                _lastHoveredParent = Parent.ContainsPoint(new Vector2(Main.mouseX, Main.mouseY));
                _lastMouseLeft = Main.mouseLeft;
                _lastHover = hoveredElement;
            }

            if (!ContainsPoint(new Vector2(Main.mouseX, Main.mouseY)) && _showingOptions)
            {
                HideOptions();
            } 
        }

        public override void Recalculate()
        {
            base.Recalculate();

            // update element dimensions
            _border.Width.Set(this.Width.Pixels, 0f);
            _border.Height.Set(Height.Pixels - _selectedOption.Height.Pixels, 0f);
            _border.Top.Set(_selectedOption.Height.Pixels - 2, 0f);
            if (!_showingOptions)
            {
                _selectedOption.Height.Set(this.Height.Pixels, 0f);
                _selectedOption.Width.Set(this.Width.Pixels + 2 - (ShowDropDownButton ? _dropDownButton.Width.Pixels : 0f), 0f);
                RecalculateItems();
            }
        }

        public void RecalculateItems()
        {
            _border.RemoveAllChildren();
            foreach (TIGWEDropDownItem item in _items)
            {
                item.Top.Set(4 + _items.IndexOf(item) * (this.Height.Pixels - 2), 0f);
                item.Left.Set(4, 0f);
                item.Height.Set(this.Height.Pixels, 0f);
                item.Width.Set(this.Width.Pixels - 8, 0f);
                _border.Append(item);
            }
        }

        private void LeftMouseClick()
        {
            if (_items.Count > 0)
            {
                if (!_showingOptions)
                {
                    ShowOptions();
                } 
                else
                {
                    HideOptions();
                }
            }
        }

        private void ShowOptions()
        {
            if (!_showingOptions)
            {
                UIElement parent = Parent;
                parent.RemoveChild(this);
                parent.Append(this);
                _showingOptions = true;
                Append(_border);
                Height.Set(Height.Pixels + (_items.Count * Height.Pixels) - (_items.Count - 1) * 2 + 8, 0f);
                if (_selectedOption.IsMouseHovering)
                {
                    RemoveChild(_selectedOption);
                    Append(_selectedOption);
                }
                if (_dropDownButton.IsMouseHovering)
                {
                    RemoveChild(_dropDownButton);
                    Append(_dropDownButton);
                }
            }
        }

        private void HideOptions()
        {
            if (_showingOptions)
            {
                _showingOptions = false;
                RemoveChild(_border);
                Height.Set(_selectedOption.Height.Pixels, 0f);
            }
        }

        public void SetDefaultOption(string option)
        {
            foreach (TIGWEDropDownItem item in _items)
            {
                if (item.Text.Equals(option))
                {
                    _selectedOption.SetText(option);
                    SelectedOption = option;
                    SelectedOptionIndex = _items.IndexOf(item);
                    return;
                }
            }
        }

        public void SetSelectedOption(string option)
        {
            foreach (TIGWEDropDownItem item in _items)
            {
                // make sure the option exists
                if (item.Text.Equals(option))
                {
                    _selectedOption.SetText(option);
                    HideOptions();
                    SelectedOption = option;
                    SelectedOptionIndex = _items.IndexOf(item);
                    OnOptionChanged?.Invoke(option);
                    return;
                }
            }
        }

        public void AddOption(string option)
        {
            // dont add if an option with the same text already exists
            foreach (TIGWEDropDownItem item in _items)
            {
                if (item.Text.Equals(option))
                {
                    return;
                }
            }

            // add new option
            TIGWEDropDownItem newItem = new TIGWEDropDownItem(option);
            _items.Add(newItem);
            newItem.Top.Set(4 +_items.IndexOf(newItem) * (this.Height.Pixels - 2), 0f);
            newItem.Left.Set(4, 0f);
            newItem.Height.Set(this.Height.Pixels, 0f);
            newItem.Width.Set(this.Width.Pixels - 8, 0f);
            _border.Append(newItem);
            newItem.DropDownParent = this;
        }

        public void RemoveOption(string option)
        {
            foreach (TIGWEDropDownItem item in _items)
            {
                if (item.Text.Equals(option))
                {
                    _items.Remove(item);
                    if (SelectedOption.Equals(option))
                    {
                        SetSelectedOption(_items[0].Text);
                    }
                    break;
                }
            }
            RecalculateItems();
        }
    }
}
