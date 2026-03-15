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
        public static ModKeybind FastMoveMK { get; private set; }

        public override void OnModLoad()
        {
            base.OnModLoad();
            OpenEditorMK = KeybindLoader.RegisterKeybind(Mod, "Open Editor", Keys.P);
            DeleteMK = KeybindLoader.RegisterKeybind(Mod, "Delete (Ctrl + Key)", Keys.Delete);
            CopyMK = KeybindLoader.RegisterKeybind(Mod, "Copy (Ctrl + Key)", Keys.C);
            PasteMK = KeybindLoader.RegisterKeybind(Mod, "Paste (Ctrl + Key)", Keys.V);
            ChangeCornerOnPasteMK = KeybindLoader.RegisterKeybind(Mod, "Change Paste Corner (Ctrl + Key)", Keys.R);
            RotateMK = KeybindLoader.RegisterKeybind(Mod, "Rotate selection (Ctrl + Key)", Keys.T);
            MirrorMK = KeybindLoader.RegisterKeybind(Mod, "Mirror selection (Ctrl + Key)", Keys.M);
            CutMK = KeybindLoader.RegisterKeybind(Mod, "Cut (Ctrl + Key)", Keys.X);
            UndoMK = KeybindLoader.RegisterKeybind(Mod, "Undo (Ctrl + Key)", Keys.Z);
            RedoMK = KeybindLoader.RegisterKeybind(Mod, "Redo (Ctrl + Key)", Keys.Y);
            SaveMK = KeybindLoader.RegisterKeybind(Mod, "Save (Ctrl + Key)", Keys.S);
            FastMoveMK = KeybindLoader.RegisterKeybind(Mod, "Move faster in editor", Keys.LeftShift);
        }
    }
}
