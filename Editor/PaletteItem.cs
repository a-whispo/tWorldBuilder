using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Editor
{
    internal class PaletteItem : TIGWEButton
    {
        public TileCopy TileCopy { get; private set; }
        private Asset<Texture2D> _xTexture;

        public PaletteItem(TileCopy tileCopy) : base(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Editor/PaletteItem"))
        {
            TileCopy = tileCopy;
            _xTexture = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Editor/X");
            Width.Set(30, 0f);
            Height.Set(30, 0f);
            SetVisibility(0.5f, 1f);
            OnLeftClick += (evt, listeningElement) =>
            {
                if (Parent.Parent.Parent is EditorPalette palette && palette.IsDeletingItems)
                {
                    palette.RemoveItem(this);
                }
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (TileCopy == null)
            {
                return;
            }

            // draw the tile on top
            CalculatedStyle thisDimensions = GetDimensions();
            Rectangle dimensions = new Rectangle((int)(thisDimensions.X + thisDimensions.Width / 2 - 16 / 2), (int)(thisDimensions.Y + thisDimensions.Height / 2 - 16 / 2), 16, 16);
            DrawUtils.DrawTileCopyInUI(TileCopy, dimensions, IsMouseHovering ? 0.8f : 1f);
            base.Draw(spriteBatch);

            // draw an x over it if we are deleting items
            if (Parent.Parent.Parent is EditorPalette palette && palette.IsDeletingItems)
            {
                Vector2 position = GetDimensions().Position();
                position.X += 7;
                position.Y += 7;
                spriteBatch.Draw(_xTexture.Value, position, new Color(215, 215, 215) * 0.9f * (IsMouseHovering ? 0.6f : 1f));
            }
        }
    }
}
