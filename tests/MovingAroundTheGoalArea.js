/**
 * MovingAroundTheGoalArea
 * -----------------------
 * Tests that players 'slide' around the goal area when they try to
 * move into it.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;
var Team = GameLib.Team;

/**
 * Slide around
 * ------------
 */
exports['Slide around'] = function(test) {
    // We create a game...
    var game = new MockGame_CalculationInterval(1.0);
    game._calculationIntervalSeconds = 1.0;

    // We get a player and put them at the edge of the goal-area...
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 85, 25, 135);

    // We get the player to move to a place in the goal-area...
    player._setAction_MOVE({destination:{x:100, y:40}, speed:100});
    game.calculate();

    // The player should have slid around the goal area, to a location
    // with the same y-coordinate as the result without sliding, but
    // with x such that it is on the goal-area line...
    var playerPosition = player.dynamicState.position;
    test.approx(playerPosition.x, 86.77024344467705);
    test.approx(playerPosition.y, 32.071067811865476);

    test.done();
};



