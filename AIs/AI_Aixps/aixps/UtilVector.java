package aixps;

import aixps.UtilPoint;

import java.lang.Math;

class UtilVector
{

	UtilPoint xVector;
	double	  dLength;

	public UtilVector()
	{
		xVector = new UtilPoint();
		xVector.set(0,0);
		dLength = 0;
	}

	public UtilVector(double dVectorX, double dVectorY)
	{
		double x = dVectorX;
		double y = dVectorY;
		xVector = new UtilPoint();
		xVector.set(dVectorX, dVectorY);
		dLength = Math.sqrt((x)*(x) + (y)*(y));
	}	

	public UtilVector(UtilVector xVector)
	{
		this.xVector = new UtilPoint();
		xVector.set(xVector.xVector.dValueX, xVector.xVector.dValueY);
		dLength = xVector.dLength;
	}

	public UtilVector(UtilPoint xPoint)
	{
		double x = xPoint.dValueX;
		double y = xPoint.dValueY;		
		xVector = new UtilPoint();
		xVector.set(xPoint.dValueX, xPoint.dValueY);
		dLength = Math.sqrt((x)*(x) + (y)*(y));
	}

	public void set(double dVectorX, double dVectorY)
	{
		double x = dVectorX;
		double y = dVectorY;		
		xVector.set(dVectorX, dVectorY);
		dLength = Math.sqrt((x)*(x) + (y)*(y));
	}	
}