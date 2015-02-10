package request.processor;

import java.util.Map;
import java.util.Set;
import java.util.logging.Level;
import java.util.logging.Logger;

import main.GameData;
import utils.GeometryUtils;
import utils.JsonConstants;
import utils.JsonUtil;

import com.google.gson.JsonArray;
import com.google.gson.JsonObject;

import element.Ball;
import element.Circle;
import element.Goal;
import element.Player;
import element.Position;

public class DefaultRequestProcessor extends RequestProcessor {

	private final static Logger LOGGER = Logger
			.getLogger(DefaultRequestProcessor.class.getName());

	private static final Circle CENTRE_DEFENDER_CIRCLE = new Circle(
			new Position(50, 25), 27);

	private static final Circle ATTACK_CIRCLE = new Circle(
			new Position(50, 25), 17);

	private static final Circle centreDefenderPrepareCircle = new Circle(
			new Position(50, 25), 17);
	private final static double DEFENDER_RADIUS = 10;
	private final Circle DOWN_LEFT_CIRCLE = new Circle(new Position(30, 36),
			DEFENDER_RADIUS);
	private final Circle TOP_RIGHT_CIRCLE = new Circle(new Position(70, 14),
			DEFENDER_RADIUS);
	private final Circle TOP_LEFT_CIRCLE = new Circle(new Position(30, 14),
			DEFENDER_RADIUS);
	private final Circle DOWN_RIGHT_CIRCLE = new Circle(new Position(70, 36),
			DEFENDER_RADIUS);

	private static double SHOOTING_SPEED = 100;
	private static double PASSING_SPEED = 50;
	private static double FORWARD_SIDE_PARAMETER = 0.4;
	private static double FORWARD_MIN_DISTANCE_WITH_OPPONENT = 8;

