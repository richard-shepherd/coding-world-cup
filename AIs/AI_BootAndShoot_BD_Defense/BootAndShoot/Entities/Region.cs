using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class Region
    {

        public enum region_modifier{halfsize, normal};



        public double top { get; set; }
        public double left { get; set; }
        public double right { get; set; }
        public double bottom { get; set; }


        public double Width() { return Math.Abs(right - left); }
        public double Height() { return Math.Abs(top - bottom); } 
        public double Length() {  return Math.Max(Width(), Height());} 
        public double Breadth() { return Math.Min(Width(), Height()); }

        public Vector center { get; set; }

        public int id { get; set; }






        public Region()
        {
            top = 0;
            bottom = 0;
            left = 0;
            right = 0;
        }

        public Region(double dleft, double dtop, double dright, double dbottom, int iid = -1)
        {
            top = dtop;
            bottom = dbottom;
            left = dleft;
            right = dright;
            id = iid;

            //calculate center of region
            center = new Vector( (left+right)*0.5, (top+bottom)*0.5 );


        }

 


        public bool Inside(Vector pos, region_modifier r = region_modifier.normal)
        {
            if (r == region_modifier.normal)
            {
            return ((pos.x > left) && (pos.x < right) &&
                    (pos.y > top) && (pos.y < bottom));
            }
            else
            {
            double marginX = Width() * 0.25;
            double marginY = Height() * 0.25;

            return ((pos.x > (left+marginX)) && (pos.x < (right-marginX)) &&
                    (pos.y > (top+marginY)) && (pos.y < (bottom-marginY)));
            }

        }

        public Vector GetRandomPosition()
        {
            Random r = new Random();
            double r1 = r.Next((int)left, (int)right);
            double r2 = r.Next((int)top, (int)bottom);
            return new Vector(r1,r2);
        }


    }
}
