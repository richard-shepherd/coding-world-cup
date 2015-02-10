using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class GlobalKeeperState : State<GoalKeeper>
    {

        #region Singlton
        private static GlobalKeeperState m_instance;
        public static GlobalKeeperState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new GlobalKeeperState();
                }
                return m_instance;
            }
        }
        private GlobalKeeperState() { }

        #endregion

        public override bool Enter(GoalKeeper player) { return true; }

        public override void Execute(GoalKeeper player) { }

        public override void Exit(GoalKeeper player) { }

        public override bool OnMessage(GoalKeeper player, Telegram t)
        {
            switch (t.msg)
            {
                case Messages.MessageType.Msg_GoHome:
                    {
                        if (!player.position.IsEqual(player.returnPosition))
                        {
                            player.stateMachine.ChangeState(KeeperReturnState.instance);
                        }
                        return true;
                    }
            //    case Messages.MessageType.Msg_PassToMe:
            //        {
            //            return true;
            //        }


            }
            return false;
        }

    };
}
