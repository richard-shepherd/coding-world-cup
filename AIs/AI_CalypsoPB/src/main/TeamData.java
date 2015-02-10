package main;

import java.util.HashMap;
import java.util.Map;

import element.Player;

public class TeamData {

	private int teamNumber;
	private Map<Integer, Player> playerMap;
	private int opponentTeamNumber;
	private Map<Integer, Player> opponentPlayerMap;
	private int teamToKickoff = 0;

	private String teamName;
	private int teamScore;
	private String teamdirection;
	private String opponentTeamName;
	private int opponentScore;
	private String opponentTeamdirection;

	public TeamData() {
		playerMap = new HashMap<Integer, Player>();
		opponentPlayerMap = new HashMap<Integer, Player>();
	}

	public TeamData(int teamNumber, Map<Integer, Player> playerMap,
			int opponentTeamNumber, Map<Integer, Player> opponentPlayerMap) {
		this.teamNumber = teamNumber;
		this.playerMap = playerMap;
		this.opponentPlayerMap = opponentPlayerMap;
		this.opponentTeamNumber = opponentTeamNumber;
	}

	public int getTeamNumber() {
		return teamNumber;
	}

	public int getOpponentTeamNumber() {
		return opponentTeamNumber;
	}

	public Map<Integer, Player> getPlayerMap() {
		return playerMap;
	}

	public Map<Integer, Player> getOpponentPlayerMap() {
		return opponentPlayerMap;
	}

	public String getTeamName() {
		return teamName;
	}

	public void setTeamName(String teamName) {
		this.teamName = teamName;
	}

	public int getTeamScore() {
		return teamScore;
	}

	public void setTeamScore(int teamScore) {
		this.teamScore = teamScore;
	}

	public String getTeamdirection() {
		return teamdirection;
	}

	public void setTeamdirection(String teamdirection) {
		this.teamdirection = teamdirection;
	}

	public String getOpponentTeamName() {
		return opponentTeamName;
	}

	public void setOpponentTeamName(String teamName) {
		this.opponentTeamName = teamName;
	}

	public int getOpponentScore() {
		return opponentScore;
	}

	public void setOpponentScore(int opponentTeamScore) {
		this.opponentScore = opponentTeamScore;
	}

	public String getOpponentTeamDirection() {
		return opponentTeamdirection;
	}

	public void setOpponentTeamDirection(String opponentTeamDirection) {
		this.opponentTeamdirection = opponentTeamDirection;
	}

	public int getTeamToKickoff() {
		return teamToKickoff;
	}

	public void setTeamToKickoff(int teamToKickoff) {
		this.teamToKickoff = teamToKickoff;
	}

	public void setTeamNumber(int teamNumber) {
		this.teamNumber = teamNumber;
	}

	public void setPlayerMap(Map<Integer, Player> playerMap) {
		this.playerMap = playerMap;
	}

	public void setOpponentTeamNumber(int opponentTeamNumber) {
		this.opponentTeamNumber = opponentTeamNumber;
	}

	public void setOpponentPlayerMap(Map<Integer, Player> opponentPlayerMap) {
		this.opponentPlayerMap = opponentPlayerMap;
	}

	public void setTeamsDirectionAndName(String teamDirection1,String teamDirection2,String teamName1,
			String teamName2) {
		if (getTeamNumber() == 1) {
			setTeamdirection(teamDirection1);
			setOpponentTeamDirection(teamDirection2);
			setTeamName(teamName1);
			setOpponentTeamName(teamName2);
		} else {
			setTeamdirection(teamDirection2);
			setOpponentTeamDirection(teamDirection1);
			setTeamName(teamName2);
			setOpponentTeamName(teamName1);
		}
	}

	public void setTeamsDirectionNameAndScore(String teamDirection1,String teamDirection2,String teamName1,
			String teamName2,int teamScore1,int teamScore2) {
		if (getTeamNumber() == 1) {
			setTeamdirection(teamDirection1);
			setOpponentTeamDirection(teamDirection2);
			setTeamName(teamName1);
			setOpponentTeamName(teamName2);
			setTeamScore(teamScore1);
			setOpponentScore(teamScore2);
		} else {
			setTeamdirection(teamDirection2);
			setOpponentTeamDirection(teamDirection1);
			setTeamName(teamName2);
			setOpponentTeamName(teamName1);
			setTeamScore(teamScore2);
			setOpponentScore(teamScore1);
		}

	}

}
