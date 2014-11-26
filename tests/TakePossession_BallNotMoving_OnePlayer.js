/**
 * TakePossession_BallNotMoving_OnePlayer
 * --------------------------------------
 * Tests one player taking possession when the ball is not moving.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var MockRandom = TestUtilsLib.MockRandom;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;


/**
 * Ball not moving
 * ---------------
 */
exports['Ball not moving'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We place the ball...
    var ballState = game.ball.state;
    var ballPosition = ballState.position;
    ballPosition.x = 40.0;
    ballPosition.y = 20.0;

    // We choose one of the players, and put him near the ball...
    var player = game.getPlayer(2);
    PlayerTestUtils.setPlayerPosition(player, 38.0, 21.0, 47.0);

    // We tell the player to take possession, and calculate...
    player._setAction_TAKE_POSSESSION();
    game.calculate();

    // The player should have moved to the ball and have possession...
    var playerDynamicState = player.dynamicState;
    var playerActionState = player.actionState;
    var playerPosition = playerDynamicState.position;
    test.equal(playerDynamicState.hasBall, true);
    test.equal(ballState.controllingPlayerNumber, 2);
    test.equal(playerActionState.action, PlayerState_Action.Action.NONE);
    test.approx(playerPosition.x, 40.0);
    test.approx(playerPosition.y, 20.0);

    test.done();
};

/**
 * Too far away
 * ------------
 * The player is too far away to take possession.
 */
exports['Too far away'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We place the ball...
    var ballState = game.ball.state;
    var ballPosition = ballState.position;
    ballPosition.x = 40.0;
    ballPosition.y = 20.0;

    // We choose one of the players, and put him near the ball...
    var player = game.getPlayer(2);
    PlayerTestUtils.setPlayerPosition(player, 34.0, 20.0, 47.0);

    // We tell the player to take possession, and calculate...
    player._setAction_TAKE_POSSESSION();
    game.calculate();

    // The player was too far away, so will not have moved
    // or taken possession...
    var playerDynamicState = player.dynamicState;
    var playerActionState = player.actionState;
    var playerPosition = playerDynamicState.position;
    test.approx(playerPosition.x, 34.0);
    test.approx(playerPosition.y, 20.0);
    test.equal(playerDynamicState.hasBall, false);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(playerActionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Another player has the ball
 * ---------------------------
 */
exports['Another player has the ball'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We create two players and give one the ball...
    var player1 = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player1, 34.0, 20.0, 47.0);

    var player2 = game.getPlayer(2);
    PlayerTestUtils.setPlayerPosition(player2, 36.0, 20.0, 42.0);
    game.giveBallToPlayer(player2);

    // We tell the player to take possession, and calculate...
    player1._setAction_TAKE_POSSESSION();
    game.calculate();

    // The other player had possession, so will not have moved
    // or taken possession...
    var playerDynamicState = player1.dynamicState;
    var playerActionState = player1.actionState;
    var playerPosition = playerDynamicState.position;
    var ballState = game.ball.state;
    var ballPosition = ballState.position;
    test.approx(playerPosition.x, 34.0);
    test.approx(playerPosition.y, 20.0);
    test.equal(playerDynamicState.hasBall, false);
    test.equal(ballState.controllingPlayerNumber, 2);
    test.approx(ballPosition.x, 36.0);
    test.approx(ballPosition.y, 20.0);
    test.equal(playerActionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

