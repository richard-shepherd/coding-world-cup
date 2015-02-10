using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace BootAndShoot
{
    public class GoalKeeper : Player
    {
        public GoalKeeper(int number, player_role role, State<GoalKeeper> startState, Team team)
            : base(number, role, team)
        {
            returnPosition = new Position();
            stateMachine = new StateMachine<GoalKeeper>(this);

            if (startState != null)
            {
                stateMachine.SetCurrentState(startState);
                stateMachine.SetPreviousState(startState);
                stateMachine.SetGlobalState(GlobalKeeperState.instance);
            }          
        }

        public StateMachine<GoalKeeper> stateMachine { get; private set; }


        public override void Update()
        {
            returnPosition.y = 25.0;
            returnPosition.x = (team.playingDirection == global::BootAndShoot.Team.DirectionType.RIGHT) ? 0.5 : 99.5;
            calculateClosestDefender();
            if (stateMachine != null)
            {
                stateMachine.Update();
            }
        }

        #region override methods
        //-------------------- HandleMessage -------------------------------------
        
        //  routes any messages appropriately
        //------------------------------------------------------------------------
        public override bool HandleMessage(Telegram msg)
        {
            // return m_pStateMachine.HandleMessage(msg);
            return stateMachine.HandleMessage(msg);

        }
        #endregion

        private bool calculateClosestDefender()
        {
            double distance, minDistance = 100;
            foreach (var playerNumber in this.team.Members.Keys)
            {
                var player = this.team.Members[playerNumber];
                if (player.role == Player.player_role.defender || player.role == player_role.dead)
                {
                    distance = this.position.distanceFrom(player.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        safestDefender = player;
                    }
                }
            }
            safestDefenderDistance = minDistance;
            return true;
        }
        public Player safestDefender {get; private set;}
        public double safestDefenderDistance { get; private set; }
        public Position returnPosition { get; set; }

        /// <summary>
        /// True if the ball is in our goal-area, false if not.
        /// </summary>
        public bool BallIsInGoalArea()
        {
            var ballPosition = this.Ball().position;
            var goalCentre = team.getGoalCentre(global::BootAndShoot.Team.GoalType.OUR_GOAL);
            return ballPosition.distanceFrom(goalCentre) < this.team.pitch.goalAreaRadius;
        }

    }
}
