/**
 * Tackling_InvalidPlayer
 * ----------------------
 * Some tests of trying to tackle a player that cannot be tackled
 * by the one attempting to do the tackling.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;
var UtilsLib = require('../utils');


exports['On same team'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We get two players (from the same team) and give one of them the ball...
    var player1 = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player1, 40.0, 21.0, 88.0);
    var player2 = game.getPlayer(2);
    PlayerTestUtils.setPlayerPosition(player2, 38.0, 21.0, 47.0);
    game.giveBallToPlayer(player2);

    // We tell player1 to tackle player2...
    player1._setAction_TACKLE({player:2, strength:100.0});
    game.calculate();

    // We check the results...
    var player1Position = player1.dynamicState.position;
    test.equal(player1.actionState.action, PlayerState_Action.Action.NONE);
    test.equal(player1Position.x, 40.0);
    test.equal(player1Position.y, 21.0);

    test.done();
};



