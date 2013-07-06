using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MagicalBlockSmackdown
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private static Texture2D whitePixel = null;
        public static Texture2D WhitePixel { get { return whitePixel; } }
        private static Texture2D tileSheet = null;
        public static Texture2D TileSheet { get { return tileSheet; } }
        private static SpriteFont commodore = null;
        public static SpriteFont InGameText { get { return commodore; } }

        private static Random gameRand = null;
        public static Random GameRandom { get { return gameRand; } }

        private GameplayModel model = null;

        private float cursorAnimationTime = 0;

        private bool rightDown = false;
        private bool leftDown = false;
        private bool downDown = false;
        private bool upDown = false;
        private bool spaceDown = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            gameRand = new Random();

            model = new GameplayModel();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            whitePixel = Utilities.ColorTexture.Create(GraphicsDevice, Color.White);
            tileSheet = Content.Load<Texture2D>("tilesheet");
            commodore = Content.Load<SpriteFont>("commodore");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            //
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            cursorAnimationTime += gameTime.ElapsedGameTime.Milliseconds;

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Right) && !rightDown)
            {
                if (model.CursorX < model.Grid.GetLength(0) - 2)
                {
                    model.CursorX++;
                }

                rightDown = true;
            }
            else if (!ks.IsKeyDown(Keys.Right) && rightDown)
            {
                rightDown = false;
            }

            if (ks.IsKeyDown(Keys.Left) && !leftDown)
            {
                if (model.CursorX > 0)
                {
                    model.CursorX--;
                }

                leftDown = true;
            }
            else if (!ks.IsKeyDown(Keys.Left) && leftDown)
            {
                leftDown = false;
            }

            if (ks.IsKeyDown(Keys.Down) && !downDown)
            {
                if (model.CursorY < model.Grid.GetLength(1) - 1)
                {
                    model.CursorY++;
                }

                downDown = true;
            }
            else if (!ks.IsKeyDown(Keys.Down) && downDown)
            {
                downDown = false;
            }

            if (ks.IsKeyDown(Keys.Up) && !upDown)
            {
                if (model.CursorY > 1)
                {
                    model.CursorY--;
                }

                upDown = true;
            }
            else if (!ks.IsKeyDown(Keys.Up) && upDown)
            {
                upDown = false;
            }

            if (ks.IsKeyDown(Keys.Space) && !spaceDown)
            {
                model.pushSwap(model.CursorX, model.CursorY);

                spaceDown = true;
            }
            else if (!ks.IsKeyDown(Keys.Space) && spaceDown)
            {
                spaceDown = false;
            }

            model.update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);

            spriteBatch.DrawString(Game1.InGameText, "Score: " + model.Score, new Vector2(200, 100), Color.Black);

            for (int i = 0; i < model.Grid.GetLength(0); i++)
            {
                for (int j = 0; j < model.Grid.GetLength(1); j++)
                {
                    if (model.Grid[i, j].state == GameplayModel.PanelState.None)
                    {
                        continue;
                    }

                    Vector2 drawPos = new Vector2(100, 100 - (model.PushingUpValue * 16)) + new Vector2(i * 16, j * 16);

                    spriteBatch.Draw(tileSheet, drawPos, new Rectangle((int)(model.Grid[i, j].color) * 16, 0, 16, 16), Color.White, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.5f);

                    if (model.Grid[i, j].state == GameplayModel.PanelState.Exploding)
                    {
                        spriteBatch.Draw(whitePixel, drawPos + new Vector2(4), null, Color.Black, 0.0f, Vector2.Zero, 8, SpriteEffects.None, 0.5f);
                    }
                }
            }

            for (int i = 0; i < model.NextLineOfPanels.Length; i++)
            {
                if (model.NextLineOfPanels[i].state == GameplayModel.PanelState.None)
                {
                    continue;
                }

                Vector2 drawPos = new Vector2(100, 100 - (model.PushingUpValue * 16)) + new Vector2(i * 16, model.Grid.GetLength(1) * 16);

                spriteBatch.Draw(tileSheet, drawPos, new Rectangle((int)(model.NextLineOfPanels[i].color) * 16, 0, 16, 16), Color.Lerp(Color.White, Color.Black, 0.75f), 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            }

            if ((int)(cursorAnimationTime / 500) % 2 == 0) //crude animation
            {
                spriteBatch.Draw(tileSheet, new Vector2(98, 98 - (model.PushingUpValue * 16)) + new Vector2(model.CursorX * 16, model.CursorY * 16), new Rectangle(14, 30, 19, 19), Color.White, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
                spriteBatch.Draw(tileSheet, new Vector2(98, 98 - (model.PushingUpValue * 16)) + new Vector2((model.CursorX + 1) * 16, model.CursorY * 16), new Rectangle(14, 30, 19, 19), Color.White, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            }
            else
            {
                spriteBatch.Draw(tileSheet, new Vector2(98, 98 - (model.PushingUpValue * 16)) + new Vector2(model.CursorX * 16, model.CursorY * 16), new Rectangle(46, 30, 19, 19), Color.White, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
                spriteBatch.Draw(tileSheet, new Vector2(98, 98 - (model.PushingUpValue * 16)) + new Vector2((model.CursorX + 1) * 16, model.CursorY * 16), new Rectangle(46, 30, 19, 19), Color.White, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            }

            spriteBatch.Draw(whitePixel, new Vector2(100, 0), null, Color.Black, 0.0f, Vector2.Zero, new Vector2(96, 100), SpriteEffects.None, 0.5f);
            spriteBatch.Draw(whitePixel, new Vector2(100, 100 + (model.Grid.GetLength(1) * 16)), null, Color.Black, 0.0f, Vector2.Zero, new Vector2(96, 64), SpriteEffects.None, 0.5f);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
