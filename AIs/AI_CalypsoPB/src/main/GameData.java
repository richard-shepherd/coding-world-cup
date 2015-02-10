package main;

import element.Ball;

public class GameData {

	private double currentTimeSeconds;
	private Ball ball;
	private double speed;
	private int controllingPlayerNumber;
	private boolean halfTime;

	private GroundData groundData;
	private TeamData teamData;

	public GameData() {
		groundData = new GroundData();
		teamData = new TeamData();
	}

	public GroundData getGroundData() {
		return groundData;
	}

	public void setGroundData(GroundData groundData) {
		this.groundData = groundData;
	}

	public TeamData getTeamData() {
		return teamData;
	}

	public void setTeamData(TeamData teamData) {
		this.teamData = teamData;
	}

	public double getCurrentTimeSeconds() {
		return currentTimeSeconds;
	}

	public void setCurrentTimeSeconds(double currentTimeSeconds) {
		this.currentTimeSeconds = currentTimeSeconds;
	}

	public Ball getBall() {
		return ball;
	}

	public void setBall(Ball ball) {
		this.ball = ball;
	}

	public double getSpeed() {
		return speed;
	}

	public void setSpeed(double speed) {
		this.speed = speed;
	}

	public int getControllingPlayerNumber() {
		return controllingPlayerNumber;
	}

	public void setControllingPlayerNumber(int controllingPlayerNumber) {
		this.controllingPlayerNumber = controllingPlayerNumber;
	}

	public boolean isHalfTime() {
		return halfTime;
	}

	public void setHalfTime(boolean halfTime) {
		this.halfTime = halfTime;
	}

}
