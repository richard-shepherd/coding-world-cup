package element;

public class Player {

	private Position position;
	private double direction;
	private boolean hasBall;
	private double energy;

	private String type;
	private int playerNumber;

	private double kickingAbility;
	private double runningAbility;
	private double ballControlAbility;
	private double tacklingAbility;

	private String role;
	public static final String GOALKEEPER = "goalKeeper";
	public static final String LEFT_DEFENDER = "leftDefender";
	public static final String CENTER_DEFENDER = "centerDefender";
	public static final String RIGHT_DEFENDER = "rightDefender";
	public static final String LEFT_FORWARD = "leftForward";
	public static final String RIGHT_FORWARD = "rightForward";

	public Position getPosition() {
		return position;
	}

	public void setPosition(Position position) {
		this.position = position;
	}

	public double getDirection() {
		return direction;
	}

	public void setDirection(double direction) {
		this.direction = direction;
	}

	public boolean hasBall() {
		return hasBall;
	}

	public void setHasBall(boolean hasBall) {
		this.hasBall = hasBall;
	}

	public String getType() {
		return type;
	}

	public void setType(String type) {
		this.type = type;
	}

	public int getPlayerNumber() {
		return playerNumber;
	}

	public void setPlayerNumber(int playerNumber) {
		this.playerNumber = playerNumber;
	}

	public double getKickingAbility() {
		return kickingAbility;
	}

	public void setKickingAbility(double kickingAbility) {
		this.kickingAbility = kickingAbility;
	}

	public double getRunningAbility() {
		return runningAbility;
	}

	public void setRunningAbility(double runningAbility) {
		this.runningAbility = runningAbility;
	}

	public double getBallControlAbility() {
		return ballControlAbility;
	}

	public void setBallControlAbility(double ballControlAbility) {
		this.ballControlAbility = ballControlAbility;
	}

	public double getTacklingAbility() {
		return tacklingAbility;
	}

	public void setTacklingAbility(double tacklingAbility) {
		this.tacklingAbility = tacklingAbility;
	}

	public double setEnergy() {
		return energy;
	}

	public void setEnergy(double energy) {
		this.energy = energy;
	}

	public boolean isHasBall() {
		return hasBall;
	}

	public double getEnergy() {
		return energy;
	}

	public String getRole() {
		return role;
	}

	public void setRole(String role) {
		this.role = role;
	}


}
