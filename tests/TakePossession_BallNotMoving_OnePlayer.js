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

    // We set the random generator.
    // In this case the random values don't matter much, as the ball
    // is not moving and any klutz can get it!
    game._random = new MockRandom([0.4]);

    // We place the ball...
    var ballState = game.ball.state;
    var ballPosition = ballState.position;
    ballPosition.x = 40.0;
    ballPosition.y = 20.0;

    // We choose one of the players, and put him near the ball.
    // He has fairly poor ball-control. But the ball isn't moving...
    var player = game.getPlayer(2);
    PlayerTestUtils.setPlayerPosition(player, 38.0, 21.0, 47.0);
    player.staticState.ballControlAbility = 30.0;

    // We tell the player to take possession, and calculate...
    player._setAction_TAKE_POSSESSION();
    game.calculate();

    // The player should have moved close to the ball and have possession...
    var playerDynamicState = player.dynamicState;
    var playerActionState = player.actionState;
    var playerPosition = playerDynamicState.position;
    var distance = Utils.distanceBetween(playerPosition, {x:40, y:20});
    test.equal(playerDynamicState.hasBall, true);
    test.equal(ballState.controllingPlayerNumber, 2);
    test.equal(playerActionState.action, PlayerState_Action.Action.NONE);
    test.lessThanOrEqual(distance, 0.5);

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

