using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.Content;

namespace TerrariaInGameWorldEditor.UI
{
    internal class MainScreenSystem : ModSystem
    {
        public static MainScreen MainScreen;
        public static UserInterface MainScreenUI;

        public override void Load()
        {
            base.Load();
            MainScreen = new MainScreen();
            MainScreen.Activate();
            MainScreenUI = new UserInterface();
            MainScreenUI.SetState(MainScreen);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // update UI
            if (MainScreen.Visible)
            {
                if (MainScreenUI.CurrentState == null)
                {
                    MainScreenUI.SetState(MainScreen);
                    return;
                }

                // scale mouse position to account for UIScale before we update
                // (update handles mouse input and hover events)
                Main.mouseX = (int)(Main.mouseX * Main.UIScale);
                Main.mouseY = (int)(Main.mouseY * Main.UIScale);
                MainScreenUI.Update(gameTime);
            } else
            {
                MainScreenUI.SetState(null);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) // layer stuff
        {
            if (MainScreen.Visible)
            {
                // removes all layers except the cursor layer
                layers.RemoveAll(layer =>
                {
                    return !layer.Name.Equals("Vanilla: Cursor");
                });
                // insert our layer before the cursor layer
                layers.Insert(0, new LegacyGameInterfaceLayer(
                    $"{TerrariaInGameWorldEditor.MODNAME}: MainScreen",
                    delegate
                    {
                        if (MainScreenUI.CurrentState != null)
                        {
                            // start a new spritebatch with SamplerState.PointClamp and no UIScaleMatrix to 1f since the normal one is kinda ugly with UI scaling
                            SpriteBatch spriteBatch = new SpriteBatch(Main.graphics.GraphicsDevice);
                            
                            // temporarily set UIScale to 1f and draw our UI with that
                            float temp = Main.UIScale;
                            Main.UIScale = 1f;

                            // draw
                            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, default, Main.UIScaleMatrix);
                            MainScreenUI.Draw(spriteBatch, Main.gameTimeCache);
                            spriteBatch.End();
                            spriteBatch.Dispose();

                            // restore the original UIScale
                            Main.UIScale = temp;
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
            base.ModifyInterfaceLayers(layers);
        }

        public override void PostUpdateInput()
        {
            base.PostUpdateInput();
            // toggle the main screen visibility if the keybind is pressed
            if (Keybinds.OpenEditor.JustPressed)
            {
                // close the ingame options window if its open
                Main.ingameOptionsWindow = false;
                MainScreen.Visible = !MainScreen.Visible;
            }

            // close the main screen if escape is pressed
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Escape))
            {
                MainScreen.Visible = false;
            }
        }
    }
}
