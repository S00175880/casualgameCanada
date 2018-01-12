using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//this script is to allow the ScoreText to be positioned in a specific location and colour
namespace GameComponentNS
{
    class ScoreTextManager : DrawableGameComponent
    {
        public ScoreTextManager(Game game) : base(game)
        {
            game.Components.Add(this);
        }

    }

    class ScoreText : DrawableGameComponent
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

        public ScoreText(Game game, Vector2 Position, string Text) :base(game)
        {
            game.Components.Add(this);
            text = Text;
            //The text position top left (0,0) plus 120 pixels to the right on the X coordinate
            this.Position = new Vector2(Position.X + 120, Position.Y);
        }

        public override void Draw(GameTime gameTime)
        {
            var sp = Game.Services.GetService<SpriteBatch>();
            var font = Game.Services.GetService<SpriteFont>();
            
            //Makes the Sprite for the text opaque (clear) so that the numbers don't become
            //a square block (numbers writing on each other)
            sp.Begin(SpriteSortMode.Immediate,BlendState.Opaque);
            sp.DrawString(font, text, Position, new Color((byte)255, (byte)0, (byte)255, blend));
            sp.End();
            base.Draw(gameTime);
        }

    }
}


//THE BELOW CODE IS NOT USED
//public override void Update(GameTime gameTime)
//{
//    //var faders = Game.Components.Where(
//    //                t => t.GetType() == typeof(ScoreText));

//    base.Update(gameTime);
//}

//public override void Update(GameTime gameTime)
//{
//    //if(blend > 0)
//    //    blend--;
//    //else { Game.Components.Remove(this); }

//    base.Update(gameTime);
//}

//protected override void LoadContent()
//{          
//    base.LoadContent();
//}

