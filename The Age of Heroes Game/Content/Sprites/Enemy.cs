﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using The_Age_of_Heroes_Game.Content.Manager;
using The_Age_of_Heroes_Game.Content.Models;
using Squared.Tiled;


namespace The_Age_of_Heroes_Game.Content.Sprites
{
    // enemy is a class of sprite
    public class Enemy : Sprite
    {
        public Squared.Tiled.Object mapobj;
        // constructor to pass everything to sprite bas class
        public Enemy(Dictionary<string, Animation> animations2, bool health, Squared.Tiled.Object obj) : base(animations2, health,null, true)
        {
            mapobj = obj;
        }

        // set enemy animations
        protected virtual void SetAnimations(bool check)
        {
            if (Velocity.Y < 0 && Velocity.X!=0)
                _animationManager.Play(_animations["Enemy Forward"]);
            else if (Velocity.Y > 0 && Velocity.X != 0)
                _animationManager.Play(_animations["Enemy Backwards"]);
            else if (Velocity.Y < 0)
                _animationManager.Play(_animations["Enemy Forward"]);
            else if (Velocity.Y > 0)
                _animationManager.Play(_animations["Enemy Backwards"]);
            else if (Velocity.X > 0)
                _animationManager.Play(_animations["Enemy Right"]);
            else if (Velocity.X < 0)
                _animationManager.Play(_animations["Enemy Left"]);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 vp, bool check)
        {
            if (Health > 0)
            {
                // if it uses a texture then draw
                if (_texture != null)
                {
                    spriteBatch.Draw(_texture, Position - vp, Color.White);
                }
                // if it uses an animation then draw
                else if (_animationManager != null)
                {
                    // draw animation for enemy
                    _animationManager.Draw(spriteBatch, vp);

                    // if it has a health bar get texture and draw
                    if (HealthBar != null)
                    {

                        healthtexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);

                        // Create a 1D array of color data to fill the pixel texture with.  
                        Color[] colorData = {
                                Color.White,
                            };

                        // Set the texture data with our color information.  
                        healthtexture.SetData<Color>(colorData);
                        HealthBar = new Rectangle(0, 0, 30 / HealthMax * Health, 5);
                        HealthBar.X = (int)Position.X - (int)vp.X;
                        HealthBar.Y = (int)Position.Y - (int)vp.Y + 80;
                        spriteBatch.Draw(healthtexture, HealthBar, Color.Red);

                    }
                }
                else throw new Exception("DRAW ERROR!!!");
            }
        }
        public virtual void Update(GameTime gameTime, Vector2 pposition)
        {
            if (Health > 0)
            {
                // check if enemy within 10 of the player
                if (Vector2.Distance(_position, pposition) < 10)
                {


                }
                // if the enemy is within 200 of the player move towards player
                else if (Vector2.Distance(_position, pposition) < 200)
                {
                    Velocity = (pposition - _position) / Vector2.Distance(_position, pposition);
                }
                // too far away from player
                else
                {
                    Velocity = Vector2.Zero;
                }

                OldVelocity = Velocity;

                int projcount = 0;
                foreach (Projectile proj in PlayerProjectiles)
                {
                    if (proj.active)
                    {
                        proj.Update(gameTime);
                    }
                    projcount++;
                }


                // set animations for the enemy
                SetAnimations(true);

                // set position of the enemy
                Position += Velocity;

                // update animation if enemy is moving
                if (Velocity != Vector2.Zero)
                    _animationManager.Update(gameTime);
                Velocity = Vector2.Zero;
            }
            else
            {

                Console.WriteLine("DEAD" + Health);
            }
        }
    }
}
