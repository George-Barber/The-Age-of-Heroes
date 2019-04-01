using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using The_Age_of_Heroes_Game.Content.Manager;
using The_Age_of_Heroes_Game.Content.Models;

namespace The_Age_of_Heroes_Game.Content.Sprites
{
    public class Projectile : Sprite
    {
        public bool active = false;
        // constructor to pass everything to sprite bas class
        public Projectile(Texture2D texture,Vector2 pos, Vector2 move) : base(texture)
        {
            active = true;
            Velocity = move;
            Position = pos;
            PlayerProjectiles = new List<Projectile>();
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 vp, bool check)
        {
            // if it uses a texture then draw
            if (_texture != null)
            {
                spriteBatch.Draw(_texture, Position - vp, Color.White);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            // set position of the enemy
            Position += Velocity;


            int projcount = 0;
            foreach (Projectile proj in PlayerProjectiles)
            {
                proj.Update(gameTime);
                projcount++;
            }
            Console.WriteLine(projcount);
        }

    }
}
