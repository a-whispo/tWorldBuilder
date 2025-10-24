using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace TerrariaInGameWorldEditor.Content
{
    internal class Keybinds : ModSystem
    {
        public static ModKeybind OpenEditorMK { get; private set; }
        public static ModKeybind Key1MK { get; private set; }
        public static ModKeybind DeleteMK { get; private set; }
        public static ModKeybind CopyMK { get; private set; }
        public static ModKeybind PasteMK { get; private set; }
        public static ModKeybind ChangeCornerOnPasteMK { get; private set; }
        public static ModKeybind RotateMK { get; private set; }
        public static ModKeybind MirrorMK { get; private set; }
        public static ModKeybind CutMK { get; private set; }
        public static ModKeybind UndoMK { get; private set; }
        public static ModKeybind RedoMK { get; private set; }

        public override void Load()
        {
            OpenEditorMK = KeybindLoader.RegisterKeybind(Mod, "Open Editor", Keys.P); // open editor
            Key1MK = KeybindLoader.RegisterKeybind(Mod, "Key 1", Keys.LeftControl); // key 1
            DeleteMK = KeybindLoader.RegisterKeybind(Mod, "Delete", Keys.Delete); // delete keybind
            CopyMK = KeybindLoader.RegisterKeybind(Mod, "Copy (Key 1 + selected key)", Keys.C); // copy keybind
            PasteMK = KeybindLoader.RegisterKeybind(Mod, "Paste (Key 1 + selected key)", Keys.V); // paste keybind
            ChangeCornerOnPasteMK = KeybindLoader.RegisterKeybind(Mod, "Change Paste Corner (Key 1 + selected key)", Keys.R); // change paste corner keybind
            RotateMK = KeybindLoader.RegisterKeybind(Mod, "Rotate selection (Key 1 + selected key)", Keys.T); // rotate keybind
            MirrorMK = KeybindLoader.RegisterKeybind(Mod, "Mirror selection (Key 1 + selected key)", Keys.M); // mirror keybind
            CutMK = KeybindLoader.RegisterKeybind(Mod, "Cut (Key 1 + selected key)", Keys.X); // cut keybind
            UndoMK = KeybindLoader.RegisterKeybind(Mod, "Undo (Key 1 + selected key)", Keys.Z); // undo keybind
            RedoMK = KeybindLoader.RegisterKeybind(Mod, "Redo (Key 1 + selected key", Keys.Y); // redo keybind
        }
    }
}
