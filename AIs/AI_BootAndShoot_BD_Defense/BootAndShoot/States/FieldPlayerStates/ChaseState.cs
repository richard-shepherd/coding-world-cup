using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class ChaseState : State<FieldPlayer>
    {
        #region Singlton
        private static ChaseState m_instance;
        public static ChaseState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ChaseState();
                }
                return m_instance;
            }
        }
        private ChaseState() { }

        #endregion

        public override bool Enter(FieldPlayer player)
        {
            Logger.log("Player " + player.id + " enters chase state", Logger.LogLevel.INFO);
            if (player.action == null)
            {
                return true;
            }
            return false;

        }


        public override void Execute(FieldPlayer player)
        {
            //if the ball is within kicking range the player changes state to KickBall.
            if (player.hasBall)
            {
                player.stateMachine.ChangeState(KickState.instance);
                return;
            }
            Position ballPosition = player.Ball().position;
            //if the player is the closest player to the ball then he should keep
            //chasing it
            if (player.IsClosestTeamMemberToBall() && (player.team.receivingPlayer == null || player.team.receivingPlayer == player) && !player.team.BallStopPositionIsInGoalArea())
            {
                double distance = player.position.distanceFrom(ballPosition);
                if (distance <= 1.0)
                {
                    player.action = new Response(player.playerNumber, Response.Action.TakePossession);

                    return;
                }
                player.action = new Response();
                player.action.MoveOrTurn(player, ballPosition);
                return;
            }

            //if the player is not closest to the ball anymore, he should return back
            //to his home region and wait for another opportunity
            player.stateMachine.ChangeState(ReturnState.instance);
        }

        public override void Exit(FieldPlayer player) 
        {
            Logger.log("Player " + player.id + " exits chase state", Logger.LogLevel.INFO);
        }

        public override bool OnMessage(FieldPlayer player, Telegram t) { return false; }

    }
}
