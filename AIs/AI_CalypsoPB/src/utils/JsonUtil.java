package utils;

import java.util.logging.Logger;

import main.GameData;
import request.processor.DefaultRequestProcessor;

import com.google.gson.JsonObject;

import element.Player;
import element.Position;

public class JsonUtil {
	private final static Logger LOGGER = Logger
			.getLogger(DefaultRequestProcessor.class.getName());

	private final static Integer TOLERANCE = 45;

	public static boolean isEvent(JsonObject jsonObject) {
		return JsonConstants.MESSAGE_TYPE_EVENT.equals(jsonObject.get(
				JsonConstants.MESSAGE_TYPE).getAsString());
	}

	public static boolean isRequest(JsonObject jsonObject) {
		return JsonConstants.MESSAGE_TYPE_REQUEST.equals(jsonObject.get(
				JsonConstants.MESSAGE_TYPE).getAsString());
	}

	public static boolean isConfigureAbilitiesRequest(JsonObject jsonObject) {
		return JsonConstants.REQUEST_TYPE_CONFIGURE_ABILITIES.equals(jsonObject
				.get(JsonConstants.REQUEST_TYPE).getAsString());
	}

	public static boolean isKickoffRequest(JsonObject jsonObject) {
		return JsonConstants.REQUEST_TYPE_KICKOFF.equals(jsonObject.get(
				JsonConstants.REQUEST_TYPE).getAsString());
	}

	public static boolean isPlayRequest(JsonObject jsonObject) {
		return JsonConstants.REQUEST_TYPE_PLAY.equals(jsonObject.get(
				JsonConstants.REQUEST_TYPE).getAsString());
	}

	public static JsonObject createActionMove(int playNumber,
			Position destinationP) {
		JsonObject action = new JsonObject();
		action.addProperty(JsonConstants.PLAYER_NUMBER, playNumber);
		action.addProperty(JsonConstants.ACTION, JsonConstants.ACTION_MOVE);

		JsonObject destination = new JsonObject();
		destination.addProperty(JsonConstants.X, destinationP.getX());
		destination.addProperty(JsonConstants.Y, destinationP.getY());
		action.add(JsonConstants.DESTINATION, destination);
		return action;
	}

	public static JsonObject createActionTurn(Player player,
			Position destinationP) {
		JsonObject action = new JsonObject();
		action.addProperty(JsonConstants.PLAYER_NUMBER,
				player.getPlayerNumber());
		action.addProperty(JsonConstants.ACTION, JsonConstants.ACTION_TURN);
		double angle = GeometryUtils.getAngle(player.getPosition(),
				destinationP);
		action.addProperty(JsonConstants.DIRECTION, angle);
		return action;
	}

	public static JsonObject createActionKick(Player player,
			Position destinationP, double speed) {
		JsonObject action = new JsonObject();
		action.addProperty(JsonConstants.PLAYER_NUMBER,
				player.getPlayerNumber());
		action.addProperty(JsonConstants.ACTION, JsonConstants.ACTION_KICK);

		JsonObject destination = new JsonObject();
		destination.addProperty(JsonConstants.X, destinationP.getX());
		destination.addProperty(JsonConstants.Y, destinationP.getY());
		action.add(JsonConstants.DESTINATION, destination);
		action.addProperty(JsonConstants.SPEED, speed);
		return action;
	}

	public static JsonObject createActionTurnOrKick(Player player,
			Position destinationP, double speed) {
		JsonObject action = new JsonObject();
		double angle = GeometryUtils.getAngle(player.getPosition(),
				destinationP);
		double angleActually = player.getDirection();
		if (Math.abs(angleActually - angle) <= TOLERANCE)
			action = createActionKick(player, destinationP, speed);
		else
			action = createActionTurn(player, destinationP);
		return action;
	}

	public static JsonObject createActionTakePossession(int playNumber) {
		JsonObject action = new JsonObject();
		action.addProperty(JsonConstants.PLAYER_NUMBER, playNumber);
		action.addProperty(JsonConstants.ACTION,
				JsonConstants.ACTION_TAKE_POSSESSION);

		return action;
	}

	public static JsonObject createActionGoTOGetBall(Player player,
			GameData gameData) {
		double distance = GeometryUtils.getDistance(player.getPosition(),
				gameData.getBall().getPosition());
		JsonObject action = null;
		if (distance < 1) {
			action = JsonUtil.createActionTakePossession(player
					.getPlayerNumber());
		} else {
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					gameData.getBall().getPosition());
		}
		return action;
	}

}
