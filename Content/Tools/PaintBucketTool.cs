using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaInGameWorldEditor.Common;
using TerrariaInGameWorldEditor.Common.Utils;
using TerrariaInGameWorldEditor.Editor;
using TerrariaInGameWorldEditor.UIElements.Button;

namespace TerrariaInGameWorldEditor.Content.Tools
{
    internal class PaintBucketTool : FillTool
    {
        public PaintBucketTool()
        {
            ToggleToolButton = new TIGWEButton(ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/Tools/PaintBucketTool", AssetRequestMode.ImmediateLoad));
            ToggleToolButton.HoverText = Language.GetText("Mods.TerrariaInGameWorldEditor.Tools.PaintBucketTool.HoverText");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Point16 point = new Point16(Player.tileTargetX, Player.tileTargetY);
            TileCollection tc = new TileCollection();
            tc.TryAddTile(point, new TileCopy(point.X, point.Y));
            DrawUtils.DrawTileCollectionOutline(tc, point.ToPoint(), EditorSystem.Local.Settings.ToolColor);
        }

        protected override void OnFill(TileCollection tiles)
        {
            base.OnFill(tiles);
            TileCollection toPaste = new TileCollection();
            foreach (var tile in tiles)
            {
                toPaste.TryAddTile(tile.Key, EditorSystem.Local.SelectedTile);
            }
            ToolUtils.Paste(toPaste, new Point16(toPaste.GetMinX(), toPaste.GetMinY()), true, EditorSystem.Local.Settings.ShouldUpdateDrawnTiles);
        }

        protected override void OnFillFailed()
        {
            TerrariaInGameWorldEditor.NewText(LocalizationUtils.GetTextValue("Tools.PaintBucketTool.Messages.AreaTooBig"));
            base.OnFillFailed();
        }

        protected override bool IsMatch(Point16 coords, TileCopy clickedTile)
        {
            if (!(((EditorSystem.Local.CurrentSelection?.ContainsCoord(coords)) ?? false) || EditorSystem.Local.CurrentSelection?.Count == 0))
            {
                return false;
            }
            return base.IsMatch(coords, clickedTile);
        }
    }
}
