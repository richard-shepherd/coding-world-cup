using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    /// <summary>
    /// Represents a vector between two Positions.
    /// </summary>
    public class Vector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// 
        public Vector()
        {
            this.x = 0.0;
            this.y = 0.0;
        }
        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public Vector(dynamic data)
        {
            this.x = (double)data.x;
            this.y = (double)data.y;
        }

        public Vector(Position data)
        {
            this.x = data.x;
            this.y = data.y;
        }

        /**
         * vectorFromDirection
         * -------------------
         * Returns a vector (with unit length) that points
         * in the direction passed in.
         *
         * The direction is in degrees measured clockwise from vertical.
         */
        public Vector(double direction) 
        {
            var angle = 90.0 - direction;
            var radians = angle * Math.PI / 180.0;
            this.x = Math.Cos(radians);
            this.y = -1.0 * Math.Sin(radians);
        }
        public Vector(Position from, Position to)
        {
            this.x = to.x - from.x;
            this.y = to.y - from.y;
        }

        public Vector(Vector from, Vector to)
        {
            this.x = to.x - from.x;
            this.y = to.y - from.y;
        }
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return String.Format("x:{0}, y:{1}", this.x, this.y);
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
        /// The Squeare of length of the vector.
        /// </summary>
        public double SquareLength
        {
            get
            {
                double xSquared = this.x * this.x;
                double ySquared = this.y * this.y;
                return (xSquared + ySquared);
            }
        }
        /// <summary>
        ///   calculates the dot product
        /// </summary>
         public double Dot(Vector v2)
        {
            return x*v2.x + y*v2.y;
        }

        public static double VecDistanceSq(Vector v1, Vector v2)
        {

          double ySeparation = v2.y - v1.y;
          double xSeparation = v2.x - v1.x;

          return ySeparation*ySeparation + xSeparation*xSeparation;
        }


        public static double VecDistance(Vector v1, Vector v2)
        {

            double ySeparation = v2.y - v1.y;
            double xSeparation = v2.x - v1.x;

            return Math.Sqrt(ySeparation*ySeparation + xSeparation*xSeparation);
        }

         /// <summary>
        /// Normalize Given Vector
        /// </summary>
        public void Normalize()
        { 
            double length = this.Length;

             if (length > Double.Epsilon)
             {
                this.x /= length;
                this.y /= length;
             }   
        }
         /// <summary>
        /// Normalize Vector
        /// </summary>
        public static Vector VecNormalize(Vector v)
        {
            Vector vec = (Vector)v.MemberwiseClone();

            double vector_length = vec.Length;

            if (vector_length > Double.Epsilon)
            {
                 vec.x /= vector_length;
                 vec.y /= vector_length;
            }

             return vec;
        }

        /// <summary>
        /// Normalize Vector
        /// </summary>
        public static  Vector PointToLocalSpace(Vector point,  Vector AgentHeading, Vector AgentSide, Vector AgentPosition)
        {

	          //make a copy of the point
              Vector TransPoint = (Vector)point.MemberwiseClone();
  
              //create a transformation matrix
	          C2DMatrix matTransform = new C2DMatrix();

              double Tx = -AgentPosition.Dot(AgentHeading);
              double Ty = -AgentPosition.Dot(AgentSide);

              //create the transformation matrix
              matTransform._11(AgentHeading.x); matTransform._12(AgentSide.x);
              matTransform._21(AgentHeading.y); matTransform._22(AgentSide.y);
              matTransform._31(Tx);           matTransform._32(Ty);
	
              //now transform the vertices
              matTransform.TransformVector2Ds(TransPoint);

              return TransPoint;
        }

        /// <summary>
        /// Perpendicular Vector.
        /// </summary>
        public Vector Perp()
        {
            return new Vector(-y, x);
        }
        public Position ToPosition()
        {
            return new Position(this);
        }

        public Vector Subtract(Vector rhs)
        {
            Vector result = new Vector(this);
            result.x -= rhs.x;
            result.y -= rhs.y;
  
            return result;
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

        //------------------------- LengthSq -------------------------------------
        //
        //  returns the squared length of a 2D vector
        //------------------------------------------------------------------------
        public double LengthSq()
        {
          return (x * x + y * y);
        }
        //------------------------ Sign ------------------------------------------
        //
        //  returns positive if v2 is clockwise of this vector,
        //  minus if anticlockwise (Y axis pointing down, X axis to right)
        //------------------------------------------------------------------------
        public enum Rotation {clockwise = 1, anticlockwise = -1};

        public int Sign(Vector v2)
        {
          if (y*v2.x > x*v2.y)
          { 
            return (int)Rotation.anticlockwise;
          }
          else 
          {
            return (int)Rotation.clockwise;
          }
        }

        public double x{get;set;}
        public double y{get;set;}
    }
}
