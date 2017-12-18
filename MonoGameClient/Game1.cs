using System;
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

namespace MonoGameClient
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        string connectionMessage = string.Empty;

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

            // TODO: Add your initialization logic here change local host to newly created local host
            //http://s00175879gameserver.azurewebsites.net
            serverConnection = new HubConnection("http://localhost:53922/");

            //serverConnection = new HubConnection("http://s00175879gameserver.azurewebsites.net");
            serverConnection.StateChanged += ServerConnection_StateChanged;
            proxy = serverConnection.CreateHubProxy("GameHub");
            serverConnection.Start();


            Action<PlayerData> joined = clientJoined;
            proxy.On<PlayerData>("Joined", joined);

            Action<List<PlayerData>> currentPlayers = clientPlayers;
            proxy.On<List<PlayerData>>("CurrentPlayers", currentPlayers);

            Action<string, Position> otherMove = clientOtherMoved;
            proxy.On<string, Position>("OtherMove", otherMove);

            Action<List<CollectableData>> updateCollectables = clientCollectables;
            proxy.On<List<CollectableData>>("UpdateCollectables", updateCollectables);

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
            }
        }

        private void clientJoined(PlayerData otherPlayerData)
        {
            // Create an other player sprite
            new OtherPlayerSprite(this, otherPlayerData, Content.Load<Texture2D>(otherPlayerData.imageName),
                                    new Point(otherPlayerData.playerPosition.X, otherPlayerData.playerPosition.Y));
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
            


        }

        // When we get new player Data Create 
        private void CreatePlayer(PlayerData player)
        {
            // Create an other player sprites in this client afte
            new SimplePlayerSprite(this, player, Content.Load<Texture2D>(player.imageName),
                                    new Point(player.playerPosition.X, player.playerPosition.Y));

        

            //new FadeText(this, Vector2.Zero, "You rock " + player.GamerTag);

            connectionMessage = player.playerID + " created ";

            new FadeText(this, Vector2.Zero, "Welcome" + player.GamerTag + "you are playing as " + player.imageName);
          
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService<SpriteBatch>(spriteBatch);

            font = Content.Load<SpriteFont>("Message");
            Services.AddService<SpriteFont>(font);
            
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
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

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
            spriteBatch.DrawString(font, connectionMessage, new Vector2(10, 10), Color.White);
            // TODO: Add your drawing code here
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
