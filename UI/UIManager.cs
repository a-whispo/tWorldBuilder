using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaInGameWorldEditor.UI
{
    internal class UIManager : ModSystem
    {
        // UIs
        private static List<TIGWEUI> _states = [];

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
                state.UpdateUI(gameTime);
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

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) // layer stuff
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    $"{TerrariaInGameWorldEditor.MODNAME}: UI",
                    delegate
                    {
                        // go over all the UIs
                        foreach (TIGWEUI state in _states)
                        {
                            if (state.UI?.CurrentState != null)
                            {
                                state.UI.Draw(Main.spriteBatch, Main.gameTimeCache);
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
