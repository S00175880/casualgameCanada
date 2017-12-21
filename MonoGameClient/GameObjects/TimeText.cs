using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameComponentNS
{
    class TimeTextManager : DrawableGameComponent
    {
        //Vector2 basePosition;

        public TimeTextManager(Game game) : base(game)
        {
            game.Components.Add(this);
            //basePosition = new Vector2(10, game.GraphicsDevice.Viewport.Height - 20);
        }
        protected override void LoadContent()
        {
            
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            var faders = Game.Components.Where(
                            t => t.GetType() == typeof(TimeText));

            base.Update(gameTime);
        }


    }


    class TimeText : DrawableGameComponent
    {
        string text;
        Vector2 position;
        byte blend = 255;

        public Vector2 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public TimeText(Game game, Vector2 Position, string Text) :base(game)
        {
            game.Components.Add(this);
            text = Text;
            this.Position = new Vector2(Position.X + 520, Position.Y);
        }

        public override void Update(GameTime gameTime)
        {
            if (blend > 0)
                blend--;
            else { Game.Components.Remove(this); }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var sp = Game.Services.GetService<SpriteBatch>();
            var font = Game.Services.GetService<SpriteFont>();
            //Color myColor = new Color((byte)0, (byte)0, (byte)0, blend);
            sp.Begin(SpriteSortMode.Immediate,BlendState.Opaque);
            sp.DrawString(font, text, Position, new Color((byte)0, (byte)255, (byte)255));
            sp.End();
            base.Draw(gameTime);
        }

    }
}
