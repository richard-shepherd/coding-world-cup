using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class PassBall : State<GoalKeeper>
    {
        #region Singlton
        private static PassBall m_instance;
        public static PassBall instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new PassBall();
                }
                return m_instance;
            }
        }
        private PassBall() { }

        #endregion

        public override bool Enter(GoalKeeper player) 
        {
            Logger.log("Golakeepr " + player.playerNumber + " enters PassBall state", Logger.LogLevel.INFO);
            
            //let the team know this player is controlling
            player.team.SetControllingPlayer(player);

            //send Msg_GoHome to each player.
            player.team.ReturnAllFieldPlayersToHome();
            if (player.action == null)
            {
                return true;
            }
            return false;
        }

        public override void Execute(GoalKeeper player) 
        {
            if(!player.hasBall)
            {
                player.stateMachine.ChangeState(KeeperWaitState.instance);
                return;
            }

            if (!player.position.IsEqual(player.returnPosition))
            {
                player.stateMachine.ChangeState(KeeperReturnState.instance);
                return;
            }
            // We choose a defender and find his position...
            var defenderPosition = player.safestDefender.position;

            // We kick the ball to the defender...

            double speed = Math.Sqrt(2 * 10 * player.position.distanceFrom(defenderPosition));
            double speedPercentage = (speed / Ball.maxSpeed) * 100;
            player.action = new Response();
            player.action.KickOrTurn(player, defenderPosition, speedPercentage);
            if (player.action.action == Response.Action.Kick)
            {
                //MessageDispatcher.instance.DispatchMsg(0.0, player.playerNumber, player.safestDefender.playerNumber, Messages.MessageType.Msg_ReceiveBall, defenderPosition);
                player.stateMachine.ChangeState(KeeperWaitState.instance);
            }



            return;
            
        }

        public override void Exit(GoalKeeper player) 
        {
            Logger.log("Golakeepr " + player.playerNumber + " exits PassBall state", Logger.LogLevel.INFO);
            //if (player.IsControllingPlayer())
            //{
            //    player.team.SetControllingPlayer(null);
            //}
        }

        public override bool OnMessage(GoalKeeper player, Telegram t)
        {

            return false;
        }
    }
}
