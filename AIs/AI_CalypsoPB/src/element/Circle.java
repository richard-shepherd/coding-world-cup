package element;

import utils.GeometryUtils;

public class Circle {

	Position center;

	double radius;
	static public final int CentreCircle = 0;
	static public final int LeftGoalCircle = 1;
	static public final int RightGoalCircle = 2;

	int circleType;

	public Circle(Position center, double radius) {
		this.center = center;
		this.radius = radius;
	}

	public boolean isInArea(Position p) {
		double distance = GeometryUtils.getDistance(p, center);
		return distance <= radius;
	}

	public Position getCenter() {
		return center;
	}

	public void setCenter(Position center) {
		this.center = center;
	}

	public double getRadius() {
		return radius;
	}

	public void setRadius(double radius) {
		this.radius = radius;
	}

	public Circle getMovedCircled(int x, int y){
		Position centerNew = new Position (getCenter().getX()+x,getCenter().getY()+y);
		return new Circle(centerNew,getRadius());
	}







}
