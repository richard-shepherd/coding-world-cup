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


exports['Ball not moving'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);

    // We place the ball...
    var ball = game.ball;
    var ballPosition = ball.state.position;
    ballPosition.x = 40.0;
    ballPosition.y = 20.0;

    // We choose one of the players, and put him near the ball...
    var player = game.getPlayer(2);
    

    test.ok(false, "Write this!");
    test.done();
};