	@Override
	public JsonObject processConfigureAbilities(JsonObject request,
			GameData gameData) {
		// LOGGER.log(Level.INFO, "processConfigureAbilities called with : " +
		// request);

		int totalKickingAbility = request.get(
				JsonConstants.TOTAL_KICKING_ABILITY).getAsInt();
		int totalRunningAbility = request.get(
				JsonConstants.TOTAL_RUNNING_ABILITY).getAsInt();
		int totalBallControlAbility = request.get(
				JsonConstants.TOTAL_BALL_CONTROL_ABILITY).getAsInt();
		int totalTacklingAbility = request.get(
				JsonConstants.TOTAL_TACKLING_ABILITY).getAsInt();

		String[] roles = { Player.LEFT_DEFENDER, Player.CENTER_DEFENDER,
				Player.RIGHT_DEFENDER, Player.LEFT_FORWARD,
				Player.RIGHT_FORWARD };
		int indexRole = 0;

		JsonArray jsonPlayersArray = new JsonArray();
		Map<Integer, Player> playerMap = gameData.getTeamData().getPlayerMap();
		// for (Integer num : playerMap.keySet()) {
		// JsonObject jsonPlayer = new JsonObject();
		// jsonPlayer.addProperty(JsonConstants.PLAYER_NUMBER, num);
		// // jsonPlayer.addProperty(JsonConstants.KICKING_ABILITY, 66);
		// jsonPlayer.addProperty(JsonConstants.RUNNING_ABILITY, 66);
		// jsonPlayer.addProperty(JsonConstants.BALL_CONTROL_ABILITY, 66);
		// jsonPlayer.addProperty(JsonConstants.TACKLING_ABILITY, 66);
		// jsonPlayersArray.add(jsonPlayer);
		// Player player = playerMap.get(num);
		// if (JsonConstants.PLAYER_TYPE_G.equals(player.getType())) {
		// player.setRole(Player.GOALKEEPER);
		// jsonPlayer.addProperty(JsonConstants.KICKING_ABILITY, 0);
		// } else {
		// if (indexRole == 3 || indexRole == 4) {
		// jsonPlayer.addProperty(JsonConstants.KICKING_ABILITY, 100);
		// } else {
		// jsonPlayer.addProperty(JsonConstants.KICKING_ABILITY, 66);
		// }
		// player.setRole(roles[indexRole++]);
		// }
		// }

		int goalKeeperNumber = 0;
		Integer[] numberArrayWithoutGk = new Integer[5];
		int index = 0;
		for (Integer i : playerMap.keySet()) {
			if (JsonConstants.PLAYER_TYPE_G.equals(playerMap.get(i).getType())) {
				goalKeeperNumber = i;
			} else {
				numberArrayWithoutGk[index++] = i;
			}
		}

		Set<Integer> numberSetWithoutGk = playerMap.keySet();
		// numberSetWithoutGk.remove(goalKeeperNumber);
		// Integer[] numberArrayWithoutGk = numberSetWithoutGk.toArray(new
		// Integer[numberSetWithoutGk.size()]);
		int fieldPlayerIndex = 0;

		JsonObject jsonGoalKeeper = new JsonObject();
		jsonGoalKeeper.addProperty(JsonConstants.PLAYER_NUMBER,
				goalKeeperNumber);
		jsonGoalKeeper.addProperty(JsonConstants.KICKING_ABILITY, 30);
		jsonGoalKeeper.addProperty(JsonConstants.RUNNING_ABILITY, 60);
		jsonGoalKeeper.addProperty(JsonConstants.BALL_CONTROL_ABILITY, 0);
		jsonGoalKeeper.addProperty(JsonConstants.TACKLING_ABILITY, 0);
		jsonPlayersArray.add(jsonGoalKeeper);
		playerMap.get(goalKeeperNumber).setRole(Player.GOALKEEPER);

		JsonObject jsonLeftDefender = new JsonObject();
		jsonLeftDefender.addProperty(JsonConstants.PLAYER_NUMBER,
				numberArrayWithoutGk[fieldPlayerIndex]);
		jsonLeftDefender.addProperty(JsonConstants.KICKING_ABILITY, 56.6);
		jsonLeftDefender.addProperty(JsonConstants.RUNNING_ABILITY, 66.6);
		jsonLeftDefender.addProperty(JsonConstants.BALL_CONTROL_ABILITY, 80);
		jsonLeftDefender.addProperty(JsonConstants.TACKLING_ABILITY, 80);
		jsonPlayersArray.add(jsonLeftDefender);
		playerMap.get(numberArrayWithoutGk[fieldPlayerIndex++]).setRole(
				Player.LEFT_DEFENDER);

		JsonObject jsonCenterDefender = new JsonObject();
		jsonCenterDefender.addProperty(JsonConstants.PLAYER_NUMBER,
				numberArrayWithoutGk[fieldPlayerIndex]);
		jsonCenterDefender.addProperty(JsonConstants.KICKING_ABILITY, 56.6);
		jsonCenterDefender.addProperty(JsonConstants.RUNNING_ABILITY, 66.6);
		jsonCenterDefender.addProperty(JsonConstants.BALL_CONTROL_ABILITY, 80);
		jsonCenterDefender.addProperty(JsonConstants.TACKLING_ABILITY, 90);
		jsonPlayersArray.add(jsonCenterDefender);
		playerMap.get(numberArrayWithoutGk[fieldPlayerIndex++]).setRole(
				Player.CENTER_DEFENDER);

		JsonObject jsonRightDefender = new JsonObject();
		jsonRightDefender.addProperty(JsonConstants.PLAYER_NUMBER,
				numberArrayWithoutGk[fieldPlayerIndex]);
		jsonRightDefender.addProperty(JsonConstants.KICKING_ABILITY, 76.6);
		jsonRightDefender.addProperty(JsonConstants.RUNNING_ABILITY, 66.6);
		jsonRightDefender.addProperty(JsonConstants.BALL_CONTROL_ABILITY, 80);
		jsonRightDefender.addProperty(JsonConstants.TACKLING_ABILITY, 80);
		jsonPlayersArray.add(jsonRightDefender);
		playerMap.get(numberArrayWithoutGk[fieldPlayerIndex++]).setRole(
				Player.RIGHT_DEFENDER);

		JsonObject jsonLeftForward = new JsonObject();
		jsonLeftForward.addProperty(JsonConstants.PLAYER_NUMBER,
				numberArrayWithoutGk[fieldPlayerIndex]);
		jsonLeftForward.addProperty(JsonConstants.KICKING_ABILITY, 100);
		jsonLeftForward.addProperty(JsonConstants.RUNNING_ABILITY, 83.2);
		jsonLeftForward.addProperty(JsonConstants.BALL_CONTROL_ABILITY, 80);
		jsonLeftForward.addProperty(JsonConstants.TACKLING_ABILITY, 90);
		jsonPlayersArray.add(jsonLeftForward);
		playerMap.get(numberArrayWithoutGk[fieldPlayerIndex++]).setRole(
				Player.LEFT_FORWARD);

		JsonObject jsonRightForward = new JsonObject();
		jsonRightForward.addProperty(JsonConstants.PLAYER_NUMBER,
				numberArrayWithoutGk[fieldPlayerIndex]);
		jsonRightForward.addProperty(JsonConstants.KICKING_ABILITY, 80);
		jsonRightForward.addProperty(JsonConstants.RUNNING_ABILITY, 56.6);
		jsonRightForward.addProperty(JsonConstants.BALL_CONTROL_ABILITY, 80);
		jsonRightForward.addProperty(JsonConstants.TACKLING_ABILITY, 60);
		jsonPlayersArray.add(jsonRightForward);
		playerMap.get(numberArrayWithoutGk[fieldPlayerIndex++]).setRole(
				Player.RIGHT_FORWARD);

		JsonObject response = new JsonObject();
		response.addProperty(JsonConstants.REQUEST_TYPE,
				JsonConstants.REQUEST_TYPE_CONFIGURE_ABILITIES);
		response.add(JsonConstants.PLAYERS, jsonPlayersArray);

		// LOGGER.log(Level.INFO, "processConfigureAbilities responded with : "
		// + response);
		return response;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * request.processor.RequestProcessor#processKickoff(com.google.gson.JsonObject
	 * , main.GameData)
	 */
	@Override
	public JsonObject processKickoff(JsonObject request, GameData gameData) {

		double width = gameData.getGroundData().getWidth();
		double height = gameData.getGroundData().getHeight();

		int teamNumber = gameData.getTeamData().getTeamNumber();
		int teamToKickoff = gameData.getTeamData().getTeamToKickoff();
		Map<Integer, Player> playerMap = gameData.getTeamData().getPlayerMap();

		Position[] positions = new Position[5];

		JsonArray jsonPlayers = new JsonArray();
		if (teamNumber == teamToKickoff) {
			positions[0] = new Position(width / 5, height / 3);
			positions[1] = new Position(width / 5, 2 * height / 3);
			positions[2] = new Position(width / 4, height / 2);
			positions[3] = new Position(width / 2, height / 2 - 5);
			positions[4] = new Position(width / 2, height / 2);
		} else {
			positions[0] = new Position(width / 5, height / 3);
			positions[1] = new Position(width / 5, 2 * height / 3);
			positions[2] = new Position(width / 4, height / 2);
			positions[3] = new Position(width / 3, height / 3);
			positions[4] = new Position(width / 3, 2 * height / 3);
		}
		int index = 0;
		for (Integer num : playerMap.keySet()) {
			if (JsonConstants.PLAYER_TYPE_G
					.equals(playerMap.get(num).getType())) {
				continue;
			}
			JsonObject jsonCurrentPlayer = new JsonObject();
			jsonCurrentPlayer.addProperty(JsonConstants.PLAYER_NUMBER, num);
			JsonObject jsonPosition = new JsonObject();
			jsonPosition.addProperty(JsonConstants.X, positions[index].getX());
			jsonPosition.addProperty(JsonConstants.Y, positions[index].getY());
			jsonCurrentPlayer.add(JsonConstants.POSITION, jsonPosition);
			jsonCurrentPlayer.addProperty(JsonConstants.DIRECTION, 0);
			jsonPlayers.add(jsonCurrentPlayer);
			index++;
		}

		JsonObject response = new JsonObject();
		response.addProperty(JsonConstants.REQUEST_TYPE,
				JsonConstants.REQUEST_TYPE_KICKOFF);
		response.add(JsonConstants.PLAYERS, jsonPlayers);
		return response;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * request.processor.RequestProcessor#processPlay(com.google.gson.JsonObject
	 * )
	 */
	@Override
	public JsonObject processPlay(JsonObject request, GameData gameData) {
		JsonObject response = new JsonObject();

		if (getBallOwner(gameData) == WE_OWN_THE_BALL) {
			response = attack(request, gameData);
		} else {
			// if(getBallOwner(gameData) == THEY_OWN_THE_BALL) {
			response = defend(request, gameData);
		}

		return response;
	}

