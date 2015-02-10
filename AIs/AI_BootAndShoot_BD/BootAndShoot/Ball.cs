using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class Ball
    {
        #region Public Methods
        public void SetValues(dynamic ball)
        {
            this.m_position = new Position(ball.position);
            this.m_vector = new Vector(ball.vector);
            this.m_dSpeed = (double)ball.speed;
            this.m_iControllingPlayerNumber = ball.controllingPlayerNumber;
        }
        //gives the position where ball will stop
        public Position BallStopPosition(dynamic pitch)
        {
            double distance = (speed * speed) / (2 * 10);
            Vector scaledVector = vector.getScaledVector(distance);
            Position stopPosition = position.getPositionPlusVector(scaledVector);

            // Did the ball bounce?
            if (stopPosition.x < 0.0)
            {
                stopPosition.x *= -1.0;
            }
            if (stopPosition.x > pitch.width)
            {
                stopPosition.x = pitch.width - (stopPosition.x - pitch.width);
            }

            if (stopPosition.y < 0.0)
            {
                stopPosition.y *= -1.0;
            }

            if (stopPosition.y > pitch.height)
            {
                stopPosition.y = pitch.height - (stopPosition.y - pitch.height);
            }
            return stopPosition;
        }
        public Position position
        {
            get
            {
                return m_position;
            }
            set
            {
                m_position = value;
            }
        }
         public Vector vector
        {
            get
            {
                return m_vector;
            }
            set
            {
                m_vector = value;
            }
        }
        public double speed
        {
            get
            {
                return m_dSpeed;
            }
            set
            {
                m_dSpeed = value;
            }
        }
        public int controllingPlayerNumber
        {
            get
            {
                return m_iControllingPlayerNumber;
            }
            set
            {
                m_iControllingPlayerNumber = value;
            }
        }
        public static Ball instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new Ball();
                }
                return m_instance;
            }
        }

        #endregion
        #region Private Methods
        
        private Ball(){}
        
        #endregion

        #region Private Data

        private Position m_position;
        private Vector m_vector;
        private double m_dSpeed;
        private int m_iControllingPlayerNumber;

        private static Ball m_instance;
        #endregion


    }
}
