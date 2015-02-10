using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{

    public class KeeperReturnState : State<GoalKeeper>
    {
        #region Singlton
        private static KeeperReturnState m_instance;
        public static KeeperReturnState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new KeeperReturnState();
                }
                return m_instance;
            }
        }
        private KeeperReturnState() { }

        #endregion



        public override bool Enter(GoalKeeper player) 
        {
            Logger.log("Golakeepr " + player.playerNumber + " enters Return state", Logger.LogLevel.INFO);
            if (player.action == null)
            {
                return true;
            }
            return false; 
        }

        public override void Execute(GoalKeeper player)
        {
            if(player.position.IsEqual(player.returnPosition)) 
            {
                player.stateMachine.ChangeState(KeeperWaitState.instance);
                return;
            }
            //move to default position
            var defaultResposne = new Response();
            defaultResposne.MoveOrTurn(player, player.returnPosition);
            player.action = defaultResposne;

        }

        public override void Exit(GoalKeeper player) 
        {
            Logger.log("Golakeepr " + player.playerNumber + " exits Return state", Logger.LogLevel.INFO);
        }

        public override bool OnMessage(GoalKeeper player, Telegram t) { return false; }

        
    };
}