	@Override
	protected JsonObject searchBall(JsonObject jsonObject, GameData gameData) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	protected JsonObject attack(JsonObject jsonObject, GameData gameData) {
		JsonObject response = new JsonObject();

		JsonArray actionArray = new JsonArray();
		// if (controllingPlayer.getPosition().getX() != 85
		// || controllingPlayer.getPosition().getY() != 25) {
		// JsonObject action = JsonUtil.createActionMove(
		// controllingPlayerNumber, new Position(85, 25));
		// actionArray.add(action);
		// } else {
		// JsonObject action = JsonUtil.createActionKick(
		// controllingPlayerNumber, new Position(100, 25), 100);
		// actionArray.add(action);
		// }

		actionArray.add(createGoalKeeperAttackAction(gameData));
		actionArray.add(createLeftDefenderAttackAction(gameData));
		actionArray.add(createRightDefenderAttackAction(gameData));
		actionArray.add(createCenterDefenderAttackAction(gameData));
		actionArray
				.add(createForwardAttackAction(Player.LEFT_FORWARD, gameData));
		actionArray.add(createForwardAttackAction(Player.RIGHT_FORWARD,
				gameData));

		response.addProperty(JsonConstants.REQUEST_TYPE,
				JsonConstants.REQUEST_TYPE_PLAY);
		response.add(JsonConstants.ACTIONS, actionArray);
		return response;
	}

