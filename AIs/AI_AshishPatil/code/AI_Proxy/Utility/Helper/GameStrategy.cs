using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Utility.DTO;

namespace Utility.Helper
{
    public class GameStrategy : IFootBalStrategy
    {
        protected GameStartDTO fieldInfo;
        protected GameUpdateDTO gameState;
        protected TeamInfoPlayer goalKeeper;
        protected Position ballprevPosition;
        protected Position ballcurPosition;
        protected Position goalPostTop;
        protected Position goalPostBottom;
        protected Position goalPostTopOpponent;
        protected Position goalPostBottomOpponent;
        protected DirectionType goalPostDirection;
        protected Player ourPlayerHavingBall;
        protected Position goalPostMid;
        protected Position goalPostOppositionMid;
        protected TeamInfoDTO teamInfo;
        protected string direction;
        protected TeamInfo ourTeam;
        protected List<PlayerAbility> playerAbilities;
        protected TeamInfo opponentTeam;
        protected Player targetPlayer = null;

        protected KickOffResponseDTO ProcessKickOff(KickOffRequestDTO request)
        {
            if (request.eventType == null)
                return null;

            InitializeGameVariables(request);
            var k = SetPlayerPositionAccordingToAbilities(request);
            k.requestType = "KICKOFF";
            return k;

        }

        private KickOffResponseDTO SetPlayerPositionAccordingToAbilities(KickOffRequestDTO request)
        {
            var k = new KickOffResponseDTO();
            double initialPosition = 90;

            k.players = new KickOffPlayer[teamInfo.players.Length];
            var dirAngle = 270;

            if (goalPostDirection == DirectionType.RIGHT)
            {
                dirAngle = 90;
                initialPosition = 10;
            }

            var gkPos = new Position { x = goalPostBottom.x, y = fieldInfo.pitch.height / 2 };
            k.players[0] = new KickOffPlayer()
            {
                direction = dirAngle,
                playerNumber = goalKeeper.playerNumber,
                position = gkPos
            };

            var pos = GetHotlistedGridCells(new Position { x = initialPosition, y = fieldInfo.pitch.height / 2 }, goalPostDirection);
            var remainingNumbers = playerAbilities
                     .OrderByDescending(p => p.runningAbility)
                     .ThenByDescending(p => p.ballControlAbility)
                     .ThenByDescending(p => p.kickingAbility)
                     .ThenByDescending(p => p.tacklingAbility)
                     .Where(p => p.playerNumber != goalKeeper.playerNumber)
                     .Select(p => p.playerNumber).ToArray();

            if (goalPostDirection == DirectionType.LEFT)
            {
                pos = pos.OrderByDescending(p => p.x).ToList();
            }
            else
            {
                pos = pos.OrderBy(p => p.x).ToList();
            }

            for (int i = 1; i < k.players.Count(); i++)
            {
                Position position;
                if (teamInfo.teamNumber == request.teamKickingOff && i == 1)
                {
                    position = new Position { x = fieldInfo.pitch.width / 2, y = fieldInfo.pitch.height / 2 };
                }
                {
                    position = pos[i - 1];
                }

                if (targetPlayer != null && remainingNumbers[i - 1] == targetPlayer.staticState.playerNumber)
                {
                    k.players[i] = new KickOffPlayer()
                    {
                        direction = dirAngle,
                        playerNumber = remainingNumbers[i - 1],
                        position = targetPlayer.dynamicState.position
                    };
                }
                else
                {
                    k.players[i] = new KickOffPlayer()
                    {
                        direction = dirAngle,
                        playerNumber = remainingNumbers[i - 1],
                        position = position
                    };
                }
            }
            targetPlayer = null;
            return k;
        }

        protected void ProcessTeamInfo(TeamInfoDTO info)
        {
            this.teamInfo = info;
            goalKeeper = this.teamInfo.players.Where(p => p.playerType == "G").First();
        }

        protected void ProcessGameStart(GameStartDTO request)
        {
            this.fieldInfo = request;
        }

        protected void ProcessHalfTime()
        {

        }

        protected PlayResponseDTO ProcessPlay()
        {
            var actions = new List<DTO.Action>();
            var response = new PlayResponseDTO() { requestType = "PLAY" };
            if (ourPlayerHavingBall != null)
            {
                MoveWithBallOrKick(ourTeam, actions);
            }
            else
            {
                ChaseOrCollectBall(ourTeam, actions);
            }

            MoveRemainingPlayers(ourTeam, actions);

            response.actions = actions.ToArray();
            response.requestType = "PLAY";
            return response;
        }

