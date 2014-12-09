/**
 * KickoffPositions
 * ----------------
 * Tests setting players up in kickoff positions.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;


/**
 * Player put on centre spot
 * -------------------------
 * Tests that the nearest player to the centre spot (from the team kicking off)
 * is put on the centre spot and given the ball.
 */
exports['Player put on centre spot'] = function(test) {
    // We create a game, and put all players in default positions...
    var game = new MockGame_CalculationInterval(1.0);
    game.setDefaultKickoffPositions();

    // We set the kickoff positions, putting one player closer to the centre spot...
    var team1 = game.getTeam1();
    team1.processKickoffResponse(game, {players:[{playerNumber:1, position:{x:35, y:25}, direction:45}]},  true);

    // Player 0 should be in the default position...
    var player0DynamicState = game.getPlayer(0).dynamicState;
    test.approx(player0DynamicState.direction, 90.0);
    test.approx(player0DynamicState.position.x, 25.0);
    test.approx(player0DynamicState.position.y, 25.0);

    // The goalkeeper should be in the default position...
    var goalKeeperDynamicState = game.getPlayer(5).dynamicState;
    test.approx(goalKeeperDynamicState.direction, 90.0);
    test.approx(goalKeeperDynamicState.position.x, 0.5);
    test.approx(goalKeeperDynamicState.position.y, 25.0);

    // Player 1 should be on the centre spot and have the ball...
    var player1DynamicState = game.getPlayer(1).dynamicState;
    test.approx(player1DynamicState.direction, 45.0);
    test.approx(player1DynamicState.position.x, 50.0);
    test.approx(player1DynamicState.position.y, 25.0);
    test.equal(player1DynamicState.hasBall, true);

    test.done();
};
