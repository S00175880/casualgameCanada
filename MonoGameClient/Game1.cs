﻿using System;
using CommonDataItems;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine.Engines;
using Sprites;
using System.Collections.Generic;
using GameComponentNS;
using MonoGameClient.GameObjects;
using textInput;
using System.Text;
using Microsoft.Xna.Framework.Media;

namespace MonoGameClient
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public enum PlayerDataState { LOGGEDOUT, LOGGEDIN, CHARACTER_ASSIGNED }
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Song backgroundSong;
        string connectionMessage = string.Empty;

        //for background image in the game
        Texture2D background;
        Rectangle mainFrame;

        public PlayerData pData;
        PlayerDataState state = PlayerDataState.LOGGEDOUT;

        public bool timerSwtich = false;

        //variable for the local time for the players;
        public float localTime = 0;

        // SignalR Client object delarations

        HubConnection serverConnection;
        IHubProxy proxy;

        public bool Connected { get; private set; }

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
            new FadeTextManager(this);
            // create input engine
            new InputEngine(this);
            Helpers.GraphicsDevice = GraphicsDevice; //for the user keyboard
            new GetGameInputComponent(this); //for user keyboard
            // TODO: Add your initialization logic here change local host to newly created local host
            //https://casualgamescanada.azurewebsites.net
            //serverConnection = new HubConnection("http://localhost:53922/");

            serverConnection = new HubConnection("https://casualgamescanada.azurewebsites.net");
            serverConnection.StateChanged += ServerConnection_StateChanged;
            proxy = serverConnection.CreateHubProxy("GameHub");
            serverConnection.Start();


            Action<PlayerData> joined = clientJoined;
            proxy.On<PlayerData>("Joined", joined);

            //Action<List<PlayerData>> usrlogin = checkLogin;
            //proxy.On<List<PlayerData>>("checkLogin", usrlogin);

            Action<List<PlayerData>> currentPlayers = clientPlayers;
            proxy.On<List<PlayerData>>("CurrentPlayers", currentPlayers);

            Action<string, Position> otherMove = clientOtherMoved;
            proxy.On<string, Position>("OtherMove", otherMove);

            //FOR COLLECTABLES

            Action<CollectableData> cJoined = collectableJoined;
            proxy.On<CollectableData>("collectableJoined", cJoined);

            Action<List<CollectableData>> currentCollectables = clientCollectables;
            proxy.On<List<CollectableData>>("CurrentCollectables", currentCollectables);

            // Add the proxy client as a Game service o components can send messages 
            Services.AddService<IHubProxy>(proxy);

            base.Initialize();
        }

        //NEW FOR COLLECTABLES
        private void clientCollectables(List<CollectableData> otherCollectables)
        {
            foreach (CollectableData collectable in otherCollectables)
            {
                // Create an other player sprites in this client afte
                new Collectables(this, collectable, Content.Load<Texture2D>(collectable.CollectableImageName),
                                        new Point(collectable.CollectablePosition.X, collectable.CollectablePosition.Y));
                connectionMessage = collectable.CollectableID + " delivered ";
            }
        }

        private void collectableJoined(CollectableData otherCollectableData)
        {
            // Create an other collectable sprite
            new Collectables(this, otherCollectableData, Content.Load<Texture2D>(otherCollectableData.CollectableImageName),
                                    new Point(otherCollectableData.CollectablePosition.X, otherCollectableData.CollectablePosition.Y));
        }

        //END COLLECTABLES
        private void clientOtherMoved(string playerID, Position newPos)
        {
            // iterate over all the other player components 
            // and check to see the type and the right id
            foreach (var player in Components)
            {
                if (player.GetType() == typeof(OtherPlayerSprite)
                    && ((OtherPlayerSprite)player).pData.playerID == playerID)
                {
                    OtherPlayerSprite p = ((OtherPlayerSprite)player);
                    p.pData.playerPosition = newPos;
                    p.Position = new Point(p.pData.playerPosition.X, p.pData.playerPosition.Y);
                    break; // break out of loop as only one player position is being updated
                           // and we have found it
                }
            }
        }


        // Only called when the client joins a game
        private void clientPlayers(List<PlayerData> otherPlayers)
        {
            foreach (PlayerData player in otherPlayers)
            {
                // Create an other player sprites in this client afte
                new OtherPlayerSprite(this, player, Content.Load<Texture2D>(player.imageName),
                                        new Point(player.playerPosition.X, player.playerPosition.Y));
                connectionMessage = player.playerID + " delivered ";

                //displays a text on the game screen with the current player id on the screen
                new FadeText(this, Vector2.Zero, "ClientPlayers");

                //the timer in the game will be set to 60 seconds once a second player joins
                localTime = 60000;
            }
        }

        private void clientJoined(PlayerData otherPlayerData)
        {
            // Create an other player sprite
            new OtherPlayerSprite(this, otherPlayerData, Content.Load<Texture2D>(otherPlayerData.imageName),
                                    new Point(otherPlayerData.playerPosition.X, otherPlayerData.playerPosition.Y));

            new FadeText(this, Vector2.Zero, "ClientJoined");//infroms current players that this player joined

            //the timer now starts for all players
            localTime = 60000;
        }

        private void ServerConnection_StateChanged(StateChange State)
    {
        switch (State.NewState)
        {
            case ConnectionState.Connected:
                connectionMessage = "Connected......";
                Connected = true;
                startGame();
                break;
            case ConnectionState.Disconnected:
                connectionMessage = "Disconnected.....";
                if (State.OldState == ConnectionState.Connected)
                    connectionMessage = "Lost Connection....";
                    Connected = false;
                break;
            case ConnectionState.Connecting:
                connectionMessage = "Connecting.....";
                Connected = false;
                break;

        }
    }

        //Checks for a valid user login when a player starts up the game
        private void checkLogin()
        {
            if (Connected && GetGameInputComponent.name != string.Empty && GetGameInputComponent.password != string.Empty

                )
            {
                proxy.Invoke<PlayerData>("checkCredentials",
                    new object[] { GetGameInputComponent.name, GetGameInputComponent.password })
                    .ContinueWith( // This is an inline delegate pattern that processes the message 
                                   // returned from the async Invoke Call
                            (p) =>
                            { // With p do 
                                if (p.Result == null)
                                    connectionMessage = "Invalid Login";
                                else
                                {
                                    pData = p.Result;
                                    state = PlayerDataState.LOGGEDIN;
                                }
                            });
            }
        }

        private void startGame()
        {
            // Continue on and subscribe to the incoming messages joined, currentPlayers, otherMove messages

            // Immediate Pattern
            proxy.Invoke<PlayerData>("Join")
                .ContinueWith( // This is an inline delegate pattern that processes the message 
                               // returned from the async Invoke Call
                        (p) => { // Wtih p do 
                            if (p.Result == null)
                                connectionMessage = "No player Data returned";
                            else
                            {
                                CreatePlayer(p.Result);
                                // Here we'll want to create our game player using the image name in the PlayerData 
                                // Player Data packet to choose the image for the player
                                // We'll use a simple sprite player for the purposes of demonstration 
                                
                            }
                            
                        });



            proxy.Invoke<CollectableData>("CollectablesJoin")
                .ContinueWith( // This is an inline delegate pattern that processes the message 
                   // returned from the async Invoke Call
            (c) => { // Wtih p do 
                            if (c.Result == null)
                            connectionMessage = "No player Data returned";
                            else
                            {
                                CreateCollectables(c.Result);
                                //this will create a collectible when the player joins
                            }
            });

            proxy.Invoke("hello");

        }

        // When we get new player Data Create 
        private void CreatePlayer(PlayerData player)
        {
            // Create an other player sprites in this client afte
            new SimplePlayerSprite(this, player, Content.Load<Texture2D>(player.imageName),
                                    new Point(player.playerPosition.X, player.playerPosition.Y));

            connectionMessage = player.playerID + " created ";

            new FadeText(this, Vector2.Zero, "Welcome" + player.GamerTag + "you are playing as " + player.imageName);
          
        }

        //COLLECTABLE CREATE when a player joins 
        private void CreateCollectables(CollectableData collectable)
        {
            new Collectables(this, collectable, Content.Load<Texture2D>(collectable.CollectableImageName),
                        new Point(collectable.CollectablePosition.X, collectable.CollectablePosition.Y));


            connectionMessage = collectable.CollectableID + " created ";
        }
        //END COLLECTABLE

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //loads the background music
            this.backgroundSong = Content.Load<Song>("Defense Line"); //load music 
            MediaPlayer.Play(backgroundSong); //plays the background music

            MediaPlayer.MediaStateChanged += MediaPlayerStateChanged; 

            //Load the background
            background = Content.Load<Texture2D>("spacebackground");
            //Set the rectangle parameter
            mainFrame = new Rectangle(0, 0, GraphicsDevice.Viewport.Width * 2, GraphicsDevice.Viewport.Height * 2);

            Services.AddService<SpriteBatch>(spriteBatch);

            font = Content.Load<SpriteFont>("Message");
            Services.AddService<SpriteFont>(font);
            
        }

        public void MediaPlayerStateChanged(object sender, System.EventArgs e)
        {
            MediaPlayer.Volume -= 0.1f;
            MediaPlayer.Play(backgroundSong);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        private void timer(PlayerData player)
        {
            new TimeText(this, Vector2.Zero, "Time:" + localTime);
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            {
                switch (state)
                {
                    case PlayerDataState.LOGGEDOUT:
                        if (Connected)
                        {
                            checkLogin();
                        }
                        break;
                    case PlayerDataState.LOGGEDIN:
                        break;
                }
                                
            }
            //SCORE DISPLAY
            //displays scores for player (from collision function in SimplePlayerSprite.cs)
            new ScoreText(this, Vector2.Zero, "Score:           " + SimplePlayerSprite.points);


            //This Displays the time in the game 
            new TimeText(this, Vector2.Zero, "Time:" + localTime);

            //if the timer time is greater than 5 seconds decrease the time
            if (localTime > 5)
            {
                localTime -= gameTime.ElapsedGameTime.Milliseconds;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
             if (state == PlayerDataState.LOGGEDIN)
            {
               
            }

            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.DrawString(font, connectionMessage, new Vector2(10, 10), Color.White);

            //Draws background
            spriteBatch.Draw(background, mainFrame, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
