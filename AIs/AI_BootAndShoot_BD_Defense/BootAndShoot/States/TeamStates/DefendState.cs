using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class DefendState : State<Team>
    {

        #region Singlton
        private static DefendState m_instance;
        public static DefendState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new DefendState();
                }
                return m_instance;
            }
        }
        private DefendState() { }

        #endregion

        public override bool Enter(Team team)
        {
            Logger.log("Team " + team.teamNumber + " enters Defend state", Logger.LogLevel.INFO);
            //these define the home regions for this state of each of the players
            int[] RightRegions = new int[6] { 22, 19, 13, 7, 9, 3 };
            int[] LeftRegions = new int[6] { 27, 34, 38, 44, 42, 48 };

            //set up the player's home regions
            if (team.playingDirection == Team.DirectionType.LEFT)
            {
                team.ChangePlayerHomeRegions(team, LeftRegions);
            }
            else
            {
                team.ChangePlayerHomeRegions(team, RightRegions);
            }


            //just not to call execute method in changestate method
            return false;
        }

        public override void Execute(Team team)
        {
            //if in control change states
            //if ((!team.OwnHalf() && !team.Opponents.HasBall()) || team.OwnHalf() && team.HasBall())
            if(team.InControl())
            {
                team.GetFSM().ChangeState(AttackState.instance);
            }
        }

        public override void Exit(Team team) 
        {
            Logger.log("Team " + team.teamNumber + " exits Defend state", Logger.LogLevel.INFO);
        }

        public override bool OnMessage(Team team, Telegram t) { return false; }

    }
}
