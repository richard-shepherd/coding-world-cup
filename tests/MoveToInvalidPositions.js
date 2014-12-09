/**
 * MoveToInvalidPositions
 * ----------------------
 * Tests that players and goalkeepers cannot move to invalid positions.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;


/**
 * Player moves into left goal area
 * --------------------------------
 */
exports['Player moves into left goal area'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We get a player and put him outside the left goal area...
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 18.5, 25, 270);

    // We get him to move into the goal area...
    player._setAction_MOVE({destination:{x:0.0, y:25.0}, speed:100.0});
    game.calculate();

    // The player should have moved to the edge of the goal area,
    // but not into it...
    var playerPosition = player.dynamicState.position;
    test.equal(player.actionState.action, PlayerState_Action.Action.MOVE);
    test.approx(playerPosition.x, 15.001);
    test.approx(playerPosition.y, 25.0);

    test.done();
};

/**
 * Player moves into right goal area
 * ---------------------------------
 */
exports['Player moves into right goal area'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We get a player and put him outside the left goal area...
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 80.5, 25, 90);

    // We get him to move into the goal area...
    player._setAction_MOVE({destination:{x:100.0, y:25.0}, speed:100.0});
    game.calculate();

    // The player should have moved to the edge of the goal area,
    // but not into it...
    var playerPosition = player.dynamicState.position;
    test.equal(player.actionState.action, PlayerState_Action.Action.MOVE);
    test.approx(playerPosition.x, 84.999);
    test.approx(playerPosition.y, 25.0);

    test.done();
};

/**
 * Right hand goalkeeper moves out of goal area
 * --------------------------------------------
 */
exports['Right hand goalkeeper moves out of goal area'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(0.1);
    game._aiUpdateIntervalSeconds = 1.0;

    // We get the goalkeeper from team 2, and put him in the goal area...
    var player = game.getPlayer(11);
    PlayerTestUtils.setPlayerPosition(player, 93.5, 25, 270);

    // We get him to move out of the goal area...
    player._setAction_MOVE({destination:{x:0.0, y:25.0}, speed:100.0});
    game.calculate();

    // The player should have moved to the edge of the goal area,
    // but not out of it...
    var playerPosition = player.dynamicState.position;
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);
    test.approx(playerPosition.x, 85.5);
    test.approx(playerPosition.y, 25.0);

    test.done();
};

