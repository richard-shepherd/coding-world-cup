using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{

    public class ReturnState : State<FieldPlayer>
    {
        #region Singlton
        private static ReturnState m_instance;
        public static ReturnState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ReturnState();
                }
                return m_instance;
            }
        }
        private ReturnState() { }

        #endregion

        public override bool Enter(FieldPlayer player)
        {
            Logger.log("Player " + player.id + " enters Return state", Logger.LogLevel.INFO);
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
            //if the ball is nearer this player than any other team member, go chase it
            if (player.IsClosestTeamMemberToBall() && player.team.receivingPlayer == null && !player.team.BallStopPositionIsInGoalArea())
            {
                player.stateMachine.ChangeState(ChaseState.instance);
                return;
            }
            Position destination = new Position(player.HomeRegion().center);
            if (player.position.IsEqual(destination))
            {
                player.stateMachine.ChangeState(WaitState.instance);
                return;
            }

            player.action = new Response(player.playerNumber, Response.Action.Move, destination);
          

        }

        public override void Exit(FieldPlayer player) 
        {
            Logger.log("Player " + player.id + " exits Return state", Logger.LogLevel.INFO);
        }

        public override bool OnMessage(FieldPlayer player, Telegram t) { return false; }

    }
}
