/**
 * Kicking_100pcAbility_Angles
 * ---------------------------
 * Some tests of kicking at an angles different from the direction
 * the player is facing. This introduces a degree of variation in
 * the direction the ball ends up travelling.
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
 * Kick backwards - Lucky
 * ----------------------
 * Kicks backwards, and is "lucky" that it works.
 */
exports['Kick backwards - Lucky'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set ball friction to zero to make the calculations easier...
    var ball = game.ball;
    ball.friction = 0.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 20.0);
    game.giveBallToPlayer(player);

    // We mock the random number generator...
    // In this case to make the ball go where the player intended.
    var mockRandom = new MockRandom([0.5, 0.5]);
    player._random = mockRandom;

    // The player kicks the ball...
    var kickTo = Utils.pointFromDirection({x:50.0, y:25.0}, 200.0, 20.0);
    player._setAction_KICK({destination:kickTo, speed:100});
    player.processAction(game);

    // The ball should have moved 3m...
    var ballState = ball.state;
    var ballPosition = ballState.position;
    var ballAt = Utils.pointFromDirection({x:50.0, y:25.0}, 200.0, 3.0);
    test.approx(ballPosition.x, ballAt.x);
    test.approx(ballPosition.y, ballAt.y);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(player.dynamicState.hasBall, false);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Kick backwards
 * --------------
 */
exports['Kick backwards'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set ball friction to zero to make the calculations easier...
    var ball = game.ball;
    ball.friction = 0.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 20.0);
    game.giveBallToPlayer(player);

    // We mock the random number generator...
    // In this case to make the ball go where the player intended.
    var mockRandom = new MockRandom([0.5, 0.2]);
    player._random = mockRandom;

    // The player kicks the ball...
    var kickTo = Utils.pointFromDirection({x:50.0, y:25.0}, 200.0, 20.0);
    player._setAction_KICK({destination:kickTo, speed:100});
    player.processAction(game);

    // The ball should have moved 3m...
    var ballState = ball.state;
    var ballPosition = ballState.position;
    var ballAt = Utils.pointFromDirection({x:50.0, y:25.0}, 173.0, 3.0); // 173 = 200 + 0.2 * 90 - 45
    test.approx(ballPosition.x, ballAt.x);
    test.approx(ballPosition.y, ballAt.y);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(player.dynamicState.hasBall, false);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Kick sideways
 * -------------
 */
exports['Kick sideways'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set ball friction to zero to make the calculations easier...
    var ball = game.ball;
    ball.friction = 0.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 250.0);
    game.giveBallToPlayer(player);

    // We mock the random number generator...
    // In this case to make the ball go where the player intended.
    var mockRandom = new MockRandom([0.5, 0.8]);
    player._random = mockRandom;

    // The player kicks the ball...
    var kickTo = Utils.pointFromDirection({x:50.0, y:25.0}, 160.0, 20.0);
    player._setAction_KICK({destination:kickTo, speed:100});
    player.processAction(game);

    // The ball should have moved 3m...
    var ballState = ball.state;
    var ballPosition = ballState.position;
    var ballAt = Utils.pointFromDirection({x:50.0, y:25.0}, 173.5, 3.0); // 173.5 = 160 + 45 * 0.8 - 45/2.0
    test.approx(ballPosition.x, ballAt.x);
    test.approx(ballPosition.y, ballAt.y);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(player.dynamicState.hasBall, false);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

