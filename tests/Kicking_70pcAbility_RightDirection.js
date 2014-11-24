/**
 * Kicking_70pcAbility_RightDirection
 * ----------------------------------
 * Tests of kicking in the same direction as the player is facing, by
 * a player with 70% passing ability.
 */
var TestUtilsLib = require('../test_utils');
var CreatePlayers = TestUtilsLib.CreatePlayers;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var MockRandom = TestUtilsLib.MockRandom;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;


/**
 * Lucky
 * -----
 * Despite the lower ability, the ball randomly goes straight.
 */
exports['Lucky'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set ball friction to zero to make the calculations easier...
    var ball = game.ball;
    ball.friction = 0.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 288.0);
    player.staticState.passingAbility = 70.0;
    game.giveBallToPlayer(player);

    // We mock the random number generator...
    var mockRandom = new MockRandom([0.5, 0.5]);
    player._random = mockRandom;

    // The player kicks the ball...
    var kickTo = Utils.pointFromDirection({x:50.0, y:25.0}, 288.0, 20.0);
    player._setAction_KICK({destination:kickTo, speed:100});
    player.processAction(game);

    // The ball should have moved 3m...
    var ballState = ball.state;
    var ballPosition = ballState.position;
    var ballAt = Utils.pointFromDirection({x:50.0, y:25.0}, 288.0, 3.0);
    test.approx(ballPosition.x, ballAt.x);
    test.approx(ballPosition.y, ballAt.y);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(player.dynamicState.hasBall, false);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * 70pc ability
 * ------------
 */
exports['70pc ability'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set ball friction to zero to make the calculations easier...
    var ball = game.ball;
    ball.friction = 0.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 77.0);
    player.staticState.passingAbility = 70.0;
    game.giveBallToPlayer(player);

    // We mock the random number generator...
    var mockRandom = new MockRandom([0.65, 0.0]);
    player._random = mockRandom;

    // The player kicks the ball...
    var kickTo = Utils.pointFromDirection({x:50.0, y:25.0}, 77.0, 20.0);
    player._setAction_KICK({destination:kickTo, speed:100});
    player.processAction(game);

    // The ball should have moved 3m...
    var ballState = ball.state;
    var ballPosition = ballState.position;
    var ballAt = Utils.pointFromDirection({x:50.0, y:25.0}, 93.2, 3.0); // 93.2 = 77 + (0.3 * 360) * 0.65 - (0.3 * 360) / 2
    test.approx(ballPosition.x, ballAt.x);
    test.approx(ballPosition.y, ballAt.y);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(player.dynamicState.hasBall, false);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

