package aixps;

class UtilPoint
{
	double dValueX = 0;
	double dValueY = 0;

	public UtilPoint()
	{
		dValueX = 0;
		dValueY = 0;
	}

	public UtilPoint(double dPositionX, double dPositionY)
	{
		dValueX = dPositionX;
		dValueY = dPositionY;
	}

	public UtilPoint(UtilPoint xPoint)
	{
		if (xPoint != null)
		{
			dValueX = xPoint.dValueX;
			dValueY = xPoint.dValueY;
		}
	}

	public void set(double dPositionX, double dPositionY)
	{
		dValueX = dPositionX;
		dValueY = dPositionY;
	}

	static public double getDistance(double x1, double y1, double x2, double y2)
	{
		double d = 9999;

		//System.err.println("coords: x:" + x1 + " y:" + y1 + "x2:" + x2 + "y2:" + y2);
		d = Math.sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));

		return d;
	}

	static public double getDistance(UtilPoint p1, UtilPoint p2)
	{
		double d = 9999;

		double x1 = 0;
		double y1 = 0;
		double x2 = 0;
		double y2 = 0;

		x1 = p1.dValueX;
		y1 = p1.dValueY;
		x2 = p2.dValueX;
		y2 = p2.dValueY;

		d = Math.sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));

		return d;
	}

	public String toString()
	{
		return "(" + dValueX + "/" + dValueY + ")";
	}
}