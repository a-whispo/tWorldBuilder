using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public static TIGWEUISystem Local; // local instance
        public bool ShouldRenderUI = true;

        // states
        public SelectTileMenu SelectTileUI;
        public SettingsUI SettingsUI;
        private List<TIGWEUI> _states = [];
        private SpriteBatch _spriteBatch;

        public override void OnModLoad()
        {
            base.OnModLoad();
            Local = this;
        }

        public override void PostSetupContent()
        {
            base.PostSetupContent();
            SelectTileUI = new SelectTileMenu();
            SettingsUI = new SettingsUI();
            RegisterUI(SelectTileUI);
            RegisterUI(SettingsUI);
        }

        public void RegisterUI(TIGWEUI ui)
        {
            ui.Activate();
            ui.OnShow += () =>
            {
                MoveToTop(ui);
            };
            ui.OnClick += () =>
            {
                MoveToTop(ui);
            };
            _states.Add(ui);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!ShouldRenderUI)
            {
                return;
            }

            // check if mouse is hovering over ui
            foreach (TIGWEUI state in _states.ToArray())
            {
                // update UI
                state.UpdateUI(gameTime);
            }
        }

        public void MoveToTop(TIGWEUI state)
        {
            // moves the state to the end of the array so its rendered last and therefore on top
            int? index = null;

            // find index of the state
            for (int i = 0; i < _states.Count; i++)
            {
                if (_states[i] == state)
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

        public override void PostUpdateInput()
        {
            base.PostUpdateInput();

            // on click
            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                foreach (TIGWEUI state in _states)
                {
                    if (IsMouseHoveringState(state))
                    {
                        MoveToTop(state);
                        break;
                    }
                }
            }
        }

        private bool IsMouseHoveringState(UIState stateToCheck)
        {
            for (int i = _states.Count - 1; i >= 0; i--)
            {
                if (_states[i].GetDimensions().ToRectangle().Contains(Main.mouseX, Main.mouseY) && _states[i] == stateToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) // layer stuff
        {
            if (!ShouldRenderUI)
            {
                return;
            }

            int cursorIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Cursor"));
            if (cursorIndex != -1)
            {
                layers.Insert(cursorIndex, new LegacyGameInterfaceLayer(
                    $"{TerrariaInGameWorldEditor.MODNAME}: UI",
                    delegate
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

                        // go over all the UIs
                        foreach (TIGWEUI state in _states)
                        {
                            if (state != null)
                            {
                                state.Draw(_spriteBatch, Main.gameTimeCache);
                            }
                        }

                        _spriteBatch.End();
                        // restore originals
                        Main.UIScale = tempUIScale;
                        Main.screenWidth = tempWidth;
                        Main.screenHeight = tempHeight;

                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
            base.ModifyInterfaceLayers(layers);
        }
    }
}
