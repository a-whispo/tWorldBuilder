using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements;

namespace TerrariaInGameWorldEditor.UI
{
    internal class TIGWEUI : UIState
    {
        // public
        public bool Visible { get; set; } = false;
        public bool IsDragging { get; set; } = false;
        public (int Left, int Top) Offset { get; set; }
        public UserInterface UI { get; set; } = new UserInterface();

        // private
        private TIGWEImageResizeable _titleBar;
        private TIGWEImageResizeable _body;
        private TIGWEButton _xButton;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // default size
            this.Height.Set(300, 0);
            this.Width.Set(300, 0);

            // title bar
            _titleBar = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TitleBar"), 6, 4);
            _titleBar.OnLeftMouseDown += DragStart;
            _titleBar.OnLeftMouseUp += DragEnd;
            Append(_titleBar);

            // main body
            _body = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/UIBody"), 6, 4);
            _body.OnLeftMouseDown += DragStart;
            _body.OnLeftMouseUp += DragEnd;
            Append(_body);
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
            UIManager.MoveToTop(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // update left and right offsets when dragging
            if (IsDragging)
            {
                // get bounds
                Rectangle screenBounds = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                var dimensions = this.GetDimensions();

                // check so the midpoint of the UI is within the screen bounds

                // check if we should clamp the x position to the screen bounds
                if (!screenBounds.Contains((int)(Main.mouseX - Offset.Left + dimensions.Width / 2), (int)(dimensions.Y + dimensions.Height / 2)))
                {
                    Left.Set(Math.Clamp(Main.mouseX - Offset.Left, -dimensions.Width / 2, Main.screenWidth - dimensions.Width / 2), 0);
                }
                else
                {
                    Left.Set(Main.mouseX - Offset.Left, 0);
                }

                // check if we should clamp the y position to the screen bounds
                if (!screenBounds.Contains((int)(dimensions.X + dimensions.Width / 2), (int)(Main.mouseY - Offset.Top + dimensions.Height / 2)))
                {
                    Top.Set(Math.Clamp(Main.mouseY - Offset.Top, -dimensions.Height / 2, Main.screenHeight - dimensions.Height / 2), 0);
                }
                else
                {
                    Top.Set(Main.mouseY - Offset.Top, 0);
                }

                Recalculate();
            }

            // update title bar and body sizes and location to match resizes
            _titleBar.Top.Set(0, 0);
            _titleBar.Left.Set(0, 0);
            _titleBar.Height.Set(28, 0);
            _titleBar.Width.Set(this.Width.Pixels, 0);
            _body.Top.Set(_titleBar.Height.Pixels, 0);
            _body.Left.Set(0, 0);
            _body.Height.Set(this.Height.Pixels - _titleBar.Height.Pixels, 0);
            _body.Width.Set(this.Width.Pixels, 0);
        }

        // handle UserInterface states which determines visibility, this will pretty much only be called from UIManager when updating each TIGWEUIs UserInterface
        // the UserInterface will then set its state, if its not null it will update the UIState (which is the overriden Update method), which will then update the UI elements
        // in hindsight its probably kinda weird to make it like this
        public void UpdateUI(GameTime gametime)
        {
            // handle visibility and stuff
            if (Visible && UI.CurrentState == null)
            {
                UIManager.MoveToTop(this);
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
