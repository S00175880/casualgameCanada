using CommonDataItems;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalrGameServer
{
    public static class GameDataObjects
    {
        static public List<PlayerData> RegisteredPlayers = new List<PlayerData>()
            {
                //stores the info of two regstered players
                new PlayerData {
                playerID = Guid.NewGuid().ToString(),
                //CharacterImage = "Player 1",
                GamerTag ="Rocket Man",  Password = "plyrx1",
                PlayerName= "Stewart", XP = 100},

                new PlayerData {
                playerID = Guid.NewGuid().ToString(),
                //CharacterImage = "Player 2",
                GamerTag ="Storm Warning",  Password = "plyrxx2",
                PlayerName= "Sarah", XP = 400},

            };

    }
}