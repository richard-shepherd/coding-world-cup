using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Helpers;

namespace BrillTown
{
    /// <summary>
    /// Helps manage the coding-world-cup API.
    /// 
    /// You can derive from this class and implement its abstract 
    /// functions in your API.
    /// </summary>
    public abstract class CodingWorldCupAPI
    {
        #region Public and protected methods

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
            Logger.log("Sending reply: " + strReply, Logger.LogLevel.DEBUG);
            Console.WriteLine(strReply);
        }

        /// <summary>
        /// Returns a Position for the centre of the requested goal.
        /// </summary>
        protected Position getGoalCentre(GoalType goal)
        {
            double x = 0.0;
            if ((goal == GoalType.OUR_GOAL && this.playingDirection == DirectionType.LEFT)
                ||
                (goal == GoalType.THEIR_GOAL && this.playingDirection == DirectionType.RIGHT))
            {
                x = this.pitch.width;
            }
            double y = this.pitch.goalCentre;
            return new Position(x, y);
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

            int numberOfPlayers = this.teamPlayers.Count + 1; // +1 for the goalkeeper

            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "CONFIGURE_ABILITIES");

            // We give each player an average ability in all categories...
            var playerInfos = new List<JSObject>();
            foreach(var playerNumber in this.allTeamPlayers.Keys)
            {
                var playerInfo = new JSObject();
                playerInfo.add("playerNumber", playerNumber);
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
        protected virtual void processRequest_Kickoff()
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
        protected virtual void processRequest_Play()
        {
            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "PLAY");
            reply.add("actions", new List<JSObject>());
            sendReply(reply);
        }

        /// <summary>
        /// Called after the TEAM_INFO event has been processed.
        /// (If the implementation in this class has been called.)
        /// </summary>
        protected virtual void onTeamInfoUpdated()
        {
        }

        /// <summary>
        /// Called after the GOAL event has been processed.
        /// (If the implementation in this class has been called.)
        /// </summary>
        protected virtual void onGoal()
        {
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
                    //throw new Exception("Unexpected messageType :" + messageType + " [" + message + "]");
                    Logger.log("Unexpected messageType :" + messageType + " [" + message + "]", Logger.LogLevel.DEBUG);
                    var reply = new JSObject();
                    reply.add("requestType", "PLAY");
                    var actions = new List<JSObject>();
                    reply.add("actions", actions);
                    sendReply(reply);
                    break;
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

                case "GOAL":
                    processEvent_Goal(data);
                    break;

                case "KICKOFF":
                    processEvent_Kickoff(data);
                    break;

                case "HALF_TIME":
                    processEvent_HalfTime(data);
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
                    processRequest_Kickoff();
                    break;

                case "PLAY":
                    processRequest_Play();
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
            this.gameLengthSeconds = data.gameLengthSeconds;
        }

        /// <summary>
        /// Called when we receive a TEAM_INFO event.
        /// </summary>
        private void processEvent_TeamInfo(dynamic data)
        {
            this.teamNumber = data.teamNumber;

            // We keep a collection of all the player-numbers in out team
            // as well as the information sorted by player vs. goalkeeper...
            foreach(var player in data.players)
            {
                int playerNumber = player.playerNumber;

                // We set up the all-players collection...
                this.allTeamPlayers[playerNumber] = new { };

                // And the player / goalkeeper split...
                if(player.playerType == "P")
                {
                    // This is a player...
                    this.teamPlayers[playerNumber] = new { };
                }
                else
                {
                    // This is the goalkeeper...
                    this.goalkeeperPlayerNumber = playerNumber;
                }
            }

            // We notify that team inf has been updated...
            onTeamInfoUpdated();
        }

        /// <summary>
        /// Called when we receive a START_OF_TURN event.
        /// </summary>
        private void processEvent_StartOfTurn(dynamic data)
        {
            // We store the whole game-state...
            this.gameState = data;

            // And we split up parts of it.

            // The time...
            this.gameTimeSeconds = (double)data.game.currentTimeSeconds;

            // The ball...
            this.ball = data.ball;

            // The teams...
            if(this.teamNumber == 1)
            {
                // We are team 1...
                storeTeamInfo(data.team1);
                storeOpposingTeamInfo(data.team2);
            }
            else
            {
                // We are team 2...
                storeTeamInfo(data.team2);
                storeOpposingTeamInfo(data.team1);
            }

            if (this.gameTimeSeconds > this.gameLengthSeconds)
            {
                // Game is about to end log score to summary file
                string message;
                if (this.teamNumber == 1)
                {
                    // We are team 1...
                    message = String.Format("{0} {1} - {2} {3}", this.teamInfo.name, this.teamScore.ToString(), this.otherScore.ToString(), this.opposingTeamInfo.name);
                }
                else
                {
                    // We are team 2...
                    message = String.Format("{0} {1} - {2} {3}", this.opposingTeamInfo.name, this.otherScore.ToString(), this.teamScore.ToString(), this.teamInfo.Name);
                }

                var staticState1 = " {";
                foreach (var player in this.gameState.team1.players) {
                     var staticState = player.staticState;
                     staticState1 += "(" + staticState.playerNumber + "," + staticState.ballControlAbility.ToString("F") + "," + staticState.kickingAbility.ToString("F") + "," + staticState.runningAbility.ToString("F") + "," + staticState.tacklingAbility.ToString("F") + ") ";
                }
                staticState1 += "} ";

                var staticState2 = " {";
                foreach (var player in this.gameState.team2.players) {
                    var staticState = player.staticState;
                    staticState2 += "(" + staticState.playerNumber + "," + staticState.ballControlAbility.ToString("F") + "," + staticState.kickingAbility.ToString("F") + "," + staticState.runningAbility.ToString("F") + "," + staticState.tacklingAbility.ToString("F") + ") ";
                }
                staticState2 += "} ";

                message += staticState1 + staticState2;
                Logger.summary(message);
            }
        }

