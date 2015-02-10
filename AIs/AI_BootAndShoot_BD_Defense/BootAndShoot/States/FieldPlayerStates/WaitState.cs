using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{

    public class WaitState : State<FieldPlayer>
    {
        #region Singlton
        private static WaitState m_instance;
        public static WaitState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new WaitState();
                }
                return m_instance;
            }
        }
        private WaitState() { }

        #endregion

        public override bool Enter(FieldPlayer player)
        {

            Logger.log("Player " + player.playerNumber + " enters wait state", Logger.LogLevel.INFO);
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
            else
            {
                if (player.IsClosestTeamMemberToBall() && player.team.receivingPlayer == null && !player.team.BallStopPositionIsInGoalArea())
                {
                    player.stateMachine.ChangeState(ChaseState.instance);
                    return;
                }
                else if (!player.position.IsEqual(new Position(player.HomeRegion().center)))
                {
                    player.stateMachine.ChangeState(ReturnState.instance);
                    return;
                }
                //if this player's team is controlling AND this player is not the attacker
                //AND is further up the field than the attacker he should request a pass.
                else if (player.team.InControl() && !player.IsControllingPlayer() && player.IsAheadOfAttacker())
                {
                    player.team.RequestPass(player);
                    player.TrackBall();
                    return;

                }
                else
                {
                    player.TrackBall();
                    return;
                }
                   
            }
        }

        public override void Exit(FieldPlayer player) 
        {
            Logger.log("Player " + player.playerNumber + " exits wait state", Logger.LogLevel.INFO);
        }

        public override bool OnMessage(FieldPlayer player, Telegram t) { return false; }

    }
}
