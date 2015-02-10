using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GermanyYogesh
{
    /// <summary>
    /// Represents a vector between two Positions.
    /// </summary>
    public class Vector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Vector(Position from, Position to)
        {
            this.x = to.x - from.x;
            this.y = to.y - from.y;
        }

        /// <summary>
        /// The length of the vector.
        /// </summary>
        public double Length
        {
            get
            {
                double xSquared = this.x * this.x;
                double ySquared = this.y * this.y;
                return Math.Sqrt(xSquared + ySquared);
            }
        }

        /// <summary>
        /// Returns a new Vector, scaled to the length passed in.
        /// </summary>
        public Vector getScaledVector(double length)
        {
            if(this.Length == 0.0)
            {
                return new Vector(0.0, 0.0);
            }
            else
            {
                double x = this.x * length / this.Length;
                double y = this.y * length / this.Length;
                return new Vector(x, y);
            }
        }

        public double x;
        public double y;
    }
}
