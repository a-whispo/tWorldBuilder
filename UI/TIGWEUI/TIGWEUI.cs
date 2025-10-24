using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements.Button;
using TerrariaInGameWorldEditor.UI.UIElements.ImageResizeable;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI
{
    internal class TIGWEUI : UIState
    {
        // public
        public bool Visible { get; set; } = false;
        public bool IsDragging { get; set; } = false;
        public (int Left, int Top) Offset { get; set; }
        public UserInterface UI { get; set; } = new UserInterface();

        // private
        private TIGWEImageResizeable _body;
        private TIGWEButton _xButton;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // default size
            Height.Set(300, 0);
            Width.Set(300, 0);

            // main body
            _body = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TIGWEUIBody"), 42, 2);
            _body.OnLeftMouseDown += DragStart;
            _body.OnLeftMouseUp += DragEnd;
            Append(_body);

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

        protected virtual void DragEnd(UIMouseEvent evt, UIElement listeningElement)
        {
            // stop dragging when letting go
            IsDragging = false;
        }

        protected virtual void DragStart(UIMouseEvent evt, UIElement listeningElement)
        {
            // set dragging to true and grab the offset from the mouse position
            IsDragging = true;
            Offset = ((int)evt.MousePosition.X - (int)Left.Pixels, (int)evt.MousePosition.Y - (int)Top.Pixels);

            // since we clicked the border we should also move it to the top
            TIGWEUISystem.MoveToTop(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
                if (!screenBounds.Contains((int)(Main.mouseX - Offset.Left + dimensions.Width / 2), (int)(dimensions.Y + dimensions.Height / 2)))
                {
                    Left.Set(Math.Clamp(Main.mouseX - Offset.Left, -dimensions.Width / 2, screenWidth - dimensions.Width / 2), 0);
                }
                else
                {
                    Left.Set(Main.mouseX - Offset.Left, 0);
                }

                // check if we should clamp the y position to the screen bounds
                if (!screenBounds.Contains((int)(dimensions.X + dimensions.Width / 2), (int)(Main.mouseY - Offset.Top + dimensions.Height / 2)))
                {
                    Top.Set(Math.Clamp(Main.mouseY - Offset.Top, -dimensions.Height / 2, screenHeight - dimensions.Height / 2), 0);
                }
                else
                {
                    Top.Set(Main.mouseY - Offset.Top, 0);
                }

                Recalculate();
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

        // handle UserInterface states which determines visibility, this will pretty much only be called from UIManager when updating each TIGWEUIs UserInterface
        // the UserInterface will then set its state, if its not null it will update the UIState (which is the overriden Update method), which will then update the UI elements
        // in hindsight its probably kinda weird to make it like this
        public void UpdateUI(GameTime gametime)
        {
            // handle visibility and stuff
            if (Visible && UI.CurrentState == null)
            {
                TIGWEUISystem.MoveToTop(this);
                UI.SetState(this);
                UI.CurrentState.Activate();
            }
            if (!Visible && UI.CurrentState == this)
            {
                UI.SetState(null);
            }
            // update the UI
            UI.Update(gametime);
        }
    }
}
