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

        // The x and y coordinates...
        public double x;
        public double y;

    }
}
