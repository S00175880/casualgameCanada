using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CommonDataItems;
using MonoGameClient.GameObjects;

namespace Sprites
{
    public class OtherPlayerSprite : DrawableGameComponent
    {
        public Texture2D Image;
        public Point Position;
        public Rectangle BoundingRect;
        public bool Visible = true;
        public Color tint = Color.White;
		public PlayerData pData;
        public Point Target;

        // Constructor epects to see a loaded Texture
        // and a start position
        public OtherPlayerSprite(Game game, PlayerData data, Texture2D spriteImage,
                            Point startPosition) : base(game)
        {
            pData = data;
            game.Components.Add(this);
            // Take a copy of the texture passed down
            Image = spriteImage;
            // Take a copy of the start position
            Position = startPosition;
            // Calculate the bounding rectangle
            BoundingRect = new Rectangle(startPosition.X, startPosition.Y, Image.Width, Image.Height);
            Target = startPosition;
        }

        public override void Update(GameTime gameTime)
        {
            BoundingRect = new Rectangle(Position.X, Position.Y, Image.Width, Image.Height);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch sp = Game.Services.GetService<SpriteBatch>();
            if (Image != null && Visible)
            {
                sp.Begin();
                sp.Draw(Image, BoundingRect, tint);
                sp.End();
            }

            base.Draw(gameTime);
        }


    }
}
