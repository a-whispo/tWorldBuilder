using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UI.TIGWEUI
{
    internal class TIGWEUISystem : ModSystem
    {
        public static TIGWEUISystem Local { get; private set; } // local instance
        public bool ShouldRenderUI { get; set; } = true;
        public float Scale = 1f;

        // states
        private List<TIGWEUI> _states = [];
        private SpriteBatch _spriteBatch;

        public override void OnModLoad()
        {
            base.OnModLoad();
            Local = this;
        }

        public override void OnModUnload()
        {
            base.OnModUnload();
            Local = null;
        }

        public void RegisterUI(TIGWEUI ui)
        {
            ui.Activate();
            ui.OnShow += (_, _) =>
            {
                MoveToTop(ui);
            };
            ui.OnHide += (_, _) =>
            {
                MoveToBottom(ui);
            };
            ui.OnLeftMouseDown += (_, _) =>
            {
                if (IsMouseHoveringState(ui))
                {
                    MoveToTop(ui);
                }
            };
            ui.Body.OnLeftMouseDown += (_, _) =>
            {
                if (IsMouseHoveringState(ui))
                {
                    ui.StartDrag();
                }
            };
            ui.Body.OnLeftMouseUp += (_, _) =>
            {
                if (IsMouseHoveringState(ui))
                {
                    ui.StopDrag();
                }
            };
            _states.Add(ui);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!ShouldRenderUI)
            {
                return;
            }

            // only update the state that the player is hovering, otherwise other states under it will also update its elements with click/hover events
            foreach (TIGWEUI state in _states.ToArray())
            {
                if (state.ContainsPoint(new Vector2(Main.mouseX, Main.mouseY)) && !IsMouseHoveringState(state) && state.Visible)
                {
                    UIElement element = state.GetElementAt(new Vector2(Main.mouseX, Main.mouseY));
                    if (element.IsMouseHovering)
                    {
                        element.MouseOut(new UIMouseEvent(element, new Vector2(Main.mouseX, Main.mouseY)));
                    }
                    continue;
                }
                state.UpdateUI(gameTime);
            }
        }

        public void MoveToTop(TIGWEUI state)
        {
            _states.Remove(state);
            _states.Add(state);
        }

        public void MoveToBottom(TIGWEUI state)
        {
            _states.Remove(state);
            _states.Insert(0, state);
        }

        private bool IsMouseHoveringState(UIState stateToCheck)
        {
            for (int i = _states.Count - 1; i >= 0; i--)
            {
                if (_states[i].ContainsPoint(new Vector2(Main.mouseX, Main.mouseY)) || _states[i].IsDragging)
                {
                    return _states[i] == stateToCheck;
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
                        _spriteBatch ??= new SpriteBatch(Main.graphics.GraphicsDevice);

                        // temporarily adjust to make everything act as if the UI scale is 1f
                        int tempWidth = Main.screenWidth;
                        int tempHeight = Main.screenHeight;
                        float tempUIScale = Main.UIScale;
                        Main.screenWidth = (int)(Main.screenWidth * Main.UIScale);
                        Main.screenHeight = (int)(Main.screenHeight * Main.UIScale);
                        Main.UIScale = Scale;

                        // start a new spritebatch with SamplerState.PointClamp and no UIScaleMatrix to 1f since the normal one is kinda ugly with UI scaling
                        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, default, Main.UIScaleMatrix);
                        foreach (TIGWEUI state in _states)
                        {
                            state?.Draw(_spriteBatch, Main.gameTimeCache);
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
