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
            setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.RIGHT, new Position(25, 10), 90);
            setDefaultPosition(this.playerNumber_CentreDefender, DirectionType.RIGHT, new Position(25, 25), 90);
            setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(25, 40), 90);
            setDefaultPosition(this.playerNumber_LeftWingForward, DirectionType.RIGHT, new Position(75, 15), 90);
            setDefaultPosition(this.playerNumber_RightWingForward, DirectionType.RIGHT, new Position(75, 35), 90);

            setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.LEFT, new Position(75, 40), 270);
            setDefaultPosition(this.playerNumber_CentreDefender, DirectionType.LEFT, new Position(75, 25), 270);
            setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(75, 10), 270);
            setDefaultPosition(this.playerNumber_LeftWingForward, DirectionType.LEFT, new Position(25, 35), 270);
            setDefaultPosition(this.playerNumber_RightWingForward, DirectionType.LEFT, new Position(25, 15), 270);

            // We set up the kickoff positions. 
            // Note: The player nearest the centre will be automatically "promoted"
            //       to the centre by the game.
            setKickoffPosition(this.playerNumber_LeftWingDefender, DirectionType.RIGHT, new Position(25, 10), 90);
            setKickoffPosition(this.playerNumber_CentreDefender, DirectionType.RIGHT, new Position(25, 25), 90);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(25, 40), 90);
            setKickoffPosition(this.playerNumber_LeftWingForward, DirectionType.RIGHT, new Position(49, 14), 90);
            setKickoffPosition(this.playerNumber_RightWingForward, DirectionType.RIGHT, new Position(49, 36), 90);

            setKickoffPosition(this.playerNumber_LeftWingDefender, DirectionType.LEFT, new Position(75, 40), 270);
            setKickoffPosition(this.playerNumber_CentreDefender, DirectionType.LEFT, new Position(75, 25), 270);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(75, 10), 270);
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

        #endregion

        #region Private functions

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
                getAction_OwnHalf(actions, player);
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
                action.add("speed", 100.0);
                actions.Add(action);
            }
        }

        /// <summary>
        /// Kicks the ball from a defending player to a forward.
        /// </summary>
        private void getAction_Defender_HasBall(List<JSObject> actions, dynamic player)
        {
            int playerNumber = player.staticState.playerNumber;

            // We choose a forward and find his position...
            int forwardPlayerNumber = (this.rnd.NextDouble() < 0.5) ? this.playerNumber_LeftWingForward : this.playerNumber_RightWingForward;
            var forward = this.allTeamPlayers[forwardPlayerNumber];
            var destination = new Position(forward.dynamicState.position);

            // We kick the ball towards him...
            var action = new JSObject();
            action.add("action", "KICK");
            action.add("playerNumber", playerNumber);
            action.add("destination", destination);
            action.add("speed", 100.0);
            actions.Add(action);

            Logger.log(String.Format("{0}: Defender {1} kicks the ball to {2}", this.gameTimeSeconds, playerNumber, destination), Logger.LogLevel.INFO);
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
            var ballPosition = new Position(this.ball.position);
            double distance = playerPosition.distanceFrom(ballPosition);
            if(distance < 5.0)
            {
                // We attempt to take possession of the ball...
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "TAKE_POSSESSION");
                actions.Add(action);
            }
            else
            {
                // We move towards the ball...
                var action = new JSObject();
                action.add("playerNumber", playerNumber);
                action.add("action", "MOVE");
                action.add("destination", ballPosition);
                action.add("speed", 100.0);
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
                getAction_OwnHalf(actions, player);
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
                action.add("speed", 100.0);
                actions.Add(action);
            }
        }

        /// <summary>
        /// The forward shoots at the goal.
        /// </summary>
        private void getAction_Forward_HasBall(List<JSObject> actions, dynamic player)
        {
            int playerNumber = player.staticState.playerNumber;

            // We find the goal to aim at, and choose a point a bit to the side
            // of the centre...
            var shootAt = getGoalCentre(GoalType.THEIR_GOAL);
            shootAt.y += ((this.rnd.NextDouble() < 0.5) ? 2.0 : -2.0);

            // We kick the ball at the goal...
            var action = new JSObject();
            action.add("action", "KICK");
            action.add("playerNumber", playerNumber);
            action.add("destination", shootAt);
            action.add("speed", 100.0);
            actions.Add(action);

            Logger.log(String.Format("{0}: Forward {1} shoots to {2}", this.gameTimeSeconds, playerNumber, shootAt), Logger.LogLevel.INFO);
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
            // We choose a defender and find his position...
            int defenderNumber = (this.rnd.NextDouble() < 0.5) ? this.playerNumber_LeftWingDefender : this.playerNumber_RightWingDefender;
            var defender = this.allTeamPlayers[defenderNumber];
            var defenderPosition = new Position(defender.dynamicState.position);

            // We kick the ball to the defender...
            var action = new JSObject();
            action.add("action", "KICK");
            action.add("playerNumber", this.goalkeeperPlayerNumber);
            action.add("destination", defenderPosition);
            action.add("speed", 60.0);
            actions.Add(action);

            Logger.log(String.Format("Goalkeeper kicks the ball to {0}", defenderPosition), Logger.LogLevel.INFO);
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
            if(distance < 5.0)
            {
                // We are close to the ball, so we try to take possession...
                var action = new JSObject();
                action.add("action", "TAKE_POSSESSION");
                action.add("playerNumber", this.goalkeeperPlayerNumber);
                actions.Add(action);
            }
            else
            {
                // We are too far away, so we move towards the ball...
                var action = new JSObject();
                action.add("playerNumber", this.goalkeeperPlayerNumber);
                action.add("action", "MOVE");
                action.add("destination", ballPosition);
                action.add("speed", 100.0);
                actions.Add(action);
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
            action.add("speed", 100.0);
            actions.Add(action);
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

        #endregion
    }
}
