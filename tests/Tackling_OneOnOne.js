/**
 * Tackling_OneOnOne
 * -----------------
 * Some tests of one player tackling another.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;
var Team = GameLib.Team;
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;


exports['Equal ability'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We get two players...
    var player1 = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player1, 40.0, 21.0, 88.0);
    var playerNumberToTackle = Team.NUMBER_OF_PLAYERS + 2;
    var player2 = game.getPlayer(playerNumberToTackle);
    PlayerTestUtils.setPlayerPosition(player2, 38.0, 21.0, 47.0);
    game.giveBallToPlayer(player2);

    // We tell player1 to tackle player2...
    player1._setAction_TACKLE({player:playerNumberToTackle, strength:100.0});
    game.calculate();

    // We check the results (player1 should have the ball)...
    var player1Position = player1.dynamicState.position;
    var player2Position = player2.dynamicState.position;
    var ballState = game.ball.state;
    test.equal(player1.actionState.action, PlayerState_Action.Action.NONE);
    test.equal(ballState.controllingPlayerNumber, 1);
    var distance = Utils.distanceBetween(player1Position, player2Position);
    test.lessThanOrEqual(distance, 0.5);

    test.done();
};
