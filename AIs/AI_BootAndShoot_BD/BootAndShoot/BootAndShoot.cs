using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Helpers;

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
            setDefaultPosition(this.goalkeeperPlayerNumber, DirectionType.RIGHT, new Position(10.000, 25.000), 90);
            setDefaultPosition(this.goalkeeperPlayerNumber, DirectionType.LEFT, new Position(90.000, 25.000), 270);

            setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.RIGHT, new Position(15, 21), 90);
             setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(15, 29), 90);
             //setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.RIGHT, new Position(25, 10), 90);
            setDefaultPosition(this.playerNumber_CentreDefender, DirectionType.RIGHT, new Position(25, 25), 90);
             //setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(25, 40), 90);
            setDefaultPosition(this.playerNumber_LeftWingForward, DirectionType.RIGHT, new Position(75, 15), 90);
            setDefaultPosition(this.playerNumber_RightWingForward, DirectionType.RIGHT, new Position(75, 35), 90);

            setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.LEFT, new Position(85, 29), 270);
            setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(85, 21), 270);
            //setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.LEFT, new Position(75, 40), 270);
            setDefaultPosition(this.playerNumber_CentreDefender, DirectionType.LEFT, new Position(75, 25), 270);
            //setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(75, 10), 270);
            setDefaultPosition(this.playerNumber_LeftWingForward, DirectionType.LEFT, new Position(25, 35), 270);
            setDefaultPosition(this.playerNumber_RightWingForward, DirectionType.LEFT, new Position(25, 15), 270);

            //Set Attack Position
            setAttackPosition(this.playerNumber_LeftWingForward, DirectionType.RIGHT, new Position(80, 15), 90);
            setAttackPosition(this.playerNumber_RightWingForward, DirectionType.RIGHT, new Position(80, 35), 90);

            setAttackPosition(this.playerNumber_LeftWingForward, DirectionType.LEFT, new Position(20, 35), 270);
            setAttackPosition(this.playerNumber_RightWingForward, DirectionType.LEFT, new Position(20, 15), 270);

            // We set up the kickoff positions. 
            // Note: The player nearest the centre will be automatically "promoted"
            //       to the centre by the game.
            setKickoffPosition(this.playerNumber_LeftWingDefender, DirectionType.RIGHT, new Position(15, 21), 90);
            setKickoffPosition(this.playerNumber_CentreDefender, DirectionType.RIGHT, new Position(25, 25), 90);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(15, 29), 90);
            setKickoffPosition(this.playerNumber_LeftWingForward, DirectionType.RIGHT, new Position(49, 14), 90);
            setKickoffPosition(this.playerNumber_RightWingForward, DirectionType.RIGHT, new Position(49, 36), 90);

            setKickoffPosition(this.playerNumber_LeftWingDefender, DirectionType.LEFT, new Position(85, 29), 270);
            setKickoffPosition(this.playerNumber_CentreDefender, DirectionType.LEFT, new Position(75, 25), 270);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(85, 21), 270);
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

            // We add actions to the reply for each of our players...
            getAction_Defender(actions, this.playerNumber_LeftWingDefender);
            getAction_Defender(actions, this.playerNumber_CentreDefender);
            getAction_Defender(actions, this.playerNumber_RightWingDefender);
            getAction_Forward(actions, this.playerNumber_LeftWingForward);
            getAction_Forward(actions, this.playerNumber_RightWingForward);
            getAction_Goalkeeper(actions);

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

        protected override void processRequest_ConfigureAbilities(dynamic data)
        {
            // As a default, we give all players equal abilities...
            double totalKickingAbility = data.totalKickingAbility;
            double totalRunningAbility = data.totalRunningAbility;
            double totalBallControlAbility = data.totalBallControlAbility;
            double totalTacklingAbility = data.totalTacklingAbility;

            //1	70	60
            //2	70	60
            //3	70	60
            //4	65	80
            //5	65	80
            //6	60	60
	        //T 400	400

            double playerKickingAbility =0;
            double playerRunningAbility = 0;
            double playerBallControlAbility = 0;
            double playerTacklingAbility = 0;

            int numberOfPlayers = this.teamPlayers.Count + 1; // +1 for the goalkeeper

            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "CONFIGURE_ABILITIES");

            // We give each player an average ability in all categories...
            var playerInfos = new List<JSObject>();
            foreach (var playerNumber in this.allTeamPlayers.Keys)
            {

                if (playerNumber == playerNumber_LeftWingDefender || playerNumber == playerNumber_RightWingDefender)
                {
                    playerKickingAbility = 0;
                    playerRunningAbility = 0;
                    playerBallControlAbility = 0;
                    playerTacklingAbility = 0;
                }
                else if(playerNumber == goalkeeperPlayerNumber)
                {
                    playerKickingAbility = 100;
                    playerRunningAbility = 100;//60
                    playerBallControlAbility = 100;
                    playerTacklingAbility = 100;
                }
                else
                {
                    playerKickingAbility = 100;
                    playerRunningAbility = 100;//60
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
            }
            reply.add("players", playerInfos);

            sendReply(reply);
        }
        #endregion

        #region Private functions
        //calls only when player is moving to default position
        private dynamic moveOrTurn(int playerNumber)
        {
            var player = this.allTeamPlayers[playerNumber];
            var defaultPosition = getDefaultPosition(playerNumber, this.playingDirection);
            return moveOrTurn(player, defaultPosition.position);        
        }
        private dynamic moveOrTurn(dynamic player, Position targetPosition)
        {
            int playerNumber = player.staticState.playerNumber;
            var ballPosition = this.ball.position;
            var playerPosition = new Position(player.dynamicState.position);
            if (!playerPosition.IsEqual(targetPosition))
            {
                var action = js.move(playerNumber, targetPosition);
                return action;
            }
            else
            {
                double direction = playerPosition.getAngle(ballPosition);
                var action = js.turn(playerNumber, direction);
                return action;
            }
        }

        private dynamic kickOrTurn(dynamic player, Position destination,double speed = 100.0)
        {
            var playerNumber = player.staticState.playerNumber;
            var playerPosition = new Position(player.dynamicState.position);
            double playerDirection = (double)player.dynamicState.direction;
            double destinationDirection = Math.Round(playerPosition.getAngle(destination),3);
            double distanceFCP = distanceFromClosest(playerNumber);
            if (playerDirection == destinationDirection || distanceFCP <= 1 )
            {
                var action = js.kick(playerNumber, destination, speed);
                return action;
            }
            else
            {
                var action=js.turn(playerNumber, destinationDirection);
                return action;
            }

        }
       

        //Called by goalkeeper only
        // not used yet
        private dynamic moveOrTP(Position moveTo)
        {
            var player = this.goalkeeper;
            var ballPosition = this.ball.position;
            int playerNumber = this.goalkeeperPlayerNumber;

            var playerPosition = new Position(player.dynamicState.position);
            if (!playerPosition.IsEqual(moveTo))
            {
                var action = js.move(playerNumber, moveTo);
                return action;
            }
            else
            {
                var action = js.takePossession(playerNumber);
                return action;
            }
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
                     (this.playingDirection == DirectionType.LEFT && ballPosition.x > this.pitch.centreSpot.x))
            {
                // The ball is in the defender's half, so he tries to get it...

                if(playerNumber == findClosestTeamPlayerFromBall())
                {
                    getAction_OwnHalf(actions, player);
                }
                else
                {
                    // The ball is in the other region, so the player moves back 
                    // to his default position...
                    var action = moveOrTurn(playerNumber);
                    actions.Add(action);        
                }
            }
            else
            {
                // The ball is in the other half, so the player moves back 
                // to his default position...
                var action = moveOrTurn(playerNumber);
                actions.Add(action); 
            }
        }

        /// <summary>
        /// find out the distance from closest player.
        /// calling this method only for our team player
        /// </summary>
        public double distanceFromClosest(int playerNumber)
        {

            var player = this.allTeamPlayers[playerNumber];
            var playerPosition = new Position(player.dynamicState.position);
            double distance,minDistance = 100;
            foreach (var otherplayerNumber in this.teamPlayers.Keys)
            {
                if (otherplayerNumber != playerNumber)
                {
                    var otherPlayer = this.allTeamPlayers[otherplayerNumber];
                    var otherPlayerPosition = new Position(otherPlayer.dynamicState.position);
                    distance = playerPosition.distanceFrom(otherPlayerPosition);
                    minDistance = minDistance < distance ? minDistance : distance;
                }
            }

            foreach (var otherplayerNumber in this.allOpposingTeamPlayers.Keys)
            {
                var otherPlayer = this.allOpposingTeamPlayers[otherplayerNumber];
                var otherPlayerPosition = new Position(otherPlayer.dynamicState.position);
                distance = playerPosition.distanceFrom(otherPlayerPosition);
                minDistance = minDistance < distance ? minDistance : distance;
            }

            return minDistance;

        }

        /// <summary>
        /// find out the closest player from ball.
        /// calling this method only for our team player
        /// </summary>
        public int findClosestTeamPlayerFromBall()
        {
            Position ballPosition = this.ball.position;
            double distance, minDistance = 100;
            int tempPlayerNumber = -1;
            foreach (var playerNumber in this.teamPlayers.Keys)
            {
                if (playerNumber != playerNumber_LeftWingDefender && playerNumber != playerNumber_RightWingDefender && playerNumber != goalkeeperPlayerNumber)
                {

                    var player = this.allTeamPlayers[playerNumber];
                    var playerPosition = new Position(player.dynamicState.position);
                    distance = playerPosition.distanceFrom(ballPosition);
                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        tempPlayerNumber = playerNumber;
                    }                   
                }

            }

            return tempPlayerNumber;


        }

        /// <summary>
        /// Kicks the ball from a defending player to a forward.
        /// </summary>
        private void getAction_Defender_HasBall(List<JSObject> actions, dynamic player)
        {
            var playerNumber = player.staticState.playerNumber;

            //Choose forward base on closest distance
            var leftWingForwardPosition = new Position(this.allTeamPlayers[this.playerNumber_LeftWingForward].dynamicState.position);
            var rightWingForwardPosition = new Position(this.allTeamPlayers[this.playerNumber_RightWingForward].dynamicState.position);
            var playerPosition = new Position(player.dynamicState.position);

            double distanceFromRWF = playerPosition.distanceFrom(rightWingForwardPosition);
            double distanceFromLWF = playerPosition.distanceFrom(leftWingForwardPosition);

            int forwardPlayerNumber = (distanceFromLWF < distanceFromRWF) ? this.playerNumber_LeftWingForward : this.playerNumber_RightWingForward;

            var forward = this.allTeamPlayers[forwardPlayerNumber];
            var destination = new Position(forward.dynamicState.position);

            // We kick the ball towards him...
            var action = kickOrTurn(player, destination);
            actions.Add(action);
                        
               
                
 
        }

        /// <summary>
        /// The player tries to get the ball.
        /// </summary>
        private void getAction_OwnHalf(List<JSObject> actions, dynamic player)
        {
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
            var ballPosition = this.ball.position;
            double distance = playerPosition.distanceFrom(ballPosition);
            if (distance < 1)
            {
                // We attempt to take possession of the ball...
                var action = js.takePossession(playerNumber);
                actions.Add(action);
            }
            else
            {
                // We move towards the ball...
                var destination = ballPosition;
                if (ball.speed > 0)
                {
                    destination = ball.BallStopPosition(pitch);
                }
                var action = js.move(playerNumber, destination);
                actions.Add(action);
            }

        }

        /// <summary>
        /// Gets the current action for one of the forwards.
        /// </summary>
        private void getAction_Forward(List<JSObject> actions, int playerNumber)
        {
            var player = this.allTeamPlayers[playerNumber];
            var ballPosition = this.ball.position;

            if (player.dynamicState.hasBall)
            {
                // The player has the ball, so we kick it at the goal...
                getAction_Forward_HasBall(actions, player);
            }
            else if ((this.playingDirection == DirectionType.RIGHT && ballPosition.x > this.pitch.centreSpot.x)
                     ||
                     (this.playingDirection == DirectionType.LEFT && ballPosition.x < this.pitch.centreSpot.x))
            {
                // The ball is in the forward's half, so he tries to get it...
                if(playerNumber == findClosestTeamPlayerFromBall())
                {
                    getAction_OwnHalf(actions, player);
                }
                else
                {
                    // The ball is in the other region, so the player moves to attacking position
                    var attackPosition = getAttackPosition(playerNumber, this.playingDirection);
                    var action = moveOrTurn(player, attackPosition.position);
                    actions.Add(action);

                }
            }
            else
            {
                // The ball is in the other half, so the player moves back 
                // to his default position...
                var action = moveOrTurn(playerNumber);
                actions.Add(action);

            }
        }

        /// <summary>
        /// The forward shoots at the goal.
        /// </summary>
        private void getAction_Forward_HasBall(List<JSObject> actions, dynamic player)
        {
            int playerNumber = player.staticState.playerNumber;


            var playerPosition = new Position(player.dynamicState.position);

            int nextForward = (playerNumber == this.playerNumber_RightWingForward )? this.playerNumber_LeftWingForward : this.playerNumber_RightWingForward;
            var nextForwardPosition = new Position(this.allTeamPlayers[nextForward].dynamicState.position);

            double distanceFromGoal = playerPosition.distanceFrom(getGoalCentre(GoalType.THEIR_GOAL));
            double distanceFromAnotherForward = playerPosition.distanceFrom(nextForwardPosition);
                
            var linePoint = new Position(50.0, playerPosition.y);
            double distanceFromCentreLine = playerPosition.distanceFrom(linePoint);
            if (distanceFromCentreLine < 10 && distanceFromGoal > distanceFromAnotherForward)
            {
                 //We kick the ball towards next Forward...
                 double speed = Math.Sqrt(2*10*distanceFromAnotherForward);
                 double speedPercentage = (speed / 30.0)*100;
                 var action = kickOrTurn(player, nextForwardPosition, speedPercentage);
                actions.Add(action);
            }
            else
            {

                var shootAt = getTarget();

                // We kick the ball at the goal...
                var action = shootOrTurn(player, shootAt);


                actions.Add(action);
            }
        }
        private Position getTarget()
        {
            dynamic opponentGoalKeeper = null;
            foreach(int playerNumber in allOpposingTeamPlayers.Keys)
            {
                opponentGoalKeeper = allOpposingTeamPlayers[playerNumber];
                if (opponentGoalKeeper.staticState.playerType != "P")
                    break;
            }
            var shootAt = getGoalCentre(GoalType.THEIR_GOAL);
            if(opponentGoalKeeper != null)
            {
                double maxDistance = 0;
                Position opponentGoalKeeperPosition = new Position(opponentGoalKeeper.dynamicState.position);
                for(int i = 21; i< 30 ; i++)
                {
                    var shootPosition = new Position(shootAt.x,i);
                    if(opponentGoalKeeperPosition.distanceFrom(shootPosition) > maxDistance)
                    {
                        maxDistance = opponentGoalKeeperPosition.distanceFrom(shootPosition);
                        shootAt = shootPosition;
                    }
                }
                
            }
            return shootAt;
        }
        //Call to score goal only
        private dynamic shootOrTurn(dynamic player, Position destination, double speed = 100.0)
        {
            var playerNumber = player.staticState.playerNumber;
            var playerPosition = new Position(player.dynamicState.position);
            double playerDirection = (double)player.dynamicState.direction;
            double destinationDirection = Math.Round(playerPosition.getAngle(destination), 3);
            double distanceFCP = distanceFromClosest(playerNumber);
            if (playerDirection == destinationDirection || distanceFCP <= 1)
            {
                if (distanceFCP <= 1)
                {
                    destination = getGoalCentre(GoalType.THEIR_GOAL);
                    destination.y += ((this.rnd.NextDouble() < 0.5) ? 2.0 : -2.0);
                }
                var action = js.kick(playerNumber, destination, speed);
                return action;
            }
            else
            {
                var action = js.turn(playerNumber, destinationDirection);
                return action;
            }

        }

        /// <summary>
        /// Gets the current action for the goalkeeper.
        /// </summary>
        private void getAction_Goalkeeper(List<JSObject> actions)
        {
            Position ballPosition = new Position(this.ball.position);

            Position goalkeeperPosition = new Position(this.goalkeeper.dynamicState.position);
            double distance = goalkeeperPosition.distanceFrom(ballPosition);
            if (this.goalkeeper.dynamicState.hasBall)
            {
                // 1. The goalkeeper has the ball, so he kicks it to a defender...
                getAction_Goalkeeper_HasBall(actions);
            }

            else if (ballIsInGoalArea() && distance <= 0.5)
            {

                // We are close to the ball, so we try to take possession...
                var action = js.takePossession(this.goalkeeperPlayerNumber);

                //clearing old position
                oldPosition = new Position(0.0,0.0);
                actions.Add(action);
            }
            else if (ballIsInGoalArea() && (ball.speed == 0))
            {
                var action = js.move(this.goalkeeperPlayerNumber, ballPosition);
                actions.Add(action);
            }
            else if (ball.speed != 0)
            {
                //ball is coming towards goal need to defend

                Vector ballVector = this.ball.vector;
                Position goalCentre = getGoalCentre(GoalType.OUR_GOAL);
                bool found = false;
                int scale = (int)(ball.speed * ball.speed) / (2 * 10);
                double counter = 0.0;
                var moveTo = goalCentre;
                for(int i = 0; i <=scale; i++)
                {
                    Vector scaledVector = ball.vector.getScaledVector(++counter);
                    moveTo = ball.position.getPositionPlusVector(scaledVector);
                    if ((this.playingDirection == DirectionType.RIGHT && (int)moveTo.x <= (int)goalCentre.x) || (this.playingDirection == DirectionType.LEFT && (int)moveTo.x >= (int)goalCentre.x))
                    {
                        found = true;
                        break;
                    }
                }
               
                if (found && moveTo.y > 20 && moveTo.y < 30)
                {

                    Vector targetVector = new Vector(moveTo, ballPosition);

                    Vector scaleDistance = targetVector.getScaledVector(1.000);
                    // We move to this position...
                    Position move = moveTo.getPositionPlusVector(scaleDistance);
                    move.x = this.playingDirection == DirectionType.RIGHT ? 1.000 : 99.000;


                    if(oldPosition.distanceFrom(move) > 0.5)
                    {
                        var action = js.move(goalkeeperPlayerNumber, move);
                        actions.Add(action);
                        oldPosition = move;
                    }
                    else
                    {
                        var action = moveOrTP(oldPosition);
                        actions.Add(action);
                    }

                }
                else
                {

                    getAction_Goalkeeper_DefaultPosition(actions);

                }
            }
            else
            {

                getAction_Goalkeeper_DefaultPosition(actions);

            }
        }

        /// <summary>
        /// The goalkeeper kicks the ball to a defender.
        /// </summary>
        private void getAction_Goalkeeper_HasBall(List<JSObject> actions)
        {
            var defaultPosition = getDefaultPosition(goalkeeperPlayerNumber, this.playingDirection);
            var playerPosition = new Position(goalkeeper.dynamicState.position);
            if (!playerPosition.IsEqual(defaultPosition.position))
            {
                actions.Add(moveOrTurn(goalkeeper, defaultPosition.position));
                return;
            }
            //clear the ball
            var clearPosition = new Position(this.pitch.centreSpot);
            var random = this.rnd.Next(20,40);
            clearPosition.y = random;

            //-------------------------------------------------------


            var action = js.kick(this.goalkeeperPlayerNumber, clearPosition);
            actions.Add(action);
      
        }

        /// <summary>
        /// The goalkeeper keeps between the ball and the goal.
        /// </summary>
        private void getAction_Goalkeeper_DefaultPosition(List<JSObject> actions)
        {
            //ball is coming towards goal need to defend
            var ballPosition = ball.position;

            // We move to a point between the ball and the goal, 5m away from 
            // the goal centre.

            // We find the vector between the goal-centre and the ball...
            var goalCentre = getGoalCentre(GoalType.OUR_GOAL);
            var vector = new Vector(goalCentre, ballPosition);

            // We move to this position...
            var scale = vector.getScaledVector(1.000);
            var moveTo = goalCentre.getPositionPlusVector(scale);

            moveTo.x = this.playingDirection == DirectionType.RIGHT ? 1.000 : 99.000;
            var action = moveOrTurn(goalkeeper, moveTo);
            actions.Add(action);
      
        }

        
        /// <summary>
        /// True if the ball is in our goal-area, false if not.
        /// </summary>
        private bool ballIsInGoalArea()
        {
            var ballPosition = ball.position;
            var goalCentre = getGoalCentre(GoalType.OUR_GOAL);
            return ballPosition.distanceFrom(goalCentre) < this.pitch.goalAreaRadius;
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

        /// <summary>
        /// Sets the kickoff position for (player, direction).
        /// </summary>
        private void setAttackPosition(int playerNumber, DirectionType playingDirection, Position position, double direction)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            var value = new PositionValue { position = position, direction = direction };
            this.attackPositions[key] = value;
        }

        /// <summary>
        /// Returns the default position for (player, direction).
        /// </summary>
        private PositionValue getAttackPosition(int playerNumber, DirectionType playingDirection)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            return this.attackPositions[key];
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
        private Dictionary<PositionKey, PositionValue> attackPositions = new Dictionary<PositionKey, PositionValue>();

        // For some random choices...
        private Random rnd = new Random();

        private JSWrapper js = new JSWrapper();

        #endregion
    }
}
