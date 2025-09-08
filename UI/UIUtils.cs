using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UI
{
    static internal class UIUtils
    {
        public static void DrawTexture2DWithDimensions(Texture2D texture, Rectangle dimensions, SpriteBatch spriteBatch, Color color = default, int cornerSize = 8, int barSize = 16)
        {
            if (color == default)
            {
                color = Color.White;
            }

            // calculate bounds
            Point topLeft = new Point((int)dimensions.X, (int)dimensions.Y);
            Point bottomRight = new Point(topLeft.X + dimensions.Width - cornerSize, topLeft.Y + dimensions.Height - cornerSize);

            // middle part
            spriteBatch.Draw(texture, new Rectangle(topLeft.X + cornerSize, topLeft.Y + cornerSize, dimensions.Width - cornerSize * 2, dimensions.Height - cornerSize * 2), new Rectangle(cornerSize, cornerSize, barSize - 2, barSize - 2), color); // middle part

            // corners
            spriteBatch.Draw(texture, new Rectangle(topLeft.X, topLeft.Y, cornerSize, cornerSize), new Rectangle(0, 0, cornerSize, cornerSize), color); // top left
            spriteBatch.Draw(texture, new Rectangle(bottomRight.X, topLeft.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, 0, cornerSize, cornerSize), color); // top right
            spriteBatch.Draw(texture, new Rectangle(topLeft.X, bottomRight.Y, cornerSize, cornerSize), new Rectangle(0, cornerSize + barSize, cornerSize, cornerSize), color); // bottom left
            spriteBatch.Draw(texture, new Rectangle(bottomRight.X, bottomRight.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, cornerSize + barSize, cornerSize, cornerSize), color); // bottom right

            // top and bottom
            spriteBatch.Draw(texture, new Rectangle(topLeft.X + cornerSize, topLeft.Y, dimensions.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, 0, barSize, cornerSize), color); // top
            spriteBatch.Draw(texture, new Rectangle(topLeft.X + cornerSize, bottomRight.Y, dimensions.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, cornerSize + barSize, barSize, cornerSize), color); // bottom

            // left and right
            spriteBatch.Draw(texture, new Rectangle(topLeft.X, topLeft.Y + cornerSize, cornerSize, dimensions.Height - cornerSize * 2), new Rectangle(0, cornerSize, cornerSize, barSize), color); // left
            spriteBatch.Draw(texture, new Rectangle(bottomRight.X, topLeft.Y + cornerSize, cornerSize, dimensions.Height - cornerSize * 2), new Rectangle(cornerSize + barSize, cornerSize, cornerSize, barSize), color); // right
        }

        public static void DrawTexture(Texture2D texture, int width, int height, UIElement element, SpriteBatch spriteBatch)
        {
            Color color = Color.White;

            // calculate bounds
            CalculatedStyle dimensions = element.GetDimensions();

            // draw the texture
            spriteBatch.Draw(texture, new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height), color);
        }
    }
}
