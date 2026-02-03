using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UIElements
{
    internal static class UIElementUtils
    {
        public static SpriteBatch SpriteBatch => _spriteBatch ??= new SpriteBatch(Main.graphics.GraphicsDevice);
        private static SpriteBatch _spriteBatch;

        public static string Path;

        private static RasterizerState _overflowHiddenRasterizerState = new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true };

        public static void DrawTexture2DWithDimensions(Texture2D texture, Rectangle dimensions, Color color = default, int cornerSize = 8, int barSize = 16)
        {
            SpriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, _overflowHiddenRasterizerState, default, Main.UIScaleMatrix);
            if (color == default)
            {
                color = Color.White;
            }

            // calculate bounds
            Point topLeft = new Point(dimensions.X, dimensions.Y);
            Point bottomRight = new Point(topLeft.X + dimensions.Width - cornerSize, topLeft.Y + dimensions.Height - cornerSize);

            // middle part
            SpriteBatch.Draw(texture, new Rectangle(topLeft.X + cornerSize, topLeft.Y + cornerSize, dimensions.Width - cornerSize * 2, dimensions.Height - cornerSize * 2), new Rectangle(cornerSize, cornerSize, Math.Max(barSize, 2), Math.Max(barSize, 2)), color); // middle part

            // corners
            SpriteBatch.Draw(texture, new Rectangle(topLeft.X, topLeft.Y, cornerSize, cornerSize), new Rectangle(0, 0, cornerSize, cornerSize), color); // top left
            SpriteBatch.Draw(texture, new Rectangle(bottomRight.X, topLeft.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, 0, cornerSize, cornerSize), color); // top right
            SpriteBatch.Draw(texture, new Rectangle(topLeft.X, bottomRight.Y, cornerSize, cornerSize), new Rectangle(0, cornerSize + barSize, cornerSize, cornerSize), color); // bottom left
            SpriteBatch.Draw(texture, new Rectangle(bottomRight.X, bottomRight.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, cornerSize + barSize, cornerSize, cornerSize), color); // bottom right

            // top and bottom
            SpriteBatch.Draw(texture, new Rectangle(topLeft.X + cornerSize, topLeft.Y, dimensions.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, 0, barSize, cornerSize), color); // top
            SpriteBatch.Draw(texture, new Rectangle(topLeft.X + cornerSize, bottomRight.Y, dimensions.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, cornerSize + barSize, barSize, cornerSize), color); // bottom

            // left and right
            SpriteBatch.Draw(texture, new Rectangle(topLeft.X, topLeft.Y + cornerSize, cornerSize, dimensions.Height - cornerSize * 2), new Rectangle(0, cornerSize, cornerSize, barSize), color); // left
            SpriteBatch.Draw(texture, new Rectangle(bottomRight.X, topLeft.Y + cornerSize, cornerSize, dimensions.Height - cornerSize * 2), new Rectangle(cornerSize + barSize, cornerSize, cornerSize, barSize), color); // right
            SpriteBatch.End();
        }

        public static void DrawTexture(Texture2D texture, int width, int height, UIElement element)
        {
            SpriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, _overflowHiddenRasterizerState, default, Main.UIScaleMatrix);
            CalculatedStyle dimensions = element.GetDimensions();
            SpriteBatch.Draw(texture, new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height), Color.White);
            SpriteBatch.End();
        }
    }
}
