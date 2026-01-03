using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace TerrariaInGameWorldEditor.Content
{
    internal class Keybinds : ModSystem
    {
        public static ModKeybind OpenEditorMK { get; private set; }
        public static ModKeybind DeleteMK { get; private set; }
        public static ModKeybind CopyMK { get; private set; }
        public static ModKeybind PasteMK { get; private set; }
        public static ModKeybind ChangeCornerOnPasteMK { get; private set; }
        public static ModKeybind RotateMK { get; private set; }
        public static ModKeybind MirrorMK { get; private set; }
        public static ModKeybind CutMK { get; private set; }
        public static ModKeybind UndoMK { get; private set; }
        public static ModKeybind RedoMK { get; private set; }
        public static ModKeybind SaveMK { get; private set; }

        public override void Load()
        {
            OpenEditorMK = KeybindLoader.RegisterKeybind(Mod, "Open Editor", Keys.P); // open editor
            DeleteMK = KeybindLoader.RegisterKeybind(Mod, "Delete (Ctrl + Key)", Keys.Delete); // delete keybind
            CopyMK = KeybindLoader.RegisterKeybind(Mod, "Copy (Ctrl + Key)", Keys.C); // copy keybind
            PasteMK = KeybindLoader.RegisterKeybind(Mod, "Paste (Ctrl + Key)", Keys.V); // paste keybind
            ChangeCornerOnPasteMK = KeybindLoader.RegisterKeybind(Mod, "Change Paste Corner (Ctrl + Key)", Keys.R); // change paste corner keybind
            RotateMK = KeybindLoader.RegisterKeybind(Mod, "Rotate selection (Ctrl + Key)", Keys.T); // rotate keybind
            MirrorMK = KeybindLoader.RegisterKeybind(Mod, "Mirror selection (Ctrl + Key)", Keys.M); // mirror keybind
            CutMK = KeybindLoader.RegisterKeybind(Mod, "Cut (Ctrl + Key)", Keys.X); // cut keybind
            UndoMK = KeybindLoader.RegisterKeybind(Mod, "Undo (Ctrl + Key)", Keys.Z); // undo keybind
            RedoMK = KeybindLoader.RegisterKeybind(Mod, "Redo (Ctrl + Key)", Keys.Y); // redo keybind
            SaveMK = KeybindLoader.RegisterKeybind(Mod, "Save (Ctrl + Key)", Keys.S); // save keybind
        }
    }
}
