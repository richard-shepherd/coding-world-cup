package tests;

import java.util.HashMap;
import java.util.Map;

import main.AICalypsoPB;
import main.GameData;
import main.GroundData;
import main.TeamData;

import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;

import request.processor.DefaultRequestProcessor;
import request.processor.RequestProcessor;
import utils.JsonConstants;

import com.google.gson.JsonArray;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import element.Player;

public class DefaultRequestProcessorTest {

	static final String CONFIGURE_ABILITIES_REQUEST = "{\"requestType\":\"CONFIGURE_ABILITIES\","
			+ "\"totalKickingAbility\":400,\"totalRunningAbility\":400,\"totalBallControlAbility\":400,"
			+ "\"totalTacklingAbility\":400,\"messageType\":\"REQUEST\"}";

	static final String KICKOFF_REQUEST = "{\"requestType\":\"KICKOFF\",\"messageType\":\"REQUEST\"}";

	JsonParser parser = new JsonParser();
	AICalypsoPB aiCalypsoPB = new AICalypsoPB();
	GameData gameData = new GameData();
	RequestProcessor requestProcessor = new DefaultRequestProcessor();

	@Before
	public void setUp() {
		Map<Integer, Player> playerMap = new HashMap<Integer, Player>();
		Player goalKeeper = new Player();
		goalKeeper.setType(JsonConstants.PLAYER_TYPE_G);
		playerMap.put(0, goalKeeper);
		playerMap.put(1, new Player());
		playerMap.put(2, new Player());
		playerMap.put(3, new Player());
		playerMap.put(4, new Player());
		playerMap.put(5, new Player());

		TeamData teamData = new TeamData(1, playerMap, 2, null);
		gameData.setTeamData(teamData);
		GroundData groundData = new GroundData();
		groundData.setHeight(50);
		groundData.setWidth(100);
		gameData.setGroundData(groundData);
	}

	@Test
	public void processConfigureAbilities() {
		JsonObject request = (JsonObject) parser
				.parse(CONFIGURE_ABILITIES_REQUEST);

		JsonObject response = requestProcessor.processConfigureAbilities(request, gameData);
		Assert.assertEquals(JsonConstants.REQUEST_TYPE_CONFIGURE_ABILITIES, response.get(JsonConstants.MESSAGE_TYPE_REQUEST).getAsString());
		JsonArray players = response.get(JsonConstants.PLAYERS).getAsJsonArray();
		Assert.assertEquals(6, players.size());
	}

	@Test
	public void processKickoff() {
		JsonObject request = (JsonObject) parser
				.parse(KICKOFF_REQUEST);

		JsonObject response = requestProcessor.processKickoff(request, gameData);
	}

	@Test
	public void processPlay() {

	}
}