        protected ConfigureAbilitiesResponseDTO ProcessConfigureAbilities(ConfigureAbilitiesRequestDTO request)
        {
            var response = new ConfigureAbilitiesResponseDTO();
            response.players = new PlayerAbility[teamInfo.players.Length];
            var normalPlayerNumbers = teamInfo.players.Where(p => p.playerType != "G")
                .Select(p => p.playerNumber).ToList();

            var runningAbilities = new float[] { 90, 90, 80, 80, 30, 30 };
            var kickingAbility = new float[] { 30, 86, 86, 86, 56, 56 };
            var ballControlAbilities = new float[] { 96, 96, 76, 56, 56, 20 };
            var tacklingAbilities = new float[] { 96, 96, 76, 56, 56, 20 };


            response.players[0] = new PlayerAbility()
            {
                playerNumber = goalKeeper.playerNumber,
                ballControlAbility = ballControlAbilities[0],
                runningAbility = runningAbilities[0],
                tacklingAbility = tacklingAbilities[0],
                kickingAbility = runningAbilities[0]
            };

            for (int i = 1; i < 6; i++)
            {
                response.players[i] = new PlayerAbility()
                {
                    playerNumber = normalPlayerNumbers[i - 1],
                    ballControlAbility = ballControlAbilities[i],
                    runningAbility = runningAbilities[i],
                    tacklingAbility = tacklingAbilities[i],
                    kickingAbility = runningAbilities[i]
                };
            }

            response.requestType = "CONFIGURE_ABILITIES";
            playerAbilities = response.players.ToList();
            return response;

        }

        protected void ProcessGoalRequest(GoalRequestdTO request)
        {
            //  return null;
        }

        protected void ProcessGameEvent(GameUpdateDTO update)
        {
            gameState = update;
            ballprevPosition = ballcurPosition;
            ballcurPosition = gameState.ball.position;
            if (ballprevPosition == null)
            { ballprevPosition = ballcurPosition; }

            if (teamInfo.teamNumber == 1)
            {
                ourTeam = gameState.team1;
                ourPlayerHavingBall = gameState.team1.players.Where(p => p.dynamicState.hasBall).FirstOrDefault();
                opponentTeam = gameState.team2;
            }
            else
            {
                ourTeam = gameState.team2;
                ourPlayerHavingBall = gameState.team2.players.Where(p => p.dynamicState.hasBall).FirstOrDefault();
                opponentTeam = gameState.team1;
            }
        }

        public string Play(string input)
        {
            var request = GameHelper.GetRequestType(input);
            if (request == "GAME_START")
            {
                this.ProcessGameStart(JsonConvert.DeserializeObject<GameStartDTO>(input));
            }
            else if (request == "CONFIGURE_ABILITIES")
            {
                var dto = JsonConvert.DeserializeObject<ConfigureAbilitiesRequestDTO>(input);
                var response = this.ProcessConfigureAbilities(dto);
                return JsonConvert.SerializeObject(response);
            }
            else if (request == "KICKOFF")
            {
                var dto = JsonConvert.DeserializeObject<KickOffRequestDTO>(input);
                var response = this.ProcessKickOff(dto);
                if (response != null)
                {
                    return JsonConvert.SerializeObject(response);
                }
            }
            else if (request == "START_OF_TURN")
            {
                ProcessGameEvent(JsonConvert.DeserializeObject<GameUpdateDTO>(input));
            }
            else if (request == "PLAY")
            {
                var response = ProcessPlay();
                return JsonConvert.SerializeObject(response);
            }
            else if (request == "HALF_TIME")
            {
                this.ProcessHalfTime();
            }
            else if (request == "TEAM_INFO")
            {
                this.ProcessTeamInfo(JsonConvert.DeserializeObject<TeamInfoDTO>(input));
            }

            return null;
        }

