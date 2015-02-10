using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrillTown
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

        /// <summary>
        /// Returns the dot product with the passed in vector.
        /// </summary>
        public double Dot(Vector v)
        {
            return ((this.x * v.x) + (this.y * v.y)) / (this.Length * v.Length);
        }

        /// <summary>
        /// Returns the distance of point, end, from the vector
        ///
        ///               end
        ///             /|
        ///            / |
        ///         a /  |distance
        ///          /   |
        ///         /    |
        ///   start ------------------ vector
        ///                 b
        ///
        /// a.b = |a| * |b| * cos(theta)
        /// a^2 = b^2 + d^2
        /// </summary>
        public double DistanceTo(Position start, Position end)
        {
            Vector point    = new Vector(start, end);
            double cosTheta = ((this.x * point.x) + (this.y * point.y)) / (this.Length * point.Length);
            double distance = point.Length * Math.Sqrt(1 - cosTheta * cosTheta);

            return distance;
        }

        /// <summary>
        /// Returns the distance of point end from the vector and also distance divided by vector length.
        /// </summary>
        public double [] DistanceTo2(Position start, Position end)
        {
            Vector point = new Vector(start, end);
            double dot = ((this.x * point.x) + (this.y * point.y)) / (this.Length * point.Length);
            double projected = dot / this.Length;
            double distance = Math.Sqrt(point.Length * point.Length - projected * projected);
            return new double [] { distance, distance / point.Length };
        }

        public double x;
        public double y;
    }

}
