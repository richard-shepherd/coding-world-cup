using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class AttackState : State<Team>
    {


        #region Singlton
        private static AttackState m_instance;
        public static AttackState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new AttackState();
                }
                return m_instance;
            }
        }
        private AttackState() { }

        #endregion


        public override bool Enter(Team team)
        {
            Logger.log("Team " + team.teamNumber + " enters Attack state", Logger.LogLevel.INFO);
            //these define the home regions for this state of each of the players
            int[] RightRegions = new int[6] { 42, 39, 28, 13, 24, 3 };
            int[] LeftRegions = new int[6] {7, 14, 23, 29, 38, 48 };

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
            //if this team is no longer in control change states
            //if (team.OwnHalf() && !team.HasBall() || (!team.OwnHalf() && team.Opponents.HasBall()))
            if(!team.InControl())
            {
                team.GetFSM().ChangeState(DefendState.instance);
                return;
            }

            //calculate the best position for any supporting attacker to move to
            //team.DetermineBestSupportingPosition();

        }

        public override void Exit(Team team) 
        {
            Logger.log("Team " + team.teamNumber + " exits Attack state", Logger.LogLevel.INFO);
            team.supportingPlayer = null;
        }

        public override bool OnMessage(Team team, Telegram t) { return false; }

    }

}
