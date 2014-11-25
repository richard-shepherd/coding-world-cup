/**
 * Kicking_100pcAbility
 * --------------------
 * Some tests of players kicking the ball where they have 100% passing
 * ability.
 *
 * These just test the effect of kicking at different angles away from
 * the direction the player is facing.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;


/**
 * Kick up facing up
 * -----------------
 * Tests kicking the ball upwards by a player facing up.
 */
exports['Kick up facing up'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set ball friction to zero to make the calculations easier...
    var ball = game.ball;
    ball.friction = 0.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = PlayerTestUtils.createPlayerAt(50.0, 25.0, 0.0);
    game.giveBallToPlayer(player);

    // The player kicks the ball...
    player._setAction_KICK({destination:{x:50.0, y:2.0}, speed:100});
    player.processAction(game);

    // The ball should have moved 3m...
    var ballState = ball.state;
    var ballPosition = ballState.position;
    test.approx(ballPosition.x, 50.0);
    test.approx(ballPosition.y, 22.0);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(player.dynamicState.hasBall, false);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Kick down facing down
 * ---------------------
 * Tests kicking the ball down by a player facing down.
 */
exports['Kick up facing up'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set ball friction to zero to make the calculations easier...
    var ball = game.ball;
    ball.friction = 0.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = PlayerTestUtils.createPlayerAt(50.0, 25.0, 18.0);
    game.giveBallToPlayer(player);

    // The player kicks the ball...
    player._setAction_KICK({destination:{x:50.0, y:50.0}, speed:100});
    player.processAction(game);

    // The ball should have moved 3m...
    var ballState = ball.state;
    var ballPosition = ballState.position;
    test.approx(ballPosition.x, 50.0);
    test.approx(ballPosition.y, 28.0);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(player.dynamicState.hasBall, false);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Kick right facing right
 * -----------------------
 * Tests kicking the ball right by a player facing right.
 */
exports['Kick up facing up'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set ball friction to zero to make the calculations easier...
    var ball = game.ball;
    ball.friction = 0.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = PlayerTestUtils.createPlayerAt(50.0, 25.0, 90.0);
    game.giveBallToPlayer(player);

    // The player kicks the ball...
    player._setAction_KICK({destination:{x:90.0, y:25.0}, speed:100});
    player.processAction(game);

    // The ball should have moved 3m...
    var ballState = ball.state;
    var ballPosition = ballState.position;
    test.approx(ballPosition.x, 53.0);
    test.approx(ballPosition.y, 25.0);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(player.dynamicState.hasBall, false);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Kick left facing left
 * ---------------------
 * Tests kicking the ball left by a player facing left.
 */
exports['Kick up facing up'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set ball friction to zero to make the calculations easier...
    var ball = game.ball;
    ball.friction = 0.0;

    // We create a player in the centre of the pitch and give him the ball...
    var player = PlayerTestUtils.createPlayerAt(50.0, 25.0, 270.0);
    game.giveBallToPlayer(player);

    // The player kicks the ball...
    player._setAction_KICK({destination:{x:10.0, y:25.0}, speed:100});
    player.processAction(game);

    // The ball should have moved 3m...
    var ballState = ball.state;
    var ballPosition = ballState.position;
    test.approx(ballPosition.x, 47.0);
    test.approx(ballPosition.y, 25.0);
    test.equal(ballState.controllingPlayerNumber, -1);
    test.equal(player.dynamicState.hasBall, false);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};
