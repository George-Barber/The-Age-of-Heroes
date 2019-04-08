using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Squared.Tiled;
using System.IO;
using System.Collections.Generic;
using System;
using The_Age_of_Heroes_Game.Content.Sprites;
using The_Age_of_Heroes_Game.Content.Models;
using A1r.SimpleTextUI;
using System.Timers;

namespace The_Age_of_Heroes_Game
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Map map,map1,map2,map3,map4;
        Layer collision;
        private Vector2 viewportPosition;
        int tilepixel;
        public Texture2D PlayerAnimation;
        private List<Sprite> _sprites;
        private Texture2D blankTexture;
        private Texture2D coinTexture;
        private Texture2D projTexture;
        private Texture2D menuBackground;
        private readonly Texture2D keyTexture;
        List<Squared.Tiled.Object> Inventory;
        public List<Enemy> EnemyList;
        //public List<Projectile> PlayerProjectiles;
        int coin_collected = 0;
        SimpleTextUI menu;
        SimpleTextUI inventory;
        SimpleTextUI options;
        SimpleTextUI current;
        SpriteFont big;
        SpriteFont small;
        Timer keytimer;
        Timer exittimer;
        int projcount = 0;

        // property to handle changing maps
        public Map CurrentMap
        {
            get { return map; }
            set
            {
                // if the map value changes it also needs to reload the code below
                map = value;

                // reset collision layer
                collision = map.Layers["Collision"];
                tilepixel = map.TileWidth;

                // reset the coins
                int coinCount = Convert.ToInt32(map.ObjectGroups["Objects"].Properties["Coin_Count"]);
                for (int i = 1; i <= coinCount; i++)
                {
                    map.ObjectGroups["Objects"].Objects["Coin" + i].Texture = coinTexture;
                }

                // reset the enemies
                int EnemyCount = Convert.ToInt32(map.ObjectGroups["Objects"].Properties["Enemy_Count"]);
                EnemyList = new List<Enemy>();
                var animations2 = new Dictionary<string, Animation>()
                {
                    { "Enemy Forward", new Animation(Content.Load<Texture2D>("Enemy/Enemy Forward"), 3) },
                    { "Enemy Backwards", new Animation(Content.Load<Texture2D>("Enemy/Enemy Backwards"), 3) },
                    { "Enemy Left", new Animation(Content.Load<Texture2D>("Enemy/Enemy Left"), 3) },
                    { "Enemy Right", new Animation(Content.Load<Texture2D>("Enemy/Enemy Right"), 3) },
                };
                for (int i = 1; i <= EnemyCount; i++)
                {
                    Enemy temp = new Enemy(animations2, true);
                    temp.Position = new Vector2(map.ObjectGroups["Objects"].Objects["Enemy" + i].X, map.ObjectGroups["Objects"].Objects["Enemy" + i].Y);
                    EnemyList.Add(temp);
                }
            }
        }

        // enum to control the menu currently selected
        public enum Menu
        {
            Main,
            Play,
            Options,
            Gameover,
            Inventory
        }
        Menu currentScreen = Menu.Main;

        public Vector2 Position { get; private set; }
        public object mouse_position { get; private set; }
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            graphics.ApplyChanges();

        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // texture for coin ojects, blank once collected
            coinTexture = Content.Load<Texture2D>("coinTexture");
            projTexture = Content.Load<Texture2D>("Magic");
            blankTexture = Content.Load<Texture2D>("Transparent");
            menuBackground = Content.Load<Texture2D>("Age Of Heroes Menu");

            // set and load maps
            map1 = Map.Load(Path.Combine(Content.RootDirectory, "SimpleRPG.tmx"), Content);
            map2 = Map.Load(Path.Combine(Content.RootDirectory, "SimpleRPG.tmx"), Content);
            map3 = Map.Load(Path.Combine(Content.RootDirectory, "SimpleRPG.tmx"), Content);
            map4 = Map.Load(Path.Combine(Content.RootDirectory, "SimpleRPG.tmx"), Content);

            // set first map to map 1
            map = map1;

            // load animations for the player
            var animations = new Dictionary<string, Animation>()
            {
                { "Player Forward", new Animation(Content.Load<Texture2D>("Player/Player Forward"), 3) },
                { "Player Backwards", new Animation(Content.Load<Texture2D>("Player/Player Backwards"), 3) },
                { "Player Left", new Animation(Content.Load<Texture2D>("Player/Player Left"), 3) },
                { "Player Right", new Animation(Content.Load<Texture2D>("Player/Player Right"), 3) },
            };

            // load animations for the enemy
            var animations2 = new Dictionary<string, Animation>()
            {
                { "Enemy Forward", new Animation(Content.Load<Texture2D>("Enemy/Enemy Forward"), 3) },
                { "Enemy Backwards", new Animation(Content.Load<Texture2D>("Enemy/Enemy Backwards"), 3) },
                { "Enemy Left", new Animation(Content.Load<Texture2D>("Enemy/Enemy Left"), 3) },
                { "Enemy Right", new Animation(Content.Load<Texture2D>("Enemy/Enemy Right"), 3) },
            };

            // create list of sprites for player
            _sprites = new List<Sprite>()
            {
                new Sprite(animations,true,projTexture)
                {
                    Position = new Vector2(100, 100),
                    Input = new Input()
                    {
                        Up = Keys.Up,
                        Down = Keys.Down,
                        Left = Keys.Left,
                        Right = Keys.Right,
                    }
                },

            };

            // load spritefonts to draw text to the screen
            big = Content.Load<SpriteFont>("Big");
            small = Content.Load<SpriteFont>("Small");

            // Set menus and screens
            menu = new SimpleTextUI(this, big, new[] { "Continue", "Options", "Credits", "Exit" })
            {
                TextColor = Color.Black,
                SelectedElement = new TextElement(">", Color.White),
                Align = Alignment.LowerLeft
            };

            options = new SimpleTextUI(this, big, new TextElement[]
            {
                new SelectElement("Video", new[]{"FullScreen","Windowed"}),
                new NumericElement("Music",1,3,0f,10f,1f),
                new TextElement("Back")
            });

            inventory = new SimpleTextUI(this, big, new[] { "Coins", "Items", "Keys", "Exit" })
            {
                TextColor = Color.Black,
                SelectedElement = new TextElement(">", Color.White),
                Align = Alignment.Left
            };

            // starting menu is menu
            current = menu;

            // timer to control input on menu screens
            keytimer = new Timer();

            // timer to control use of exits
            exittimer = new Timer();

            

            // get coin count
            int coinCount = Convert.ToInt32(map.ObjectGroups["Objects"].Properties["Coin_Count"]);

            // for each coin add the texture to map
            for (int i = 1; i <= coinCount; i++)
            {
                map.ObjectGroups["Objects"].Objects["Coin" + i].Texture = coinTexture;
            }

            // get enemy count
            int EnemyCount = Convert.ToInt32(map.ObjectGroups["Objects"].Properties["Enemy_Count"]);
            
            // set up list of enemies
            EnemyList = new List<Enemy>();

            // add an enemy to the list for each enemy
            for (int i = 1; i <= EnemyCount; i++)
            {
                //create a temp enemy
                Enemy temp = new Enemy(animations2, true);
                temp.Position = new Vector2(map.ObjectGroups["Objects"].Objects["Enemy" + i].X, map.ObjectGroups["Objects"].Objects["Enemy" + i].Y);

                // add temp enemy to list
                EnemyList.Add(temp);
            }

            // create inventory list
            Inventory = new List<Squared.Tiled.Object>();

            // set the current map to map1
            CurrentMap = map1;
            // TODO: use this.Content to load your game content here
        }
        


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {

            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void clean()
        {
            List<KeyValuePair<string, Squared.Tiled.Object>> dellist = new List<KeyValuePair<string, Squared.Tiled.Object>>();
            foreach (KeyValuePair <string,Squared.Tiled.Object> o in map.ObjectGroups["Objects"].Objects)
            {
                if(o.Value.Texture==blankTexture && o.Value.Type=="proj")
                {
                    dellist.Add(o);
                }
            }
            if(dellist.Count>0)
                foreach (KeyValuePair<string, Squared.Tiled.Object> o in dellist)
                {
                    map.ObjectGroups["Objects"].Objects.Remove(o.Key);
                }
        }

        protected override void Update(GameTime gameTime)
        {
            if (currentScreen == Menu.Play)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();
                clean();
                // get player input
                GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
                KeyboardState keyState = Keyboard.GetState();

                // record current position before movement
                int tempx = map.ObjectGroups["Objects"].Objects["Player"].X;
                int tempy = map.ObjectGroups["Objects"].Objects["Player"].Y;
                Position = new Vector2(map.ObjectGroups["Objects"].Objects["Player"].X, map.ObjectGroups["Objects"].Objects["Player"].Y);

                // process movement input move player object
                ProcessMovement(keyState, gamePadState);

                //now we have moved checkbounds
                if (CheckBounds(map.ObjectGroups["Objects"].Objects["Player"]))
                {
                    map.ObjectGroups["Objects"].Objects["Player"].X = tempx;
                    map.ObjectGroups["Objects"].Objects["Player"].Y = tempy;
                }

                // check for player interaction with coins and exits within the game
                var p = map.ObjectGroups["Objects"].Objects["Player"];
                Rectangle playerRec = new Rectangle(p.X, p.Y, p.Width, p.Height);
                CheckCoins(playerRec);
                CheckExits(playerRec);

                // update player object with position and viewport
                Vector2 Test = (viewportPosition + new Vector2(0, 100) - new Vector2((graphics.PreferredBackBufferWidth / 2), (graphics.PreferredBackBufferHeight / 2)));
                foreach (var sprite in _sprites)
                {
                    sprite.Update(gameTime, Position, Test);
                    sprite.Position = new Vector2(map.ObjectGroups["Objects"].Objects["Player"].X, map.ObjectGroups["Objects"].Objects["Player"].Y);
                }
                // update each enemy
                int i = 1;
                foreach (Enemy E in EnemyList)
                {
                    // get position of enemy
                    Vector2 temp = E.Position;

                    // update the enemy to move
                    E.Update(gameTime, Position);

                    // check for enemy collision between the bounds and other enemies
                    if (CheckBounds(map.ObjectGroups["Objects"].Objects["Enemy" + i]) || CheckEnemy(map.ObjectGroups["Objects"].Objects["Enemy" + i], E))
                    {
                        // collision so set position back to temp value
                        E.Position = temp;
                    }
                    else
                    {
                        // no collision so update the enemy position in map
                        map.ObjectGroups["Objects"].Objects["Enemy" + i].X = (int)E.Position.X;
                        map.ObjectGroups["Objects"].Objects["Enemy" + i].Y = (int)E.Position.Y;

                        if (true)
                        {
                            Squared.Tiled.Object tempp = new Squared.Tiled.Object();
                            tempp.X = (int)E.Position.X;
                            tempp.Y = (int)E.Position.Y;
                            tempp.Width = 30;
                            tempp.Height = 30;
                            tempp.Type = "proj";
                            tempp.Texture = projTexture;
                            if (E.Fire(E.Position, tempp, blankTexture))
                            {
                                map.ObjectGroups["Objects"].Objects.Add("proj" + projcount, tempp);
                                projcount++;
                            }
                        }
                    }
                    i++;
                }



                viewportPosition = new Vector2(map.ObjectGroups["Objects"].Objects["Player"].X, map.ObjectGroups["Objects"].Objects["Player"].Y);
                KeyboardState keys = Keyboard.GetState();
                // Takes to main menu or inventory
                if (keys.IsKeyDown(Keys.Tab))
                {
                    currentScreen = Menu.Main;
                }
                else if (keys.IsKeyDown(Keys.I))
                {
                    currentScreen = Menu.Inventory;
                    current = inventory;
                }
            }
            // code for the controlling menu screens
            else if (currentScreen == Menu.Main)
            {
                KeyboardState keys = Keyboard.GetState();
                bool change = true;

                if (!keytimer.Enabled)
                {
                    if (keys.IsKeyDown(Keys.Up))
                    {
                        current.Move(Direction.Up);
                    }

                    else if (keys.IsKeyDown(Keys.Down))
                    {
                        current.Move(Direction.Down);
                    }
                    else if (keys.IsKeyDown(Keys.Enter))
                    {
                        string test = current.GetCurrentCaption();

                        if (current == menu)
                        {
                            if (test == "Exit")
                                Exit();
                            else if (test == "Options")
                            {
                                current = options;
                            }
                            else if (test == "Continue")
                            {
                                currentScreen = Menu.Play;
                            }

                        }

                        else if (current == options)
                        {
                            if (test == "Back")
                            {
                                current = menu;
                            }
                        }
                        else if (current == options)
                        {
                            if (test == "FullScreen")
                            {
                                graphics.IsFullScreen = false;
                            }
                        }
                    }
                    else
                        change = false;

                    if (change)
                    {
                        keytimer = new Timer();
                        keytimer.Interval = 200;
                        keytimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                        keytimer.Enabled = true;
                    }
                }
            }
            else if (currentScreen == Menu.Inventory)
            {

            }
                base.Update(gameTime);
        }

        // timed event to control key presses
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            keytimer.Enabled = false;
        }

        // timed event to control jumping to and from exits
        private void OnExitTimedEvent(object source, ElapsedEventArgs e)
        {
            exittimer.Enabled = false;
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);
            
            // if the game is playing
            if(currentScreen==Menu.Play)
            {
                spriteBatch.Begin();
                
                // draw map
                map.Draw(spriteBatch, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), viewportPosition - new Vector2((graphics.PreferredBackBufferWidth / 2), (graphics.PreferredBackBufferHeight / 2)));
                
                // draw the player
                foreach (var sprite in _sprites)
                {
                    sprite.Draw(spriteBatch, viewportPosition + new Vector2(0, 100) - new Vector2((graphics.PreferredBackBufferWidth / 2), (graphics.PreferredBackBufferHeight / 2)));
                }


                // draw each enemy
                foreach (Enemy sprite in EnemyList)
                {
                    sprite.Draw(spriteBatch, viewportPosition + new Vector2(0, 100) - new Vector2((graphics.PreferredBackBufferWidth / 2), (graphics.PreferredBackBufferHeight / 2)), true);
                }

                // draw foreground layer
                map.Layers["DTile Layer 1"].Draw(spriteBatch,map.Tilesets.Values, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), viewportPosition - new Vector2((graphics.PreferredBackBufferWidth / 2), (graphics.PreferredBackBufferHeight / 2)),tilepixel,tilepixel);
                spriteBatch.End();
            }
            else
            {
                // not playing so draw current menu
                spriteBatch.Begin();
                spriteBatch.Draw(menuBackground, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                spriteBatch.End();

                current.Draw(gameTime);
            }
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);

        }


        
        
        public void ProcessMovement(KeyboardState keyState, GamePadState gamePadState)
        {
            //detect key press and xy scroll values
            int scrollx = 0, scrolly = 0, moveSpeed = 2;
            if (keyState.IsKeyDown(Keys.Left))
                scrollx = -1;
            if (keyState.IsKeyDown(Keys.Right))
                scrollx = 1;
            if (keyState.IsKeyDown(Keys.Up))
                scrolly = 1;
            if (keyState.IsKeyDown(Keys.Down))
                scrolly = -1;
            if(keyState.IsKeyDown(Keys.Space))
            {
                Squared.Tiled.Object temp = new Squared.Tiled.Object();
                temp.X = (int)Position.X;
                temp.Y = (int)Position.Y;
                temp.Width = 30;
                temp.Height = 30;
                temp.Texture = projTexture;
                temp.Type = "proj";

                
                if (_sprites[0].Fire(Position, temp, blankTexture))
                {
                    map.ObjectGroups["Objects"].Objects.Add("proj" + projcount, temp);
                    projcount++;
                }

            }

            // get game pad input
            scrollx += (int)gamePadState.ThumbSticks.Left.X;
            scrolly += (int)gamePadState.ThumbSticks.Left.Y;

            // move player in the map
            map.ObjectGroups["Objects"].Objects["Player"].X += (scrollx * moveSpeed);
            map.ObjectGroups["Objects"].Objects["Player"].Y -= (scrolly * moveSpeed);
            
        }
        
        // checks for the bounds of the map with a given object
        public bool CheckBounds(Squared.Tiled.Object obj)
        {
            bool check = false;

            // creates rectangle
            Rectangle objrec = new Rectangle(
                obj.X,
                obj.Y,
                obj.Width,
                obj.Height
                );

            int startx = ((int)obj.X / 16)-5;
            int starty = ((int)obj.Y / 16) - 5;
            // cycles through each tile on the map
            for (int x =startx; x <startx+10; x++)
            {
                for (int y = starty; y < starty+10; y++)
                {
                    // if the tile is in the collision layer
                    if (collision.GetTile(x, y) != 0)
                    {
                        // create rectangle for the tile
                        Rectangle tile = new Rectangle(
                            (int)x * tilepixel,
                            (int)y * tilepixel,
                            tilepixel,
                            tilepixel
                            );

                        // check if the object intersects the tile
                        if (objrec.Intersects(tile))
                            check = true;
                    }
                }
            }

            return check;
        }

        // used to check the enemy collision with an object
        // returns true if collision false if not
        // enemy e is to stop collision with itself
        public bool CheckEnemy(Squared.Tiled.Object obj, Enemy e)
        {
            bool check = false;

            // get rectangle for object
            Rectangle objrec = new Rectangle(
                obj.X,
                obj.Y,
                obj.Width,
                obj.Height
                );

            // cycles through each enemy
            foreach(Enemy E in EnemyList)
            {
                // the enemy can't collide with itself
                if (E != e)
                {
                    // creates rectangle for enemy
                    Rectangle enemyrec = new Rectangle(
                        (int)E.Position.X,
                        (int)E.Position.Y,
                        (int)32,
                        (int)32
                     );

                    // checks for collision
                    if (objrec.Intersects(enemyrec))
                    {
                        check = true;
                    }
                }
            }

            return check;
        }

        public void CheckCoins(Rectangle player)
        {
            // get number of coins from the map
            int coinCount = Convert.ToInt32(map.ObjectGroups["Objects"].Properties["Coin_Count"]);

            // variable used to count the total number collected
            int collectCount = 0;

            // cycle through each coin object
            for (int i = 1; i <= coinCount; i++)
            {
                // get the coin
                var coin = map.ObjectGroups["Objects"].Objects["Coin" + i];

                // if the texture is not blank it can be collected
                if (coin.Texture != blankTexture)
                {
                    // get rectangle for coin
                    Rectangle coinRec = new Rectangle(coin.X, coin.Y, coin.Width, coin.Height);

                    // check to see if it intersects with player
                    if (player.Intersects(coinRec))
                    {
                        // add to inventory
                        Inventory.Add(coin);
                        coin_collected++;
                        coin.Texture = blankTexture;
                    }
                }
                else
                {
                    // coin already collected
                    collectCount++;
                }
            }

        }

        public void CheckExits(Rectangle player)
        {
            // get number of exits from the current map
            int exitCount = Convert.ToInt32(map.ObjectGroups["Objects"].Properties["Exit_Count"]);

            // loop to check each exit
            for (int i = 1; i <= exitCount; i++)
            {
                // gets exit object from map
                var exit = map.ObjectGroups["Objects"].Objects["Exit" + i];

                // creates rectangle for exit
                Rectangle exitRec = new Rectangle(exit.X, exit.Y, exit.Width, exit.Height);

                // check for intersect with player, timer to stop constant jumping 
                if (player.Intersects(exitRec) && !exittimer.Enabled)
                {
                    // location is stored in the type field of the object
                    string location = exit.Type;

                    // the map and exit are separated by :
                    string[] parts = location.Split(':');

                    // set the right map
                    switch (parts[0])
                    {
                        case "Map1":
                            CurrentMap = map1;
                            break;
                        case "Map2":
                            CurrentMap = map2;
                            break;
                        case "Map3":
                            CurrentMap = map3;
                            break;
                        case "Map4":
                            CurrentMap = map4;
                            break;
                    }

                    // move player to exit location on this map
                    CurrentMap.ObjectGroups["Objects"].Objects["Player"].X = CurrentMap.ObjectGroups["Objects"].Objects[parts[1]].X;
                    CurrentMap.ObjectGroups["Objects"].Objects["Player"].Y = CurrentMap.ObjectGroups["Objects"].Objects[parts[1]].Y;

                    // set timer so you can't jump for 1 second
                    exittimer = new Timer();
                    exittimer.Interval = 1000;
                    exittimer.Elapsed += new ElapsedEventHandler(OnExitTimedEvent);
                    exittimer.Enabled = true;

                }
            }
        }

    }
}

