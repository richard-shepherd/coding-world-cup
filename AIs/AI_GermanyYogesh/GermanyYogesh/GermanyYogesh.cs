using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Helpers;
using System.Linq;
using System.Collections;

namespace GermanyYogesh
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
    class GermanyYogesh : CodingWorldCupAPI
    {
        #region CodingWorldCupAPI implementation

        /// <summary>
        /// We request semi-random abilities for our players.
        /// </summary>
        protected override void processRequest_ConfigureAbilities(dynamic data)
        {
            var reply = new JSObject();
            reply.add("requestType", "CONFIGURE_ABILITIES");
            var playerInfos = new List<JSObject>();
            playerInfos = SetAbilities(data);
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
            // We assign players to the positions...
            var playerNumbers = new List<int>(this.teamPlayers.Keys);
            this.playerNumber_LeftWingDefender = playerNumbers[0];
            this.playerNumber_RightWingDefender = playerNumbers[1];
            this.playerNumber_LeftMidField = playerNumbers[2];
            this.playerNumber_RightMidField = playerNumbers[3];
            this.playerNumber_Forward = playerNumbers[4];

            SetDefaultPositions();
            SetKickOffPositions();
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
            foreach (var pair in this.kickoffPositions)
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


            // We add actions to the reply for each of our players...
            var actions = GetActionsForAllPlayers();
            //var actions = new List<JSObject>();

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

        #region Custom GermanyYogesh Code
        #region Declarations
        // The player-numbers for the various positions...
        private int playerNumber_LeftWingDefender = -1;
        private int playerNumber_RightWingDefender = -1;
        private int playerNumber_LeftMidField = -1;
        private int playerNumber_RightMidField = -1;
        private int playerNumber_Forward = -1;

        public enum ActionType
        {
            Kick,
            Move,
            TakePossession
        }
        #endregion

        #region Positions
        // Default positions and directions, keyed by player-number and direction...
        private class PositionKey : Tuple<int, DirectionType>
        {
            public PositionKey(int playerNumber, DirectionType direction) : base(playerNumber, direction) { }
        }
        private class PositionValue
        {
            public Position position;
            public double direction;
        }
        private Dictionary<PositionKey, PositionValue> defaultPositions = new Dictionary<PositionKey, PositionValue>();
        private Dictionary<PositionKey, PositionValue> kickoffPositions = new Dictionary<PositionKey, PositionValue>();
        private void SetDefaultPositions()
        {
            setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.RIGHT, new Position(20, 15), 90);
            setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(20, 35), 90);
            setDefaultPosition(this.playerNumber_LeftMidField, DirectionType.RIGHT, new Position(49, 10), 90);
            setDefaultPosition(this.playerNumber_RightMidField, DirectionType.RIGHT, new Position(49, 40), 90);
            setDefaultPosition(this.playerNumber_Forward, DirectionType.RIGHT, new Position(78, 25), 90);

            setDefaultPosition(this.playerNumber_LeftWingDefender, DirectionType.LEFT, new Position(80, 15), 270);
            setDefaultPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(80, 35), 270);
            setDefaultPosition(this.playerNumber_LeftMidField, DirectionType.LEFT, new Position(51, 10), 270);
            setDefaultPosition(this.playerNumber_RightMidField, DirectionType.LEFT, new Position(51, 40), 270);
            setDefaultPosition(this.playerNumber_Forward, DirectionType.LEFT, new Position(22, 25), 270);
        }

        private void SetKickOffPositions()
        {
            setKickoffPosition(this.playerNumber_LeftWingDefender, DirectionType.RIGHT, new Position(20, 15), 90);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.RIGHT, new Position(20, 35), 90);
            setKickoffPosition(this.playerNumber_LeftMidField, DirectionType.RIGHT, new Position(49, 10), 90);
            setKickoffPosition(this.playerNumber_RightMidField, DirectionType.RIGHT, new Position(49, 25), 90);
            setKickoffPosition(this.playerNumber_Forward, DirectionType.RIGHT, new Position(49, 40), 90);

            setKickoffPosition(this.playerNumber_LeftWingDefender, DirectionType.LEFT, new Position(80, 15), 270);
            setKickoffPosition(this.playerNumber_RightWingDefender, DirectionType.LEFT, new Position(80, 35), 270);
            setKickoffPosition(this.playerNumber_LeftMidField, DirectionType.LEFT, new Position(51, 10), 270);
            setKickoffPosition(this.playerNumber_RightMidField, DirectionType.LEFT, new Position(51, 25), 270);
            setKickoffPosition(this.playerNumber_Forward, DirectionType.LEFT, new Position(51, 40), 270);

        }

        private void setDefaultPosition(int playerNumber, DirectionType playingDirection, Position position, double direction)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            var value = new PositionValue { position = position, direction = direction };
            this.defaultPositions[key] = value;
        }

        private PositionValue getDefaultPosition(int playerNumber, DirectionType playingDirection)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            return this.defaultPositions[key];
        }

        private void setKickoffPosition(int playerNumber, DirectionType playingDirection, Position position, double direction)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            var value = new PositionValue { position = position, direction = direction };
            this.kickoffPositions[key] = value;
        }

        private PositionValue getKickoffPosition(int playerNumber, DirectionType playingDirection)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            return this.kickoffPositions[key];
        }
        #endregion

        #region important functions
        private List<JSObject> SetAbilities(dynamic data)
        {
            var playerNumbers = new List<int>(this.teamPlayers.Keys);
            this.playerNumber_LeftWingDefender = playerNumbers[0];
            this.playerNumber_RightWingDefender = playerNumbers[1];
            this.playerNumber_LeftMidField = playerNumbers[2];
            this.playerNumber_RightMidField = playerNumbers[3];
            this.playerNumber_Forward = playerNumbers[4];

            var playerInfos = new List<JSObject>();

            var playerInfo1 = new JSObject();
            playerInfo1.add("playerNumber", this.playerNumber_LeftWingDefender);
            playerInfo1.add("runningAbility", (data.totalKickingAbility * 0.15));
            playerInfo1.add("kickingAbility", (data.totalKickingAbility * 0.15));
            playerInfo1.add("ballControlAbility", (data.totalKickingAbility * 0.15));
            playerInfo1.add("tacklingAbility", (data.totalKickingAbility * 0.18));
            playerInfos.Add(playerInfo1);

            var playerInfo2 = new JSObject();
            playerInfo2.add("playerNumber", this.playerNumber_RightWingDefender);
            playerInfo2.add("runningAbility", (data.totalKickingAbility * 0.15));
            playerInfo2.add("kickingAbility", (data.totalKickingAbility * 0.15));
            playerInfo2.add("ballControlAbility", (data.totalKickingAbility * 0.15));
            playerInfo2.add("tacklingAbility", (data.totalKickingAbility * 0.18));
            playerInfos.Add(playerInfo2);

            var playerInfo3 = new JSObject();
            playerInfo3.add("playerNumber", this.playerNumber_LeftMidField);
            playerInfo3.add("runningAbility", (data.totalKickingAbility * 0.22));
            playerInfo3.add("kickingAbility", (data.totalKickingAbility * 0.20));
            playerInfo3.add("ballControlAbility", (data.totalKickingAbility * 0.20));
            playerInfo3.add("tacklingAbility", (data.totalKickingAbility * 0.20));
            playerInfos.Add(playerInfo3);

            var playerInfo4 = new JSObject();
            playerInfo4.add("playerNumber", this.playerNumber_RightMidField);
            playerInfo4.add("runningAbility", (data.totalKickingAbility * 0.22));
            playerInfo4.add("kickingAbility", (data.totalKickingAbility * 0.20));
            playerInfo4.add("ballControlAbility", (data.totalKickingAbility * 0.20));
            playerInfo4.add("tacklingAbility", (data.totalKickingAbility * 0.20));
            playerInfos.Add(playerInfo4);

            var playerInfo5 = new JSObject();
            playerInfo5.add("playerNumber", this.playerNumber_Forward);
            playerInfo5.add("runningAbility", (data.totalKickingAbility * 0.17));
            playerInfo5.add("kickingAbility", (data.totalKickingAbility * 0.20));
            playerInfo5.add("ballControlAbility", (data.totalKickingAbility * 0.20));
            playerInfo5.add("tacklingAbility", (data.totalKickingAbility * 0.20));
            playerInfos.Add(playerInfo5);

            var playerInfo6 = new JSObject();
            playerInfo6.add("playerNumber", this.goalkeeperPlayerNumber);
            playerInfo6.add("runningAbility", (data.totalKickingAbility * 0.09));
            playerInfo6.add("kickingAbility", (data.totalKickingAbility * 0.10));
            playerInfo6.add("ballControlAbility", (data.totalKickingAbility * 0.10));
            playerInfo6.add("tacklingAbility", (data.totalKickingAbility * 0.04));
            playerInfos.Add(playerInfo6);

            return playerInfos;

        }

        #region Actions
        private JSObject GetActionsForPlayer(int playerNumber, Position position, ActionType actionType, double speed)
        {
            var player = this.allTeamPlayers[playerNumber];
            if (actionType == ActionType.Move)
            {

                var action = new JSObject();
                action.add("playerNumber", player.staticState.playerNumber);
                action.add("action", "MOVE");
                action.add("destination", position);
                action.add("speed", speed);
                return action;
            }
            else if (actionType == ActionType.TakePossession)
            {
                var action = new JSObject();
                action.add("playerNumber", player.staticState.playerNumber);
                action.add("action", "TAKE_POSSESSION");
                return action;
            }
            else
            {
                var action = new JSObject();
                action.add("action", "KICK");
                action.add("playerNumber", player.staticState.playerNumber);
                action.add("destination", position);
                action.add("speed", 100.0);
                return action;
            }
        }
        private List<JSObject> GetActionsForAllPlayers()
        {
            List<JSObject> allPlayersActions = new List<JSObject>();

            List<JSObject> goalKeeperActions = GoalKeeperStrategy();
            foreach (JSObject action in goalKeeperActions)
            {
                allPlayersActions.Add(action);
            }



            List<JSObject> playersActions = new List<JSObject>();
            var opposingClosestPlayerToBall = OpposingPlayerByBallPosition.First().Value.Distance;
            var ourClosestPlayerToBall = PlayersByBallPosition.First().Value.Distance;

            if (this.allTeamPlayers.ContainsKey(this.ball.controllingPlayerNumber) || (ourClosestPlayerToBall < opposingClosestPlayerToBall))
            {
                playersActions = ForwardStrategy();
            }
            else
            {
                playersActions = DefenseStrategy();
            }

            foreach (JSObject action in playersActions)
            {
                allPlayersActions.Add(action);
            }

            return allPlayersActions;
        }
        #endregion

        #endregion

        #region support functions

        #region PlayersByPosition
        public Dictionary<int, DynamicState> PlayersByBallPosition
        {
            get
            {

                Dictionary<int, DynamicState> playerByBallPosition = new Dictionary<int, DynamicState>();
                Dictionary<int, DynamicState> playerDetails = new Dictionary<int, DynamicState>();
                foreach (var playerNumber in teamPlayers.Keys)
                {
                    var player = this.allTeamPlayers[playerNumber];
                    if (player.staticState.playerType == "G")
                        break;
                    playerDetails.Add(playerNumber,
                        new DynamicState(new Position(player.dynamicState.position),
                                            new Position(player.dynamicState.position).distanceFrom(BallPosition),
                                            (Double)player.staticState.runningAbility,
                                            GetLocationByPosition(new Position(player.dynamicState.position))));
                }
                var playersByPosition = playerDetails.OrderBy(x => ((DynamicState)x.Value).Distance);
                foreach (var player in playersByPosition)
                    playerByBallPosition.Add(player.Key, player.Value);
                return playerByBallPosition;
            }
        }

        public Dictionary<int, DynamicState> OpposingPlayerByBallPosition
        {
            get
            {
                Dictionary<int, DynamicState> opposingPlayerByBallPosition = new Dictionary<int, DynamicState>();
                Dictionary<int, DynamicState> opposingPlayerDetails = new Dictionary<int, DynamicState>();

                foreach (var playerNumber in this.allOpposingTeamPlayers.Keys)
                {

                    var player = this.allOpposingTeamPlayers[playerNumber];
                    if (player.staticState.playerType == "G")
                        break;

                    var playerPosition = new Position(player.dynamicState.position);
                    var distanceToBall = playerPosition.distanceFrom(BallPosition);
                    int playerLocation = GetLocationByPosition(new Position(player.dynamicState.position));
                    double playerRuningAbility = (Double)player.staticState.runningAbility;

                    opposingPlayerDetails.Add(playerNumber,
                        new DynamicState(playerPosition, distanceToBall, playerRuningAbility, playerLocation));
                }
                var opposingPlayersByPosition = opposingPlayerDetails.OrderBy(x => ((DynamicState)x.Value).Distance);

                foreach (var player in opposingPlayersByPosition)
                    opposingPlayerByBallPosition.Add(player.Key, player.Value);
                return opposingPlayerByBallPosition;
            }
        }

        public Dictionary<int, DynamicState> OpposingPlayerByGoalPosition
        {
            get
            {
                Dictionary<int, DynamicState> opposingPlayerByGoalPosition = new Dictionary<int, DynamicState>();
                Dictionary<int, DynamicState> opposingPlayerDetails = new Dictionary<int, DynamicState>();

                foreach (var playerNumber in this.allOpposingTeamPlayers.Keys)
                {
                    var player = this.allOpposingTeamPlayers[playerNumber];
                    if (player.staticState.playerType == "G")
                        break;
                    opposingPlayerDetails.Add(playerNumber,
                                            new DynamicState(new Position(player.dynamicState.position),
                                            new Position(player.dynamicState.position).distanceFrom(OurGoalPosition),
                                            player.staticState.runningAbility,
                                            GetLocationByPosition(new Position(player.dynamicState.position))));
                }
                var opposingPlayersByPosition = opposingPlayerDetails.OrderBy(x => ((DynamicState)x.Value).Distance);

                foreach (var player in opposingPlayersByPosition)
                    opposingPlayerByGoalPosition.Add(player.Key, player.Value);
                return opposingPlayerByGoalPosition;
            }
        }

        public Dictionary<int, DynamicState> PlayersByClosestPositionToBisect(Position targetPosition, Position sourcePosition)
        {
            Dictionary<int, DynamicState> playerByPositionToMark = new Dictionary<int, DynamicState>();
            Dictionary<int, DynamicState> playerDetails = new Dictionary<int, DynamicState>();

            foreach (var playerNumber in this.teamPlayers.Keys)
            {
                var player = this.allTeamPlayers[playerNumber];
                var moveTo = BisectPlayers(targetPosition, sourcePosition, new Position(player.dynamicState.position));
                playerDetails.Add(playerNumber,
                                        new DynamicState(new Position(player.dynamicState.position),
                                        new Position(player.dynamicState.position).distanceFrom(moveTo),
                                        (double)player.staticState.runningAbility,
                                        GetLocationByPosition(new Position(player.dynamicState.position)), moveTo));
            }
            var PlayersByPosition = playerDetails.OrderBy(x => ((DynamicState)x.Value).Distance);

            foreach (var player in PlayersByPosition)
                playerByPositionToMark.Add(player.Key, player.Value);
            return playerByPositionToMark;
        }


        public Dictionary<int, DynamicState> OppositionPlayersByClosestPositionToBisect(Position targetPosition, Position sourcePosition)
        {
            Dictionary<int, DynamicState> playerByPositionToMark = new Dictionary<int, DynamicState>();
            Dictionary<int, DynamicState> playerDetails = new Dictionary<int, DynamicState>();

            foreach (var playerNumber in this.allOpposingTeamPlayers.Keys)
            {
                var player = this.allOpposingTeamPlayers[playerNumber];
                var moveTo = BisectPlayers(targetPosition, sourcePosition, new Position(player.dynamicState.position));
                
                Position playerPosition = new Position(player.dynamicState.position);
                double distanceFrom = playerPosition.distanceFrom(moveTo);
                int Location = GetLocationByPosition(new Position(player.dynamicState.position));
                double runningAbility = (double)player.staticState.runningAbility;
                DynamicState state = new DynamicState(playerPosition,distanceFrom,
                                        runningAbility, Location, moveTo);
                playerDetails.Add(playerNumber,state);
            }
            var PlayersByPosition = playerDetails.OrderBy(x => ((DynamicState)x.Value).Distance);

            foreach (var player in PlayersByPosition)
                playerByPositionToMark.Add(player.Key, player.Value);
            return playerByPositionToMark;
        }
        #endregion

        public Position BisectPlayers(Position A, Position B, Position P)
        {
            Position a_to_p = new Position(P.x - A.x, P.y - A.y);
            Position a_to_b = new Position(B.x - A.x, B.y - A.y);
            var atb2 = a_to_b.x * a_to_b.x + a_to_b.y * a_to_b.y;
            var atp_dot_atb = a_to_p.x * a_to_b.x + a_to_p.y * a_to_b.y;
            var t = atp_dot_atb / atb2;
            var closestPoint = new Position(A.x + a_to_b.x * t, A.y + a_to_b.y * t);


            if (!(closestPoint.x.IsBetween(A.x,B.x) && closestPoint.y.IsBetween(A.y,B.y)))
            {
                double incrementValueX = 5;
                double incrementValueY = 5;
                if (Math.Abs(A.x-B.x)<10)
                    incrementValueX = Math.Abs(A.x - B.x) / 2;
                if (Math.Abs(A.y - B.y) < 10)
                    incrementValueY = Math.Abs(A.y - B.y) / 2;

                if(!closestPoint.x.IsBetween(A.x,B.x))
                {
                    if((closestPoint.x<A.x) && (A.x<B.x))
                        closestPoint.x = A.x + incrementValueX;
                    else if(closestPoint.x<A.x)
                        closestPoint.x = B.x + incrementValueX;
                    else if((closestPoint.x>A.x) && (A.x>B.x))
                        closestPoint.x = A.x - incrementValueX;
                    else
                        closestPoint.x=B.x-5;
                }
                
                if(!closestPoint.y.IsBetween(A.y,B.y))
                {
                    if((closestPoint.y<A.y) && (A.y<B.y))
                        closestPoint.y = A.y + incrementValueY;
                    else if(closestPoint.y<A.y)
                        closestPoint.y = B.y + incrementValueY;
                    else if((closestPoint.y>A.y) && (A.y>B.y))
                        closestPoint.y = A.y - incrementValueY;
                    else
                        closestPoint.y = B.y - incrementValueY;
                }
            }

            if (positionInGoalArea(closestPoint))
            {
                if (!positionInGoalArea(A))
                    return A;
                else
                    return B;
            }
            else
                return closestPoint;
        }

        public Position GetDefaultPosition(int playerNumber)
        {
            return new Position(getDefaultPosition(playerNumber, this.playingDirection).position);
        }

        #region CommonProperties

        public Position BallPosition
        {
            get { return new Position(this.ball.position); }
        }

        public Position OurGoalPosition
        {
            get { return new Position(getGoalCentre(GoalType.OUR_GOAL)); }
        }

        public Position TheirGoalPosition
        {
            get { return new Position(getGoalCentre(GoalType.THEIR_GOAL)); }
        }

        public int ClosestPlayerToBall
        {
            get { return PlayersByBallPosition.First().Key; }
        }

        public JSObject MovePlayer(int playerNumber, Position position)
        {
            var action = new JSObject();
            action.add("playerNumber", playerNumber);
            action.add("action", "MOVE");
            action.add("destination", position);
            action.add("speed", 100);
            return action;
        }

        public JSObject TakePosession(int playerNumber)
        {
            var action = new JSObject();
            action.add("playerNumber", playerNumber);
            action.add("action", "TAKE_POSSESSION");
            return action;
        }

        public JSObject Kick(int playerNumber, Position kicker, Position passee)
        {
            double distance = kicker.distanceFrom(passee);
            double speed = 100;
            if (distance < 5)
                speed = 30;
            else if (distance < 8)
                speed = 40;
            else if (distance < 12)
                speed = 50;
            else if (distance < 17)
                speed = 60;
            else if (distance < 23)
                speed = 70;
            else if (distance < 28)
                speed = 80;
            else if (distance < 36)
                speed = 90;
            else
                speed = 100;
            if (this.playingDirection == DirectionType.RIGHT)
            {
                if (passee == new Position(100, 25) && speed != 100)
                    speed = speed + 10;
            }
            else
            {
                if (passee == new Position(0, 25) && speed != 100)
                    speed = speed + 10;
            }


            var action = new JSObject();
            action.add("action", "KICK");
            action.add("playerNumber", playerNumber);
            action.add("destination", passee);
            action.add("speed", speed);
            return action;
        }

        public JSObject Kick(int playerNumber, Position kicker, Position passee, double speed)
        {
            double distance = kicker.distanceFrom(passee);
            var action = new JSObject();
            action.add("action", "KICK");
            action.add("playerNumber", playerNumber);
            action.add("destination", passee);
            action.add("speed", speed);
            return action;
        }

        public bool CanManeuver(int playerNumber, ManeuverDirection direction)
        {
            var player = this.allTeamPlayers[playerNumber];
            Position playerPosition = new Position(player.dynamicState.position);
            Position destinationPosition = new Position(0, 0);
            if (direction == ManeuverDirection.Forward)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x + 5, playerPosition.y);
                else
                    destinationPosition = new Position(playerPosition.x - 5, playerPosition.y);

            }
            else if (direction == ManeuverDirection.DiagonallyLeft)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x + 5, playerPosition.y - 5);
                else
                    destinationPosition = new Position(playerPosition.x - 5, playerPosition.y + 5);
            }
            else if (direction == ManeuverDirection.Left)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x, playerPosition.y - 5);
                else
                    destinationPosition = new Position(playerPosition.x, playerPosition.y + 5);
            }
            else if (direction == ManeuverDirection.DiagonallyRight)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x + 5, playerPosition.y + 5);
                else
                    destinationPosition = new Position(playerPosition.x - 5, playerPosition.y - 5);
            }
            else if (direction == ManeuverDirection.Right)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x, playerPosition.y + 5);
                else
                    destinationPosition = new Position(playerPosition.x, playerPosition.y - 5);
            }
            else
            {
                return (CanManeuver(playerNumber, ManeuverDirection.Forward) || CanManeuver(playerNumber, ManeuverDirection.Left)
                    || CanManeuver(playerNumber, ManeuverDirection.DiagonallyLeft) || CanManeuver(playerNumber, ManeuverDirection.DiagonallyRight)
                    || CanManeuver(playerNumber, ManeuverDirection.Right));
            }
            var closestOppPlayer = OppositionPlayersByClosestPositionToBisect(playerPosition, destinationPosition).First();

            if ((closestOppPlayer.Value.Distance > 2) && (destinationPosition.x > 0 && destinationPosition.x < 100 && destinationPosition.y > 0 && destinationPosition.y < 100))
                return true;
            else
                return false;
        }

        public ManeuverDirection GetAvailableManeuverDirection(int playerNumber)
        {
            if (CanManeuver(playerNumber, ManeuverDirection.Forward))
                return ManeuverDirection.Forward;
            else if (CanManeuver(playerNumber, ManeuverDirection.DiagonallyLeft))
                return ManeuverDirection.DiagonallyLeft;
            else if (CanManeuver(playerNumber, ManeuverDirection.DiagonallyRight))
                return ManeuverDirection.DiagonallyRight;
            else if (CanManeuver(playerNumber, ManeuverDirection.Left))
                return ManeuverDirection.Left;
            else if (CanManeuver(playerNumber, ManeuverDirection.Right))
                return ManeuverDirection.Right;
            else
                return ManeuverDirection.None;
        }


        public enum ManeuverDirection
        {
            None,
            Left,
            DiagonallyLeft,
            Forward,
            DiagonallyRight,
            Right,
            AnyAvailable
        }

        public JSObject Maneuver(int playerNumber, ManeuverDirection direction)
        {
            var player = this.allTeamPlayers[playerNumber];
            Position playerPosition = new Position(player.dynamicState.position);
            Position destinationPosition = new Position(0, 0);
            if (direction==ManeuverDirection.Forward)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x + 5, playerPosition.y);
                else
                    destinationPosition = new Position(playerPosition.x - 5, playerPosition.y);
            }
            else if (direction == ManeuverDirection.DiagonallyLeft)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x+5, playerPosition.y - 5);
                else
                    destinationPosition = new Position(playerPosition.x-5, playerPosition.y + 5);
            }
            else if (direction == ManeuverDirection.Left)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x, playerPosition.y - 5);
                else
                    destinationPosition = new Position(playerPosition.x, playerPosition.y + 5);
            }
            else if (direction == ManeuverDirection.DiagonallyRight)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x + 5, playerPosition.y + 5);
                else
                    destinationPosition = new Position(playerPosition.x - 5, playerPosition.y - 5);
            }
            else if (direction == ManeuverDirection.Right)
            {
                if (this.playingDirection == DirectionType.RIGHT)
                    destinationPosition = new Position(playerPosition.x, playerPosition.y + 5);
                else
                    destinationPosition = new Position(playerPosition.x, playerPosition.y - 5);
            }
            else
            {
                if (CanManeuver(playerNumber, ManeuverDirection.AnyAvailable))
                {
                    ManeuverDirection directionToManeuver = GetAvailableManeuverDirection(playerNumber);
                    return Maneuver(playerNumber, directionToManeuver);
                }
            }
            return Kick(playerNumber, playerPosition, destinationPosition, 30);
        }
        #endregion

        #endregion

        #region Strategy
        private void OldDefenseStrategy()
        {

            var actions = new List<JSObject>();
            if (GetBallLocation() == 1)
            {
                #region Ball Location 1

                #region Player 1 action
                if (opposingPlayerByPosition.HasPlayerAtLocation(2, 6, 7))
                {
                    var desitinationPosition = BisectPlayers(OpposingPlayerByPosition.First().Value.PlayerPosition,
                        OpposingPlayerByPosition.PlayersPositionAtLocation(2, 6, 7),
                        PlayerByPosition[ClosestDefender()].PlayerPosition);

                    if (PlayerByPosition[ClosestDefender()].PlayerPosition == desitinationPosition)
                        actions.Add(GetActionsForPlayer(ClosestDefender(), new Position(this.ball.position), ActionType.Move, 100));
                    else
                        actions.Add(GetActionsForPlayer(ClosestDefender(), desitinationPosition, ActionType.Move, 100));
                }
                else
                {
                    var vector = new Vector(getGoalCentre(GoalType.OUR_GOAL), new Position(this.ball.position));
                    var vector16m = vector.getScaledVector(16.0);
                    var moveTo = getGoalCentre(GoalType.OUR_GOAL).getPositionPlusVector(vector16m);
                    actions.Add(GetActionsForPlayer(ClosestDefender(), moveTo, ActionType.Move, 60));
                }
                #endregion

                #region Player 2 action
                if (opposingPlayerByPosition.Second().HasPlayerAtLocation(3, 4, 5, 8, 9, 10, 13, 14, 15))
                {
                    var desitinationPosition = BisectPlayers(OpposingPlayerByPosition.Second().PlayersPositionAtLocation(3, 4, 5, 8, 9, 10, 13, 14, 15),
                               getGoalCentre(GoalType.OUR_GOAL),
                                PlayerByPosition[SecondDefender()].PlayerPosition);

                    actions.Add(GetActionsForPlayer(SecondDefender(), desitinationPosition, ActionType.Move, 100));
                }
                else if (opposingPlayerByPosition.Second().HasPlayerAtLocation(6, 7, 11, 12, 16, 17))
                {
                    var desitinationPosition = BisectPlayers(OpposingPlayerByPosition.First().Value.PlayerPosition,
                        OpposingPlayerByPosition.Second().PlayersPositionAtLocation(6, 7, 11, 12, 16, 17),
                                PlayerByPosition[SecondDefender()].PlayerPosition);

                    actions.Add(GetActionsForPlayer(SecondDefender(), desitinationPosition, ActionType.Move, 100));
                }
                else
                    actions.Add(GetActionsForPlayer(SecondDefender(), GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                #endregion

                #region Player 3 action
                if (opposingPlayerByPosition.Second().HasPlayerAtLocation(8, 9, 10, 13, 14, 15, 18, 19, 20))
                {
                    var vector = new Vector(OpposingPlayerByPosition.Second().PlayersPositionAtLocation(8, 9, 10, 13, 14, 15, 18, 19, 20),
                        OpposingPlayerByPosition.First().Value.PlayerPosition);
                    var vector5m = vector.getScaledVector(5.0);
                    var moveTo = OpposingPlayerByPosition.Second().PlayersPositionAtLocation(8, 9, 10, 13, 14, 15, 18, 19, 20).getPositionPlusVector(vector5m);

                    actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, moveTo, ActionType.Move, 100));
                }
                else
                    actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(40, 10)), ActionType.Move, 100));
                #endregion

                #region Player 4 action
                if (opposingPlayerByPosition.HasPlayerAtLocation(2, 6, 7, 11, 12, 16, 17, 21, 22))
                {
                    var vector = new Vector(OpposingPlayerByPosition.PlayersPositionAtLocation(2, 6, 7, 11, 12, 16, 17, 21, 22),
                        OpposingPlayerByPosition.First().Value.PlayerPosition);
                    var vector5m = vector.getScaledVector(5.0);
                    var moveTo = OpposingPlayerByPosition.PlayersPositionAtLocation(2, 6, 7, 11, 12, 16, 17, 21, 22).getPositionPlusVector(vector5m);

                    actions.Add(GetActionsForPlayer(playerNumber_RightMidField, moveTo, ActionType.Move, 100));
                }
                else
                    actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(45, 25)), ActionType.Move, 100));
                #endregion

                #region Player 5 action
                actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(70, 15)), ActionType.Move, 100));
                #endregion

                #endregion
            }
            else if (GetBallLocation().In(3, 4, 8, 9, 13, 14, 18, 19))
            {
                #region Ball on Left side of the court
                #region Player 1 action
                var desitinationPosition = BisectPlayers(new Position(this.ball.position),
                    getGoalCentre(GoalType.OUR_GOAL),
                    PlayerByPosition[ClosestDefender()].PlayerPosition);

                if (PlayerByPosition[ClosestDefender()].PlayerPosition == desitinationPosition)
                    actions.Add(GetActionsForPlayer(ClosestDefender(), new Position(this.ball.position), ActionType.Move, 100));
                else
                    actions.Add(GetActionsForPlayer(ClosestDefender(), desitinationPosition, ActionType.Move, 100));
                #endregion

                #region Player 2 action
                if (opposingPlayerByPosition.Second().Value.Location < 22)
                {
                    var desitinationPositionPlayer = BisectPlayers(opposingPlayerByPosition.Second().Value.PlayerPosition,
                            getGoalCentre(GoalType.OUR_GOAL),
                            PlayerByPosition[SecondDefender()].PlayerPosition);
                    actions.Add(GetActionsForPlayer(SecondDefender(), desitinationPositionPlayer, ActionType.Move, 100));
                }
                else
                {
                    actions.Add(GetActionsForPlayer(SecondDefender(), GetRelativePosition(new Position(20, 25)), ActionType.Move, 100));
                }
                #endregion

                #region Player 3 action
                if (opposingPlayerByPosition.Second().HasPlayerAtLocation(1, 3, 4, 5, 8, 9, 10, 13, 14))
                {
                    var vector = new Vector(OpposingPlayerByPosition.Second().PlayersPositionAtLocation(1, 3, 4, 5, 8, 9, 10, 13, 14),
                            OpposingPlayerByPosition.First().Value.PlayerPosition);
                    var vector5m = vector.getScaledVector(5.0);
                    var moveTo = OpposingPlayerByPosition.Second().PlayersPositionAtLocation(1, 3, 4, 5, 8, 9, 10, 13, 14).getPositionPlusVector(vector5m);

                    actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, moveTo, ActionType.Move, 100));
                }
                else if (opposingPlayerByPosition.Second().HasPlayerAtLocation(15, 18, 19, 20))
                {
                    var vector = new Vector(OpposingPlayerByPosition.Second().PlayersPositionAtLocation(15, 18, 19, 20),
                            OpposingPlayerByPosition.First().Value.PlayerPosition);
                    var vector5m = vector.getScaledVector(5.0);
                    var moveTo = OpposingPlayerByPosition.Second().PlayersPositionAtLocation(15, 18, 19, 20).getPositionPlusVector(vector5m);

                    actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, moveTo, ActionType.Move, 100));
                }
                else
                {
                    if (new Position(this.ball.position).y < 26)
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, new Position(this.ball.position), ActionType.Move, 100));
                }
                #endregion

                #region Player 4 action
                if (opposingPlayerByPosition.Second().HasPlayerAtLocation(2, 6, 7, 11, 12, 16, 17, 21, 22))
                {
                    var vector = new Vector(OpposingPlayerByPosition.Second().PlayersPositionAtLocation(2, 6, 7, 11, 12, 16, 17, 21, 22),
                            OpposingPlayerByPosition.First().Value.PlayerPosition);
                    var vector5m = vector.getScaledVector(5.0);
                    var moveTo = OpposingPlayerByPosition.Second().PlayersPositionAtLocation(2, 6, 7, 11, 12, 16, 17, 21, 22).getPositionPlusVector(vector5m);

                    actions.Add(GetActionsForPlayer(playerNumber_RightMidField, moveTo, ActionType.Move, 100));
                }
                else
                {

                    if (new Position(this.ball.position).y > 25)
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, new Position(this.ball.position), ActionType.Move, 100));
                }
                #endregion

                #region Player 5 action
                actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(75, 20)), ActionType.Move, 100));
                #endregion
                #endregion
            }
            else if (GetBallLocation().In(5, 10, 15, 20))
            {
                #region ball on the center side of the court

                #region Player 1 action
                var desitinationPosition = BisectPlayers(new Position(this.ball.position),
                    getGoalCentre(GoalType.OUR_GOAL),
                    PlayerByPosition[ClosestDefender()].PlayerPosition);

                if (PlayerByPosition[ClosestDefender()].PlayerPosition == desitinationPosition)
                    actions.Add(GetActionsForPlayer(ClosestDefender(), new Position(this.ball.position), ActionType.Move, 100));
                else
                    actions.Add(GetActionsForPlayer(ClosestDefender(), desitinationPosition, ActionType.Move, 100));
                #endregion

                #region Player 2 action
                if (PlayerByPosition[SecondDefender()].PlayerPosition.y < 26)
                {
                    if ((opposingPlayerByPosition.Second().Value.PlayerPosition.x < 50) && (opposingPlayerByPosition.Second().Value.PlayerPosition.y < 26))
                    {
                        var desitinationPositionPlayer = BisectPlayers(opposingPlayerByPosition.Second().Value.PlayerPosition,
                                getGoalCentre(GoalType.OUR_GOAL),
                                PlayerByPosition[SecondDefender()].PlayerPosition);
                        actions.Add(GetActionsForPlayer(SecondDefender(), desitinationPositionPlayer, ActionType.Move, 100));
                    }
                    else if ((opposingPlayerByPosition.Third().Value.PlayerPosition.x < 50) && (opposingPlayerByPosition.Third().Value.PlayerPosition.y < 26))
                    {
                        var desitinationPositionPlayer = BisectPlayers(opposingPlayerByPosition.Third().Value.PlayerPosition,
                                getGoalCentre(GoalType.OUR_GOAL),
                                PlayerByPosition[SecondDefender()].PlayerPosition);
                        actions.Add(GetActionsForPlayer(SecondDefender(), desitinationPositionPlayer, ActionType.Move, 100));
                    }
                    else
                    {
                        actions.Add(GetActionsForPlayer(SecondDefender(), GetRelativePosition(new Position(16, 32)), ActionType.Move, 100));
                    }
                }
                else
                {
                    if ((opposingPlayerByPosition.Second().Value.PlayerPosition.x < 50) && (opposingPlayerByPosition.Second().Value.PlayerPosition.y > 25))
                    {
                        var desitinationPositionPlayer = BisectPlayers(opposingPlayerByPosition.Second().Value.PlayerPosition,
                                getGoalCentre(GoalType.OUR_GOAL),
                                PlayerByPosition[SecondDefender()].PlayerPosition);
                        actions.Add(GetActionsForPlayer(SecondDefender(), desitinationPositionPlayer, ActionType.Move, 100));
                    }
                    else if ((opposingPlayerByPosition.Third().Value.PlayerPosition.x < 50) && (opposingPlayerByPosition.Third().Value.PlayerPosition.y > 25))
                    {
                        var desitinationPositionPlayer = BisectPlayers(opposingPlayerByPosition.Third().Value.PlayerPosition,
                                getGoalCentre(GoalType.OUR_GOAL),
                                PlayerByPosition[SecondDefender()].PlayerPosition);
                        actions.Add(GetActionsForPlayer(SecondDefender(), desitinationPositionPlayer, ActionType.Move, 100));
                    }
                    else
                    {
                        actions.Add(GetActionsForPlayer(SecondDefender(), GetRelativePosition(new Position(16, 18)), ActionType.Move, 100));
                    }
                }
                #endregion

                #region Player 3 action
                if ((opposingPlayerByPosition.Second().Value.PlayerPosition.x < 50) && (opposingPlayerByPosition.Second().Value.PlayerPosition.y < 26))
                {
                    var vector = new Vector(OpposingPlayerByPosition.Second().Value.PlayerPosition,
                                OpposingPlayerByPosition.First().Value.PlayerPosition);
                    var vector5m = vector.getScaledVector(5.0);
                    var moveTo = OpposingPlayerByPosition.Second().Value.PlayerPosition.getPositionPlusVector(vector5m);

                    actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, moveTo, ActionType.Move, 100));
                }
                else if ((opposingPlayerByPosition.Second().Value.PlayerPosition.x < 50) && (opposingPlayerByPosition.Second().Value.PlayerPosition.y > 25))
                {
                    var vector = new Vector(OpposingPlayerByPosition.Second().Value.PlayerPosition,
                            OpposingPlayerByPosition.First().Value.PlayerPosition);
                    var vector5m = vector.getScaledVector(5.0);
                    var moveTo = OpposingPlayerByPosition.Second().Value.PlayerPosition.getPositionPlusVector(vector5m);

                    actions.Add(GetActionsForPlayer(playerNumber_RightMidField, moveTo, ActionType.Move, 100));
                }
                else
                {
                    if (PlayerByPosition[playerNumber_LeftMidField].PlayerPosition.distanceFrom(new Position(this.ball.position)) <
                        PlayerByPosition[playerNumber_RightMidField].PlayerPosition.distanceFrom(new Position(this.ball.position)))
                    {

                    }
                    else
                    {

                    }
                }
                #endregion

                #endregion
            }
        }

        private List<JSObject> DefenseStrategy()
        {
            var opposingPlayerByBallPosition = OpposingPlayerByBallPosition;
            var playersByBallPosition = PlayersByBallPosition;
            var actions = new List<JSObject>();

            if (GetRelativePositionNew(BallPosition).x < 50)
            {
                var player1 = playersByBallPosition.SkipPlayers(playerNumber_Forward).Key;
                actions.Add(MovePlayer(player1, BallPosition));

                var player2 = PlayersByClosestPositionToBisect(OurGoalPosition, BallPosition).SkipPlayers(player1, playerNumber_Forward);
                actions.Add(MovePlayer(player2.Key, player2.Value.DestinationPosition));

                var firstOpposingClosestPlayerToGoal = opposingPlayerByBallPosition.SkipPlayers(opposingPlayerByBallPosition.First().Key);
                var player3 = PlayersByClosestPositionToBisect(firstOpposingClosestPlayerToGoal.Value.PlayerPosition, BallPosition).SkipPlayers(
                    player1, player2.Key, playerNumber_Forward);
                actions.Add(MovePlayer(player3.Key, player3.Value.DestinationPosition));

                var secondOpposingClosestPlayerToGoal = opposingPlayerByBallPosition.SkipPlayers(opposingPlayerByBallPosition.First().Key, opposingPlayerByBallPosition.Skip(1).First().Key);
                var player4 = PlayersByClosestPositionToBisect(secondOpposingClosestPlayerToGoal.Value.PlayerPosition, BallPosition).SkipPlayers(
                    player1, player2.Key, player3.Key, playerNumber_Forward);
                actions.Add(MovePlayer(player4.Key, player4.Value.DestinationPosition));

                var player5 = playerNumber_Forward;
                actions.Add(MovePlayer(player5, GetDefaultPosition(player5)));
            }
            else if (GetRelativePositionNew(BallPosition).x < 65)
            {
                var player1 = playerNumber_LeftWingDefender;
                actions.Add(MovePlayer(player1, GetDefaultPosition(player1)));

                var player2 = playerNumber_RightWingDefender;
                actions.Add(MovePlayer(player2, GetDefaultPosition(player2)));

                var player3 = playersByBallPosition.SkipPlayers(playerNumber_LeftWingDefender, playerNumber_RightWingDefender, playerNumber_Forward).Key;
                actions.Add(MovePlayer(player3, BallPosition));

                int markedOppositionPlayer = -1;
                var firstOpposingClosestPlayerToBall = from s in opposingPlayerByBallPosition.Skip(1)
                                                       where s.Value.PlayerPosition.x > opposingPlayerByBallPosition.First().Value.PlayerPosition.x
                                                       select s;

                if (firstOpposingClosestPlayerToBall.Count() != 0)
                {
                    var firstOpponentPlayerClosestToBall = firstOpposingClosestPlayerToBall.First();
                    var firstClosestPlayerToBisect = PlayersByClosestPositionToBisect(firstOpponentPlayerClosestToBall.Value.PlayerPosition,
                        opposingPlayerByBallPosition.First().Value.PlayerPosition).SkipPlayers(player1, player2, player3, playerNumber_Forward);
                    var player4 = firstClosestPlayerToBisect;

                    actions.Add(MovePlayer(player4.Key, player4.Value.DestinationPosition));
                    markedOppositionPlayer = firstOpponentPlayerClosestToBall.Key;
                }
                else
                {
                    var player4 = playersByBallPosition.SkipPlayers(player1, player2, player3, playerNumber_Forward).Key;
                    actions.Add(MovePlayer(player4, GetDefaultPosition(player4)));
                }

                var player5 = playerNumber_Forward;
                actions.Add(MovePlayer(player5, GetDefaultPosition(player5)));

            }
            else
            {
                var player1 = playerNumber_LeftWingDefender;
                actions.Add(MovePlayer(player1, GetDefaultPosition(player1)));

                var player2 = playerNumber_RightWingDefender;
                actions.Add(MovePlayer(player2, GetDefaultPosition(player2)));

                var player3 = playersByBallPosition.SkipPlayers(playerNumber_LeftWingDefender, playerNumber_RightWingDefender).Key;
                actions.Add(MovePlayer(player3, BallPosition));

                int markedOppositionPlayer = -1;
                var firstOpposingClosestPlayerToBall = from s in opposingPlayerByBallPosition.Skip(1)
                                                       where s.Value.PlayerPosition.x > opposingPlayerByBallPosition.First().Value.PlayerPosition.x
                                                       select s;

                int player4 = -1;

                if (firstOpposingClosestPlayerToBall.Count() != 0)
                {
                    var firstOpponentPlayerClosestToBall = firstOpposingClosestPlayerToBall.First();
                    var firstClosestPlayerToBisect = PlayersByClosestPositionToBisect(firstOpponentPlayerClosestToBall.Value.PlayerPosition,
                        opposingPlayerByBallPosition.First().Value.PlayerPosition).SkipPlayers(player1, player2, player3);
                    player4 = firstClosestPlayerToBisect.Key;

                    actions.Add(MovePlayer(player4, firstClosestPlayerToBisect.Value.DestinationPosition));
                    markedOppositionPlayer = firstOpponentPlayerClosestToBall.Key;
                }
                else
                {
                    player4 = playersByBallPosition.SkipPlayers(player1, player2, player3).Key;
                    actions.Add(MovePlayer(player4, GetDefaultPosition(player4)));
                }

                var player5 = playersByBallPosition.SkipPlayers(player1, player2, player3, player4);
                actions.Add(MovePlayer(player5.Key, GetDefaultPosition(player5.Key)));

                var secondOpposingClosestPlayerToBall = from s in opposingPlayerByBallPosition.Skip(1)
                                                        where s.Key != markedOppositionPlayer
                                                        && s.Value.PlayerPosition.x > opposingPlayerByBallPosition.First().Value.PlayerPosition.x
                                                        && s.Key != markedOppositionPlayer
                                                        select s;

                if (secondOpposingClosestPlayerToBall.Count() != 0)
                {
                    var secondOpponentPlayer = secondOpposingClosestPlayerToBall.First();
                    var destinationToBisect = BisectPlayers(secondOpponentPlayer.Value.PlayerPosition, opposingPlayerByBallPosition.First().Value.PlayerPosition,
                        player5.Value.PlayerPosition);
                    if ((destinationToBisect.x > 65) && (player5.Key == playerNumber_Forward))
                    {
                        actions.Add(MovePlayer(player5.Key, GetDefaultPosition(player5.Key)));
                    }
                    else
                    {
                        actions.Add(MovePlayer(player5.Key, destinationToBisect));
                    }
                }
                else
                {
                    actions.Add(MovePlayer(player5.Key, GetDefaultPosition(player5.Key)));
                }
            }
            return actions;

        }

        private List<JSObject> ForwardStrategyOld()
        {
            var actions = new List<JSObject>();
            var ballPosition = new Position(this.ball.position);

            Tuple<int, int> ballArea = GetBallLocationOld();

            //GoalKeeperaction



            if (this.allTeamPlayers.ContainsKey(this.ball.controllingPlayerNumber))
            {
                //Somebody in the team already has the ball. so others would be recipient.
                if (playerNumber_LeftWingDefender == this.ball.controllingPlayerNumber)
                {
                    var destinationPlayerPosition = new Position(this.allTeamPlayers[this.playerNumber_LeftMidField].dynamicState.position);
                    actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, destinationPlayerPosition, ActionType.Kick, 60));
                }
                else if (playerNumber_RightWingDefender == this.ball.controllingPlayerNumber)
                {
                    var destinationPlayerPosition = new Position(this.allTeamPlayers[this.playerNumber_RightMidField].dynamicState.position);
                    actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, destinationPlayerPosition, ActionType.Kick, 60));
                }
                else if ((playerNumber_LeftMidField == this.ball.controllingPlayerNumber))
                {
                    var destinationPlayerPosition = new Position(this.allTeamPlayers[this.playerNumber_Forward].dynamicState.position);
                    actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, destinationPlayerPosition, ActionType.Kick, 60));
                }
                else if ((playerNumber_RightMidField == this.ball.controllingPlayerNumber))
                {
                    var destinationPlayerPosition = new Position(this.allTeamPlayers[this.playerNumber_Forward].dynamicState.position);
                    actions.Add(GetActionsForPlayer(playerNumber_RightMidField, destinationPlayerPosition, ActionType.Kick, 60));
                }
                else
                {
                    actions.Add(GetActionsForPlayer(playerNumber_Forward, getGoalCentre(GoalType.THEIR_GOAL), ActionType.Kick, 60));
                }
            }
            else
            {
                //We need to move towards the ball or take possession
                if (ballArea.Item1 == 1)
                {
                    #region Area1 Movement
                    if (new Position(this.allTeamPlayers[playerNumber_LeftWingDefender].dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, ballPosition, ActionType.TakePossession, 100));
                    else
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, ballPosition, ActionType.Move, 100));


                    if (ballArea.Item2 == 1)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(80, 20)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(65, 10)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(40, 35)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 2)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(75, 30)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(55, 05)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(40, 30)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 3)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, new Position(getDefaultPosition(playerNumber_LeftMidField, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, new Position(getDefaultPosition(playerNumber_RightMidField, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, new Position(getDefaultPosition(playerNumber_Forward, this.playingDirection).position), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 4)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(84, 20)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, new Position(getDefaultPosition(playerNumber_LeftMidField, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(35, 35)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(20, 25)), ActionType.Move, 100));
                    }
                    else
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(84, 25)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, new Position(getDefaultPosition(playerNumber_LeftMidField, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(35, 35)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(20, 25)), ActionType.Move, 100));
                    }
                    #endregion
                }
                else if (ballArea.Item1 == 2)
                {
                    #region Area2 Movement

                    if (new Position(this.allTeamPlayers[playerNumber_LeftWingDefender].dynamicState.position).distanceFrom(ballPosition) <
                            new Position(this.allTeamPlayers[playerNumber_RightWingDefender].dynamicState.position).distanceFrom(ballPosition))
                    {
                        if (new Position(this.allTeamPlayers[playerNumber_LeftWingDefender].dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, ballPosition, ActionType.TakePossession, 100));
                        else
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, ballPosition, ActionType.Move, 100));
                    }
                    else
                    {
                        if (new Position(this.allTeamPlayers[playerNumber_LeftWingDefender].dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, ballPosition, ActionType.TakePossession, 100));
                        else
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, ballPosition, ActionType.Move, 100));
                    }

                    if (ballArea.Item2 == 1)
                    {
                        if (new Position(this.allTeamPlayers[playerNumber_LeftWingDefender].dynamicState.position).distanceFrom(ballPosition) <
                            new Position(this.allTeamPlayers[playerNumber_RightWingDefender].dynamicState.position).distanceFrom(ballPosition))
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(75, 35)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(35, 20)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(55, 30)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }
                        else
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(75, 25)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(55, 20)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(35, 30)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }
                    }
                    else if (ballArea.Item2 == 2)
                    {
                        if (new Position(this.allTeamPlayers[playerNumber_LeftWingDefender].dynamicState.position).distanceFrom(ballPosition) <
                            new Position(this.allTeamPlayers[playerNumber_RightWingDefender].dynamicState.position).distanceFrom(ballPosition))
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(80, 25)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, new Position(getDefaultPosition(playerNumber_LeftMidField, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(40, 30)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }
                        else
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(80, 25)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(40, 20)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightMidField, new Position(getDefaultPosition(playerNumber_RightMidField, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }

                    }
                    #endregion
                }
                else if (ballArea.Item1 == 3)
                {
                    #region Area3 movement

                    if (new Position(this.allTeamPlayers[playerNumber_RightWingDefender].dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, ballPosition, ActionType.TakePossession, 100));
                    else
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, ballPosition, ActionType.Move, 100));


                    if (ballArea.Item2 == 1)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(80, 30)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(40, 25)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(65, 40)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 2)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(75, 20)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(55, 45)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(40, 20)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 3)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, new Position(getDefaultPosition(playerNumber_LeftMidField, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, new Position(getDefaultPosition(playerNumber_RightMidField, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, new Position(getDefaultPosition(playerNumber_Forward, this.playingDirection).position), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 4)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(84, 30)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, new Position(getDefaultPosition(playerNumber_RightMidField, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(35, 15)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(20, 25)), ActionType.Move, 100));
                    }
                    else
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(84, 25)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, new Position(getDefaultPosition(playerNumber_RightMidField, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(35, 15)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(20, 25)), ActionType.Move, 100));
                    }
                    #endregion
                }
                else if (ballArea.Item1 == 4)
                {
                    #region Area4 Movement

                    if (new Position(this.allTeamPlayers[playerNumber_LeftMidField].dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, ballPosition, ActionType.TakePossession, 100));
                    else
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, ballPosition, ActionType.Move, 100));

                    if (ballArea.Item2 == 1)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(80, 25)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(40, 30)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 2)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(80, 20)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(84, 28)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(40, 30)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 3)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(35, 40)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 4)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(35, 40)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 5)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(35, 40)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(35, 40)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    #endregion
                }
                else if (ballArea.Item1 == 5)
                {
                    #region Area5 Movement


                    if (new Position(this.allTeamPlayers[playerNumber_LeftMidField].dynamicState.position).distanceFrom(ballPosition) <
                           new Position(this.allTeamPlayers[playerNumber_RightMidField].dynamicState.position).distanceFrom(ballPosition))
                    {
                        if (new Position(this.allTeamPlayers[playerNumber_LeftMidField].dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                            actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, ballPosition, ActionType.TakePossession, 100));
                        else
                            actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, ballPosition, ActionType.Move, 100));
                    }
                    else
                    {
                        if (new Position(this.allTeamPlayers[playerNumber_RightMidField].dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                            actions.Add(GetActionsForPlayer(playerNumber_RightMidField, ballPosition, ActionType.TakePossession, 100));
                        else
                            actions.Add(GetActionsForPlayer(playerNumber_RightMidField, ballPosition, ActionType.Move, 100));
                    }


                    if (ballArea.Item2 == 1)
                    {
                        if (new Position(this.allTeamPlayers[playerNumber_LeftMidField].dynamicState.position).distanceFrom(ballPosition) <
                                        new Position(this.allTeamPlayers[playerNumber_RightMidField].dynamicState.position).distanceFrom(ballPosition))
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(85, 20)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(85, 25)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(40, 30)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }
                        else
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(85, 20)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(85, 25)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(40, 20)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }
                    }
                    else if (ballArea.Item2 == 2)
                    {
                        if (new Position(this.allTeamPlayers[playerNumber_LeftMidField].dynamicState.position).distanceFrom(ballPosition) <
                                        new Position(this.allTeamPlayers[playerNumber_RightMidField].dynamicState.position).distanceFrom(ballPosition))
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(40, 40)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }
                        else
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(40, 10)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }
                    }
                    else
                    {
                        if (new Position(this.allTeamPlayers[playerNumber_LeftMidField].dynamicState.position).distanceFrom(ballPosition) <
                                        new Position(this.allTeamPlayers[playerNumber_RightMidField].dynamicState.position).distanceFrom(ballPosition))
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(35, 40)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }
                        else
                        {
                            actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(35, 10)), ActionType.Move, 100));
                            actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                        }
                    }
                    #endregion
                }
                else if (ballArea.Item1 == 6)
                {
                    #region Area6 Movement

                    if (new Position(this.allTeamPlayers[playerNumber_RightMidField].dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, ballPosition, ActionType.TakePossession, 100));
                    else
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, ballPosition, ActionType.Move, 100));

                    if (ballArea.Item2 == 1)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(80, 25), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(40, 20)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 2)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, GetRelativePosition(new Position(80, 30)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, GetRelativePosition(new Position(84, 23)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(40, 20)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 3)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(35, 10)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 4)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(35, 10)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 5)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(35, 10)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    else
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(35, 10)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, GetRelativePosition(new Position(16, 25)), ActionType.Move, 100));
                    }
                    #endregion
                }
                else
                {
                    #region Area7 Movement

                    if (new Position(this.allTeamPlayers[playerNumber_Forward].dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, ballPosition, ActionType.TakePossession, 100));
                    else
                        actions.Add(GetActionsForPlayer(playerNumber_Forward, ballPosition, ActionType.Move, 100));

                    if (ballArea.Item2 == 1)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(30, 10)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(30, 40)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 2)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(30, 10)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(30, 40)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 3)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(30, 10)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(30, 40)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 4)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(26, 15)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(26, 35)), ActionType.Move, 100));
                    }
                    else if (ballArea.Item2 == 5)
                    {
                        actions.Add(GetActionsForPlayer(playerNumber_LeftWingDefender, new Position(getDefaultPosition(playerNumber_LeftWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightWingDefender, new Position(getDefaultPosition(playerNumber_RightWingDefender, this.playingDirection).position), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_LeftMidField, GetRelativePosition(new Position(26, 15)), ActionType.Move, 100));
                        actions.Add(GetActionsForPlayer(playerNumber_RightMidField, GetRelativePosition(new Position(26, 35)), ActionType.Move, 100));
                    }
                    #endregion
                }
            }

            return actions;
        }

        private List<JSObject> ForwardStrategy()
        {
            List<JSObject> actions = new List<JSObject>();

            

            var opposingPlayerByBallPosition = OpposingPlayerByBallPosition;
            var playersByBallPosition = PlayersByBallPosition;
            int incrementDecrementCounter = 1;
            if (this.playingDirection == DirectionType.LEFT)
                incrementDecrementCounter = -1;
            
            if (GetRelativePositionNew(BallPosition).x < 35)
            {
                #region Ball < 35
                var bcp = playersByBallPosition.SkipPlayers(playerNumber_Forward);

                #region Ball Controlling Player Option
                if (this.allTeamPlayers[bcp.Key].dynamicState.HasBall)
                {
                    if (CanManeuver(bcp.Key,ManeuverDirection.Forward))
                    {
                        var action = Maneuver(bcp.Key, ManeuverDirection.Forward);
                        actions.Add(action);
                    }
                    else
                    {
                        #region Kick Carefully

                        //Look for kick options
                        var passOptions = from s in playersByBallPosition
                                          where (s.Value.PlayerPosition.x > BallPosition.x + (14 * incrementDecrementCounter)
                                              || s.Value.PlayerPosition.x > BallPosition.x + (36 * incrementDecrementCounter))
                                              && s.Value.Distance<41
                                              orderby s.Value.Distance
                                              select s.Value;
                        if (passOptions.Count() != 0)
                        {
                            DynamicState passOption=null;
                            foreach (DynamicState player in passOptions)
                            {
                                var closestOppositionPlayerToBisect = PlayersByClosestPositionToBisect(bcp.Value.PlayerPosition, player.PlayerPosition);
                                if (closestOppositionPlayerToBisect.First().Value.Distance > 2)
                                {
                                    passOption = player;
                                    break;
                                }
                            }
                            if (passOption != null)
                            {
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, passOption.PlayerPosition));
                            }
                            else
                            {
                                if (this.playingDirection == DirectionType.RIGHT)
                                    actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(100, 25)));
                                else
                                    actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(0, 25)));
                            }
                        }
                        else
                        {
                            if (this.playingDirection == DirectionType.RIGHT)
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(100, 25)));
                            else
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(0, 25)));
                        }
                        #endregion
                    }
                }
                else if (bcp.Value.PlayerPosition.distanceFrom(BallPosition) < 5.0)
                    actions.Add(TakePosession(bcp.Key));
                else
                    actions.Add(MovePlayer(bcp.Key, BallPosition));
                #endregion

                #region Other Players Movements
                var player2 = PlayersByClosestPositionToBisect(OurGoalPosition, BallPosition).SkipPlayers(bcp.Key, playerNumber_Forward, playerNumber_LeftMidField, playerNumber_RightMidField);
                actions.Add(MovePlayer(player2.Key, player2.Value.DestinationPosition));

                var player3 = playersByBallPosition.SkipPlayers(bcp.Key, player2.Key, playerNumber_Forward, playerNumber_LeftWingDefender, playerNumber_RightWingDefender);
                if (this.playingDirection == DirectionType.RIGHT)
                    actions.Add(MovePlayer(player3.Key, new Position(BallPosition.x + 25, BallPosition.y)));
                else
                    actions.Add(MovePlayer(player3.Key, new Position(BallPosition.x - 25, BallPosition.y)));

                if (bcp.Key.In(playerNumber_LeftWingDefender, playerNumber_RightWingDefender))
                {
                    var player4 = playersByBallPosition.SkipPlayers(bcp.Key, player2.Key, player3.Key, playerNumber_Forward, playerNumber_LeftWingDefender, playerNumber_RightWingDefender);
                    if (this.playingDirection == DirectionType.RIGHT)
                    {
                        if (player3.Value.PlayerPosition.y < 21)
                            actions.Add(MovePlayer(player4.Key, new Position(player3.Value.PlayerPosition.x + 15, player3.Value.PlayerPosition.y + 15)));
                        else
                            actions.Add(MovePlayer(player4.Key, new Position(player3.Value.PlayerPosition.x + 15, player3.Value.PlayerPosition.y - 15)));
                    }
                    else
                    {
                        if (player3.Value.PlayerPosition.y < 21)
                            actions.Add(MovePlayer(player4.Key, new Position(player3.Value.PlayerPosition.x - 15, player3.Value.PlayerPosition.y + 15)));
                        else
                            actions.Add(MovePlayer(player4.Key, new Position(player3.Value.PlayerPosition.x - 15, player3.Value.PlayerPosition.y - 15)));
                    }
                }
                else
                {
                    var player4 = playersByBallPosition.SkipPlayers(bcp.Key, player2.Key, player3.Key, playerNumber_Forward, playerNumber_LeftMidField, playerNumber_RightMidField);

                    if (BallPosition.y < 25)
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x, BallPosition.y + 15)));
                    else
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x, BallPosition.y - 15)));
                }

                var player5 = playerNumber_Forward;
                actions.Add(MovePlayer(player5, GetDefaultPosition(player5)));
                #endregion

                #endregion
            }
            else if (GetRelativePositionNew(BallPosition).x < 50)
            {
                #region BallPosition x <50
                var bcp = playersByBallPosition.SkipPlayers(playerNumber_Forward);
                if (this.allTeamPlayers[bcp.Key].dynamicState.HasBall)
                {
                    if ((CanManeuver(bcp.Key, ManeuverDirection.AnyAvailable)) && GetRelativePositionNew(BallPosition).x <45)
                    {
                        var action = Maneuver(bcp.Key, ManeuverDirection.AnyAvailable);
                        actions.Add(action);
                    }
                    else
                    {
                        #region Kick Smartly

                        //Look for kick options
                        var passOptions = from s in playersByBallPosition
                                          where (s.Value.PlayerPosition.x > BallPosition.x + (14 * incrementDecrementCounter)
                                              || s.Value.PlayerPosition.x > BallPosition.x + (36 * incrementDecrementCounter))
                                              && s.Value.Distance < 41
                                          orderby s.Value.Distance
                                          select s.Value;
                        if (passOptions.Count() != 0)
                        {
                            DynamicState passOption = null;
                            foreach (DynamicState player in passOptions)
                            {
                                var closestOppositionPlayerToBisect = PlayersByClosestPositionToBisect(bcp.Value.PlayerPosition, player.PlayerPosition);
                                if (closestOppositionPlayerToBisect.First().Value.Distance > 2)
                                {
                                    passOption = player;
                                    break;
                                }
                            }
                            if (passOption != null)
                            {
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, passOption.PlayerPosition));
                            }
                            else
                            {
                                Position destinationPosition = new Position(0, 0);

                                if(passOptions.First().PlayerPosition.x<25)
                                    destinationPosition = new Position(passOptions.First().PlayerPosition.x + (10 * incrementDecrementCounter), passOptions.First().PlayerPosition.y + (10* incrementDecrementCounter));
                                else
                                    destinationPosition = new Position(passOptions.First().PlayerPosition.x + (10 * incrementDecrementCounter), passOptions.First().PlayerPosition.y - (10 * incrementDecrementCounter));
                            }
                        }
                        else
                        {
                            if (this.playingDirection == DirectionType.RIGHT)
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(100, 25)));
                            else
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(0, 25)));
                        }
                        #endregion
                    }
                }
                else if (bcp.Value.PlayerPosition.distanceFrom(BallPosition) < 5.0)
                {
                    actions.Add(TakePosession(bcp.Key));
                }
                else
                {
                    actions.Add(MovePlayer(bcp.Key, BallPosition));
                }
                #endregion

                #region Other Players Movements
                var player2 = PlayersByClosestPositionToBisect(OurGoalPosition, BallPosition).SkipPlayers(bcp.Key, playerNumber_Forward, playerNumber_LeftMidField, playerNumber_RightMidField);
                actions.Add(MovePlayer(player2.Key, player2.Value.DestinationPosition));

                var player3 = playersByBallPosition.SkipPlayers(bcp.Key, player2.Key, playerNumber_Forward, playerNumber_LeftWingDefender, playerNumber_RightWingDefender);
                if (this.playingDirection == DirectionType.RIGHT)
                    actions.Add(MovePlayer(player3.Key, new Position(BallPosition.x + 25, BallPosition.y)));
                else
                    actions.Add(MovePlayer(player3.Key, new Position(BallPosition.x - 25, BallPosition.y)));

                if (bcp.Key.In(playerNumber_LeftWingDefender, playerNumber_RightWingDefender))
                {
                    var player4 = playersByBallPosition.SkipPlayers(bcp.Key, player2.Key, player3.Key, playerNumber_Forward, playerNumber_LeftWingDefender, playerNumber_RightWingDefender);
                    if (this.playingDirection == DirectionType.RIGHT)
                    {
                        if (player3.Value.PlayerPosition.y < 21)
                            actions.Add(MovePlayer(player4.Key, new Position(player3.Value.PlayerPosition.x + 15, player3.Value.PlayerPosition.y + 15)));
                        else
                            actions.Add(MovePlayer(player4.Key, new Position(player3.Value.PlayerPosition.x + 15, player3.Value.PlayerPosition.y - 15)));
                    }
                    else
                    {
                        if (player3.Value.PlayerPosition.y < 21)
                            actions.Add(MovePlayer(player4.Key, new Position(player3.Value.PlayerPosition.x - 15, player3.Value.PlayerPosition.y + 15)));
                        else
                            actions.Add(MovePlayer(player4.Key, new Position(player3.Value.PlayerPosition.x - 15, player3.Value.PlayerPosition.y - 15)));
                    }
                }
                else
                {
                    var player4 = playersByBallPosition.SkipPlayers(bcp.Key, player2.Key, player3.Key, playerNumber_Forward, playerNumber_LeftMidField, playerNumber_RightMidField);

                    if (BallPosition.y < 25)
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x, BallPosition.y + 15)));
                    else
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x, BallPosition.y - 15)));
                }

                var player5 = playerNumber_Forward;
                actions.Add(MovePlayer(player5, GetDefaultPosition(player5)));
                #endregion
            }
            else if (GetRelativePositionNew(BallPosition).x < 70)
            {
                #region BallPosition < 70
                var bcp = playersByBallPosition.SkipPlayers(playerNumber_LeftWingDefender, playerNumber_RightWingDefender, playerNumber_Forward);
                if (this.allTeamPlayers[bcp.Key].dynamicState.HasBall)
                {

                    if ((CanManeuver(bcp.Key, ManeuverDirection.AnyAvailable)) && GetRelativePositionNew(BallPosition).x < 80)
                    {
                        var action = Maneuver(bcp.Key, ManeuverDirection.AnyAvailable);
                        actions.Add(action);
                    }
                    else
                    {
                        #region Kick Smartly

                        //Look for kick options
                        var passOptions = from s in playersByBallPosition
                                          where (s.Value.PlayerPosition.x > BallPosition.x + (9 * incrementDecrementCounter)
                                              || s.Value.PlayerPosition.x > BallPosition.x + (25 * incrementDecrementCounter))
                                              && s.Value.Distance < 35
                                          orderby s.Value.Distance
                                          select s.Value;
                        if (passOptions.Count() != 0)
                        {
                            DynamicState passOption = null;
                            foreach (DynamicState player in passOptions)
                            {
                                var closestOppositionPlayerToBisect = PlayersByClosestPositionToBisect(bcp.Value.PlayerPosition, player.PlayerPosition);
                                if (closestOppositionPlayerToBisect.First().Value.Distance > 2)
                                {
                                    passOption = player;
                                    break;
                                }
                            }
                            if (passOption != null)
                            {
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, passOption.PlayerPosition));
                            }
                            else
                            {
                                Position destinationPosition = new Position(0, 0);

                                if (passOptions.First().PlayerPosition.x < 25)
                                    destinationPosition = new Position(passOptions.First().PlayerPosition.x + (10 * incrementDecrementCounter), passOptions.First().PlayerPosition.y + (10 * incrementDecrementCounter));
                                else
                                    destinationPosition = new Position(passOptions.First().PlayerPosition.x + (10 * incrementDecrementCounter), passOptions.First().PlayerPosition.y - (10 * incrementDecrementCounter));
                            }
                        }
                        else
                        {
                            if (this.playingDirection == DirectionType.RIGHT)
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(100, 25)));
                            else
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(0, 25)));
                        }
                        #endregion
                    }
                }
                else if (bcp.Value.PlayerPosition.distanceFrom(BallPosition) < 5.0)
                {
                    actions.Add(TakePosession(bcp.Key));
                }
                else
                {
                    actions.Add(MovePlayer(bcp.Key, BallPosition));
                }

                #region Other Players Movements
                var player2 = PlayersByClosestPositionToBisect(OurGoalPosition, BallPosition).SkipPlayers(bcp.Key, playerNumber_Forward, playerNumber_LeftMidField, playerNumber_RightMidField);
                actions.Add(MovePlayer(player2.Key, player2.Value.DestinationPosition));

                
                //next forward
                var player3 = playersByBallPosition.SkipPlayers(bcp.Key, player2.Key, playerNumber_Forward, playerNumber_LeftWingDefender, playerNumber_RightWingDefender);
                if (this.playingDirection == DirectionType.RIGHT)
                    actions.Add(MovePlayer(player3.Key, new Position(BallPosition.x + 25, BallPosition.y)));
                else
                    actions.Add(MovePlayer(player3.Key, new Position(BallPosition.x - 25, BallPosition.y)));


                var player4 = playersByBallPosition.SkipPlayers(bcp.Key, player2.Key, playerNumber_Forward, playerNumber_LeftMidField, playerNumber_RightMidField);
                actions.Add(MovePlayer(player4.Key, GetDefaultPosition(player4.Key)));


                var player5 = playerNumber_Forward;
                actions.Add(MovePlayer(player5, GetDefaultPosition(player5)));
                #endregion
                #endregion
            }
            else if (GetRelativePositionNew(BallPosition).x < 85)
            {
                #region BallPosition>85
                var bcp = playersByBallPosition.SkipPlayers(playerNumber_LeftWingDefender, playerNumber_RightWingDefender);
                if (this.allTeamPlayers[bcp.Key].dynamicState.HasBall)
                {
                    if ((CanManeuver(bcp.Key, ManeuverDirection.AnyAvailable)) && GetRelativePositionNew(BallPosition).x < 80)
                    {
                        var action = Maneuver(bcp.Key, ManeuverDirection.AnyAvailable);
                        actions.Add(action);
                    }
                    else
                    {
                        #region Kick Smartly

                        //Look for kick options
                        var passOptions = from s in playersByBallPosition
                                          where (s.Value.PlayerPosition.x > BallPosition.x + (9 * incrementDecrementCounter)
                                              || s.Value.PlayerPosition.x > BallPosition.x + (19 * incrementDecrementCounter))
                                              && s.Value.Distance < 25
                                              && s.Value.PlayerPosition.x<90
                                          orderby s.Value.Distance
                                          select s.Value;
                        if (passOptions.Count() != 0)
                        {
                            DynamicState passOption = null;
                            foreach (DynamicState player in passOptions)
                            {
                                var closestOppositionPlayerToBisect = PlayersByClosestPositionToBisect(bcp.Value.PlayerPosition, player.PlayerPosition);
                                if (closestOppositionPlayerToBisect.First().Value.Distance > 5)
                                {
                                    passOption = player;
                                    break;
                                }
                            }
                            if (passOption != null)
                            {
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, passOption.PlayerPosition));
                            }
                            else
                            {
                                if (this.playingDirection == DirectionType.RIGHT)
                                    actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(100, 25)));
                                else
                                    actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(0, 25)));
                            }
                        }
                        else
                        {
                            if (this.playingDirection == DirectionType.RIGHT)
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(100, 25)));
                            else
                                actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(0, 25)));
                        }
                        #endregion
                    }
                }
                else if (bcp.Value.PlayerPosition.distanceFrom(BallPosition) < 5.0)
                {
                    actions.Add(TakePosession(bcp.Key));
                }
                else
                {
                    actions.Add(MovePlayer(bcp.Key, BallPosition));
                }

                #region Other Players Movements
                var player2 = playerNumber_LeftWingDefender;
                actions.Add(MovePlayer(player2, GetDefaultPosition(player2)));

                var player3 = playerNumber_RightWingDefender;
                actions.Add(MovePlayer(player3, GetDefaultPosition(player3)));

                //next forward
                var player4 = playersByBallPosition.SkipPlayers(bcp.Key, player2, player3);
                if (this.playingDirection == DirectionType.RIGHT)
                {
                    if(BallPosition.y<10)
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x + 10, 10)));
                    else
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x + 10, BallPosition.y)));
                }
                else
                {
                    if(BallPosition.y>40)
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x - 10, 40)));
                    else
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x - 10, BallPosition.y)));
                }

                var player5 = playersByBallPosition.SkipPlayers(bcp.Key, player2, player3, player4.Key);
                if (this.playingDirection == DirectionType.RIGHT)
                {
                    if(BallPosition.y<10)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x + 10, 40)));
                    else if (BallPosition.y > 20 && BallPosition.y < 30)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x + 10, 40)));
                    else
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x + 10, 50-BallPosition.y)));
                }
                else
                {
                    if(BallPosition.y>40)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x - 10, 40)));
                    else if (BallPosition.y > 20 && BallPosition.y < 30)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x - 10, 40)));
                    else
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x - 10, 50-BallPosition.y)));
                }
                #endregion
                #endregion
            }
            else if (GetRelativePositionNew(BallPosition).x > 84 && GetRelativePositionNew(BallPosition).x < 93)
            {
                var bcp = playersByBallPosition.SkipPlayers(playerNumber_LeftWingDefender, playerNumber_RightWingDefender);
                if (this.allTeamPlayers[bcp.Key].dynamicState.HasBall)
                {
                    #region Shoot at the goal
                    if (this.playingDirection == DirectionType.RIGHT)
                        actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(100, 25)));
                    else
                        actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, new Position(0, 25)));
                    #endregion
                }
                else if (bcp.Value.PlayerPosition.distanceFrom(BallPosition) < 5.0)
                {
                    actions.Add(TakePosession(bcp.Key));
                }
                else
                {
                    actions.Add(MovePlayer(bcp.Key, BallPosition));
                }

                #region Other Players Movements
                var player2 = playerNumber_LeftWingDefender;
                actions.Add(MovePlayer(player2, GetDefaultPosition(player2)));

                var player3 = playerNumber_RightWingDefender;
                actions.Add(MovePlayer(player3, GetDefaultPosition(player3)));

                //next forward
                var player4 = playersByBallPosition.SkipPlayers(bcp.Key, player2, player3);
                if (this.playingDirection == DirectionType.RIGHT)
                {
                    if (BallPosition.y < 10)
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x - 10, 10)));
                    else
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x - 10, BallPosition.y)));
                }
                else
                {
                    if (BallPosition.y > 40)
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x + 10, 40)));
                    else
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x + 10, BallPosition.y)));
                }

                var player5 = playersByBallPosition.SkipPlayers(bcp.Key, player2, player3, player4.Key);
                if (this.playingDirection == DirectionType.RIGHT)
                {
                    if (BallPosition.y < 10)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x - 10, 40)));
                    else if (BallPosition.y > 20 && BallPosition.y < 30)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x - 10, 40)));
                    else
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x - 10, 50 - BallPosition.y)));
                }
                else
                {
                    if (BallPosition.y > 40)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x + 10, 40)));
                    else if (BallPosition.y > 20 && BallPosition.y < 30)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x + 10, 40)));
                    else
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x + 10, 50 - BallPosition.y)));
                }
                #endregion

            }
            else
            {
                var bcp = playersByBallPosition.SkipPlayers(playerNumber_LeftWingDefender, playerNumber_RightWingDefender);
                if (this.allTeamPlayers[bcp.Key].dynamicState.HasBall)
                {
                    #region Kick Smartly
                    var bestPassee = playersByBallPosition.SkipPlayers(playerNumber_LeftWingDefender, playerNumber_RightWingDefender, bcp.Key);
                    actions.Add(Kick(bcp.Key, bcp.Value.PlayerPosition, bestPassee.Value.PlayerPosition));
                    #endregion
                }
                else if (bcp.Value.PlayerPosition.distanceFrom(BallPosition) < 5.0)
                {
                    actions.Add(TakePosession(bcp.Key));
                }
                else
                {
                    actions.Add(MovePlayer(bcp.Key, BallPosition));
                }

                #region Other Players Movements
                var player2 = playerNumber_LeftWingDefender;
                actions.Add(MovePlayer(player2, GetDefaultPosition(player2)));

                var player3 = playerNumber_RightWingDefender;
                actions.Add(MovePlayer(player3, GetDefaultPosition(player3)));

                //next forward
                var player4 = playersByBallPosition.SkipPlayers(bcp.Key, player2, player3);
                if (this.playingDirection == DirectionType.RIGHT)
                {
                    if (BallPosition.y < 10)
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x - 10, 10)));
                    else
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x - 10, BallPosition.y)));
                }
                else
                {
                    if (BallPosition.y > 40)
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x + 10, 40)));
                    else
                        actions.Add(MovePlayer(player4.Key, new Position(BallPosition.x + 10, BallPosition.y)));
                }

                var player5 = playersByBallPosition.SkipPlayers(bcp.Key, player2, player3, player4.Key);
                if (this.playingDirection == DirectionType.RIGHT)
                {
                    if (BallPosition.y < 10)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x - 10, 40)));
                    else if (BallPosition.y > 20 && BallPosition.y < 30)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x - 10, 40)));
                    else
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x - 10, 50 - BallPosition.y)));
                }
                else
                {
                    if (BallPosition.y > 40)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x + 10, 40)));
                    else if (BallPosition.y > 20 && BallPosition.y < 30)
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x + 10, 40)));
                    else
                        actions.Add(MovePlayer(player5.Key, new Position(BallPosition.x + 10, 50 - BallPosition.y)));
                }
                #endregion
            }
            return actions;
        }

        private List<JSObject> GoalKeeperStrategy()
        {
            var actions = new List<JSObject>();
            #region GoalKeeperActions
            if (this.goalkeeper.dynamicState.hasBall)
            {
                if (new Position(this.allTeamPlayers[playerNumber_LeftWingDefender].dynamicState.position).distanceFrom(new Position(this.goalkeeper.dynamicState.position)) <
                            new Position(this.allTeamPlayers[playerNumber_RightWingDefender].dynamicState.position).distanceFrom(new Position(this.goalkeeper.dynamicState.position)))
                {
                    var destinationPlayerPosition = new Position(this.allTeamPlayers[this.playerNumber_LeftMidField].dynamicState.position);
                    actions.Add(GetActionsForPlayer(this.goalkeeperPlayerNumber, destinationPlayerPosition, ActionType.Kick, 60));
                }
                else
                {
                    var destinationPlayerPosition = new Position(this.allTeamPlayers[this.playerNumber_RightWingDefender].dynamicState.position);
                    actions.Add(GetActionsForPlayer(this.goalkeeperPlayerNumber, destinationPlayerPosition, ActionType.Kick, 60));
                }
            }
            else if (ballIsInGoalArea())
            {
                if (new Position(this.goalkeeper.dynamicState.position).distanceFrom(new Position(this.ball.position)) < 5.0)
                    actions.Add(GetActionsForPlayer(this.goalkeeperPlayerNumber, new Position(this.ball.position), ActionType.TakePossession, 60));
                else
                    actions.Add(GetActionsForPlayer(this.goalkeeperPlayerNumber, new Position(this.ball.position), ActionType.Move, 60));
            }
            else
            {
                var vector = new Vector(getGoalCentre(GoalType.OUR_GOAL), new Position(this.ball.position));
                var vector5m = vector.getScaledVector(5.0);
                var moveTo = getGoalCentre(GoalType.OUR_GOAL).getPositionPlusVector(vector5m);
                actions.Add(GetActionsForPlayer(this.goalkeeperPlayerNumber, moveTo, ActionType.Move, 60));
            }
            return actions;
            #endregion
        }
        #endregion

        #region Junk


        private Position GetRelativePosition(Position position)
        {
            if (this.playingDirection == DirectionType.RIGHT)
                position.x = 100 - position.x;
            return position;
        }

        private Position GetRelativePositionNew(Position position)
        {
            if (this.playingDirection == DirectionType.LEFT)
                position.x = 100 - position.x;
            return position;
        }
        private Dictionary<int, DynamicState> playerByPosition = new Dictionary<int, DynamicState>();

        public Dictionary<int, DynamicState> PlayerByPosition
        {
            get
            {
                if (playerByPosition.Count() != 0)
                    return playerByPosition;
                else
                {
                    Dictionary<int, DynamicState> playerDetails = new Dictionary<int, DynamicState>();

                    foreach (var playerNumber in this.allTeamPlayers.Keys)
                    {
                        var player = this.allTeamPlayers[playerNumber];

                        playerDetails.Add(playerNumber,
                            new DynamicState(new Position(player.dynamicState.position),
                                                new Position(player.dynamicState.position).distanceFrom(new Position(this.ball.position)),
                                                player.staticState.runningAbility,
                                                GetLocationByPosition(new Position(player.dynamicState.position))));
                    }
                    var playersByPosition = playerDetails.OrderBy(x => ((DynamicState)x.Value).Distance);

                    foreach (var player in playersByPosition)
                        playerByPosition.Add(player.Key, player.Value);
                    return playerByPosition;
                }

            }


        }

        private Dictionary<int, DynamicState> opposingPlayerByPosition = new Dictionary<int, DynamicState>();

        public Dictionary<int, DynamicState> OpposingPlayerByPosition
        {
            get
            {
                if (opposingPlayerByPosition.Count() != 0)
                    return opposingPlayerByPosition;
                else
                {
                    Dictionary<int, DynamicState> opposingPlayerDetails = new Dictionary<int, DynamicState>();

                    foreach (var playerNumber in this.allOpposingTeamPlayers.Keys)
                    {
                        var player = this.allOpposingTeamPlayers[playerNumber];

                        opposingPlayerDetails.Add(playerNumber,
                            new DynamicState(new Position(player.dynamicState.position),
                                                new Position(player.dynamicState.position).distanceFrom(new Position(this.ball.position)),
                                                player.staticState.runningAbility,
                                                GetLocationByPosition(new Position(player.dynamicState.position))));
                    }
                    var opposingPlayersByPosition = opposingPlayerDetails.OrderBy(x => ((DynamicState)x.Value).Distance);

                    foreach (var player in opposingPlayersByPosition)
                        opposingPlayerByPosition.Add(player.Key, player.Value);
                    return opposingPlayerByPosition;
                }

            }


        }

        private Tuple<int, int> GetBallLocationOld()
        {
            var ballPosition = this.ball.position;

            if (this.playingDirection == DirectionType.RIGHT)
            {
                if (ballPosition.x < 36 && ballPosition.y < 21)
                {
                    if (ballPosition.x < 10)
                        return new Tuple<int, int>(1, 1);
                    else if (ballPosition.x < 25 && ballPosition.y < 10)
                        return new Tuple<int, int>(1, 2);
                    else if (ballPosition.x < 25)
                        return new Tuple<int, int>(1, 3);
                    else if (ballPosition.y < 10)
                        return new Tuple<int, int>(1, 4);
                    else
                        return new Tuple<int, int>(1, 5);
                }
                else if (ballPosition.x < 36 && ballPosition.y < 31)
                {
                    if (ballPosition.x < 25)
                        return new Tuple<int, int>(2, 1);
                    else
                        return new Tuple<int, int>(2, 2);
                }
                else if (ballPosition.x < 36)
                {
                    if (ballPosition.x < 10)
                        return new Tuple<int, int>(3, 1);
                    else if (ballPosition.x < 25 && ballPosition.y > 40)
                        return new Tuple<int, int>(3, 2);
                    else if (ballPosition.x < 25)
                        return new Tuple<int, int>(3, 3);
                    else if (ballPosition.y > 40)
                        return new Tuple<int, int>(3, 4);
                    else
                        return new Tuple<int, int>(3, 5);
                }
                else if (ballPosition.x < 75 && ballPosition.y < 21)
                {
                    if (ballPosition.x < 45 && ballPosition.y < 10)
                        return new Tuple<int, int>(4, 1);
                    else if (ballPosition.x < 45)
                        return new Tuple<int, int>(4, 2);
                    else if (ballPosition.x < 60 && ballPosition.y < 10)
                        return new Tuple<int, int>(4, 3);
                    else if (ballPosition.x < 60)
                        return new Tuple<int, int>(4, 4);
                    else if (ballPosition.y < 10)
                        return new Tuple<int, int>(4, 5);
                    else
                        return new Tuple<int, int>(4, 5);
                }
                else if (ballPosition.x < 75 && ballPosition.y < 31)
                {
                    if (ballPosition.x < 45)
                        return new Tuple<int, int>(5, 1);
                    else if (ballPosition.x < 60)
                        return new Tuple<int, int>(5, 2);
                    else
                        return new Tuple<int, int>(5, 3);
                }
                else if (ballPosition.x < 75)
                {
                    if (ballPosition.x < 45 && ballPosition.y > 40)
                        return new Tuple<int, int>(6, 1);
                    else if (ballPosition.x < 45)
                        return new Tuple<int, int>(6, 2);
                    else if (ballPosition.x < 60 && ballPosition.y > 40)
                        return new Tuple<int, int>(6, 3);
                    else if (ballPosition.x < 60)
                        return new Tuple<int, int>(6, 4);
                    else if (ballPosition.y > 40)
                        return new Tuple<int, int>(6, 5);
                    else
                        return new Tuple<int, int>(6, 6);
                }
                else
                {
                    if (ballPosition.x < 90 && ballPosition.y < 10)
                        return new Tuple<int, int>(7, 1);
                    else if (ballPosition.x < 90 && ballPosition.y < 40)
                        return new Tuple<int, int>(7, 2);
                    else if (ballPosition.x < 90)
                        return new Tuple<int, int>(7, 3);
                    else if (ballPosition.y < 10)
                        return new Tuple<int, int>(7, 4);
                    else
                        return new Tuple<int, int>(7, 5);
                }
            }
            else
            {
                if (ballPosition.x < 26)
                {
                    if (ballPosition.x > 10 && ballPosition.y < 10)
                        return new Tuple<int, int>(7, 1);
                    else if (ballPosition.x > 10 && ballPosition.y < 40)
                        return new Tuple<int, int>(7, 2);
                    else if (ballPosition.x > 10)
                        return new Tuple<int, int>(7, 3);
                    else if (ballPosition.y < 10)
                        return new Tuple<int, int>(7, 4);
                    else
                        return new Tuple<int, int>(7, 5);
                }
                else if (ballPosition.x < 65 && ballPosition.y < 21)
                {
                    if (ballPosition.x > 55 && ballPosition.y < 10)
                        return new Tuple<int, int>(4, 1);
                    else if (ballPosition.x > 55)
                        return new Tuple<int, int>(4, 2);
                    else if (ballPosition.x > 40 && ballPosition.y < 10)
                        return new Tuple<int, int>(4, 3);
                    else if (ballPosition.x > 40)
                        return new Tuple<int, int>(4, 4);
                    else if (ballPosition.y < 10)
                        return new Tuple<int, int>(4, 5);
                    else
                        return new Tuple<int, int>(4, 5);
                }
                else if (ballPosition.x < 65 && ballPosition.y < 31)
                {
                    if (ballPosition.x > 55)
                        return new Tuple<int, int>(5, 1);
                    else if (ballPosition.x > 40)
                        return new Tuple<int, int>(5, 2);
                    else
                        return new Tuple<int, int>(5, 3);
                }
                else if (ballPosition.x < 65)
                {
                    if (ballPosition.x > 55 && ballPosition.y > 40)
                        return new Tuple<int, int>(6, 1);
                    else if (ballPosition.x > 55)
                        return new Tuple<int, int>(6, 2);
                    else if (ballPosition.x > 40 && ballPosition.y > 40)
                        return new Tuple<int, int>(6, 3);
                    else if (ballPosition.x > 40)
                        return new Tuple<int, int>(6, 4);
                    else if (ballPosition.y > 40)
                        return new Tuple<int, int>(6, 5);
                    else
                        return new Tuple<int, int>(6, 6);
                }
                else if (ballPosition.y < 21)
                {
                    if (ballPosition.x > 90)
                        return new Tuple<int, int>(1, 1);
                    else if (ballPosition.x > 75 && ballPosition.y < 10)
                        return new Tuple<int, int>(1, 2);
                    else if (ballPosition.x > 75)
                        return new Tuple<int, int>(1, 3);
                    else if (ballPosition.y < 10)
                        return new Tuple<int, int>(1, 4);
                    else
                        return new Tuple<int, int>(1, 5);
                }
                else if (ballPosition.y < 31)
                {
                    if (ballPosition.x > 75)
                        return new Tuple<int, int>(2, 1);
                    else
                        return new Tuple<int, int>(2, 2);
                }
                else
                {
                    if (ballPosition.x > 90)
                        return new Tuple<int, int>(3, 1);
                    else if (ballPosition.x > 75 && ballPosition.y > 40)
                        return new Tuple<int, int>(3, 2);
                    else if (ballPosition.x > 75)
                        return new Tuple<int, int>(3, 3);
                    else if (ballPosition.y > 40)
                        return new Tuple<int, int>(3, 4);
                    else
                        return new Tuple<int, int>(3, 5);
                }
            }
        }

        private bool ballIsInGoalArea()
        {
            var ballPosition = new Position(this.ball.position);
            var goalCentre = getGoalCentre(GoalType.OUR_GOAL);
            return ballPosition.distanceFrom(goalCentre) < this.pitch.goalAreaRadius;
        }

        private bool positionInGoalArea(Position p)
        {
            return p.distanceFrom(getGoalCentre(GoalType.OUR_GOAL)) < this.pitch.goalAreaRadius;
        }

        private int GetBallLocation()
        {
            var ballPosition = this.ball.position;
            return GetLocationByPosition(ballPosition);
        }

        private int GetLocationByPosition(Position fromPosition)
        {
            if (ballIsInGoalArea()) return 0;
            else if (fromPosition.x < 11 && fromPosition.y < 26) return 1;
            else if (fromPosition.x < 11) return 2;
            else if (fromPosition.x < 21 && fromPosition.y < 21) return 3;
            else if (fromPosition.x < 21 && fromPosition.y < 31) return 4;
            else if (fromPosition.x < 21 && fromPosition.y < 41) return 5;
            else if (fromPosition.x < 21 && fromPosition.y < 51) return 6;
            else if (fromPosition.x < 21) return 7;
            else if (fromPosition.x < 31 && fromPosition.y < 21) return 8;
            else if (fromPosition.x < 31 && fromPosition.y < 31) return 9;
            else if (fromPosition.x < 31 && fromPosition.y < 41) return 10;
            else if (fromPosition.x < 31 && fromPosition.y < 51) return 11;
            else if (fromPosition.x < 31) return 12;
            else if (fromPosition.x < 41 && fromPosition.y < 21) return 13;
            else if (fromPosition.x < 41 && fromPosition.y < 31) return 14;
            else if (fromPosition.x < 41 && fromPosition.y < 41) return 15;
            else if (fromPosition.x < 41 && fromPosition.y < 51) return 16;
            else if (fromPosition.x < 41) return 17;
            else if (fromPosition.x < 51 && fromPosition.y < 21) return 18;
            else if (fromPosition.x < 51 && fromPosition.y < 31) return 19;
            else if (fromPosition.x < 51 && fromPosition.y < 41) return 20;
            else if (fromPosition.x < 51 && fromPosition.y < 51) return 21;
            else if (fromPosition.x < 51) return 22;
            else if (fromPosition.x < 61 && fromPosition.y < 21) return 23;
            else if (fromPosition.x < 61 && fromPosition.y < 31) return 24;
            else if (fromPosition.x < 61 && fromPosition.y < 41) return 25;
            else if (fromPosition.x < 61 && fromPosition.y < 51) return 26;
            else if (fromPosition.x < 61) return 27;
            else if (fromPosition.x < 71 && fromPosition.y < 21) return 28;
            else if (fromPosition.x < 71 && fromPosition.y < 31) return 29;
            else if (fromPosition.x < 71 && fromPosition.y < 41) return 30;
            else if (fromPosition.x < 71 && fromPosition.y < 51) return 31;
            else if (fromPosition.x < 71) return 32;
            else if (fromPosition.x < 81 && fromPosition.y < 21) return 33;
            else if (fromPosition.x < 81 && fromPosition.y < 31) return 34;
            else if (fromPosition.x < 81 && fromPosition.y < 41) return 35;
            else if (fromPosition.x < 81 && fromPosition.y < 51) return 36;
            else if (fromPosition.x < 81) return 37;
            else if (fromPosition.x < 91 && fromPosition.y < 21) return 38;
            else if (fromPosition.x < 91 && fromPosition.y < 31) return 39;
            else if (fromPosition.x < 91 && fromPosition.y < 41) return 40;
            else if (fromPosition.x < 91 && fromPosition.y < 51) return 41;
            else if (fromPosition.x < 91) return 42;
            else if (fromPosition.x < 95 && fromPosition.y < 25) return 43;
            else if (fromPosition.x < 95 && fromPosition.y < 25) return 44;
            else if (fromPosition.y < 25) return 45;
            else return 46;
        }











        public int ClosestDefender()
        {
            var closestDefender = from s in PlayerByPosition where s.Key == 0 || s.Key == 1 select s;
            return closestDefender.First().Key;
        }




        public int SecondDefender()
        {
            var secondDefender = from s in PlayerByPosition where s.Key == 0 || s.Key == 1 select s;
            return secondDefender.Skip(1).First().Key;
        }
        #endregion
        #endregion

    }

    #region support class
    public class DynamicState
    {
        public Position PlayerPosition;
        public double Distance;
        public double RunningAbility;
        public int Location;
        public Position DestinationPosition;

        public DynamicState(Position playerPosition, double distance, double runningAbility, int location)
        {
            PlayerPosition = playerPosition;
            Distance = distance;
            RunningAbility = runningAbility;
            Location = location;
            DestinationPosition = new Position(0, 0);
        }

        public DynamicState(Position playerPosition, double distance, double runningAbility, int location, Position destinationPosition)
        {
            PlayerPosition = playerPosition;
            Distance = distance;
            RunningAbility = runningAbility;
            Location = location;
            DestinationPosition = destinationPosition;
        }
    }

    public static class ExtensionsClass
    {
        public static Position PlayersPositionAtLocation(this Dictionary<int, DynamicState> dictionary, int location1, int location2 = 0, int location3 = 0, int location4 = 0, int location5 = 0, int location6 = 0, int location7 = 0, int location8 = 0, int location9 = 0)
        {
            var player = from s in dictionary
                         where (
                            (s.Value.Location == location1)
                         || (s.Value.Location == location2)
                         || (s.Value.Location == location3)
                         || (s.Value.Location == location4)
                         || (s.Value.Location == location5)
                         || (s.Value.Location == location6)
                         || (s.Value.Location == location7)
                         || (s.Value.Location == location8)
                         || (s.Value.Location == location9))
                         select s;
            if (player.Count() == 0)
                return null;
            else
                return player.First().Value.PlayerPosition;
        }

        public static bool HasPlayerAtLocation(this Dictionary<int, DynamicState> dictionary, int location1, int location2 = 0, int location3 = 0, int location4 = 0, int location5 = 0, int location6 = 0, int location7 = 0, int location8 = 0, int location9 = 0)
        {
            var player = from s in dictionary
                         where (
                            (s.Value.Location == location1)
                         || (s.Value.Location == location2)
                         || (s.Value.Location == location3)
                         || (s.Value.Location == location4)
                         || (s.Value.Location == location5)
                         || (s.Value.Location == location6)
                         || (s.Value.Location == location7)
                         || (s.Value.Location == location8)
                         || (s.Value.Location == location9))
                         select s;
            if (player.Count() == 0)
                return true;
            else
                return false;
        }

        public static KeyValuePair<int, DynamicState> Second(this Dictionary<int, DynamicState> dictionary)
        {
            return dictionary.Skip(1).First();
        }

        public static KeyValuePair<int, DynamicState> Third(this Dictionary<int, DynamicState> dictionary)
        {
            return dictionary.Skip(2).First();
        }

        public static bool In(this int value, int location1, int location2 = 0, int location3 = 0, int location4 = 0, int location5 = 0, int location6 = 0, int location7 = 0, int location8 = 0, int location9 = 0)
        {
            if ((value == location1) || (value == location2) || (value == location3) || (value == location4) || (value == location5) || (value == location6) ||
                (value == location7) || (value == location8) || (value == location9))
            {
                return true;
            }
            else
                return false;
        }

        public static bool HasPlayerAtLocation(this KeyValuePair<int, DynamicState> pair, int location1, int location2 = 0, int location3 = 0, int location4 = 0, int location5 = 0, int location6 = 0, int location7 = 0, int location8 = 0, int location9 = 0)
        {
            if ((pair.Value.Location == location1) || (pair.Value.Location == location2) || (pair.Value.Location == location3)
                || (pair.Value.Location == location4) || (pair.Value.Location == location5) || (pair.Value.Location == location6)
                || (pair.Value.Location == location7) || (pair.Value.Location == location8) || (pair.Value.Location == location9))
            {
                return true;
            }
            else
                return false;
        }

        public static Position PlayersPositionAtLocation(this KeyValuePair<int, DynamicState> pair, int location1, int location2 = 0, int location3 = 0, int location4 = 0, int location5 = 0, int location6 = 0, int location7 = 0, int location8 = 0, int location9 = 0)
        {
            if ((pair.Value.Location == location1) || (pair.Value.Location == location2) || (pair.Value.Location == location3)
                || (pair.Value.Location == location4) || (pair.Value.Location == location5) || (pair.Value.Location == location6)
                || (pair.Value.Location == location7) || (pair.Value.Location == location8) || (pair.Value.Location == location9))
            {
                return pair.Value.PlayerPosition;
            }
            else
                return null;
        }

        public static KeyValuePair<int, DynamicState> SkipPlayers(this Dictionary<int, DynamicState> dict, int skipPlayer1, int skipPlayer2 = -1, int skipPlayer3 = -1, int skipPlayer4 = -1, int skipPlayer5 = -1, int skipPlayer6 = -1)
        {
            var returnPlayers = from s in dict
                                where s.Key != skipPlayer1
                                && s.Key != skipPlayer2
                                && s.Key != skipPlayer3
                                && s.Key != skipPlayer4
                                && s.Key != skipPlayer5
                                && s.Key != skipPlayer6
                                select s;
            return returnPlayers.First();

        }

        public static KeyValuePair<int, DynamicState> GetFirstOppositionByArea(this Dictionary<int, DynamicState> dict, Position position, int skipPlayer1 = -1, int skipPlayer2 = -1, int skipPlayer3 = -1, int skipPlayer4 = -1)
        {
            var returnPlayers = from s in dict
                                where s.Key != skipPlayer1
                                && s.Key != skipPlayer2
                                && s.Key != skipPlayer3
                                && s.Key != skipPlayer4
                                && s.Value.PlayerPosition.x < position.x
                                select s;
            return returnPlayers.First();

        }

        public static bool IfAnyOppositionInSpecifiedArea(this Dictionary<int, DynamicState> dict, Position position, int skipPlayer1 = -1, int skipPlayer2 = -1, int skipPlayer3 = -1, int skipPlayer4 = -1)
        {
            var returnPlayers = from s in dict
                                where s.Key != skipPlayer1
                                && s.Key != skipPlayer2
                                && s.Key != skipPlayer3
                                && s.Key != skipPlayer4
                                && s.Value.PlayerPosition.x < position.x
                                select s;
            return returnPlayers.Count() != 0;

        }

        public static bool IsBetween(this double value, double value1, double value2)
        {
            if (value1 < value2)
                return ((value > value1) && (value < value2));
            else
                return ((value > value2) && (value < value1));
        }
    }
    #endregion
}

