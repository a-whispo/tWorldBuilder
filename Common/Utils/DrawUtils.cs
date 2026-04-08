using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace TerrariaInGameWorldEditor.Common.Utils
{
    internal static class DrawUtils
    {
        public static Texture2D TransparentTexture2D
        {
            get
            {
                if (_transparentTexture2D == null)
                {
                    _transparentTexture2D = new Texture2D(Main.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                    _transparentTexture2D.SetData(new[] { Color.White });
                }
                return _transparentTexture2D;
            }
        }
        private static Texture2D _transparentTexture2D;
        public static Asset<Texture2D> BlankTexture2D => _blankTexture2D ??= ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/Color");
        private static Asset<Texture2D> _blankTexture2D;
        public static SpriteBatch SpriteBatch => _spriteBatch ??= new SpriteBatch(Main.graphics.GraphicsDevice);
        private static SpriteBatch _spriteBatch;

        public static void DrawLine(Vector2 point1, Vector2 point2, int width = 4, Color color = default)
        {
            SpriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = (point2 - point1).Length();
            SpriteBatch.Draw(BlankTexture2D.Value, point1, null, color, angle, default, new Vector2(length, width), SpriteEffects.None, 0);
            SpriteBatch.End();
        }

        public static void DrawRectangleOutline(Rectangle rect, Color customColor)
        {
            Texture2D outlineTexture = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/TextureEmpty").Value;
            SpriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            // transparent part
            rect = new Rectangle(rect.X * 16 - (int)Main.screenPosition.X - 2, rect.Y * 16 - (int)Main.screenPosition.Y - 2, rect.Width * 16 + 4, rect.Height * 16 + 4);
            SpriteBatch.Draw(TransparentTexture2D, rect, Color.White * 0.2f);

            // calculate bounds
            int cornerSize = 8;
            int barSize = 16;
            Point topLeft = new Point(rect.X, rect.Y);
            Point bottomRight = new Point(topLeft.X + rect.Width - cornerSize, topLeft.Y + rect.Height - cornerSize);

            // middle part
            SpriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X + cornerSize, topLeft.Y + cornerSize, rect.Width - cornerSize * 2, rect.Height - cornerSize * 2), new Rectangle(cornerSize, cornerSize, barSize - 2, barSize - 2), customColor); // middle part

            // corners
            SpriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X, topLeft.Y, cornerSize, cornerSize), new Rectangle(0, 0, cornerSize, cornerSize), customColor); // top left
            SpriteBatch.Draw(outlineTexture, new Rectangle(bottomRight.X, topLeft.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, 0, cornerSize, cornerSize), customColor); // top right
            SpriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X, bottomRight.Y, cornerSize, cornerSize), new Rectangle(0, cornerSize + barSize, cornerSize, cornerSize), customColor); // bottom left
            SpriteBatch.Draw(outlineTexture, new Rectangle(bottomRight.X, bottomRight.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, cornerSize + barSize, cornerSize, cornerSize), customColor); // bottom right

            // top and bottom
            SpriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X + cornerSize, topLeft.Y, rect.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, 0, barSize, cornerSize), customColor); // top
            SpriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X + cornerSize, bottomRight.Y, rect.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, cornerSize + barSize, barSize, cornerSize), customColor); // bottom

            // left and right
            SpriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X, topLeft.Y + cornerSize, cornerSize, rect.Height - cornerSize * 2), new Rectangle(0, cornerSize, cornerSize, barSize), customColor); // left
            SpriteBatch.Draw(outlineTexture, new Rectangle(bottomRight.X, topLeft.Y + cornerSize, cornerSize, rect.Height - cornerSize * 2), new Rectangle(cornerSize + barSize, cornerSize, cornerSize, barSize), customColor); // right
            SpriteBatch.End();
        }

        public static void DrawTileCollectionOutline(TileCollection tileCollectionToDraw, Point coordToDrawAt, Color customColor)
        { 
            // make minx and miny into draw coordinates
            int minX = (int)(Main.screenPosition.X / 16) - coordToDrawAt.X;
            int maxX = minX + 1 + (int)(Main.screenWidth * Main.UIScale) / 16;
            int minY = (int)(Main.screenPosition.Y / 16) - coordToDrawAt.Y;
            int maxY = minY + 1 + (int)(Main.screenHeight * Main.UIScale) / 16;
            coordToDrawAt.X = coordToDrawAt.X * 16 - (int)Main.screenPosition.X;
            coordToDrawAt.Y = coordToDrawAt.Y * 16 - (int)Main.screenPosition.Y;

            // texture stuff
            Texture2D texture = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/TextureEmpty").Value;
            Texture2D textureInnerCorners = ModContent.Request<Texture2D>($"{TerrariaInGameWorldEditor.ASSET_PATH}/Assets/General/TextureEmptyInnerCorners").Value;

            // draw
            SpriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    Point16 tileCoord = new Point16(tileCollectionToDraw.GetMinX() + x, tileCollectionToDraw.GetMinY() + y);
                    if (!tileCollectionToDraw.ContainsCoord(tileCoord))
                    {
                        continue;
                    }
                    int drawX = x * 16 + coordToDrawAt.X;
                    int drawY = y * 16 + coordToDrawAt.Y;

                    // check for coords around the coord
                    bool hasTop = tileCollectionToDraw.ContainsCoord(new Point16(tileCoord.X, tileCoord.Y - 1));
                    bool hasBottom = tileCollectionToDraw.ContainsCoord(new Point16(tileCoord.X, tileCoord.Y + 1));
                    bool hasRight = tileCollectionToDraw.ContainsCoord(new Point16(tileCoord.X + 1, tileCoord.Y));
                    bool hasLeft = tileCollectionToDraw.ContainsCoord(new Point16(tileCoord.X - 1, tileCoord.Y));

                    SpriteBatch.Draw(TransparentTexture2D, new Rectangle(drawX, drawY, 16, 16), Color.White * 0.2f);

                    if (hasTop && hasBottom && hasRight && hasLeft)
                    {
                        continue;
                    }
                    int cornerSize = 6;
                    int barSize = 20;

                    // sides
                    if (!hasTop) // top
                    {
                        SpriteBatch.Draw(texture, new Rectangle(drawX, drawY - 2, 16, 6), new Rectangle(cornerSize, 0, barSize, cornerSize), customColor);
                    }
                    if (!hasBottom) // bottom
                    {
                        SpriteBatch.Draw(texture, new Rectangle(drawX, drawY + 16 - 4, 16, 6), new Rectangle(cornerSize, cornerSize + barSize, barSize, cornerSize), customColor);
                    }
                    if (!hasRight) // right
                    {
                        SpriteBatch.Draw(texture, new Rectangle(drawX + 16 - 4, drawY, 6, 16), new Rectangle(cornerSize + barSize, cornerSize, cornerSize, barSize), customColor); // right
                    }
                    if (!hasLeft) // left
                    {
                        SpriteBatch.Draw(texture, new Rectangle(drawX - 2, drawY, 6, 16), new Rectangle(0, cornerSize, cornerSize, barSize), customColor); // left
                    }

                    // outer corners
                    if (!hasTop && !hasLeft) // top left
                    {
                        SpriteBatch.Draw(texture, new Rectangle(drawX - 2, drawY - 2, 6, 6), new Rectangle(0, 0, cornerSize, cornerSize), customColor); // top left
                    }
                    if (!hasTop && !hasRight) // top right
                    {
                        SpriteBatch.Draw(texture, new Rectangle(drawX + 16 - 4, drawY - 2, 6, 6), new Rectangle(cornerSize + barSize, 0, cornerSize, cornerSize), customColor); // top right
                    }
                    if (!hasBottom && !hasRight) // bottom right
                    {
                        SpriteBatch.Draw(texture, new Rectangle(drawX + 16 - 4, drawY + 16 - 4, 6, 6), new Rectangle(cornerSize + barSize, cornerSize + barSize, cornerSize, cornerSize), customColor); // bottom right
                    }
                    if (!hasBottom && !hasLeft) // bottom left
                    {
                        SpriteBatch.Draw(texture, new Rectangle(drawX - 2, drawY + 16 - 4, 6, 6), new Rectangle(0, cornerSize + barSize, cornerSize, cornerSize), customColor); // bottom left
                    }

                    // inner corners
                    if (tileCollectionToDraw.ContainsCoord(new Point16(tileCoord.X + 1, tileCoord.Y - 1)) && !hasRight && hasTop) // top left
                    {
                        SpriteBatch.Draw(textureInnerCorners, new Rectangle(drawX + 16 - 4, drawY - 4, 6, 6), new Rectangle(0, 0, cornerSize, cornerSize), customColor); // top left
                    }
                    if (tileCollectionToDraw.ContainsCoord(new Point16(tileCoord.X - 1, tileCoord.Y - 1)) && !hasLeft && hasTop) // top right
                    {
                        SpriteBatch.Draw(textureInnerCorners, new Rectangle(drawX - 2, drawY - 4, 6, 6), new Rectangle(6, 0, cornerSize, cornerSize), customColor); // top right
                    }
                    if (tileCollectionToDraw.ContainsCoord(new Point16(tileCoord.X - 1, tileCoord.Y + 1)) && !hasLeft && hasBottom) // bottom right
                    {
                        SpriteBatch.Draw(textureInnerCorners, new Rectangle(drawX - 2, drawY + 16 - 2, 6, 6), new Rectangle(6, 6, cornerSize, cornerSize), customColor); // bottom right
                    }
                    if (tileCollectionToDraw.ContainsCoord(new Point16(tileCoord.X + 1, tileCoord.Y + 1)) && !hasRight && hasBottom) // bottom left
                    {
                        SpriteBatch.Draw(textureInnerCorners, new Rectangle(drawX + 16 - 4, drawY + 16 - 2, 6, 6), new Rectangle(0, 6, cornerSize, cornerSize), customColor); // bottom left
                    }
                }
            }
            SpriteBatch.End();
        }

        public static void DrawTileCollection(TileCollection tilesToDraw, Point coordToDrawAt, bool drawTiles = true, bool drawWalls = true, bool drawLiquid = true, bool drawWires = true)
        {
            // maybe find a better way to do this since recreating all the rendering stuff looks very ugly
            try
            {
                // make minx and miny into draw coordinates
                float opacity = 0.75f;
                int minX = (int)(Main.screenPosition.X / 16) - coordToDrawAt.X;
                int maxX = minX + 1 + (int)(Main.screenWidth * Main.UIScale) / 16;
                int minY = (int)(Main.screenPosition.Y / 16) - coordToDrawAt.Y;
                int maxY = minY + 1 + (int)(Main.screenHeight * Main.UIScale) / 16;
                coordToDrawAt.X = coordToDrawAt.X * 16 - (int)Main.screenPosition.X;
                coordToDrawAt.Y = coordToDrawAt.Y * 16 - (int)Main.screenPosition.Y;

                TilePaintSystemV2 ps = Main.instance.TilePaintSystem;
                SpriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                // draw walls first to avoid them clipping into other tiles
                if (drawWalls)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        for (int y = minY; y < maxY; y++)
                        {
                            Point16 tileCoord = new Point16(tilesToDraw.GetMinX() + x, tilesToDraw.GetMinY() + y);
                            if (!tilesToDraw.TryGetTile(tileCoord, out TileCopy tile) || tile.WallType == 0)
                            {
                                continue;
                            }
                            int drawX = x * 16 + coordToDrawAt.X;
                            int drawY = y * 16 + coordToDrawAt.Y;

                            Main.instance.LoadWall(tile.WallType);
                            Vector2 wallPos = new Vector2(drawX - 8, drawY - 8);
                            if (ps.TryGetWallAndRequestIfNotReady(tile.WallType, tile.WallColor) is Texture2D tileWallTex)
                            {
                                SpriteBatch.Draw(tileWallTex, wallPos, new Rectangle(tile.WallFrameX, tile.WallFrameY, 32, 32), Color.White * opacity);
                            }
                        }
                    }
                }

                // go over all the tiles
                for (int x = minX; x < maxX; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        Point16 tileCoord = new Point16(tilesToDraw.GetMinX() + x, tilesToDraw.GetMinY() + y);
                        tilesToDraw.TryGetTile(tileCoord, out TileCopy tile);
                        tilesToDraw.TryGetTile(new Point16(tileCoord.X, tileCoord.Y - 1), out TileCopy topTile);
                        tilesToDraw.TryGetTile(new Point16(tileCoord.X, tileCoord.Y + 1), out TileCopy downTile);
                        tilesToDraw.TryGetTile(new Point16(tileCoord.X - 1, tileCoord.Y), out TileCopy leftTile);
                        tilesToDraw.TryGetTile(new Point16(tileCoord.X + 1, tileCoord.Y), out TileCopy rightTile);
                        if (tile == null)
                        {
                            continue;
                        }
                        int drawX = x * 16 + coordToDrawAt.X;
                        int drawY = y * 16 + coordToDrawAt.Y;
                        Color tileColor = tile.IsActuated ? new Color(120, 120, 120) : Color.White;

                        // pre tile liquid rendering
                        if (drawLiquid && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) && tile.HasTile)
                        {
                            byte liquidAmount = 0;
                            int liquidType = 0;
                            if (topTile != null && topTile.LiquidAmount != 0)
                            {
                                liquidAmount = 255;
                                liquidType = topTile.LiquidType;
                            }
                            else if (rightTile != null && rightTile.LiquidAmount != 0)
                            {
                                liquidAmount = rightTile.LiquidAmount;
                                liquidType = rightTile.LiquidType;
                            }
                            else if (leftTile != null && leftTile.LiquidAmount != 0)
                            {
                                liquidAmount = leftTile.LiquidAmount;
                                liquidType = leftTile.LiquidType;
                            }
                            else
                            {
                                liquidAmount = tile.LiquidAmount;
                                liquidType = tile.LiquidType;
                            }

                            if (liquidAmount != 0)
                            {
                                int num = 4;
                                // determite if the surface part of the liquid should be shown
                                if (topTile == null || (topTile.LiquidAmount == 0 && !topTile.HasTile))
                                {
                                    num = 0;
                                }
                                int height = (int)(((float)liquidAmount / 255) * 16);
                                height = Math.Clamp(height, 6, 16);
                                SpriteBatch.Draw(TextureAssets.Liquid[liquidType].Value, new Vector2(drawX, drawY + (16 - height)), new Rectangle(0, num, 16, height), Color.White * opacity);
                            }
                        }

                        if (drawTiles)
                        {
                            Main.instance.LoadTiles(tile.TileType);

                            // treetops
                            if (tile.IsTreeTop)
                            {
                                if (tile.TileType == 72) // underground shroomcaps
                                {
                                    // some calculations and stuff i took from the source code
                                    Vector2 tilePos = new Vector2(drawX - 22, drawY - 26);
                                    Texture2D top = TextureAssets.ShroomCap.Value;
                                    int num = 0;
                                    if (tile.TileFrameY == 18)
                                    {
                                        num = 1;
                                    }
                                    else if (tile.TileFrameY == 36)
                                    {
                                        num = 2;
                                    }
                                    SpriteBatch.Draw(top, tilePos, new Rectangle(num * 62, 0, 60, 42), Color.White * opacity);
                                }
                                else if (tile.TileType == 323) // palm treetops
                                {
                                    // some calculations and stuff i took from the source code
                                    int treeTextureIndex = 15;
                                    int width = 80;
                                    int height = 80;
                                    int num1 = 32;
                                    int num2 = 0;
                                    int y2 = tile.TreeBiome * 82;
                                    if (tile.TreeBiome >= 4 && tile.TreeBiome <= 7)
                                    {
                                        treeTextureIndex = 21;
                                        width = 114;
                                        height = 98;
                                        num1 = 48;
                                        num2 = 2;
                                        y2 = (tile.TreeBiome - 4) * 98;
                                    }
                                    Texture2D top = ps.TryGetTreeTopAndRequestIfNotReady(treeTextureIndex, tile.TreeBiome, tile.TileColor);
                                    SpriteBatch.Draw(top, new Vector2(drawX - num1 + tile.TileFrameY, drawY + 16 + num2 - height), new Rectangle(tile.TreeFrame * (tile.TreeFrameWidth + 2), tile.TreeFrame * (tile.TreeFrameHeight + 2) + y2, width, height), Color.White * opacity);
                                }
                                else // rest of the treetops
                                { 
                                    if (ps.TryGetTreeTopAndRequestIfNotReady(tile.TreeStyle, 0, tile.TileColor) is Texture2D top)
                                    {
                                        SpriteBatch.Draw(top, new Vector2(drawX - tile.TreeFrameWidth / 2 + 8, drawY - tile.TreeFrameHeight + 16), new Rectangle(tile.TreeFrame * (tile.TreeFrameWidth + 2), 0, tile.TreeFrameWidth, tile.TreeFrameHeight), Color.White * opacity);
                                    }
                                }
                            }

                            // tree branches
                            if (tile.IsTreeBranch)
                            {
                                // texture dimensions are kind of hardcoded so this might not work with modded trees
                                // (40 is the width and height of the branch sprite)
                                if (ps.TryGetTreeBranchAndRequestIfNotReady(tile.TreeStyle, 0, tile.TileColor) is Texture2D branch)
                                {
                                    SpriteBatch.Draw(branch, new Rectangle(drawX - (tile.IsFlipped ? 24 : 0), drawY - (40 / 2) + (16 / 2), 40, 40), new Rectangle(42, tile.TreeFrame * 42, 40, 40), tileColor * opacity, default, default, (tile.IsFlipped ? SpriteEffects.FlipHorizontally : default), default);
                                }
                            }

                            // tile
                            if (!tile.IsTreeTop && !tile.IsTreeBranch && tile.HasTile)
                            {
                                Texture2D tileTex = ps.TryGetTileAndRequestIfNotReady(tile.TileType, tile.TileFrameNumber, tile.TileColor);

                                // this draws general trees, underground mushroom trees should be drawn like normal tiles tho
                                if (tileTex != null && tile.IsTreeTrunk && tile.TileType != 72)
                                {
                                    // width for tree trunks should be 20 for the whole sprite
                                    SpriteBatch.Draw(tileTex, new Vector2(drawX - 2, drawY), new Rectangle((176 * tile.TreeVariant) + tile.TileFrameX, tile.TileFrameY, 20, 16), tileColor * opacity);
                                }
                                else if (tile.TileType == 323) // this draws palm trees
                                {
                                    // width for tree trunks should be 20 for the whole sprite
                                    Vector2 tilePos = new Vector2(drawX - 2, drawY);

                                    // this determines the palm tree curve
                                    if (!(tile.TileFrameX <= 132 && tile.TileFrameX >= 88))
                                    {
                                        tilePos.X += tile.TileFrameY;
                                    }
                                    if (ps.TryGetTileAndRequestIfNotReady(tile.TileType, tile.TreeBiome, tile.TileColor) is Texture2D palmTexture)
                                    {
                                        SpriteBatch.Draw(palmTexture, tilePos, new Rectangle(tile.TileFrameX, 22 * tile.TreeBiome, 20, 16), tileColor * opacity);
                                    }
                                }
                                else if (tileTex != null)// draw normal tiles
                                {
                                    if (tile.IsHalfBlock)
                                    {
                                        Vector2 tilePos = new Vector2(drawX, drawY + 8);
                                        bool hasTileBelow = tilesToDraw.TryGetTile(new Point16(tileCoord.X, tileCoord.Y + 1), out TileCopy belowTile) && belowTile.HasTile && !belowTile.IsHalfBlock;
                                        SpriteBatch.Draw(tileTex, tilePos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, hasTileBelow ? 8 : 4), tileColor * opacity);
                                        if (!hasTileBelow)
                                        {
                                            tilePos.Y += 4;
                                            SpriteBatch.Draw(tileTex, tilePos, new Rectangle(8 * 18, 3 * 18 + 12, 16, 4), tileColor * opacity); // the bottom part of half block tiles
                                        }
                                    }
                                    else if (tile.Slope != 0)
                                    {
                                        for (int index = 0; index < 8; ++index)
                                        {
                                            switch ((int)tile.Slope)
                                            {
                                                case 1:
                                                    SpriteBatch.Draw(tileTex, new Vector2(drawX + index * 2, drawY + index * 2), new Rectangle(2 * 18 + index * 2, 0 * 18, 2, 16 - index * 2), tileColor * opacity);
                                                    break;
                                                case 2:
                                                    SpriteBatch.Draw(tileTex, new Vector2(drawX + index * 2, drawY + 16 - (index + 1) * 2), new Rectangle(2 * 18 + index * 2, 0 * 18, 2, (index + 1) * 2), tileColor * opacity);
                                                    break;
                                                case 3:
                                                    SpriteBatch.Draw(tileTex, new Vector2(drawX + index * 2, drawY), new Rectangle(1 * 18 + index * 2, 2 * 18 + 2 * index, 2, 16 - index * 2), tileColor * opacity);
                                                    break;
                                                case 4:
                                                    SpriteBatch.Draw(tileTex, new Vector2(drawX + index * 2, drawY), new Rectangle(1 * 18 + index * 2, 2 * 18 - index * 2 + 16, 2, (index + 1) * 2), tileColor * opacity);
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // cases when theres a half block to the right/left of the tile, this still doesnt look right so maybe come back to it later
                                        bool hasLeftTile = leftTile != null && leftTile.IsHalfBlock;
                                        bool hasRightTile = rightTile != null && rightTile.IsHalfBlock;
                                        if (hasLeftTile)
                                        {
                                            SpriteBatch.Draw(tileTex, new Vector2(drawX, drawY), new Rectangle(7 * 18, 0 * 18, 4, 8), tileColor * opacity);
                                        }
                                        if (hasRightTile)
                                        {
                                            SpriteBatch.Draw(tileTex, new Vector2(drawX + 12, drawY), new Rectangle(7 * 18 + 12, 0 * 18, 4, 8), tileColor * opacity);
                                        }
                                        SpriteBatch.Draw(tileTex, new Vector2(drawX + (hasLeftTile ? 4 : 0), drawY), new Rectangle(tile.TileFrameX + (hasLeftTile ? 4 : 0), tile.TileFrameY, 16 - (hasLeftTile ? 4 : 0) - (hasRightTile ? 4 : 0), 8), tileColor * opacity);
                                        SpriteBatch.Draw(tileTex, new Vector2(drawX, drawY + 8), new Rectangle(tile.TileFrameX, tile.TileFrameY + 8, 16, 8), tileColor * opacity);
                                    }
                                }
                            }
                        }

                        // liquid
                        if (drawLiquid && tile.LiquidAmount > 0)
                        {
                            int num = 4;
                            if (topTile != null && (tile.LiquidAmount < 255 || !topTile.HasTile) && topTile.LiquidAmount == 0) // determite if the surface part of the liquid should be shown
                            {
                                num = 0;
                            }
                            int height = (int)(((float)tile.LiquidAmount / 255) * 16);
                            height = Math.Clamp(height, 6, 16);
                            SpriteBatch.Draw(TextureAssets.Liquid[tile.LiquidType].Value, new Vector2(drawX, drawY + (16 - height)), new Rectangle(0, num, 16, height), Color.White * opacity);
                        }

                        // wires
                        if (drawWires && (tile.GreenWire || tile.RedWire || tile.YellowWire || tile.BlueWire))
                        {
                            // this whole thing is kinda silly and isnt super accurate when it comes to mixed wire colors
                            // rewrite this at some point probably
                            Vector2 tilePos = new Vector2(drawX, drawY);

                            bool HasMatchingWire(TileCopy tile1, TileCopy tile2) => (tile1.GreenWire && tile2.GreenWire) || (tile1.RedWire && tile2.RedWire) || (tile1.YellowWire && tile2.YellowWire) || (tile1.BlueWire && tile2.BlueWire);
                            bool topWire = topTile != null && HasMatchingWire(tile, topTile) && topTile.HasWire;
                            bool bottomWire = downTile != null && HasMatchingWire(tile, downTile) && downTile.HasWire;
                            bool rightWire = rightTile != null && HasMatchingWire(tile, rightTile) && rightTile.HasWire;
                            bool leftWire = leftTile != null && HasMatchingWire(tile, leftTile) && leftTile.HasWire;
                            
                            void DrawWire(Texture2D texture, bool topWire, bool bottomWire, bool rightWire, bool leftWire, float t)
                            {
                                switch (topWire, bottomWire, rightWire, leftWire)
                                {
                                    case (false, false, false, false):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(0, 54, 16, 16), Color.White * t * opacity); // none
                                        break;
                                    case (false, false, false, true):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(54, 36, 16, 16), Color.White * t * opacity); // left
                                        break;
                                    case (false, false, true, false):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(72, 36, 16, 16), Color.White * t * opacity); // right
                                        break;
                                    case (false, false, true, true):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(18, 0, 16, 16), Color.White * t * opacity); // left right
                                        break;
                                    case (false, true, false, false):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(18, 36, 16, 16), Color.White * t * opacity); // bottom
                                        break;
                                    case (false, true, false, true):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(72, 18, 16, 16), Color.White * t * opacity); // bottom left
                                        break;
                                    case (false, true, true, false):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(0, 36, 16, 16), Color.White * t * opacity); // bottom right
                                        break;
                                    case (false, true, true, true):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(72, 0, 16, 16), Color.White * t * opacity); // bottom left right
                                        break;
                                    case (true, false, false, false):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(36, 36, 16, 16), Color.White * t * opacity); // top
                                        break;
                                    case (true, false, false, true):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(54, 18, 16, 16), Color.White * t * opacity); // top left
                                        break;
                                    case (true, false, true, false):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(36, 18, 16, 16), Color.White * t * opacity); // top right
                                        break;
                                    case (true, false, true, true):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(0, 18, 16, 16), Color.White * t * opacity); // top left right
                                        break;
                                    case (true, true, false, false):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(0, 0, 16, 16), Color.White * t * opacity); // top bottom
                                        break;
                                    case (true, true, false, true):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(54, 0, 16, 16), Color.White * t * opacity); // top bottom left
                                        break;
                                    case (true, true, true, false):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(36, 0, 16, 16), Color.White * t * opacity); // top bottom right
                                        break;
                                    case (true, true, true, true):
                                        SpriteBatch.Draw(texture, tilePos, new Rectangle(18, 18, 16, 16), Color.White * t * opacity); // top bottom right left
                                        break;
                                }
                            }

                            // draw the wires
                            if (tile.RedWire)
                            {
                                DrawWire(TextureAssets.Wire.Value, topWire, bottomWire, rightWire, leftWire, 1f);
                            }
                            if (tile.BlueWire)
                            {
                                DrawWire(TextureAssets.Wire2.Value, topWire, bottomWire, rightWire, leftWire, 0.6f);
                            }
                            if (tile.GreenWire)
                            {
                                DrawWire(TextureAssets.Wire3.Value, topWire, bottomWire, rightWire, leftWire, 0.6f);
                            }
                            if (tile.YellowWire)
                            {
                                DrawWire(TextureAssets.Wire4.Value, topWire, bottomWire, rightWire, leftWire, 0.6f);
                            }
                        }

                        // actuator
                        if (drawWires && tile.HasActuator)
                        {
                            Texture2D texture = TextureAssets.Actuator.Value;
                            SpriteBatch.Draw(texture, new Vector2(drawX, drawY), Color.White * opacity);
                        }
                    }
                }
                SpriteBatch.End();
            } 
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.Error("Unknown error trying to draw TileCollection.", ex);
                SpriteBatch.End();
            }
        }

        public static void DrawTileCopyInUI(TileCopy tile, Rectangle dimensions, float opacity = 1f)
        {
            try
            {
                if (tile == null)
                {
                    return;
                }

                SpriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);
                TilePaintSystemV2 ps = Main.instance.TilePaintSystem;
                int tileSide = dimensions.Height;
                int x = dimensions.X;
                int y = dimensions.Y;
                Color tileColor = tile.IsActuated ? new Color(120, 120, 120) : Color.White;

                // wall part
                if (tile.WallType != 0)
                {
                    // get wall texture with tilepaintsystem to get the texture with paint if it has any
                    Main.instance.LoadWall(tile.WallType);
                    Texture2D tileWallTex = ps.TryGetWallAndRequestIfNotReady(tile.WallType, tile.WallColor);
                    if (tileWallTex != null)
                    {
                        SpriteBatch.Draw(tileWallTex, new Rectangle(x - (tileSide / 2), y - (tileSide / 2), tileSide * 2, tileSide * 2), new Rectangle(tile.WallFrameX, tile.WallFrameY, 16 * 2, 16 * 2), Color.White * opacity);
                    }
                }

                // tile part
                if (tile.HasTile)
                {
                    if (tile.TileType != 696 && tile.TileType != 697 && tile.TileType != 698)
                    {
                        Main.instance.LoadTiles(tile.TileType);
                    }
                    Texture2D tileTex = ps.TryGetTileAndRequestIfNotReady(tile.TileType, tile.TileFrameNumber, tile.TileColor);

                    if (tileTex != null)
                    {
                        if (tile.IsHalfBlock)
                        {
                            Rectangle tilePos = new Rectangle(x, y + tileSide / 2, tileSide, tileSide / 4);
                            SpriteBatch.Draw(tileTex, tilePos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 4), tileColor * opacity);
                            tilePos.Y += tileSide / 4;
                            tilePos.Height = tileSide / 4; ;
                            SpriteBatch.Draw(tileTex, tilePos, new Rectangle(8 * 18, 3 * 18 + 12, 16, 4), tileColor * opacity); // the bottom part of half block tiles
                        }
                        else if (tile.Slope != 0)
                        {
                            for (int index = 0; index < 8; ++index)
                            {
                                switch ((int)tile.Slope)
                                {
                                    case 1:
                                        SpriteBatch.Draw(tileTex, new Rectangle(x + index * (tileSide / 8), y + index * (tileSide / 8), (tileSide / 8), tileSide - index * (tileSide / 8)), new Rectangle(2 * 18 + index * 2, 0 * 18, 2, 16 - index * 2), tileColor * opacity);
                                        break;
                                    case 2:
                                        SpriteBatch.Draw(tileTex, new Rectangle(x + index * (tileSide / 8), y + tileSide - (index + 1) * (tileSide / 8), (tileSide / 8), (index + 1) * (tileSide / 8)), new Rectangle(2 * 18 + index * 2, 0 * 18, 2, (index + 1) * 2), tileColor * opacity);
                                        break;
                                    case 3:
                                        SpriteBatch.Draw(tileTex, new Rectangle(x + index * (tileSide / 8), y, (tileSide / 8), tileSide - index * (tileSide / 8)), new Rectangle(2 * 18 + index * 2, 0 * 18 + 2 * index, 2, 16 - index * 2), tileColor * opacity);
                                        break;
                                    case 4:
                                        SpriteBatch.Draw(tileTex, new Rectangle(x + index * (tileSide / 8), y, (tileSide / 8), (index + 1) * (tileSide / 8)), new Rectangle(2 * 18 + index * 2, 0 * 18 - index * 2 + 16, 2, (index + 1) * 2), tileColor * opacity);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            SpriteBatch.Draw(tileTex, new Rectangle(x, y, tileSide, tileSide), new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), tileColor * opacity);
                        }
                    }
                }

                // liquid
                if (tile.LiquidAmount > 0)
                {
                    int num = 0;
                    int height = (int)(((float)tile.LiquidAmount / 255) * 16);
                    height = Math.Clamp(height, 6, 16);
                    SpriteBatch.Draw(TextureAssets.Liquid[tile.LiquidType].Value, new Rectangle(x, y - (int)(tileSide * (float)(height / 16f) - tileSide), tileSide, (int)(tileSide * (float)(height / 16f))), new Rectangle(0, num, 16, height), Color.White * 0.6f * opacity);
                }

                // wire
                if (tile.HasWire)
                {
                    Rectangle boundsTile = new Rectangle(x, y, tileSide, tileSide);

                    // draw the wires
                    if (tile.RedWire)
                    {
                        SpriteBatch.Draw(TextureAssets.Wire.Value, boundsTile, new Rectangle(0, 54, 16, 16), Color.White * 0.65f * opacity);
                    }
                    if (tile.BlueWire)
                    {
                        SpriteBatch.Draw(TextureAssets.Wire2.Value, boundsTile, new Rectangle(0, 54, 16, 16), Color.White * 0.4f * opacity);
                    }
                    if (tile.GreenWire)
                    {
                        SpriteBatch.Draw(TextureAssets.Wire3.Value, boundsTile, new Rectangle(0, 54, 16, 16), Color.White * 0.4f * opacity);
                    }
                    if (tile.YellowWire)
                    {
                        SpriteBatch.Draw(TextureAssets.Wire4.Value, boundsTile, new Rectangle(0, 54, 16, 16), Color.White * 0.4f * opacity);
                    }
                }

                // actuator
                if (tile.HasActuator)
                {
                    Rectangle boundsTile = new Rectangle(x, y, tileSide, tileSide);

                    Texture2D texture = TextureAssets.Actuator.Value;
                    SpriteBatch.Draw(texture, boundsTile, Color.White * 0.6f * opacity);
                }
                SpriteBatch.End();
            }
            catch (Exception ex)
            {
                TerrariaInGameWorldEditor.Error("Unknown error trying to draw TileCopy in UI.", ex);
                SpriteBatch.End();
            }
        }

        public static void DrawMiscOptions(Rectangle bounds, bool drawCenterLines, bool drawMeasureLines)
        {
            // rewrite all of this at some point it all sucks
            SpriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            bounds = new Rectangle(bounds.X * 16 - (int)Main.screenPosition.X, bounds.Y * 16 - (int)Main.screenPosition.Y, (bounds.Width + 1) * 16, (bounds.Height + 1) * 16);

            // add center lines
            if (drawCenterLines)
            {
                Color color = new Color(45, 43, 46);
                SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle(bounds.X + 2, bounds.Y + (bounds.Height / 2) - 1, bounds.Width - 4, 2), color); // horizontal line
                SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle(bounds.X + (bounds.Width / 2) - 1, bounds.Y + 2, 2, bounds.Height - 4), color); // vertical line
            }

            // measuring lines
            if (drawMeasureLines)
            {
                Color color = new Color(215, 215, 215);
                bounds.X = (bounds.X + (int)Main.screenPosition.X) / 16;
                bounds.Y = (bounds.Y + (int)Main.screenPosition.Y) / 16;
                bounds.Width = bounds.Width / 16;
                bounds.Height = bounds.Height / 16;

                int distanceLeft = GetDistanceHorizontal(bounds, 0, -1, 0, 1);
                int distanceRight = GetDistanceHorizontal(bounds, bounds.Width - 1, 1, 0, 1);
                int distanceTop = GetDistanceVertical(bounds, 0, 1, 0, -1);
                int distanceBottom = GetDistanceVertical(bounds, 0, 1, bounds.Height - 1, 1);
                if (distanceLeft != -1)
                {
                    SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle((bounds.X * 16 - distanceLeft * 16) - (int)Main.screenPosition.X, (bounds.Y * 16) - (int)Main.screenPosition.Y, 2, bounds.Height * 16), color); // vertical line
                    SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle((bounds.X * 16 - distanceLeft * 16) - (int)Main.screenPosition.X, (bounds.Y * 16) + ((bounds.Height * 16) / 2) - (int)Main.screenPosition.Y - 1, distanceLeft * 16 - 2, 2), color); // horizontal line
                }
                if (distanceRight != -1)
                {
                    SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle((bounds.X * 16 + bounds.Width * 16 + distanceRight * 16) - (int)Main.screenPosition.X - 2, (bounds.Y * 16) - (int)Main.screenPosition.Y, 2, bounds.Height * 16), color); // vertical line
                    SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle((bounds.X * 16 + bounds.Width * 16) - (int)Main.screenPosition.X + 2, (bounds.Y * 16) + ((bounds.Height * 16) / 2) - (int)Main.screenPosition.Y - 1, distanceRight * 16 - 2, 2), color); // horizontal line
                }
                if (distanceTop != -1)
                {
                    SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle((bounds.X * 16) + (bounds.Width * 16) / 2 - (int)Main.screenPosition.X - 1, (bounds.Y * 16 - distanceTop * 16) - (int)Main.screenPosition.Y, 2, distanceTop * 16 - 2), color); // vertical line
                    SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle((bounds.X * 16) - (int)Main.screenPosition.X, (bounds.Y * 16 - distanceTop * 16) - (int)Main.screenPosition.Y, bounds.Width * 16, 2), color); // horizontal line
                }
                if (distanceBottom != -1)
                {
                    SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle((bounds.X * 16) + (bounds.Width * 16) / 2 - (int)Main.screenPosition.X - 1, (bounds.Y * 16 + bounds.Height * 16) - (int)Main.screenPosition.Y + 2, 2, distanceBottom * 16 - 2), color); // vertical line
                    SpriteBatch.Draw(BlankTexture2D.Value, new Rectangle((bounds.X * 16) - (int)Main.screenPosition.X, (bounds.Y * 16 + distanceBottom * 16 + bounds.Height * 16) - (int)Main.screenPosition.Y - 2, bounds.Width * 16, 2), color); // horizontal line
                }
                Main.instance.MouseText($"Top distance: {(distanceTop > 100 ? ">100" : Math.Max(0, distanceTop))} \nRight distance: {(distanceRight > 100 ? ">100" : Math.Max(0, distanceRight))} \nBottom distance: {(distanceBottom > 100 ? ">100" : Math.Max(0, distanceBottom))} \nLeft distance: {(distanceLeft > 100 ? ">100" : Math.Max(0, distanceLeft))}\n");
            }
            SpriteBatch.End();
        }

        public static int GetDistanceHorizontal(Rectangle bounds, int xOffset, int xMult, int yOffset, int yMult)
        {
            int distance = 101;
            for (int x = 0; x < 101; x++)
            {
                for (int y = 0; y < bounds.Height; y++)
                {
                    int tx = bounds.X - x;
                    int ty = bounds.Y + y;
                    if (tx > Main.maxTilesX || tx < 0 || ty > Main.maxTilesY || ty < 0)
                    {
                        continue;
                    }
                    if (Main.tile[bounds.X + xOffset + x * xMult, bounds.Y + yOffset + y * yMult].HasTile)
                    {
                        if ((x - 1) < distance)
                        {
                            distance = x - 1;
                        }
                        return distance;
                    }
                }
            }
            return distance;
        }

        public static int GetDistanceVertical(Rectangle bounds, int xOffset, int xMult, int yOffset, int yMult)
        {
            int distance = 101;
            for (int y = 0; y < 101; y++)
            {
                for (int x = 0; x < bounds.Width; x++)
                {
                    int tx = bounds.X + x;
                    int ty = bounds.Y - y;
                    if (tx > Main.maxTilesX || tx < 0 || ty > Main.maxTilesY || ty < 0)
                    {
                        continue;
                    }
                    if (Main.tile[bounds.X + xOffset + x * xMult, bounds.Y + yOffset + y * yMult].HasTile)
                    {
                        if ((y - 1) < distance)
                        {
                            distance = y - 1;
                        }
                        return distance;
                    }
                }
            }
            return distance;
        }
    }
}
