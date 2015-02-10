using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class DribbleState : State<FieldPlayer>
    {
        #region Singlton
        private static DribbleState m_instance;
        public static DribbleState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new DribbleState();
                }
                return m_instance;
            }
        }
        private DribbleState() { }

        #endregion

        public override bool Enter(FieldPlayer player)
        {
            //let the team know this player is controlling
            player.team.SetControllingPlayer(player);

            Logger.log("Player " + player.id + " enters dribble state", Logger.LogLevel.INFO);

            target = getTarget(player);
            if (player.action == null)
            {
                return true;
            }
            return false;
        }

        public override void Execute(FieldPlayer player)
        {
            if (!player.hasBall)
            {
                player.stateMachine.ChangeState(ChaseState.instance);
                return;
            }
            if (player.position.distanceFrom(target) <= 17.0)
            {
                player.action = new Response();
                player.action.ShootOrTurn(player, target);
            }
            else
            {
                player.action = new Response();
                target = findSafestPosition(player, 5.0);
                double speed = Math.Sqrt(2 * 10 * player.position.distanceFrom(target));
                double speedPercentage = (speed / Ball.maxSpeed) * 100;
                player.action.KickOrTurn(player, target, speedPercentage);
            }

            //the player has kicked the ball so he must now change state to follow it
            player.stateMachine.ChangeState(ChaseState.instance);
        }

        public override void Exit(FieldPlayer player) 
        {
            Logger.log("Player " + player.id + " exits dribble state", Logger.LogLevel.INFO);
            //if (player.IsControllingPlayer())
            //{
            //    player.team.SetControllingPlayer(null);
            //}
        }

        public override bool OnMessage(FieldPlayer player, Telegram t) { return false; }

        private Position getTarget(FieldPlayer player)
        {
            Player opponentGoalKeeper = player.team.Opponents.GetKeeper();
            var shootAt = player.team.getGoalCentre(Team.GoalType.THEIR_GOAL);
            if (opponentGoalKeeper != null)
            {
                double maxDistance = 0;
                Position opponentGoalKeeperPosition = new Position(opponentGoalKeeper.position);
                for (int i = 21; i < 30; i++)
                {
                    var shootPosition = new Position(shootAt.x, i);
                    if (opponentGoalKeeperPosition.distanceFrom(shootPosition) > maxDistance)
                    {
                        maxDistance = opponentGoalKeeperPosition.distanceFrom(shootPosition);
                        shootAt = shootPosition;
                    }
                }

            }
            return shootAt;
        }
        private Position findSafestPosition(FieldPlayer player, double r)
        {
            Position safestPosition = new Position();
            Position bestSafestPosition = null;
            Position p = player.position;
            double distance, maxDistance = 0;
            for(int i = 0; i < 360 ; i++)
            {
                safestPosition.x = Math.Round(r * Math.Cos(i) + p.x,3);
                safestPosition.y = Math.Round(r * Math.Sin(i) + p.y,3);
                distance = player.closestOpponentPlayer.position.distanceFrom(safestPosition);
                if (distance > maxDistance && !player.team.PositionIsInGoalArea(safestPosition) && player.Pitch().playingArea.Inside(safestPosition.ToVector()))
                {
                    maxDistance = distance;
                    bestSafestPosition = safestPosition;
                }
            }
            return bestSafestPosition;
        }

        public Position target { get; set; }
    }
    

}
