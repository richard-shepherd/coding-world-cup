package utils;

import element.Circle;
import element.Position;

public class GeometryUtils {

	public static double getDistance(Position p1, Position p2) {
		return Math.sqrt(Math.pow((p1.getX() - p2.getX()), 2)
				+ Math.pow((p1.getY() - p2.getY()), 2));
	}

	public static double getAngle(Position P1, Position P2) {
		double angle = 90 + Math.toDegrees((Math.atan2(P2.getY() - P1.getY(),
				P2.getX() - P1.getX())));
		if (angle < 0)
			angle += 360;
		return angle;
	}

	public static boolean isInRectangle(Position target, Position topLeft,
			Position downRight) {
		return target.getX() <= downRight.getX()
				&& target.getX() >= topLeft.getX()
				&& target.getY() <= downRight.getY()
				&& target.getX() >= topLeft.getX();
	}

	public static Position getPositionInCircle(Position target, Circle circle) {
		Position center = circle.getCenter();
		double distance = getDistance(target, center);
		if (distance <= circle.getRadius()) {
			return target;
		}
		double destinationX = center.getX() + (target.getX() - center.getX())
				* circle.getRadius() / distance;
		double destinationY = center.getY() + (target.getY() - center.getY())
				* circle.getRadius() / distance;
		return new Position(destinationX, destinationY);
	}

}
