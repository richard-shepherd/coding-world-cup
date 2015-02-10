using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class GamePitch
    {

        public Ball ball { get; set; }
        public Region playingArea { get; set; }

        public void Update()
        {

        }

        public void SetData(dynamic data)
        {
            width = (double)data.width;
            height = (double)data.height;
            goalCentre = (double)data.goalCentre;
            goalY1 = (double)data.goalY1;
            goalY2 = (double)data.goalY2;
            centreSpot = new Position(data.centreSpot);
            centreCircleRadius = (double)data.centreCircleRadius;
            goalAreaRadius = (double)data.goalAreaRadius;
                
        }
        private void CreateRegions(double width, double height)
        {
            //index into the vector
            regions = new Dictionary<int, Region>();
            int count = 0;
            for (int col = 0; col < 10; ++col)
            {
                for (int row = 0; row < 5; ++row)
                {
                    var region = new Region(playingArea.left + col * width,
                                                 playingArea.top + row * height,
                                                 playingArea.left + (col + 1) * width,
                                                 playingArea.top + (row + 1) * height,
                                                 ++count);
                    regions.Add(count ,region);
                }
            }
        }

        #region Data

        public double width { get; set; }
        public double height { get; set; }
        public double goalCentre { get; set; }
        public double goalY1 { get; set; }
        public double goalY2 { get; set; }
        public Position centreSpot { get; set; }
        public double centreCircleRadius { get; set; }
        public double goalAreaRadius { get; set; }

        public Dictionary<int,Region> regions { get;  set; }
        #endregion

        #region Singlton
        private static GamePitch m_instance;
        public static GamePitch instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new GamePitch();
                }
                return m_instance;
            }
        }

        public bool GoalKeeperHasBall() 
        {
            return false;
        }

        private GamePitch() 
        {
            ball = Ball.instance;
            //define the playing area
            playingArea = new Region(0, 0, 100, 50);

            //create the regions  
            CreateRegions(playingArea.Width() / (double)10, playingArea.Height() / (double)5);

        }
        
        #endregion
    }
}
