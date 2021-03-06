﻿using Engine.Engines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace textInput
{

    class GetGameInputComponent : DrawableGameComponent
    {
        InputEngine input;
        SimpleKeyboard keyboard;
        SpriteFont sfont;
        public static string name = string.Empty;
        public static string password = string.Empty;
        bool firstText = false;
        private SpriteBatch spriteBatch;
        public bool Done = false;
        public bool IsMouseVisible { get; private set; }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                password = value;
            }
        }

        public string Output
        {
            get
            {
                return output;
            }

            set
            {
                output = value;
            }
        }

        public GetGameInputComponent(Game g) : base(g)
        {
            g.Components.Add(this);
            IsMouseVisible = true;
            input = new InputEngine(g);
            keyboard = new SimpleKeyboard(new Vector2(10, 10));
        }

        public void Clear()
        {
            firstText = false;
            Name = string.Empty;
            output = string.Empty;
            input.KeysPressedInLastFrame.Clear();
            keyboard.Output = string.Empty;
            InputEngine.ClearState();
            Done = false;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            sfont = Game.Content.Load<SpriteFont>("keyboardfont"); //loads keyboard font
            keyboard.LoadContent(Game.Content); //loads keyboard
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            keyboard.Update(gameTime);

            //grabs key input and stores it in the Output
            if (InputEngine.IsKeyPressed(Keys.Enter) && !firstText && !Done)
            {
                Name = Output; 
                Output = string.Empty;
                firstText = true;
                keyboard.Output = string.Empty;
                InputEngine.ClearState();
            }
            if (InputEngine.IsKeyPressed(Keys.Enter) && firstText && !Done)
            {
                Output = string.Empty;
                keyboard.Output = string.Empty;
                Done = true;
                Enabled = false;
                Visible = false;
            }
            if(InputEngine.IsKeyPressed(Keys.Back))
                if (Output.Length > 0)
                    Output = Output.Remove(Output.Length - 1);
            if (InputEngine.IsKeyPressed(Keys.Space))
                Output += " ";

            //

                base.Update(gameTime);
        }

        string output = "";

        public override void Draw(GameTime gameTime)
        {
            //Draws the background for the keyboard
            GraphicsDevice.Clear(Color.Turquoise);
            if (!firstText)
                foreach (var s in input.KeysPressedInLastFrame)
                    Output += s;
            else
                foreach (var s in input.KeysPressedInLastFrame)
                {
                    Output += "*";
                    Password += s;
                }

            keyboard.Draw();
            if (Done) return;
            spriteBatch.Begin();
            if(!firstText)
                spriteBatch.DrawString(sfont, "User Name " + Output, new Vector2(10, GraphicsDevice.Viewport.Height - 60), Color.Black); //draws the user name that the player inputs
            else spriteBatch.DrawString(sfont, "Password " + Output, new Vector2(10, GraphicsDevice.Viewport.Height - 60), Color.Black); //draws the password that the player inputs
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
