/**
 * Turn360ThenMove
 * ---------------
 * From a bug reported by Kostas, where the direction of a player becomes
 * 360 degrees, and then the player cannot move.
 */
var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;


exports['Turn 360 then move'] = function(test) {
    // The game and player...
    var game = new MockGame_CalculationInterval(0.1);
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 50.0, 25.0, 350.0);

    // We turn the player to 360 degrees...
    player._setAction_TURN({direction:360});
    game.calculate();

    // The direction should be set to zero...
    test.approx(player.dynamicState.direction, 0.0);

    // The player moves up...
    player._setAction_MOVE({destination:{x:50, y:5}});
    game.calculate();

    // The player should have moved up...
    test.approx(player.dynamicState.position.x, 50);
    test.approx(player.dynamicState.position.y, 24);

    test.done();
};




