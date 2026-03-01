using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaInGameWorldEditor.UIElements
{
    internal static class UIElementUtils
    {
        public static string Path { get; set; }
        public static Effect ThemeEffect
        {
            get
            {
                if (_themeEffect == null && Path != null)
                {
                    _themeEffect ??= ModContent.Request<Effect>($"{Path}/UIElements/Theme", AssetRequestMode.ImmediateLoad).Value;
                    _themeEffect.Parameters["OriginalPrimary"].SetValue(new Color(43, 56, 101).ToVector4());
                    _themeEffect.Parameters["OriginalSecondary"].SetValue(new Color(72, 92, 168).ToVector4());
                }
                return _themeEffect;
            }
        }
        private static Effect _themeEffect;
        public static Color PrimaryColor
        {
            get => new Color(ThemeEffect.Parameters["NewPrimary"].GetValueVector4());
            set => ThemeEffect.Parameters["NewPrimary"].SetValue(value.ToVector4());
        }
        public static Color SecondaryColor
        {
            get => new Color(ThemeEffect.Parameters["NewSecondary"].GetValueVector4());
            set => ThemeEffect.Parameters["NewSecondary"].SetValue(value.ToVector4());
        }

        public static void SetSpriteBatchToTheme(ref SpriteBatch spriteBatch)
        {
            try
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, default, new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true }, ThemeEffect, Main.UIScaleMatrix);
            } 
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.ModLogger.Error("Failed to set spritebatch to theme.", ex);
            }
        }

        public static void SetSpriteBatchToNormal(ref SpriteBatch spriteBatch)
        {
            try
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true }, null, Main.UIScaleMatrix);
            }
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.ModLogger.Error("Failed to set spritebatch to normal.", ex);
            }
        }

        public static void DrawTexture2DWithDimensions(SpriteBatch spriteBatch, Texture2D texture, Rectangle dimensions, Color color = default, int cornerSize = 8, int barSize = 16)
        {
            if (color == default)
            {
                color = Color.White;
            }

            // calculate bounds
            Point topLeft = new Point(dimensions.X, dimensions.Y);
            Point bottomRight = new Point(topLeft.X + dimensions.Width - cornerSize, topLeft.Y + dimensions.Height - cornerSize);

            // middle part
            spriteBatch.Draw(texture, new Rectangle(topLeft.X + cornerSize, topLeft.Y + cornerSize, dimensions.Width - cornerSize * 2, dimensions.Height - cornerSize * 2), new Rectangle(cornerSize, cornerSize, Math.Max(barSize, 2), Math.Max(barSize, 2)), color); // middle part

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
    }
}
