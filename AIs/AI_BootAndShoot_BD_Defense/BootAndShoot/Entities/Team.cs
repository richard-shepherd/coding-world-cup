using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BootAndShoot
{
    public class Team
    {
        #region Public Methods
        //This constructor is called only for oppoenent team
        public Team(int number)
        {
            //Variable initialization
            Members = new Dictionary<int , Player>();


            teamNumber = number;
            int offSet = teamNumber == 1 ? 0 : 6;
            for (int i = 0; i < 6; i++)
            {
                // We set up the all-players collection...
                Player player = new Player();
                player.playerNumber = offSet++;
                this.Members.Add(player.playerNumber, player);
            }

        }
        //This constrctor is for ownTeam
        public Team(dynamic data)
        {
            // Varibale intilization
            this.Members = new Dictionary<int,Player>();
           

            
            //setup the state machine
            stateMachine = new StateMachine<Team>(this);

            stateMachine.SetCurrentState(DefendState.instance);
            stateMachine.SetPreviousState(DefendState.instance);
            stateMachine.SetGlobalState(null);
            teamNumber = data.teamNumber;
         
            int offSet = teamNumber == 1 ? 0 : 6;

            //Need to find better way of finding number

            this.Members.Add(offSet, new FieldPlayer(offSet++, Player.player_role.attacker, WaitState.instance, this));
            this.Members.Add(offSet, new FieldPlayer(offSet++, Player.player_role.attacker, WaitState.instance, this));
            this.Members.Add(offSet, new FieldPlayer(offSet++, Player.player_role.defender, WaitState.instance, this));
            this.Members.Add(offSet, new FieldPlayer(offSet++, Player.player_role.dead, DeadState.instance, this));
            this.Members.Add(offSet, new FieldPlayer(offSet++, Player.player_role.dead, DeadState.instance, this));
            this.Members.Add(offSet, new GoalKeeper(offSet++, Player.player_role.goal_keeper, KeeperWaitState.instance, this));

            foreach (var dataPlayer in data.players)
            {
                int playerNumber = dataPlayer.playerNumber;
                this.Members[playerNumber].playerType = dataPlayer.playerType; 
  
            }



            //register the players with the entity manager
            foreach(Player player in Members.Values)
            {
                EntityManager.instance.RegisterEntity(player);
            }


        }

        public void SetDetails(dynamic data)
        {
            name = data.name;
            score = data.score;
            // We find the direction we are playing...
            if (data.direction == "LEFT")
            {
                this.playingDirection = DirectionType.LEFT;
            }
            else
            {
                this.playingDirection = DirectionType.RIGHT;
            }

            // Are we the team kicking off?
            this.weAreKickingOff = (data.teamKickingOff == this.teamNumber);

        }


        #endregion


        #region Data
        public int teamNumber { get; set; }
        public string name { get; set; }
        public int score { get; set; }

        #endregion

        #region Private

        private bool weAreKickingOff = false;
        //an instance of the state machine class
        StateMachine<Team>  stateMachine;

        //a pointer to the soccer pitch
        public GamePitch pitch { get; set; }

        public Player receivingPlayer { get; set; }
        public Player playerClosestToBall { get; set; }
        public Player controllingPlayer { get; private set; }
        public Player supportingPlayer { get; set; }

        //the squared distance the closest player is from the ball
        public double distToBallOfClosestPlayer { get; protected set; }

        public  enum DirectionType
        {
            // We don't yet know the playing direction...
            DONT_KNOW,

            // We are shooting at the left goal...
            LEFT,

            // We are shooting at the right goal...
            RIGHT
        }
        

        //players use this to determine strategic positions on the playing field
        public SupportSpotCalculator supportSpotCalc;

        public Vector GetSupportSpot()
        {
            return supportSpotCalc.GetBestSupportingSpot();
        }

        public void DetermineBestSupportingPosition()
        {
            supportSpotCalc.DetermineBestSupportingPosition();
        }


        public Player GetKeeper()
        {
            Player keeper = null;
            foreach (int playerNumber in Members.Keys)
            {
                keeper = Members[playerNumber];
                if (keeper.playerType != "P")
                    break;
            }
            return keeper;

        }
        //called each frame. Sets m_pClosestPlayerToBall to point to the player
        //closest to the ball. 
        public void CalculateClosestPlayerToBall()
        {
            Position ballPosition = this.pitch.ball.position;
            double distance, minDistance = 100;
            foreach (var playerNumber in this.Members.Keys)
            {
                var player = this.Members[playerNumber];
                if (player.role != Player.player_role.goal_keeper && player.role != Player.player_role.dead)
                {
                    var playerPosition = player.position;
                    player.distanceFromBall = playerPosition.distanceFrom(ballPosition);
                    distance = player.distanceFromBall;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        playerClosestToBall = player;
                    }
                }
            }
            distToBallOfClosestPlayer = minDistance;         
        }
        //#endregion




        //the usual suspects
        public void Update()
        {
            CalculateClosestPlayerToBall();

            stateMachine.Update();
            //if opponent goal keeper has ball then return all players to home region
            if(this.Opponents.HasBall() && this.Opponents.Members[pitch.ball.controllingPlayerNumber].playerType == "G")
            {
                this.ReturnAllFieldPlayersToHome();
            }
            if(this.Opponents.HasBall() || (this.pitch.ball.controllingPlayerNumber == -1 && this.pitch.ball.speed == 0))
            {
                this.LostControl();
            }

            
            //now update each player
            foreach (var player in Members.Values)
            {
                if (player.action == null)
                {
                    player.Update();
                }
            }
            


        }

        public void StoreInfo(dynamic data)
        {
            // We store info about each player in the team in a map 
            // by player-number, and we also split out the player info
            // from the goalkeeper info...
            foreach (var playerInfo in data.players)
            {
                var staticState = playerInfo.staticState;
                int playerNumber = staticState.playerNumber;

                // We store all the players in one collection...
                if(this.Members.ContainsKey(playerNumber))
                {
                    this.Members[playerNumber].SetInfo(playerInfo);
                    this.Members[playerNumber].team = this;
                }
                

            }

        }
        /// <summary>
        /// True if the ball stop position is in our goal-area, false if not.
        /// </summary>
        public bool BallStopPositionIsInGoalArea()
        {
            var ballPosition = pitch.ball.position;
            var goalCentre = getGoalCentre(GoalType.OUR_GOAL);
            return ballPosition.distanceFrom(goalCentre) < pitch.goalAreaRadius;
        }

        public bool PositionIsInGoalArea(Position p)
        {
            var ourGoalCentre = getGoalCentre(GoalType.OUR_GOAL);
            var oppGoalCentre = getGoalCentre(GoalType.THEIR_GOAL);
            if (p.distanceFrom(ourGoalCentre) < pitch.goalAreaRadius || p.distanceFrom(oppGoalCentre) < pitch.goalAreaRadius)
            {
                return true;
            }
            return false;
        }

        public void ChangePlayerHomeRegions(Team team, int[] NewRegions)
        {
            int count = 0;
          foreach (int plyr in team.Members.Keys)
          {
            SetPlayerHomeRegion(plyr, NewRegions[count++]);
          }
        }

        public void SetPlayerHomeRegion(int plyr, int region)
        {
          if(Members.ContainsKey(plyr))
          {
              Members[plyr].homeRegion = region;
          }

          
        }


        //calling this changes the state of all field players to that of 
        //ReturnToHomeRegion. Mainly used when a goal keeper has
        //possession
        public void ReturnAllFieldPlayersToHome()
        {
            foreach(Player player in Members.Values)
            {

                MessageDispatcher.instance.DispatchMsg(0.0, 1, player.id, Messages.MessageType.Msg_GoHome, null);
            }


        }
        /// <summary>
        //----------------------- isPassSafeFromOpponent -------------------------
        //
        //  test if a pass from 'from' to 'to' can be intercepted by an opposing
        //  player
        //------------------------------------------------------------------------
        /// </summary>
        bool IsPassSafeFromOpponent(Vector from, Vector target, Player receiver, Player opp, double ballSpeedAfterKick)
        {
            Vector toTarget = new Vector(from, target);
            Vector toTargetNormalized = Vector.VecNormalize(toTarget);
            Vector perpToTarget = toTargetNormalized.Perp();
            Vector opponentPosition = opp.position.ToVector();
            Vector localPosOpp = Vector.PointToLocalSpace(opponentPosition, toTargetNormalized, perpToTarget, from);

            //if opponent is behind the kicker then pass is considered okay(this is 
            //based on the assumption that the ball is going to be kicked with a 
            //velocity greater than the opponent's max velocity)

            //ballSpeedAfterKick should be more than player max speed i.e 10.0m/s
            if (localPosOpp.x < 0)
            {
                return true;
            }

            //if the opponent is further away than the target we need to consider if
            //the opponent can reach the position before the receiver.
            if (Vector.VecDistanceSq(from, target) < Vector.VecDistanceSq(opponentPosition, from))
            {
                if (receiver != null)
                {
                    Vector receiverPosition = new Vector(receiver.position.x, receiver.position.y);
                    if (Vector.VecDistanceSq(target, opponentPosition) > Vector.VecDistanceSq(target, receiverPosition))
                    {
                        return true;
                    }

                    else
                    {
                        return false;
                    }

                }

                else
                {
                    return true;
                }
            }
            //calculate how long it takes the ball to cover the distance to the 
            //position orthogonal to the opponents position

            Vector origin = new Vector(0.0, 0.0);
            Vector localPosX = new Vector(localPosOpp.x, 0.0);
            double TimeForBall = this.pitch.ball.TimeToCoverDistance(origin, localPosX, ballSpeedAfterKick);

            //now calculate how far the opponent can run in this time
            // double reach = opp.runningAbility * TimeForBall + Pitch()->Ball()->BRadius() + opp->BRadius();

            double reach = ((opp.runningAbility / 100) * 10) * TimeForBall;

            //if the distance to the opponent's y position is less than his running
            //range plus the radius of the ball and the opponents radius then the
            //ball can be intercepted
            if (Math.Abs(localPosOpp.y) < reach)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        //---------------------- isPassSafeFromAllOpponents ----------------------
        //
        //  tests a pass from position 'from' to position 'target' against each member
        //  of the opposing team. Returns true if the pass can be made without
        //  getting intercepted
        //------------------------------------------------------------------------
        /// </summary>
        public bool IsPassSafeFromAllOpponents(Vector from, Vector target, Player receiver , double ballSpeedAfterKick)
        {
            foreach (var otherPlayerNumber in this.Opponents.Members.Keys)
            {
                var otherPlayer = this.Opponents.Members[otherPlayerNumber];

                if (!IsPassSafeFromOpponent(from, target, receiver, otherPlayer, ballSpeedAfterKick))
                {
                    return false;
                }
                

            }

            return true;
        }
        //------------------------ CanShoot --------------------------------------
        //
        //  Given a ball position, a kicking power and a reference to a vector
        //  this function will sample random positions along the opponent's goal-
        //  mouth and check to see if a goal can be scored if the ball was to be
        //  kicked in that direction with the given power. If a possible shot is 
        //  found, the function will immediately return true, with the target 
        //  position stored in the vector ShotTarget.
        //------------------------------------------------------------------------
        public bool CanShoot(Vector  fromPosition, double ballSpeedAfterKick, Vector shotTarget = null)
        {

            if (shotTarget == null)
            {
                shotTarget = new Vector(getGoalCentre(GoalType.THEIR_GOAL));
            }
            Vector foundBestTarget = null;
            double distance, minDistance = 100;
            for(int i = 21; i<30;i++)
            {
                shotTarget.y = i;

                //make sure striking the ball with the given power is enough to drive
                //the ball over the goal line.
                double time = pitch.ball.TimeToCoverDistance(fromPosition, shotTarget, ballSpeedAfterKick);
    
                //if it is, this shot is then tested to see if any of the opponents
                //can intercept it.
                if (time >= 0)
                {
                    if (IsPassSafeFromAllOpponents(fromPosition, shotTarget, null, ballSpeedAfterKick))
                    {
                        distance = Vector.VecDistance(fromPosition, shotTarget);
                        if (distance < minDistance)
                        {
                            foundBestTarget = shotTarget;
                            minDistance = distance;
                        }
                    }
                }
            }
            if(foundBestTarget != null)
            {
                return true;
            }
            return false;
        }

        //------------------------- RequestPass ---------------------------------------
        //
        //  this tests to see if a pass is possible between the requester and
        //  the controlling player. If it is possible a message is sent to the
        //  controlling player to pass the ball asap.
        //-----------------------------------------------------------------------------
        public void RequestPass(FieldPlayer requester)
        {
          //maybe put a restriction here
          //if (RandFloat() > 0.1) return;
  
          if (IsPassSafeFromAllOpponents(controllingPlayer.position.ToVector(),requester.position.ToVector(),requester , 30.0))
          {

            //tell the player to make the pass
            //let the receiver know a pass is coming 
            MessageDispatcher.instance.DispatchMsg(0.0,requester.playerNumber,controllingPlayer.playerNumber,Messages.MessageType.Msg_PassToMe,requester);


          }
        }
        /// <summary>
        /// Returns a Position for the centre of the requested goal.
        /// </summary>
        public Position getGoalCentre(GoalType goal)
        {
            double x = 0.0;
            if ((goal == GoalType.OUR_GOAL && this.playingDirection == DirectionType.LEFT)
                ||
                (goal == GoalType.THEIR_GOAL && this.playingDirection == DirectionType.RIGHT))
            {
                x = this.pitch.width;
            }
            double y = this.pitch.goalCentre;
            return new Position(x, y);
        }
        // An enum for the two goals...
        public enum GoalType
        {
            OUR_GOAL,
            THEIR_GOAL
        }

        //The best pass is considered to be the pass that cannot be intercepted 
        //by an opponent and that is as far forward of the receiver as possible  
        //If a pass is found, the receiver's address is returned in the 
        //reference, 'receiver' and the position the pass will be made to is 
        //returned in the  reference 'PassTarget'
        public bool FindPass(Player passer, ref Player receiver, ref Vector passTarget, double MinPassingDistance, double speed)
        {

            double ClosestToGoalSoFar = 100.0;
            Vector target = new Vector();

            //iterate through all this player's team members and calculate which
            //one is in a position to be passed the ball 
            foreach (Player curPlyr in Members.Values)
            {   
                //make sure the potential receiver being examined is not this player
                //and that it is further away than the minimum pass distance
                if ( (curPlyr != passer) && 
                    (Vector.VecDistanceSq(passer.positionVector, curPlyr.positionVector) > MinPassingDistance*MinPassingDistance))                  
                {           
                    if (GetBestPassToReceiver(passer, curPlyr, ref target , speed))
                    {
                        //if the pass target is the closest to the opponent's goal line found
                        // so far, keep a record of it
                        double Dist2Goal = Math.Abs(target.x - getGoalCentre(GoalType.THEIR_GOAL).x);

                        if (Dist2Goal < ClosestToGoalSoFar)
                        {
                            ClosestToGoalSoFar = Dist2Goal;
          
                            //keep a record of this player
                            receiver = curPlyr;

                            //and the target
                            passTarget = target;
                        }     
                        }
                }
            }//next team member

            if (receiver != null) return true;
 
            return false;
        }

        //Three potential passes are calculated. One directly toward the receiver's
        //current position and two that are the tangents from the ball position
        //to the circle of radius 'range' from the receiver.
        //These passes are then tested to see if they can be intercepted by an
        //opponent and to make sure they terminate within the playing area. If
        //all the passes are invalidated the function returns false. Otherwise
        //the function returns the pass that takes the ball closest to the 
        //opponent's goal area.
        public bool GetBestPassToReceiver(Player passer, Player receiver, ref Vector PassTarget, double speed)
        {
            //first, calculate how much time it will take for the ball to reach 
            //this receiver, if the receiver was to remain motionless 
            double time = pitch.ball.TimeToCoverDistance(pitch.ball.position.ToVector(), receiver.positionVector, speed);

            //return false if ball cannot reach the receiver after having been
            //kicked with the given power
            if (time < 0) return false;

            //the maximum distance the receiver can cover in this time
            // 10.0 is player max speed
            double interceptRange = time * 10.0;

            //Scale the intercept range
            const double ScalingFactor = 0.3;
            interceptRange *= ScalingFactor;

            //now calculate the pass targets which are positioned at the intercepts
            //of the tangents from the ball to the receiver's range circle.
            Vector ip1 = new Vector();
            Vector ip2 = new Vector();

            Geometry.GetTangentPoints(receiver.positionVector, interceptRange,pitch.ball.position.ToVector(),ip1,ip2);

            const int NumPassesToTry = 3;
            Vector[] passes = new Vector[NumPassesToTry]{ip1, receiver.positionVector, ip2};
  
  
            // this pass is the best found so far if it is:
            //
            //  1. Further upfield than the closest valid pass for this receiver
            //     found so far
            //  2. Within the playing area
            //  3. Cannot be intercepted by any opponents

            double ClosestSoFar = 100.0;
            bool  bResult      = false;

            for (int pass=0; pass<NumPassesToTry; ++pass)
            {    
            double dist = Math.Abs(passes[pass].x - getGoalCentre(GoalType.THEIR_GOAL).x);

            if (( dist < ClosestSoFar) && pitch.playingArea.Inside(passes[pass]) && !PositionIsInGoalArea(passes[pass].ToPosition()) &&
                IsPassSafeFromAllOpponents(pitch.ball.position.ToVector(),passes[pass],receiver, speed))
        
            {
                ClosestSoFar = dist;
                PassTarget   = passes[pass];
                bResult      = true;
            }
            }

            return bResult;
        }

        //calculates the best supporting position and finds the most appropriate
        //attacker to travel to the spot
        public Player DetermineBestSupportingAttacker()
        {
            double ClosestSoFar = Double.MaxValue;
            Player bestPlayer = null;

            foreach(Player player in Members.Values)
            {
                if(player.role == Player.player_role.attacker && player != controllingPlayer)
                {
                    //calculate the dist. Use the squared value to avoid sqrt
                    double dist = Vector.VecDistanceSq(player.positionVector, supportSpotCalc.GetBestSupportingSpot());

                    //if the distance is the closest so far and the player is not a
                    //goalkeeper and the player is not the one currently controlling
                    //the ball, keep a record of this player
                    if ((dist < ClosestSoFar))
                    {
                        ClosestSoFar = dist;

                        bestPlayer = player;
                    }
                }
            }
            return bestPlayer;

        }


        

        public StateMachine<Team> GetFSM(){return stateMachine;}

        ////Goal*const           HomeGoal()const{return m_pHomeGoal;}
        ////Goal*const           OpponentsGoal()const{return m_pOpponentsGoal;}

        //public GamePitch Pitch() { return m_pPitch; }





        public bool InControl() { if (controllingPlayer != null)return true; else return false; }
        public void LostControl() { controllingPlayer = null; }
        public void SetControllingPlayer(Player player) 
        {
            controllingPlayer = player;
        }

        public bool OwnHalf()
        {
            return (this.playingDirection == DirectionType.RIGHT && this.pitch.ball.position.x < this.pitch.centreSpot.x)
                                   ||
                                   (this.playingDirection == DirectionType.LEFT && this.pitch.ball.position.x > this.pitch.centreSpot.x);
        }
        public bool HasBall()
        {
            foreach(Player player in Members.Values)
            {
                if(player.hasBall)
                {
                    return true;
                }
            }
            return false;
        }

        //public Player GetPlayerFromID(int id);


        //public void SetPlayerHomeRegion(int plyr, int region);


        //public void UpdateTargetsOfWaitingPlayers();

        //returns false if any of the team are not located within their home region
        public bool AllPlayersAtHome()
        {
            foreach(Player player in Members.Values)
            {
                if(player.InHomeRegion() == false)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion


        #region Public

        //a pointer to the opposing team
        public Team Opponents { get; set; }
        public Dictionary<int , Player> Members { get; set; }

        public DirectionType playingDirection { get; set; }
        public double Direction()
        {
            if(playingDirection == DirectionType.RIGHT)
            {
                return 90.0;
            }
            if(playingDirection == DirectionType.LEFT)
            {
                return 270.0;
            }
            return 0.0;
        }
        #endregion 

    }


}

