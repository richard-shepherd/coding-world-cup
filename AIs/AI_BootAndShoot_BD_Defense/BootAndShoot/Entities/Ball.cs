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
            this.position = new Position(ball.position);
            this.vector = new Vector(ball.vector);
            this.speed = (double)ball.speed;
            this.controllingPlayerNumber = ball.controllingPlayerNumber;
        }

        public double TimeToCoverDistance(Vector A, Vector B, double ballSpeed = 0)
        {
            if(ballSpeed == 0)
            {
                ballSpeed = speed;
            }

            //calculate the velocity at B using the equation
            //
            //  v^2 = u^2 + 2as
            //

            //first calculate s (the distance between the two positions)
            double DistanceToCover = Vector.VecDistance(A, B);

            //assuming a is 1m/s2
            double term = ballSpeed * ballSpeed - 2.0 * DistanceToCover * friction;

            //if  (u^2 + 2as) is negative it means the ball cannot reach point B.
            if (term <= 0.0) return -1.0;

            double v = Math.Sqrt(term);

            //it IS possible for the ball to reach B and we know its speed when it
            //gets there, so now it's easy to calculate the time using the equation
            //
            //    t = v-u
            //        ---
            //         a
            //
            return (ballSpeed - v) / friction;
        }

        public Position StopPosition(GamePitch pitch)
        {
            double distance = (speed * speed) / (2 * friction);
            Vector scaledVector = vector.getScaledVector(distance);
            Position stopPosition = position.getPositionPlusVector(scaledVector);
            stopPosition.AdjustBounce(pitch);
            return stopPosition;
        }


        #endregion
        #region Singlton 
        private static Ball m_instance;
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
        private Ball()
        {
        }
        
        #endregion
        #region  Data

        public Position position { get; set; }
        public Vector vector { get; set;}
        public double speed { get; set;}
        public int controllingPlayerNumber{get;set;}

        public const double maxSpeed = 30.0;
        public const double friction = 10.0;

        #endregion

    }
}
