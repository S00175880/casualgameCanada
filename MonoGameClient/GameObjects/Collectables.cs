﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CommonDataItems;


namespace MonoGameClient.GameObjects
{
    public class Collectables : DrawableGameComponent
    {
        public Texture2D CollectablesImage;
        public Point CollectablesPosition;
        public Rectangle CollectablesBoundingRect;
        public bool CollectablesVisible = true;
        public Color tint = Color.White;
        public CollectableData cData;
        public Point CollectablesTarget;



        // Constructor epects to see a loaded Texture
        // and a start position
        public Collectables(Game game, CollectableData data, Texture2D spriteImage,
                            Point startPosition) : base(game)
        {
            cData = data;
            game.Components.Add(this);
            // Take a copy of the texture passed down
            CollectablesImage = spriteImage;
            // Take a copy of the start position
            CollectablesPosition = startPosition;
            // Calculate the bounding rectangle
            CollectablesBoundingRect = new Rectangle(startPosition.X, startPosition.Y, CollectablesImage.Width, CollectablesImage.Height);
            CollectablesTarget = startPosition;
        }
    }
}