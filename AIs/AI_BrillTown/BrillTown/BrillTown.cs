using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Helpers;

namespace BrillTown
{
    /// <summary>
    /// A coding-world-cup API which:
    /// - Keeps three players in its own half as defenders.
    /// - Puts two players in the opponents half as forwards.
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
    class BrillTown : CodingWorldCupAPI
    {
        #region CodingWorldCupAPI implementation

        /// <summary>
        /// We request semi-random abilities for our players.
        /// </summary>
        protected override void processRequest_ConfigureAbilities(dynamic data)
        {
            int numberOfPlayers = this.teamPlayers.Count + 1; // +1 for the goalkeeper
            double averageKickingAbility = data.totalKickingAbility / (numberOfPlayers + 2);
            double averageRunningAbility = data.totalRunningAbility / numberOfPlayers;
            double averageBallControlAbility = data.totalBallControlAbility / (numberOfPlayers + 2);
            double averageTacklingAbility = data.totalTacklingAbility / numberOfPlayers;

            // We create the reply...
            var reply = new JSObject();
            reply.add("requestType", "CONFIGURE_ABILITIES");

            var playerInfos = new List<JSObject>();
            foreach (var playerNumber in this.allTeamPlayers.Keys)
            {
                var playerInfo = new JSObject();
                playerInfo.add("playerNumber", playerNumber);
                playerInfo.add("runningAbility", averageRunningAbility);
                playerInfo.add("tacklingAbility", averageTacklingAbility);

                if (playerNumber == this.playerNumber_CentreDefender || playerNumber == this.goalkeeperPlayerNumber)
                {
                    playerInfo.add("ballControlAbility", averageBallControlAbility * 2);
                }
                else
                {
                    playerInfo.add("ballControlAbility", averageBallControlAbility);
                }

                if (playerNumber == this.playerNumber_CentreDefender)
                {
                    playerInfo.add("kickingAbility", averageKickingAbility);
                }
                else if (playerNumber == this.playerNumber_LeftWingForward || playerNumber == playerNumber_RightWingForward)
                {
                    playerInfo.add("kickingAbility", averageKickingAbility * 2);
                }
                else
                {
                    playerInfo.add("kickingAbility", averageKickingAbility);
                }

                playerInfos.Add(playerInfo);
            }
            reply.add("players", playerInfos);

            sendReply(reply);
        }

        /// <summary>
        /// Called when team-info has been updated.
        /// 
        /// At this point we know the player-numbers for the players
        /// in our team, so we can choose which players are playing in
        /// which positions.
        /// </summary>
        protected override void onTeamInfoUpdated()
        {
            HorizontalCenterLower = this.pitch.centreSpot.x / 2;
            HorizontalCenterUpper = this.pitch.centreSpot.x * 3 / 2;

            // We assign players to the positions...
            var playerNumbers = new List<int>(this.teamPlayers.Keys);

            foreach (int playerNumber in this.allTeamPlayers.Keys)
            {
                this.selectedPlayer[playerNumber]    = -1;
                this.goalShootPosition[playerNumber] = new Position(-1, -1);
            }

            this.playerNumber_LeftWingDefender = playerNumbers[0];
            this.playerNumber_CentreDefender = playerNumbers[1];
            this.playerNumber_RightWingDefender = playerNumbers[2];
            this.playerNumber_LeftWingForward = playerNumbers[3];
            this.playerNumber_RightWingForward = playerNumbers[4];

            // We set up the default positions...
            setDefaultPosition(this.playerNumber_LeftWingDefender,  DirectionType.RIGHT, new Position(25, 10), 90);
            setDefaultPosition(this.playerNumber_CentreDefender,    DirectionType.RIGHT, new Position(45, 25), 90);
            setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(25, 40), 90);
            setDefaultPosition(this.playerNumber_LeftWingForward,   DirectionType.RIGHT, new Position(71, 15), 90);
            setDefaultPosition(this.playerNumber_RightWingForward,  DirectionType.RIGHT, new Position(71, 35), 90);

            setDefaultPosition(this.playerNumber_LeftWingDefender,  DirectionType.LEFT, new Position(75, 40), 270);
            setDefaultPosition(this.playerNumber_CentreDefender,    DirectionType.LEFT, new Position(55, 25), 270);
            setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(75, 10), 270);
            setDefaultPosition(this.playerNumber_LeftWingForward,   DirectionType.LEFT, new Position(29, 35), 270);
            setDefaultPosition(this.playerNumber_RightWingForward,  DirectionType.LEFT, new Position(29, 15), 270);

