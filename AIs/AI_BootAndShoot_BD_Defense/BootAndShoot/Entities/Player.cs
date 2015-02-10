using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




namespace BootAndShoot
{
    public class Player: BaseGameEntity
    {
        #region Public Methods

        public enum player_role{goal_keeper, attacker, defender,dead};

        public Player() : base(BaseGameEntity.GetNextValidID()) { }
        public Player(int number , player_role role, Team team) : base(BaseGameEntity.GetNextValidID())
        {
            //varibale initialization
            position = new Position();
            
            this.team = team;

            playerNumber = number;
            //player number is the id of the player
            id = playerNumber;

            this.role = role;
            
        }
      

        //returns true if there is an opponent within this player's 
        //comfort zone
        public bool IsThreatened()
        {
            foreach(Player player in team.Opponents.Members.Values)
            {

                //5.0 is player comfort zone distance
               // if (PositionInFrontOfPlayer(player.positionVector) && Vector.VecDistance(positionVector, player.positionVector) < 5.0 )
                if (Vector.VecDistance(positionVector, player.positionVector) < 1.6)
                {
                    return true;
                }
            }
            return false;

        }

        //------------------------- WithinFieldOfView ---------------------------
        //
        //  returns true if subject is within field of view of this player
        //-----------------------------------------------------------------------
        public bool PositionInFrontOfPlayer(Vector subjectPos)
        {
            Vector ToSubject = subjectPos.Subtract(positionVector);

          if (ToSubject.Dot(heading) > 0) 
    
            return true;

          else

            return false;
        }


        //this messages the player that is closest to the supporting spot to
        //change state to support the attacking player
        public void FindSupport()
        {

   
              //if there is no support we need to find a suitable player.
              if (team.supportingPlayer == null)
              {
                  team.supportingPlayer = team.DetermineBestSupportingAttacker();
                  MessageDispatcher.instance.DispatchMsg(0.0, id, team.supportingPlayer.id, Messages.MessageType.Msg_SupportAttacker, null);
                  return;
              }
    
              Player bestSupportPly = team.DetermineBestSupportingAttacker();
    
              //if the best player available to support the attacker changes, update
              //the pointers and send messages to the relevant players to update their
              //states
              if (bestSupportPly != null && (bestSupportPly != team.supportingPlayer))
              {
    
                    if (team.supportingPlayer != null)
                    {
                        MessageDispatcher.instance.DispatchMsg(0.0, id, team.supportingPlayer.id, Messages.MessageType.Msg_GoHome,null);
                    }


                    team.supportingPlayer = bestSupportPly;
                    MessageDispatcher.instance.DispatchMsg(0.0, id, team.supportingPlayer.id, Messages.MessageType.Msg_SupportAttacker, null);
                }
        }

        //returns true if the player is located within the boundaries 
        //of his home region
        public bool InHomeRegion()
        {
            if (team.pitch.regions.ContainsKey(homeRegion))
            {
                if (role == player_role.goal_keeper)
                {
                    return team.pitch.regions[homeRegion].Inside(positionVector, Region.region_modifier.normal);
                }
                else
                {
                    return team.pitch.regions[homeRegion].Inside(positionVector, Region.region_modifier.halfsize);
                }
            }
            return false;
        }

        public bool IsControllingPlayer()
        {
            return (team.controllingPlayer == this);
        }

        public bool IsAheadOfAttacker()
        {
          return Math.Abs(positionVector.x - team.getGoalCentre(global::BootAndShoot.Team.GoalType.THEIR_GOAL).x) <
                 Math.Abs(team.controllingPlayer.positionVector.x - team.getGoalCentre(global::BootAndShoot.Team.GoalType.THEIR_GOAL).x);
        }

        public void TrackBall()
        {
            double direction =  Math.Round(position.getAngle(Ball().position),3);
            action = new Response(playerNumber, Response.Action.Turn, direction);
        }
       
        public void SetDefaultHomeRegion()
        {
            homeRegion = defaultRegion;
        }


        public GamePitch Pitch()
        {
            return team.pitch;
        }
        public Region HomeRegion()
        {
            if(Pitch().regions.ContainsKey(homeRegion))
            {
                return Pitch().regions[homeRegion];
            }
            else
            {
                return null;
            }
        
        }

        public Region DefaultRegion()
        {
            return Pitch().regions[defaultRegion];
        }
        public Team Team(){return team;}
        
        public void SetInfo(dynamic player)
        {
            dynamic staticState = player.staticState;

            this.playerNumber = staticState.playerNumber;
            this.playerType =  staticState.playerType;
            this.kickingAbility = (double)staticState.kickingAbility;
            this.runningAbility = (double)staticState.runningAbility;
            this.ballControlAbility = (double)staticState.ballControlAbility;
            this.tacklingAbility = (double)staticState.tacklingAbility;

            dynamic dynamicState = player.dynamicState;
            
            this.hasBall = dynamicState.hasBall;
            this.direction = (double)dynamicState.direction;
            this.position = new Position(dynamicState.position);
            
            this.positionVector = new Vector(position);
            this.heading = new Vector(direction);
       
        }

        public bool IsClosestTeamMemberToBall()
        {
          return (team.playerClosestToBall == this);
        }
        public Ball Ball()
        {
            return team.pitch.ball;
        }

            





        #endregion


        #region Static Data

        public int playerNumber{get;set;}
        public string playerType { get; set; }
        public double kickingAbility { get; set; }
        public double runningAbility { get; set; }
        public double ballControlAbility { get; set; }
        public double tacklingAbility { get; set; }

  
        
        #endregion

        #region Dynamic Data
        public Position position { get; set;}
        public bool hasBall {get;set;}
        public double direction { get; set;}
        
        #endregion

        #region Private Data
        
        #endregion

        #region Protected Data

        

        //this player's role in the team
        public player_role role { get; protected set; }

        //a pointer to this player's team
        public Team team { get; set; }
 


        //the region that this player is assigned to.
        public int homeRegion { get; set; }

        //the region this player moves to before kickoff
        public int defaultRegion { get; set; }

        //the distance to the ball (in squared-space). This value is queried 
        //a lot so it's calculated once each time-step and stored here.
        public double distanceFromBall{get;set;}

  
        //the vertex buffer
        protected List<Vector> m_vecPlayerVB;
        //the buffer for the transformed vertices
        protected List<Vector> m_vecPlayerVBTrans;
        #endregion

        public const double MaxSpeed = 10.0;

        public Response action { get; set; }




    }
}
