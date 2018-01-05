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
                new PlayerData {
                playerID = Guid.NewGuid().ToString(),
                //CharacterImage = "Player 1",
                GamerTag ="High Flyer",  Password = "plrxxx1",
                PlayerName= "Paul", XP = 2000},

                new PlayerData {
                playerID = Guid.NewGuid().ToString(),
                //CharacterImage = "Player 2",
                GamerTag ="Bug Hunter",  Password = "plrxxx2",
                PlayerName= "Fred", XP = 200},

            };

    }
}