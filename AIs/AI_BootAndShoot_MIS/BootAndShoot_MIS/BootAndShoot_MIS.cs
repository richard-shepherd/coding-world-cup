using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Helpers;

//using System.Diagnostics;


namespace BootAndShoot
{
    /// <summary>
    /// A coding-world-cup API which:
    /// - Keeps three players in its own half as defenders.
    /// - Puts two players in the opponent's half as forwards.
    /// 
    /// Defenders
    /// ---------
    /// The defenders try to take possession of the ball when it
    /// is in their half. If they get it, they kick it towards the
    /// forwards.
    /// 
    /// When the ball is in the other half, they move back to their
    /// default positions.
    /// 
    /// Forwards
    /// --------
    /// The forwards try to take possession when the ball is in the
    /// opponent's half. If they get it, they shoot for the goal.
    /// 
    /// When the ball is in the other half, they move back to their
    /// default positions.
    /// 
    /// Goalkeeper
    /// ----------
    /// The goalkeeper tries to keep between the ball and the goal centre.
    /// He tries to take possession when the ball is close.
    /// </summary><remarks>
    /// Derives from CodingWorldCupAPI and implements various virtual methods.
    /// </remarks>
    class BootAndShoot : CodingWorldCupAPI
    {
        #region CodingWorldCupAPI implementation

        
        /// <summary>
        /// Called when team-info has been updated.
        /// 
        /// At this point we know the player-numbers for the players
        /// in our team, so we can choose which players are playing in
        /// which positions.
        /// </summary>
        protected override void onTeamInfoUpdated()
        {
            // We assign players to the positions...
            var playerNumbers = new List<int>(this.teamPlayers.Keys);
            this.playerNumber_LeftWingDefender = playerNumbers[0];
            this.playerNumber_CentreDefender = playerNumbers[1];
            this.playerNumber_RightWingDefender = playerNumbers[2];
            this.playerNumber_LeftWingForward = playerNumbers[3];
            this.playerNumber_RightWingForward = playerNumbers[4];

            // We set up the default positions...
            setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.RIGHT, new Position(25, 40), 90);
            setDefaultPosition(this.playerNumber_CentreDefender, DirectionType.RIGHT, new Position(17, 25), 90);
            setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(17, 23), 90);
            setDefaultPosition(this.playerNumber_LeftWingForward, DirectionType.RIGHT, new Position(71, 15), 90);
            setDefaultPosition(this.playerNumber_RightWingForward, DirectionType.RIGHT, new Position(71, 35), 90);

            setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.LEFT, new Position(75, 40), 270);
            setDefaultPosition(this.playerNumber_CentreDefender, DirectionType.LEFT, new Position(83, 25), 270);
            setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(83, 22), 270);
            setDefaultPosition(this.playerNumber_LeftWingForward, DirectionType.LEFT, new Position(29, 35), 270);
            setDefaultPosition(this.playerNumber_RightWingForward, DirectionType.LEFT, new Position(29, 15), 270);


            // We set up the kickoff positions. 
            // Note: The player nearest the centre will be automatically "promoted"
            //       to the centre by the game.
            setKickoffPosition(this.playerNumber_LeftWingDefender, DirectionType.RIGHT, new Position(25, 40), 90);
            setKickoffPosition(this.playerNumber_CentreDefender, DirectionType.RIGHT, new Position(17, 25), 90);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(17, 23), 90);
            setKickoffPosition(this.playerNumber_LeftWingForward, DirectionType.RIGHT, new Position(49, 14), 90);
            setKickoffPosition(this.playerNumber_RightWingForward, DirectionType.RIGHT, new Position(49, 36), 90);

            setKickoffPosition(this.playerNumber_LeftWingDefender, DirectionType.LEFT, new Position(75, 40), 270);
            setKickoffPosition(this.playerNumber_CentreDefender, DirectionType.LEFT, new Position(83, 25), 270);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(83, 22), 270);
            setKickoffPosition(this.playerNumber_LeftWingForward, DirectionType.LEFT, new Position(51, 36), 270);
            setKickoffPosition(this.playerNumber_RightWingForward, DirectionType.LEFT, new Position(51, 14), 270);
        }

        /// <summary>
        /// Called when we receive the kickoff request.
        /// We reply with the positions we set up (for the current playing direction)
        /// set up (in onTeamInfoUpdated) above.
        /// </summary>
        protected override void processRequest_Kickoff()
        {
            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "KICKOFF");

            // We set the position for each member of the team.
            // Note: We are only setting the positions of the players.
            //       We will use the default position of the goalkeeper 
            //       assigned to us by the game.
            var players = new List<JSObject>();
            foreach(var pair in this.kickoffPositions)
            {
                int playerNumber = pair.Key.Item1;
                var playingDirection = pair.Key.Item2;

                // We only want values for this kickoff's paying direction...
                if (playingDirection == this.playingDirection)
                {
                    // We create an object to hold the kickoff position and 
                    // direction for this player...
                    var player = new JSObject();
                    player.add("playerNumber", playerNumber);
                    player.add("position", pair.Value.position.toJSObject());
                    player.add("direction", pair.Value.direction);
                    players.Add(player);
                }
            }
            reply.add("players", players);

            sendReply(reply);
        }

        /// <summary>
        /// Called when we receive a PLAY request. We act as discussed in
        /// the heading comments.
        /// </summary>
        protected override void processRequest_Play()
        {
            // We create the reply object...
            var reply = new JSObject();
            reply.add("requestType", "PLAY");
            var actions = new List<JSObject>();

            try
            {
                // static player action
                getAction_staticPlayerAction(ref actions);
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception in getAction_staticPlayerAction: {0} \nStack:\n{1}", ex.Message, ex.StackTrace);
                Logger.log(message, Logger.LogLevel.ERROR);
            }

            try
            {
                // Goal Keeper action
                getAction_Goalkeeper(actions);
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception in getAction_Goalkeeper: {0} \nStack:\n{1}", ex.Message, ex.StackTrace);
                Logger.log(message, Logger.LogLevel.ERROR);
            }
            
            try
            {
                // dynamic player action
                getAction_ForwardDefender(actions);
            }
            catch (Exception ex)
            {
                string message = String.Format("Exception in getAction_ForwardDefender: {0} \nStack:\n{1}", ex.Message, ex.StackTrace);
                Logger.log(message, Logger.LogLevel.ERROR);
            }
            
            reply.add("actions", actions);
            sendReply(reply);
        }

        /// <summary>
        /// Called when a goal has been scored.
        /// </summary>
        protected override void onGoal()
        {
            var message = String.Format("{0}: GOAL!!!", this.gameTimeSeconds);
            Logger.log(message, Logger.LogLevel.INFO);
        }


        private void calculateTargetCoordinates()
        {
            double angle = 100.0;
            double radius = 10.0;
            double x, y =0.0;

            while (angle != 0.0)
            {
                angle = angle -10;
                x = Math.Round(radius * Math.Cos(angle * Math.PI/180), 2);
                y = Math.Round(radius * Math.Sin(angle * Math.PI/180 ), 2);
                
                PositionValue posValue = new PositionValue();
                posValue.position=new Position(x, y);
                posValue.direction = 90.0 - angle;
                getSignCoordinates(posValue.direction, posValue.position);
                coordinates10RadiusRIGHT.Add(posValue);
                if ((90.0 - angle) == 0.0)
                    coordinates10RadiusLEFT.Add(posValue);

                PositionValue posValueOpp = new PositionValue();
                posValueOpp.position = new Position(x, y);
                posValueOpp.direction = 90.0 - angle + 180;
                getSignCoordinates(posValueOpp.direction, posValueOpp.position);
                coordinates10RadiusLEFT.Add(posValueOpp);
                if ((90.0 - angle + 180) == 180.0)
                    coordinates10RadiusRIGHT.Add(posValueOpp);

                if (((90.0 - angle) != 0.0) && ( angle != 0.0))
                {
                    PositionValue posValue1 = new PositionValue();
                    posValue1.position = new Position(x, y);
                    posValue1.direction = 360 - (90.0 - angle);
                    getSignCoordinates(posValue1.direction, posValue1.position);
                    coordinates10RadiusLEFT.Add(posValue1);

                    PositionValue posValue2 = new PositionValue();
                    posValue2.position = new Position(x, y);
                    posValue2.direction = 180 - (90.0 - angle);
                    getSignCoordinates(posValue2.direction, posValue2.position);
                    coordinates10RadiusRIGHT.Add(posValue2);
                }
               

            }


        }

        private void getSignCoordinates(double direction, Position p)
        {
            if (direction >=0  && direction <90)
            {
                p.y = -p.y;
            }
            else if (direction >=90  && direction <= 180)
            {
                //do nothing
            }
            else if (direction > 180  && direction < 270)
            {
                p.x = -p.x;
            }
            else if (direction >= 270 && direction < 360 )
            {
                p.x = -p.x;
                p.y = -p.y;
            }
        }

        protected override void processRequest_ConfigureAbilities(dynamic data)
        {
            calculateTargetCoordinates();

            ProjectedPositions[playerNumber_LeftWingForward] = null;
            ProjectedPositions[playerNumber_RightWingForward] = null;
            ProjectedPositions[goalkeeperPlayerNumber] = null;
            ProjectedPositions[playerNumber_LeftWingDefender] = null;
            ProjectedPositions[playerNumber_RightWingDefender] = null;
            ProjectedPositions[playerNumber_CentreDefender] = null;
            
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

            var playerInfo = new JSObject();
            playerInfo.add("playerNumber", playerNumber_LeftWingForward);
            playerInfo.add("kickingAbility", totalKickingAbility * 0.25);
            playerInfo.add("runningAbility", totalRunningAbility * 0.25);
            playerInfo.add("ballControlAbility", totalBallControlAbility * 0.25);
            playerInfo.add("tacklingAbility", totalTacklingAbility *0.25);
            playerInfos.Add(playerInfo);


            playerInfo = new JSObject();
            playerInfo.add("playerNumber", playerNumber_RightWingForward);
            playerInfo.add("kickingAbility", totalKickingAbility * 0.25);
            playerInfo.add("runningAbility", totalRunningAbility * 0.25);
            playerInfo.add("ballControlAbility", totalBallControlAbility * 0.25);
            playerInfo.add("tacklingAbility", totalTacklingAbility * 0.25);
            playerInfos.Add(playerInfo);

            playerInfo = new JSObject();
            playerInfo.add("playerNumber", playerNumber_LeftWingDefender);
            playerInfo.add("kickingAbility", totalKickingAbility * 0.25);
            playerInfo.add("runningAbility", totalRunningAbility * 0.25);
            playerInfo.add("ballControlAbility", totalBallControlAbility * 0.25);
            playerInfo.add("tacklingAbility", totalTacklingAbility * 0.25);
            playerInfos.Add(playerInfo);

            playerInfo = new JSObject();
            playerInfo.add("playerNumber", goalkeeperPlayerNumber);
            playerInfo.add("kickingAbility", totalKickingAbility * 0.23);
            playerInfo.add("runningAbility", totalRunningAbility * 0.25);
            playerInfo.add("ballControlAbility", totalBallControlAbility * 0.15);
            playerInfo.add("tacklingAbility", totalTacklingAbility * 0.15);
            playerInfos.Add(playerInfo);

            playerInfo = new JSObject();
            playerInfo.add("playerNumber", playerNumber_RightWingDefender);
            playerInfo.add("kickingAbility", totalKickingAbility * 0.01);
            playerInfo.add("runningAbility", totalRunningAbility * 0.00);
            playerInfo.add("ballControlAbility", totalBallControlAbility * 0.05);
            playerInfo.add("tacklingAbility", totalTacklingAbility * 0.05);
            playerInfos.Add(playerInfo);


            playerInfo = new JSObject();
            playerInfo.add("playerNumber", playerNumber_CentreDefender);
            playerInfo.add("kickingAbility", totalKickingAbility * 0.01);
            playerInfo.add("runningAbility", totalRunningAbility * 0.00);
            playerInfo.add("ballControlAbility", totalBallControlAbility * 0.05);
            playerInfo.add("tacklingAbility", totalTacklingAbility * 0.05);
            playerInfos.Add(playerInfo);

            /*
            foreach (var playerNumber in this.allTeamPlayers.Keys)
            {
                if (playerNumber == playerNumber_LeftWingForward || playerNumber == playerNumber_RightWingForward)
                {
                     playerKickingAbility = 100;
                     playerRunningAbility = 100;
                     playerBallControlAbility = 100;
                     playerTacklingAbility = 100;
                    
                    totalKickingAbility = totalKickingAbility -100;
                    totalRunningAbility = totalRunningAbility -100;
                    totalBallControlAbility = totalBallControlAbility -100;
                    totalTacklingAbility = totalTacklingAbility - 100;


                 }
                 else if( playerNumber == playerNumber_LeftWingDefender || playerNumber == playerNumber_RightWingDefender)
                 {
                     

                    playerKickingAbility = 40;
                    playerRunningAbility = 40;
                    playerBallControlAbility = 40;
                    playerTacklingAbility = 40;


                     
                }
                else if (playerNumber == playerNumber_LeftWingDefender)
                {
                    playerKickingAbility = 100;
                    playerRunningAbility = 100;
                    playerBallControlAbility = 100;
                    playerTacklingAbility = 100;

                }
                else
                {
                    //goalkeeper
                    playerKickingAbility = 100;
                    playerRunningAbility = 100;
                    playerBallControlAbility = 100;
                    playerTacklingAbility = 100;
                }
                var playerInfo = new JSObject();
                playerInfo.add("playerNumber", playerNumber);
                playerInfo.add("kickingAbility", playerKickingAbility);
                playerInfo.add("runningAbility", playerRunningAbility);
                playerInfo.add("ballControlAbility", playerBallControlAbility);
                playerInfo.add("tacklingAbility", playerTacklingAbility);
                playerInfos.Add(playerInfo);
            }*/
            reply.add("players", playerInfos);

            sendReply(reply);
        }


        #endregion

        #region Private functions

        /// <summary>
        /// Return true if the player is closer to the ball compared to other players.
        /// </summary>
        private double getPlayerBallDistance(Position p1, Position p2)
        {
            return p1.distanceFrom(p2);
        }

        private void getOtherPlayerSecondNearToBall(int playerNumber, Position ballPosition, ref int otherPlayerNumber)
        {

            foreach (var player in allTeamPlayers.Values)
            {
                if ((playerNumber_CentreDefender == (int)player.staticState.playerNumber) ||
                    (playerNumber_RightWingDefender == (int)player.staticState.playerNumber) ||
                     (playerNumber == (int)player.staticState.playerNumber))
                    continue;

                if (IsPlayerSecondNearToBall((int)player.staticState.playerNumber, ballPosition))
                {
                    otherPlayerNumber = (int)player.staticState.playerNumber;
                    return;
                }
            }
        }
        
        private bool IsPlayerSecondNearToBall(int playerNumber, Position ballPosition)
        {
            var currplayer = this.allTeamPlayers[playerNumber];
            var playerPosition = new Position(currplayer.dynamicState.position);
            double distance = getPlayerBallDistance(playerPosition, ballPosition);
            double scale = (double)(currplayer.staticState.runningAbility) / 100;

            distance = distance / scale;
            int count = 0;
            // for loop
            foreach(var player in allTeamPlayers.Values)
            {
                if ((playerNumber_CentreDefender == (int)player.staticState.playerNumber) ||
                    (playerNumber_RightWingDefender == (int)player.staticState.playerNumber) ||
                    (playerNumber == (int)player.staticState.playerNumber))
                    continue;
                var otherPlayerPosition = new Position(player.dynamicState.position);
                double otherPlayerdistance = getPlayerBallDistance(otherPlayerPosition, ballPosition);
                double otherPlayerScale = (double)(player.staticState.runningAbility)/100;
                otherPlayerdistance = otherPlayerdistance / otherPlayerScale;
                if ((otherPlayerdistance <= distance) && (player.staticState.playerType != "G"))
                {
                    ++count;
                }
            }

            if (count == 1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Return true if the player is closer to the ball compared to other players.
        /// </summary>
        private bool IsPlayerNearToBall(int playerNumber, Position ballPosition)
        {
            var currplayer = this.allTeamPlayers[playerNumber];
            var playerPosition = new Position(currplayer.dynamicState.position);
            double scale = (double)(currplayer.staticState.runningAbility) / 100;
            double distance = getPlayerBallDistance(playerPosition, ballPosition)/scale;

            // for loop
            foreach(var player in allTeamPlayers.Values)
            {
                var otherPlayerPosition = new Position(player.dynamicState.position);
                double otherPlayerdistance = getPlayerBallDistance(otherPlayerPosition, ballPosition);
                double otherPlayerScale = (double)(player.staticState.runningAbility)/100;
                otherPlayerdistance = otherPlayerdistance / otherPlayerScale;
                if ((otherPlayerdistance < distance) && (player.staticState.playerType != "G"))
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Works out the action we want to take for this defender.
        /// </summary>
        private void getAction_Defender(List<JSObject> actions, int playerNumber)
        {
            var player = this.allTeamPlayers[playerNumber];
            var ballPosition = this.ball.position;

            if(player.dynamicState.hasBall)
            {
                // The player has the ball, so we kick it to one of the forwards...
                getAction_Defender_HasBall(actions, player);
            }
            else if ((this.playingDirection == DirectionType.RIGHT && ballPosition.x < this.pitch.centreSpot.x)
                     ||
                     (this.playingDirection == DirectionType.LEFT && ballPosition.x > this.pitch.centreSpot.x)
                     ||
                 (IsPlayerNearToBall(playerNumber, new Position(this.ball.position))))
            {
                // The ball is in the defender's half, so he tries to get it...
                if (IsPlayerNearToBall(playerNumber, new Position(this.ball.position)))
                {
                    getAction_OwnHalf(ref actions, player);
                    return;
                }

                if (IsNextPositionSet(playerNumber))
                {
                    getAction_nextPosition(actions, player);
                    return;
                }

                // The ball is in the other half, so the player moves back 
                // to his default position...
                var defaultPosition = getDefaultPosition(playerNumber, this.playingDirection);
                moveToPosition(ref actions, playerNumber, defaultPosition.position);
                /*var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", defaultPosition.position);
                action.add("speed", 100.0);
                actions.Add(action);*/
            }
            else
            {
                // The ball is in the other half, so the player moves back 
                // to his default position...
                var defaultPosition = getDefaultPosition(playerNumber, this.playingDirection);
                moveToPosition(ref actions, playerNumber, defaultPosition.position);
                /*var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", defaultPosition.position);
                action.add("speed", 100.0);
                actions.Add(action);*/
            }
        }


        private void getAction_Defender_HasBall1(List<JSObject> actions, dynamic player)
        {
            int playerNumber = player.staticState.playerNumber;
            var playerPosition = new Position(player.dynamicState.position);
            Position goalkeeperPosition = new Position(this.goalkeeper.dynamicState.position);

            bool opponentClose = isOpponentWithinRadius(playerPosition, 1.0);
            Vector v = new Vector(playerPosition, goalkeeperPosition);
            if (playerPosition.distanceFrom(goalkeeperPosition) > 10)
                v= v.getScaledVector(10);
            else v = v.getScaledVector(playerPosition.distanceFrom(goalkeeperPosition));
            Position destination = playerPosition.getPositionPlusVector(v);

            double absAngle = getAbsoluteAngle(playerPosition, destination);

            double angleDiff = Math.Abs(absAngle - (double)player.dynamicState.direction);

            // if (angleDiff <= 10.0) // CHECK IF player is in appropriate direction to kick the ball
            {
                // We kick the ball towards him...
                if (destination != null)
                {
                    var action = new JSObject();
                    action.add("action", "KICK");
                    action.add("playerNumber", playerNumber);
                    action.add("destination", destination);
                    action.add("speed", 50.0);
                    actions.Add(action);
                }

                //Logger.log(String.Format("{0}: Defender {1} kicks the ball to Player {2} with position {3}", this.gameTimeSeconds, playerNumber, destPlayer.staticState.playerNumber, destination), Logger.LogLevel.INFO);
            }
            
        }

        /// <summary>
        /// Kicks the ball from a defending player to a forward.
        /// </summary>
        private void getAction_Defender_HasBall(List<JSObject> actions, dynamic player)
        {
            int playerNumber = player.staticState.playerNumber;
            // find the next player to pass
            // 1. find nearest player
            // 2. find the line of projection - ball motion if kicked.

            var LeftWingForward = this.allTeamPlayers[this.playerNumber_LeftWingForward];
            var RightWingForward = this.allTeamPlayers[this.playerNumber_RightWingForward];

            var Leftdestination = new Position(LeftWingForward.dynamicState.position);
            var Rightdestination = new Position(RightWingForward.dynamicState.position);
            var playerPosition = new Position(player.dynamicState.position);

            double leftDist = getPlayerBallDistance(playerPosition, Leftdestination);
            double rightDist = getPlayerBallDistance(playerPosition, Rightdestination);

            var destination = Rightdestination;
            var destPlayer = RightWingForward;
            if (leftDist <= rightDist)
            {
                destination = Leftdestination;
                destPlayer = LeftWingForward;
            }

            double absAngle = getAbsoluteAngle(playerPosition, destination);

            double angleDiff = Math.Abs(absAngle - (double)player.dynamicState.direction);

            if (angleDiff <= 10.0) // CHECK IF player is in appropriate direction to kick the ball
            {
                // We kick the ball towards him...
                if (destination != null)
                {
                    var action = new JSObject();
                    action.add("action", "KICK");
                    action.add("playerNumber", playerNumber);
                    action.add("destination", destination);
                    action.add("speed", 100.0);
                    actions.Add(action);
                }

                Logger.log(String.Format("{0}: Defender {1} kicks the ball to Player {2} with position {3}", this.gameTimeSeconds, playerNumber, destPlayer.staticState.playerNumber, destination), Logger.LogLevel.INFO);
            }
            else
            {
                //ask the player to turn
                var action = new JSObject();
                action.add("action", "TURN");
                action.add("playerNumber", playerNumber);
                action.add("direction", absAngle);
                actions.Add(action);

                Logger.log(String.Format("{0}: Defender {1} current direction {2} turns to direction {3}", this.gameTimeSeconds, playerNumber, player.dynamicState.direction, absAngle), Logger.LogLevel.INFO);
            }
        }

        private bool isBallMovingAway(Position playerPosition)
        {
            Position ballPosition = new Position(this.ball.position);
            double speed = (double)this.ball.speed;
            Vector v = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
            v = v.getScaledVector(0.2);
            Position nextBallPosition = ballPosition.getPositionPlusVector(v);
            if (playerPosition.distanceFrom(ballPosition) < playerPosition.distanceFrom(nextBallPosition))
                return true;
            else 
                return false;
        }

        /// <summary>
        /// The player tries to get the ball.
        /// </summary>
        private void getAction_OwnHalf(ref List<JSObject> actions, dynamic player)
        {
            // if ball is in our goal radius move to certain position  
            // Is the ball already controlled by someone in our own team?
            if (this.allTeamPlayers.ContainsKey(this.ball.controllingPlayerNumber))
            {
                // The ball is already controlled by this team...
                return;
            }

            // The ball is not controlled by us, so we try to take possession...
            int playerNumber = player.staticState.playerNumber;

            // If we are less than 5m from the ball, we try to take possession.
            // If we are further away, we move towards it.
            var playerPosition = new Position(player.dynamicState.position);
            Position ballPosition = new Position(this.ball.position);
            double distance = playerPosition.distanceFrom(ballPosition);
			double speed = (double)this.ball.speed;

            if ( (distance < 0.5) || 
                ((distance < 1.0) && (speed <=0)) ||
                ((distance <= 0.8) && !isOpponentWithinRadius(playerPosition, 0.5) && !isBallMovingAway(playerPosition)))
            {
                //TO TEST
                // We attempt to take possession of the ball...
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "TAKE_POSSESSION");
                actions.Add(action);
                Logger.log(String.Format("{0}: Player {1} current position {2} attempts to take possession of ball position {3} ball speed{4}", this.gameTimeSeconds, playerNumber, playerPosition, ballPosition, (double)this.ball.speed), Logger.LogLevel.INFO);
            }
            else if (getNextBallPosition().distanceFrom(playerPosition) <= 1.5)
            {
                moveToPosition(ref actions, playerNumber, getNextBallPosition());
            }
            else
            {
                // We move towards the ball...
                // If ball is travelling, FIND THE final position getPositionPlusVector
                if (speed >= 1)
                {
                    ballPosition = getBallInterceptPosition(playerNumber, playerPosition, (double)player.dynamicState.direction, speed);
                    if (ballPosition == null)
                        ballPosition = new Position(this.ball.position);
                    //Logger.log(String.Format("{0}: XXX Player {1} current position {2} attempts to take possession of ball position {3} ball speed{4}", this.gameTimeSeconds, playerNumber, playerPosition, ballPosition, (double)this.ball.speed), Logger.LogLevel.INFO);
                }
                moveToPosition(ref actions, playerNumber, ballPosition);
                Logger.log(String.Format("{0}: Player {1} current position {2} moves to ball position {3}", this.gameTimeSeconds, playerNumber, playerPosition, ballPosition), Logger.LogLevel.INFO);
            }
        }

        private void setPlayerNextPostion(int playerNumber, Position position)
        {
            ProjectedPositions[playerNumber] = position;
        }

        private bool IsNextPositionSet(int playerNumber)
        {
            if (ProjectedPositions[playerNumber] != null)
                return true;
            else
                return false;
        }
        
        private void getAction_nextPosition(List<JSObject> actions, dynamic player)
        {
            int playerNumber = player.staticState.playerNumber;
            Position nextPostion = ProjectedPositions[playerNumber];
            moveToPosition(ref actions, playerNumber, nextPostion);
        }

        private void getStrikerPlayers(ref int playerOne, ref int playerTwo, int playerNumber)
        {
            if (playerNumber_LeftWingForward == playerNumber) { playerOne = playerNumber_RightWingForward; playerTwo = playerNumber_LeftWingDefender; }
            else if (playerNumber_RightWingForward == playerNumber) { playerOne = playerNumber_LeftWingForward; playerTwo = playerNumber_LeftWingDefender; }
            else if (playerNumber_LeftWingDefender == playerNumber) { playerOne = playerNumber_LeftWingForward; playerTwo = playerNumber_RightWingForward; }

            Position playerOnePosition = new Position(this.allTeamPlayers[playerOne].dynamicState.Position);
            Position playerTwoPosition = new Position(this.allTeamPlayers[playerTwo].dynamicState.Position);

            if (playerOnePosition.distanceFrom(getGoalCentre(GoalType.THEIR_GOAL)) > playerTwoPosition.distanceFrom(getGoalCentre(GoalType.THEIR_GOAL)))
            {
                //swap
                int temp = playerOne;
                playerOne = playerTwo;
                playerTwo = temp;
            }
        }

        private void getPlayersInOrderNearBall(ref int playerOne, ref int playerTwo, ref int playerThree)
        {
            var leftForward = this.allTeamPlayers[playerNumber_LeftWingForward];
            var rightForward = this.allTeamPlayers[playerNumber_RightWingForward];
            var leftDefender = this.allTeamPlayers[playerNumber_LeftWingDefender];

            Position leftForwardPos = new Position(leftForward.dynamicState.Position);
            Position rightForwardPos = new Position(rightForward.dynamicState.Position);
            Position leftDefenderPos = new Position(leftDefender.dynamicState.Position);
            Position ballPosition = new Position((double)this.ball.position.x, (double)this.ball.position.y);
            double speed = (double)this.ball.speed;

            if (speed > 5)
                ballPosition = getEndBallPosition();

            double leftForwardDist = leftForwardPos.distanceFrom(ballPosition);
            double rightForwardDist = rightForwardPos.distanceFrom(ballPosition);
            double leftDefenderDist = leftDefenderPos.distanceFrom(ballPosition);
            
            List<double> parts = new List<double>();
            parts.Add(leftForwardDist);
            parts.Add(rightForwardDist);
            parts.Add(leftDefenderDist);
            parts.Sort();
            bool leftF = true, rightF = true, leftD = true;
            int count = 0;
            foreach(var d in parts)
            {
                ++count;
                if ((d == leftForwardDist) && leftF)
                {
                    if (count == 1) playerOne = playerNumber_LeftWingForward;
                    else if (count == 2) playerTwo = playerNumber_LeftWingForward;
                    else if (count == 3) playerThree = playerNumber_LeftWingForward;
                    leftF = false;
                }
                else if ((d == rightForwardDist) && rightF)
                {
                    if (count == 1) playerOne = playerNumber_RightWingForward;
                    else if (count == 2) playerTwo = playerNumber_RightWingForward;
                    else if (count == 3) playerThree = playerNumber_RightWingForward;
                    rightF = false;
                }
                else if(leftD)
                {
                    if (count == 1) playerOne = playerNumber_LeftWingDefender;
                    else if (count == 2) playerTwo = playerNumber_LeftWingDefender;
                    else if (count == 3) playerThree = playerNumber_LeftWingDefender;
                    leftD = false;
                }
            }
            
        }

        private void moveToPosition(ref List<JSObject> actions, int playerNumber, Position position)
        {
            if (position == null)
            {
                Logger.log(String.Format("{0}: NULL", this.gameTimeSeconds), Logger.LogLevel.INFO);
                return;
            }
            
            var action = new JSObject();
            action.add("playerNumber", playerNumber);
            action.add("action", "MOVE");
            action.add("destination", position);
            action.add("speed", 100.0);
            actions.Add(action);

            Logger.log(String.Format("{0}: Player {1}  moves to  position {2}", this.gameTimeSeconds, playerNumber, position), Logger.LogLevel.INFO);
        }

        private Position getEndBallPositionBeforeGoal()
        {
            double speed = (double)this.ball.speed;
            //double scale = speed * speed / 20.0;

            Vector v = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
            Position ballPosition = new Position((double)this.ball.position.x, (double)this.ball.position.y);
            Position prevPos = ballPosition;

            while (speed > 0)
            {
                double scale = (speed /10) - 0.05;
                Vector scaledVector = v.getScaledVector(scale);
                prevPos = ballPosition;
                ballPosition = ballPosition.getPositionPlusVector(scaledVector);

                if (this.playingDirection == DirectionType.LEFT)
                {
                    if (ballPosition.x > this.pitch.width)
                        speed = 0;
                }
                else
                {
                    if (ballPosition.x < 0.0)
                        speed = 0;
                }
                speed--;
            }

            if (speed == 0)
                return ballPosition;
            else return prevPos;
        }

        private Position getEndBallPosition()
        {
            double speed = (double)this.ball.speed;
            double scale = speed * speed / 20.0;

            Vector v = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
            Vector scaledVector = v.getScaledVector(scale);
            Position ballPosition = new Position((double)this.ball.position.x, (double)this.ball.position.y);
            ballPosition = ballPosition.getPositionPlusVector(scaledVector);

            return ballPosition;
        }


        private void goToDefaultPosition(ref List<JSObject> actions, int playerNumber)
        {
            var defaultPosition = getDefaultPosition(playerNumber, this.playingDirection);
            //setPlayerNextPostion(playerNumber, defaultPosition.position);
            moveToPosition(ref actions, playerNumber,  defaultPosition.position);
        }

        private void getAction_staticPlayerAction(ref List<JSObject> actions)
        {
            var playerCenterDefender = this.allTeamPlayers[playerNumber_CentreDefender];
            var playerRightWingDefender = this.allTeamPlayers[playerNumber_RightWingDefender];

            Position playerCenterDefPos = new Position(playerCenterDefender.dynamicState.position);
            Position playerRightWingDefPos = new Position(playerRightWingDefender.dynamicState.position);
            Position ballPosition = new Position((double)this.ball.position.x, (double)this.ball.position.y);

            if (!ballIsInGoalArea())
                this.bBallOutSideGoalArea = true;

            if (playerCenterDefender.dynamicState.hasBall)
            {
                // The player has the ball, so we kick it to one of the forwards...
                getAction_Defender_HasBall1(actions, playerCenterDefender);
                goToDefaultPosition(ref actions, playerNumber_RightWingDefender);
                return;
            }
            else if (playerRightWingDefender.dynamicState.hasBall)
            {
                // The player has the ball, so we kick it to one of the forwards...
                getAction_Defender_HasBall1(actions, playerRightWingDefender);
                goToDefaultPosition(ref actions, playerNumber_CentreDefender);
                return;
            }

            if ((playerCenterDefPos.distanceFrom(ballPosition) < 2.0) ||
                (playerCenterDefPos.distanceFrom(getEndBallPosition()) < 2.0) ||
                IsPlayerNearToBall(playerNumber_CentreDefender, ballPosition))
            {
                getAction_OwnHalf(ref actions, playerCenterDefender);
                goToDefaultPosition(ref actions, playerNumber_RightWingDefender);
            }
            else if ((playerRightWingDefPos.distanceFrom(ballPosition) < 2.0) ||
                (playerRightWingDefPos.distanceFrom(getEndBallPosition()) < 2.0) ||
                IsPlayerNearToBall(playerNumber_RightWingDefender, ballPosition))
            {
                getAction_OwnHalf(ref actions, playerRightWingDefender);
                goToDefaultPosition(ref actions, playerNumber_CentreDefender);
            }
        }

        private void getPos1Pos2(ref Position Pos1, ref Position Pos2)
        {   
            if (this.playingDirection == DirectionType.RIGHT)
            {
                Pos1 = new Position(20, 10);
                Pos2 = new Position(20, 40);
            }
            else
            {
                Pos1 = new Position(80, 10);
                Pos2 = new Position(80, 40);
            }
        }

        private bool isPosInOurHalf(Position pos)
        {
            if (pos == null) return false;
            if (this.playingDirection == DirectionType.RIGHT)
            {
                if (pos.x < this.pitch.centreSpot.x)
                    return true;
                else
                    return false;
            }
            else
            {
                if (pos.x > this.pitch.centreSpot.x)
                    return true;
                else
                    return false;
            }
        }

        private void getAction_ForwardDefender(List<JSObject> actions)
        {
            int playerOne = 0, playerTwo = 0, playerThree = 0;
            getPlayersInOrderNearBall(ref playerOne, ref playerTwo, ref playerThree);

            //// PLAYER ONE LOGIC
            var playerOnevar = this.allTeamPlayers[playerOne];
            var playerTwovar = this.allTeamPlayers[playerTwo];
            var playerThreevar = this.allTeamPlayers[playerThree];
            Position ballPosition = new Position((double)this.ball.position.x, (double)this.ball.position.y);
            Position PlayerOnePosition = new Position(playerOnevar.dynamicState.position);
            Position PlayerTwoPosition = new Position(playerTwovar.dynamicState.position);
            Position PlayerThreePosition = new Position(playerThreevar.dynamicState.position);

            if (playerOnevar.dynamicState.hasBall)
            {
                getAction_Forward_HasBall(ref actions, playerOnevar);
                moveOtherPlayers(ref actions, playerOnevar);
                bBallOutSideGoalArea = true;
            }
            else
            {
                if (ballIsInGoalArea() && bBallOutSideGoalArea)
                {
                    bBallOutSideGoalArea = false;
                    getGoalKeeperToPlayer(new Position(this.goalkeeper.dynamicState.position));
                    moveToPosition(ref actions, playerOne, ProjectedPositions[playerOne]);
                    moveToPosition(ref actions, playerTwo, ProjectedPositions[playerTwo]);

                    if (ProjectedPositions[playerThree] != null)
                        moveToPosition(ref actions, playerThree, ProjectedPositions[playerThree]);
                }
                else if (ballIsInGoalArea())// && (this.lastControlPlayerNumber == goalkeeperPlayerNumber))
                {
                    moveToPosition(ref actions, playerOne, ProjectedPositions[playerOne]);
                    moveToPosition(ref actions, playerTwo, ProjectedPositions[playerTwo]);
                    if (ProjectedPositions[playerThree] != null)
                        moveToPosition(ref actions, playerThree, ProjectedPositions[playerThree]);
                }
                else if (this.ball.controllingPlayerNumber == getOppGoalKeeperNumber())
                {
                    setPlayerNextPostion(playerThree, getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position);
                    moveToPosition(ref actions, playerThree, ProjectedPositions[playerThree]);

                    //get opp players with power within their  half.
                    List<int> oppList = new List<int>();
                    getOppPlayerinTheirHalf(ref oppList);

                    oppList.Sort(delegate(int x, int y)
                    {
                        var xVar = allOpposingTeamPlayers[x];
                        var yVar = allOpposingTeamPlayers[y];
                        if ((double)xVar.staticState.kickingAbility > (double)yVar.staticState.kickingAbility)
                            return -1;
                        else
                            return 1;
                    });
                    int opp1 = -1, opp2 = -1, j = 0;
                    foreach (var i in oppList)
                    {
                        if (j == 0) opp1 = i;
                        else if (j == 1) opp2 = i;
                        else break;
                        j++;
                    }
                    
                    if (opp1 != -1)
                    {
                        Position opp1Pos = new Position(allOpposingTeamPlayers[opp1].dynamicState.position);
                        if (PlayerOnePosition.distanceFrom(opp1Pos) < PlayerTwoPosition.distanceFrom(opp1Pos))
                        {
                            Vector v = new Vector(opp1Pos, getOppGoalKeeperPosition());
                            v = v.getScaledVector(1.0);
                            setPlayerNextPostion(playerOne, opp1Pos.getPositionPlusVector(v));
                            moveToPosition(ref actions, playerOne, ProjectedPositions[playerOne]);
                            if (opp2 != -1)
                            {
                                Position opp2Pos = new Position(allOpposingTeamPlayers[opp2].dynamicState.position);
                                v = new Vector(opp2Pos, getOppGoalKeeperPosition());
                                v = v.getScaledVector(1.0);
                                setPlayerNextPostion(playerTwo, opp2Pos.getPositionPlusVector(v));
                                moveToPosition(ref actions, playerTwo, ProjectedPositions[playerTwo]);
                            }
                        }
                        else
                        {
                            Vector v = new Vector(opp1Pos, getOppGoalKeeperPosition());
                            v = v.getScaledVector(1.0);
                            setPlayerNextPostion(playerTwo, opp1Pos.getPositionPlusVector(v));
                            moveToPosition(ref actions, playerTwo, ProjectedPositions[playerTwo]);
                            if (opp2 != -1)
                            {
                                Position opp2Pos = new Position(allOpposingTeamPlayers[opp2].dynamicState.position);
                                v = new Vector(opp2Pos, getOppGoalKeeperPosition());
                                v = v.getScaledVector(1.0);
                                setPlayerNextPostion(playerOne, opp2Pos.getPositionPlusVector(v));
                                moveToPosition(ref actions, playerOne, ProjectedPositions[playerOne]);
                            }
                        }
                    }
                }
                else
                {
                    getAction_OwnHalf(ref actions, playerOnevar);
                    
                    if ((this.lastControlPlayerNumber == playerOne) ||
                        (this.lastControlPlayerNumber == playerTwo) ||
                        (this.lastControlPlayerNumber == playerThree) ||
                        (this.lastControlPlayerNumber == goalkeeperPlayerNumber))
                    {
                        // I expect that player 2 & 3 projected positions are calculated.
                        moveToPosition(ref actions, playerTwo, ProjectedPositions[playerTwo]);
                        moveToPosition(ref actions, playerThree, ProjectedPositions[playerThree]);
                    }
                    else
                    {
                        if (PlayerThreePosition.distanceFrom(getGoalCentre(GoalType.OUR_GOAL))< 
                            PlayerTwoPosition.distanceFrom(getGoalCentre(GoalType.OUR_GOAL)))
                        {
                            //swap
                            int tmp = playerTwo;
                            Position tmpP = PlayerTwoPosition;
                            var tmpVar = playerTwovar;

                            playerTwo = playerThree;
                            PlayerTwoPosition = PlayerThreePosition;
                            playerTwovar = playerThreevar;

                            playerThree = tmp;
                            PlayerThreePosition = tmpP;
                            playerThreevar = tmpVar;
                        }
                        int OppPlayer = 0;
                        Position oppPlayerPos = null;
                        getOppPlayerNearBall(ref OppPlayer, ref oppPlayerPos);
                        Position oppPlayerPosNearOurGoal = getOppPlayerPosNearOurGoal(OppPlayer, oppPlayerPos);
                        if ((oppPlayerPosNearOurGoal != null) && isPosInOurHalf(oppPlayerPosNearOurGoal))
                        {
                            Vector v = new Vector(oppPlayerPosNearOurGoal, ballPosition);
                            v = v.getScaledVector(1.5);
                            Position PlayerTwoTargetPosition = oppPlayerPosNearOurGoal.getPositionPlusVector(v);
                            setPlayerNextPostion(playerTwo, PlayerTwoTargetPosition);
                            moveToPosition(ref actions, playerTwo, PlayerTwoTargetPosition);
                        }
                        else
                        {
                            if (isPosInOurHalf(ballPosition))
                            {
                                double scale = 0.0;
                                Position goalPos = getGoalCentre(GoalType.OUR_GOAL);
                                Vector v = new Vector(goalPos, ballPosition);
                                if (ballPosition.distanceFrom(goalPos) < 30)
                                    scale = this.pitch.goalAreaRadius + 0.5;
                                else
                                    scale = this.pitch.goalAreaRadius + 5.0;
                                v = v.getScaledVector(scale);
                                Position PlayerTwoTargetPosition = ballPosition.getPositionPlusVector(v);
                                setPlayerNextPostion(playerTwo, PlayerTwoTargetPosition);
                            }
                            else
                            {
                                setPlayerNextPostion(playerTwo, getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position);          
                            }
                            moveToPosition(ref actions, playerTwo, ProjectedPositions[playerTwo]);
                        }
                         
                        // ThirdPlayer logic to move
                        if (isPosInOurHalf(getEndBallPosition()))
                        {
                            setPlayerNextPostion(playerThree, getDefaultPosition(playerNumber_LeftWingForward, this.playingDirection).position);          
                        }
                        else
                        {
                            getStrikePosition(PlayerThreePosition, PlayerOnePosition, true);
                        }
                        moveToPosition(ref actions, playerThree, ProjectedPositions[playerThree]);
                    }
                }
            }
        }


        private void getOppPlayerNearBall(ref int playerNumber, ref Position returnP)
        {
            Position ballPosition = new Position(this.ball.position);
            double minDist = 100;
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                Position oppPosition = new Position(oppPlayer.dynamicState.position);
                if (oppPosition.distanceFrom(ballPosition) < minDist)
                {
                    playerNumber = oppPlayer.staticState.playerNumber;
                    minDist = oppPosition.distanceFrom(ballPosition);
                    returnP = oppPosition;
                }
            }
        }
        
        private Position getOppPlayerPosNearOurGoal(int OppPlayerNum, Position OppPosition)
        {
            Position ourGoalPosition = getGoalCentre(GoalType.OUR_GOAL);
            Position returnPosition = null;
            double kickingAbility = 0.0;
            double oppPositionDist = OppPosition.distanceFrom(ourGoalPosition);
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                if (oppPlayer.staticState.playerNumber != OppPlayerNum)
                {
                    Position oppPosition1 = new Position(oppPlayer.dynamicState.position);
                    if (oppPosition1.distanceFrom(ourGoalPosition) < oppPositionDist &&
                        ((double)oppPlayer.staticState.kickingAbility > 50) && isPosInOurHalf(oppPosition1))
                    {
                        if (kickingAbility < (double)oppPlayer.staticState.kickingAbility)
                        {
                            kickingAbility = (double)oppPlayer.staticState.kickingAbility;
                            oppPositionDist = oppPosition1.distanceFrom(ourGoalPosition);
                            returnPosition = oppPosition1;
                        }
                    }
                }
            }
            return returnPosition;
        }

        private Position getOppPlayerPosNearTheirGoal(int OppPlayerNum, Position OppPosition)
        {
            Position ourGoalPosition = getGoalCentre(GoalType.OUR_GOAL);
            Position returnPosition = null;
            double kickingAbility = 0.0;
            double oppPositionDist = OppPosition.distanceFrom(ourGoalPosition);
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                if (oppPlayer.staticState.playerNumber != OppPlayerNum)
                {
                    Position oppPosition1 = new Position(oppPlayer.dynamicState.position);
                    if (oppPosition1.distanceFrom(ourGoalPosition) < oppPositionDist &&
                        ((double)oppPlayer.staticState.kickingAbility > 50) && !isPosInOurHalf(oppPosition1))
                    {
                        if (kickingAbility < (double)oppPlayer.staticState.kickingAbility)
                        {
                            kickingAbility = (double)oppPlayer.staticState.kickingAbility;
                            oppPositionDist = oppPosition1.distanceFrom(ourGoalPosition);
                            returnPosition = oppPosition1;
                        }
                    }
                }
            }
            return returnPosition;
        }

        /// <summary>
        /// Gets the current action for one of the forwards.
        /// </summary>
        private void getAction_Forward(List<JSObject> actions, int playerNumber)
        {
            var player = this.allTeamPlayers[playerNumber];
            var ballPosition = this.ball.position;
            bool actionFound = false; 
            if (player.dynamicState.hasBall)
            {
                // The player has the ball, so we kick it at the goal...
                getAction_Forward_HasBall(ref actions, player);
                actionFound = true;
            }
            else if ((this.playingDirection == DirectionType.RIGHT && ballPosition.x > this.pitch.centreSpot.x)
                     ||
                     (this.playingDirection == DirectionType.LEFT && ballPosition.x < this.pitch.centreSpot.x)
                     ||
                    (IsPlayerNearToBall(playerNumber, new Position(this.ball.position))))
            {
                // The ball is in the forward's half, so he tries to get it...
                if (IsPlayerNearToBall(playerNumber, new Position(this.ball.position)))
                {
                    getAction_OwnHalf(ref actions, player);
                    actionFound = true;
                }
            }

            if ((!actionFound) && (IsNextPositionSet(playerNumber)))
            {
                getAction_nextPosition(actions, player);
                actionFound = true;
            }

            if (!actionFound)
            {
                // The ball is in the other half, 

                //check the ball vector /direction and speed
                double speed = (double)this.ball.speed;

                //double velocity = 0.9;
                //double scalefactor = velocity * speed;
                double scalefactor = speed * speed/20.0;

                Vector v = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
                Vector scaledVector = v.getScaledVector(scalefactor);
                Position ballPositionP = new Position((double)ballPosition.x, (double)ballPosition.y);
                ballPositionP = ballPositionP.getPositionPlusVector(scaledVector);

                if (IsPlayerNearToBall(playerNumber, ballPositionP)) 
                {
                    // get to the projected ball position
                    // TODO check if I am nearest to projected ball position, also If ball is already is my team possession
                    moveToPosition(ref actions, playerNumber, ballPositionP);
                    Logger.log(String.Format("{0}: Player {1} current position moves to projected ball position {2}", this.gameTimeSeconds, playerNumber, ballPositionP), Logger.LogLevel.INFO);
                    actionFound = true;
                }  
            }
            
            if (!actionFound)
            {
                // check if move to 20+ position from ball    
                Position ballPosition1 = new Position((double)ballPosition.x, (double)ballPosition.y);
                Position playerPosition = new Position(player.dynamicState.position);
                goToDefaultPosition(ref actions, playerNumber);

            }
        }


        /// <summary>
        /// return the angle between two positions.
        /// </summary>
        private double getAbsoluteAngle(Position p1, Position p2)
        {
                        
            /*                     | 
                             1     |    2
                        -----------|------------
                             4     |    3
                                   | 
                        
              */                      
            //determine the quadarant
            int q = 0;
            double absAngle = 0.0;

            double Angle = getAngleBtwPositions(p1, p2);

            if (isEqual(p1.x, p2.x) || isEqual(p1.y, p2.y)) // if either of the co-ordinates are equal
            {
                if (isEqual(p1.x, p2.x)) // quad 1 or 3
                {
                    if (p1.y > p2.y)
                        q = 1;
                    else
                        q = 3;
                }
                else // quad 2 or 4
                {
                    if (p1.x > p2.x)
                        q = 4;
                    else
                        q = 2;
                }
            }

            if (q ==0)
            { 
                if (p1.x > p2.x)  // First or fourth quadrant
                {
                    if (p1.y > p2.y)
                        q = 1;
                    else
                        q = 4;
                }
                else  // Second or third 
                {
                    if (p1.y > p2.y)
                        q = 2;
                    else
                        q = 3;
                }
            }

            switch (q)
            {
                case 1: 
                        if (Angle != 0.0)  // if angle == 0.0, Angle will be 0.0  ie facing striaght up
                        {
                            absAngle = 270.0 + Angle;
                        }
                        break;
                case 2: if (Angle != 0.0) 
                        {
                            absAngle = 90.0 - Angle;
                        }
                        else
                        {
                            absAngle = 90.0; // if angle == 0.0, Angle will be 0.0  ie facing right 
                        }
                        break;
                case 3: if (Angle != 0.0)
                        {
                            absAngle = 90.0 + Angle;
                        }
                        else
                        {
                            absAngle = 180; // if angle == 0.0, Angle will be 0.0  ie facing down 
                        }
                        break;
                case 4: if (Angle != 0.0)
                        {
                            absAngle = 270.0 - Angle;
                        }
                        else
                        {
                            absAngle = 270.0; // if angle == 0.0, Angle will be 0.0  ie facing left 
                        }
                        break;
                default: break;
            }
            return absAngle;

        }

        private bool isEqual (double p1, double p2)
        {
            if (Math.Round(p1) == Math.Round(p2))
                return true;
            else return false;
        }

        /// <summary>
        /// return the angle between two positions.
        /// </summary>
        private double getAngleBtwPositions(Position p1, Position p2)
        {
            //if ((p1.x == p2.x) || (p1.y == p2.y)) //straight line - no angle
            if (isEqual(p1.x, p2.x) || isEqual(p1.y, p2.y)) //straight line - no angle
                return 0.0;

            //get the third positon
            Position p3 = new Position(p2.x, p1.y);
            
            // calculate lengths of each line
            double line1, line2, line3;

            if (p1.x < p3.x)
                line1 = p3.x - p1.x;
            else
                line1 = p1.x - p3.x;

            if (p2.y < p3.y)
                line2 = p3.y - p2.y;
            else
                line2 = p2.y - p3.y;

            line3 = Math.Sqrt((line1 * line1) + (line2 * line2));

            double value = (Math.Pow(line1, 2) + Math.Pow(line3, 2) - Math.Pow(line2, 2)) / (2 * line1 * line3);
            double angle = Math.Acos(value) *180/Math.PI;

            angle = Math.Round(angle, 1, MidpointRounding.AwayFromZero);

            return angle;
        }

        private bool isLineClear(Position P1, Position P2, Position O)
        {
            //find the slope
            double slope = (P2.y - P1.y) / (P2.x - P1.x);
            double y = (slope * (O.x - P1.x)) + P1.y;

            if (y < 0) y = Math.Abs(y);

            if (Math.Abs(y - O.y) <= 0.5)
                return false;

            return true;
        }

        private bool isLineClear1(Position P1, Position P2, ref List<Position> OList)
        {
            Vector v = new Vector(P1, P2);
            double distance = P1.distanceFrom(P2);
            //List<Position> lPositionList = new List<Position>();
            double counter = 1.0;
            distance = distance - counter;
            while (distance != 0.0)
            {
                if (distance < 0.3)
                {
                    counter += distance;
                    distance = 0.0;
                }
                else
                {
                    counter += 0.3;
                    distance -= 0.3;
                }
                Vector sV = v.getScaledVector(counter);
                //lPositionList.Add(P1.getPositionPlusVector(sV));
                Position sP = P1.getPositionPlusVector(sV);
                foreach (var O in OList)
                {
                    if (sP.distanceFrom(O) < 2)
                        return false;
                }
            }
            return true;
        }


        private bool IsGoalPossible(Position playerPosition, Position goalPos)
        {
            double direction = getAbsoluteAngle(playerPosition, goalPos);
            // is valid direction ?
            if ((direction >= 170.0 && direction <= 190.0) || //stright line, goal not possible
            (direction >= 350.0 && direction <= 359.99) ||
            (direction >= 0.0 && direction <= 10.0))
            {
                Logger.log(String.Format("Goal not possible due to angle {0}", direction), Logger.LogLevel.INFO);
                return false;
            }

            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                //if (oppPlayer.staticState.playerType == "G")
                {
                    var oppPosition = new Position(oppPlayer.dynamicState.position);
                    //ignore opponents which are on top of me :-)

                    if (Math.Abs(playerPosition.x - oppPosition.x) <= 0.5)
                        continue;
                    if (this.playingDirection == DirectionType.RIGHT)
                    {
                        //ignore all players towards my left 
                        if ((playerPosition.x - oppPosition.x) >= 0.0)
                            continue;
                    }
                    else
                    {
                        //ignore all players towards my right
                        if ((playerPosition.x - oppPosition.x) <= 0.0)
                            continue;
                    }

                    if(! isLineClear(playerPosition, goalPos, oppPosition))
                        return false;
                }
            }
            return true;
        }
        private void getOppPlayerinTheirHalf(ref List<int> listOpp)
        {
            Position ourGoalCenter = getGoalCentre(GoalType.OUR_GOAL);
            Position theirGoalCenter = getGoalCentre(GoalType.THEIR_GOAL);

            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                if (oppPlayer.staticState.playerType != "G")
                {
                    Position p =  new Position(oppPlayer.dynamicState.position);
                    if (p.distanceFrom(theirGoalCenter) < p.distanceFrom(ourGoalCenter))
                    {
                        listOpp.Add(oppPlayer.staticState.playerNumber);
                    }
                }
            }
        }

        private int getOppGoalKeeperNumber()
        {
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                if (oppPlayer.staticState.playerType == "G")
                {
                    return oppPlayer.staticState.playerNumber;
                }
            }
            return -1;
        }

        private Position getOppGoalKeeperPosition()
        {
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                if (oppPlayer.staticState.playerType == "G")
                {
                    return new Position(oppPlayer.dynamicState.position);
                }
            }
            return null;
        }
        
        private Position getBestGoalPosition(Position playerPosition, ref List<Position> goalPositionList, Position oppGoalKeeperPosition)
        {
            double maxDist = 0.0;
            Position position = null;
            foreach (var goalPosition in goalPositionList)
            {
                 //find the slope
                double slope = (goalPosition.y -playerPosition.y) /(goalPosition.x - playerPosition.x);

                double y = (slope * (oppGoalKeeperPosition.x-playerPosition.x)) + playerPosition.y;

                if (y < 0) y = Math.Abs(y);

                if (maxDist <  (Math.Abs(y - oppGoalKeeperPosition.y)))
                {
                    maxDist = (Math.Abs(y - oppGoalKeeperPosition.y));
                    position = new Position(goalPosition.x, goalPosition.y);
                }
                else if ((maxDist == (Math.Abs(y - oppGoalKeeperPosition.y))) && (position!= null))
                {
                    if (playerPosition.distanceFrom(goalPosition) < playerPosition.distanceFrom(position))
                    {
                        maxDist = (Math.Abs(y - oppGoalKeeperPosition.y));
                        position = new Position(goalPosition.x, goalPosition.y);
                    }

                }
            }
            return position;
        }

        private Position getGoalPosition(Position playerPosition)
        {
            //get all possible goal positions.
            var goalCenter = getGoalCentre(GoalType.THEIR_GOAL);
            var goalStartY = this.pitch.goalY1;
            var goalEndY = this.pitch.goalY2;
            var goalPositionList = new List<Position>();
            Position shootAt = null;

            --goalEndY;
            while (goalEndY > goalStartY)
            {
                var goalPos = new Position(goalCenter.x, goalEndY);
                if (IsGoalPossible(playerPosition, goalPos))
                    goalPositionList.Add(goalPos);
                --goalEndY;
            }

            if (goalPositionList.Count > 1)
            {
                Logger.log(String.Format("SHOOT TO GOAL"), Logger.LogLevel.INFO);
                // multipule position exits, find position away from keeper.
                var oppGoalKeeperPosition = getOppGoalKeeperPosition();
                if (oppGoalKeeperPosition != null)
                {
                    shootAt = getBestGoalPosition(playerPosition, ref goalPositionList, oppGoalKeeperPosition);

                    if (shootAt == null)
                    {
                        //some issue with logic, assign  the first position
                        shootAt = goalPositionList[0];
                        Logger.log(String.Format("Issue with get best goal position logic"), Logger.LogLevel.INFO);
                    }
                }
                else
                    shootAt = goalPositionList[0];
            }
            else if (goalPositionList.Count == 1)
                shootAt = goalPositionList[0];
            else shootAt = null;

            return shootAt;
        }

        private bool isOpponentWithinRadius(Position playerPosition, double radius)
        {
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {                
                var oppPosition = new Position(oppPlayer.dynamicState.position);

                if (((double)oppPlayer.staticState.runningAbility == 0) )
                {
                    if (radius > 0.4 )
                    {
                        if (playerPosition.distanceFrom(oppPosition) <= 0.4)
                            return true;
                    }
                    else
                    {
                         if (playerPosition.distanceFrom(oppPosition) <= radius)
                            return true;
                    }
                }
             
                double scale = (double)oppPlayer.staticState.runningAbility/100;
                if (playerPosition.distanceFrom(oppPosition)/scale <= radius)
                    return true;
            }
            return false;
        }

        private double getOpponentDistance(Position currPosition)
        {
            double minDistance = 100.0;
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {   
                var oppPosition = new Position(oppPlayer.dynamicState.position);
                if (currPosition.distanceFrom(oppPosition) < minDistance)
                    minDistance = currPosition.distanceFrom(oppPosition);
            }
            return minDistance;
        }
        private double roundDirection(double playerDirection)
        {
            double reminder = playerDirection % 10;
            playerDirection = playerDirection - reminder;
            if (reminder > 5.0) playerDirection = playerDirection + 10;
            if (playerDirection >= 360.0) playerDirection = playerDirection - 360.0;

            return playerDirection;
        }

        private Position getNewScaledPosition(Position playerPosition, double playerDirection, double distance)
        {
            Position newPosition = null;
            playerDirection = roundDirection(playerDirection);

            foreach (var posValue in coordinates10RadiusRIGHT)
            {
                if (posValue.direction == playerDirection)
                {
                    newPosition= new Position(playerPosition.x, playerPosition.y);
                    newPosition.x = newPosition.x + posValue.position.x;
                    newPosition.y = newPosition.y + posValue.position.y;
                    //return newPosition;
                    Vector v = new Vector(playerPosition, newPosition);
                    if (distance != 10.0)
                        v = v.getScaledVector(distance);
                    return playerPosition.getPositionPlusVector(v);
                }
            }

            foreach (var posValue in coordinates10RadiusLEFT)
            {
                if (posValue.direction == playerDirection)
                {
                    newPosition = new Position(playerPosition.x, playerPosition.y);
                    newPosition.x = newPosition.x + posValue.position.x;
                    newPosition.y = newPosition.y + posValue.position.y;
                    //return newPosition;

                   Vector v = new Vector(playerPosition, newPosition);
                    if (distance != 10.0)
                        v = v.getScaledVector(distance);
                    return playerPosition.getPositionPlusVector(v);
                }
            }
            return newPosition;
        }

        private bool isPlayerDirectionTowardsPlayignDirection(double playerDirection)
        {
            playerDirection = roundDirection(playerDirection);
             if (this.playingDirection == DirectionType.RIGHT)
             {
                if ((playerDirection >= DIRECTION_UP )  && (playerDirection<=DIRECTION_DOWN))
                    return true;
                else
                    return false;
             }
             else
             {
                 if (((playerDirection >= DIRECTION_DOWN )  && (playerDirection<=DIRECTION_LEFT_HIGH)) || (playerDirection == DIRECTION_UP))
                     return true;
                 else
                     return false;
             }
        }

        private HashSet<double> getGoalDirections(Position playerPosition)
        {
            HashSet<double> goalDirections = new HashSet<double>();
            //get all possible goal positions.
            var goalCenter = getGoalCentre(GoalType.THEIR_GOAL);
            var goalStartY = this.pitch.goalY1;
            var goalEndY = this.pitch.goalY2;

            --goalEndY;
            while (goalEndY > goalStartY)
            {
                var goalPos = new Position(goalCenter.x, goalEndY);
                double direction = roundDirection(getAbsoluteAngle(playerPosition, goalPos));
                if (!goalDirections.Contains(direction))
                    goalDirections.Add(direction);
                --goalEndY;
            }
            
            return goalDirections;
        }

        private bool areOpponentInLine(Position playerPosition, Position newPosition)
        {
            List<Position> oppPositionList = new List<Position>();
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                var oppPosition = new Position(oppPlayer.dynamicState.position);
                //ignore opponents which are on top of me :-)

                if ( (Math.Abs(playerPosition.x - oppPosition.x) <= 0.7) && 
                     (Math.Abs(playerPosition.y - oppPosition.y) <= 0.7))
                    continue;

                if (playerPosition.distanceFrom(oppPosition) <= 1.5)
                    continue;

                if ((newPosition.x - playerPosition.x) > 0)
                {
                    //ignore all players towards my left 
                    if ((playerPosition.x - oppPosition.x) >= 0.5)
                        continue;
                }
                else
                {
                    //ignore all players on other side of direction
                    if ((oppPosition.x - playerPosition.x) >= 0.5)
                        continue;
                }
                oppPositionList.Add(oppPosition);
            }

            if (!isLineClear1(playerPosition, newPosition, ref oppPositionList))
                return true;
            else
                return false;
        }

        private bool GoalToPlayerClear(Position playerPosition)
        {
            List<Position> oppPosList = new List<Position>();
            Position goalKeeperPosition = new Position(this.goalkeeper.dynamicState.position);
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                var oppPosition = new Position(oppPlayer.dynamicState.position);
                //ignore opponents which are on top of me :-)
                if ((playerPosition.x - goalKeeperPosition.x) > 0)
                {
                    //ignore all players towards my left 
                    if ((oppPosition.x - playerPosition.x) >= 0.5)
                        continue;
                }
                else
                {
                    //ignore all players on other side of direction
                    if ((playerPosition.x - oppPosition.x) >= 0.5)
                        continue;
                }
                oppPosList.Add(oppPosition);
            }
            if (!isLineClear1(goalKeeperPosition, playerPosition, ref oppPosList))
                return false;
            else
                return true;
        }

        private List<double> getOppositeVariationDirections(double playerDirectionRound, double direction_mid)
        {
            List<double> variationDirection = new List<double>();
           
            double playerDirectionRound_var = playerDirectionRound;
            //check if player is more near towards up or down direction
            if (playerDirectionRound > direction_mid)
            {
                double playerDirectionVariation = playerDirectionRound_var + 30.0;
                while (playerDirectionVariation != playerDirectionRound_var)
                {
                    if (playerDirectionVariation >= 360.0)
                        variationDirection.Add(playerDirectionVariation - 360.0);
                    else
                        variationDirection.Add(playerDirectionVariation);
                    playerDirectionVariation = playerDirectionVariation - 10.0;
                }
            }
            else
            {
                double playerDirectionVariation = playerDirectionRound_var - 30.0;
                while (playerDirectionVariation != playerDirectionRound_var)
                {
                    if (playerDirectionVariation < 0)
                        variationDirection.Add(playerDirectionVariation + 360.0 );
                    else
                        variationDirection.Add(playerDirectionVariation);
                    playerDirectionVariation = playerDirectionVariation + 10.0;
                }
            }
            return variationDirection;
       }
        
        private  List<double> getVariationDirections(double playerDirectionRound, double direction_mid)
        {
            List<double> variationDirection = new List<double>();

            if (Math.Abs(playerDirectionRound - direction_mid) <= 20.0)
                variationDirection.Add(playerDirectionRound);
            double playerDirectionRound_var = playerDirectionRound;

            if (playerDirectionRound <= direction_mid)
            {
                double playerDirectionVariation = playerDirectionRound_var + 30.0;
                while (playerDirectionVariation != playerDirectionRound_var)
                {
                    if (playerDirectionVariation >=360.0)
                        variationDirection.Add(playerDirectionVariation - 360.0);
                    else
                        variationDirection.Add(playerDirectionVariation);
                    playerDirectionVariation = playerDirectionVariation - 10.0;
                }
            }
            else
            {
                double playerDirectionVariation = playerDirectionRound_var - 30.0;
                while (playerDirectionVariation != playerDirectionRound_var)
                {
                    playerDirectionRound_var = playerDirectionRound_var - 10.0;
                    if (playerDirectionRound_var < 0)
                        variationDirection.Add(playerDirectionRound_var + 360.0);
                    else
                        variationDirection.Add(playerDirectionRound_var);
                }
            }
            if (Math.Abs(playerDirectionRound - direction_mid) > 20.0)
                variationDirection.Add(playerDirectionRound);


            playerDirectionRound_var = playerDirectionRound;
            if (playerDirectionRound <= direction_mid)
            {
                double playerDirectionVariation = playerDirectionRound_var - 30.0;
                while (playerDirectionVariation != playerDirectionRound_var)
                {
                    playerDirectionRound_var = playerDirectionRound_var - 10.0;

                    if (playerDirectionRound_var < 0)
                    {
                        variationDirection.Add(playerDirectionRound_var + 360.0);
                    }
                    else
                        variationDirection.Add(playerDirectionRound_var);
                }
            }
            else
            {
                double playerDirectionVariation = playerDirectionRound_var + 30.0;
                while (playerDirectionVariation != playerDirectionRound_var)
                {
                    playerDirectionRound_var = playerDirectionRound_var + 10.0;
                    if (playerDirectionRound_var >= 360.0 )
                        variationDirection.Add(playerDirectionRound_var - 360.0);
                    else
                        variationDirection.Add(playerDirectionRound_var);
                }
            }
            return variationDirection;
        }

        private List<double> getGoalFilteredDirection(Position playerPosition, double playerDirectionRound, double threshold)
        {
            HashSet<double> goalDirections = getGoalDirections(playerPosition);
            List<double> filteredDirections = new List<double>();

            foreach (var direction in goalDirections)
            {
                if (Math.Abs(playerDirectionRound - direction) <= threshold)
                    filteredDirections.Add(direction);
            }

            return filteredDirections;
        }

        private double getMinOppCalcDistance(Position p)
        {
            double minDistance = 500.0;
            double oppDistance = 0.0;
            double scale = 100;
            foreach (var oppPlayer in allOpposingTeamPlayers.Values)
            {
                if (oppPlayer.staticState.playerType == "G") continue;

                var oppPosition = new Position(oppPlayer.dynamicState.position);
                scale = (double)oppPlayer.staticState.runningAbility;
                if (scale == 0) scale = 10;
                scale = scale/100.0;
                oppDistance = p.distanceFrom(oppPosition) / scale;

                if (minDistance > oppDistance)
                    minDistance = oppDistance;
            }
            return minDistance;
        }

        private void getBestVariationPostion(Position playerPosition, List<double> variationDirection, double scale, ref Position bestNewPosition, ref double oppMinCalcDist)
        {
            oppMinCalcDist = 0.0;
            bestNewPosition = null;

            foreach (var direction in variationDirection)
            {
                Position newPosition = getNewScaledPosition(playerPosition, direction, scale);
                AdjustIfInGoalArea(ref newPosition);
                if (newPosition == null)
                {
                    Logger.log(String.Format("CRASH POSSIBLE ERROR getNewScaledPostion returned null, filtered direction {0}, player position {1} ", direction, playerPosition), Logger.LogLevel.INFO);
                }
                if (!areOpponentInLine(playerPosition, newPosition))
                {
                    double oppDist = getMinOppCalcDistance(newPosition);

                    if (oppDist > oppMinCalcDist)
                    {
                        oppMinCalcDist = oppDist;
                        bestNewPosition = newPosition;
                    }
                }                
            }
        }

        private void getKickTargetPosition2(Position playerPosition, ref List<double> goalVariationDirection, ref List<double> angleVariationDirection, double scale, ref Position returnPos, ref double returnOppDist)
        {
            List<Position> finaPostList = new List<Position>();
            foreach (var direction in goalVariationDirection)
            {
                Position newPosition = getNewScaledPosition(playerPosition, direction, scale);
                AdjustIfInGoalArea(ref newPosition);

                if (newPosition!= null)
                {
                    if (!areOpponentInLine(playerPosition, newPosition))
                        finaPostList.Add(newPosition);
                }
            }

            foreach (var direction in angleVariationDirection)
            {
                Position newPosition = getNewScaledPosition(playerPosition, direction, scale);
                AdjustIfInGoalArea(ref newPosition);

                if (newPosition != null)
                {
                    if (!areOpponentInLine(playerPosition, newPosition))
                        finaPostList.Add(newPosition);
                }
            }

            finaPostList.Sort(delegate(Position x, Position y)
            {
                if (getMinOppCalcDistance(x) > getMinOppCalcDistance(y))
                    return -1;
                else
                    return 1;
            });

            foreach (var pos in finaPostList)
            {
                returnPos = pos;
                returnOppDist = getMinOppCalcDistance(pos);
                break;
            }
        }

        private Position getKickTargetPosition(Position playerPosition, ref List<double> goalVariationDirection, ref List<double> angleVariationDirection, double scale, double opponentDist)
        {
            Position goalVariationPostion =null, angleVariationPostion = null;
            double goalVariationMinOppDist = 100.0, angleVariationMinOppDist = 100.0;

            getBestVariationPostion(playerPosition, goalVariationDirection, scale, ref goalVariationPostion, ref goalVariationMinOppDist);

            if ((goalVariationMinOppDist >= opponentDist) && (goalVariationPostion != null))
                return goalVariationPostion;

            getBestVariationPostion(playerPosition, angleVariationDirection, scale, ref angleVariationPostion, ref angleVariationMinOppDist);

            if ((angleVariationMinOppDist >= opponentDist) && (angleVariationDirection != null))
                return angleVariationPostion;

            if (goalVariationMinOppDist > angleVariationMinOppDist)
                return goalVariationPostion;
            else
            {
                if (angleVariationMinOppDist - goalVariationMinOppDist >= 1)
                    return angleVariationPostion;
                else
                    return goalVariationPostion;
            }
        }

        private Position getStrongDefenderPosition()
        {
            var Defender = this.allTeamPlayers[this.playerNumber_CentreDefender];
            var LeftWingDefender =  this.allTeamPlayers[this.playerNumber_LeftWingDefender];
            var RightWingDefender =  this.allTeamPlayers[this.playerNumber_RightWingDefender];

            if ((double)Defender.staticState.runningAbility < (double)LeftWingDefender.staticState.runningAbility)
                Defender = LeftWingDefender;

            if ( (double)Defender.staticState.runningAbility < (double) RightWingDefender.staticState.runningAbility)
                Defender = RightWingDefender;

            return new Position(Defender.dynamicState.Position);
        }

        private Position getTargetDoggedPosition(int playerNumber, Position playerPosition, double playerDirection, double radius, bool opponentClose, ref double targetSpeed, ref double angleVariation)
        {
            List<PositionValue> coordinates10Radius = null;
            Position kickTargetPosition = null;
            double direction_mid, opp_direction_mid, playerDirectionRound = roundDirection(playerDirection);
            
            if (this.playingDirection == DirectionType.RIGHT)
            {
               coordinates10Radius = coordinates10RadiusRIGHT;
               direction_mid = DIRECTION_RIGHT_MID;
               opp_direction_mid = DIRECTION_LEFT_MID;
            }
            else
            {
               coordinates10Radius = coordinates10RadiusLEFT;
               direction_mid = DIRECTION_LEFT_MID;
               opp_direction_mid = DIRECTION_RIGHT_MID;
            }

            List<double> filteredDirections = getGoalFilteredDirection(playerPosition, playerDirectionRound, 30.0);
            List<double> variationDirection = getVariationDirections(playerDirectionRound, direction_mid);
            Position forwardPosition = getOtherForwardPosition(playerNumber);

            if (opponentClose)  //do we have any opponent within 0.5m raidius
            {
                //danger of loosing the ball, immediate action needed. algorithm with minimum direction movement 
                Logger.log(String.Format("{0}: Opponent Close", this.gameTimeSeconds), Logger.LogLevel.INFO);

                //Is player facing towards attacking direction ?
                if (isPlayerDirectionTowardsPlayignDirection(playerDirection))
                {
                    Logger.log(String.Format("{0}: Playing in correct direction", this.gameTimeSeconds), Logger.LogLevel.INFO);

                    //situation is better as player is atleast facing at opponent goal area.
                    kickTargetPosition = getKickTargetPosition(playerPosition, ref filteredDirections, ref variationDirection, 10.0, 5.0);
                    
                    if (kickTargetPosition != null)
                    {
                        angleVariation = 60;
                        targetSpeed = 50.0;
                        return kickTargetPosition;
                    }
                    
                    // Evaluate option of kicking towards  another forward.
                    
                    if ((playerPosition.distanceFrom(forwardPosition) < 15.0) && ((playerPosition.distanceFrom(forwardPosition) > 2.0)) &&
                        !areOpponentInLine(playerPosition, forwardPosition) &&
                        !isOpponentWithinRadius(forwardPosition, 0.5) &&
                        isForwardClosertoGoalThanI(playerPosition, forwardPosition))
                    {
                        targetSpeed = getSpeed(playerPosition, forwardPosition);
                        angleVariation = 60;
                        return forwardPosition;
                    }
                    // catch all for now
                    {
                        kickTargetPosition = getKickTargetPosition(playerPosition, ref filteredDirections, ref variationDirection, 10.0, 1.0);  
                        if (kickTargetPosition != null)
                        {
                            angleVariation = 60;
                            targetSpeed = 50.0;
                            return kickTargetPosition;
                        }
                        else
                        {
                            Logger.log(String.Format("{0}: catch all returned null", this.gameTimeSeconds), Logger.LogLevel.INFO);

                        }
                    }
                }
                else //player not towards attack area
                { 
                    //TODO what to do now  .. Slowly Shift towards attack area. 
                    List<double> oppVariationDirection = getOppositeVariationDirections(playerDirectionRound, opp_direction_mid);

                    kickTargetPosition = getKickTargetPosition(playerPosition, ref oppVariationDirection, ref oppVariationDirection, 10.0, 5.0);
                    if (kickTargetPosition != null)
                    {
                        angleVariation = 60;
                        targetSpeed = 50.0;
                        return kickTargetPosition;
                    }
                    else
                    {
                       // EVALUATE TO FORWARD TO fast DEFEND;
                        Position defenderPosition = getStrongDefenderPosition();
                        if ((playerPosition.distanceFrom(defenderPosition) < 15) &&
                            !areOpponentInLine(playerPosition, defenderPosition))
                        {
                            angleVariation = 60;
                            targetSpeed = 80.0;
                            return defenderPosition;
                        }
                    }

                    // now what
                    kickTargetPosition = getKickTargetPosition(playerPosition, ref oppVariationDirection, ref oppVariationDirection, 10.0, 1.0);
                    if (kickTargetPosition != null)
                    {
                        angleVariation = 60;
                        targetSpeed = 50.0;
                        return kickTargetPosition;
                    }
                }

            }
            else
            {
                //best algorithm with more turns allowed. TODO
                // if forward in attacking position
                //shoot to forward. 
                int playerOne = 0, playerTwo = 0, playerThree = 0;
                getPlayersInOrderNearBall(ref playerOne, ref playerTwo, ref playerThree);
                var playerTwovar = this.allTeamPlayers[playerTwo];
                var playerThreevar = this.allTeamPlayers[playerThree];
              
                Position PlayerTwoPosition = new Position(playerTwovar.dynamicState.position);
                Position PlayerThreePosition = new Position(playerThreevar.dynamicState.position);
                if (isForwardClosertoGoalThanI(playerPosition, PlayerTwoPosition) &&
                    (PlayerTwoPosition.distanceFrom(playerPosition) < 40) &&
                    (PlayerTwoPosition.distanceFrom(playerPosition) > 2) &&
                     !isOpponentWithinRadius(PlayerTwoPosition, 0.5) && !areOpponentInLine(playerPosition, PlayerTwoPosition))
                {
                    //setOtherPlayerPosition(getOtherForwardNumber(playerNumber), forwardPosition);
                    //targetSpeed = 80;
                    targetSpeed = getSpeed(playerPosition, PlayerTwoPosition);
                    return PlayerTwoPosition;
                }

                if (isForwardClosertoGoalThanI(playerPosition, PlayerThreePosition) &&
                    (PlayerThreePosition.distanceFrom(playerPosition) < 40) &&
                    (PlayerThreePosition.distanceFrom(playerPosition) > 2) &&
                     !isOpponentWithinRadius(PlayerThreePosition, 0.5) && !areOpponentInLine(playerPosition, PlayerThreePosition))
                {
                    //setOtherPlayerPosition(getOtherForwardNumber(playerNumber), forwardPosition);
                    targetSpeed = getSpeed(playerPosition, PlayerThreePosition);
                    return PlayerThreePosition;
                }

                List<double> filteredDirectionsAll = getGoalFilteredDirection(playerPosition, playerDirectionRound, 360.0);
                variationDirection = getVariationDirections(filteredDirectionsAll[0], direction_mid);

                double minOppDist = 0.0;
                getKickTargetPosition2(playerPosition, ref filteredDirectionsAll, ref variationDirection, 10.0, ref kickTargetPosition, ref minOppDist);
                if (kickTargetPosition != null)
                {
                    targetSpeed = 50.0;
                    return kickTargetPosition;
                }
            }
            return null;
        }

        private void AdjustIfInOutsideArea(Position source, ref Position dest)
        {
            Position goalCentre = getGoalCentre(GoalType.THEIR_GOAL);
            double width = 0.0;

            if (this.playingDirection == DirectionType.LEFT)
                width = this.pitch.width;
            while ((dest.x > this.pitch.width) || (dest.x < 0))
            {
                double scale = 0.35;
                if (Math.Abs(dest.x - width) >= 10)
                    scale = 5;
                else if (Math.Abs(dest.x - width) >= 5)
                    scale = 2.5;
                else if (Math.Abs(dest.x - width) >= 2.5)
                    scale = 1.25;
                else scale = 0.35;
                Vector v = new Vector(dest, source);
                v = v.getScaledVector(scale);
                dest = dest.getPositionPlusVector(v);
            }
        }


        private void AdjustIfInGoalArea(ref Position tempP)
        {
            Position goalCentre = getGoalCentre(GoalType.THEIR_GOAL);
            while (tempP.distanceFrom(goalCentre) < this.pitch.goalAreaRadius + 1)
            {
                Vector v = new Vector(goalCentre, tempP);
                v = v.getScaledVector(1);
                tempP = tempP.getPositionPlusVector(v);
            }
        }

        private List<Position> populatePosForStrike()
        {
            Position theirGoalCenter = getGoalCentre(GoalType.THEIR_GOAL);
            Position strikePosition = null;
            List<Position> strikePosList = new List<Position>();
            if (this.playingDirection == DirectionType.RIGHT)
            {
                double angle = 210;
                while (angle <= 330)
                {
                    strikePosition = getNewScaledPosition(theirGoalCenter, angle, (double)this.pitch.goalAreaRadius + 2.0);
                    strikePosList.Add(strikePosition);
                    angle += 20;
                }
            }
            else
            {
                double angle = 30;
                while (angle <= 150)
                {
                    strikePosition = getNewScaledPosition(theirGoalCenter, angle, (double)this.pitch.goalAreaRadius + 2.0);
                    strikePosList.Add(strikePosition);
                    angle += 20;
                }
            }
            return strikePosList;
        }

        private Position getStrikePosition(Position playerPosition, Position ballPosition, bool oppCheck)
        {
        
            Position theirGoalCenter = getGoalCentre(GoalType.THEIR_GOAL);
            Position strikePosition = getNewScaledPosition(theirGoalCenter, 40.0, (double)this.pitch.goalAreaRadius + 2.0); 
            List<Position> strikePosList = populatePosForStrike();
            List<Position> strikePosList1 = new List<Position>();
            List<Position> strikePosList2 = new List<Position>();
            
            int i = 0;
            foreach (var pos in strikePosList)
            {
                if (pos.distanceFrom(playerPosition) <= 0.2)
                    return playerPosition;
                if (!isOpponentWithinRadius(pos, 1))
                {
                    if (ballPosition.y < this.pitch.goalCentre)
                    {
                        if (pos.y > this.pitch.goalCentre)
                            strikePosList1.Add(pos);
                        else
                            strikePosList2.Add(pos);
                    }
                    else
                    {
                        if (pos.y < this.pitch.goalCentre)
                            strikePosList1.Add(pos);
                        else
                            strikePosList2.Add(pos);
                    }
                }
                i++;
            }
           
            strikePosList1.Sort(delegate(Position x, Position y)
            {
                if (getMinOppCalcDistance(x) > getMinOppCalcDistance(y))
                    return -1;
                else
                    return 1;
            });

            strikePosList2.Sort(delegate(Position x, Position y)
            {
                if (getMinOppCalcDistance(x) > getMinOppCalcDistance(y))
                    return -1;
                else
                    return 1;
            });

            foreach (var pos in strikePosList1)
            {
                double dist = getMinOppCalcDistance(pos);
                if (getMinOppCalcDistance(pos) > 3)
                {
                    if (oppCheck)
                    {
                        if (!areOpponentInLine(pos, ballPosition))
                            return pos;
                    }
                    else
                        return pos;
                }
            }

            foreach (var pos in strikePosList2)
            {
                double dist = getMinOppCalcDistance(pos);
                if (getMinOppCalcDistance(pos) > 3) 
                {
                    if (oppCheck)
                    {
                        if (!areOpponentInLine(pos, ballPosition))
                            return pos;
                    }
                    else
                        return pos;
                }
            }

            strikePosList.Sort(delegate(Position x, Position y)
            {
                if (getMinOppCalcDistance(x) > getMinOppCalcDistance(y))
                    return -1;
                else
                    return 1;
            });

            foreach (var pos in strikePosList)
            {
                double dist = getMinOppCalcDistance(pos);
                if (!isOpponentWithinRadius(pos, 1))
                {
                    if (oppCheck)
                    {
                        if (!areOpponentInLine(pos, ballPosition))
                            return pos;
                    }
                    else
                        return pos;
                }
            }

            foreach (var pos in strikePosList)
                return pos;

            return strikePosition;
        }
            
           
        private Position getTargetDoggedPosition1(int playerNumber, Position playerPosition, double playerDirection, bool opponentClose, ref double targetSpeed, ref double angleVariation)
        {
            List<PositionValue> coordinates10Radius = null;
            //Position kickTargetPosition = null;
            double direction_mid, opp_direction_mid, playerDirectionRound = roundDirection(playerDirection);

            if (this.playingDirection == DirectionType.RIGHT)
            {
                coordinates10Radius = coordinates10RadiusRIGHT;
                direction_mid = DIRECTION_RIGHT_MID;
                opp_direction_mid = DIRECTION_LEFT_MID;
            }
            else
            {
                coordinates10Radius = coordinates10RadiusLEFT;
                direction_mid = DIRECTION_LEFT_MID;
                opp_direction_mid = DIRECTION_RIGHT_MID;
            }

            // is any striker closer to goal than I ??
            int playerOne = 0, playerTwo = 0;
            getStrikerPlayers(ref playerOne, ref playerTwo, playerNumber);
            var playerOnevar = this.allTeamPlayers[playerOne];
            Position PlayerOnePosition = new Position(playerOnevar.dynamicState.position);

            var playerTwovar = this.allTeamPlayers[playerTwo];
            Position PlayerTwoPosition = new Position(playerTwovar.dynamicState.position);
            Position theirGoalCenter = getGoalCentre(GoalType.THEIR_GOAL);
            Position ourGoalCenter = getGoalCentre(GoalType.OUR_GOAL);
            //bool bPlayer2AheadPlayer = false;
            //bool bPlayer1AheadPlayer = false;

            if (playerPosition.distanceFrom(ourGoalCenter) < playerPosition.distanceFrom(theirGoalCenter))
            {   //own half
                if ((PlayerOnePosition.distanceFrom(getGoalCentre(GoalType.THEIR_GOAL)) < this.pitch.goalAreaRadius + 3) &&
                    !isOpponentWithinRadius(PlayerOnePosition, 5.0) &&
                    (PlayerOnePosition.distanceFrom(playerPosition) < 50) &&
                    !areOpponentInLine(playerPosition, PlayerOnePosition))
                {
                    setPlayerNextPostion(playerOne, PlayerOnePosition);
                    setOtherPlayerPosition(playerNumber, playerTwo, playerPosition, PlayerTwoPosition, PlayerOnePosition);

                    if (isOpponentWithinRadius(playerPosition, 1.0))
                        angleVariation = 360;
                    else if (isOpponentWithinRadius(playerPosition, 3.0))
                        angleVariation = 180;
                    targetSpeed = getSpeed(playerPosition, PlayerOnePosition);
            
                    return PlayerOnePosition;
                }

                if ((playerPosition.distanceFrom(theirGoalCenter) - PlayerTwoPosition.distanceFrom(theirGoalCenter)) > 10)
                {
                    //bPlayer2AheadPlayer = true;
                    if ((PlayerTwoPosition.distanceFrom(playerPosition) < 40) &&
                        !isOpponentWithinRadius(PlayerTwoPosition, 0.5) &&
                        !areOpponentInLine(playerPosition, PlayerTwoPosition))
                    {
                        setPlayerNextPostion(playerTwo, PlayerTwoPosition);
                        Position OnePlayerStrikePos = getStrikePosition(PlayerOnePosition, PlayerTwoPosition, true);
                        setPlayerNextPostion(playerOne, OnePlayerStrikePos);

                        setPlayerNextPostion(playerNumber, getDefaultPosition(this.playerNumber_LeftWingDefender, this.playingDirection).position);
                        if (isOpponentWithinRadius(playerPosition, 1.0))
                            angleVariation = 360;
                        else if (isOpponentWithinRadius(playerPosition, 3.0))
                            angleVariation = 180;
                        targetSpeed = getSpeed(playerPosition, PlayerTwoPosition);
                        return PlayerTwoPosition;
                    }
                }

                if ((playerPosition.distanceFrom(theirGoalCenter) - PlayerOnePosition.distanceFrom(theirGoalCenter)) > 10)
                {
                    //bPlayer1AheadPlayer = true;
                    if ((PlayerOnePosition.distanceFrom(playerPosition) < 40) &&
                        !isOpponentWithinRadius(PlayerOnePosition, 0.5) &&
                        !areOpponentInLine(playerPosition, PlayerOnePosition))
                    {
                        setPlayerNextPostion(playerOne, PlayerOnePosition);
                        setOtherPlayerPosition(playerNumber, playerTwo, playerPosition, PlayerTwoPosition, PlayerOnePosition);

                        if (isOpponentWithinRadius(playerPosition, 1.0))
                            angleVariation = 360;
                        else if (isOpponentWithinRadius(playerPosition, 3.0))
                            angleVariation = 180;
                        targetSpeed = getSpeed(playerPosition, PlayerOnePosition);

                        return PlayerOnePosition;
                    }
                }

                if (((playerPosition.distanceFrom(theirGoalCenter) - PlayerTwoPosition.distanceFrom(theirGoalCenter)) >= 0.0) &&
                    !isOpponentWithinRadius(PlayerTwoPosition, 1.5) &&
                    !areOpponentInLine(playerPosition, PlayerTwoPosition))
                {
                    setPlayerNextPostion(playerTwo, PlayerTwoPosition);
                    
                    Position OnePlayerStrikePos = getStrikePosition(PlayerOnePosition, PlayerTwoPosition, true);
                    setPlayerNextPostion(playerOne, OnePlayerStrikePos);
                    setPlayerNextPostion(playerNumber, getDefaultPosition(this.playerNumber_LeftWingDefender, this.playingDirection).position);

                    if (isOpponentWithinRadius(playerPosition, 1.0))
                        angleVariation = 360;
                    else if (isOpponentWithinRadius(playerPosition, 3.0))
                        angleVariation = 180;
                     targetSpeed = getSpeed(playerPosition, PlayerTwoPosition);

                    return PlayerTwoPosition;
                }

                if (((playerPosition.distanceFrom(theirGoalCenter) - PlayerOnePosition.distanceFrom(theirGoalCenter)) >= 0.0) &&
                    !isOpponentWithinRadius(PlayerOnePosition, 1.5) &&
                    !areOpponentInLine(playerPosition, PlayerOnePosition))
                {
                    //bPlayer1AheadPlayer = true;
                    
                    setPlayerNextPostion(playerOne, PlayerOnePosition);
                    setOtherPlayerPosition(playerNumber, playerTwo, playerPosition, PlayerTwoPosition, PlayerOnePosition);

                    if (isOpponentWithinRadius(playerPosition, 1.0))
                        angleVariation = 360;
                    else if (isOpponentWithinRadius(playerPosition, 3.0))
                        angleVariation = 180;
                    targetSpeed = getSpeed(playerPosition, PlayerOnePosition);
                    return PlayerOnePosition;
                }


                // get variation toggle
            }
            else
            { //other half
                if ((
                    (isOpponentWithinRadius(playerPosition, 3) && !isOpponentWithinRadius(PlayerOnePosition, 5)) ||
                    (distFromTheirGoalCenter(playerPosition) < 25)
                    ) &&
                    isPlayerInOtherHalf(PlayerOnePosition) &&
                    (!areOpponentInLine(playerPosition, PlayerOnePosition) || (distFromBoundary(playerPosition) < 2 )))
                {
                    if (distFromTheirGoalCenter(playerPosition) < 25)
                        Logger.log(String.Format(" CHeck"), Logger.LogLevel.INFO);
                    setPlayerNextPostion(playerOne, PlayerOnePosition);
                    setOtherPlayerPosition(playerNumber, playerTwo, playerPosition, PlayerTwoPosition, PlayerOnePosition);
                    angleVariation = 360;
                    targetSpeed = getSpeed(playerPosition, PlayerOnePosition);
                    //targetSpeed = 80;
                    return PlayerOnePosition;
                }
                else
                    if ((
                        (isOpponentWithinRadius(playerPosition, 3) && !isOpponentWithinRadius(PlayerTwoPosition, 5)) ||
                        (distFromTheirGoalCenter(playerPosition) < 25)
                        ) &&
                        isPlayerInOtherHalf(PlayerTwoPosition) &&
                        (!areOpponentInLine(playerPosition, PlayerTwoPosition) || (distFromBoundary(playerPosition) < 2 ))) 
                    {
                        setPlayerNextPostion(playerTwo, PlayerTwoPosition);
                        
                        setOtherPlayerPosition(playerNumber, playerOne, playerPosition, PlayerOnePosition, PlayerTwoPosition);
                        angleVariation = 360;
                        targetSpeed = getSpeed(playerPosition, PlayerTwoPosition);
                        return PlayerTwoPosition;
                    }
            }

            if (isOpponentWithinRadius(playerPosition, 3.0) || playerPosition.distanceFrom(getGoalCentre(GoalType.THEIR_GOAL)) < 30)
            {
                // Look to pass 
                if (playerOne != 0)
                {
                    // if I am in my half
                    // forward avaiable
                    // 
                    if (isForwardClosertoGoalThanI(playerPosition, PlayerOnePosition) &&
                        (PlayerOnePosition.distanceFrom(playerPosition) < 40) &&
                        (PlayerOnePosition.distanceFrom(playerPosition) > 5) &&
                        !isOpponentWithinRadius(PlayerOnePosition, 0.5) &&
                        !areOpponentInLine(playerPosition, PlayerOnePosition))
                    {
                        if (isOpponentWithinRadius(playerPosition, 1.0))
                            angleVariation = 360;
                        else if (isOpponentWithinRadius(playerPosition, 3.0))
                            angleVariation = 180;
                        targetSpeed = getSpeed(playerPosition, PlayerOnePosition);
                        setOtherPlayerPosition(playerNumber, playerTwo, playerPosition, PlayerTwoPosition, PlayerOnePosition);
                        return PlayerOnePosition;
                    }
                }
                // Look to pass 
                if (playerTwo != 0)
                {
                    if (ProjectedPositions[playerTwo].distanceFrom(PlayerTwoPosition) > 5)
                    {
                        Vector v = new Vector(PlayerTwoPosition, ProjectedPositions[playerTwo]);
                        v = v.getScaledVector(ProjectedPositions[playerTwo].distanceFrom(PlayerTwoPosition) / 2);
                        PlayerTwoPosition = PlayerTwoPosition.getPositionPlusVector(v);
                    }
                    if (isForwardClosertoGoalThanI(playerPosition, PlayerTwoPosition) &&
                        (PlayerTwoPosition.distanceFrom(playerPosition) < 40) &&
                        (PlayerTwoPosition.distanceFrom(playerPosition) > 5) &&
                            !isOpponentWithinRadius(PlayerTwoPosition, 0.5) &&
                            !areOpponentInLine(playerPosition, PlayerTwoPosition))
                    {
                        if (isOpponentWithinRadius(playerPosition, 3.0))
                            angleVariation = 100;
                        targetSpeed = getSpeed(playerPosition, PlayerTwoPosition);
                        setOtherPlayerPosition(playerNumber, playerOne, playerPosition, PlayerOnePosition, PlayerTwoPosition);
                        return PlayerTwoPosition;
                    }
                }
            }
            return null; 
        }  

        private double distFromBoundary(Position playerPosition)
        {
            Position pos1 = new Position(0.0, playerPosition.y);
            Position pos2 = new Position(this.pitch.width, playerPosition.y);
            if (playerPosition.distanceFrom(pos1) <= playerPosition.distanceFrom(pos2))
            {
                if (playerPosition.distanceFrom(pos1) <= 2)
                    Logger.log(String.Format("Player boundary"), Logger.LogLevel.INFO);
                return playerPosition.distanceFrom(pos1);
            }
            else
            {
                if (playerPosition.distanceFrom(pos2) <= 2)
                    Logger.log(String.Format("Player boundary"), Logger.LogLevel.INFO);
                return playerPosition.distanceFrom(pos2);
            }
        }
        private void setOtherPlayerPosition(int playerOne, int playerTwo, Position playerOnePos, Position playerTwoPos, Position targetPos)
        {
            Position theirGoalCenter = getGoalCentre(GoalType.THEIR_GOAL);
            if (playerOnePos.distanceFrom(theirGoalCenter) < playerTwoPos.distanceFrom(theirGoalCenter))
            {
                Position PlayerStrikePos = getStrikePosition(playerOnePos, targetPos, true);
                setPlayerNextPostion(playerOne, PlayerStrikePos);
                setPlayerNextPostion(playerTwo, getDefaultPosition(this.playerNumber_LeftWingDefender, this.playingDirection).position);
            }
            else
            {
                Position PlayerStrikePos = getStrikePosition(playerTwoPos, targetPos, true);
                setPlayerNextPostion(playerTwo, PlayerStrikePos);
                setPlayerNextPostion(playerOne, getDefaultPosition(this.playerNumber_LeftWingDefender, this.playingDirection).position);
            }

        }
        private double getSpeed(Position posFrom, Position posTo)
        {
            if (posFrom.distanceFrom(posTo) > 25)
                return 100;
            else
                return 80;
        }

        private bool isPlayerInOtherHalf(Position pos)
        {
            if (pos.distanceFrom(getGoalCentre(GoalType.OUR_GOAL)) > pos.distanceFrom(getGoalCentre(GoalType.THEIR_GOAL)))
                return true;
            else
                return false; 
        }

        private bool isForwardClosertoGoalThanI(Position playerPosition, Position otherforward)
        {
            double otherFDist = otherforward.distanceFrom(getGoalCentre(GoalType.THEIR_GOAL)); 
            double playerDist = playerPosition.distanceFrom(getGoalCentre(GoalType.THEIR_GOAL));
            if ((playerDist - otherFDist) >= 3.0 )
                return true;
            else
                return false;
        }

        private int getOtherForwardNumber(int playerNumber)
        {
            int otherForwardNumber;
            if (playerNumber == this.playerNumber_LeftWingForward)
                //pass to rightwing forward
                otherForwardNumber = this.playerNumber_RightWingForward;
            else
                //pass to left wing forward.
                otherForwardNumber = this.playerNumber_LeftWingForward;

            return otherForwardNumber;
        }

        private Position getOtherForwardPosition(int playerNumber)
        {
            //forward the ball to forward.
            int passForwardNumber = getOtherForwardNumber(playerNumber);

            var forward = this.allTeamPlayers[passForwardNumber];
            return new Position(forward.dynamicState.position);

        }

        private double getAngleDiff (double angle1, double angle2)
        {
            double diff = Math.Abs(angle1 - angle2);
            if ((angle1 <= 180.0 && angle2 <= 180.0) || (angle1 >= 180.0 && angle2 >= 180.0))
                return diff;
            else
            {
                if (angle1 >= 180.0)
                    angle1 = 360.0 - angle1;
                else
                    angle2 = 360.0 - angle2;

                double diff2 = angle1 + angle2;

                if (diff2 < diff)
                    return diff2;
                else return diff;
            }
        }

        Position getBallInterceptPosition(int playerNumber, Position playerPosition, double playerDirection, double speed)
        {
            int i = 0;
            Position ballPosition = new Position(this.ball.position);
            Position ScaledBallPosition = new Position(this.ball.position);
            Position origBallPosition = new Position(this.ball.position);
            Vector v = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
            Vector scaledVector = null;
            
            double origSpeed = speed;
            double scaleFactor = (speed / 10.0) - 0.05;
 
            while (speed > 1)
            {
                --speed;
                ++i;

                if (speed > 1)
                {
                    // one more scale required
                    scaleFactor = scaleFactor + ((speed / 10.0) - 0.05);

                }
                scaledVector = v.getScaledVector(scaleFactor);
                ScaledBallPosition = ballPosition.getPositionPlusVector(scaledVector);

                double absAngle = getAbsoluteAngle(playerPosition, ScaledBallPosition);
                double angleDiff = getAngleDiff(absAngle, playerDirection);
                double noOfTurns = getNoOfTurns(angleDiff, 59.0);

                if (i >= noOfTurns)
                {
                    //getplayerPositionScaledPosition
                    double scaling = i - noOfTurns;

                    //if (scaling > 1) scaling = scaling - 1;
                    Vector vec = new Vector(playerPosition, ScaledBallPosition);

                    // We find the position 5m from the goal-centre...
                    var vScaled = vec.getScaledVector(scaling);
                    Position moveTo = playerPosition.getPositionPlusVector(vScaled);

                    if (moveTo.distanceFrom(ScaledBallPosition) <= 0.5)
                    {

                        Logger.log(String.Format("{0} OKAY Player Number{8} Player Position {1}, Direction {2} required direction {3} no of turn {4} ball position{5} speed{6} intercept position {7}",
                            this.gameTimeSeconds, playerPosition, playerDirection, absAngle, noOfTurns,
                            origBallPosition, speed, ScaledBallPosition, playerNumber), Logger.LogLevel.INFO);
                        return ScaledBallPosition;
                    }
                }
            }

            if (origSpeed != 0.0)
            {
                double scalefactor = speed * speed / 20.0;
                scaledVector = v.getScaledVector(scalefactor);
                origBallPosition = origBallPosition.getPositionPlusVector(scaledVector);
                Logger.log(String.Format("FAILED INTERCEPT LOGIC, therefore final position {0} return for player {1}", origBallPosition, playerNumber), Logger.LogLevel.INFO);
            }
            Logger.log(String.Format("FAILED INTERCEPT LOGIC"), Logger.LogLevel.INFO);
            return origBallPosition;
        }


        private double getNoOfTurns(double angleDiff, double salt)
        {

            double noOfTurns = 0.0;
            if (angleDiff <= salt)
                noOfTurns = 0;
            else
            {
                if (angleDiff < 60.0) return 0;
                noOfTurns = Math.Truncate(angleDiff/60.0);
                if ((noOfTurns % 10.0) > salt)
                    noOfTurns = Math.Truncate(noOfTurns) + 1;
                else
                    noOfTurns = Math.Truncate(noOfTurns);
            }

            return noOfTurns;

        }

        /// <summary>
        /// The forward shoots at the goal.
        /// </summary>
        private double distFromTheirGoalCenter(Position p)
        {
            return p.distanceFrom(getGoalCentre(GoalType.THEIR_GOAL));
        }
        private bool doPlay(ref List<JSObject> actions, dynamic player, Position shootAt, double speed, double angleVariation)
        {
            int playerNumber = player.staticState.playerNumber;
            Position playerPosition = new Position(player.dynamicState.position);

            double absAngle = getAbsoluteAngle(playerPosition, shootAt);
            double angleDiff = Math.Abs(absAngle - (double)player.dynamicState.direction);
            globalShootAt = shootAt;

            if (isOpponentWithinRadius(playerPosition, 1.0))
                angleVariation = 360;
            //else if (isOpponentWithinRadius(playerPosition, 3.0))
              //  angleVariation = 180;

            if (angleDiff <= angleVariation)  // CHECK IF player is in appropriate direction to kick the ball
            {
                if (shootAt != null)
                {
                    var action = new JSObject();
                    action.add("action", "KICK");
                    action.add("playerNumber", playerNumber);
                    action.add("destination", shootAt);
                    action.add("speed", speed);
                    actions.Add(action);
                    Logger.log(String.Format("{0}: Player {1} shoots to {2} with speed {3}", this.gameTimeSeconds, playerNumber, shootAt, speed), Logger.LogLevel.INFO);
                }
            }
            else //ask the player to turn
            {
                var action = new JSObject();
                action.add("action", "TURN");
                action.add("playerNumber", playerNumber);
                action.add("direction", absAngle);
                actions.Add(action);
                Logger.log(String.Format("{0}: Player {1} current direction {2} turns to direction {3}", this.gameTimeSeconds, playerNumber, player.dynamicState.direction, absAngle), Logger.LogLevel.INFO);
            }

            return true;
        }

        private void moveOtherPlayers(ref List<JSObject> actions, dynamic player)
        {
            int playerNumber = player.staticState.playerNumber;
            if (playerNumber != playerNumber_LeftWingDefender)
                moveToPosition(ref actions, playerNumber_LeftWingDefender, ProjectedPositions[playerNumber_LeftWingDefender]);

            if (playerNumber != playerNumber_LeftWingForward)
                moveToPosition(ref actions, playerNumber_LeftWingForward, ProjectedPositions[playerNumber_LeftWingForward]);

            if (playerNumber != playerNumber_RightWingForward)
                moveToPosition(ref actions, playerNumber_RightWingForward, ProjectedPositions[playerNumber_RightWingForward]);
        }

        private void getAction_Forward_HasBall(ref List<JSObject> actions, dynamic player)
        {
            int playerNumber = player.staticState.playerNumber;
            var playerPosition = new Position(player.dynamicState.position);
            double targetSpeed = 100;
            Position shootAt =null;
            bool actionDone=false;

            Logger.log(String.Format("{0} Player {1}  has the ball player position {2}.", this.gameTimeSeconds, playerNumber, playerPosition), Logger.LogLevel.INFO);

            //1. Check if within distance to goal
            
            Position centerPosition = new Position(this.pitch.centreSpot);
            if (playerPosition.distanceFrom(centerPosition) < 0.2)
            {
                var leftWingDefvar = this.allTeamPlayers[playerNumber_LeftWingDefender];
                Position PlayerLeftDefPos = new Position(leftWingDefvar.dynamicState.position);
                if (!isOpponentWithinRadius(PlayerLeftDefPos, 5.0))
                {
                    actionDone = doPlay(ref actions, player, PlayerLeftDefPos, 80.0, 10.0);
                    Logger.log(String.Format("{0} Player position{1} passes back to player at position {2} - KICKOFF", this.gameTimeSeconds, playerNumber, PlayerLeftDefPos), Logger.LogLevel.INFO);

                    //set the next position of other players and move them
                    Position theirGoalCenter = getGoalCentre(GoalType.THEIR_GOAL);
                    double angle1 = 40.0, angle2 = 140.0;

                    if (this.playingDirection == DirectionType.RIGHT)
                    {
                        angle1 = 320;
                        angle2 = 220;
                    }
                    Position strikeOnePos = getNewScaledPosition(theirGoalCenter, angle1, (double)this.pitch.goalAreaRadius + 2.0);
                    Position strikeTwoPos = getNewScaledPosition(theirGoalCenter, angle2, (double)this.pitch.goalAreaRadius + 2.0);

                    int otherPlayer = playerNumber_LeftWingForward;
                    if (playerNumber == playerNumber_LeftWingForward)
                        otherPlayer = playerNumber_RightWingForward;
                    var otherPlayervar = this.allTeamPlayers[otherPlayer];
                    Position PlayerOtherfPos = new Position(otherPlayervar.dynamicState.position);

                    if (PlayerOtherfPos.distanceFrom(strikeOnePos) < PlayerOtherfPos.distanceFrom(strikeTwoPos))
                    {
                        setPlayerNextPostion(otherPlayer, strikeOnePos);
                        moveToPosition(ref actions, otherPlayer, strikeOnePos);
                        setPlayerNextPostion(playerNumber, strikeTwoPos);
                    }
                    else
                    {
                        setPlayerNextPostion(otherPlayer, strikeTwoPos);
                        moveToPosition(ref actions, otherPlayer, strikeTwoPos);
                        setPlayerNextPostion(playerNumber, strikeOnePos);
                    }
                    return;
                }
            }

            double distanceFromGoal = distFromTheirGoalCenter(playerPosition);
            if (distanceFromGoal < 25.0)
            {
                if (!isOpponentWithinRadius(playerPosition, 4.0) && Math.Round(distanceFromGoal, 0) > ((double)this.pitch.goalAreaRadius + 1.0))
                {
                    double diffDist = Math.Abs((double)this.pitch.goalAreaRadius - distanceFromGoal) - 1;
                    // move close to goal area.
                    Vector v = new Vector(playerPosition, getGoalCentre(GoalType.THEIR_GOAL));
                    v = v.getScaledVector(diffDist);
                    playerPosition = playerPosition.getPositionPlusVector(v);
                    moveToPosition(ref actions, playerNumber, playerPosition);
                    actionDone = true;
                }
                else
                {
                    Logger.log(String.Format("{0} Distance from goal is {1}, so getting goal kick position", this.gameTimeSeconds, distanceFromGoal), Logger.LogLevel.INFO);
                    if (distanceFromGoal > (this.pitch.goalAreaRadius + 3))
                    {
                        int playerOne = 0, playerTwo = 0;
                        getStrikerPlayers(ref playerOne, ref playerTwo, playerNumber);
                        Position PlayerOnePosition = new Position(this.allTeamPlayers[playerOne].dynamicState.position);
                        double playerOneDistTheirGoal = distFromTheirGoalCenter(PlayerOnePosition);
                        if (((distanceFromGoal - playerOneDistTheirGoal) > 3) && !areOpponentInLine(playerPosition, PlayerOnePosition) &&
                            !isOpponentWithinRadius(PlayerOnePosition, 1.0))
                        {
                            if (getGoalPosition(PlayerOnePosition) != null)
                            {
                                actionDone = doPlay(ref actions, player, PlayerOnePosition, getSpeed(playerPosition, PlayerOnePosition), 10.0);
                                Logger.log(String.Format("{0} Player position{1} kicks to goal at position {2} with speed {3} ", this.gameTimeSeconds, playerNumber, playerPosition, shootAt, 100.0), Logger.LogLevel.INFO);
                                return;
                            }
                        }
                    }
                    shootAt = getGoalPosition(playerPosition);
                    if (shootAt != null)
                    {
                        actionDone = doPlay(ref actions, player, shootAt, 100.0, 10.0);
                        Logger.log(String.Format("{0} Player position{1} kicks to goal at position {2} with speed {3} ", this.gameTimeSeconds, playerNumber, playerPosition, shootAt, 100.0), Logger.LogLevel.INFO);
                    }
                    else
                    {
                        Logger.log(String.Format("{0} Player position{1} didnt find goal kick, so evaluting other options ", this.gameTimeSeconds, playerNumber), Logger.LogLevel.INFO);
                    }
                }
            }

            if (!actionDone)
            {
                double radius = 10.0;
                double angleVariation = 10.0;
                bool opponentClose = isOpponentWithinRadius(new Position(this.ball.position), 1.0);

                shootAt = getTargetDoggedPosition1(playerNumber, playerPosition, (double)player.dynamicState.direction, opponentClose, ref targetSpeed, ref angleVariation);
                
                if (shootAt == null)    
                    shootAt = getTargetDoggedPosition(playerNumber, playerPosition, (double)player.dynamicState.direction, radius/*not used*/, opponentClose, ref targetSpeed, ref angleVariation);
                
                if (shootAt != null)
                {
                    actionDone = doPlay(ref actions, player, shootAt, targetSpeed, angleVariation);
                    Logger.log(String.Format("{0} Position returned by getTargetDoggedPosition is {1} with speed {2}", this.gameTimeSeconds, shootAt, targetSpeed), Logger.LogLevel.INFO);
                }
                else
                    Logger.log(String.Format("{0} No Position returned by getTargetDoggedPosition", this.gameTimeSeconds), Logger.LogLevel.INFO);
            }

            if (!actionDone)
            {
                Logger.log(String.Format("{0} using goal center to kick as all algo failed.", this.gameTimeSeconds), Logger.LogLevel.INFO);
                shootAt = getGoalCentre(GoalType.THEIR_GOAL);
                actionDone = doPlay(ref actions, player, shootAt, 100.0, 10.0);
            }
            
        }

        /// <summary>
        /// Gets the current action for the goalkeeper.
        /// </summary>
        private void getAction_Goalkeeper(List<JSObject> actions)
        {
            if(this.goalkeeper.dynamicState.hasBall)
            {
                // 1. The goalkeeper has the ball, so he kicks it to a defender...
                getAction_Goalkeeper_HasBall(actions);
            }
            else if(ballIsInGoalArea())
            {
                // 2. The ball is in the goal area, so the goalkeeper tries
                //    to take possession...
                getAction_Goalkeeper_BallInGoalArea(actions);
            }
            else
            {
                // 3. The ball is outside the goal-area, so the goalkeeper
                //    keeps between it and the goal...
                getAction_Goalkeeper_BallOutsideGoalArea(actions);
            }
        }

        private List<Position> getGoalKeeperToPositionList()
        {
            List<Position> positionList = new List<Position>();
            //double pitchHeight = this.pitch.height;
            double offset = 5.0;
            int counter = 4;
            if (this.playingDirection == DirectionType.RIGHT)
                offset = -5.0;
            double weight = (double)this.pitch.centreSpot.x + 2*offset;
            while (counter > 0)
            {
                double pHeight = (double)this.pitch.height - 10;
                while (pHeight > 0)
                {
                    positionList.Add(new Position(weight, pHeight));
                    pHeight -= 10;
                }
                weight += offset;
                counter--;
            }
            return positionList;
        }

        private void getGoalKeeperToPlayer(Position goalkeeperPosition)
        {
            int playerOne = 0, playerTwo = 0, playerThree = 0;
            getPlayersInOrderNearBall(ref playerOne, ref playerTwo, ref playerThree);

            var playerOnevar = this.allTeamPlayers[playerOne];
            var playerTwovar = this.allTeamPlayers[playerTwo];
            Position PlayerOnePosition = new Position(playerOnevar.dynamicState.position);
            Position PlayerTwoPosition = new Position(playerTwovar.dynamicState.position);

            List<Position> positionList = getGoalKeeperToPositionList();
            double diffDistanceOne = 0.0,diffDistanceTwo = 0.0 ;
            Position destPositionOne = null, destPositionTwo= null ;
            foreach(var pos in positionList)
            {
                double oppDistance = getMinOppCalcDistance(pos);
                if (PlayerOnePosition.distanceFrom(pos) < PlayerTwoPosition.distanceFrom(pos))
                {
                    double myDistance = PlayerOnePosition.distanceFrom(pos);
                    if (myDistance < oppDistance)
                    {
                        if ((diffDistanceOne == 0.0) ||((diffDistanceOne < (oppDistance - myDistance)) && (diffDistanceOne < 2.0)))
                        {
                            diffDistanceOne = oppDistance - myDistance;
                            destPositionOne = pos;
                        }
                    }
                }
                else
                {
                    double myDistance = PlayerTwoPosition.distanceFrom(pos);
                    if (myDistance < oppDistance)
                    {
                        if ((diffDistanceTwo == 0.0) ||((diffDistanceTwo < (oppDistance - myDistance)) && (diffDistanceTwo < 2.0)))
                        {
                            diffDistanceTwo = oppDistance - myDistance;
                            destPositionTwo = pos;
                        }
                    }
                }   
            }

            double offset = 1.0;
            if (this.playingDirection == DirectionType.LEFT)
                offset = -1.0;

            if (destPositionOne == null)
            {
                if (destPositionTwo != null)
                {
                    double yoffset = 15.0;
                    if (destPositionTwo.y > (this.pitch.height / 2))
                        yoffset = -15;

                    setPlayerNextPostion(playerOne, new Position((double)destPositionTwo.x + (offset*10), destPositionTwo.y + yoffset));
                }
                else
                    setPlayerNextPostion(playerOne, new Position((double)PlayerOnePosition.x + (offset *15), PlayerOnePosition.y));
            }
            else
                setPlayerNextPostion(playerOne, destPositionOne);

            if (destPositionTwo == null)
            {
                if (destPositionOne != null)
                {
                    double yoffset = 15.0;
                    if (destPositionOne.y > (this.pitch.height / 2))
                        yoffset = -15;

                    setPlayerNextPostion(playerTwo, new Position((double)destPositionOne.x + (offset *10), destPositionOne.y + yoffset));
                }
                setPlayerNextPostion(playerTwo, new Position((double)PlayerTwoPosition.x + (offset *15), PlayerTwoPosition.y));
            }
            else
                setPlayerNextPostion(playerTwo, destPositionTwo);


            Position thirdPlayerPosition = getNewScaledPosition(getOppGoalKeeperPosition(), 40.0, (double)this.pitch.goalAreaRadius + 2.0);
            setPlayerNextPostion(playerThree, thirdPlayerPosition);
        }

        private void getPlayerNumToPass2ndAttempt(Position goalkeeperPosition, ref int passPlayerNumber, ref Position passPosition, ref int otherPlayerNumber)
        {
            int playerOne = 0, playerTwo = 0, playerThree = 0;
            double diffDistance = 0.0, myDistance = 0.0;
            getPlayersInOrderNearBall(ref playerOne, ref playerTwo, ref playerThree);

            Position PlayerOnePosition = new Position(this.allTeamPlayers[playerOne].dynamicState.position);
            Position PlayerTwoPosition = new Position(this.allTeamPlayers[playerTwo].dynamicState.position);
            Position PlayerThreePosition = new Position(this.allTeamPlayers[playerThree].dynamicState.position);
            List<Position> positionList = getGoalKeeperToPositionList();

            foreach (var pos in positionList)
            {
                if (!GoalToPlayerClear(pos))
                    continue;
                double oppDistance = getMinOppCalcDistance(pos);

                if (PlayerOnePosition.distanceFrom(pos) < PlayerTwoPosition.distanceFrom(pos))
                {
                    myDistance = PlayerOnePosition.distanceFrom(pos);
                    if ((oppDistance - myDistance) > diffDistance)
                    {
                        diffDistance = oppDistance - myDistance;
                        passPosition = pos;
                        passPlayerNumber = playerOne;
                        otherPlayerNumber = playerTwo;
                    }
                }
                else
                {
                    myDistance = PlayerTwoPosition.distanceFrom(pos);
                    if ((oppDistance - myDistance) > diffDistance)
                    {
                        diffDistance = oppDistance - myDistance;
                        passPosition = pos;
                        passPlayerNumber = playerTwo;
                        otherPlayerNumber = playerOne;
                    }
                }
            }

            if (passPlayerNumber == 0)
            {
                if (GoalToPlayerClear(PlayerOnePosition) && !areOpponentInLine(goalkeeperPosition, PlayerOnePosition))
                {
                    passPosition = PlayerOnePosition;
                    passPlayerNumber = playerOne;
                    otherPlayerNumber = playerTwo;
                }
                else if (GoalToPlayerClear(PlayerTwoPosition) && !areOpponentInLine(goalkeeperPosition, PlayerTwoPosition))
                {
                    passPosition = PlayerTwoPosition;
                    passPlayerNumber = playerTwo;
                    otherPlayerNumber = playerOne;
                }
                else
                {
                    passPosition = PlayerThreePosition;
                    passPlayerNumber = playerThree;
                    otherPlayerNumber = playerTwo;
                }
            }
        }

        private void getPlayerNumberToPass(Position goalkeeperPosition, ref int passPlayerNumber, ref Position passPosition, ref int otherPlayerNumber)
        {
            int playerOne = 0, playerTwo = 0, playerThree = 0;
            getPlayersInOrderNearBall(ref playerOne, ref playerTwo, ref playerThree);

            var playerOnevar = this.allTeamPlayers[playerOne];
            var playerTwovar = this.allTeamPlayers[playerTwo];

            Position PlayerOnePosition = new Position(playerOnevar.dynamicState.position);
            Position PlayerTwoPosition = new Position(playerTwovar.dynamicState.position);
   
            Position playerOneProjPos = ProjectedPositions[playerOne];
            Position playerTwoProjPos = ProjectedPositions[playerTwo];
            bool bOne = false, bTwo = false;

            if (((getMinOppCalcDistance(playerOneProjPos) - PlayerOnePosition.distanceFrom(playerOneProjPos)) > 1.5) &&
                GoalToPlayerClear(playerOneProjPos))
            {
                bOne = true;
                passPlayerNumber = playerOne;
                passPosition = playerOneProjPos;
                otherPlayerNumber = playerTwo;
            }
            if (((getMinOppCalcDistance(playerTwoProjPos) - PlayerTwoPosition.distanceFrom(playerTwoProjPos)) > 1.5) &&
                GoalToPlayerClear(playerTwoProjPos))
            {
                bTwo = true;
                passPlayerNumber = playerTwo;
                passPosition = playerTwoProjPos;
                otherPlayerNumber = playerOne;
            }

            if (bOne && bTwo)
            {
                if ((getOppGoalKeeperPosition().distanceFrom(playerOneProjPos) < getOppGoalKeeperPosition().distanceFrom(playerTwoProjPos)) && bOne)
                {
                    bTwo = false;
                    passPlayerNumber = playerOne;
                    passPosition = playerOneProjPos;
                    otherPlayerNumber = playerTwo;
                }
                else if ((getOppGoalKeeperPosition().distanceFrom(playerTwoProjPos) < getOppGoalKeeperPosition().distanceFrom(playerOneProjPos)) && bTwo)
                {
                    bOne = false;
                    passPlayerNumber = playerTwo;
                    passPosition = playerTwoProjPos;
                    otherPlayerNumber = playerOne;
                }
            }
        }

        /// <summary>
        /// The goalkeeper kicks the ball to a defender.
        /// 
        /// </summary>
        private void getAction_Goalkeeper_HasBall(List<JSObject> actions)
        {
            Position passPosition = null;
            int passPlayerNumber = 0, otherPlayerNumber = 0;

            Position goalCenterPos = getGoalCentre(GoalType.OUR_GOAL);
            this.goalKeeperCount++;
            if (this.goalKeeperCount <= 10)
            {
                moveToPosition(ref actions, this.goalkeeperPlayerNumber, goalCenterPos);
                return;
            }
            
            Position goalkeeperPosition = new Position(this.goalkeeper.dynamicState.position);
            if (this.goalKeeperPassPosition == null)
            {
                if (passPlayerNumber == 0)
                {
                    getPlayerNumToPass2ndAttempt(goalkeeperPosition, ref passPlayerNumber, ref passPosition, ref otherPlayerNumber);
                }

                if (passPlayerNumber == 0)
                {
                    Logger.log(String.Format("NULL"), Logger.LogLevel.INFO);
                }
                setPlayerNextPostion(passPlayerNumber, passPosition);
                setPlayerNextPostion(otherPlayerNumber, new Position(passPosition.x + (getTheirOffSet() * 15.0), ProjectedPositions[otherPlayerNumber].y));
                this.goalKeeperPassPosition = passPosition;
            }
            else
                passPosition = this.goalKeeperPassPosition;

            double absAngle = getAbsoluteAngle(goalkeeperPosition, passPosition);
            double angleDiff = Math.Abs(absAngle - (double)this.goalkeeper.dynamicState.direction);
            if (angleDiff <= 10.0) // CHECK IF player is in appropriate direction to kick the ball
            {
                this.goalKeeperCount = 0;
                this.goalKeeperPassPosition = null;
                // We kick the ball to the defender...
                if (passPosition != null)
                {
                    var action = new JSObject();
                    action.add("action", "KICK");
                    action.add("playerNumber", this.goalkeeperPlayerNumber);
                    action.add("destination", passPosition);
                    action.add("speed", 100);
                    actions.Add(action);
                    Logger.log(String.Format("Goalkeeper kicks the ball to {0}", passPosition), Logger.LogLevel.INFO);
                }
            }
            else //ask the player to turn
            {
                var action = new JSObject();
                action.add("action", "TURN");
                action.add("playerNumber", this.goalkeeperPlayerNumber);
                action.add("direction", absAngle);
                actions.Add(action);
                Logger.log(String.Format("{0}: GoalKeeper current direction {1} turns to direction {2}", this.gameTimeSeconds, this.goalkeeper.dynamicState.direction, absAngle), Logger.LogLevel.INFO);
            }
            
        }

        private int getTheirOffSet()
        {
            if (this.playingDirection == DirectionType.RIGHT)
                return 1;
            else
                return -1;
        }

        private bool nextBallPosPassGolie()
        {
            Position nextBallPosition = getNextBallPosition();
            Position goalkeeperPosition = new Position(this.goalkeeper.dynamicState.position);

            if (this.playingDirection == DirectionType.RIGHT)
            {
                if ((nextBallPosition.x < goalkeeperPosition.x) &&
                    ((goalkeeperPosition.x - nextBallPosition.x) > 0.1))
                    return true;
                else
                    return false;
            }
            else
            {
                if ((nextBallPosition.x > goalkeeperPosition.x) &&
                       (( nextBallPosition.x - goalkeeperPosition.x) > 0.1))
                    return true;
                else
                    return false;

            }
        }

        private Position getNextBallPosition()
        {
            double scaleFactor = ((double)this.ball.speed / 10.0) - 0.05;
            Position ballPosition = new Position(this.ball.position);
            Vector v = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
            v = v.getScaledVector(scaleFactor);
            return ballPosition.getPositionPlusVector(v);
        }

        /// <summary>
        /// The goalkeeper tries to take possession of the ball.
        /// </summary>
        private void getAction_Goalkeeper_BallInGoalArea(List<JSObject> actions)
        {
            // If we are within 5m of the ball, we try to take possession.
            // Otherwise we move towards the ball.
            var ballPosition = new Position(this.ball.position);
            var goalkeeperPosition = new Position(this.goalkeeper.dynamicState.position);

            double distance = goalkeeperPosition.distanceFrom(ballPosition);
            if((distance < 1.5) || nextBallPosPassGolie())
            {
                // We are close to the ball, so we try to take possession...
                var action = new JSObject();
                action.add("action", "TAKE_POSSESSION");
                action.add("playerNumber", this.goalkeeperPlayerNumber);
                actions.Add(action);
                Logger.log(String.Format("GOAL STOPPED goalkeeper position {0}, ball position {1}, ball speed {2}", goalkeeperPosition, ballPosition, this.ball.speed), Logger.LogLevel.INFO);

            }
            else
            {
                // We are too far away, so we move towards the ball...              
                Vector ballvector = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
                Position origBallPosition = new Position(ballPosition.x, ballPosition.y);

                if ((distance < 3) && (this.lastControlPlayerNumber != this.goalkeeperPlayerNumber))
                {
                    ballPosition = getNextBallPosition();
                    moveToPosition(ref actions, this.goalkeeperPlayerNumber, ballPosition);
                    return;

                }

                if ((this.ball.speed > 3) )//&& (ballvector.x != 0))//|| ballvector.y != 0))
                {
                    Position endBallPosition = getEndBallPositionBeforeGoal();
                    if ((endBallPosition.y >= this.pitch.goalY1 -0.5) && (endBallPosition.y <= this.pitch.goalY2 + 0.5)
                        && (endBallPosition.distanceFrom(getGoalCentre(GoalType.OUR_GOAL)) < this.pitch.goalAreaRadius))
                    {
                        moveToPosition(ref actions, this.goalkeeperPlayerNumber, endBallPosition);
                        return;
                    }
                    else
                    {
                        moveToPosition(ref actions, this.goalkeeperPlayerNumber, getGoalCentre(GoalType.OUR_GOAL));
                        return;
                    }
                }

                moveToPosition(ref actions, this.goalkeeperPlayerNumber, ballPosition);
                Logger.log(String.Format("Ball in Goal Area,  goalkeeper position {0}, ball position {1}, goalkeeper moves to position {2}", goalkeeperPosition, origBallPosition, ballPosition), Logger.LogLevel.INFO);

            }
        }

        /// <summary>
        /// The goalkeeper keeps between the ball and the goal.
        /// </summary>
        private void getAction_Goalkeeper_BallOutsideGoalArea(List<JSObject> actions)
        {
            // We move to a point between the ball and the goal, 5m away from 
            // the goal centre.
            Position goalCentre = getGoalCentre(GoalType.OUR_GOAL);
            var ballPosition = new Position(this.ball.position);
            Vector ballvector = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);

            if ((this.ball.speed > 0) && (ballIsInGoalAreaPlus15()))
            {
                Position endBallPosition = getEndBallPositionBeforeGoal();

                if ( (endBallPosition.x > this.pitch.width) ||(endBallPosition.x < 0))
                {
                    AdjustIfInOutsideArea(ballPosition, ref endBallPosition);
                }

                if (ballIsInGoalArea(endBallPosition))
                {
                    if ((endBallPosition.y >= this.pitch.goalY1-0.5) && (endBallPosition.y <= this.pitch.goalY2+0.5) &&
                        (endBallPosition.distanceFrom(goalCentre) < this.pitch.goalAreaRadius))
                    {
                        moveToPosition(ref actions, this.goalkeeperPlayerNumber, endBallPosition);
                        Logger.log(String.Format("Goalkeeper moves to end position {0}, ball outside goal area with speed {1} and position {2} ", endBallPosition, this.ball.speed, ballPosition), Logger.LogLevel.INFO);
                        return;
                    }
                }
                else
                {
                    moveToPosition(ref actions, this.goalkeeperPlayerNumber, goalCentre);
                    return;
                }
            }
            else
            {
                moveToPosition(ref actions, this.goalkeeperPlayerNumber,  goalCentre);
                return;
            }

            double xoffset = 0.0;

            if (this.playingDirection == DirectionType.RIGHT )
                xoffset = 1.0;
            else
                xoffset = -1.0;

            var ballscaledVector = ballvector.getScaledVector(5.0);
            var ballNewPosition = ballPosition.getPositionPlusVector(ballscaledVector);
            double slope = (ballPosition.y - ballNewPosition.y) / (ballPosition.x - ballNewPosition.x);

            double y = (slope * (goalCentre.x + xoffset - ballPosition.x)) + ballPosition.y;
            if ((this.pitch.goalY1 <= y) && (this.pitch.goalY2 >= y) && ((double)this.ball.speed) >=10)
            {
                Position projectedPosition = new Position(goalCentre.x + xoffset, Math.Round(y, 2));
                // We move to this position...
                moveToPosition(ref actions, this.goalkeeperPlayerNumber, projectedPosition);

                Logger.log(String.Format("Goalkeeper moves to projected position {0}, ball outside goal area with speed {1} and position {2} ", projectedPosition, this.ball.speed, ballPosition), Logger.LogLevel.INFO);

            }
            else
            { 
                // We find the vector between the goal-centre and the ball...
                //var goalCentre = getGoalCentre(GoalType.OUR_GOAL);
                //var ballPosition = new Position(this.ball.position);
                var vector = new Vector(goalCentre, ballPosition);
                // We find the position 5m from the goal-centre...
                var vector2m = vector.getScaledVector(2.0);
                var moveTo = goalCentre.getPositionPlusVector(vector2m);
                moveToPosition(ref actions, this.goalkeeperPlayerNumber, moveTo);
            }
        }
        
        /// <summary>
        /// True if the ball is in our goal-area, false if not.
        /// </summary>
        private bool ballIsInGoalArea()
        {
            var ballPosition = new Position(this.ball.position);
            var goalCentre = getGoalCentre(GoalType.OUR_GOAL);
            return ballPosition.distanceFrom(goalCentre) < this.pitch.goalAreaRadius;
        }

        private bool ballIsInGoalArea(Position ballPosition)
        {
            var goalCentre = getGoalCentre(GoalType.OUR_GOAL);
            return ballPosition.distanceFrom(goalCentre) < this.pitch.goalAreaRadius;
        }


        /// <summary>
        /// True if the ball is in our goal-area, false if not.
        /// </summary>
        private bool ballIsInGoalAreaPlus15()
        {
            var ballPosition = new Position(this.ball.position);
            var goalCentre = getGoalCentre(GoalType.OUR_GOAL);
            return ballPosition.distanceFrom(goalCentre) < (this.pitch.goalAreaRadius + 15.0 );
        }

        /// <summary>
        /// Sets the default position for (player, direction).
        /// </summary>
        private void setDefaultPosition(int playerNumber, DirectionType playingDirection, Position position, double direction)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            var value = new PositionValue{position = position, direction=direction};
            this.defaultPositions[key] = value;
        }

        /// <summary>
        /// Returns the default position for the player passed in, when playing 
        /// in the direction passed in.
        /// </summary>
        private PositionValue getDefaultPosition(int playerNumber, DirectionType playingDirection)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            return this.defaultPositions[key];
        }

        /// <summary>
        /// Sets the kickoff position for (player, direction).
        /// </summary>
        private void setKickoffPosition(int playerNumber, DirectionType playingDirection, Position position, double direction)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            var value = new PositionValue{position = position, direction=direction};
            this.kickoffPositions[key] = value;
        }

        /// <summary>
        /// Returns the default position for (player, direction).
        /// </summary>
        private PositionValue getKickoffPosition(int playerNumber, DirectionType playingDirection)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            return this.kickoffPositions[key];
        }

        #endregion

        #region Private data

        // The player-numbers for the various positions...
        private int playerNumber_LeftWingDefender = -1;
        private int playerNumber_CentreDefender = -1;
        private int playerNumber_RightWingDefender = -1;
        private int playerNumber_LeftWingForward = -1;
        private int playerNumber_RightWingForward = -1;

        // Default positions and directions, keyed by player-number and direction...
        private class PositionKey : Tuple<int, DirectionType> 
        {
            public PositionKey(int playerNumber, DirectionType direction): base(playerNumber, direction) { }
        }
        private class PositionValue
        {
            public Position position;
            public double direction;
        }
        private Dictionary<PositionKey, PositionValue> defaultPositions = new Dictionary<PositionKey, PositionValue>();
        private Dictionary<PositionKey, PositionValue> kickoffPositions = new Dictionary<PositionKey, PositionValue>();

        // For some random choices...
        private Random rnd = new Random();

        private List<PositionValue> coordinates10RadiusLEFT = new List<PositionValue>();
        private List<PositionValue> coordinates10RadiusRIGHT = new List<PositionValue>();
        //private double DIRECTION_RIGHT_LOW = 10.0;
        private double DIRECTION_RIGHT_MID = 90.0;
        //private double DIRECTION_RIGHT_HIGH = 170.0;
        //private double DIRECTION_LEFT_LOW = 190.0;
        private double DIRECTION_LEFT_MID = 270.0;
        private double DIRECTION_LEFT_HIGH = 350.0;
        private double DIRECTION_UP = 0.0;
        private double DIRECTION_DOWN = 180.0;
        bool bBallOutSideGoalArea = true;
        Position goalKeeperPassPosition = null;


        private Dictionary<int, Position> ProjectedPositions = new Dictionary<int, Position>();
        private Position globalShootAt = null;
        int goalKeeperCount = 0;
        
 
        #endregion
    }
}
