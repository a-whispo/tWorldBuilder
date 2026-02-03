using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UIElements.Button;
using TerrariaInGameWorldEditor.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.Editor.Windows
{
    internal class TIGWEUI : UIState
    {
        // events
        public event EventHandler OnShow;
        public event EventHandler OnHide;
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    if (value && _UI.CurrentState == null)
                    {
                        OnShow?.Invoke(this, EventArgs.Empty);
                        _UI.SetState(this);
                        SoundEngine.PlaySound(Terraria.ID.SoundID.MenuOpen);
                    }
                    if (!value && _UI.CurrentState == this)
                    {
                        OnHide?.Invoke(this, EventArgs.Empty);
                        _UI.SetState(null);
                        SoundEngine.PlaySound(Terraria.ID.SoundID.MenuClose);
                    }
                    _visible = value;
                }
            }
        }
        public bool IsDragging { get; private set; } = false;
        public string Title 
        { 
            get => _titleText.Text; 
            set => _titleText.SetText(value);
        }
        public TIGWEImageResizeable Body;

        protected string _defaultTitle 
        { 
            set
            { 
                if (Title.Equals("")) 
                { 
                    Title = value; 
                } 
            } 
        }
        private bool _visible = false;
        private UserInterface _UI;
        private TIGWEButton _xButton;
        private UIText _titleText = new UIText("");
        private (int Left, int Top) _offset;

        public override void OnInitialize()
        {
            base.OnInitialize();
            _UI = new UserInterface();
            Height.Set(300, 0);
            Width.Set(300, 0);

            // main body
            Body = new TIGWEImageResizeable(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/EditorWindows/TIGWEUIBody"), 42, 2);
            Append(Body);

            // title text
            _titleText.Left.Set(12, 0);
            _titleText.Top.Set(12, 0);
            _titleText.IgnoresMouseInteraction = true;
            Append(_titleText);

            // x button
            _xButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/XButton"));
            _xButton.SetVisibility(0.8f, 1f);
            _xButton.Width.Set(26, 0f);
            _xButton.Height.Set(26, 0f);
            _xButton.Left.Set(Width.Pixels - _xButton.Width.Pixels - 6, 0f);
            _xButton.Top.Set(6, 0f);
            _xButton.OnLeftClick += (_, _) =>
            {
                Visible = false;
                SoundEngine.PlaySound(Terraria.ID.SoundID.MenuClose);
            };
            Append(_xButton);
        }

        public void StartDrag()
        {
            IsDragging = true;
            _offset = (Main.mouseX - (int)Left.Pixels, Main.mouseY - (int)Top.Pixels);
        }

        public void StopDrag()
        {
            IsDragging = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsMouseHovering || IsDragging)
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            // update left and right offsets when dragging
            if (IsDragging)
            {
                int screenWidth = (int)(Main.screenWidth * Main.UIScale);
                int screenHeight = (int)(Main.screenHeight * Main.UIScale);
                Rectangle screenBounds = new Rectangle(0, 0, screenWidth, screenHeight);
                var dimensions = GetDimensions();

                // check so the midpoint of the UI is within the screen bounds and clamp x and y if needed
                if (!screenBounds.Contains((int)(Main.mouseX - _offset.Left + dimensions.Width / 2), (int)(dimensions.Y + dimensions.Height / 2)))
                {
                    Left.Set(Math.Clamp(Main.mouseX - _offset.Left, -dimensions.Width / 2, screenWidth - dimensions.Width / 2), 0);
                }
                else
                {
                    Left.Set(Main.mouseX - _offset.Left, 0);
                }
                if (!screenBounds.Contains((int)(dimensions.X + dimensions.Width / 2), (int)(Main.mouseY - _offset.Top + dimensions.Height / 2)))
                {
                    Top.Set(Math.Clamp(Main.mouseY - _offset.Top, -dimensions.Height / 2, screenHeight - dimensions.Height / 2), 0);
                }
                else
                {
                    Top.Set(Main.mouseY - _offset.Top, 0);
                }
            }
        }

        public override void Recalculate()
        {
            base.Recalculate();

            // update title bar and body sizes and location to match resizes
            Body?.Top.Set(0, 0);
            Body?.Left.Set(0, 0);
            Body?.Height.Set(Height.Pixels, 0);
            Body?.Width.Set(Width.Pixels, 0);
            _xButton?.Height.Set(26, 0f);
            _xButton?.Width.Set(26, 0f);
            _xButton?.Left.Set(Width.Pixels - _xButton.Width.Pixels - 6, 0f);
            _xButton?.Top.Set(6, 0f);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _UI.Draw(spriteBatch, gameTime);
        }

        public void UpdateUI(GameTime gametime)
        {
            // update the UI
            if (Visible)
            {
                _UI.Update(gametime);
            }
        }
    }
}
