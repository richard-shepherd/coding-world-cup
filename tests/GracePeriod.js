/**
 * GracePeriod
 * -----------
 * After taking possession, there is a grace period of a number of
 * turns before other players can take possession.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var MockRandom = TestUtilsLib.MockRandom;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;
var Team = GameLib.Team;


/**
 * Tests the grace period after taking possession of a non-controlled ball.
 */
exports['take possession'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);
    game._gracePeriodTurns = 2;

    // We set the random generator.
    game._random = new MockRandom([0.4]);

    // We place the ball...
    var ballState = game.ball.state;
    var ballPosition = ballState.position;
    ballPosition.x = 40.0;
    ballPosition.y = 20.0;

    // Player 2 takes possession...
    var player2 = game.getPlayer(2);
    PlayerTestUtils.setPlayerPosition(player2, 40.0, 20.0, 47.0);
    player2._setAction_TAKE_POSSESSION();
    game.calculate();
    test.equal(ballState.controllingPlayerNumber, 2);
    test.equal(player2.actionState.action, PlayerState_Action.Action.NONE);

    // Player 3 tries to take possession.
    // This should not work on the first two attempts because of the grace period.
    var player3 = game.getPlayer(3);
    PlayerTestUtils.setPlayerPosition(player3, 40.0, 20.0, 47.0);
    player3._setAction_TAKE_POSSESSION();

    // Turn 1...
    game.calculate();
    test.equal(ballState.controllingPlayerNumber, 2);
    test.equal(player3.actionState.action, PlayerState_Action.Action.TAKE_POSSESSION);

    // Turn 2...
    game.calculate();
    test.equal(ballState.controllingPlayerNumber, 2);
    test.equal(player3.actionState.action, PlayerState_Action.Action.TAKE_POSSESSION);

    // Turn 3...
    game.calculate();
    test.equal(ballState.controllingPlayerNumber, 3);
    test.equal(player3.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

exports['tackling'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(0.1);

    // We set the random number generator...
    game._random = new MockRandom([0.4]);

    // We get two players...
    var player1 = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player1, 40.0, 21.0, 88.0);
    var playerNumberToTackle = Team.NUMBER_OF_PLAYERS + 2;
    var player2 = game.getPlayer(playerNumberToTackle);
    PlayerTestUtils.setPlayerPosition(player2, 40.0, 21.0, 47.0);
    game.giveBallToPlayer(player2);

    // Player 1 tackles and gets the ball...
    player1._setAction_TAKE_POSSESSION();
    game.calculate();
    test.equal(player1.dynamicState.hasBall, true);
    test.equal(player2.dynamicState.hasBall, false);

    // Player 2 tackles back, but should not get the ball on the first
    // attempt because of the grace period...
    game._gracePeriodTurns = 1;
    player2._setAction_TAKE_POSSESSION();

    // Turn 1...
    game.calculate();
    test.equal(player1.dynamicState.hasBall, true);
    test.equal(player2.dynamicState.hasBall, false);

    // Turn 2...
    game.calculate();
    test.equal(player1.dynamicState.hasBall, false);
    test.equal(player2.dynamicState.hasBall, true);

    test.done();
};


