using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace TerrariaInGameWorldEditor.Editor.Windows.Settings
{
    internal class SettingsOption<T> : SettingsNode where T : UIElement
    {
        public string Name { get; private set; }
        public T OptionElement { get; private set; }
        public bool Enabled
        {
            get => _propertyText.TextColor == Color.White;
            set
            {
                _propertyText.TextColor = Color.White * (value ? 1f : 0.6f);
                OptionElement.IgnoresMouseInteraction = !value;
            }
        }

        private int _textMarginTop = 10;
        private int _textMarginRight = 8;
        private bool _shouldFit;
        private UIText _propertyText;
        private Vector2 _propertyTextSize;

        public SettingsOption(string name, T element, bool shouldFit = false)
        {
            Name = name;
            OptionElement = element;
            _shouldFit = shouldFit;

            // ui stuff
            Width.Set(-4, 1);
            Height.Set(38, 0);
            _propertyText = new UIText(Name);
            _propertyText.Top.Set(_textMarginTop, 0);
            _propertyText.Left.Set(10, 0);
            Append(_propertyText);
            Append(OptionElement);
            Recalculate();
        }

        private void RecalculateHeightAndText()
        {
            string allWords = string.Empty;
            string currentLine = string.Empty;
            string[] words = Name.Split(' ');
            float maxLineLength = GetDimensions().Width * 0.5f;
            Height.Set(38, 0);
            for (int i = 0; i <= words.Length - 1; i++)
            {
                currentLine += words[i] + " ";
                Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, currentLine, new Vector2(1));

                // insert a new line if needed
                if (size.X > maxLineLength || i == words.Length - 1)
                {
                    allWords += currentLine;
                    currentLine = string.Empty;
                    if (size.X > maxLineLength && i != words.Length - 1)
                    {
                        allWords += "\n";
                        Height.Set(Height.Pixels + 28, 0);
                    }
                }
            }
            _propertyTextSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, allWords, new Vector2(1));
            _propertyText.SetText(allWords);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            RecalculateHeightAndText();
            if (_shouldFit)
            {
                OptionElement.Width.Set(GetDimensions().Width - _propertyTextSize.X - _textMarginRight - _propertyText.Left.Pixels - 4, 0);
                OptionElement.Height.Set(26, 0);
            }
            OptionElement.Left.Set(-OptionElement.Width.Pixels - 6, 1);
            OptionElement.Top.Set(6, 0);
        }
    }
}
