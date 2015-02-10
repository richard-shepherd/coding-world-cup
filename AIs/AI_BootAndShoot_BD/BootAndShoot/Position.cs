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
            this.x = Math.Round((double)position.x, 3);
            this.y = Math.Round((double)position.y, 3);
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
            double x = Math.Round(this.x + vector.x, 3);
            double y = Math.Round(this.y + vector.y,3);
            return new Position(x,y);
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
            return (resultInDegree + 90) % 360;
           // Position diff = new Position(xDiff, yDiff);
           // return result;
        }

        public bool IsEqual(Position obj)
        {
            double x1 = Math.Round(this.x,3);
            double y1 = Math.Round(this.y,3);

            double x2 = Math.Round(obj.x,3);
            double y2 = Math.Round(obj.y, 3);


            return ((x1 == x2) && (y1 == y2));
        }
        public void AddVector(Vector v)
        {
            this.x += v.x;
            this.y += v.y;
        }
        public void clear()
        {
            x = 0;
            y = 0;
        }
        // The x and y coordinates...
        public double x;
        public double y;

    }
}
