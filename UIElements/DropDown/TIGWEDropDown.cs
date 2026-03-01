using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;
using TerrariaInGameWorldEditor.UIElements.Scrollbar;
using TerrariaInGameWorldEditor.UIElements.SearchGrid;
using TerrariaInGameWorldEditor.UIElements.TextField;

namespace TerrariaInGameWorldEditor.UIElements.DropDown
{
    internal class TIGWEDropDown<T> : UIElement
    {
        public delegate void OptionChangedEventHandler(TIGWEDropDownItem<T> optionText);
        public event OptionChangedEventHandler OnOptionChanged;
        public TIGWEDropDownItem<T> SelectedOption { get; private set; }
        public int MaxShownItems { get; set; } = 5;
        public bool ShowDropDownButton // just visual, only looks good with a height of 26 though
        {
            get => HasChild(_dropDownButton);
            set
            {
                if (value)
                {
                    Append(_dropDownButton);
                    return;
                }
                RemoveChild(_dropDownButton);
            }
        }
        public bool ShowScrollbar
        {
            get => _dropDown.HasChild(_scrollbar);
            set
            {
                if (value)
                {
                    _dropDown.Append(_scrollbar);
                    return;
                }
                _dropDown.RemoveChild(_scrollbar);
            }
        }

        private Dictionary<T, TIGWEDropDownItem<T>> _heldValues = new Dictionary<T, TIGWEDropDownItem<T>>();
        private UIElement _dropDown;
        private TIGWETextField _selectedOptionTextField;
        private TIGWEScrollbar _scrollbar;
        private TIGWESearchGrid _itemGrid;
        private TIGWEImageResizeable _border;
        private TIGWEButton _dropDownButton;
        private float _elementHeight;
        private float _widestOptionWidth = 0;
        private bool _isShowingOptions = false;

        public TIGWEDropDown()
        {
            _selectedOptionTextField = new TIGWETextField("");
            _selectedOptionTextField.OnMouseOver += (_, _) =>
            {
                RemoveChild(_selectedOptionTextField);
                Append(_selectedOptionTextField);
            };
            _selectedOptionTextField.OnLeftClick += (_, _) =>
            {
                _itemGrid.SetSearchBar(_selectedOptionTextField);
                _selectedOptionTextField.SetText("");
                if (_selectedOptionTextField.IsFocused && _isShowingOptions)
                {
                    return;
                }
                LeftMouseClick();
            };
            Append(_selectedOptionTextField);

            _dropDownButton = new TIGWEButton(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DropDown/DropDownButton"));
            _dropDownButton.SetHoverImage(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/DropDown/DropDownButtonHover"));
            _dropDownButton.SetVisibility(1f, 1f);
            _dropDownButton.HAlign = 1f;
            _dropDownButton.Width.Set(22, 0);
            _dropDownButton.Height.Set(26, 0);
            _dropDownButton.OnMouseOver += (_, _) =>
            {
                RemoveChild(_dropDownButton);
                Append(_dropDownButton);
            };
            _dropDownButton.OnMouseOut += (_, _) =>
            {
                RemoveChild(_selectedOptionTextField);
                Append(_selectedOptionTextField);
            };
            _dropDownButton.OnLeftClick += (_, _) =>
            {
                _itemGrid.RemoveSearchBar(_selectedOptionTextField);
                _itemGrid.SearchFor("");
                LeftMouseClick();
            };
            Append(_dropDownButton);

            _dropDown = new UIElement();
            _border = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{UIElementUtils.Path}/UIElements/Assets/Border"), 6, 4);
            _border.IgnoresMouseInteraction = true;
            _dropDown.Append(_border);
            _scrollbar = new TIGWEScrollbar();
            _scrollbar.Width.Set(20, 0);
            _dropDown.Append(_scrollbar);
            _itemGrid = new TIGWESearchGrid((item, searchTerm) =>
            {
                return ((TIGWEDropDownItem<T>)item).Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            });
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)) || typeof(IComparable).IsAssignableFrom(typeof(T)))
            {
                _itemGrid.ManualSortMethod = (list) =>
                {
                    list.Sort((item1, item2) =>
                    {
                        return Comparer<T>.Default.Compare(((TIGWEDropDownItem<T>)item1).Value, ((TIGWEDropDownItem<T>)item2).Value);
                    });
                };
            }
            _itemGrid.ListPadding = -2;
            _itemGrid.PaddingLeft = -2;
            _itemGrid.PaddingRight = -2;
            _itemGrid.Left.Set(6, 0);
            _itemGrid.Top.Set(4, 0);
            _itemGrid.SetScrollbar(_scrollbar);
            _dropDown.Append(_itemGrid);
        }

        public override bool ContainsPoint(Vector2 point)
        {
            return base.ContainsPoint(point) || _dropDown.ContainsPoint(point) && _isShowingOptions;
        }

