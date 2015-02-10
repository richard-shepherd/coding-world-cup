using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{

    class KickOffState : State<Team>
    {
        #region Singlton
        private static KickOffState m_instance;
        public static KickOffState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new KickOffState();
                }
                return m_instance;
            }
        }
        private KickOffState() { }

        #endregion

        public override bool Enter(Team team)
        {
            Logger.log("Team " + team.teamNumber + " enters KickOff state", Logger.LogLevel.INFO);

            team.SetControllingPlayer(null);
            team.receivingPlayer = null;
            team.supportingPlayer = null;
            team.playerClosestToBall = null;


            //these define the home regions for this state of each of the players
            int[] RightRegions = new int[6] { 16, 20, 13, 7, 9, 3 };
            int[] LeftRegions = new int[6] { 35, 31, 38, 44, 42, 48 };

            //set up the player's home regions
            if (team.playingDirection == Team.DirectionType.LEFT)
            {
                team.ChangePlayerHomeRegions(team, LeftRegions);
            }
            else
            {
                team.ChangePlayerHomeRegions(team, RightRegions);
            }

            foreach(Player player in team.Members.Values)
            {
                if (player.playerType == "P")
                {
                    FieldPlayer fieldPlayer = player as FieldPlayer;
                    if (!fieldPlayer.stateMachine.IsInState(DeadState.instance))
                    {
                        fieldPlayer.stateMachine.ChangeState(WaitState.instance);
                    }
                }
                else
                {
                    GoalKeeper keeper = player as GoalKeeper;
                    keeper.stateMachine.ChangeState(KeeperWaitState.instance);
                }
            }
            //just not to call execute method in changestate method
            return false;
        }

        public override void Execute(Team team)
        {
            //if both teams in position, start the game
            if (team.HasBall())
            {
                team.GetFSM().ChangeState(AttackState.instance);
            }
            else
            {
                team.GetFSM().ChangeState(DefendState.instance);
            }
        }

        public override void Exit(Team team)
        {
            Logger.log("Team " + team.teamNumber + " exits KickOff state", Logger.LogLevel.INFO);

        }

        public override bool OnMessage(Team team, Telegram t) { return false; }

    }
}
