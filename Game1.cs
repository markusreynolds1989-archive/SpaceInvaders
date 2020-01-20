﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceInvaders
{
    public class Game1 : Game
    {
        private SpriteBatch spriteBatch;
        private Texture2D ship;
        private Texture2D playerBullet;
        private Texture2D enemy;
        private Texture2D enemyLaserTexture;
        private SpriteFont font;
        private Player player;
        private List<PlayerBullet> bullets;
        private List<Enemy> enemies;
        private List<Laser> lasers;
        private TimeSpan bulletSpawnTime;
        private TimeSpan previousBulletSpawnTime;
        private bool gameOver = false;
        private int score = 0;
        private const int ScreenWidth = 600;
        private const int ScreenHeight = 800;
        private double enemySpeedModifier = 1;
        
        public Game1()
        {
            var graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = ScreenWidth, PreferredBackBufferHeight = ScreenHeight
            };
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            bullets = new List<PlayerBullet>();
            enemies = new List<Enemy>();
            bulletSpawnTime = TimeSpan.FromSeconds((0.5f));
            player = new Player {Position = new Vector2(0, ScreenHeight - 55)};  
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ship = Content.Load<Texture2D>("Ship");
            player.Texture = ship;
            playerBullet = Content.Load<Texture2D>("PlayerBullet");
            enemy = Content.Load<Texture2D>("Enemy");
            enemyLaserTexture = Content.Load <Texture2D>("Laser"); 
            font = Content.Load<SpriteFont>("Score");
            //InitEnemies has to go here because of texture loading. 
            InitEnemies();
        }

        protected override void Update(GameTime gameTime)
        {
            if (gameOver == false)
            {
                EventHandler(gameTime);
                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            //Draw all the bullets.
            foreach (var b in bullets)
            {
                b.Draw(spriteBatch);
            }
            //Draw all the enemies.
            foreach (var e in enemies)
            {
                e.Draw(spriteBatch);
            }

            foreach (var l in lasers)
            {
                l.Draw(spriteBatch);
            }
            
            //Draw the player's Ship.
            spriteBatch.Draw(ship, player.Position, Color.White);
            //Draw Score.
            spriteBatch.DrawString(font
                ,$"Score: {score.ToString()}"
                ,new Vector2(ScreenWidth - 200, 10)
                ,Color.White );
            spriteBatch.DrawString(font
                , $"Lives : {player.Lives.ToString()}"
                , new Vector2(0, 10)
                , Color.White);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
        
        private void EventHandler(GameTime gameTime)
        {
            var state = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
            
            if (state.IsKeyDown(Keys.Right)
                && player.Position.X <= ScreenWidth - player.Width) 
            {
                player.Position.X += player.Speed;
            }

            if (state.IsKeyDown(Keys.Left)
                && player.Position.X >= 0) 
            {
                player.Position.X -= player.Speed;
            }

            if (state.IsKeyDown(Keys.Space))
            {
                CreateBullet(gameTime);
            }

            foreach (var bl in bullets)
            {
                foreach (var en in enemies)
                {
                    {
                        if (!Collision.RectangleCollision(bl.Position.X
                            , bl.Position.Y
                            , bl.Texture.Width
                            , bl.Texture.Height
                            , en.Position.X
                            , en.Position.Y
                            , en.Texture.Width
                            , en.Texture.Height)) continue;
                        en.Active = false;
                        bl.Active = false;
                        score++;
                        enemySpeedModifier += 0.25;
                    }
                }
            }
            UpdateBullets(gameTime);            
            UpdateEnemies(gameTime);
            UpdateLasers(gameTime);
            if (player.Lives < 0)
            {
                gameOver = true;
            }
        }

        private void UpdateLasers(GameTime gameTime)
        {
            for (var i = 0; i < lasers.Count; i++)
            {
                lasers[i].Update(gameTime);
                if (!lasers[i].Active || Math.Abs(lasers[i].Position.Y) < 0)
                {
                    lasers.Remove(lasers[i]);
                }
            }
        }

        private void UpdateBullets(GameTime gameTime)
        {
            for (var i = 0; i < bullets.Count; i++)
            {
                bullets[i].Update(gameTime);
                if (!bullets[i].Active || Math.Abs(bullets[i].Position.Y) < 0)
                {
                    bullets.Remove(bullets[i]);
                }
            }
        }
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (var en in enemies)
            {
                if (en.Position.X + en.Texture.Width >= ScreenWidth)
                {
                    foreach (var item in enemies)
                    {
                        item.Speed = -enemySpeedModifier;
                        item.Position.Y += (float)enemySpeedModifier;
                    }
                }
                if (en.Position.X <= 0)
                {
                    foreach (var item in enemies)
                    {
                        item.Speed = enemySpeedModifier;
                        item.Position.Y += (float)enemySpeedModifier;
                    }
                }
                if ((int)en.Position.Y + en.Texture.Height == ScreenHeight)
                {
                    gameOver = true;
                }

                en.Position.X += (float)en.Speed;
                
            }
                        
            for (var i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update(gameTime);
                if (!enemies[i].Active)
                {
                    enemies.Remove(enemies[i]);
                }
            }
        }

        private void CreateBullet(GameTime gameTime)
        {
            if (gameTime.TotalGameTime - previousBulletSpawnTime > bulletSpawnTime)
            {
                previousBulletSpawnTime = gameTime.TotalGameTime;
                var bullet = new PlayerBullet();
                bullet.Initialize(playerBullet, new Vector2(
                    player.Position.X + player.Width / 2
                    , player.Position.Y - player.Height/4));
                bullets.Add(bullet);
            }
        }

        private void CreateLasers(GameTime gameTime)
        {
            if (gameTime.TotalGameTime - previousBulletSpawnTime > bulletSpawnTime)
            {
                previousBulletSpawnTime = gameTime.TotalGameTime;
                var laser = new Laser();
                laser.Initialize(enemyLaserTexture
                    ,new Vector2(ScreenWidth/2, ScreenHeight/2));
            }
        }
        private void InitEnemies()
        {
            for (var i = 50; i < ScreenWidth / 1.5; i += 50)
            {
                for (var j = 50; j < ScreenHeight/2; j += 50)
                {
                    var en = new Enemy();
                    en.Initialize(enemy, new Vector2(i, j));
                    enemies.Add(en);
                }
            }
        }
    }
}