        public override void Recalculate()
        {
            base.Recalculate();

            // update element dimensions
            if (_isShowingOptions && _dropDown.Parent != null)
            {
                _dropDown.Width.Set(Math.Max(Width.Pixels, _widestOptionWidth), 0);
                _dropDown.Height.Set(Math.Clamp(_itemGrid.ShownItems.Count, 1, MaxShownItems) * _elementHeight - (Math.Clamp(_itemGrid.ShownItems.Count, 1, MaxShownItems) - 1) * 2 + 8, 0);
                _dropDown.Top.Set(GetDimensions().Y + _elementHeight - 2 - _dropDown.Parent.Top.Pixels, 0);
                _dropDown.Left.Set(GetDimensions().X - _dropDown.Parent.Left.Pixels, 0);

                _border.Height.Set(_dropDown.Height.Pixels, 0);
                _border.Width.Set(_dropDown.Width.Pixels - (ShowScrollbar ? _scrollbar.Width.Pixels : 0) + 2, 0);
                _scrollbar.Left.Set(_border.Width.Pixels - 2, 0);
                _scrollbar.Height.Set(_dropDown.Height.Pixels, 0);
                _itemGrid.Width.Set(_border.Width.Pixels - 12, 0);
                _itemGrid.Height.Set(_border.Height.Pixels - 8, 0);
            }
            else
            {
                _elementHeight = Height.Pixels;
                _selectedOptionTextField.Height.Set(_elementHeight, 0);
                _selectedOptionTextField.Width.Set(Width.Pixels + 2 - (ShowDropDownButton ? _dropDownButton.Width.Pixels : 0), 0);
            }
        }

        private void LeftMouseClick()
        {
            if (_itemGrid.AllItems.Count > 0)
            {
                if (!_isShowingOptions)
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
            if (!_isShowingOptions)
            {
                _isShowingOptions = true;

                // get the topmost element so we can append _dropDown there causing it to render and handle input on top of everything else
                UIElement top = this;
                while (top.Parent != null)
                {
                    top = top.Parent;
                }
                top.Append(_dropDown);

                // remove when clicking outside (also make sure the parent removes the event)
                top.OnUpdate += CheckIfShouldHide;

                // recalculate width and height of items
                Recalculate();
                foreach (TIGWEDropDownItem<T> item in _itemGrid.AllItems)
                {
                    item.Height.Set(_elementHeight, 0f);
                    item.Width.Set(_dropDown.Width.Pixels - 8, 0f);
                }
            }
        }

        private void CheckIfShouldHide(UIElement element)
        {
            Vector2 mouse = new Vector2(Main.mouseX, Main.mouseY);
            if (!ContainsPoint(mouse) && _isShowingOptions && !_selectedOptionTextField.IsFocused && !_scrollbar.IsDragging || !ContainsPoint(mouse) && PlayerInput.ScrollWheelDelta != 0)
            {
                HideOptions();
                _selectedOptionTextField.IsFocused = false;
            }
        }

        private void HideOptions()
        {
            if (_isShowingOptions && _dropDown.Parent != null)
            {
                _isShowingOptions = false;
                _dropDown.Parent.OnUpdate -= CheckIfShouldHide;
                _dropDown.Remove();
                _selectedOptionTextField.SetText(SelectedOption.Text);
                Recalculate();
            }
        }

        public void SetSelectedValue(T value)
        {
            if (_heldValues.TryGetValue(value, out TIGWEDropDownItem<T> item))
            {
                _selectedOptionTextField.SetText(item.Text);
                SelectedOption = item;
                OnOptionChanged?.Invoke(item);
                HideOptions();
                return;
            }
        }

        public void SetSelectedOption(TIGWEDropDownItem<T> item)
        {
            _selectedOptionTextField.SetText(item.Text);
            SelectedOption = item;
            OnOptionChanged?.Invoke(item);
            HideOptions();
        }

        public void AddOption(T value, string text)
        {
            if (_heldValues.ContainsKey(value))
            {
                return;
            }

            // make sure to keep track of the widest option
            DynamicSpriteFont spriteFont = FontAssets.MouseText.Value;
            Vector2 bounds = ChatManager.GetStringSize(spriteFont, text, new Vector2(1));
            _widestOptionWidth = Math.Max(_widestOptionWidth, bounds.X + 50);

            // add
            TIGWEDropDownItem<T> option = new TIGWEDropDownItem<T>(value, text);
            option.Left.Set(4, 0f);
            option.Height.Set(_elementHeight, 0f);
            option.Width.Set(_dropDown.Width.Pixels - 8, 0f);
            option.OnLeftClick += (_, _) =>
            {
                SetSelectedOption(option);
            };
            _itemGrid.Add(option);
            _heldValues.Add(value, option);

            // make sure we have a selected option
            if (SelectedOption == null)
            {
                SelectedOption = option;
                _selectedOptionTextField.SetText(SelectedOption.Text);
            }
        }

        public void RemoveOption(T value)
        {
            if (!_heldValues.TryGetValue(value, out TIGWEDropDownItem<T> item))
            {
                return;
            }
            _itemGrid.Remove(item);
            _heldValues.Remove(value);
            if (SelectedOption.Value.Equals(value))
            {
                SelectedOption = (TIGWEDropDownItem<T>)_itemGrid.AllItems[0];
                _selectedOptionTextField.SetText(SelectedOption.Text);
            }
            Recalculate();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            UIElementUtils.SetSpriteBatchToTheme(ref spriteBatch);
            base.DrawSelf(spriteBatch);
            UIElementUtils.SetSpriteBatchToNormal(ref spriteBatch);
        }
    }
}
