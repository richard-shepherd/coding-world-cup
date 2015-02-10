using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class DeadState : State<FieldPlayer>
    {
        #region Singlton
        private static DeadState m_instance;
        public static DeadState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new DeadState();
                }
                return m_instance;
            }
        }
        private DeadState() { }

        #endregion

        public override bool Enter(FieldPlayer player)
        {
            Logger.log("Player " + player.id + " enters Dead state", Logger.LogLevel.INFO);
            if (player.action == null)
            {
                return true;
            }
            return false;

        }


        public override void Execute(FieldPlayer player)
        {
            Position theirGoal = player.team.getGoalCentre(Team.GoalType.THEIR_GOAL);
            if(player.hasBall)
            {
                player.action = new Response(player.playerNumber,Response.Action.Kick,theirGoal);
            }
            else if(player.position.distanceFrom(player.Ball().position) < 0.5)
            {
                player.action = new Response(player.playerNumber, Response.Action.TakePossession);
            }
            else
            {
                player.action = new Response(player.playerNumber, Response.Action.Turn, Math.Round(player.position.getAngle(theirGoal), 3));
            }
       
        }

        public override void Exit(FieldPlayer player)
        {
            Logger.log("Player " + player.id + " exits Dead state", Logger.LogLevel.INFO);
        }

        public override bool OnMessage(FieldPlayer player, Telegram t) { return false; }

    }
}
