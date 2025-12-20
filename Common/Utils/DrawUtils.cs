using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TerrariaInGameWorldEditor.Common.Utils
{
    internal static class DrawUtils
    {
        // spritebatch
        private static SpriteBatch _spriteBatch { get; set; } = new SpriteBatch(Main.graphics.GraphicsDevice);

        // textures
        public static Texture2D TransparentTexture2D { get; set; } = new Texture2D(Main.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        public static Texture2D BlankTexture2D => (Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color");

        public static void DrawLine(Vector2 point1, Vector2 point2, int width = 4, Color color = default)
        {
            // spritebatch stuff
            _spriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = (point2 - point1).Length();

            _spriteBatch.Draw(BlankTexture2D, point1, null, color, angle, default, new Vector2(length, width), SpriteEffects.None, 0);
            _spriteBatch.End();
        }

        public static void DrawTextAtMouse(string text)
        {
            if (Main.LocalPlayer.mouseInterface == false)
            {
                Main.instance.MouseText(text);
            }
        }

        public static void DrawRectangleOutline(Rectangle rect, Color customColor)
        {
            rect = new Rectangle(rect.X * 16 - (int)Main.screenPosition.X - 2, rect.Y * 16 - (int)Main.screenPosition.Y - 2, rect.Width * 16 + 4, rect.Height * 16 + 4);
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);

            // start spritebatch and set color + texture
            _spriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            Color[] c = new Color[1];
            c[0] = Color.FromNonPremultiplied(255, 255, 255, 50); // add transparency
            TransparentTexture2D.SetData<Color>(c);
            Color color = new Color(150, 150, 150); // transparent selection color

            // draw rectangle
            _spriteBatch.Draw(TransparentTexture2D, rect, color);

            // draw outline
            Texture2D outlineTexture = (Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TextureEmpty");

            // calculate bounds
            int cornerSize = 8;
            int barSize = 16;
            Point topLeft = new Point(rect.X, rect.Y);
            Point bottomRight = new Point(topLeft.X + rect.Width - cornerSize, topLeft.Y + rect.Height - cornerSize);

            // middle part
            _spriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X + cornerSize, topLeft.Y + cornerSize, rect.Width - cornerSize * 2, rect.Height - cornerSize * 2), new Rectangle(cornerSize, cornerSize, barSize - 2, barSize - 2), customColor); // middle part

            // corners
            _spriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X, topLeft.Y, cornerSize, cornerSize), new Rectangle(0, 0, cornerSize, cornerSize), customColor); // top left
            _spriteBatch.Draw(outlineTexture, new Rectangle(bottomRight.X, topLeft.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, 0, cornerSize, cornerSize), customColor); // top right
            _spriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X, bottomRight.Y, cornerSize, cornerSize), new Rectangle(0, cornerSize + barSize, cornerSize, cornerSize), customColor); // bottom left
            _spriteBatch.Draw(outlineTexture, new Rectangle(bottomRight.X, bottomRight.Y, cornerSize, cornerSize), new Rectangle(cornerSize + barSize, cornerSize + barSize, cornerSize, cornerSize), customColor); // bottom right

            // top and bottom
            _spriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X + cornerSize, topLeft.Y, rect.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, 0, barSize, cornerSize), customColor); // top
            _spriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X + cornerSize, bottomRight.Y, rect.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, cornerSize + barSize, barSize, cornerSize), customColor); // bottom

            // left and right
            _spriteBatch.Draw(outlineTexture, new Rectangle(topLeft.X, topLeft.Y + cornerSize, cornerSize, rect.Height - cornerSize * 2), new Rectangle(0, cornerSize, cornerSize, barSize), customColor); // left
            _spriteBatch.Draw(outlineTexture, new Rectangle(bottomRight.X, topLeft.Y + cornerSize, cornerSize, rect.Height - cornerSize * 2), new Rectangle(cornerSize + barSize, cornerSize, cornerSize, barSize), customColor); // right

            // end spritebatch and dispose of texture
            _spriteBatch.End();
        }

        public static void DrawTileCollectionOutline(TileCollection tc, Point coordToDrawAt, Color customColor)
        {
            // get minx and miny
            int minX = tc.GetMinX();
            int minY = tc.GetMinY();

            // make into draw coordinates
            coordToDrawAt.X = coordToDrawAt.X * 16 - (int)Main.screenPosition.X;
            coordToDrawAt.Y = coordToDrawAt.Y * 16 - (int)Main.screenPosition.Y;

            // spritebatch stuff
            _spriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            Texture2D texture = (ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TextureEmpty")).Value;
            Texture2D textureInnerCorners = (ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/TextureEmptyInnerCorners")).Value;

            // draw transparent layer
            Color[] c = new Color[1];
            c[0] = Color.FromNonPremultiplied(255, 255, 255, 50); // add transparency
            TransparentTexture2D.SetData<Color>(c);
            Color transparentColor = new Color(150, 150, 150); // transparent selection color

            // go over all the coords in the list
            foreach (var coord in tc.ToListOfPoints())
            {
                // normalize and get variables needed for drawing
                int drawX = (coord.X - minX) * 16 + coordToDrawAt.X;
                int drawY = (coord.Y - minY) * 16 + coordToDrawAt.Y;

                // check if its worth doing calculations
                if (drawX > Main.screenWidth * Main.UIScale || drawX < 0 || drawY > Main.screenHeight * Main.UIScale || drawY < 0)
                {
                    continue;
                }

                // get x and y
                int tileX = coord.X;
                int tileY = coord.Y;

                var tcDict = tc.AsDictionary();

                // check for coords around the coord
                bool hasTop = tcDict.ContainsKey(new Point(tileX, tileY - 1));
                bool hasBottom = tcDict.ContainsKey(new Point(tileX, tileY + 1));
                bool hasRight = tcDict.ContainsKey(new Point(tileX + 1, tileY));
                bool hasLeft = tcDict.ContainsKey(new Point(tileX - 1, tileY));

                // draw rectangle
                _spriteBatch.Draw(TransparentTexture2D, new Rectangle(drawX, drawY, 16, 16), transparentColor);

                if (hasTop && hasBottom && hasRight && hasLeft)
                {
                    continue;
                }

                // corner and barsize
                int cornerSize = 6;
                int barSize = 20;

                // sides
                if (!hasTop) // top
                {
                    _spriteBatch.Draw(texture, new Rectangle(drawX, drawY - 2, 16, 6), new Rectangle(cornerSize, 0, barSize, cornerSize), customColor);
                }
                if (!hasBottom) // bottom
                {
                    _spriteBatch.Draw(texture, new Rectangle(drawX, drawY + 16 - 4, 16, 6), new Rectangle(cornerSize, cornerSize + barSize, barSize, cornerSize), customColor);
                }
                if (!hasRight) // right
                {
                    _spriteBatch.Draw(texture, new Rectangle(drawX + 16 - 4, drawY, 6, 16), new Rectangle(cornerSize + barSize, cornerSize, cornerSize, barSize), customColor); // right
                }
                if (!hasLeft) // left
                {
                    _spriteBatch.Draw(texture, new Rectangle(drawX - 2, drawY, 6, 16), new Rectangle(0, cornerSize, cornerSize, barSize), customColor); // left
                }

                // outer corners
                if (!hasTop && !hasLeft) // top left
                {
                    _spriteBatch.Draw(texture, new Rectangle(drawX - 2, drawY - 2, 6, 6), new Rectangle(0, 0, cornerSize, cornerSize), customColor); // top left
                }
                if (!hasTop && !hasRight) // top right
                {
                    _spriteBatch.Draw(texture, new Rectangle(drawX + 16 - 4, drawY - 2, 6, 6), new Rectangle(cornerSize + barSize, 0, cornerSize, cornerSize), customColor); // top right
                }
                if (!hasBottom && !hasRight) // bottom right
                {
                    _spriteBatch.Draw(texture, new Rectangle(drawX + 16 - 4, drawY + 16 - 4, 6, 6), new Rectangle(cornerSize + barSize, cornerSize + barSize, cornerSize, cornerSize), customColor); // bottom right
                }
                if (!hasBottom && !hasLeft) // bottom left
                {
                    _spriteBatch.Draw(texture, new Rectangle(drawX - 2, drawY + 16 - 4, 6, 6), new Rectangle(0, cornerSize + barSize, cornerSize, cornerSize), customColor); // bottom left
                }

                // inner corners
                if (tcDict.ContainsKey(new Point(tileX + 1, tileY - 1)) && !hasRight && hasTop) // top left
                {
                    _spriteBatch.Draw(textureInnerCorners, new Rectangle(drawX + 16 - 4, drawY - 4, 6, 6), new Rectangle(0, 0, cornerSize, cornerSize), customColor); // top left
                }
                if (tcDict.ContainsKey(new Point(tileX - 1, tileY - 1)) && !hasLeft && hasTop) // top right
                {
                    _spriteBatch.Draw(textureInnerCorners, new Rectangle(drawX - 2, drawY - 4, 6, 6), new Rectangle(6, 0, cornerSize, cornerSize), customColor); // top right
                }
                if (tcDict.ContainsKey(new Point(tileX - 1, tileY + 1)) && !hasLeft && hasBottom) // bottom right
                {
                    _spriteBatch.Draw(textureInnerCorners, new Rectangle(drawX - 2, drawY + 16 - 2, 6, 6), new Rectangle(6, 6, cornerSize, cornerSize), customColor); // bottom right
                }
                if (tcDict.ContainsKey(new Point(tileX + 1, tileY + 1)) && !hasRight && hasBottom) // bottom left
                {
                    _spriteBatch.Draw(textureInnerCorners, new Rectangle(drawX + 16 - 4, drawY + 16 - 2, 6, 6), new Rectangle(0, 6, cornerSize, cornerSize), customColor); // bottom left
                }
            }
            _spriteBatch.End();
        }

        public static void DrawTileCollection(TileCollection tilesToDraw, Point coordToDrawAt, bool drawTiles = true, bool drawWalls = true, bool drawLiquid = true, bool drawWires = true)
        {
            // spritebatch and draw stuff
            _spriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.LinearClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            TilePaintSystemV2 ps = Main.instance.TilePaintSystem;

            // faster to get index with list
            var tilesList = tilesToDraw.AsDictionary().ToList();
            var tilesDict = tilesToDraw.AsDictionary();
            int minX = tilesToDraw.GetMinX();
            int minY = tilesToDraw.GetMinY();

            // make into draw coordinates
            coordToDrawAt.X = coordToDrawAt.X * 16 - (int)Main.screenPosition.X;
            coordToDrawAt.Y = coordToDrawAt.Y * 16 - (int)Main.screenPosition.Y;

            // draw walls first to avoid them clipping into other tiles
            foreach (var tile in tilesList)
            {
                int drawX = (tile.Key.X - minX) * 16 + coordToDrawAt.X;
                int drawY = (tile.Key.Y - minY) * 16 + coordToDrawAt.Y;

                // check if its worth doing calculations
                if (drawX > Main.screenWidth * Main.UIScale || drawX < 0 || drawY > Main.screenHeight * Main.UIScale || drawY < 0)
                {
                    continue;
                }

                TileCopy tileTc = tile.Value;

                // wall
                if ((tileTc.WallType != 0) && drawWalls) // dont draw if there isnt a wall
                {
                    // load wall texture
                    Main.instance.LoadWall(tile.Value.WallType);

                    // calculate bounds
                    Rectangle boundsWall = new Rectangle(drawX - 8, drawY - 8, 32, 32);

                    // get wall texture with tilepaintsystem to get the texture with paint if it has any
                    Texture2D tileWallTex = ps.TryGetWallAndRequestIfNotReady(tileTc.WallType, tileTc.WallColor);

                    // get texture from spritesheet with help from frameX and frameY and draw it
                    if (tileWallTex != null)
                    {
                        _spriteBatch.Draw(tileWallTex, boundsWall, new Rectangle(tileTc.WallFrameX, tileTc.WallFrameY, 32, 32), Color.White * 0.6f);
                    }
                }
            }

            // go over all the tiles
            foreach (var tile in tilesList)
            {
                int drawX = (tile.Key.X - minX) * 16 + coordToDrawAt.X;
                int drawY = (tile.Key.Y - minY) * 16 + coordToDrawAt.Y;

                if (drawX > Main.screenWidth * Main.UIScale || drawX < 0 || drawY > Main.screenHeight * Main.UIScale || drawY < 0)
                {
                    continue;
                }

                // load tile texture
                Main.instance.LoadTiles(tile.Value.TileType);

                TileCopy tileTc = tile.Value;

                if (drawTiles)
                {
                    // treetops
                    if (tileTc.IsTreeTop)
                    {
                        if (tileTc.TileType == 72) // underground shroomcaps
                        {
                            // some calculations and stuff i took from the source code
                            Rectangle boundsTile = new Rectangle(drawX - 22, drawY - 26, 60, 42);
                            Texture2D top = TextureAssets.ShroomCap.Value;
                            int num = 0;
                            if (tileTc.TileFrameY == 18)
                            {
                                num = 1;
                            }
                            else if (tileTc.TileFrameY == 36)
                            {
                                num = 2;
                            }
                            _spriteBatch.Draw(top, boundsTile, new Rectangle?(new Rectangle(num * 62, 0, 60, 42)), Color.White * 0.6f);
                        }
                        else if (tileTc.TileType == 323) // palm treetops
                        {
                            // some calculations and stuff i took from the source code
                            int treeTextureIndex = 15;
                            int width = 80;
                            int height = 80;
                            int num1 = 32;
                            int num2 = 0;
                            int y2 = tileTc.TreeBiome * 82;
                            if (tileTc.TreeBiome >= 4 && tileTc.TreeBiome <= 7)
                            {
                                treeTextureIndex = 21;
                                width = 114;
                                height = 98;
                                num1 = 48;
                                num2 = 2;
                                y2 = (tileTc.TreeBiome - 4) * 98;
                            }
                            Rectangle boundsTile = new Rectangle(drawX - num1 + tileTc.TileFrameY, drawY + 16 + num2 - height, width, height);
                            Texture2D top = ps.TryGetTreeTopAndRequestIfNotReady(treeTextureIndex, tileTc.TreeBiome, tileTc.TileColor);
                            _spriteBatch.Draw(top, boundsTile, new Rectangle?(new Rectangle(tileTc.TreeFrame * (tileTc.TreeFrameWidth + 2), tileTc.TreeFrame * (tileTc.TreeFrameHeight + 2) + y2, width, height)), Color.White * 0.6f);
                        }
                        else
                        { // rest of the treetops
                            Rectangle boundsTile = new Rectangle(drawX - tileTc.TreeFrameWidth / 2 + 8, drawY - tileTc.TreeFrameHeight + 16, tileTc.TreeFrameWidth, tileTc.TreeFrameHeight);
                            Texture2D top = ps.TryGetTreeTopAndRequestIfNotReady(tileTc.TreeStyle, 0, tileTc.TileColor);
                            _spriteBatch.Draw(top, boundsTile, new Rectangle?(new Rectangle(tileTc.TreeFrame * (tileTc.TreeFrameWidth + 2), 0, tileTc.TreeFrameWidth, tileTc.TreeFrameHeight)), Color.White * 0.6f);
                        }
                    }

                    // tree branches
                    if (tileTc.IsTreeBranch) // texture dimensions are kind of hardcoded so this might not work with modded trees
                    {
                        Texture2D branch = ps.TryGetTreeBranchAndRequestIfNotReady(tileTc.TreeStyle, 0, tileTc.TileColor);

                        // (40 is the width and height of the branch sprite)
                        if (tileTc.IsFlipped)
                        {
                            Rectangle boundsTile = new Rectangle(drawX - 24, drawY - (40 / 2) + (16 / 2), 40, 40);
                            _spriteBatch.Draw(branch, boundsTile, new Rectangle?(new Rectangle(42, tileTc.TreeFrame * 42, 40, 40)), Color.White * 0.6f, default, default, SpriteEffects.FlipHorizontally, default);
                        }
                        else
                        {
                            Rectangle boundsTile = new Rectangle(drawX, drawY - (40 / 2) + (16 / 2), 40, 40);
                            _spriteBatch.Draw(branch, boundsTile, new Rectangle?(new Rectangle(42, tileTc.TreeFrame * 42, 40, 40)), Color.White * 0.6f);
                        }
                    }

                    // tile
                    if (!tileTc.IsTreeTop && !tileTc.IsTreeBranch && tileTc.HasTile) // dont draw if theres no tile
                    {
                        // calculate bounds
                        Rectangle boundsTile = new Rectangle(drawX, drawY + (tileTc.IsHalfBlock ? 8 : 0), 16, (tileTc.IsHalfBlock ? 8 : 16));

                        // get tile texture with tilepaintsystem to get the texture with paint if it has any
                        Texture2D tileTex = ps.TryGetTileAndRequestIfNotReady(tileTc.TileType, tileTc.TileFrameNumber, tileTc.TileColor);

                        // get texture from spritesheet with help from frameX and frameY and draw it
                        if (tileTc.IsTreeTrunk && tileTc.TileType != 72) // this draws general trees, underground mushroom trees should be drawn like normal tiles tho
                        {
                            // width for tree trunks should be 20 for the whole sprite
                            boundsTile = new Rectangle(drawX - 2, drawY, 20, 16);

                            // this one draws tree trunks better
                            _spriteBatch.Draw(tileTex, boundsTile, new Rectangle((176 * tileTc.TreeVariant) + tileTc.TileFrameX, tileTc.TileFrameY, 20, 16), Color.White * 0.6f);
                        }
                        else if (tileTc.TileType == 323) // this draws palm trees
                        {
                            // width for tree trunks should be 20 for the whole sprite
                            boundsTile = new Rectangle(drawX - 2, drawY, 20, 16);

                            // get texture
                            tileTex = ps.TryGetTileAndRequestIfNotReady(tileTc.TileType, tileTc.TreeBiome, tileTc.TileColor);

                            // this determines the palm tree curve
                            if (!(tileTc.TileFrameX <= 132 && tileTc.TileFrameX >= 88))
                            {
                                boundsTile.X += tileTc.TileFrameY;
                            }
                            _spriteBatch.Draw(tileTex, boundsTile, new Rectangle(tileTc.TileFrameX, 22 * tileTc.TreeBiome, 20, 16), Color.White * 0.6f);
                        }
                        else // draw normal tiles
                        {
                            if (tileTex != null)
                            {
                                // make this draw slopes and half blocks at some point
                                _spriteBatch.Draw(tileTex, boundsTile, new Rectangle(tileTc.TileFrameX, tileTc.TileFrameY, 16, tileTc.IsHalfBlock ? 8 : 16), Color.White * 0.6f);
                            }
                        }
                    }
                }


                // liquid
                if (drawLiquid && tileTc.LiquidAmount > 0)
                {
                    int num = 4;
                    try
                    {
                        if ((tileTc.LiquidAmount < 255 || !tilesDict[new Point(tile.Key.X, tile.Key.Y - 1)].HasTile) && tilesDict[new Point(tile.Key.X, tile.Key.Y - 1)].LiquidAmount == 0) // determite if the surface part of the liquid should be shown
                        {
                            num = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        num = 0;
                    }
                    int height = (int)(((float)tileTc.LiquidAmount / 255) * 16);
                    if (height < 6)
                    {
                        height = 6;
                    }
                    Rectangle boundsTile = new Rectangle(drawX, drawY + (16 - height), 16, height);
                    _spriteBatch.Draw(TextureAssets.Liquid[tileTc.LiquidType].Value, boundsTile, new Rectangle(0, num, 16, height), Color.White * 0.6f);
                }

                // wires
                if (drawWires && (tileTc.GreenWire || tileTc.RedWire || tileTc.YellowWire || tileTc.BlueWire))
                {
                    // this whole thing is kinda silly and isnt super accurate when it comes to mixed wire colors
                    // rewrite this at some point probably
                    Rectangle boundsTile = new Rectangle(drawX, drawY, 16, 16);

                    bool topWire = false;
                    bool bottomWire = false;
                    bool rightWire = false;
                    bool leftWire = false;

                    bool HasMatchingWire(TileCopy tile1, TileCopy tile2)
                    {
                        return (tile1.GreenWire && tile2.GreenWire) || (tile1.RedWire && tile2.RedWire) || (tile1.YellowWire && tile2.YellowWire) || (tile1.BlueWire && tile2.BlueWire);
                    }

                    if (tilesDict.TryGetValue(new Point(tile.Key.X, tile.Key.Y - 1), out TileCopy top) && HasMatchingWire(tileTc, top))
                    {
                        topWire = top.HasWire;
                    }
                    if (tilesDict.TryGetValue(new Point(tile.Key.X, tile.Key.Y + 1), out TileCopy bottom) && HasMatchingWire(tileTc, bottom))
                    {
                        bottomWire = bottom.HasWire;
                    }
                    if (tilesDict.TryGetValue(new Point(tile.Key.X + 1, tile.Key.Y), out TileCopy right) && HasMatchingWire(tileTc, right))
                    {
                        rightWire = right.HasWire;
                    }
                    if (tilesDict.TryGetValue(new Point(tile.Key.X - 1, tile.Key.Y), out TileCopy left) && HasMatchingWire(tileTc, left))
                    {
                        leftWire = left.HasWire;
                    }

                    void DrawWire(string wireType, bool topWire, bool bottomWire, bool rightWire, bool leftWire, float t)
                    {
                        Texture2D texture = TextureAssets.Wire.Value;
                        switch (wireType)
                        {
                            case "Red":
                                texture = TextureAssets.Wire.Value;
                                break;
                            case "Blue":
                                texture = TextureAssets.Wire2.Value;
                                break;
                            case "Green":
                                texture = TextureAssets.Wire3.Value;
                                break;
                            case "Yellow":
                                texture = TextureAssets.Wire4.Value;
                                break;
                        }

                        switch (topWire, bottomWire, rightWire, leftWire)
                        {
                            case (false, false, false, false):
                                // none
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(0, 54, 16, 16), Color.White * t);
                                break;
                            case (false, false, false, true):
                                // left
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(54, 36, 16, 16), Color.White * t);
                                break;
                            case (false, false, true, false):
                                // right
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(72, 36, 16, 16), Color.White * t);
                                break;
                            case (false, false, true, true):
                                // left right
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(18, 0, 16, 16), Color.White * t);
                                break;
                            case (false, true, false, false):
                                // bottom
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(18, 36, 16, 16), Color.White * t);
                                break;
                            case (false, true, false, true):
                                // bottom left
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(72, 18, 16, 16), Color.White * t);
                                break;
                            case (false, true, true, false):
                                // bottom right
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(0, 36, 16, 16), Color.White * t);
                                break;
                            case (false, true, true, true):
                                // bottom left right
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(72, 0, 16, 16), Color.White * t);
                                break;
                            case (true, false, false, false):
                                // top
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(36, 36, 16, 16), Color.White * t);
                                break;
                            case (true, false, false, true):
                                // top left
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(54, 18, 16, 16), Color.White * t);
                                break;
                            case (true, false, true, false):
                                // top right
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(36, 18, 16, 16), Color.White * t);
                                break;
                            case (true, false, true, true):
                                // top left right
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(0, 18, 16, 16), Color.White * t);
                                break;
                            case (true, true, false, false):
                                // top bottom
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(0, 0, 16, 16), Color.White * t);
                                break;
                            case (true, true, false, true):
                                // top bottom left
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(54, 0, 16, 16), Color.White * t);
                                break;
                            case (true, true, true, false):
                                // top bottom right
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(36, 0, 16, 16), Color.White * t);
                                break;
                            case (true, true, true, true):
                                // top bottom right left
                                _spriteBatch.Draw(texture, boundsTile, new Rectangle(18, 18, 16, 16), Color.White * t);
                                break;
                        }
                    }

                    // draw the wires
                    if (tileTc.RedWire)
                    {
                        DrawWire("Red", topWire, bottomWire, rightWire, leftWire, 0.65f);
                    }
                    if (tileTc.BlueWire)
                    {
                        DrawWire("Blue", topWire, bottomWire, rightWire, leftWire, 0.4f);
                    }
                    if (tileTc.GreenWire)
                    {
                        DrawWire("Green", topWire, bottomWire, rightWire, leftWire, 0.4f);
                    }
                    if (tileTc.YellowWire)
                    {
                        DrawWire("Yellow", topWire, bottomWire, rightWire, leftWire, 0.4f);
                    }
                }

                // actuator
                if (drawWires && tileTc.HasActuator)
                {
                    Rectangle boundsTile = new Rectangle(drawX, drawY, 16, 16);

                    Texture2D texture = TextureAssets.Actuator.Value;
                    _spriteBatch.Draw(texture, boundsTile, Color.White * 0.6f);
                }
            }
            _spriteBatch.End();
        }

        public static void DrawMiscOptions(Rectangle rect, bool drawLines, bool drawMeasure)
        {
            _spriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            rect = new Rectangle(rect.X * 16 - (int)Main.screenPosition.X, rect.Y * 16 - (int)Main.screenPosition.Y, (rect.Width + 1) * 16, (rect.Height + 1) * 16);

            // add center lines
            // can be toggled
            if (drawLines)
            {
                Rectangle bounds = rect;
                Color color = new Color(45, 43, 46);
                _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle(bounds.X + 2, bounds.Y + (bounds.Height / 2) - 1, bounds.Width - 4, 2), color); // horizontal line
                _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle(bounds.X + (bounds.Width / 2) - 1, bounds.Y + 2, 2, bounds.Height - 4), color); // vertical line
            }

            try
            {
                if (drawMeasure) // rewrite this thing probably, atleast make the tiles visualName displayed better
                {
                    Rectangle bounds = rect;
                    Color color = new Color(215, 215, 215);

                    // convert to terraria tiles coordinates
                    bounds.X = (bounds.X + (int)Main.screenPosition.X) / 16;
                    bounds.Y = (bounds.Y + (int)Main.screenPosition.Y) / 16;
                    bounds.Width = bounds.Width / 16;
                    bounds.Height = bounds.Height / 16;

                    // left line
                    int distance1 = 101;
                    for (int y = 0; y < bounds.Height; y++)
                    {
                        for (int x = 0; x <= 101; x++)
                        {
                            if (Main.tile[bounds.X - x, bounds.Y + y].HasTile)
                            {
                                if ((x - 1) < distance1)
                                {
                                    distance1 = x - 1;
                                }
                            }
                        }
                    }
                    if (distance1 != -1)
                    {
                        Terraria.Utils.DrawBorderStringFourWay(_spriteBatch, FontAssets.MouseText.Value, "Tiles: " + (distance1 > 100 ? ">100" : distance1), bounds.X * 16 - 6 * 16 - (int)Main.screenPosition.X, (bounds.Y * 16) + ((bounds.Height * 16) / 2) - (int)Main.screenPosition.Y, new Color(215, 215, 215), Color.Black, Vector2.Zero, 1f);
                        _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle((bounds.X * 16 - distance1 * 16) - (int)Main.screenPosition.X, (bounds.Y * 16) - (int)Main.screenPosition.Y, 2, bounds.Height * 16), color); // vertical line
                        _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle((bounds.X * 16 - distance1 * 16) - (int)Main.screenPosition.X, (bounds.Y * 16) + ((bounds.Height * 16) / 2) - (int)Main.screenPosition.Y - 1, distance1 * 16 - 2, 2), color); // horizontal line
                    }

                    // right line
                    int distance2 = 101;
                    for (int y = 0; y < bounds.Height; y++)
                    {
                        for (int x = 0; x <= 101; x++)
                        {
                            if (Main.tile[bounds.X + bounds.Width + x - 1, bounds.Y + y].HasTile)
                            {
                                if ((x - 1) < distance2)
                                {
                                    distance2 = x - 1;
                                }
                            }
                        }
                    }
                    if (distance2 != -1)
                    {
                        Terraria.Utils.DrawBorderStringFourWay(_spriteBatch, FontAssets.MouseText.Value, "Tiles: " + (distance2 > 100 ? ">100" : distance2), (bounds.X * 16 + bounds.Width * 16) + 16 - (int)Main.screenPosition.X, (bounds.Y * 16) + ((bounds.Height * 16) / 2) - (int)Main.screenPosition.Y, new Color(215, 215, 215), Color.Black, Vector2.Zero, 1f);
                        _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle((bounds.X * 16 + bounds.Width * 16 + distance2 * 16) - (int)Main.screenPosition.X - 2, (bounds.Y * 16) - (int)Main.screenPosition.Y, 2, bounds.Height * 16), color); // vertical line
                        _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle((bounds.X * 16 + bounds.Width * 16) - (int)Main.screenPosition.X + 2, (bounds.Y * 16) + ((bounds.Height * 16) / 2) - (int)Main.screenPosition.Y - 1, distance2 * 16 - 2, 2), color); // horizontal line
                    }

                    // top line
                    int distance3 = 101;
                    for (int x = 0; x < bounds.Width; x++)
                    {
                        for (int y = 0; y <= 101; y++)
                        {
                            if (Main.tile[bounds.X + x, bounds.Y - y].HasTile)
                            {
                                if ((y - 1) < distance3)
                                {
                                    distance3 = y - 1;
                                }
                            }
                        }
                    }
                    if (distance3 != -1)
                    {
                        Terraria.Utils.DrawBorderStringFourWay(_spriteBatch, FontAssets.MouseText.Value, "Tiles: " + (distance3 > 100 ? ">100" : distance3), (bounds.X * 16) + (bounds.Width * 16) / 2 - (int)Main.screenPosition.X, (bounds.Y * 16) - 24 - (int)Main.screenPosition.Y, new Color(215, 215, 215), Color.Black, Vector2.Zero, 1f);
                        _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle((bounds.X * 16) + (bounds.Width * 16) / 2 - (int)Main.screenPosition.X - 1, (bounds.Y * 16 - distance3 * 16) - (int)Main.screenPosition.Y, 2, distance3 * 16 - 2), color); // vertical line
                        _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle((bounds.X * 16) - (int)Main.screenPosition.X, (bounds.Y * 16 - distance3 * 16) - (int)Main.screenPosition.Y, bounds.Width * 16, 2), color); // horizontal line
                    }

                    // bottom line
                    int distance4 = 101;
                    for (int x = 0; x < bounds.Width; x++)
                    {
                        for (int y = 0; y <= 101; y++)
                        {
                            if (Main.tile[bounds.X + x, bounds.Y + bounds.Height + y - 1].HasTile)
                            {
                                if ((y - 1) < distance4)
                                {
                                    distance4 = y - 1;
                                }
                            }
                        }
                    }
                    if (distance4 != -1)
                    {
                        Terraria.Utils.DrawBorderStringFourWay(_spriteBatch, FontAssets.MouseText.Value, "Tiles: " + (distance4 > 100 ? ">100" : distance4), (bounds.X * 16) + (bounds.Width * 16) / 2 - (int)Main.screenPosition.X, (bounds.Y * 16 + bounds.Height * 16) + 8 - (int)Main.screenPosition.Y, new Color(215, 215, 215), Color.Black, Vector2.Zero, 1f);
                        _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle((bounds.X * 16) + (bounds.Width * 16) / 2 - (int)Main.screenPosition.X - 1, (bounds.Y * 16 + bounds.Height * 16) - (int)Main.screenPosition.Y + 2, 2, distance4 * 16 - 2), color); // vertical line
                        _spriteBatch.Draw((Texture2D)ModContent.Request<Texture2D>("TerrariaInGameWorldEditor/UI/UIImages/Color"), new Rectangle((bounds.X * 16) - (int)Main.screenPosition.X, (bounds.Y * 16 + distance4 * 16 + bounds.Height * 16) - (int)Main.screenPosition.Y - 2, bounds.Width * 16, 2), color); // horizontal line
                    }
                }
            }
            catch (Exception e)
            {

            }
            _spriteBatch.End();
        }
    }
}
