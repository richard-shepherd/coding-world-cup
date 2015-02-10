using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class Geometry
    {
        public const double Pi = 3.14159;
        public const double TwoPi = Pi * 2;
        public const double HalfPi = Pi / 2;
        public const double QuarterPi = Pi / 4;
        public static bool GetTangentPoints (Vector C, double R, Vector P, Vector T1, Vector T2)
        {
            Vector PmC = P.Subtract(C);
            double SqrLen = PmC.LengthSq();
            double RSqr = R*R;
            if ( SqrLen <= RSqr )
            {
                // P is inside or on the circle
                return false;
            }

            double InvSqrLen = 1/SqrLen;
            double Root = Math.Sqrt(Math.Abs(SqrLen - RSqr));

            T1.x = C.x + R*(R*PmC.x - PmC.y*Root)*InvSqrLen;
            T1.y = C.y + R*(R*PmC.y + PmC.x*Root)*InvSqrLen;
            T2.x = C.x + R*(R*PmC.x + PmC.y*Root)*InvSqrLen;
            T2.y = C.y + R*(R*PmC.y - PmC.x*Root)*InvSqrLen;

            return true;
        }
    }
}