            // We set up the kickoff positions. 
            // Note: The player nearest the centre will be automatically "promoted"
            //       to the centre by the game.
            setKickoffPosition(this.playerNumber_LeftWingDefender,  DirectionType.RIGHT, new Position(25, 10), 90);
            setKickoffPosition(this.playerNumber_CentreDefender,    DirectionType.RIGHT, new Position(39, 25), 90);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(25, 40), 90);
            setKickoffPosition(this.playerNumber_LeftWingForward,   DirectionType.RIGHT, new Position(49, 14), 90);
            setKickoffPosition(this.playerNumber_RightWingForward,  DirectionType.RIGHT, new Position(49, 36), 90);

            setKickoffPosition(this.playerNumber_LeftWingDefender,  DirectionType.LEFT, new Position(75, 40), 270);
            setKickoffPosition(this.playerNumber_CentreDefender,    DirectionType.LEFT, new Position(61, 25), 270);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(75, 10), 270);
            setKickoffPosition(this.playerNumber_LeftWingForward,   DirectionType.LEFT, new Position(51, 36), 270);
            setKickoffPosition(this.playerNumber_RightWingForward,  DirectionType.LEFT, new Position(51, 14), 270);

            // We set up the attack positions...
            setAttackPosition(this.playerNumber_LeftWingDefender,  DirectionType.RIGHT, new Position(35, 10), 90);
            setAttackPosition(this.playerNumber_CentreDefender,    DirectionType.RIGHT, new Position(45, 25), 90);
            setAttackPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(35, 40), 90);
            setAttackPosition(this.playerNumber_LeftWingForward,   DirectionType.RIGHT, new Position(85, 10), 90);
            setAttackPosition(this.playerNumber_RightWingForward,  DirectionType.RIGHT, new Position(85, 40), 90);

            setAttackPosition(this.playerNumber_LeftWingDefender,  DirectionType.LEFT, new Position(65, 40), 270);
            setAttackPosition(this.playerNumber_CentreDefender,    DirectionType.LEFT, new Position(55, 25), 270);
            setAttackPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(65, 10), 270);
            setAttackPosition(this.playerNumber_LeftWingForward,   DirectionType.LEFT, new Position(15, 40), 270);
            setAttackPosition(this.playerNumber_RightWingForward,  DirectionType.LEFT, new Position(15, 10), 270);

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

            var ballPosition = new Position(this.ball.position);

            // predict ballposition
            var ballVec                 = new Position(this.ball.vector);
            int ballSpeed               = (int)this.ball.speed;
            int controllingPlayerNumber = (int)this.ball.controllingPlayerNumber;
            int gameTimeMillisec        = (int)(1000 * this.gameTimeSeconds);

            ballDirection = angleBetween(origin, ballVec);

            PositionVector ballPositionVector = getNextBallPosition();
            this.ballPositionNext = new Position(ballPositionVector.position.x, ballPositionVector.position.y);

            // Is the ball controlled by any player?
            if (this.allTeamPlayers.ContainsKey(this.ball.controllingPlayerNumber) || this.allOpposingTeamPlayers.ContainsKey(this.ball.controllingPlayerNumber))
            {
                this.selectedForward = -1;
            }

            if (this.ball.controllingPlayerNumber == -1)
            {
                getBallPositions();
            }
            else if (this.ball.controllingPlayerNumber != this.playerNumber_LeftWingForward && this.ball.controllingPlayerNumber != this.playerNumber_RightWingForward)
            {

                this.selectedPlayer[this.playerNumber_LeftWingForward]  = -1;
                this.selectedPlayer[this.playerNumber_RightWingForward] = -1; 
            }

            int[] playerIds = {this.playerNumber_LeftWingDefender,this.playerNumber_CentreDefender,this.playerNumber_RightWingDefender,
                               this.playerNumber_LeftWingForward, this.playerNumber_RightWingForward};
            foreach (int playerId in playerIds)
            {
                 Position playerPosition = new Position(this.allTeamPlayers[playerId].dynamicState.position);
                 this.playerBallDistance[playerId] = playerPosition.distanceFrom(ballPosition);
            }

             var actions = new List<JSObject>();
            // We add actions to the reply for each of our players...
            getAction_Defender(actions, this.playerNumber_LeftWingDefender);
            getAction_Defender(actions, this.playerNumber_CentreDefender);
            getAction_Defender(actions, this.playerNumber_RightWingDefender);
            getAction_Forward(actions,  this.playerNumber_LeftWingForward);
            getAction_Forward(actions,  this.playerNumber_RightWingForward);
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

        #endregion

        #region Private functions

        /// <summary>
        /// find closest opponent distance
        /// </summary>
        private double playerClosestOpponent(Position playerPosition)
        {
            var closestDistance = Double.MaxValue;
            foreach (var item in this.allOpposingTeamPlayers)
            {
                var opponent = item.Value;
                var opponentPosition = new Position(opponent.dynamicState.position);
                var distance = playerPosition.distanceFrom(opponentPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
            return closestDistance;
        }

        /// <summary>
        /// find closest forward 
        /// </summary>
        private int getClosestForward()
        {
            Position ballPosition  = new Position(this.ball.position);
            Vector   ballVector    = new Vector(ballPosition, this.ballPositionNext);
            Position leftPosition  = new Position(this.allTeamPlayers[this.playerNumber_LeftWingForward].dynamicState.position);
            Position rightPosition = new Position(this.allTeamPlayers[this.playerNumber_RightWingForward].dynamicState.position);

            double leftDistance  = ballVector.DistanceTo(ballPosition, leftPosition);
            double rightDistance = ballVector.DistanceTo(ballPosition, rightPosition);
            bool   right         = this.playerBallDistance[this.playerNumber_LeftWingForward] > this.playerBallDistance[this.playerNumber_RightWingForward];
            int    speed         = (int)this.ball.speed;
            if (Math.Abs(ballPosition.x - this.pitch.centreSpot.x) < 5)
            {
                if (( right && this.selectedForward == this.playerNumber_RightWingForward)
                 || (!right && this.selectedForward == this.playerNumber_LeftWingForward))
                {
                    // selected forward is closest so do nothing
                }
                else if (speed > 10 
                     && (Math.Min(this.playerBallDistance[this.playerNumber_LeftWingForward], this.playerBallDistance[this.playerNumber_RightWingForward]) > 20))
                {
                    right = leftDistance > rightDistance;
                }
            }

            if (right)
            {
                return this.playerNumber_RightWingForward;
            }
            return this.playerNumber_LeftWingForward;
        }


        /// <summary>
        /// find the closest opponent distance to the pass vector
        /// </summary>
        private double getClosestOpponentDistance(Position start, Position end)
        {
            var shotVector = new Vector(start, end);

            double minDist = Double.MaxValue;
            foreach (var item in this.allOpposingTeamPlayers)
            {
                var opponent = item.Value;
                var opponentPosition = new Position(opponent.dynamicState.position);
                if (contains(start, end, opponentPosition))
                {
                    double distance = shotVector.DistanceTo(start, opponentPosition);
                    if (minDist > distance)
                    {
                        minDist = distance;
                    }
                }
            }
            return minDist;
        }

        /// <summary>
        /// find the playerNum which is in space
        /// </summary>
        private int getSafestPlayer(int[] playerIds, Position start)
        {
            var playerSpace = Double.MinValue;
            int playerNum   = playerIds[0];

            foreach (int playerId in playerIds)
            {
                var potential = this.allTeamPlayers[playerId];
                var end = new Position(potential.dynamicState.position);
                var shotVector = new Vector(start, end);

                var minDist = Double.MaxValue;
                foreach (var item in this.allOpposingTeamPlayers)
                {
                    var opponent = item.Value;
                    var opponentPosition = new Position(opponent.dynamicState.position);
                    double distance = shotVector.DistanceTo(start, opponentPosition);
                    if (minDist > distance)
                    {
                        minDist = distance;
                    }
                }
                if (playerSpace < minDist)
                {
                    playerSpace = minDist;
                    playerNum = playerId;
                }
            }
            return playerNum;
        }

        /// <summary>
        /// find closest defender
        /// </summary>
        private int getClosestDefender()
        {
            int closestPlayer = this.playerNumber_LeftWingDefender;
            double closestDistance = this.playerBallDistance[closestPlayer];

            int [] playerIds = {this.playerNumber_CentreDefender,this.playerNumber_RightWingDefender};
            foreach (int playerId in playerIds)
            {
                if (closestDistance > this.playerBallDistance[playerId])
                {
                     closestPlayer   = playerId;
                     closestDistance = this.playerBallDistance[playerId];
                }
            }
            return closestPlayer;
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
            else if (player.staticState.playerNumber == this.playerNumber_CentreDefender)
            {
                         getAction_Centre(actions, player);
            }
            else if (((this.playingDirection == DirectionType.RIGHT && ballPosition.x < this.pitch.centreSpot.x)
                 ||   (this.playingDirection == DirectionType.LEFT  && ballPosition.x > this.pitch.centreSpot.x))
                 &&   (player.staticState.playerNumber != this.playerNumber_CentreDefender && getClosestDefender() == playerNumber) )
            {
                // The ball is in the defender's half, so he tries to get it...
                getAction_OwnHalf(actions, player, false);
            }
            else
            {
                // The ball is in the other half, so the player moves back 
                // to his default position...
                var defaultPosition = getDefaultPosition(playerNumber, this.playingDirection);
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", defaultPosition.position);
                actions.Add(action);
                Logger.log(String.Format("{0}: Defender default {1} Move {2}", this.gameTimeSeconds, playerNumber, defaultPosition.position), Logger.LogLevel.INFO);
            }
        }

        /// <summary>
        /// Kicks the ball from a defending player to a forward.
        /// </summary>
        private void getAction_Defender_HasBall(List<JSObject> actions, dynamic player)
        {
            int playerNumber     = player.staticState.playerNumber;
            Position   start     = new Position(player.dynamicState.position);
            double leftDistance  = getClosestOpponentDistance(start, new Position(this.allTeamPlayers[this.playerNumber_LeftWingForward].dynamicState.position));
            double rightDistance = getClosestOpponentDistance(start, new Position(this.allTeamPlayers[this.playerNumber_RightWingForward].dynamicState.position));

            // We choose a forward and find his position...
            int forwardPlayerNumber = -1;
            if (this.selectedPlayer[playerNumber] < 0)
            {
                bool useLeft         = this.rnd.NextDouble() < 0.5;
                forwardPlayerNumber  = (useLeft) ? this.playerNumber_LeftWingForward : this.playerNumber_RightWingForward;

                if (leftDistance > OPPONENT_CLOSEST && rightDistance < OPPONENT_CLOSEST)
                {
                    forwardPlayerNumber = this.playerNumber_LeftWingForward;
                }
                else if (leftDistance < OPPONENT_CLOSEST && rightDistance > OPPONENT_CLOSEST)
                {
                    forwardPlayerNumber = this.playerNumber_RightWingForward;
                }
                this.selectedPlayer[playerNumber] = forwardPlayerNumber;
            }
            else
            {
                forwardPlayerNumber = this.selectedPlayer[playerNumber];
            }

            var forward = this.allTeamPlayers[forwardPlayerNumber];
            var destination = new Position(forward.dynamicState.position);
            double direction = angleBetween(start, destination);
            double closestOpponent = playerClosestOpponent(start);
            double forwardDistance = start.distanceFrom(destination);

            if (closestOpponent > this.distanceTolerance)
            {
                if (closestOpponent > turnDistance && Math.Abs(direction - normalizeAngle((double)player.dynamicState.direction)) > this.defenderPassTolerance)
                {
                    // We turn towards him...
                    var action = new JSObject();
                    action.add("action", "TURN");
                    action.add("playerNumber", playerNumber);
                    action.add("direction", direction);
                    actions.Add(action);
                    Logger.log(String.Format("{0}: Defender {1} turn the ball to {2}", this.gameTimeSeconds, playerNumber, direction), Logger.LogLevel.INFO);
                    return;
                }
                else if (forwardDistance > passDistance)
                {
                    // We move towards the goal...
                    var action = new JSObject();
                    action.add("playerNumber", playerNumber);
                    action.add("action", "MOVE");
                    action.add("destination", destination);
                    actions.Add(action);
                    Logger.log(String.Format("{0}: Defender {1} far move the ball to {2}", this.gameTimeSeconds, playerNumber, direction), Logger.LogLevel.INFO);
                    return;
                }
            }

            if (leftDistance < OPPONENT_CLOSEST && rightDistance < OPPONENT_CLOSEST)
            {
                // we are surrounded so shoot at goal
                destination = getGoalCentre(GoalType.THEIR_GOAL);
            }

            {
                // We kick the ball towards him...
                this.selectedPlayer[playerNumber] = -1;
                this.selectedForward = forwardPlayerNumber;

                var action = new JSObject();
                action.add("action", "KICK");
                action.add("playerNumber", playerNumber);
                action.add("destination", destination);
                action.add("speed", 100.0);
                actions.Add(action);
                Logger.log(String.Format("{0}: Defender {1} kicks the ball to {2}", this.gameTimeSeconds, playerNumber, destination), Logger.LogLevel.INFO);
            }
        }

        /// <summary>
        /// The player tries to get the ball.
        /// </summary>
        private void getAction_OwnHalf(List<JSObject> actions, dynamic player, bool intersect)
        {
            // Is the ball already controlled by someone in our own team?
            if (this.allTeamPlayers.ContainsKey(this.ball.controllingPlayerNumber))
            {
                // The ball is already controlled by this team...
                return;
            }

            // The ball is not controlled by us, so we try to take possession...
            int playerNumber = (int)player.staticState.playerNumber;

            // If we are less than 5m from the ball, we try to take possession.
            // If we are further away, we move towards it.
            Position playerPosition = new Position(player.dynamicState.position);
            Position ballPosition   = new Position(this.ball.position);
            double distance         = playerPosition.distanceFrom(ballPosition);
            Position ballPredicted  = getClosestBallPosition(player, 0.0);
            double speed            = (double)this.ball.speed;

            if (intersect)
            {
                Position destination = getPlayeBallIntersection(player, getGoalCentre(GoalType.THEIR_GOAL));
                ballPredicted = destination;
            }

            double possessionDistance = Math.Max(speed * EVENT_INTERVAL_SECONDS, 0.5);
            if (distance < possessionDistance)
            {
                // We attempt to take possession of the ball...
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "TAKE_POSSESSION");
                actions.Add(action);
                Logger.log(String.Format("{0}: ownhalf player {1} Take_possession", this.gameTimeSeconds, playerNumber), Logger.LogLevel.INFO);
            }
            else
            {
                // We move towards the ball...
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", ballPredicted);
                actions.Add(action);
                Logger.log(String.Format("{0}: ownhalf player {1} move predicted {2}", this.gameTimeSeconds, playerNumber, ballPredicted), Logger.LogLevel.INFO);
            }
        }

        /// <summary>
        /// The player tries to get the ball.
        /// </summary>
        private void getAction_Centre(List<JSObject> actions, dynamic player)
        {
            // The ball may be controlled by us, so we try to take possession and move forward...
            int playerNumber      = player.staticState.playerNumber;
            Position ballPosition = new Position(this.ball.position);
            double speed          = (double)this.ball.speed;

            // If we are less than 5m from the ball, we try to take possession.
            // If we are further away, we move towards it.
            double distance           = this.playerBallDistance[playerNumber];
            double possessionDistance = Math.Max(speed * EVENT_INTERVAL_SECONDS, 0.5);
            if (distance < possessionDistance)
            {
                // We attempt to take possession of the ball...
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "TAKE_POSSESSION");
                actions.Add(action);
                Logger.log(String.Format("{0}: central defender {1} Take_possession {2}", this.gameTimeSeconds, playerNumber, distance), Logger.LogLevel.INFO);
                return;
            }

            if (ballPosition.x > HorizontalCenterLower && ballPosition.x < HorizontalCenterUpper)
            {
                // We move towards the ball...
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", ballPosition);
                actions.Add(action);
                Logger.log(String.Format("{0}: central defender {1} move {2}", this.gameTimeSeconds, playerNumber, ballPosition), Logger.LogLevel.INFO);
            }
            else
            {
                // We move towards the ball's y ...
                double horizontal = (double)player.dynamicState.position.x;
                if (ballPosition.x > HorizontalCenterUpper)
                {
                    horizontal = HorizontalCenterUpper;
                }
                if (ballPosition.x < HorizontalCenterLower)
                {
                    horizontal = HorizontalCenterLower;
                }

                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", new Position(horizontal, (double)ballPosition.y));
                actions.Add(action);
                Logger.log(String.Format("{0}: central defender {1} move horizontal", this.gameTimeSeconds, playerNumber), Logger.LogLevel.INFO);
            }
        }

        /// <summary>
        /// Gets the current action for one of the forwards.
        /// </summary>
        private void getAction_Forward(List<JSObject> actions, int playerNumber)
        {
            var player       = this.allTeamPlayers[playerNumber];
            var ballPosition = this.ball.position;
            int speed        = (int)this.ball.speed;

            bool ballInOppenentHalf = (  (this.playingDirection == DirectionType.RIGHT && ballPosition.x > this.pitch.centreSpot.x)
                                       ||(this.playingDirection == DirectionType.LEFT  && ballPosition.x < this.pitch.centreSpot.x));

            if (player.dynamicState.hasBall)
            {
                // The player has the ball, so we kick it at the goal...
                getAction_Forward_HasBall(actions, player);
            }
            else if (ballInOppenentHalf && 
                (this.selectedPlayer[playerNumber] == this.playerNumber_LeftWingForward || this.selectedPlayer[playerNumber] == this.playerNumber_RightWingForward))
            {
                // we are being passed the ball from other forward, check if ball stopped
                getAction_OwnHalf(actions, player, speed > 10);
            }
            else if (ballInOppenentHalf && getClosestForward() == playerNumber)
            {
                // The ball is in the forward's half, so he tries to get it...
                getAction_OwnHalf(actions, player, false);
            }
            else if (ballInOppenentHalf)
            {
                // The other forward has ball so move to attack position
                var attackPosition = getAttackPosition(playerNumber, this.playingDirection);
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", attackPosition.position);
                actions.Add(action);
                Logger.log(String.Format("{0}: Forward {1} moves to attack {2}", this.gameTimeSeconds, playerNumber, attackPosition.position), Logger.LogLevel.INFO);
            }
            else
            {
                // The ball is in the other half, so the player moves back 
                // to his default position...
                var defaultPosition = getDefaultPosition(playerNumber, this.playingDirection);
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", defaultPosition.position);
                actions.Add(action);
                Logger.log(String.Format("{0}: Forward {1} moves to default {2}", this.gameTimeSeconds, playerNumber, defaultPosition.position), Logger.LogLevel.INFO);
            }
        }

        /// <summary>
        /// Gets the goal position the forward aims at offset from goAl center .
        /// </summary>
        private Position get_Forward_ShootPosition(dynamic player)
        {
            // We find which part of the goal to aim at...
            int playerNumber = (int)player.staticState.playerNumber;

            Position start = new Position(player.dynamicState.position);
            Position destination = getGoalCentre(GoalType.THEIR_GOAL);
            double goalCentredirection = angleBetween(start, destination);
            double goalOffset = GOAL_WIDTH / 2.0;

            if (this.goalShootPosition[playerNumber].x < 0)
            {
                if (goalCentredirection < (90.0 - GOAL_ANGLE_DELTA) || (270.0 + GOAL_ANGLE_DELTA) < goalCentredirection)
                {
                    goalOffset = GOAL_WIDTH - GOAL_OFFSET; ;
                }
                else if (((90.0 - GOAL_ANGLE_DELTA) < goalCentredirection && goalCentredirection < (90.0 + GOAL_ANGLE_DELTA))
                     || ((270.0 - GOAL_ANGLE_DELTA) < goalCentredirection && goalCentredirection < (270.0 + GOAL_ANGLE_DELTA)))
                {
                    goalOffset = (this.rnd.NextDouble() < 0.5) ? GOAL_OFFSET : GOAL_WIDTH - GOAL_OFFSET;
                }
                else if ((90.0 + GOAL_ANGLE_DELTA) < goalCentredirection && goalCentredirection < (270.0 - GOAL_ANGLE_DELTA))
                {
                    goalOffset = GOAL_OFFSET;
                }
                else
                {
                    int should_not_be_possible = 0;
                }
                this.goalShootPosition[playerNumber].x = destination.x;
                this.goalShootPosition[playerNumber].y = destination.y + goalOffset - (GOAL_WIDTH / 2.0);
            }

            destination.y = this.goalShootPosition[playerNumber].y;
            return destination;
        }

        /// <summary>
        /// The forward shoots at the goal.
        /// (me, other)
        /// (-1,-1)     => shoot at goal
        /// (me, other) => prepare to pass to other
        /// (-1, other) => pass to other
        /// (me, -1)    => prepare to shot at goal
        /// </summary>
        private void getAction_Forward_HasBall(List<JSObject> actions, dynamic player)
        {
            int playerNumber           = player.staticState.playerNumber;
            Position start             = new Position(player.dynamicState.position);
            Position goalCentre        = getGoalCentre(GoalType.THEIR_GOAL);
            Position destination       = getGoalCentre(GoalType.THEIR_GOAL);

            int otherForward = (this.playerNumber_LeftWingForward == playerNumber) ?  this.playerNumber_RightWingForward : this.playerNumber_LeftWingForward;
            var otherPlayer = this.allTeamPlayers[otherForward];

            Position otherStart   = new Position(otherPlayer.dynamicState.position);
            double playerDistance = start.distanceFrom(destination);
            double otherDistance  = otherStart.distanceFrom(destination);
            double otherClosestDistance = getClosestOpponentDistance(start, new Position(this.allTeamPlayers[otherForward].dynamicState.position));

            // if we are closest we shoot at goal or otherForward surrounded by opponents
            if (playerDistance < otherDistance || otherClosestDistance < OPPONENT_CLOSEST)
            {
                this.selectedPlayer[this.playerNumber_LeftWingForward]  = -1;
                this.selectedPlayer[this.playerNumber_RightWingForward] = -1;
            } 
            else if (this.selectedPlayer[this.playerNumber_LeftWingForward] == -1 && this.selectedPlayer[this.playerNumber_RightWingForward] == -1)
            {
                this.selectedPlayer[this.playerNumber_LeftWingForward]  = otherForward;
                this.selectedPlayer[this.playerNumber_RightWingForward] = otherForward;
            }

            if (this.selectedPlayer[playerNumber] >= 0 && this.selectedPlayer[playerNumber] != playerNumber)
            {
                var forward                 = this.allTeamPlayers[this.selectedPlayer[playerNumber]];
                Position forwardDestination = get_Forward_ShootPosition(forward);
                Position forwardPosition    = new Position(forward.dynamicState.position);
                Vector forwardVector        = new Vector(forwardPosition, forwardDestination);
                Vector passVector           = forwardVector.getScaledVector(1);
                Vector forwardGoal          = new Vector(forwardPosition, getGoalCentre(GoalType.THEIR_GOAL));
                if (forwardGoal.Length > 6)
                {
                    destination = forwardPosition.getPositionPlusVector(passVector);
                }
                else
                {
                    destination = forwardPosition;
                }
            }
            else
            {
                destination = get_Forward_ShootPosition(player);
            }

            double closestOpponent = playerClosestOpponent(start);
            double playerDirection = (double)player.dynamicState.direction;
            double direction       = angleBetween(start, destination);
            bool   goalShootingPosition = inGoalShootingPosition(start);

            if (!goalShootingPosition && closestOpponent > turnDistance)
            {
                destination = getGoalShootingPosition();
                // We move to optimum goal shooting position...
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", destination);
                actions.Add(action);
                Logger.log(String.Format("{0}: Forward nonshoot {1} moves the ball to {2}", this.gameTimeSeconds, playerNumber, direction), Logger.LogLevel.INFO);
                return;
            }

            if (closestOpponent > distanceTolerance && Math.Abs(direction - normalizeAngle(playerDirection)) > this.forwardPassTolerance)
            {
                // We turn towards goal...
                var action = new JSObject();
                action.add("action", "TURN");
                action.add("playerNumber", playerNumber);
                action.add("direction", direction);
                actions.Add(action);
                Logger.log(String.Format("{0}: Forward {1} turn the ball to {2}", this.gameTimeSeconds, playerNumber, direction), Logger.LogLevel.INFO);
                return;
            }

            double playerMoves         = PLAYER_MAX_SPEED * player.staticState.runningAbility * SPEED_WITH_BALL_FACTOR * EVENT_INTERVAL_SECONDS;
            double distance            = start.distanceFrom(destination);
            Vector moveVector          = new Vector(start, destination);
            Vector moveScaledVector    = moveVector.getScaledVector(playerMoves);
            Position movePosition      = start.getPositionPlusVector(moveScaledVector);
            double closestOpponentMove = playerClosestOpponent(movePosition);

            if (closestOpponent > distanceTolerance && distance > kickDistance)
            {
                // We move towards the target...
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", destination);
                actions.Add(action);
                Logger.log(String.Format("{0}: Forward {1} moves far the ball to {2}", this.gameTimeSeconds, playerNumber, destination), Logger.LogLevel.INFO);
                return;
            }

            {
                // we pass to the other forward
                this.selectedPlayer[playerNumber] = -1;

                // We kick the ball at the target...
                this.goalShootPosition[playerNumber].x = -1;
                this.goalShootPosition[playerNumber].y = -1;
                var action = new JSObject();
                action.add("action", "KICK");
                action.add("playerNumber", playerNumber);
                action.add("destination", destination);
                action.add("speed", 100.0);
                actions.Add(action);
                Logger.log(String.Format("{0}: Forward {1} shoots to {2}", this.gameTimeSeconds, playerNumber, destination), Logger.LogLevel.INFO);
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

        /// <summary>
        /// The goalkeeper kicks the ball to a defender.
        /// </summary>
        private void getAction_Goalkeeper_HasBall(List<JSObject> actions)
        {
            goalkeeper = this.allTeamPlayers[this.goalkeeperPlayerNumber];
            var start = new Position(goalkeeper.dynamicState.position);

            // We choose a defender and find his position...
            int defenderNumber = -1;
            if (this.selectedPlayer[this.goalkeeperPlayerNumber] < 0)
            {

                int[] playerIds = { this.playerNumber_LeftWingDefender, this.playerNumber_CentreDefender, this.playerNumber_RightWingDefender };
                defenderNumber  = getSafestPlayer(playerIds, start);
                this.selectedPlayer[this.goalkeeperPlayerNumber] = defenderNumber;
            }
            else
            {
                defenderNumber = this.selectedPlayer[this.goalkeeperPlayerNumber];
            }

            var defender = this.allTeamPlayers[defenderNumber];
            var defenderPosition = new Position(defender.dynamicState.position);
            var destination = new Position(defender.dynamicState.position);

            double goalkeeperDirection = normalizeAngle((double)goalkeeper.dynamicState.direction);
            double direction  = angleBetween(start, destination);
            double delta      = Math.Abs(direction - goalkeeperDirection);
            double direction2 = angleBetween2(start, destination);
            double delta2     = Math.Abs(direction2 - goalkeeperDirection);

            if (Math.Abs(direction - goalkeeperDirection) > goalkeeperPassTolerance)
            {
                // We kick the ball towards him...
                var action = new JSObject();
                action.add("action", "TURN");
                action.add("playerNumber", this.goalkeeperPlayerNumber);
                action.add("direction", direction);
                actions.Add(action);
                Logger.log(String.Format("{0}: Goalkeeper turn the ball to {1}:{2} {3}/{4}", this.gameTimeSeconds, direction, direction2, delta, delta2), Logger.LogLevel.INFO);
            }
            else
            {
                this.selectedPlayer[this.goalkeeperPlayerNumber] = -1;

                // We kick the ball to the defender...
                var action = new JSObject();
                action.add("action", "KICK");
                action.add("playerNumber", this.goalkeeperPlayerNumber);
                action.add("destination", defenderPosition);
                action.add("speed", 100.0);
                actions.Add(action);
                Logger.log(String.Format("{0}: Goalkeeper kicks the ball to {1}", this.gameTimeSeconds, defenderPosition), Logger.LogLevel.INFO);
            }
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

            double distance           = goalkeeperPosition.distanceFrom(ballPosition);
            double speed              = (double)this.ball.speed;
            double possessionDistance = Math.Max(speed * EVENT_INTERVAL_SECONDS, 0.5);
            if (distance < possessionDistance)
            {
                // We are close to the ball, so we try to take possession...
                var action = new JSObject();
                action.add("action", "TAKE_POSSESSION");
                action.add("playerNumber", this.goalkeeperPlayerNumber);
                actions.Add(action);
                Logger.log(String.Format("{0}: Goalkeeper area Take_possession {1}", this.gameTimeSeconds, distance), Logger.LogLevel.INFO);
            }
            else
            {
                // We are too far away, so we move towards the ball...
                var action = new JSObject();
                Position ballPredicted  = getClosestBallPosition(this.goalkeeper, 0.0);
                if (speed > 7 && predictGoalPosition())
                {
                    ballPredicted = this.predictedGoalkeeperPosition;
                }
                action.add("playerNumber", this.goalkeeperPlayerNumber);
                action.add("action", "MOVE");
                action.add("destination", ballPredicted);
                actions.Add(action);
                Logger.log(String.Format("{0}: Goalkeeper area Move {1}", this.gameTimeSeconds, ballPredicted), Logger.LogLevel.INFO);
            }
        }

        /// <summary>
        /// The goalkeeper keeps between the ball and the goal.
        /// </summary>
        private void getAction_Goalkeeper_BallOutsideGoalArea(List<JSObject> actions)
        {
            // We move to a point between the ball and the goal, 5m away from 
            // the goal centre.

            // We find the vector between the goal-centre and the ball...
            var goalCentre = getGoalCentre(GoalType.OUR_GOAL);
            var ballPosition = new Position(this.ball.position);
            var vector = new Vector(goalCentre, ballPosition);

            // We find the position 5m from the goal-centre...
            var vector5m = vector.getScaledVector(5.0);
            var moveTo = goalCentre.getPositionPlusVector(vector5m);

            // We move to this position...
            var action = new JSObject();
            action.add("playerNumber", this.goalkeeperPlayerNumber);
            action.add("action", "MOVE");
            action.add("destination", moveTo);
            actions.Add(action);
            Logger.log(String.Format("{0}: Goalkeeper outside Move {1}", this.gameTimeSeconds, moveTo), Logger.LogLevel.INFO);
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

        /// <summary>
        /// Sets the default position for (player, direction).
        /// </summary>
        private void setDefaultPosition(int playerNumber, DirectionType playingDirection, Position position, double direction)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            var value = new PositionValue{position=position, direction=direction};
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
            var value = new PositionValue{position=position, direction=direction};
            this.kickoffPositions[key] = value;
        }

        /// <summary>
        /// Returns the kickoff position for (player, direction).
        /// </summary>
        private PositionValue getKickoffPosition(int playerNumber, DirectionType playingDirection)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            return this.kickoffPositions[key];
        }

        /// <summary>
        /// Sets the attack position for (player, direction).
        /// </summary>
        private void setAttackPosition(int playerNumber, DirectionType playingDirection, Position position, double direction)
        {
            var key   = new PositionKey(playerNumber, playingDirection);
            var value = new PositionValue{position=position, direction=direction};
            this.attackPositions[key] = value;
        }

        /// <summary>
        /// Returns the attack position for (player, direction).
        /// </summary>
        private PositionValue getAttackPosition(int playerNumber, DirectionType playingDirection)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            return this.attackPositions[key];
        }

        /// <summary>
        /// check if the angle to shoot at goal is reasonable
        /// </summary>
        protected bool inGoalShootingPosition(Position position)
        {
            if (this.playingDirection == DirectionType.RIGHT)
            {
                return position.x < this.pitch.width - minimumForwardGoalX;
            }
            else
            {
                return position.x > minimumForwardGoalX;
            }
        }

        /// <summary>
        /// Returns a Position for the optimum position outside opponents goal for forwards.
        /// </summary>
        protected Position getGoalShootingPosition()
        {
            double x = forwardGoalX;
            if (this.playingDirection == DirectionType.RIGHT)
            {
                x = this.pitch.width - forwardGoalX;
            }
            double y = this.pitch.goalCentre;
            return new Position(x, y);
        }

        /// <summary>
        /// Returns true if goal is predicted and set member variable to Position where ball enter the goal.
        /// </summary>
        protected bool predictGoalPosition()
        {
            var ballPosition = new Position(this.ball.position);
            var ballVector   = new Position(this.ball.vector);
            int ballSpeed    = (int)this.ball.speed;
            int controllingPlayerNumber = (int)this.ball.controllingPlayerNumber;

            if (Math.Abs(ballVector.x) < 0.000001 || ballSpeed < 7) {
                 this.predictedGoalPosition       = getGoalCentre(GoalType.OUR_GOAL);;
                 this.predictedGoalkeeperPosition = getGoalCentre(GoalType.OUR_GOAL);;
                 return false;
            }

            Position goalCentre = getGoalCentre(GoalType.OUR_GOAL);

            double grad = (goalCentre.x - ballPosition.x) / ballVector.x;
            double goalY = ballPosition.y + grad * ballVector.y;
            this.predictedGoalPosition = new Position(goalCentre.x, goalY);
            if ((goalCentre.y - GOAL_WIDTH / 2.0) < goalY && goalY < (goalCentre.y + GOAL_WIDTH / 2.0))
            {
                //calculate the goalkeeper position 5 meters from goal
                double delta   = (this.playingDirection == DirectionType.LEFT) ? -5.0 : 5.0;
                double grad2   = grad - (delta / ballVector.x);
                double keeperY = ballPosition.y + grad2 * ballVector.y;
                Position predictedGoalkeeperPosition2 = new Position(goalCentre.x + delta, keeperY);

                // calculate intersection with 5 meter radius working outwards from goal position
                double dx = -ballVector.x / 100.0;
                double dy = -ballVector.y / 100.0;
                double rx = this.predictedGoalPosition.x - goalCentre.x;
                double ry = this.predictedGoalPosition.y - goalCentre.y;

                while ((rx * rx + ry * ry) < 25.0)
                {
                    rx += dx;
                    ry += dy;
                }
                this.predictedGoalkeeperPosition = new Position(goalCentre.x + rx, goalCentre.y + ry);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Returns the angle in the range 0 < angle < 360.
        /// </summary>
        private double normalizeAngle(double angle)
        {
            angle = (angle + 360.0) % 360.0;

            return angle;
        }

        ///
        /// angleBetween
        ///------------
        /// Returns the angle of the line from p1 to p2 in degrees,
        /// measured clockwise from a vertical line. p1 and p2 should
        /// be Position objects.
        ///
        /// In other words, the angle is the direction a player at p1
        /// should face to be pointing towards p2.
        ////
        private double angleBetween(Position p1, Position p2) 
        {
            var deltaY = p2.y - p1.y;
            var deltaX = p2.x - p1.x;
            var angle = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;
            angle += 450;
            angle %= 360;
            return normalizeAngle(angle);
        }

        private double angleBetween2(Position p1, Position p2)
        {
            var deltaY = p1.y - p2.y;
            var deltaX = p1.x - p2.x;
            var angle = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;
            angle += 270;
            angle %= 360;
            return angle;
        }

        /// <summary>
        /// Returns true if the bounding box contains the passed in position.
        /// </summary>
        public bool contains(Position start, Position end, Position point)
        {
            bool bounded = ((Math.Min(start.x, end.x) - 0.5) <= point.x) && (point.x <= (Math.Max(start.x, end.x) + 0.5))
                        && ((Math.Min(start.y, end.y) - 0.5) <= point.y) && (point.y <= (Math.Max(start.y, end.y) + 0.5));
            return bounded;
        }

        private double getNextBallDistance(Position playerPosition)
        {
            var ballPosition = new Position(this.ball.position);
            var ballVector   = new Vector(ballPosition, this.ballPositionNext);
            double distance  = ballVector.DistanceTo(ballPosition, playerPosition);
            return distance;
        }

        ///
        /// Returns the vector for speed
        /// direction measured clockwise from a vertical line. 
        ////
        private Vector nextVector(double direction, double speed)
        {
            double distance = speed * EVENT_INTERVAL_SECONDS;
            double deltaX   = distance * Math.Sin(direction * Math.PI /180.0);
            double deltaY   = distance * Math.Cos(direction * Math.PI / 180.0);
            return new Vector(deltaX, deltaY);
        }

        /// <summary>
        /// get the ball position and velocity for next event.
        /// </summary>
        private PositionVector getNextBallPosition()
        {
            double intervalSeconds = EVENT_INTERVAL_SECONDS;
            double friction = BALL_FRICTION;

            Position position = new Position(this.ball.position);
            Vector vector = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
            double speed = (double)this.ball.speed;
            double direction = 0.0;

            int controllingPlayerNumber = (int)this.ball.controllingPlayerNumber;
            if (controllingPlayerNumber != -1 && this.allOpposingTeamPlayers.ContainsKey(controllingPlayerNumber))
            {
                var opponent = this.allOpposingTeamPlayers[controllingPlayerNumber];
                speed        = BALL_MAX_SPEED;  // assume ball is about to be kicked
                position     = new Position(opponent.dynamicState.position);
                direction    = normalizeAngle((double)opponent.dynamicState.direction);
                vector       = nextVector(direction, speed);
            }

            PositionVector PositionVector = new PositionVector(position, vector, speed);
            PositionVector PositionVector2 = updateBallPosition(PositionVector, intervalSeconds, friction);

            return PositionVector2;
        }

        /// <summary>
        /// Get the intersection of ball and player
        ///   d         b+tv
        ///     \       /
        ///       \   /
        ///         X
        ///       /   \
        ///     /       \
        ///   b           p
        /// </summary>
        private Position getPlayeBallIntersection(dynamic player, Position destination)
        {
            Position playerPosition = new Position(player.dynamicState.position);
            Position ballPosition = new Position(this.ball.position);
            Vector   ballVector   = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
            Position ballDestination = ballPosition.getPositionPlusVector(ballVector);
 
            double   denominator = (destination.x - playerPosition.x) * (ballPosition.y - ballDestination.y)
                                 - (destination.y - playerPosition.y) * (ballPosition.x - ballDestination.x);

            double x1y2my1x2 = (destination.x  * playerPosition.y  - destination.y  * playerPosition.x);
            double x3y4my3x4 = (ballPosition.x * ballDestination.y - ballPosition.y * ballDestination.x);

            if (Math.Abs(denominator)  < 0.0000001)
            {
                // lines parallel;
                return getClosestBallPosition(player, 0.0);
            }

            double x = (x1y2my1x2 * (ballPosition.x - ballDestination.x) - (destination.x - playerPosition.x) * x3y4my3x4) / denominator;
            double y = (x1y2my1x2 * (ballPosition.y - ballDestination.y) - (destination.y - playerPosition.y) * x3y4my3x4) / denominator;

            return new Position(x, y);
        }

        /// <summary>
        /// get the ball position at it's closest point.
        /// The routine has a max of 5 seconds look ahead
        /// </summary>
        private Position getClosestBallPosition(dynamic player, double range)
        {
            double intervalSeconds = CALCULATION_INTERVAL_SECONDS;
            int maxIntervals = (int)(10.0 / intervalSeconds);
            int interval = 0;
            double friction = BALL_FRICTION;
            double playerSpeed       = PLAYER_MAX_SPEED * player.staticState.runningAbility;
            Position playerPosition  = new Position(player.dynamicState.position);
            double   playerDirection = (double)player.dynamicState.direction;
            int playerNumber         = (int)player.staticState.playerNumber;

            Position position = new Position(this.ball.position);
            Vector vector = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
            double speed = (double)this.ball.speed;
            double direction = 0.0;
            double distance  = 0.0;

            int controllingPlayerNumber = (int)this.ball.controllingPlayerNumber;
            if (player.staticState.playerNumber == this.goalkeeperPlayerNumber)
            {
                //we are the goalkeeper
                int i = 0;
            }
            if (controllingPlayerNumber != -1 && this.allOpposingTeamPlayers.ContainsKey(controllingPlayerNumber))
            {
                var opponent = this.allOpposingTeamPlayers[controllingPlayerNumber];
                speed = BALL_MAX_SPEED;  // assume ball is about to be kicked
                position = new Position(opponent.dynamicState.position);
                direction = normalizeAngle((double)opponent.dynamicState.direction);
                vector = nextVector(direction, speed);
            }

            double turnAngle    = 0.0;
            double turnInterval = 0.0;
            double moveInterval = 0.0;
            PositionVector PositionVector = new PositionVector(position, vector, speed);
            PositionVector PositionVectorInterval = updateBallPosition(PositionVector, intervalSeconds, friction);
            Position positionRet = new Position(PositionVectorInterval.position);
            double closest = playerPosition.distanceFrom(positionRet);
            while (interval < maxIntervals)
            {
                ++interval;
                PositionVectorInterval = updateBallPosition(PositionVectorInterval, intervalSeconds, friction);

                turnAngle    = angleBetween(playerPosition, PositionVectorInterval.position);
                turnInterval = Math.Abs(turnAngle - normalizeAngle(playerDirection)) / (PLAYER_MAX_TURNING_RATE * CALCULATION_INTERVAL_SECONDS);
                moveInterval = (interval > turnInterval) ? interval - turnInterval : interval;

                distance = Math.Abs(playerPosition.distanceFrom(PositionVectorInterval.position) - moveInterval * intervalSeconds * playerSpeed);
                if (closest > distance)
                {
                    closest = distance;
                    positionRet = PositionVectorInterval.position;
                }
                if (distance < range) {
                    return PositionVectorInterval.position;
                }
            }

            return positionRet;
        }

        /// <summary>
        /// This must be called when the ball is not controlled by a player
        /// i.e this.ball.controllingPlayerNumber == -1
        /// get the ball position and velocity till ball stops.
        /// </summary>
        private void getBallPositions()
        {
            double intervalSeconds = CALCULATION_INTERVAL_SECONDS;

            Position position      = new Position(this.ball.position);
            Vector   vector        = new Vector((double)this.ball.vector.x, (double)this.ball.vector.y);
            double   speed         = (double)this.ball.speed;
            int gameTimeMillisec   = (int)(1000 * this.gameTimeSeconds);

            PositionVector PositionVector = new PositionVector(position, vector, speed);
            ballPositions[gameTimeMillisec] = PositionVector; 

            while (PositionVector.speed > 0.0)
            {
                PositionVector = updateBallPosition(PositionVector, intervalSeconds, BALL_FRICTION);
                gameTimeMillisec += 1;
                ballPositions[gameTimeMillisec] = PositionVector; 
            }
        }

        /// <summary>
        /// This must be called when the ball is not controlled by a player
        /// i.e this.ball.controllingPlayerNumber == -1
        /// Returns the ball position and velocity for the next time interval.
        /// </summary>
        private PositionVector updateBallPosition(PositionVector PositionVector, double intervalSeconds, double friction)
        {
            // The ball is travelling under its own steam.
            //
            // The ball slows down as it moves. So we find its speed at
            // the start and at the end of the interval we are calculating
            // and move at the average speed...
            double speedAtStart    = PositionVector.speed;
            double speedAtEnd      = speedAtStart - friction * intervalSeconds;

            if (speedAtEnd < 0.0)
            {
                // The ball has come to a stop during this interval.
                // I suppose that this really means that we should adjust
                // the interval as well, as the ball has stopped before
                // the end of it. But maybe the ball just rolls a bit at
                // the end :-)
                speedAtEnd = 0.0;
            }

            double averageSpeed = (speedAtStart + speedAtEnd) / 2.0;

            // We hold the initial position to help check whether a goal
            // has been scored...
            var initialPosition = new Position(PositionVector.position.x, PositionVector.position.y);

            // We find the distance travelled, and move the ball...
            double distance = averageSpeed * intervalSeconds;
            Vector vector = new Vector(PositionVector.vector.x, PositionVector.vector.y);
            Vector vectorMoved = vector.getScaledVector(distance);
            Position position = initialPosition.getPositionPlusVector(vectorMoved);

            //calculate distance based in CALCULATION_INTERVAL_SECONDS
            int subInterval = (int) (intervalSeconds / CALCULATION_INTERVAL_SECONDS);
            double subDistance = 0.0;
            double subStart = PositionVector.speed;
            double subEnd   = 0.0;
            for (int idx = 0; idx < subInterval; ++idx)
            {
                subEnd = subStart - friction * CALCULATION_INTERVAL_SECONDS;
                if (subEnd < 0.0)
                {
                    subEnd = 0.0;
                }
                subDistance += CALCULATION_INTERVAL_SECONDS * (subStart + subEnd) / 2.0;
                subStart = subEnd;
            }

            // Did the ball bounce?
            if (position.x < 0.0)
            {
                //game.checkForGoal(initialPosition, position, 0.0);
                position.x *= -1.0;
                vector.x   *= -1.0;
            }
            if (position.x > pitch.width)
            {
                //game.checkForGoal(initialPosition, position, pitch.width);
                position.x = pitch.width - (position.x - pitch.width);
                vector.x  *= -1.0;
            }

            if (position.y < 0.0)
            {
                position.y *= -1.0;
                vector.y   *= -1.0;
            }

            if (position.y > pitch.height)
            {
                position.y = pitch.height - (position.y - pitch.height);
                vector.y  *= -1.0;
            }

            // We change the speed of the ball...
            PositionVector PositionVector2 = new PositionVector(position, vector, speedAtEnd);
            return PositionVector2;
        }

        #endregion

        #region Private data

        private const double CALCULATION_INTERVAL_SECONDS = 0.01;
        private const double EVENT_INTERVAL_SECONDS       = 0.1;  // other team consumes one event cycle
        private const double BALL_MAX_SPEED               = 30.0;
        private const double BALL_FRICTION                = 10.0;
        private const double PLAYER_MAX_SPEED             = 10.0 / 100.0;
        private const double PLAYER_MAX_TURNING_RATE      = 600.0; // degrees per second
        private const double SPEED_WITH_BALL_FACTOR       = 0.4;
        private const double GOAL_WIDTH                   = 8.0;
        private const double GOAL_OFFSET                  = 2.0; // where to shoot in the goal from posts
        private const double GOAL_ANGLE_DELTA             = 2.5; // angle above/below horizontal, used to select goal target 
        private const double OPPONENT_CLOSEST             = 1.0; // closest distance in meters from pass vector of opponents 

        //Tolerance of angle for passing 
        private double goalkeeperPassTolerance = 1.0;   // in degrees of goalkeeper to defender
        private double defenderPassTolerance   = 10.0;  // in degrees of defender to forward
        private double forwardPassTolerance    = 1.0;   // in degrees of forward to goal
        private double distanceTolerance       = 3.0;   // in meters distance before opponent is a threat
        private double turnDistance            = 4.0;   // in meters distance before opponent is a threat but can turn

        private double passDistance            = 35.0; // distance from player before kicking
        private double kickDistance            = 30.0; // distance from goal center before kicking
        private double minimumForwardGoalX     = 10;   // the minimum horizontal distance from goal before shooting
        private double forwardGoalX            = 30;   // x distance outside goal for forwards, optimum shooting position

        // central defender pitch limits 
        private int HorizontalCenterLower = 0;
        private int HorizontalCenterUpper = 0;

        // The player-numbers for the various positions...
        private int playerNumber_LeftWingDefender = -1;
        private int playerNumber_CentreDefender = -1;
        private int playerNumber_RightWingDefender = -1;
        private int playerNumber_LeftWingForward = -1;
        private int playerNumber_RightWingForward = -1;

        private Position origin = new Position(0.0, 0.0);
        private int selectedForward = -1;

        //player distance from ball
        private double [] playerBallDistance = {0,0,0,0,0,0,0,0,0,0,0,0};

        private  double ballDirection = 0.0;
        Position ballPositionNext = new Position(50, 25);

        Position predictedGoalPosition       = new Position(50, 25);
        Position predictedGoalkeeperPosition = new Position(50, 25);

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

        private class PositionVector
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public PositionVector(Position p, Vector v, double speed)
            { 
                this.position = new Position(p.x, p.y); 
                this.vector   = new Vector(v.x, v.y);
                this.speed    = speed;
            }

            public Position position;
            public Vector   vector;
            public double   speed;
        }

        private Dictionary<int, PositionVector> ballPositions = new Dictionary<int, PositionVector>();

        // Holds the goal position the forward is targeting
        private Dictionary<int, Position> goalShootPosition = new Dictionary<int, Position>();

        private Dictionary<PositionKey, PositionValue> defaultPositions = new Dictionary<PositionKey, PositionValue>();
        private Dictionary<PositionKey, PositionValue> kickoffPositions = new Dictionary<PositionKey, PositionValue>();
        private Dictionary<PositionKey, PositionValue> attackPositions  = new Dictionary<PositionKey, PositionValue>();

        //selected player to pass to
        private Dictionary<int, int> selectedPlayer = new Dictionary<int, int>();
        
        // For some random choices...
        private Random rnd = new Random();

        enum Style
        {
            Normal,
            Defense,
            Attack,
        }
        #endregion
    }
}
