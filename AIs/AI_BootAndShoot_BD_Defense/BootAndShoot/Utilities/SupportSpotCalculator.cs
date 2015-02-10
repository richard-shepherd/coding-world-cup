using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class SupportSpotCalculator
    {

          private class SupportSpot
          {

              public Vector spot { get; set; }

              public double score { get; set; }

            public SupportSpot(Vector pos, double value)
            {
                spot = pos;
                score = value;
            }

            public SupportSpot()
            {
                spot = new Vector();
                score = 0.0;
            }
       
          }

          private Team team;

          private List<SupportSpot> spots;

          //a pointer to the highest valued spot from the last update
          private SupportSpot bestSupportingSpot;

          //this will regulate how often the spots are calculated (default is
          //one update per second)
          //Regulator* m_pRegulator;


          public SupportSpotCalculator(int numX, int numY, Team team)
          {
              this.team = team;
              spots = new List<SupportSpot>();
              Region PlayingField = this.team.pitch.playingArea;

              //calculate the positions of each sweet spot, create them and 
              //store them in m_Spots
              double HeightOfSSRegion = PlayingField.Height() * 0.8;
              double WidthOfSSRegion  = PlayingField.Width() * 0.9;
              double SliceX = WidthOfSSRegion / numX ;
              double SliceY = HeightOfSSRegion / numY;

              double left  = PlayingField.left + (PlayingField.Width()-WidthOfSSRegion)/2.0 + SliceX/2.0;
              double right = PlayingField.right - (PlayingField.Width()-WidthOfSSRegion)/2.0 - SliceX/2.0;
              double top   = PlayingField.top + (PlayingField.Height()-HeightOfSSRegion)/2.0 + SliceY/2.0;

              Vector spot;
              SupportSpot supportSpot;
              for (int x=0; x<(numX/2)-1; ++x)
              {
                for (int y=0; y<numY; ++y)
                {      
                  if (team.playingDirection == Team.DirectionType.LEFT)
                  {
                    spot = new Vector(left + x * SliceX, top + y * SliceY);
                    supportSpot = new SupportSpot(spot, 0.0);
                    spots.Add(supportSpot);
                  }

                  else
                  {
                    spot = new Vector(right - x * SliceX, top + y * SliceY);
                    supportSpot = new SupportSpot(spot, 0.0);
                    spots.Add(supportSpot);
                  }
                }
              }
  


          }

         //this method iterates through each possible spot and calculates its
         //score.
          public Vector DetermineBestSupportingPosition()
          {


              //reset the best supporting spot
              bestSupportingSpot = new SupportSpot();

              double bestScoreSoFar = 0.0;

              foreach (SupportSpot curSpot in spots)
              {
                  //first remove any previous score. (the score is set to one so that
                  //the viewer can see the positions of all the spots if he has the 
                  //aids turned on)
                  curSpot.score = 1.0;

                  //Test 1. is it possible to make a safe pass from the ball's position 
                  //to this position?
                  Vector vControllingPlayerPos = new Vector(team.controllingPlayer.position);
                  if (team.IsPassSafeFromAllOpponents(vControllingPlayerPos, curSpot.spot, null, 30.0))
                  {
                      curSpot.score += 2.0;
                  }


                  //Test 2. Determine if a goal can be scored from this position.  
                  if (team.CanShoot(curSpot.spot , 30.0))
                  {
                      curSpot.score += 1.0;
                  }

                  //Test 3. calculate how far this spot is away from the controlling
                  //player. The further away, the higher the score. Any distances further
                  //away than OptimalDistance pixels do not receive a score.
                  if (team.supportingPlayer != null)
                  {
                      const double OptimalDistance = 22.5;

                      double dist = Vector.VecDistance(new Vector(team.controllingPlayer.position), curSpot.spot);

                      double temp = Math.Abs(OptimalDistance - dist);

                      if (temp < OptimalDistance)
                      {

                          //normalize the distance and add it to the score
                          curSpot.score += 2.0 * (OptimalDistance - temp) / OptimalDistance;
                      }
                  }

                  //check to see if this spot has the highest score so far
                  if (curSpot.score > bestScoreSoFar)
                  {
                      bestScoreSoFar = curSpot.score;

                      bestSupportingSpot = curSpot;
                  }  
              }

              return bestSupportingSpot.spot;
          }




         //returns the best supporting spot if there is one. If one hasn't been
         //calculated yet, this method calls DetermineBestSupportingPosition and
         //returns the result.
          public Vector GetBestSupportingSpot() 
          {
              if (bestSupportingSpot != null)
              {
                  return bestSupportingSpot.spot;
              }

              else
              {
                  return DetermineBestSupportingPosition();
              }
          }
    }
}
