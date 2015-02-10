package main;

import java.util.Map;
import java.util.logging.Logger;

import request.processor.DefaultRequestProcessor;
import request.processor.RequestProcessor;
import utils.JsonConstants;
import utils.JsonUtil;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import element.Ball;
import element.Circle;
import element.Goal;
import element.Player;
import element.Position;

public class AICalypsoPB {

	private final static Logger LOGGER = Logger.getLogger(AICalypsoPB.class
			.getName());

	private final GameData gameData;

	private final JsonParser jsonParser;

	private final RequestProcessor requestProcessor;

	public AICalypsoPB() {
		gameData = new GameData();
		jsonParser = new JsonParser();
		requestProcessor = new DefaultRequestProcessor();
	}

	public void processJsonMessage(String message) {
		JsonObject jsonObject = (JsonObject) jsonParser.parse(message);
		// LOGGER.log(Level.INFO, "processed Json : " + jsonObject.toString());

		if (JsonUtil.isEvent(jsonObject)) {
			processEvent(jsonObject);
		} else if (JsonUtil.isRequest(jsonObject)) {
			processRequest(jsonObject);
		}
	}

	void processEvent(JsonObject jsonObject) {
		String eventType = jsonObject.get(JsonConstants.EVENT_TYPE)
				.getAsString();
		if (JsonConstants.EVENT_GAME_START.equals(eventType)) {
			processGameStart(jsonObject);
		} else if (JsonConstants.EVENT_TEAM_INFO.equals(eventType)) {
			processTeamInfo(jsonObject);
		} else if (JsonConstants.EVENT_KICK_OFF.equals(eventType)) {
			processKickOff(jsonObject);
		} else if (JsonConstants.EVENT_START_OF_TURN.equals(eventType)) {
			processStartOfTurn(jsonObject);
		} else if (JsonConstants.EVENT_GOAL.equals(eventType)) {
			processGoal(jsonObject);
		} else if (JsonConstants.EVENT_HALF_TIME.equals(eventType)) {
			processHalfTime(jsonObject);
		}

	}

	void processRequest(JsonObject request) {
		JsonObject response = null;
		if (JsonUtil.isConfigureAbilitiesRequest(request)) {

			response = requestProcessor.processConfigureAbilities(request,
					gameData);
		} else if (JsonUtil.isKickoffRequest(request)) {
			response = requestProcessor.processKickoff(request, gameData);
		} else if (JsonUtil.isPlayRequest(request)) {
			response = requestProcessor.processPlay(request, gameData);
		}
		System.out.println(response);
	}

	public void processGameStart(JsonObject jsonObject) {
		// LOGGER.log(Level.INFO, "Starting process game start event...");

		JsonObject pitch = (JsonObject) jsonObject.get(JsonConstants.PITCH);
		double width = pitch.get(JsonConstants.WIDTH).getAsDouble();
		double height = pitch.get(JsonConstants.HEIGHT).getAsDouble();
		double goalCenter = pitch.get(JsonConstants.GOAL_CENTRE).getAsDouble();
		double goalY1 = pitch.get(JsonConstants.GOALY1).getAsDouble();
		double goalY2 = pitch.get(JsonConstants.GOALY2).getAsDouble();
		JsonObject centreSpot = pitch
				.getAsJsonObject(JsonConstants.CENTRE_SPOT);
		double centreX = centreSpot.get(JsonConstants.X).getAsDouble();
		double centreY = centreSpot.get(JsonConstants.Y).getAsDouble();
		double centreCircleRadius = pitch.get(
				JsonConstants.CENTRE_CIRCLE_RADIUS).getAsDouble();
		double goalAreaRadius = pitch.get(JsonConstants.GOAL_AREA_RADIUS)
				.getAsDouble();
		Circle centre = new Circle(new Position(centreX, centreY),
				centreCircleRadius);
		Goal goalLeft = new Goal(new Position(0, goalCenter), goalAreaRadius,
				new Position(0, goalY1), new Position(0, goalY2));

		Goal goalRight = new Goal(new Position(width, goalCenter),
				goalAreaRadius, new Position(width, goalY1), new Position(
						width, goalY2));
		double gameLengthSeconds = jsonObject.get(
				JsonConstants.GAME_LENGTH_SECONDS).getAsDouble();

		GroundData groundData = new GroundData();
		groundData.setWidth(width);
		groundData.setHeight(height);
		groundData.setCentre(centre);
		groundData.setGoalLeft(goalLeft);
		groundData.setGoalRight(goalRight);
		groundData.setGameLengthSeconds(gameLengthSeconds);
		gameData.setGroundData(groundData);

		// LOGGER.log(Level.INFO, "Ending process game start event...");
	}