        private void InitializeGameVariables(KickOffRequestDTO request)
        {
            Position circleCenter, circleCenterOpposition;
            var xcor = fieldInfo.pitch.width;
            if (teamInfo.teamNumber == 1)
            { direction = request.team1.direction; }
            else
            { direction = request.team2.direction; }

            if (direction == "RIGHT")
            {
                goalPostDirection = DirectionType.RIGHT;
                goalPostTopOpponent = new Position { x = xcor, y = fieldInfo.pitch.goalY1 };
                goalPostBottomOpponent = new Position { x = xcor, y = fieldInfo.pitch.goalY2 };
                goalPostTop = new Position { x = 100 - xcor, y = fieldInfo.pitch.goalY1 };
                goalPostBottom = new Position { x = 100 - xcor, y = fieldInfo.pitch.goalY2 };
                circleCenter = new Position { x = 0, y = fieldInfo.pitch.height / 2 };
                circleCenterOpposition = new Position { x = fieldInfo.pitch.width, y = circleCenter.y };
            }
            else
            {
                goalPostDirection = DirectionType.LEFT;
                goalPostTop = new Position { x = xcor, y = fieldInfo.pitch.goalY1 };
                goalPostBottom = new Position { x = xcor, y = fieldInfo.pitch.goalY2 };
                goalPostTopOpponent = new Position { x = 100 - xcor, y = fieldInfo.pitch.goalY1 };
                goalPostBottomOpponent = new Position { x = 100 - xcor, y = fieldInfo.pitch.goalY2 };
                circleCenter = new Position { x = fieldInfo.pitch.width, y = fieldInfo.pitch.height / 2 };
                circleCenterOpposition = new Position { x = 0, y = circleCenter.y };
            }

            goalPostMid = circleCenter;
            goalPostOppositionMid = circleCenterOpposition;
        }

        private void ChaseOrCollectBall(TeamInfo ourTeam, List<DTO.Action> actions)
        {
            Player closestPlayer;
            if (GameHelper.IsPositionInGoalCircle(gameState.ball.position, goalPostMid, fieldInfo.pitch.goalAreaRadius)
                 && gameState.ball.speed == 0)
            {
                closestPlayer = ourTeam.players.Where(p => p.staticState.playerType == "G").First();
                actions.Add(new DTO.Action { action = "MOVE", destination = gameState.ball.position, playerNumber = closestPlayer.staticState.playerNumber, speed = 100 });
            }
            else
            {
                closestPlayer = GetClosestPlayer(gameState.ball.position, ourTeam.players.ToList(), false);
            }

            if (ShouldAskForPossession(gameState.ball.position, closestPlayer))
            {
                actions.Add(new DTO.Action { action = "TAKE_POSSESSION", playerNumber = closestPlayer.staticState.playerNumber });
            }
            else
            {
                if (closestPlayer.staticState.playerType != "G")
                {
                    actions.Add(new DTO.Action { action = "MOVE", destination = gameState.ball.position, playerNumber = closestPlayer.staticState.playerNumber, speed = 100 });
                }
                else
                {
                    closestPlayer = GetClosestPlayer(gameState.ball.position, ourTeam.players.ToList(), true);
                    actions.Add(new DTO.Action { action = "MOVE", destination = gameState.ball.position, playerNumber = closestPlayer.staticState.playerNumber, speed = 100 });
                }
            }
        }

        private void MoveWithBallOrKick(TeamInfo ourTeam, List<DTO.Action> actions)
        {
            Player closestPlayer;
            if (GameHelper.GetDistance(ourPlayerHavingBall.dynamicState.position, goalPostOppositionMid) < fieldInfo.pitch.goalAreaRadius + 5)
            {
                actions.Add(new DTO.Action { action = "KICK", destination = goalPostOppositionMid, playerNumber = ourPlayerHavingBall.staticState.playerNumber, speed = 100 });
            }
            else
            {
                closestPlayer = GetClosestPlayerToPass(ourPlayerHavingBall.dynamicState.position, ourTeam.players.Where(p => p.staticState.playerNumber != ourPlayerHavingBall.staticState.playerNumber).ToList(), goalPostDirection, true);

                if (ourPlayerHavingBall.staticState.playerType == "G" || ShouldKick(ourPlayerHavingBall.dynamicState.position, opponentTeam, closestPlayer, goalPostDirection))
                {
                    int kickSpeed = this.GetKickSpeed(closestPlayer.dynamicState.position, ourPlayerHavingBall.dynamicState.position);
                    if (ourPlayerHavingBall.staticState.playerType == "G") kickSpeed = 100;
                    Position kickPosition = closestPlayer.dynamicState.position;
                    actions.Add(new DTO.Action { action = "KICK", destination = kickPosition, playerNumber = ourPlayerHavingBall.staticState.playerNumber, speed = kickSpeed });
                    targetPlayer = closestPlayer;
                }
                else
                {
                    var d = goalPostTopOpponent;
                    d.y = fieldInfo.pitch.goalY1 + ((fieldInfo.pitch.goalY2 - fieldInfo.pitch.goalY1) / 2);
                    actions.Add(new DTO.Action { action = "MOVE", destination = d, playerNumber = ourPlayerHavingBall.staticState.playerNumber, speed = double.Parse(ConfigurationManager.AppSettings["SpeedMoveWithBall"]) });
                }
            }

        }

