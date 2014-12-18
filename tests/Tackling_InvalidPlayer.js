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
var Team = GameLib.Team;


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

    // We tell player1 to tackle...
    player1._setAction_TAKE_POSSESSION();
    game.calculate();

    // We check the results...
    var player1Position = player1.dynamicState.position;
    test.equal(player1Position.x, 38.0);
    test.equal(player1Position.y, 21.0);
    test.equal(player1.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

exports['Tackling the goalkeeper'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We get two players (one the goalkeeper) and give the goalkeeper the ball...
    var player1 = game.getPlayer(Team.NUMBER_OF_PLAYERS + 2);
    PlayerTestUtils.setPlayerPosition(player1, 40.0, 21.0, 88.0);
    var goalkeeperNumber = Team.NUMBER_OF_PLAYERS;
    var goalkeeper = game.getPlayer(goalkeeperNumber);
    PlayerTestUtils.setPlayerPosition(goalkeeper, 38.0, 21.0, 47.0);
    game.giveBallToPlayer(goalkeeper);

    // We tell player1 to tackle...
    player1._setAction_TAKE_POSSESSION();
    game.calculate();

    // We check the results...
    var player1Position = player1.dynamicState.position;
    test.equal(player1.actionState.action, PlayerState_Action.Action.TAKE_POSSESSION);
    test.equal(player1Position.x, 38.0);
    test.equal(player1Position.y, 21.0);

    test.done();
};

exports['Goalkeeper tries to tackle'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We get two players (one the goalkeeper) and give the other player the ball...
    var player1Number = Team.NUMBER_OF_PLAYERS + 2;
    var player1 = game.getPlayer(player1Number);
    PlayerTestUtils.setPlayerPosition(player1, 40.0, 21.0, 88.0);
    var goalkeeperNumber = Team.NUMBER_OF_PLAYERS;
    var goalkeeper = game.getPlayer(goalkeeperNumber);
    PlayerTestUtils.setPlayerPosition(goalkeeper, 38.0, 21.0, 47.0);
    game.giveBallToPlayer(player1);

    // We tell the goalkeeper to tackle player1...
    goalkeeper._setAction_TAKE_POSSESSION();
    game.calculate();

    // We check the results...
    var goalkeeperPosition = goalkeeper.dynamicState.position;
    test.equal(goalkeeper.actionState.action, PlayerState_Action.Action.NONE);
    test.equal(goalkeeperPosition.x, 38.0);
    test.equal(goalkeeperPosition.y, 21.0);

    test.done();
};

exports['Player over 5m away'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We get two players, over 5m apart...
    var player1 = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player1, 40.0, 21.0, 88.0);
    var playerNumberToTackle = Team.NUMBER_OF_PLAYERS + 2;
    var player2 = game.getPlayer(playerNumberToTackle);
    PlayerTestUtils.setPlayerPosition(player2, 32.0, 21.0, 47.0);
    game.giveBallToPlayer(player2);

    // We tell player1 to tackle player2...
    player1._setAction_TAKE_POSSESSION();
    game.calculate();

    // We check the results...
    var player1Position = player1.dynamicState.position;
    test.equal(player1.actionState.action, PlayerState_Action.Action.NONE);
    test.equal(player1Position.x, 40.0);
    test.equal(player1Position.y, 21.0);

    test.done();
};