	@Override
	protected JsonObject defend(JsonObject jsonObject, GameData gameData) {
		JsonObject response = new JsonObject();
		JsonArray actionArray = new JsonArray();

		actionArray.add(createRightDefenderDefenceAction(gameData));
		actionArray.add(createLeftDefenderDefenceAction(gameData));
		actionArray.add(createCenterDefenderDefenceAction(gameData));
		actionArray.add(createLeftForwardDefenceAction(gameData));
		actionArray.add(createRightForwardDefenceAction(gameData));
		actionArray.add(createGoalKeeperDefenceAction(gameData));
		response.addProperty(JsonConstants.REQUEST_TYPE,
				JsonConstants.REQUEST_TYPE_PLAY);
		response.add(JsonConstants.ACTIONS, actionArray);
		return response;

	}

	private JsonObject createRightDefenderDefenceAction(GameData gameData) {
		JsonObject action = new JsonObject();
		Ball ball = gameData.getBall();
		Player player = getPlayerFromRole(Player.RIGHT_DEFENDER, gameData);
		Position topLeft = null;
		Position downRight = null;
		Position statePosition = null;
		if (isAttackLeft(gameData)) {
			topLeft = new Position(gameData.getGroundData().getWidth() / 2, 0);
			downRight = new Position(gameData.getGroundData().getWidth(),
					gameData.getGroundData().getHeight() / 2);
			statePosition = GeometryUtils.getPositionInCircle(
					ball.getPosition(), TOP_RIGHT_CIRCLE.getMovedCircled(5, 0));
		} else {
			topLeft = new Position(0, gameData.getGroundData().getHeight() / 2);
			downRight = new Position(gameData.getGroundData().getWidth() / 2,
					gameData.getGroundData().getHeight());
			statePosition = GeometryUtils
					.getPositionInCircle(ball.getPosition(),
							DOWN_LEFT_CIRCLE.getMovedCircled(-5, 0));
		}
		if (GeometryUtils.isInRectangle(ball.getPosition(), topLeft, downRight))
			action = JsonUtil.createActionGoTOGetBall(player, gameData);
		else
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					statePosition);
		return action;
	}

	private JsonObject createLeftDefenderDefenceAction(GameData gameData) {
		JsonObject action = new JsonObject();
		Player player = getPlayerFromRole(Player.LEFT_DEFENDER, gameData);
		Ball ball = gameData.getBall();
		Position topLeft = null;
		Position downRight = null;
		Position statePosition = null;
		if (isAttackLeft(gameData)) {
			topLeft = new Position(gameData.getGroundData().getWidth() / 2,
					gameData.getGroundData().getHeight() / 2);
			downRight = new Position(gameData.getGroundData().getWidth(),
					gameData.getGroundData().getHeight());
			statePosition = GeometryUtils
					.getPositionInCircle(ball.getPosition(),
							DOWN_RIGHT_CIRCLE.getMovedCircled(5, 0));
		} else {
			topLeft = new Position(0, 0);
			downRight = new Position(gameData.getGroundData().getWidth() / 2,
					gameData.getGroundData().getHeight() / 2);
			statePosition = GeometryUtils.getPositionInCircle(
					ball.getPosition(), TOP_LEFT_CIRCLE.getMovedCircled(-5, 0));
		}
		if (GeometryUtils.isInRectangle(ball.getPosition(), topLeft, downRight))
			action = JsonUtil.createActionGoTOGetBall(player, gameData);
		else
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					statePosition);
		return action;
	}

	private JsonObject createCenterDefenderDefenceAction(GameData gameData) {
		JsonObject action = new JsonObject();
		Player player = getPlayerFromRole(Player.CENTER_DEFENDER, gameData);
		Circle defenderCircle = null;
		Circle defenderPrepareCircle = null;
		if (isAttackLeft(gameData)) {
			defenderCircle = CENTRE_DEFENDER_CIRCLE.getMovedCircled(5, -5);
			defenderPrepareCircle = centreDefenderPrepareCircle
					.getMovedCircled(5, -5);
		} else {
			defenderCircle = CENTRE_DEFENDER_CIRCLE.getMovedCircled(-5, 5);
			defenderPrepareCircle = centreDefenderPrepareCircle
					.getMovedCircled(-5, 5);
		}

		if (defenderCircle.isInArea(gameData.getBall().getPosition()))
			action = JsonUtil.createActionGoTOGetBall(player, gameData);
		else {
			Position position = GeometryUtils.getPositionInCircle(gameData
					.getBall().getPosition(), defenderPrepareCircle);
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					position);
		}
		return action;
	}

	private JsonObject createLeftForwardDefenceAction(GameData gameData) {
		JsonObject action = new JsonObject();
		Player player = getPlayerFromRole(Player.LEFT_FORWARD, gameData);
		Ball ball = gameData.getBall();
		Position topLeft = null;
		Position downRight = null;
		Position statePosition = null;

		Circle defenderCircle = null;
		Circle defenderPrepareCircle = null;
		if (isAttackLeft(gameData)) {
			defenderCircle = CENTRE_DEFENDER_CIRCLE.getMovedCircled(-7, 5);
			defenderPrepareCircle = ATTACK_CIRCLE.getMovedCircled(-7, 5);
			topLeft = new Position(0, 0);
			downRight = new Position(gameData.getGroundData().getWidth() / 2,
					gameData.getGroundData().getHeight());
			statePosition = GeometryUtils.getPositionInCircle(
					ball.getPosition(), defenderPrepareCircle);
		} else {
			defenderCircle = CENTRE_DEFENDER_CIRCLE.getMovedCircled(7, -5);
			defenderPrepareCircle = ATTACK_CIRCLE.getMovedCircled(7, -5);
			topLeft = new Position(gameData.getGroundData().getWidth() / 2, 0);
			downRight = new Position(gameData.getGroundData().getWidth(),
					gameData.getGroundData().getHeight());
			statePosition = GeometryUtils.getPositionInCircle(
					ball.getPosition(), defenderPrepareCircle);
		}

		if (defenderCircle.isInArea(gameData.getBall().getPosition())
				|| GeometryUtils.isInRectangle(ball.getPosition(), topLeft,
						downRight))
			action = JsonUtil.createActionGoTOGetBall(player, gameData);
		else {
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					statePosition);
		}
		return action;

	}

	private JsonObject createRightForwardDefenceAction(GameData gameData) {
		JsonObject action = new JsonObject();
		Ball ball = gameData.getBall();
		Player player = getPlayerFromRole(Player.RIGHT_FORWARD, gameData);
		Position topLeft = null;
		Position downRight = null;
		Position statePosition = null;
		if (isAttackLeft(gameData)) {
			topLeft = new Position(0, 0);
			downRight = new Position(gameData.getGroundData().getWidth() / 2,
					gameData.getGroundData().getHeight() / 2);
			statePosition = GeometryUtils.getPositionInCircle(
					ball.getPosition(), TOP_LEFT_CIRCLE);
		} else {
			topLeft = new Position(gameData.getGroundData().getWidth() / 2,
					gameData.getGroundData().getHeight() / 2);
			downRight = new Position(gameData.getGroundData().getWidth(),
					gameData.getGroundData().getHeight());
			statePosition = GeometryUtils.getPositionInCircle(
					ball.getPosition(), DOWN_RIGHT_CIRCLE);
		}
		if (GeometryUtils.isInRectangle(ball.getPosition(), topLeft, downRight))
			action = JsonUtil.createActionGoTOGetBall(player, gameData);
		else
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					statePosition);
		return action;
	}

	private JsonObject createGoalKeeperDefenceAction(GameData gameData) {
		JsonObject action = new JsonObject();

		Player player = getPlayerFromRole(Player.GOALKEEPER, gameData);
		Ball ball = gameData.getBall();
		Position statePosition = null;
		boolean inGoalArea = false;
		double angleDif;
		double distance;
		if (isAttackLeft(gameData)) {
			Goal goal = gameData.getGroundData().getGoalRight();
			Position goalPosition = goal.getCenter();
			statePosition = GeometryUtils.getPositionInCircle(
					ball.getPosition(), new Circle(goalPosition, 10));
			double angleBall = GeometryUtils.getAngle(goalPosition,
					statePosition);
			double angleGoalKeeper = GeometryUtils.getAngle(goalPosition,
					player.getPosition());
			angleDif = angleBall - angleGoalKeeper;
			inGoalArea = goal.isInArea(ball.getPosition());
			distance = GeometryUtils.getDistance(player.getPosition(),
					goalPosition);
		} else {
			Goal goal = gameData.getGroundData().getGoalLeft();
			Position goalPosition = goal.getCenter();
			statePosition = GeometryUtils.getPositionInCircle(
					ball.getPosition(), new Circle(goalPosition, 10));
			double angleBall = GeometryUtils.getAngle(goalPosition,
					statePosition);
			double angleGoalKeeper = GeometryUtils.getAngle(goalPosition,
					player.getPosition());
			angleDif = angleBall - angleGoalKeeper;
			inGoalArea = goal.isInArea(ball.getPosition());
			distance = GeometryUtils.getDistance(player.getPosition(),
					goalPosition);
		}
		if (gameData.getSpeed() < 5 && inGoalArea) {
			return action = JsonUtil.createActionGoTOGetBall(player, gameData);
		}
		if (Math.abs(angleDif) < 3 && distance > 9) {
			action = JsonUtil.createActionTakePossession(player
					.getPlayerNumber());
		} else {
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					statePosition);
		}

		return action;
	}

	private JsonObject createGoalKeeperAttackAction(GameData gameData) {
		JsonObject action = new JsonObject();

		Player player = getPlayerFromRole(Player.GOALKEEPER, gameData);
		Player centerDefender = getPlayerFromRole(Player.CENTER_DEFENDER,
				gameData);

		action = JsonUtil.createActionTurnOrKick(player,
				centerDefender.getPosition(), SHOOTING_SPEED);

		return action;
	}

	private JsonObject createLeftDefenderAttackAction(GameData gameData) {
		JsonObject action = new JsonObject();
		Player player = getPlayerFromRole(Player.LEFT_DEFENDER, gameData);
		int ControllingPlayerNumber = gameData.getControllingPlayerNumber();
		if (ControllingPlayerNumber == player.getPlayerNumber()) {
			Player playerC = getPlayerFromRole(Player.LEFT_FORWARD, gameData);
			action = JsonUtil.createActionKick(player, playerC.getPosition(),
					90);
		} else {
			Position statePosition = null;
			if (isAttackLeft(gameData)) {
				statePosition = GeometryUtils.getPositionInCircle(gameData
						.getBall().getPosition(), DOWN_RIGHT_CIRCLE);
			} else {
				statePosition = GeometryUtils.getPositionInCircle(gameData
						.getBall().getPosition(), TOP_LEFT_CIRCLE);
			}
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					statePosition);
		}
		return action;
	}

	private JsonObject createCenterDefenderAttackAction(GameData gameData) {
		JsonObject action = new JsonObject();
		Player player = getPlayerFromRole(Player.CENTER_DEFENDER, gameData);
		int ControllingPlayerNumber = gameData.getControllingPlayerNumber();
		if (ControllingPlayerNumber == player.getPlayerNumber()) {
			Position goal = null;
			if (isAttackLeft(gameData)) {
				goal = gameData.getGroundData().getGoalLeft().getCenter();

			} else {
				goal = gameData.getGroundData().getGoalRight().getCenter();

			}
			action = JsonUtil.createActionKick(player, goal, 100);
		} else {
			Position position = GeometryUtils.getPositionInCircle(gameData
					.getBall().getPosition(), centreDefenderPrepareCircle);
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					position);
		}
		return action;
	}

	private JsonObject createRightDefenderAttackAction(GameData gameData) {
		JsonObject action = new JsonObject();
		Player player = getPlayerFromRole(Player.RIGHT_DEFENDER, gameData);
		int ControllingPlayerNumber = gameData.getControllingPlayerNumber();
		if (ControllingPlayerNumber == player.getPlayerNumber()) {
			Player playerC = getPlayerFromRole(Player.LEFT_FORWARD, gameData);
			action = JsonUtil.createActionKick(player, playerC.getPosition(),
					90);
		} else {
			Position statePosition = null;
			if (isAttackLeft(gameData)) {
				statePosition = GeometryUtils.getPositionInCircle(gameData
						.getBall().getPosition(), TOP_RIGHT_CIRCLE);
			} else {
				statePosition = GeometryUtils.getPositionInCircle(gameData
						.getBall().getPosition(), DOWN_LEFT_CIRCLE);
			}
			action = JsonUtil.createActionMove(player.getPlayerNumber(),
					statePosition);
		}
		return action;
	}

	private JsonObject createForwardAttackAction(String role, GameData gameData) {
		JsonObject action = null;

		Player player = getPlayerFromRole(role, gameData);
		int controllingPlayerNumber = gameData.getControllingPlayerNumber();
		double groundWidth = gameData.getGroundData().getWidth();
		double groundHeight = gameData.getGroundData().getHeight();

		Position opponentGoalPosition = null;
		Position targetPosition = null;

		Player otherForward = Player.RIGHT_FORWARD.equals(role) ? getPlayerFromRole(
				Player.LEFT_FORWARD, gameData) : getPlayerFromRole(
				Player.RIGHT_FORWARD, gameData);
		double attackLeftY = (Player.RIGHT_FORWARD.equals(role)) ? FORWARD_SIDE_PARAMETER
				: (1 - FORWARD_SIDE_PARAMETER);
		double attackRightY = (Player.RIGHT_FORWARD.equals(role)) ? (1 - FORWARD_SIDE_PARAMETER)
				: FORWARD_SIDE_PARAMETER;

		Position shootingRectangleTopLeft = null;
		Position shootingRectangleDownRight = null;
		double goalRadius = gameData.getGroundData().getGoalLeft().getRadius();

		if (isAttackLeft(gameData)) {
			opponentGoalPosition = getGoalPosition(JsonConstants.LEFT, gameData);
			shootingRectangleTopLeft = new Position(0.8 * goalRadius,
					0.3 * groundHeight);
			shootingRectangleDownRight = new Position(1.2 * goalRadius,
					0.7 * groundHeight);
			targetPosition = new Position(
					(shootingRectangleTopLeft.getX() + shootingRectangleDownRight
							.getX()) / 2, attackLeftY * groundHeight);

		} else {
			opponentGoalPosition = getGoalPosition(JsonConstants.RIGHT,
					gameData);
			shootingRectangleTopLeft = new Position(groundWidth - 1.2
					* goalRadius, 0.3 * groundHeight);
			shootingRectangleDownRight = new Position(groundWidth - 0.8
					* goalRadius, 0.7 * groundHeight);
			targetPosition = new Position(
					(shootingRectangleTopLeft.getX() + shootingRectangleDownRight
							.getX()) / 2, attackRightY * groundHeight);
		}

		Position shootTarget = adjustShootTarget(player.getPosition(),
				opponentGoalPosition, gameData);

		LOGGER.log(Level.INFO, "topLeft x = " + shootingRectangleTopLeft.getX()
				+ "y = " + shootingRectangleTopLeft.getY());
		LOGGER.log(Level.INFO,
				"downright x = " + shootingRectangleDownRight.getX() + "y = "
						+ shootingRectangleDownRight.getY());

		if (GeometryUtils.isInRectangle(player.getPosition(),
				shootingRectangleTopLeft, shootingRectangleDownRight)) {

			if (controllingPlayerNumber == player.getPlayerNumber()) {

				action = JsonUtil.createActionKick(player, shootTarget,
						SHOOTING_SPEED);
			} else {
				action = JsonUtil
						.createActionTurn(player, opponentGoalPosition);
			}
		} else {
			if (controllingPlayerNumber == player.getPlayerNumber()) {
				if (getDistanceOfClosestOpponent(player, gameData) < FORWARD_MIN_DISTANCE_WITH_OPPONENT) {
					if (getDistanceOfClosestOpponent(otherForward, gameData) < FORWARD_MIN_DISTANCE_WITH_OPPONENT) {
						action = JsonUtil.createActionKick(player, shootTarget,
								SHOOTING_SPEED);
					} else {
						action = JsonUtil.createActionKick(player,
								otherForward.getPosition(), PASSING_SPEED);
					}

				} else {
					action = JsonUtil.createActionMove(
							player.getPlayerNumber(), targetPosition);
				}
			} else {
				action = JsonUtil.createActionMove(player.getPlayerNumber(),
						targetPosition);
			}
		}

		return action;
	}

	private Position adjustShootTarget(Position playerPosition,
			Position opponentGoalPosition, GameData gameData) {
		double goalLength = Math.abs(gameData.getGroundData().getGoalLeft()
				.getDown().getY()
				- gameData.getGroundData().getGoalLeft().getTop().getY());

		Position kickTarget;
		// if we are close we target the side of the goal
		if (GeometryUtils.getDistance(playerPosition, opponentGoalPosition) < gameData
				.getGroundData().getWidth() / 4) {
			kickTarget = new Position(opponentGoalPosition.getX(),
					opponentGoalPosition.getY() + goalLength / 3);
		} else { // otherwise we target closer to the center of the goal
			kickTarget = new Position(opponentGoalPosition.getX(),
					opponentGoalPosition.getY());
		}
		return kickTarget;

	}

}
