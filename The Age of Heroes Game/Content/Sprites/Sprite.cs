﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using The_Age_of_Heroes_Game.Content.Manager;
using System.Timers;
using The_Age_of_Heroes_Game.Content.Models;
using Squared.Tiled;

namespace The_Age_of_Heroes_Game.Content.Sprites
{
    public class Sprite
    {
        #region Fields
        public AnimationManager _animationManager;
        protected Dictionary<string, Animation> _animations;
        protected Vector2 _position;
        protected Texture2D _texture,projtexture, healthtexture;
        public Rectangle HealthBar;
        public int Health;
        public int HealthMax;
        public bool enemy;

        Timer firetimer;
        public List<Projectile> PlayerProjectiles;
        public float Speed = 1f;
        public Vector2 Velocity,OldVelocity;
        #endregion
        #region Properties
        public Input Input;

        // property to set the position of the sprite
        // it will also update the position of the animation
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;

                if (_animationManager != null)
                    _animationManager.Position = _position;
            }
        }
        private void OnFireTimedEvent(object source, ElapsedEventArgs e)
        {
            firetimer.Enabled = false;
        }
        public bool Fire(Vector2 Position, Squared.Tiled.Object obj, Texture2D blank )
        {
            if (!firetimer.Enabled)
            {
                
                Projectile Temp = new Projectile(projtexture, blank, Position,OldVelocity*5,obj, false);
                PlayerProjectiles.Add(Temp);
                firetimer = new Timer();
                firetimer.Interval = 500;
                firetimer.Elapsed += new ElapsedEventHandler(OnFireTimedEvent);
                firetimer.Enabled = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
        #region Method

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 vp)
        {
            // if the sprite uses a static texture
            if (_texture != null)
            {
                spriteBatch.Draw(_texture, Position - vp, Color.White);
            }
            // if the sprite uses an animation
            else if (_animationManager != null)
            {
                //draw the animation
                _animationManager.Draw(spriteBatch, vp);

                // if the sprite uses a health bar
                if (HealthBar != null)
                {
                    // create texture for health bar
                    healthtexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);

                    // Create a 1D array of color data to fill the pixel texture with.  
                    Color[] colorData = {
                        Color.White,
                    };

                    // Set the texture data with our color information.  
                    healthtexture.SetData<Color>(colorData);

                    // draw the health bar
                    spriteBatch.Draw(healthtexture, HealthBar, Color.Red);

                }
            }

        }
        
        // method used to get current movement to then set the animation later
        public virtual void GetMove()
        {
            if (Keyboard.GetState().IsKeyDown(Input.Up))
                Velocity.Y = -Speed;
            else if (Keyboard.GetState().IsKeyDown(Input.Down))
                Velocity.Y = Speed;
            else if (Keyboard.GetState().IsKeyDown(Input.Left))
                Velocity.X = -Speed;
            else if (Keyboard.GetState().IsKeyDown(Input.Right))
                Velocity.X = Speed;
        }

        // constructor for sprite, run when a sprite is created
        public Sprite(Dictionary<string, Animation> animations, bool health,Texture2D proj, bool en)
        {
            // setup health
            if (health)
            {
                Health = 10;
                HealthMax = 10;
                HealthBar = new Rectangle(0, 0, 30/HealthMax*Health, 5);
            }
            PlayerProjectiles = new List<Projectile>();
            // setup animations
            _animations = animations;
            _animationManager = new AnimationManager(_animations.First().Value);
            firetimer = new Timer();
            if (proj != null)
                projtexture = proj;
            enemy = en;
        }

        // second constructor for if the sprite is a single texture
        public Sprite(Texture2D texture, Texture2D proj)
        {
            _texture = texture;
            firetimer = new Timer();
            PlayerProjectiles = new List<Projectile>();
            if (proj != null)
                projtexture = proj;
        }

        // update method for the sprite
        public virtual void Update(GameTime gameTime, Vector2 Position,Vector2 vp)
        {
            // get move and set animation changes the animation according to direction
            GetMove();
            SetAnimations();

            // update position of the sprite
            Position += Velocity;

            //set health bar position

            HealthBar = new Rectangle(0, 0, 30 / HealthMax * Health, 5);
            HealthBar.X = (int)Position.X - (int)vp.X;
            HealthBar.Y = (int)Position.Y - (int)vp.Y + 80;

            // only update animation if sprite is moving
            if (Velocity != Vector2.Zero)
                _animationManager.Update(gameTime);
            OldVelocity = Velocity;
            Velocity = Vector2.Zero;
            //sets projectiles
            int projcount = 0;
            foreach (Projectile proj in PlayerProjectiles)
            {
                if (proj.active)
                {
                    proj.Update(gameTime);
                }
                projcount++;
            }

            if (Health <= 0)
            {
                Console.WriteLine("DEAD" + Health);
            }
        
                

        }

        protected virtual void SetAnimations()
        {
            // set which animation is playing
            if (Velocity.Y < 0)
                _animationManager.Play(_animations["Player Forward"]);
            if (Velocity.Y > 0)
                _animationManager.Play(_animations["Player Backwards"]);
            if (Velocity.X > 0)
                _animationManager.Play(_animations["Player Right"]);
            if (Velocity.X < 0)
                _animationManager.Play(_animations["Player Left"]);

        }




        #endregion
    }
}