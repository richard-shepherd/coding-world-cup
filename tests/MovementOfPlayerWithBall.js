/**
 * MovementOfPlayerWithBall
 * ------------------------
 * Tests that the player will the ball moves slower than normal, and that
 * the ball moves with him.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;


/**
 * Movement with ball
 * ------------------
 */
exports['Movement with ball'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(1.0);
    game._aiUpdateIntervalSeconds = 1.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 50.0, 25.0, 90.0);
    game.giveBallToPlayer(player);

    // We move the player...
    player._setAction_MOVE({destination:{x:100.0, y:25.0}, speed:100.0});
    game.calculate();

    // Normally the player would have moved 10m, but with the ball only 4m...
    var playerPosition = player.dynamicState.position;
    var ballPosition = game.ball.state.position;
    test.approx(playerPosition.x, 54.0);
    test.approx(playerPosition.y, 25.0);

    // The ball should move with the player...
    test.approx(ballPosition.x, 54.0);
    test.approx(ballPosition.y, 25.0);

    test.done();
};
