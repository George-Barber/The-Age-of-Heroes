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
using Squared.Tiled;
using System.Timers;

namespace The_Age_of_Heroes_Game.Content.Sprites
{
    public class Projectile : Sprite
    {
        public Squared.Tiled.Object mapobj;
        public Timer deathtimer;
        public Texture2D Blank;
        public bool enemy;

        public bool active = false;
        // constructor to pass everything to sprite bas class
        public Projectile(Texture2D texture, Texture2D blank,Vector2 pos, Vector2 move, Squared.Tiled.Object obj, bool en) : base(texture,null)
        {
            active = true;
            Blank = blank;
            Velocity = move;
            Position = pos;
            mapobj = obj;
            deathtimer = new Timer();
            deathtimer.Interval = 3000;
            deathtimer.Elapsed += new ElapsedEventHandler(OnFireTimedEvent);
            deathtimer.Enabled = true;
            enemy = en;
        }
        private void OnFireTimedEvent(object source, ElapsedEventArgs e)
        {
            deathtimer.Enabled = false;
            mapobj.Texture = Blank;
            active = false;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 vp, bool check)
        {
            // if it uses a texture then draw
            if (_texture != null)
            {
                //vp.Y -= 90;
                //spriteBatch.Draw(_texture, Position - vp, Color.White);
                //Console.WriteLine("Proj position: " + (Position - vp));
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            // set position of the enemy
            Vector2 pos = Position;
            Position += Velocity;
            if (pos == Position)
            {
                deathtimer.Enabled = false;
                mapobj.Texture = Blank;
                active = false;
            }
            else
            {
                mapobj.X = (int)Position.X;
                mapobj.Y = (int)Position.Y;
                Console.WriteLine("proj: " + Position + " - " + mapobj.X + " " + mapobj.Y);
            }
        }

    }
}
