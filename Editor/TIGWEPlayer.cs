using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Content;

namespace TerrariaInGameWorldEditor.Editor
{
    internal class TIGWEPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            base.OnEnterWorld();
            if (Keybinds.OpenEditorMK.GetAssignedKeys().Count == 0)
            {
                TerrariaInGameWorldEditor.NewText("Looks like you're missing a keybind for tWorldBuilder, set up some keybinds in the settings to start using the mod.");
            }
        }
    }
}
