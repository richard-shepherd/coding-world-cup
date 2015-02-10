package main;

import element.Circle;
import element.Goal;

// class to store the dimensions of the ground (height, width...)
public class GroundData {
	double width;
	double height;
	Circle centre;
	Goal goalLeft;
	Goal goalRight;
	double gameLengthSeconds;

	public double getWidth() {
		return width;
	}

	public void setWidth(double width) {
		this.width = width;
	}

	public double getHeight() {
		return height;
	}

	public void setHeight(double height) {
		this.height = height;
	}

	public Circle getCentre() {
		return centre;
	}

	public void setCentre(Circle centre) {
		this.centre = centre;
	}

	public Goal getGoalLeft() {
		return goalLeft;
	}

	public void setGoalLeft(Goal goalLeft) {
		this.goalLeft = goalLeft;
	}

	public Goal getGoalRight() {
		return goalRight;
	}

	public void setGoalRight(Goal goalRight) {
		this.goalRight = goalRight;
	}

	public double getGameLengthSeconds() {
		return gameLengthSeconds;
	}

	public void setGameLengthSeconds(double gameLengthSeconds) {
		this.gameLengthSeconds = gameLengthSeconds;
	}

}