        private void MoveRemainingPlayers(TeamInfo ourTeam, List<DTO.Action> actions)
        {
            var gkPos = GetGoalKeeperYPosition(ballcurPosition,
                      ballprevPosition, goalPostTop, goalPostBottom, goalPostMid);

            var placements = GetHotlistedGridCells(gameState.ball.position, goalPostDirection);
            var placmentPositions = placements.ToList();
            int i = 0;

            var remainingNumbers = playerAbilities
                    .OrderByDescending(p => p.runningAbility)
                    .ThenByDescending(p => p.ballControlAbility)
                    .ThenByDescending(p => p.kickingAbility)
                    .ThenByDescending(p => p.tacklingAbility)
                    .Select(p => p.playerNumber).ToList();

            if (goalPostDirection == DirectionType.LEFT)
            {
                placmentPositions = placmentPositions.OrderByDescending(p => p.x).ToList();
            }
            else
            {
                placmentPositions = placmentPositions.OrderBy(p => p.x).ToList();
            }

            foreach (var index in remainingNumbers)
            {
                var p = ourTeam.players.Single(x => x.staticState.playerNumber == index);
                if (actions.FirstOrDefault(a => a.playerNumber == p.staticState.playerNumber) == null)
                {
                    if (p.staticState.playerType == "G")
                    {
                        var goalKeeper = this.goalKeeper;
                        actions.Add(new DTO.Action { action = "MOVE", destination = gkPos, playerNumber = goalKeeper.playerNumber, speed = 100 });
                    }
                    else
                    {
                        //var closestPoint = GameHelper.GetClosestPosition(p.dynamicState.position, placmentPositions);
                        actions.Add(new DTO.Action
                        {
                            action = "MOVE",
                            destination = placmentPositions[i],
                            playerNumber = p.staticState.playerNumber,
                            speed = 100
                        });
                        //placmentPositions.Remove(closestPoint);
                    }
                    i++;
                }
            }
        }

