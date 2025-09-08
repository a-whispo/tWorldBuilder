using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace TerrariaInGameWorldEditor.Content
{
    internal class Keybinds : ModSystem
    {
        public static ModKeybind OpenEditor { get; private set; }

        public override void Load()
        {
            OpenEditor = KeybindLoader.RegisterKeybind(Mod, "Open Editor", Keys.P);
        }
    }
}
