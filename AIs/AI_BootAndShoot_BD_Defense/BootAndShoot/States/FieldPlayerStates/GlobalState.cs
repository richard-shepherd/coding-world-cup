using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot.States.FieldPlayerStates
{

    public class GlobalState : State<FieldPlayer>
    {

        #region Singlton
        private static GlobalState m_instance;
        public static GlobalState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new GlobalState();
                }
                return m_instance;
            }
        }
        private GlobalState() { }

        #endregion

        public override bool Enter(FieldPlayer player) { return true; }

        public override void Execute(FieldPlayer player)
        {

        }

        public override void Exit(FieldPlayer player) { }

        public override bool OnMessage(FieldPlayer player, Telegram t)
        {
            if (player.role == Player.player_role.dead)
                return true;
                        
            switch (t.msg)
            {
                case Messages.MessageType.Msg_ReceiveBall:
                    {

                        Logger.log("Player " + player.playerNumber + " received message of type Msg_Receive Ball", Logger.LogLevel.INFO);
                        player.receivingPosition = new Position(t.extraInfo);



                        //change state 
                        player.stateMachine.ChangeState(ReceiveState.instance);

                        return true;
                    }
               
                case Messages.MessageType.Msg_Wait:
                    {
                        //change the state
                        player.stateMachine.ChangeState(WaitState.instance);

                        return true;
                    }
                case Messages.MessageType.Msg_PassToMe:
                    {
                        //get the position of the player requesting the pass 
                        FieldPlayer receiver = (FieldPlayer)(t.extraInfo);

                        Logger.log("Player " + player.playerNumber + " received PassToMe request from " + receiver.playerNumber, Logger.LogLevel.INFO);

                        //if the ball is not within kicking range or their is already a 
                        //receiving player, this player cannot pass the ball to the player
                        //making the request.
                        if (player.team.receivingPlayer != null || !player.hasBall)
                        {
                            bool isReceivingPlayerNull = player.team.receivingPlayer != null ? true : false;
                            Logger.log("Player " + player.playerNumber + " cannot make requested pass <Receving player is not NUll ?  " + isReceivingPlayerNull + " Player hasBall ? " + player.hasBall, Logger.LogLevel.INFO);
                            return true;
                        }


                        //make the pass 
                        double speed = Math.Sqrt(2 * 10 * player.position.distanceFrom(receiver.position));
                        double speedPercentage = (speed / Ball.maxSpeed) * 100;
                        player.action = new Response(player.playerNumber, Response.Action.Kick, receiver.position, speedPercentage);
                        Logger.log("Player " + player.playerNumber + " Passed ball to requesting player", Logger.LogLevel.INFO);


                        //let the receiver know a pass is coming 
                        MessageDispatcher.instance.DispatchMsg(0.0, player.id, receiver.id, Messages.MessageType.Msg_ReceiveBall, receiver.position);



                        //change state   
                        player.stateMachine.ChangeState(WaitState.instance);

                       // player.FindSupport();


                        return true;

                    }
                case Messages.MessageType.Msg_GoHome:
                    {
                       // player.SetDefaultHomeRegion();

                            player.stateMachine.ChangeState(ReturnState.instance);
                        
                        return true;
                    }

               


            }
            return false;
        }

    }
}