	public void processTeamInfo(JsonObject jsonObject) {
		// LOGGER.log(Level.INFO, "Starting process team info event...");

		int teamNumber = jsonObject.get(JsonConstants.TEAM_NUMBER).getAsInt();
		int opponentTeamNumber = (teamNumber == 1) ? 2 : 1;

		TeamData teamData = gameData.getTeamData();
		Map<Integer, Player> playerMap = teamData.getPlayerMap();
		JsonArray players = jsonObject.getAsJsonArray(JsonConstants.PLAYERS);
		for (JsonElement object : players) {
			JsonObject playerJson = (JsonObject) object;
			int playNumber = playerJson.get(JsonConstants.PLAYER_NUMBER)
					.getAsInt();
			String playerType = playerJson.get(JsonConstants.PLAYER_TYPE)
					.getAsString();
			Player player = new Player();
			player.setType(playerType);
			player.setPlayerNumber(playNumber);
			playerMap.put(playNumber, player);
		}
		teamData.setTeamNumber(teamNumber);
		teamData.setOpponentTeamNumber(opponentTeamNumber);

		// LOGGER.log(Level.INFO, "Ending process team info event...");
	}

	public void processKickOff(JsonObject jsonObject) {
		// LOGGER.log(Level.INFO, "Starting process kickoff event...");

		JsonObject team1 = (JsonObject) jsonObject.get(JsonConstants.TEAM1);
		String teamName1 = team1.get(JsonConstants.NAME).getAsString();
		int teamScore1 = team1.get(JsonConstants.SCORE).getAsInt();
		String teamDirection1 = team1.get(JsonConstants.DIRECTION)
				.getAsString();

		JsonObject team2 = (JsonObject) jsonObject.get(JsonConstants.TEAM2);
		String teamName2 = team2.get(JsonConstants.NAME).getAsString();
		int teamScore2 = team2.get(JsonConstants.SCORE).getAsInt();
		String teamDirection2 = team2.get(JsonConstants.DIRECTION)
				.getAsString();

		int teamKickingOff = jsonObject.get(JsonConstants.TEAM_KICKING_OFF)
				.getAsInt();

		TeamData teamData = gameData.getTeamData();
		teamData.setTeamsDirectionNameAndScore(teamDirection1, teamDirection2,
				teamName1, teamName2, teamScore1, teamScore2);
		teamData.setTeamToKickoff(teamKickingOff);

		// LOGGER.log(Level.INFO, "Ending process kickoff event...");
	}

