using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Helpers;

namespace BootAndShoot
{
    /// <summary>
    /// Helps manage the coding-world-cup API.
    /// 
    /// You can derive from this class and implement its abstract 
    /// functions in your API.
    /// </summary>
    public abstract class CodingWorldCupAPI
    {
        #region Public types

        /// <summary>
        /// Helps create and serialize data to JSON. 
        /// </summary><remarks>
        /// This is basically a "typedef" for Dictionary[string, object]
        /// with a helper to serialize to JSON.
        /// </remarks>
        public class JSObject : Dictionary<string, object>
        {
            /// <summary>
            /// Adds a double field, rounding to 4dp.
            /// </summary>
            public void add(string name, double value)
            {
                value = Math.Round(value, 4);
                this.Add(name, value);
            }

            /// <summary>
            /// Adds a string field.
            /// </summary>
            public void add(string name, string value)
            {
                this.Add(name, value);
            }

            /// <summary>
            /// Adds an object field, for example a list.
            /// </summary>
            public void add(string name, object value)
            {
                this.Add(name, value);
            }

            /// <summary>
            /// Converts the object to a JSON string.
            /// </summary>
            public string toJSON()
            {
                return Json.Encode(this);
            }
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Constructor.
        /// </summary>
        public CodingWorldCupAPI()
        {
            // Uncomment this line to launch the debugger when the AI starts up.
            // you can then set breakpoints in the rest of your code.
            //System.Diagnostics.Debugger.Launch();

            // We run the game loop...
            Logger.log("Starting game loop", Logger.LogLevel.INFO);
            for (; ; )
            {
                // We "listen" to game messages on stdin, and process them...
                var message = Console.ReadLine();
                processMessage(message);
            }
        }

        /// <summary>
        /// Sends a reply back to the game.
        /// </summary>
        public void sendReply(JSObject reply)
        {
            string strReply = reply.toJSON();
            Console.WriteLine(strReply);
        }

        #endregion

        #region Abstract and virtual methods

        /// <summary>
        /// Default implementation for the CONFIGURE_ABLITIES request.
        /// </summary><remarks>
        /// We give all players an average level of ability in all areas.
        /// </remarks>
        protected virtual void processRequest_ConfigureAbilities(dynamic data)
        {
            // As a default, we give all players equal abilities...
            double totalKickingAbility = data.totalKickingAbility;
            double totalRunningAbility = data.totalRunningAbility;
            double totalBallControlAbility = data.totalBallControlAbility;
            double totalTacklingAbility = data.totalTacklingAbility;

            int numberOfPlayers = this.allPlayers.Count;

            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "CONFIGURE_ABILITIES");

            // We give each player an average ability in all categories...
            var playerInfos = new List<JSObject>();
            foreach(var player in this.allPlayers)
            {
                var playerInfo = new JSObject();
                playerInfo.add("playerNumber", player.playerNumber);
                playerInfo.add("kickingAbility", totalKickingAbility / numberOfPlayers);
                playerInfo.add("runningAbility", totalRunningAbility / numberOfPlayers);
                playerInfo.add("ballControlAbility", totalBallControlAbility / numberOfPlayers);
                playerInfo.add("tacklingAbility", totalTacklingAbility / numberOfPlayers);
                playerInfos.Add(playerInfo);
            }
            reply.add("players", playerInfos);

            sendReply(reply);
        }

        /// <summary>
        /// Default implementation for the kickoff request.
        /// </summary><remarks>
        /// Returns a minimal response (which results in default positions
        /// being allocated).
        /// </remarks>
        protected virtual void processRequest_Kickoff(dynamic data)
        {
            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "KICKOFF");
            reply.add("players", new List<JSObject>());
            sendReply(reply);
        }

        /// <summary>
        /// Default implementation for the PLAY request.
        /// </summary><remarks>
        /// We send back an empty list of actions, which means that
        /// the players do nothing.
        /// </remarks>
        protected virtual void processRequest_Play(dynamic data)
        {
            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "PLAY");
            reply.add("actions", new List<JSObject>());
            sendReply(reply);
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Called when we have received a new message from the game-engine.
        /// </summary>
        private void processMessage(string message)
        {
            Logger.log("Received message: " + message, Logger.LogLevel.DEBUG);
            
            // We decode the JSON message, and process it depending
            // on its message type...
            var data = Json.Decode(message);
            string messageType = data.messageType; 
            switch(messageType)
            {
                case "EVENT":
                    processEvent(data);
                    break;

                case "REQUEST":
                    processRequest(data);
                    break;

                default:
                    throw new Exception("Unexpected messageType");
            }
        }

        /// <summary>
        /// Called when we receive an EVENT.
        /// </summary>
        private void processEvent(dynamic data)
        {
            string eventType = data.eventType;
            switch(eventType)
            {
                case "GAME_START":
                    processEvent_GameStart(data);
                    break;

                case "TEAM_INFO":
                    processEvent_TeamInfo(data);
                    break;

                case "START_OF_TURN":
                    processEvent_StartOfTurn(data);
                    break;

                case "KICKOFF":
                    processEvent_Kickoff(data);
                    break;

                default:
                    throw new Exception("Unexpected eventType: " + eventType);
            }
        }

        /// <summary>
        /// Called when we receive a REQUEST.
        /// </summary>
        private void processRequest(dynamic data)
        {
            string requestType = data.requestType;
            switch(requestType)
            {
                case "CONFIGURE_ABILITIES":
                    processRequest_ConfigureAbilities(data);
                    break;

                case "KICKOFF":
                    processRequest_Kickoff(data);
                    break;

                case "PLAY":
                    processRequest_Play(data);
                    break;

                default:
                    throw new Exception("Unexpected requestType");
            }
        }

        /// <summary>
        /// Called when we receive a GAME_START event.
        /// </summary>
        private void processEvent_GameStart(dynamic data)
        {
            this.pitch = data.pitch;
        }

        /// <summary>
        /// Called when we receive a TEAM_INFO event.
        /// </summary>
        private void processEvent_TeamInfo(dynamic data)
        {
            this.teamNumber = data.teamNumber;
            this.allPlayers = new List<dynamic>(data.players);
        }

        /// <summary>
        /// Called when we receive a START_OF_TURN event.
        /// </summary>
        private void processEvent_StartOfTurn(dynamic data)
        {
            this.gameState = data.game;
        }

        /// <summary>
        /// Called when we receive a KICKOFF event.
        /// </summary>
        private void processEvent_Kickoff(dynamic data)
        {
            // We store the team info (including playing direction and score)...
            if(this.teamNumber == 1)
            {
                // We are team 1...
                this.teamInfo = data.team1;
                this.opposingTeamInfo = data.team2;
            }
            else
            {
                // We are team 2...
                this.teamInfo = data.team2;
                this.opposingTeamInfo = data.team1;
            }

            // Are we the team kicking off?
            this.weAreKickingOff = (data.teamKickingOff == this.teamNumber);
        }

        #endregion

        #region Protected data

        // The dimensions of the pitch...
        protected dynamic pitch = null;

        // Our team number...
        protected int teamNumber = -1;

        // The collection of players in the team...
        protected IList<dynamic> allPlayers = null;

        // The game start at the start of each turn...
        protected dynamic gameState = null;

        // Info about our team. Includes the score and the direction of play...
        protected dynamic teamInfo = null;

        // Info about the opposing team. Includes the score and the direction of play...
        protected dynamic opposingTeamInfo = null;

        // True if we are kicking off.
        // (Only valid during a KICKOFF event and request.)
        protected bool weAreKickingOff = false;

        #endregion

    }
}