        /// <summary>
        /// Stores info about our team in our internal collections.
        /// </summary>
        private void storeTeamInfo(dynamic teamInfo)
        {
            // We store info about each player in the team in a map 
            // by player-number, and we also split out the player info
            // from the goalkeeper info...
            foreach (var playerInfo in teamInfo.players)
            {
                var staticState = playerInfo.staticState;
                int playerNumber = staticState.playerNumber;

                // We store all the players in one collection...
                this.allTeamPlayers[playerNumber] = playerInfo;

                // And split by player vs. goalkeeper...
                string playerType = staticState.playerType;
                if(playerType == "P")
                {
                    // This is a player...
                    this.teamPlayers[playerNumber] = playerInfo;
                }
                else
                {
                    // This is the goalkeeper...
                    this.goalkeeper = playerInfo;
                }
            }
        }

        /// <summary>
        /// Stores info about the opposing team in our internal collections.
        /// </summary>
        private void storeOpposingTeamInfo(dynamic teamInfo)
        {
            // We store info about each player in the opposing team 
            // in a map by player-number...
            foreach(var playerInfo in teamInfo.players)
            {
                int playerNumber = playerInfo.staticState.playerNumber;
                this.allOpposingTeamPlayers[playerNumber] = playerInfo;
            }
        }

        /// <summary>
        /// Called when a goal is scored.
        /// </summary>
        private void processEvent_Goal(dynamic data)
        {
            if (this.teamNumber == 1)
            {
                // We are team 1...
                this.teamScore  = data.team1.score;
                this.otherScore = data.team2.score;
            }
            else
            {
                // We are team 2...
                this.teamScore  = data.team2.score;
                this.otherScore = data.team1.score;
            }

            // We notify the derived class...
            onGoal();
        }

        /// <summary>
        /// Called at half-time.
        /// </summary>
        private void processEvent_HalfTime(dynamic data)
        {
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

            // We find the direction we are playing...
            if(this.teamInfo.direction == "LEFT")
            {
                this.playingDirection = DirectionType.LEFT;
            }
            else
            {
                this.playingDirection = DirectionType.RIGHT;
            }

            // Are we the team kicking off?
            this.weAreKickingOff = (data.teamKickingOff == this.teamNumber);
        }

        #endregion

        #region Protected data

        // The dimensions of the pitch...
        protected dynamic pitch = new { };

        // The lngth of the game in seconds...
        protected int gameLengthSeconds = -1;

        // Our team number...
        protected int teamNumber = -1;

        protected int teamScore = 0;
        protected int otherScore = 0;

        // The collection of all players in our team...
        // This is a map of player-number to information about the player.
        protected Dictionary<int, dynamic> allTeamPlayers = new Dictionary<int, dynamic>();

        // The collection of players, not including the goalkeeper.
        // This is a map of player-number to information about the player.
        protected Dictionary<int, dynamic> teamPlayers = new Dictionary<int, dynamic>();

        // Information about the goalkeeper...
        protected int goalkeeperPlayerNumber = -1;
        protected dynamic goalkeeper = new { };

        // The collection of all players in the opposing team...
        // This is a map of player-number to information about the player.
        protected Dictionary<int, dynamic> allOpposingTeamPlayers = new Dictionary<int, dynamic>();

        // The game start at the start of each turn.
        // Some of this data is split up into the 'ball' info and the 
        // 'players' info...
        protected dynamic gameState = new { };

        // Information about the ball, including its position, direction and speed...
        protected dynamic ball = new { };

        // Info about our team. Includes the score and the direction of play...
        protected dynamic teamInfo = new { };

        // Info about the opposing team. Includes the score and the direction of play...
        protected dynamic opposingTeamInfo = new { };

        // True if we are kicking off.
        // (Only valid during a KICKOFF event and request.)
        protected bool weAreKickingOff = false;

        // The direction we are playing...
        protected enum DirectionType
        {
            // We don't yet know the playing direction...
            DONT_KNOW,

            // We are shooting at the left goal...
            LEFT,

            // We are shooting at the right goal...
            RIGHT
        }
        protected DirectionType playingDirection;

        // An enum for the two goals...
        protected enum GoalType
        {
            OUR_GOAL,
            THEIR_GOAL
        }

        // The current 'game-time' in seconds from the start of the match...
        protected double gameTimeSeconds;

        #endregion

    }
}

