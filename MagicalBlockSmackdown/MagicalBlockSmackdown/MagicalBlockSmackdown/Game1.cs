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

        private static Random gameRand = null;
        public static Random GameRandom { get { return gameRand; } }

        private GameplayModel model = null;

        private int cursorX = 0;
        private int cursorY = 0;
        private Color cursorShade = new Color(0.15f, 0.15f, 0.15f, 0.5f);

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

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Right) && !rightDown)
            {
                if (cursorX < model.Grid.GetLength(0) - 2)
                {
                    cursorX++;
                }

                rightDown = true;
            }
            else if (!ks.IsKeyDown(Keys.Right) && rightDown)
            {
                rightDown = false;
            }

            if (ks.IsKeyDown(Keys.Left) && !leftDown)
            {
                if (cursorX > 0)
                {
                    cursorX--;
                }

                leftDown = true;
            }
            else if (!ks.IsKeyDown(Keys.Left) && leftDown)
            {
                leftDown = false;
            }

            if (ks.IsKeyDown(Keys.Down) && !downDown)
            {
                if (cursorY < model.Grid.GetLength(1) - 1)
                {
                    cursorY++;
                }

                downDown = true;
            }
            else if (!ks.IsKeyDown(Keys.Down) && downDown)
            {
                downDown = false;
            }

            if (ks.IsKeyDown(Keys.Up) && !upDown)
            {
                if (cursorY > 0)
                {
                    cursorY--;
                }

                upDown = true;
            }
            else if (!ks.IsKeyDown(Keys.Up) && upDown)
            {
                upDown = false;
            }

            if (ks.IsKeyDown(Keys.Space) && !spaceDown)
            {
                model.pushSwap(cursorX, cursorY);

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

            spriteBatch.Begin();
            for (int i = 0; i < model.Grid.GetLength(0); i++)
            {
                for (int j = 0; j < model.Grid.GetLength(1); j++)
                {
                    if (model.Grid[i, j].state == GameplayModel.PanelState.None)
                    {
                        continue;
                    }

                    Vector2 drawPos = new Vector2(100, 100) + new Vector2(i * 16, j * 16);

                    spriteBatch.Draw(whitePixel, drawPos, null, model.Grid[i, j].PanelColorValue(), 0.0f, Vector2.Zero, 16f, SpriteEffects.None, 0.5f);
                }
            }

            spriteBatch.Draw(whitePixel, new Vector2(100, 100) + new Vector2(cursorX * 16, cursorY * 16), null, cursorShade, 0.0f, Vector2.Zero, 16f, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(whitePixel, new Vector2(100, 100) + new Vector2((cursorX + 1) * 16, cursorY * 16), null, cursorShade, 0.0f, Vector2.Zero, 16f, SpriteEffects.None, 0.5f);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
