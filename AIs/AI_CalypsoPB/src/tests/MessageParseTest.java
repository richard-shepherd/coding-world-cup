package tests;

import main.AICalypsoPB;

import org.junit.Before;
import org.junit.Test;

import utils.GeometryUtils;
import element.Position;

public class MessageParseTest {
	String GAME_START = "{\"eventType\":\"GAME_START\",\"pitch\":{\"width\":100,\"height\":50,\"goalCentre\":25,\"goalY1\":21,\"goalY2\":29,\"centreSpot\":{\"x\":50,\"y\":25},\"centreCircleRadius\":10,\"goalAreaRadius\":15},\"gameLengthSeconds\":1800,\"messageType\":\"EVENT\"}";

	String TEAM_INFO = "{\"eventType\":\"TEAM_INFO\",\"teamNumber\":1,\"players\":[{\"playerNumber\":0,\"playerType\":\"P\"},{\"playerNumber\":1,\"playerType\":\"P\"},{\"playerNumber\":2,\"playerType\":\"P\"},{\"playerNumber\":3,\"playerType\":\"P\"},{\"playerNumber\":4,\"playerType\":\"P\"},{\"playerNumber\":5,\"playerType\":\"G\"}],\"messageType\":\"EVENT\"}";
	String KICK_OFF = "{\"eventType\":\"KICKOFF\",\"team1\":{\"name\":\"\",\"score\":0,\"direction\":\"RIGHT\"},\"team2\":{\"name\":\"\",\"score\":0,\"direction\":\"LEFT\"},\"teamKickingOff\":1,\"messageType\":\"EVENT\"}";
	String START_OF_TURN = "{\"game\":{\"currentTimeSeconds\":7.2},\"ball\":{\"position\":{\"x\":52.239,\"y\":19.314},\"vector\":{\"x\":-0.915,\"y\":-0.403},\"speed\":14,\"controllingPlayerNumber\":-1},\"team1\":{\"team\":{\"name\":\"\",\"score\":0,\"direction\":\"RIGHT\"},\"players\":[{\"staticState\":{\"playerNumber\":0,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":25,\"y\":10},\"hasBall\":false,\"energy\":100,\"direction\":270}},{\"staticState\":{\"playerNumber\":1,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":25,\"y\":25},\"hasBall\":false,\"energy\":100,\"direction\":270}},{\"staticState\":{\"playerNumber\":2,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":25,\"y\":40},\"hasBall\":false,\"energy\":100,\"direction\":270}},{\"staticState\":{\"playerNumber\":3,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":73.068,\"y\":30.268},\"hasBall\":false,\"energy\":100,\"direction\":298.001}},{\"staticState\":{\"playerNumber\":4,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":72.957,\"y\":31.666},\"hasBall\":false,\"energy\":100,\"direction\":301.252}},{\"staticState\":{\"playerNumber\":5,\"playerType\":\"G\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":4.977,\"y\":24.526},\"hasBall\":false,\"energy\":100,\"direction\":354.952}}]},\"team2\":{\"team\":{\"name\":\"\",\"score\":0,\"direction\":\"LEFT\"},\"players\":[{\"staticState\":{\"playerNumber\":6,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":81.198,\"y\":33.952},\"hasBall\":false,\"energy\":100,\"direction\":296.958}},{\"staticState\":{\"playerNumber\":7,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":78.653,\"y\":32.417},\"hasBall\":false,\"energy\":100,\"direction\":296.521}},{\"staticState\":{\"playerNumber\":8,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":76.46,\"y\":29.984},\"hasBall\":false,\"energy\":100,\"direction\":293.775}},{\"staticState\":{\"playerNumber\":9,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":25,\"y\":35},\"hasBall\":false,\"energy\":100,\"direction\":270}},{\"staticState\":{\"playerNumber\":10,\"playerType\":\"P\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":25,\"y\":15},\"hasBall\":false,\"energy\":100,\"direction\":270}},{\"staticState\":{\"playerNumber\":11,\"playerType\":\"G\",\"kickingAbility\":66.667,\"runningAbility\":66.667,\"ballControlAbility\":66.667,\"tacklingAbility\":66.667},\"dynamicState\":{\"position\":{\"x\":95.03,\"y\":24.454},\"hasBall\":false,\"energy\":100,\"direction\":5.975}}]},\"eventType\":\"START_OF_TURN\",\"messageType\":\"EVENT\"}";
	String GOAL = "{\"eventType\":\"GOAL\",\"team1\":{\"name\":\"\",\"score\":2,\"direction\":\"LEFT\"},\"team2\":{\"name\":\"\",\"score\":7,\"direction\":\"RIGHT\"},\"messageType\":\"EVENT\"}";
	String HALF_TIME = "{\"eventType\":\"HALF_TIME\",\"messageType\":\"EVENT\"}";

	AICalypsoPB aiCalypsoPB;

	@Before
	public void setUp() {
		aiCalypsoPB = new AICalypsoPB();
	}

	/*
	 * @Test public void testGameStart() {
	 * aiCalypsoPB.processJsonMessage(GAME_START);
	 * 
	 * }
	 * 
	 * @Test public void testTeamInfo() {
	 * aiCalypsoPB.processJsonMessage(TEAM_INFO);
	 * 
	 * }
	 * 
	 * @Test public void testKickOff() {
	 * aiCalypsoPB.processJsonMessage(KICK_OFF);
	 * 
	 * }
	 * 
	 * @Test public void testStartOfTurn() {
	 * aiCalypsoPB.processJsonMessage(START_OF_TURN);
	 * 
	 * }
	 * 
	 * @Test public void testGoal() { aiCalypsoPB.processJsonMessage(GOAL);
	 * 
	 * }
	 * 
	 * @Test public void testHalfTime() {
	 * aiCalypsoPB.processJsonMessage(HALF_TIME);
	 * 
	 * }
	 * 
	 * @Test public void testALL() { aiCalypsoPB.processJsonMessage(GAME_START);
	 * aiCalypsoPB.processJsonMessage(TEAM_INFO);
	 * aiCalypsoPB.processJsonMessage(KICK_OFF);
	 * aiCalypsoPB.processJsonMessage(START_OF_TURN);
	 * aiCalypsoPB.processJsonMessage(GOAL);
	 * aiCalypsoPB.processJsonMessage(HALF_TIME); }
	 */

	@Test
	public void testAngle() {
		double t = GeometryUtils.getAngle(new Position(0, 0),
				new Position(1, 1));
		System.out.println(t);
		t = GeometryUtils.getAngle(new Position(0, 0), new Position(1, -1));
		System.out.println(t);
		t = GeometryUtils.getAngle(new Position(0, 0), new Position(-1, -1));
		System.out.println(t);
		t = GeometryUtils.getAngle(new Position(0, 0), new Position(-1, 1));
		System.out.println(t);

	}

}