        private bool ShouldAskForPossession(Position ballPosition, Player p)
        {
            if (GameHelper.GetDistance(ballPosition, p.dynamicState.position) < 5.00)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Position GetGoalKeeperYPosition(Position ball1, Position ball2, Position goalPostTop, Position goalPostBottom,
            Position goalPostMid)
        {
            var t = GameHelper.GetIntersectionPoint(ball1, ball2, goalPostTop, goalPostBottom);

            if (t == null) return goalPostMid;
            double gkYPos;
            if (t.y < goalPostTop.y)
            {
                gkYPos = goalPostTop.y;
            }
            else if (t.y > goalPostBottom.y)
            {
                gkYPos = goalPostBottom.y;
            }
            else
            {
                gkYPos = t.y;
            }

            return new Position() { x = goalPostTop.x, y = gkYPos };
        }

        private bool ShouldKick(Position playerWithBallPosition, TeamInfo opposition, Player closestOurPlayer, DirectionType goalPostDirection)
        {
            double oldDistance = 1000;
            foreach (var p in opposition.players)
            {
                var distance = GameHelper.GetDistance(playerWithBallPosition, p.dynamicState.position);

                if (distance < oldDistance)
                    oldDistance = distance;

            }

            if (oldDistance < Int32.Parse(ConfigurationManager.AppSettings["ClosestOpponentDistanceForKick"]))
            {
                if (goalPostDirection == DirectionType.LEFT)
                {
                    if (closestOurPlayer.dynamicState.position.x < playerWithBallPosition.x && GameHelper.GetDistance(closestOurPlayer.dynamicState.position, playerWithBallPosition) > Int32.Parse(ConfigurationManager.AppSettings["ClosestPlayerToPassMinDistance"]))
                        return true;
                }
                else
                {
                    if (closestOurPlayer.dynamicState.position.x > playerWithBallPosition.x && GameHelper.GetDistance(closestOurPlayer.dynamicState.position, playerWithBallPosition) > Int32.Parse(ConfigurationManager.AppSettings["ClosestPlayerToPassMinDistance"]))
                        return true;
                }

            }

            return false;
        }

        private Player GetClosestPlayer(Position p, List<Player> players, bool excludeGoalKeeper = false)
        {
            Dictionary<Player, double> distances = new Dictionary<Player, double>();
            foreach (var player in players)
            {
                if (excludeGoalKeeper && player.staticState.playerType == "G")
                {
                    continue;
                }
                distances.Add(player, GameHelper.GetDistance(player.dynamicState.position, p));

            }

            var x = distances.Min(m => m.Value);
            return distances.Where(d => d.Value == x).First().Key;
        }

        private Player GetClosestPlayerToPass(Position p, List<Player> players, DirectionType goalPostDirection, bool excludeGoalKeeper = false)
        {
            Player closest = null;
            if (excludeGoalKeeper)
            {
                var gk = players.Where(t => t.staticState.playerType == "G").FirstOrDefault();
                if (gk != null)
                    players.Remove(gk);
            }

            var playersTowardsGoal = players.Where(t =>
                (goalPostDirection == DirectionType.LEFT && t.dynamicState.position.x < p.x)
                    ||
                (goalPostDirection == DirectionType.RIGHT && t.dynamicState.position.x > p.x)).ToList();

            if (playersTowardsGoal != null && playersTowardsGoal.Count() > 0 && playersTowardsGoal.Count < players.Count)
            {
                closest = GetClosestPlayer(p, playersTowardsGoal, true);
                if (GameHelper.GetDistance(p, closest.dynamicState.position) < double.Parse(ConfigurationManager.AppSettings["ClosetPlayerMaxDistance"]))
                { return closest; }
                // players.RemoveAll(t => playersTowardsGoal.Contains(t));

            }

            return GetClosestPlayer(p, players, excludeGoalKeeper);



        }

        private List<Position> GetHotlistedGridCells(Position selectedPosition, DirectionType goalPostDirection)
        {
            Random r = new Random();
            var list = new List<Position>();
            var cell = selectedPosition;
            double x1 = 0, x2 = 0, x3 = 0;

            var Y1 = (cell.y + 15) > 40 ? 40 : (cell.y + 15);
            var Y2 = (cell.y - 20) < 0 ? 10 : (cell.y - 20);
            var Y3 = (cell.y - 15) < 0 ? 10 : (cell.y - 15);

            if (goalPostDirection == DirectionType.LEFT)
            {
                if (cell.x > 30)
                {
                    list.Add(new Position { x = 70, y = cell.y });
                }

                // list.Add(new GridCell { X = cell.X, Y = cell.Y });
                x1 = cell.x - Int32.Parse(ConfigurationManager.AppSettings["x1PlayerPos"]);
                x2 = cell.x - Int32.Parse(ConfigurationManager.AppSettings["x2PlayerPos"]);
                x3 = cell.x - Int32.Parse(ConfigurationManager.AppSettings["x3PlayerPos"]);

                if (x1 < 10) x1 = 30;
                if (x2 < 10) x2 = 20;
                if (x3 < 10) x3 = 20;
            }
            else
            {
                if (cell.x < 70)
                {
                    list.Add(new Position { x = cell.x, y = cell.y });
                }
                //list.Add(new GridCell { X = cell.X, Y = cell.Y });
                x1 = cell.x + Int32.Parse(ConfigurationManager.AppSettings["x1PlayerPos"]); ;
                x2 = cell.x + Int32.Parse(ConfigurationManager.AppSettings["x2PlayerPos"]);
                x3 = cell.x + Int32.Parse(ConfigurationManager.AppSettings["x3PlayerPos"]);
                if (x1 > 90) x1 = 70;
                if (x2 > 90) x2 = 80;
                if (x3 > 90) x3 = 80;
            }

            list.Add(new Position() { x = x1, y = Y1 });
            list.Add(new Position() { x = x1, y = Y3 });
            list.Add(new Position() { x = x3, y = Y1 });
            list.Add(new Position() { x = x3, y = Y3 });
            list.Add(new Position() { x = x2, y = Y2 });

            return list;
        }

        private List<Position> GetHotlistedGridCellsB(Position selectedPosition, DirectionType goalPostDirection)
        {
            var list = new List<Position>();
            if (goalPostDirection == DirectionType.RIGHT)
            {
                list.Add(new Position() { x = 80, y = 10 });
                list.Add(new Position() { x = 60, y = 25 });
                list.Add(new Position() { x = 80, y = 40 });
                list.Add(new Position() { x = 25, y = 15 });
                list.Add(new Position() { x = 25, y = 25 });
            }
            else
            {
                list.Add(new Position() { x = 30, y = 10 });
                list.Add(new Position() { x = 15, y = 25 });
                list.Add(new Position() { x = 30, y = 40 });
                list.Add(new Position() { x = 80, y = 15 });
                list.Add(new Position() { x = 80, y = 35 });
            }


            return list;
        }

        private int GetKickSpeed(Position target, Position yourPosition)
        {
            var distance = GameHelper.GetDistance(target, yourPosition);

            int speed = Int32.Parse(ConfigurationManager.AppSettings["KickSpeedConstant"]) + Convert.ToInt32(double.Parse(ConfigurationManager.AppSettings["KickSpeedTopUpMultiplier"]) * distance);

            if (speed > 100)
            {
                return 100;
            }
            else
            {
                return speed;
            }

        }


    }
}
