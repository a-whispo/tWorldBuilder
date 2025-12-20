using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.Button;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI
{
    internal class TIGWEUI : UIState
    {
        // events
        public event EventHandler OnShow;
        public event EventHandler OnHide;

        // public
        public bool Visible { get; set; } = false;
        public bool IsDragging { get; set; } = false;
        public string Title { get { return _titleText.Text; } set { _titleText.SetText(value); } }

        // private
        private UserInterface _UI;
        private TIGWEImageResizeable _body;
        private TIGWEButton _xButton;
        private UIText _titleText;
        private (int Left, int Top) _offset;

        public override void OnInitialize()
        {
            base.OnInitialize();
            _UI = new UserInterface();

            // default size
            Height.Set(300, 0);
            Width.Set(300, 0);

            // main body
            _body = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TIGWEUIBody"), 42, 2);
            _body.OnLeftMouseDown += (_, _) =>
            {
                // set dragging to true and grab the offset from the mouse position
                IsDragging = true;
                _offset = (Main.mouseX - (int)Left.Pixels, Main.mouseY - (int)Top.Pixels);
            };
            _body.OnLeftMouseUp += (_, _) =>
            {
                // stop dragging when letting go
                IsDragging = false;
            };
            Append(_body);

            // title text
            _titleText = new UIText("Title");
            _titleText.Left.Set(12, 0);
            _titleText.Top.Set(12, 0);
            _titleText.IgnoresMouseInteraction = true;
            Append(_titleText);

            // x button
            _xButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/XButton"));
            _xButton.SetVisibility(0.8f, 1f);
            _xButton.Width.Set(26, 0f);
            _xButton.Height.Set(26, 0f);
            _xButton.Left.Set(Width.Pixels - _xButton.Width.Pixels - 6, 0f);
            _xButton.Top.Set(6, 0f);
            _xButton.OnLeftClick += (evt, listeningElement) =>
            {
                Visible = false;
                SoundEngine.PlaySound(Terraria.ID.SoundID.MenuClose);
            };
            Append(_xButton);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // check if mouse is hovering over ui
            if (IsMouseHovering || IsDragging)
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            // update left and right offsets when dragging
            if (IsDragging)
            {
                // get bounds
                int screenWidth = (int)(Main.screenWidth * Main.UIScale);
                int screenHeight = (int)(Main.screenHeight * Main.UIScale);
                Rectangle screenBounds = new Rectangle(0, 0, screenWidth, screenHeight);
                var dimensions = GetDimensions();

                // check so the midpoint of the UI is within the screen bounds

                // check if we should clamp the x position to the screen bounds
                if (!screenBounds.Contains((int)(Main.mouseX - _offset.Left + dimensions.Width / 2), (int)(dimensions.Y + dimensions.Height / 2)))
                {
                    Left.Set(Math.Clamp(Main.mouseX - _offset.Left, -dimensions.Width / 2, screenWidth - dimensions.Width / 2), 0);
                }
                else
                {
                    Left.Set(Main.mouseX - _offset.Left, 0);
                }

                // check if we should clamp the y position to the screen bounds
                if (!screenBounds.Contains((int)(dimensions.X + dimensions.Width / 2), (int)(Main.mouseY - _offset.Top + dimensions.Height / 2)))
                {
                    Top.Set(Math.Clamp(Main.mouseY - _offset.Top, -dimensions.Height / 2, screenHeight - dimensions.Height / 2), 0);
                }
                else
                {
                    Top.Set(Main.mouseY - _offset.Top, 0);
                }
            }

            // update title bar and body sizes and location to match resizes
            _body.Top.Set(0, 0);
            _body.Left.Set(0, 0);
            _body.Height.Set(Height.Pixels, 0);
            _body.Width.Set(Width.Pixels, 0);
            _xButton.Height.Set(26, 0f);
            _xButton.Width.Set(26, 0f);
            _xButton.Left.Set(Width.Pixels - _xButton.Width.Pixels - 6, 0f);
            _xButton.Top.Set(6, 0f);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _UI.Draw(spriteBatch, gameTime);
        }

        public void UpdateUI(GameTime gametime)
        {
            // handle visibility and stuff
            if (Visible && _UI.CurrentState == null)
            {
                OnShow?.Invoke(this, EventArgs.Empty);
                _UI.SetState(this);
            }
            if (!Visible && _UI.CurrentState == this)
            {
                OnHide?.Invoke(this, EventArgs.Empty);
                _UI.SetState(null);
            }

            if (Visible)
            {
                // update the UI
                _UI.Update(gametime);
            }
        }
    }
}
