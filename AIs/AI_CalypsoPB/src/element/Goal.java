package element;

public class Goal extends Circle {
	Position top;
	Position down;

	public Goal(Position center, double radius) {
		super(center, radius);

	}

	public Goal(Position center, double radius, Position top, Position down) {
		super(center, radius);
		this.top = top;
		this.down = down;
	}

	public Position getTop() {
		return top;
	}

	public void setTop(Position top) {
		this.top = top;
	}

	public Position getDown() {
		return down;
	}

	public void setDown(Position down) {
		this.down = down;
	}
}
