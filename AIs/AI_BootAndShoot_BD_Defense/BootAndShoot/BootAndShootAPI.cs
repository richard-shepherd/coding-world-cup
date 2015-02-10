using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Helpers;

namespace BootAndShoot
{
    public class BootAndShootAPI
    {
        #region Public Methods
        public BootAndShootAPI()
        {
            // Uncomment this line to launch the debugger when the AI starts up.
            // you can then set breakpoints in the rest of your code.
           // System.Diagnostics.Debugger.Launch();

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
        #endregion

        #region Private Methods

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
            switch (messageType)
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
            switch (eventType)
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
            switch (requestType)
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
        /// Called at half-time.
        /// </summary>
        private void processEvent_HalfTime(dynamic data)
        {
        }
        /// <summary>
        /// Called when a goal is scored.
        /// </summary>
        private void processEvent_Goal(dynamic data)
        {
            var message = String.Format("{0}: GOAL!!!", this.gameTimeSeconds);
            Logger.log(message, Logger.LogLevel.INFO);
            
        }
        /// <summary>
        /// Called when we receive a START_OF_TURN event.
        /// </summary>
        private void processEvent_StartOfTurn(dynamic data)
        {

            // The time...
            this.gameTimeSeconds = (double)data.game.currentTimeSeconds;

            // The ball...
            team.pitch.ball.SetValues(data.ball);
            // The teams...
            if (this.team.teamNumber == 1)
            {
                // We are team 1...
                team.StoreInfo(data.team1);
                team.Opponents.StoreInfo(data.team2);
            }
            else
            {
                // We are team 2...
                team.StoreInfo(data.team2);
                team.Opponents.StoreInfo(data.team1);
            }
            team.Update();
        }

        /// <summary>
        /// Called when we receive a GAME_START event.
        /// </summary>
        private void processEvent_GameStart(dynamic data)
        {
            this.pitch = GamePitch.instance;
            this.pitch.SetData(data.pitch);
        }

                /// <summary>
        /// Called when we receive a TEAM_INFO event.
        /// </summary>
        private void processEvent_TeamInfo(dynamic data)
        {
            this.team = new Team(data);
            this.team.pitch = pitch;
            //initializing supportingcal after intialization of pitch
            team.supportSpotCalc = new SupportSpotCalculator(13, 6, this.team);

            int oppTeamNo = this.team.teamNumber == 1 ? 2 : 1;
            this.team.Opponents = new Team(oppTeamNo);
        }
        private void processEvent_Kickoff(dynamic data)
        {
            // We store the team info (including playing direction and score)...
            if (this.team.teamNumber == 1)
            {
                // We are team 1...
                team.SetDetails(data.team1);
                team.Opponents.SetDetails(data.team2);
            }
            else
            {
                // We are team 2...
                team.SetDetails(data.team2);
                team.Opponents.SetDetails(data.team1);
            }

            team.GetFSM().ChangeState(KickOffState.instance);






        }

        /// <summary>
        /// Default implementation for the CONFIGURE_ABLITIES request.
        /// </summary><remarks>
        /// We give all players an average level of ability in all areas.
        /// </remarks>
        private void processRequest_ConfigureAbilities(dynamic data)
        {

            // As a default, we give all players equal abilities...
            double totalKickingAbility = data.totalKickingAbility;
            double totalRunningAbility = data.totalRunningAbility;
            double totalBallControlAbility = data.totalBallControlAbility;
            double totalTacklingAbility = data.totalTacklingAbility;


            double playerKickingAbility = 0;
            double playerRunningAbility = 0;
            double playerBallControlAbility = 0;
            double playerTacklingAbility = 0;

            int numberOfPlayers = this.team.Members.Count;

            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "CONFIGURE_ABILITIES");

            // We give each player an average ability in all categories...
            var playerInfos = new List<JSObject>();
            foreach (var playerNumber in this.team.Members.Keys)
            {
                var player = team.Members[playerNumber];
                int full = 100;
                int zero = 0;
                int value = full;
                if (player.role == Player.player_role.defender)
                {
                    playerKickingAbility = value;//50
                    playerRunningAbility = value;//60
                    playerBallControlAbility = value;//70
                    playerTacklingAbility = value;//80

                }
                else if (player.role == Player.player_role.attacker)
                {
                    playerKickingAbility = value;//90
                    playerRunningAbility = value;//80
                    playerBallControlAbility = value;//95
                    playerTacklingAbility = value;//80

                }
                else if (player.role == Player.player_role.goal_keeper)
                {
                    //goalkeeper
                    playerKickingAbility = value;//70
                    playerRunningAbility = value;//60
                    playerBallControlAbility = value;//0
                    playerTacklingAbility = value;//0
                }
                else
                {
                    value = zero;
                    playerKickingAbility = value;//70
                    playerRunningAbility = value;//60
                    playerBallControlAbility = value;//0
                    playerTacklingAbility = value;//0
                }


                var playerInfo = new JSObject();
                playerInfo.add("playerNumber", playerNumber);
                playerInfo.add("kickingAbility", playerKickingAbility);
                playerInfo.add("runningAbility", playerRunningAbility);
                playerInfo.add("ballControlAbility", playerBallControlAbility);
                playerInfo.add("tacklingAbility", playerTacklingAbility);
                playerInfos.Add(playerInfo);
            }
            reply.add("players", playerInfos);

            sendReply(reply);
        }




        /// <summary>
        /// Called when we receive the kickoff request.
        /// We reply with the positions we set up (for the current playing direction)
        /// set up (in onTeamInfoUpdated) above.
        /// </summary>
        private void processRequest_Kickoff()
        {
            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "KICKOFF");

            // We set the position for each member of the team.
            // Note: We are only setting the positions of the players.
            //       We will use the default position of the goalkeeper 
            //       assigned to us by the game.
            var players = new List<JSObject>();
            foreach (Player player in team.Members.Values)
            {

                // We create an object to hold the kickoff position and 
                // direction for this player...
                Position destination;
                if(player.playerType == "P" ) 
                {
                    destination = new Position(player.HomeRegion().center);
                }
                else
                {
                    destination = new Position();
                    destination.y = 25.0;
                    destination.x = (player.team.playingDirection == global::BootAndShoot.Team.DirectionType.RIGHT) ? 0.5 : 99.5;
                }
                var response = new JSObject();
                response.add("playerNumber", player.playerNumber);
                response.add("position", destination);
                response.add("direction", player.team.Direction());
                players.Add(response);

                player.action = null;
                
            }
            reply.add("players", players);

            sendReply(reply);
        }
        
                /// <summary>
        /// Called when we receive a PLAY request. We act as discussed in
        /// the heading comments.
        /// </summary>
        private void processRequest_Play()
        {
            // We create the reply object...
            var reply = new JSObject();
            reply.add("requestType", "PLAY");
            var actions = new List<JSObject>();
            foreach(Player player in team.Members.Values)
            {
                if(player.action != null)
                {

                }
                actions.Add(player.action.ToJson());           
                player.action = null;
            }
            reply.add("actions", actions);
            sendReply(reply);
            Logger.log(gameTimeSeconds + " :----------End Turn----------", Logger.LogLevel.INFO);
            
        }

        #endregion

        #region Private Data
        private Team team;
        private GamePitch pitch;

        // The current 'game-time' in seconds from the start of the match...
        private double gameTimeSeconds;


        #endregion
    }
}
