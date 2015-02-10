using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BootAndShoot.States.FieldPlayerStates;


namespace BootAndShoot
{
    public class FieldPlayer : Player
    {

        public FieldPlayer(int number, player_role role, State<FieldPlayer> startState, Team team) : base(number, role, team)
        {

            stateMachine = new StateMachine<FieldPlayer>(this);

            if (startState != null)
            {
                stateMachine.SetCurrentState(startState);
                stateMachine.SetPreviousState(startState);
                stateMachine.SetGlobalState(GlobalState.instance);
            }
            
        }
        public StateMachine<FieldPlayer> stateMachine { get; private set; }

 


        public override void Update()
        {
            calculateClosestOpponent();
            if (stateMachine != null)
            {
               stateMachine.Update();                               
            }
        }


        public bool calculateClosestOpponent()
        {
             double distance, minDistance = 100;
            foreach (var playerNumber in this.team.Opponents.Members.Keys)
            {
                var player = this.team.Opponents.Members[playerNumber];
                distance = this.position.distanceFrom(player.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestOpponentPlayer = player;
                }
                
            }
            closestOpponentPlayerDistance = minDistance;
            return true;  
        }
        public bool calculateClosestAttacker()
        {
            double distance, minDistance = 100;
            foreach (var playerNumber in this.team.Members.Keys)
            {
                var player = this.team.Members[playerNumber];
                if ((player.role == player_role.attacker) && player != this)
                {
                    distance = this.position.distanceFrom(player.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPlayer = player;
                    }
                }
            }
            closestPlayerDistance = minDistance;
            return true;      
        }
        public Position getTarget()
        {
            Player opponentGoalKeeper = team.Opponents.GetKeeper();

            var shootAt = team.getGoalCentre(global::BootAndShoot.Team.GoalType.THEIR_GOAL);
            if (opponentGoalKeeper != null)
            {
                double maxDistance = 0;
                Position opponentGoalKeeperPosition = opponentGoalKeeper.position;
                for (int i = 21; i < 30; i++)
                {
                    var shootPosition = new Position(shootAt.x, i);
                    if (opponentGoalKeeperPosition.distanceFrom(shootPosition) > maxDistance)
                    {
                        maxDistance = opponentGoalKeeperPosition.distanceFrom(shootPosition);
                        shootAt = shootPosition;
                    }
                }

            }
            return shootAt;
        }


        #region override methods
        //-------------------- HandleMessage -------------------------------------
        //
        //  routes any messages appropriately
        //------------------------------------------------------------------------
        public override bool HandleMessage(Telegram msg)
        {
            return stateMachine.HandleMessage(msg);
        }
        #endregion


        public Position receivingPosition { get; set; }
        public Position supportingPosition { get; set; }

        public Player closestPlayer { get; set; }
        public double closestPlayerDistance {get;set;}

        public Player closestOpponentPlayer { get; set; }
        public double closestOpponentPlayerDistance {get;set;}

    }
}
