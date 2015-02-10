using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot.States.FieldPlayerStates
{

    public class ReceiveState : State<FieldPlayer>
    {
        #region Singlton
        private static ReceiveState m_instance;
        public static ReceiveState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ReceiveState();
                }
                return m_instance;
            }
        }
        private ReceiveState()
        {

        }

        #endregion
        public override bool Enter(FieldPlayer player)
        {
            //let the team know this player is controlling
            player.team.SetControllingPlayer(player);

            //let the team know this player is receiving the ball
            player.team.receivingPlayer = player;
            isPositionPredicted = false;
            Logger.log("Player " + player.playerNumber + " enters receive state", Logger.LogLevel.INFO);
            if (player.action == null)
            {
                return true;
            }
            return false;
        }


        public override void Execute(FieldPlayer player)
        {
            if(player.hasBall)
            {
                player.stateMachine.ChangeState(KickState.instance);
                return;
            }
            if (player.Ball().controllingPlayerNumber == -1 && !isPositionPredicted)
            {
                Position expectedPosition = player.receivingPosition;
                double distance = player.Ball().position.distanceFrom(expectedPosition);
                Vector actualPositionVector = player.Ball().vector.getScaledVector(distance);
                Position predictedPosition = player.Ball().position.getPositionPlusVector(actualPositionVector);

                // Did the ball bounce?
                var pitch = player.Pitch();
                predictedPosition.AdjustBounce(pitch);


                Logger.log("Player " + player.playerNumber + " got predictedPostion " + predictedPosition, Logger.LogLevel.INFO);
                if (!player.team.PositionIsInGoalArea(predictedPosition))
                {
                    player.receivingPosition = predictedPosition;
                    isPositionPredicted = true;
                }
                else
                {
                    player.stateMachine.ChangeState(ReturnState.instance);
                    return;
                }

            }
            if (player.position.IsEqual(player.receivingPosition))
            {
                if(player.IsClosestTeamMemberToBall() && player.Ball().speed == 0)
                {
                    player.stateMachine.ChangeState(ChaseState.instance);
                    return;
                }
                player.stateMachine.ChangeState(WaitState.instance);
                return;
            }
            else
            {
                player.action = new Response(player.playerNumber, Response.Action.Move, player.receivingPosition);
                return;
            }
        }

        public override void Exit(FieldPlayer player)
        {
            Logger.log("Player " + player.playerNumber + " exits receive state", Logger.LogLevel.INFO);
            player.team.receivingPlayer = null;
            player.receivingPosition = null;
            //if (player.IsControllingPlayer())
            //{
            //    player.team.SetControllingPlayer(null);
            //}
            isPositionPredicted = false;

        }

        public override bool OnMessage(FieldPlayer player, Telegram t){return false;}

        private bool isPositionPredicted;
    }

}
