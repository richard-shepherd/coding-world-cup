using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    /// <summary>
    /// Holds an (x, y) position, with some helper functions.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Position()
        {
            x = 0.000;
            y = 0.000;
        }
        public Position(double x, double y)
        { 
            this.x = Math.Round(x,3);
            this.y = Math.Round(y,3); 
        }

        /// <summary>
        /// Constructor from a dynamic object with x and y properties.
        /// </summary>
        public Position(dynamic position)
        {
            this.x = Math.Round((double)position.x,3);
            this.y = Math.Round((double)position.y,3);
        }
        public Position(Vector position)
        {
            this.x = Math.Round((double)position.x,3);
            this.y = Math.Round((double)position.y,3);
        }
        /**
         * pointFromDirection
         * ------------------
         * Returns a point 'length' away from 'start' in the direction specified.
         */
        public Position(Position start, double direction, double length) 
        {
            Vector vector = new Vector(direction);
            vector = vector.getScaledVector(length);
            var result = new Position(start.x, start.y);
            result = result.getPositionPlusVector(vector);
            this.x = result.x;
            this.y = result.y;
        }

        /// <summary>
        /// Returns the distance between two positions.
        /// </summary>
        public double distanceFrom(Position other)
        {
            double diffX = this.x - other.x;
            double diffY = this.y - other.y;
            double diffXSquared = diffX * diffX;
            double diffYSquared = diffY * diffY;
            double distance = Math.Sqrt(diffXSquared + diffYSquared);
            return distance;
        }
        /// <summary>
        /// Returns a new position from this position plus the vector passed in.
        /// </summary>
        public Position getPositionPlusVector(Vector vector)
        {
            return new Position(this.x + vector.x, this.y + vector.y);
        }

        public Vector ToVector()
        {
            return new Vector(this);
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return String.Format("x:{0}, y:{1}", this.x, this.y);
        }

        /// <summary>
        /// Returns a JSObject holding this data.
        /// </summary>
        public JSObject toJSObject()
        {
            var jsObject = new JSObject();
            jsObject.add("x", this.x);
            jsObject.add("y", this.y);
            return jsObject;
        }

        public double getAngle(Position p2)
        {
            //double angleFromXAxis = Math.Atan ((p2.y - this.y ) / (p2.x - this.x ) ); // where y = m * x + K
            //return  (p2.x - this.x < 0) ? (angleFromXAxis + Math.PI) : angleFromXAxis; // The will go to the correct Quadrant

            double xDiff = p2.x - this.x;
            double yDiff = p2.y - this.y;
            
            double result = Math.Atan2(yDiff, xDiff);
            if(result < 0)
            {
                result += (2 * Math.PI);
            }
            double resultInDegree = result * 180.0 / Math.PI;
            return Math.Round((resultInDegree + 90) % 360,3);
           // Position diff = new Position(xDiff, yDiff);
           // return result;
        }

        public bool IsEqual(Position obj)
        {

            return ((this.x == obj.x) && (this.y == obj.y));
        }

        public void clear()
        {
            x = 0;
            y = 0;
        }

        public void AdjustBounce(GamePitch pitch)
        {

            if (this.x < 0.0)
            {
                this.x *= -1.0;
            }
            if (this.x > pitch.width)
            {
                this.x = pitch.width - (this.x - pitch.width);

            }

            if (this.y < 0.0)
            {
                this.y *= -1.0;
            }

            if (this.y > pitch.height)
            {
                this.y = pitch.height - (this.y - pitch.height);
            }
        }
        // The x and y coordinates...
        public double x;
        public double y;

    }
}
