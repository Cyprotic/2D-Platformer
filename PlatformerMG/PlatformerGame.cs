#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.Linq;

namespace PlatformerMG
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        enum GameState
        {
            StartScreen,
            Gameplay,
            GameOver,
        }

        GameState _state = GameState.StartScreen;

        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Global content.
        private SpriteFont hudFont;

        private Texture2D startMenu;
        private Texture2D gameoverMenu;
        private Texture2D gameplay;
        Texture2D status = null;

        private ScoreManager _scoreManager;
        private int _score;

        // Meta-level game state.
        private int levelIndex = -1;
        private Level level;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private TouchCollection touchState;
        private AccelerometerState accelerometerState;
        
        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 3;

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif

            Accelerometer.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/gameFont");

            _scoreManager = ScoreManager.Load();

            // Load overlay textures
            //winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            //loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            //diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

            startMenu = Content.Load<Texture2D>("Menus/StartMenu");
            gameoverMenu = Content.Load<Texture2D>("Menus/gameoverMenu");

            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
            }
            catch { }

            LoadNextLevel();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            switch (_state)
            {
                case GameState.StartScreen:
                    UpdateStartScreen(gameTime);
                    break;
                case GameState.Gameplay:
                    UpdateGameplay(gameTime);
                    break;
                case GameState.GameOver:
                    UpdateGameOver(gameTime);
                    break;
            }
        }

        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
            touchState = TouchPanel.GetState();
            accelerometerState = Accelerometer.GetState();

            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();



            // Perform the appropriate action to advance the game and
            // to get the player back to playing.

            if (!level.Player.IsAlive)
            {
                status = gameoverMenu;
                //level.StartNewLife();
            }
            else if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    _score = level.Score;
                    LoadNextLevel();
                    if (levelIndex == 1)
                    {
                        _scoreManager.Add(new Score()
                        {
                            Value = _score,
                        }
         );
                        ScoreManager.Save(_scoreManager);
                    }
                }
                    
                else
                {
                    status = gameoverMenu;
                }
            }
      
        }

        private void LoadStartLevel()
        {
            if (level != null)
                level.Dispose();

            levelIndex = 0;
            level.levelCount = 0;

            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);

            _state = GameState.StartScreen;
        }

        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }

        //private void ReloadCurrentLevel()
        //{
        //    --levelIndex;
        //    LoadNextLevel();
        //}

        void UpdateStartScreen(GameTime gameTime)
        {
            // Handle polling for our input and handling high-level input
            HandleInput();
            keyboardState = Keyboard.GetState();
            base.Update(gameTime);
            if (keyboardState.IsKeyDown(Keys.Enter))
                _state = GameState.Gameplay;
        }

        void UpdateGameplay(GameTime gameTime)
        {
            // Respond to user actions in the game.
            // Update enemies
            // Handle collisions
            level.Update(gameTime, keyboardState, gamePadState, touchState,
                         accelerometerState, Window.CurrentOrientation);
            base.Update(gameTime);
            HandleInput();
            DrawGameplay(gameTime);
            if (!level.Player.IsAlive || level.TimeRemaining == TimeSpan.Zero)
                _state = GameState.GameOver;
        }

        void UpdateGameOver(GameTime gameTime)
        {
            // Update scores
            // Do any animations, effects, etc for getting a high score
            // Respond to user input to restart level, or go back to main menu
            //base.Update(gameTime);
            DrawGameOver(gameTime);
            HandleInput();
            if (keyboardState.IsKeyDown(Keys.Escape))
                LoadStartLevel();
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            switch (_state)
            {
                case GameState.StartScreen:
                    DrawStartScreen(gameTime);
                    break;
                case GameState.Gameplay:
                    DrawGameplay(gameTime);
                    break;
                case GameState.GameOver:
                    DrawGameOver(gameTime);
                    break;
            }
        }

        void DrawStartScreen(GameTime gameTime)
        {
            // Draw the main menu, any active selections, etc
            spriteBatch.Begin();
            DrawHud();
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            
            base.Draw(gameTime);
            status = startMenu;
            spriteBatch.End();
        }

        void DrawGameplay(GameTime gameTime)
        {
            // update our level, passing down the GameTime along with all of our input states
            spriteBatch.Begin();
            
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
            level.Draw(gameTime, spriteBatch);
            status = gameplay;
            DrawHud();
            spriteBatch.End();
            // Draw the background the level
            // Draw enemies
            // Draw the player
            // Draw particle effects, etc
        }

        void DrawGameOver(GameTime gameTime)
        {
            // Draw text and scores
            // Draw menu for restarting level or going back to main menu
            spriteBatch.Begin();
            DrawHud();
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            status = gameoverMenu;
            spriteBatch.End();
        }

        private void DrawHud()
        {
            
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
                
            }
            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
            DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);
            spriteBatch.DrawString(hudFont, "Highscores:\n" + string.Join("\n", _scoreManager.Highscores.Select(c => c.Value).ToArray()), new Vector2(680, 10), Color.Red);


            // Determine the status overlay message to show.

            //if (level.TimeRemaining == TimeSpan.Zero || !level.ReachedExit)
            //{
            //    status = gameoverMenu;             
            //}
            //else if (!level.Player.IsAlive)
            //{
            //    status = gameoverMenu;
            //}

        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}
