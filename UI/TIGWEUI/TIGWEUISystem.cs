using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaInGameWorldEditor.UI.TIGWEUI.Settings;
using TerrariaInGameWorldEditor.UI.TIGWEUI.TileSelector;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI
{
    internal class TIGWEUISystem : ModSystem
    {
        private SpriteBatch _spriteBatch;

        // settings
        public static TIGWESettings Settings = new TIGWESettings();

        // states
        public static SelectTileMenu SelectTileUI = new SelectTileMenu();
        public static SettingsUI SettingsUI = new SettingsUI();
        private static List<TIGWEUI> _states = [ SettingsUI, SelectTileUI ];

        public override void PostSetupContent()
        {
            base.PostSetupContent();
            SettingsUI.Activate();
            SelectTileUI.Activate();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // check if mouse is hovering over ui
            foreach (TIGWEUI state in _states.ToArray())
            {
                // check if mouse is hovering over ui
                if (state.IsMouseHovering)
                {
                    Main.LocalPlayer.mouseInterface = true;
                }

                // update UI
                if (CanDrag(state))
                {
                    state.UpdateUI(gameTime);
                }
            }
        }

        public static void MoveToTop(UIState state)
        {
            try
            {
                // moves the state to the end of the array so its rendered last and therefore on top
                int? index = null;

                // find index of the state
                for (int i = 0; i < _states.Count; i++)
                {
                    if (_states[i].UI.CurrentState == state)
                    {
                        index = i;
                        break;
                    }
                }

                // just return if we're already at the top
                if (index == _states.Count - 1 || index == null)
                {
                    return;
                }

                // swap UIs around
                var temp = _states[(int)index];
                _states.Remove(_states[(int)index]);
                _states.Add(temp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static bool CanDrag(UIState state)
        {
            // check if we should be able to drag the element, we should be able to if theres no ui state above it
            int? index = null;
            List<UIState> statesAtMouse = new List<UIState>();

            // find the index of the state
            for (int i = 0; i < _states.Count; i++)
            {
                if (_states[i].UI.CurrentState == state)
                {
                    index = i;
                    break;
                }
            }

            // check if there are any states with a higher index that also has the mouse on them, if there is that means that our element is under another element and we dont want to drag it
            for (int i = 0; i < _states.Count; i++)
            {
                if ((_states[i].UI.CurrentState?.GetDimensions().ToRectangle().Contains(Main.mouseX, Main.mouseY) ?? false) && i > index)
                {
                    return false;
                }
            }

            // if we pass all checks return true
            return true;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) // layer stuff
        {
            int cursorIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Cursor"));
            if (cursorIndex != -1)
            {
                layers.Insert(cursorIndex, new LegacyGameInterfaceLayer(
                    $"{TerrariaInGameWorldEditor.MODNAME}: UI",
                    delegate
                    {
                    // only bother drawing stuff if the editor ui is visible
                    if (layers.FindIndex((layer) => layer.Name.Equals($"{TerrariaInGameWorldEditor.MODNAME}: MainScreen")) > 0)
                        {
                            // go over all the UIs
                            foreach (TIGWEUI state in _states)
                            {
                                if (state.UI?.CurrentState != null)
                                {
                                    // setup spritebatch if we havent yet
                                    _spriteBatch ??= new SpriteBatch(Main.graphics.GraphicsDevice);

                                    // temporarily adjust to make everything act as if the UI scale is 1f
                                    int tempWidth = Main.screenWidth;
                                    int tempHeight = Main.screenHeight;
                                    float tempUIScale = Main.UIScale;
                                    Main.screenWidth = (int)(Main.screenWidth * Main.UIScale);
                                    Main.screenHeight = (int)(Main.screenHeight * Main.UIScale);
                                    Main.UIScale = 1f;

                                    // start a new spritebatch with SamplerState.PointClamp and no UIScaleMatrix to 1f since the normal one is kinda ugly with UI scaling
                                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, default, Main.UIScaleMatrix);
                                    state.UI.Draw(_spriteBatch, Main.gameTimeCache);
                                    _spriteBatch.End();

                                    // restore originals
                                    Main.UIScale = tempUIScale;
                                    Main.screenWidth = tempWidth;
                                    Main.screenHeight = tempHeight;
                                }
                            }
                        }
                        
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
            base.ModifyInterfaceLayers(layers);
        }
    }
}
