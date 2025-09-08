using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.UIElements;

namespace TerrariaInGameWorldEditor.UI
{
    public class MainScreen : UIState
    {
        #region Side Dimensions
        private int _leftWidth;
        private int _rightWidth;
        private int _topHeight;
        private int _bottomHeight;

        public int LeftWidth
        {
            get => _leftWidth;
            set
            {
                _leftWidth = value;
                RecalculateSideDimensions();
            }
        }

        public int RightWidth
        {
            get => _rightWidth;
            set
            {
                _rightWidth = value;
                RecalculateSideDimensions();
            }
        }

        public int TopHeight
        {
            get => _topHeight;
            set
            {
                _topHeight = value;
                RecalculateSideDimensions();
            }
        }

        public int BottomHeight
        {
            get => _bottomHeight;
            set
            {
                _bottomHeight = value;
                RecalculateSideDimensions();
            }
        }
        #endregion
        #region Sides
        private TIGWEImageResizeable _bottom;
        private TIGWEImageResizeable _left;
        private TIGWEImageResizeable _right;
        private TIGWEImageResizeable _titleBar;
        private TIGWEImageResizeable _top;
        private TIGWEImageResizeable _innerBorder;
        #endregion
        public bool Visible = false;

        public override void OnInitialize()
        {
            base.OnInitialize();
            Left.Set(0, 0f);
            Top.Set(0, 0f);
            Width.Set(Main.screenWidth * Main.UIScale, 0f);
            Height.Set(Main.screenHeight * Main.UIScale, 0f);

            // main body
            _bottom = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenColor"), 1, 1);
            Append(_bottom);
            _left = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenColor"), 1, 1);
            Append(_left);
            _right = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenColor"), 1, 1);
            Append(_right);
            _titleBar = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenTitleBar"), 4, 2);
            Append(_titleBar);
            _top = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenColor"), 1, 1);
            Append(_top);
            _innerBorder = new TIGWEImageResizeable(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/MainScreenInnerBorder"), 6, 4);
            Append(_innerBorder);
            
            // some default bounds
            BottomHeight = 32;
            LeftWidth = 130;
            RightWidth = 130;
            TopHeight = 34;

            // misc
            TIGWEButton xButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/XButton"));
            xButton.SetVisibility(0.8f, 1f);
            xButton.Width.Set(30, 0f);
            xButton.Height.Set(30, 0f);
            xButton.Left.Set(Width.Pixels - xButton.Width.Pixels - 2, 0f);
            xButton.Top.Set(4, 0f);
            xButton.OnLeftClick += (UIMouseEvent evt, UIElement listeningElement) =>
            {
                Visible = false;
                SoundEngine.PlaySound(Terraria.ID.SoundID.MenuClose);
            };
            Append(xButton);

            TIGWEButton tileButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Button"));
            tileButton.SetVisibility(0.8f, 1f);
            tileButton.Width.Set(30, 0f);
            tileButton.Height.Set(30, 0f);
            tileButton.Left.Set(2, 0f);
            tileButton.Top.Set(42, 0f);
            Append(tileButton);

            TIGWEButton saveTileButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/SaveButton"));
            saveTileButton.SetVisibility(0.8f, 1f);
            saveTileButton.Width.Set(30, 0f);
            saveTileButton.Height.Set(30, 0f);
            saveTileButton.Left.Set(tileButton.Left.Pixels + tileButton.Width.Pixels + 2, 0f);
            saveTileButton.Top.Set(42, 0f);
            saveTileButton.HoverText = "Save to palette";
            Append(saveTileButton);

            TIGWEButton loadTileButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/LoadButton"));
            loadTileButton.SetVisibility(0.8f, 1f);
            loadTileButton.Width.Set(30, 0f);
            loadTileButton.Height.Set(30, 0f);
            loadTileButton.Left.Set(saveTileButton.Left.Pixels + saveTileButton.Width.Pixels + 2, 0f);
            loadTileButton.Top.Set(42, 0f);
            loadTileButton.HoverText = "Load tile from clipboard";
            Append(loadTileButton);

            TIGWEButton copyTileButton = new TIGWEButton(ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/CopyButton"));
            copyTileButton.SetVisibility(0.8f, 1f);
            copyTileButton.Width.Set(30, 0f);
            copyTileButton.Height.Set(30, 0f);
            copyTileButton.Left.Set(loadTileButton.Left.Pixels + loadTileButton.Width.Pixels + 2, 0f);
            copyTileButton.Top.Set(42, 0f);
            copyTileButton.HoverText = "Copy tile to clipboard";
            Append(copyTileButton);

            // loop through all the tools and add their buttons

        }

        public void RecalculateSideDimensions()
        {
            _bottom.Width.Set(0, 1f);
            _bottom.Height.Set(BottomHeight, 0f);
            _bottom.Left.Set(0, 0f);
            _bottom.Top.Set(Height.Pixels - _bottom.Height.Pixels, 0f);

            _left.Width.Set(LeftWidth, 0f);
            _left.Height.Set(0, 1f);
            _left.Left.Set(0, 0f);
            _left.Top.Set(0, 0f);

            _right.Width.Set(RightWidth, 0f);
            _right.Height.Set(0, 1f);
            _right.Left.Set(Width.Pixels - _right.Width.Pixels, 0f);
            _right.Top.Set(0, 0f);

            _titleBar.Width.Set(0, 1f);
            _titleBar.Height.Set(40, 0f);
            _titleBar.Left.Set(0, 0f);
            _titleBar.Top.Set(0, 0f);

            _top.Width.Set(0, 1f);
            _top.Height.Set(TopHeight, 0f);
            _top.Left.Set(0, 0f);
            _top.Top.Set(_titleBar.Height.Pixels, 0f);

            _innerBorder.Width.Set(Width.Pixels - _left.Width.Pixels - _right.Width.Pixels, 0f);
            _innerBorder.Height.Set(Height.Pixels - _top.Top.Pixels - _top.Height.Pixels - _bottom.Height.Pixels, 0f);
            _innerBorder.Left.Set(_left.Width.Pixels, 0f);
            _innerBorder.Top.Set(_top.Top.Pixels + _top.Height.Pixels, 0f);
        }
    }
}
