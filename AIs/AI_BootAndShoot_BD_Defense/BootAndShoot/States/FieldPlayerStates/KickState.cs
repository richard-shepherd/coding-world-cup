using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{

    public class KickState : State<FieldPlayer>
    {
        #region Singlton
        private static KickState m_instance;
        public static KickState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new KickState();
                }
                return m_instance;
            }
        }
        private KickState() { }

        #endregion

        public override bool Enter(FieldPlayer player)
        {
            Logger.log("Player " + player.playerNumber + " enters kick state", Logger.LogLevel.INFO);

            //let the team know this player is controlling
            player.team.SetControllingPlayer(player);

            if (player.action == null)
            {
                return true;
            }
            return false;
        }


        public override void Execute(FieldPlayer player)
        {
            if(!player.hasBall)
            {
                player.stateMachine.ChangeState(ChaseState.instance);
                return;
            }
            Position theirGoal = player.team.getGoalCentre(Team.GoalType.THEIR_GOAL);
            /* Attempt a shot at the goal */
            Vector target = new Vector(theirGoal);
            if(player.team.CanShoot(player.positionVector,Ball.maxSpeed,target))
            {
                player.action = new Response();
                player.action.ShootOrTurn(player, target.ToPosition());
                if(player.action.action == Response.Action.Kick)
                {
                    player.stateMachine.ChangeState(WaitState.instance);
                }
                return;
            }
            if (!player.IsThreatened() && player.position.distanceFrom(theirGoal) > 17 )
            {
                player.action = new Response(player.playerNumber, Response.Action.Move, theirGoal);
                return;
            }
            else if(player.position.distanceFrom(theirGoal) <= 17)
            {
                player.stateMachine.ChangeState(DribbleState.instance);
                return;
            }
            /* Attempt a pass to a player */
            // 10 is player max speed
            double minPassingDistance = (10.0 *10.0) / (2 * Ball.friction);
            Player receiver = null;
            if (player.team.FindPass(player, ref receiver, ref target, minPassingDistance, Ball.maxSpeed))
            {
                double speed = Math.Sqrt(2 * 10 * player.position.distanceFrom(target.ToPosition()));
                double speedPercentage = (speed / Ball.maxSpeed) * 100;
                player.action = new Response(player.playerNumber, Response.Action.Kick, target.ToPosition(), speedPercentage);
    
                MessageDispatcher.instance.DispatchMsg(0.0, player.playerNumber, receiver.playerNumber, Messages.MessageType.Msg_ReceiveBall, target.ToPosition());
                player.stateMachine.ChangeState(WaitState.instance);
                

                return;
            }
            //cannot shoot or pass, so dribble the ball upfield
            else
            {
                player.stateMachine.ChangeState(DribbleState.instance);
                return;
            }  


        }

        public override void Exit(FieldPlayer player) 
        {
            Logger.log("Player " + player.playerNumber + " exits kick state", Logger.LogLevel.INFO);
            //if (player.IsControllingPlayer())
            //{
            //    player.team.SetControllingPlayer(null);
            //}
        }
        public override bool OnMessage(FieldPlayer player, Telegram t) { return false; }


    }
}
