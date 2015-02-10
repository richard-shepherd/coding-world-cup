package request.processor;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.Map;

import main.GameData;
import utils.GeometryUtils;
import utils.JsonConstants;

import com.google.gson.JsonObject;

import element.Ball;
import element.Player;
import element.Position;

public abstract class RequestProcessor {

	public static final int NOBODY_OWN_THE_BALL = 0;
	public static final int WE_OWN_THE_BALL = 1;
	public static final int THEY_OWN_THE_BALL = 2;

	public abstract JsonObject processConfigureAbilities(JsonObject request,
			GameData gameData);

	public abstract JsonObject processKickoff(JsonObject request,
			GameData gameData);

	public abstract JsonObject processPlay(JsonObject jsonObject,
			GameData gameData);

	protected abstract JsonObject searchBall(JsonObject jsonObject,
			GameData gameData);

	protected abstract JsonObject attack(JsonObject jsonObject,
			GameData gameData);

	protected abstract JsonObject defend(JsonObject jsonObject,
			GameData gameData);

	protected int getBallOwner(GameData gameData) {
		int controllingplayerNumber = gameData.getControllingPlayerNumber();
		if (controllingplayerNumber != -1) {
			Player owner = gameData.getTeamData().getPlayerMap()
					.get(controllingplayerNumber);
			if (owner != null)
				return WE_OWN_THE_BALL;
			else {
				owner = gameData.getTeamData().getPlayerMap()
						.get(controllingplayerNumber);
				if (owner != null)
					return THEY_OWN_THE_BALL;
			}
		}
		return NOBODY_OWN_THE_BALL;

	}

	protected int closestTeamPlayerOfTheBall(GameData gameData) {

		Map<Integer, Player> playerMap = gameData.getTeamData().getPlayerMap();

		return closestPlayerOfTheBall(playerMap.values(), gameData.getBall());
	}

	protected int closestPlayerOfTheBall(Collection<Player> players, Ball ball) {
		double distMin = Double.MAX_VALUE;
		int playerNumber = -1;
		for (Player player : players) {
			double currDist = GeometryUtils.getDistance(player.getPosition(),
					ball.getPosition());
			if (currDist < distMin) {
				distMin = currDist;
				playerNumber = player.getPlayerNumber();
			}
		}

		return playerNumber;
	}

	protected Player getPlayerFromRole(String role, GameData gameData) {
		Map<Integer, Player> playerMap = gameData.getTeamData().getPlayerMap();

		for (Player player : playerMap.values()) {
			if (role.equals(player.getRole())) {
				return player;
			}
		}

		return null;
	}

	protected Position getTeamGoal(GameData gameData) {
		return getGoalPosition(gameData.getTeamData()
				.getOpponentTeamDirection(), gameData);
	}

	protected Position getGoalPosition(String direction, GameData gameData) {
		if (JsonConstants.LEFT.equals(direction)) {
			return gameData.getGroundData().getGoalLeft().getCenter();
		} else if (JsonConstants.RIGHT.equals(direction)) {
			return gameData.getGroundData().getGoalRight().getCenter();
		}

		return null;
	}

	protected boolean isAttackLeft(GameData gameData) {
		return (JsonConstants.LEFT
				.equals(gameData.getTeamData().getTeamdirection()));
	}

	protected double getDistanceOfClosestOpponent(Player player, GameData gameData) {
		Map<Integer, Player> opponentPlayerMap = gameData.getTeamData().getOpponentPlayerMap();
		double distMin = Double.MAX_VALUE;

		for(Player opponent : opponentPlayerMap.values()) {
			double currDist = GeometryUtils.getDistance(player.getPosition(), opponent.getPosition());
			if (currDist < distMin) {
				distMin = currDist;
			}
		}

		return distMin;
	}

	protected List<Player> getOpponentAttacks (GameData gameData){
		Map<Integer,Player> opponentPlayerMap =gameData.getTeamData().getOpponentPlayerMap();
		Ball ball = gameData.getBall();
		int controllingPlayerNumber =gameData.getControllingPlayerNumber();
		List<Player> opponentAttacks = new ArrayList<Player>();

		for(Player player : opponentPlayerMap.values()){
			if (isAttackLeft(gameData)){
				if (player.getPosition().getX()>gameData.getGroundData().getWidth()/2
						&&player.getPosition().getX()>ball.getPosition().getX()
						&&controllingPlayerNumber!=player.getPlayerNumber()){
					opponentAttacks.add(player);
				}
			}
			else{
				if (player.getPosition().getX()<gameData.getGroundData().getWidth()/2
						&&player.getPosition().getX()<ball.getPosition().getX()
						&&controllingPlayerNumber!=player.getPlayerNumber()){
					opponentAttacks.add(player);
				}


			}
			return opponentAttacks;



		}




		return null;

	}



}
