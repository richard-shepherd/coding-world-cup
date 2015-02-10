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
            this.x = x; 
            this.y = y; 
        }

        /// <summary>
        /// Constructor from a dynamic object with x and y properties.
        /// </summary>
        public Position(dynamic position)
        {
            this.x = (double)position.x;
            this.y = (double)position.y;
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
            return new Position(Math.Round((this.x + vector.x),4), Math.Round((this.y + vector.y),4));
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

        // The x and y coordinates...
        public double x;
        public double y;

    }
}
