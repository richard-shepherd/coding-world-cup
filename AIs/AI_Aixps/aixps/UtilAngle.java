package aixps;

import java.lang.Math;

class UtilAngle
{
	double dAngleRadian = 0;
	double dAngleDegree = 0;
	int		iAngleGame = 0;

	UtilAngle()
	{
		dAngleRadian = 0;
		dAngleDegree = 0;
		iAngleGame = 0;
	}

	UtilAngle(double dRadianValue)
	{
		refresh();
	}

	UtilAngle(double dVectorY, double dVectorX)
	{
		dAngleRadian = Math.atan2(dVectorY, dVectorX);
		refresh();
	}

	UtilAngle(UtilPoint pDest, UtilPoint pRef)
	{
		double dVectorX = 1;
		double dVectorY = 1;
		dVectorX = pDest.dValueX - pRef.dValueX;
		dVectorY = pDest.dValueY - pRef.dValueY;
		dAngleRadian = Math.atan2(dVectorY, dVectorX);
		refresh();
	}

	private void refresh()
	{
		dAngleDegree = dAngleRadian / 3.1415 * 180;
		iAngleGame = (int) dAngleDegree + 90;			
	}

	public double getAngle()
	{
		return dAngleRadian;
	}

	public double getAngleDegree()
	{
		return dAngleDegree;
	}

	public int getAngleIngame()
	{
		return iAngleGame;
	}	

}