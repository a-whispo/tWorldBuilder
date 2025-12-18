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
        public string SelectedOption { get { return _selectedOption.GetText(); } }
        public bool ShowDropDownButton { set { Append(_dropDownButton); } get { return HasChild(_dropDownButton); } } // just visual, only looks good with a height of 26 though

        private TIGWETextField _selectedOption;
        private List<TIGWEDropDownItem> items = new List<TIGWEDropDownItem>();
        private TIGWEImageResizeable _border;
        private UIImageButton _dropDownButton;
        private bool _showingOptions = false;

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
            _border.IgnoresMouseInteraction = true;

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
            if (!_showingOptions)
            {
                _selectedOption.Height.Set(this.Height.Pixels, 0f);
                _selectedOption.Width.Set(this.Width.Pixels + 2 - (ShowDropDownButton ? _dropDownButton.Width.Pixels : 0f), 0f);
            }

            // check where player clicked, if its outside the dropdown, unfocus
            if (!ContainsPoint(new Vector2((float)(Main.MouseWorld.X - Main.screenPosition.X), (float)(Main.MouseWorld.Y - Main.screenPosition.Y))))
            {
                if (_showingOptions)
                {
                    HideOptions();
                }
            } else
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            // update border dimensions
            _border.Width.Set(this.Width.Pixels, 0f);
            _border.Height.Set(items.Count * _selectedOption.Height.Pixels + 8 - ((items.Count - 1) * 2), 0f);
        }

        private void LeftMouseClick()
        {
            if (items.Count > 0)
            {
                if (!_showingOptions)
                {
                    ShowOptions();
                } else
                {
                    HideOptions();
                }
            }
        }

        private void ShowOptions()
        {
            if (!_showingOptions)
            {
                _showingOptions = true;
                Append(_border);
                foreach (TIGWEDropDownItem item in items)
                {
                    item.Top.Set(items.IndexOf(item) * (this.Height.Pixels - 2) + 2 + this.Height.Pixels, 0f);
                    item.Left.Set(4, 0f);
                    item.Height.Set(this.Height.Pixels, 0f);
                    item.Width.Set(this.Width.Pixels - 8, 0f);
                    Append(item);
                }
                _border.Top.Set(_selectedOption.Height.Pixels - 2, 0f);
                Height.Set(Height.Pixels + items.Count * Height.Pixels + 4, 0f);
            }
        }

        private void HideOptions()
        {
            if (_showingOptions)
            {
                _showingOptions = false;
                RemoveChild(_border);
                foreach (TIGWEDropDownItem item in items)
                {
                    RemoveChild(item);
                }
                Height.Set(_selectedOption.Height.Pixels, 0f);
            }
        }

        public void SetDefaultOption(string option)
        {
            foreach (TIGWEDropDownItem item in items)
            {
                if (item.Text.Equals(option))
                {
                    _selectedOption.SetText(option);
                    return;
                }
            }
        }

        public void SetSelectedOption(string option)
        {
            foreach (TIGWEDropDownItem item in items)
            {
                // make sure the option exists
                if (item.Text.Equals(option))
                {
                    _selectedOption.SetText(option);
                    HideOptions();
                    OnOptionChanged?.Invoke(option);
                    return;
                }
            }
        }

        public void AddOption(string option)
        {
            // dont add if an option with the same text already exists
            foreach (TIGWEDropDownItem item in items)
            {
                if (item.Text.Equals(option))
                {
                    return;
                }
            }

            // add new option
            TIGWEDropDownItem newItem = new TIGWEDropDownItem(option);
            items.Add(newItem);
        }

        public void RemoveOption(string option)
        {
            foreach (TIGWEDropDownItem item in items)
            {
                if (item.Text.Equals(option))
                {
                    items.Remove(item);
                    if (SelectedOption.Equals(option))
                    {
                        SetSelectedOption(items[0].Text);
                    }
                    break;
                }
            }
        }
    }
}
