package element;

public class Position {

	private double x;
	private double y;

	public Position() {
		x = 0;
		y = 0;
	}

	public Position(double x, double y) {
		this.x = x;
		this.y = y;
	}

	public double getX() {
		return x;
	}

	public void setX(double x) {
		this.x = x;
	}

	public double getY() {
		return y;
	}

	public void setY(double y) {
		this.y = y;
	}

}