	public void processStartOfTurn(JsonObject jsonObject) {
		double currentTimeSeconds = ((JsonObject) jsonObject
				.get(JsonConstants.GAME)).get(JsonConstants.CURRENT_TIMESCONDS)
				.getAsDouble();

		JsonObject ball = jsonObject.getAsJsonObject(JsonConstants.BALL);
		double ballX = ball.getAsJsonObject(JsonConstants.POSITION)
				.get(JsonConstants.X).getAsDouble();
		double ballY = ball.getAsJsonObject(JsonConstants.POSITION)
				.get(JsonConstants.Y).getAsDouble();
		double ballVectorX = ball.getAsJsonObject(JsonConstants.VECTOR)
				.get(JsonConstants.X).getAsDouble();
		double ballvectorY = ball.getAsJsonObject(JsonConstants.VECTOR)
				.get(JsonConstants.Y).getAsDouble();
		double speed = ball.get(JsonConstants.SPEED).getAsDouble();
		int controllingPlayerNumber = ball.get(
				JsonConstants.CONTROLLING_PLAYER_NUMBER).getAsInt();
		Ball ballObject = new Ball();
		ballObject.setPosition(new Position(ballX, ballY));
		ballObject.setVectorX(ballVectorX);
		ballObject.setVectorY(ballvectorY);
		gameData.setCurrentTimeSeconds(currentTimeSeconds);
		gameData.setBall(ballObject);
		gameData.setSpeed(speed);
		gameData.setControllingPlayerNumber(controllingPlayerNumber);
		JsonObject team1 = jsonObject.getAsJsonObject(JsonConstants.TEAM1)
				.getAsJsonObject(JsonConstants.TEAM);
		String teamName1 = team1.get(JsonConstants.NAME).getAsString();
		String teamDirection1 = team1.get(JsonConstants.DIRECTION)
				.getAsString();
		JsonArray players1 = jsonObject.getAsJsonObject(JsonConstants.TEAM1)
				.getAsJsonArray(JsonConstants.PLAYERS);

		JsonObject team2 = jsonObject.getAsJsonObject(JsonConstants.TEAM2)
				.getAsJsonObject(JsonConstants.TEAM);
		String teamName2 = team2.get(JsonConstants.NAME).getAsString();
		String teamDirection2 = team2.get(JsonConstants.DIRECTION)
				.getAsString();
		JsonArray players2 = jsonObject.getAsJsonObject(JsonConstants.TEAM2)
				.getAsJsonArray(JsonConstants.PLAYERS);
		TeamData teamData = gameData.getTeamData();
		teamData.setTeamsDirectionAndName(teamDirection1, teamDirection2,
				teamName1, teamName2);
		Map<Integer, Player> map1;
		Map<Integer, Player> map2;
		if (teamData.getTeamNumber() == 1) {
			map1 = teamData.getPlayerMap();
			map2 = teamData.getOpponentPlayerMap();
		} else {
			map1 = teamData.getOpponentPlayerMap();
			map2 = teamData.getPlayerMap();
		}

		for (JsonElement object : players1) {
			JsonObject playerJson = (JsonObject) object;
			JsonObject staticState = playerJson
					.getAsJsonObject(JsonConstants.STATIC_STATE);
			int playNumber = staticState.get(JsonConstants.PLAYER_NUMBER)
					.getAsInt();
			String playerType = staticState.get(JsonConstants.PLAYER_TYPE)
					.getAsString();
			double kickingAbility = staticState.get(
					JsonConstants.KICKING_ABILITY).getAsDouble();
			double runningAbility = staticState.get(
					JsonConstants.RUNNING_ABILITY).getAsDouble();
			double ballControlAbility = staticState.get(
					JsonConstants.BALL_CONTROL_ABILITY).getAsDouble();
			double tacklingAbility = staticState.get(
					JsonConstants.TACKLING_ABILITY).getAsDouble();
			// {"position":{"x":25,"y":25},"hasBall":false,"energy":100,"direction":270}},??
			JsonObject dynamicState = playerJson
					.getAsJsonObject("dynamicState");
			JsonObject position = dynamicState
					.getAsJsonObject(JsonConstants.POSITION);
			double positionX = position.get(JsonConstants.X).getAsDouble();
			double positionY = position.get(JsonConstants.Y).getAsDouble();
			boolean hasBall = dynamicState.get(JsonConstants.HAS_BALL)
					.getAsBoolean();
			double direction = dynamicState.get(JsonConstants.DIRECTION)
					.getAsDouble();
			Player player = map1.get(playNumber);
			if (player == null) {
				player = new Player();
				map1.put(playNumber, player);
			}
			player.setType(playerType);
			player.setPlayerNumber(playNumber);
			player.setKickingAbility(kickingAbility);
			player.setRunningAbility(runningAbility);
			player.setBallControlAbility(ballControlAbility);
			player.setBallControlAbility(ballControlAbility);
			player.setTacklingAbility(tacklingAbility);
			player.setPosition(new Position(positionX, positionY));
			player.setHasBall(hasBall);
			player.setDirection(direction);
		}
		for (JsonElement object : players2) {
			JsonObject playerJson = (JsonObject) object;
			JsonObject staticState = playerJson
					.getAsJsonObject(JsonConstants.STATIC_STATE);
			int playNumber = staticState.get(JsonConstants.PLAYER_NUMBER)
					.getAsInt();
			String playerType = staticState.get(JsonConstants.PLAYER_TYPE)
					.getAsString();
			double kickingAbility = staticState.get(
					JsonConstants.KICKING_ABILITY).getAsDouble();
			double runningAbility = staticState.get(
					JsonConstants.RUNNING_ABILITY).getAsDouble();
			double ballControlAbility = staticState.get(
					JsonConstants.BALL_CONTROL_ABILITY).getAsDouble();
			double tacklingAbility = staticState.get(
					JsonConstants.TACKLING_ABILITY).getAsDouble();
			JsonObject dynamicState = playerJson
					.getAsJsonObject("dynamicState");
			JsonObject position = dynamicState
					.getAsJsonObject(JsonConstants.POSITION);
			double positionX = position.get(JsonConstants.X).getAsDouble();
			double positionY = position.get(JsonConstants.Y).getAsDouble();
			boolean hasBall = dynamicState.get(JsonConstants.HAS_BALL)
					.getAsBoolean();
			double direction = dynamicState.get(JsonConstants.DIRECTION)
					.getAsDouble();
			Player player = map2.get(playNumber);
			if (player == null) {
				player = new Player();
				map2.put(playNumber, player);
			}
			player.setType(playerType);
			player.setPlayerNumber(playNumber);
			player.setKickingAbility(kickingAbility);
			player.setRunningAbility(runningAbility);
			player.setBallControlAbility(ballControlAbility);
			player.setBallControlAbility(ballControlAbility);
			player.setTacklingAbility(tacklingAbility);
			player.setPosition(new Position(positionX, positionY));
			player.setHasBall(hasBall);
			player.setDirection(direction);
		}

	}

	public void processGoal(JsonObject jsonObject) {
		JsonObject team1 = (JsonObject) jsonObject.get(JsonConstants.TEAM1);
		String teamName1 = team1.get(JsonConstants.NAME).getAsString();
		int teamScore1 = team1.get(JsonConstants.SCORE).getAsInt();
		String teamDirection1 = team1.get(JsonConstants.DIRECTION)
				.getAsString();

		JsonObject team2 = (JsonObject) jsonObject.get(JsonConstants.TEAM2);
		String teamName2 = team2.get(JsonConstants.NAME).getAsString();
		int teamScore2 = team2.get(JsonConstants.SCORE).getAsInt();
		String teamDirection2 = team2.get(JsonConstants.DIRECTION)
				.getAsString();
		TeamData teamData = gameData.getTeamData();
		teamData.setTeamsDirectionNameAndScore(teamDirection1, teamDirection2,
				teamName1, teamName2, teamScore1, teamScore2);
	}

	public void processHalfTime(JsonObject jsonObject) {
		boolean halfTime = true;
		gameData.setHalfTime(halfTime);
	}
}
