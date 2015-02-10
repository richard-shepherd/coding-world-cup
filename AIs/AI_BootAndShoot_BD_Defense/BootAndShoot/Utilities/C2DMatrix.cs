using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class C2DMatrix
    {

  
      private struct Matrix
      {

            public double _11, _12, _13;
            public double _21, _22, _23;
            public double _31, _32, _33;

            public void Zero()
            {
              _11=0.0; _12=0.0; _13=0.0;
              _21=0.0; _22=0.0; _23=0.0;
              _31=0.0; _32=0.0; _33=0.0;
            }

       };

       Matrix m_Matrix;

      //multiplies m_Matrix with mIn
      private void  MatrixMultiply(Matrix mIn)
      {
              Matrix mat_temp = new Matrix();
  
              //first row
              mat_temp._11 = (m_Matrix._11*mIn._11) + (m_Matrix._12*mIn._21) + (m_Matrix._13*mIn._31);
              mat_temp._12 = (m_Matrix._11*mIn._12) + (m_Matrix._12*mIn._22) + (m_Matrix._13*mIn._32);
              mat_temp._13 = (m_Matrix._11*mIn._13) + (m_Matrix._12*mIn._23) + (m_Matrix._13*mIn._33);

              //second
              mat_temp._21 = (m_Matrix._21*mIn._11) + (m_Matrix._22*mIn._21) + (m_Matrix._23*mIn._31);
              mat_temp._22 = (m_Matrix._21*mIn._12) + (m_Matrix._22*mIn._22) + (m_Matrix._23*mIn._32);
              mat_temp._23 = (m_Matrix._21*mIn._13) + (m_Matrix._22*mIn._23) + (m_Matrix._23*mIn._33);

              //third
              mat_temp._31 = (m_Matrix._31*mIn._11) + (m_Matrix._32*mIn._21) + (m_Matrix._33*mIn._31);
              mat_temp._32 = (m_Matrix._31*mIn._12) + (m_Matrix._32*mIn._22) + (m_Matrix._33*mIn._32);
              mat_temp._33 = (m_Matrix._31*mIn._13) + (m_Matrix._32*mIn._23) + (m_Matrix._33*mIn._33);

              m_Matrix = mat_temp;
        }




        public C2DMatrix()
        {
        //initialize the matrix to an identity matrix
        Identity();
        }

        //create an identity matrix
        public void Identity()
        {
              m_Matrix._11 = 1; m_Matrix._12 = 0; m_Matrix._13 = 0;

              m_Matrix._21 = 0; m_Matrix._22 = 1; m_Matrix._23 = 0;

              m_Matrix._31 = 0; m_Matrix._32 = 0; m_Matrix._33 = 1;
        }
  
        //create a transformation matrix
        public void Translate(double x, double y)
        {
              Matrix mat;
  
              mat._11 = 1; mat._12 = 0; mat._13 = 0;
  
              mat._21 = 0; mat._22 = 1; mat._23 = 0;
  
              mat._31 = x;    mat._32 = y;    mat._33 = 1;
  
              //and multiply
              MatrixMultiply(mat);
        }

        //create a scale matrix
        public void Scale(double xScale, double yScale)
        {
              Matrix mat;
  
              mat._11 = xScale; mat._12 = 0; mat._13 = 0;
  
              mat._21 = 0; mat._22 = yScale; mat._23 = 0;
  
              mat._31 = 0; mat._32 = 0; mat._33 = 1;
  
              //and multiply
              MatrixMultiply(mat);
        }

        //create a rotation matrix
        public void  Rotate(double rotation)
        {
            Matrix mat;

            double Sin = Math.Sin(rotation);
            double Cos = Math.Cos(rotation);
  
            mat._11 = Cos;  mat._12 = Sin; mat._13 = 0;
  
            mat._21 = -Sin; mat._22 = Cos; mat._23 = 0;
  
            mat._31 = 0; mat._32 = 0;mat._33 = 1;
  
            //and multiply
            MatrixMultiply(mat);
        }

        //create a rotation matrix from a fwd and side 2D vector
        public void  Rotate(Vector fwd, Vector side)
        {
              Matrix mat;
  
              mat._11 = fwd.x;  mat._12 = fwd.y; mat._13 = 0;
  
              mat._21 = side.x; mat._22 = side.y; mat._23 = 0;
  
              mat._31 = 0; mat._32 = 0;mat._33 = 1;
  
              //and multiply
              MatrixMultiply(mat);
        }

        //applys a transformation matrix to a std::vector of points
        public void TransformVector2Ds(List<Vector> Points)
        {
              for (int i=0; i<Points.Count; ++i)
              {
                double tempX =(m_Matrix._11 * Points[i].x) + (m_Matrix._21*Points[i].y) + (m_Matrix._31);

                double tempY = (m_Matrix._12*Points[i].x) + (m_Matrix._22*Points[i].y) + (m_Matrix._32);
  
                Points[i].x = tempX;

                Points[i].y = tempY;

              }
        }

        //applys a transformation matrix to a point
        public void TransformVector2Ds(Vector Point)
        {
              double tempX =(m_Matrix._11*Point.x) + (m_Matrix._21*Point.y) + (m_Matrix._31);

              double tempY = (m_Matrix._12*Point.x) + (m_Matrix._22*Point.y) + (m_Matrix._32);
  
              Point.x = tempX;

              Point.y = tempY;
        }

        //accessors to the matrix elements
        public void _11(double val){m_Matrix._11 = val;}
        public void _12(double val){m_Matrix._12 = val;}
        public void _13(double val){m_Matrix._13 = val;}

        public void _21(double val) { m_Matrix._21 = val; }
        public void _22(double val) { m_Matrix._22 = val; }
        public void _23(double val) { m_Matrix._23 = val; }

        public void _31(double val) { m_Matrix._31 = val; }
        public void _32(double val) { m_Matrix._32 = val; }
        public void _33(double val) { m_Matrix._33 = val; }

};

}
