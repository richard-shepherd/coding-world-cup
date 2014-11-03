var TestUtilsLib = require('../test_utils');
var CreatePlayers = TestUtilsLib.CreatePlayers;
var GameMocks = TestUtilsLib.GameMocks;
var GameLib = require('../game');
var PlayerState_Intentions = GameLib.PlayerState_Intentions;


/**
 * Tests that we can make a short turn to the right, and
 * not overshoot.
 */
exports['Short turn to the right'] = function(test) {
    // We create a player and set their initial direction...
    var player = CreatePlayers.createPlayerFacing(20.0);

    // We set the intention to turn...
    player._intentionsState.action = PlayerState_Intentions.Action.TURN;
    player._intentionsState.direction = 55.0;

    // We simulate a 0.1 second interval, which should be more
    // than enough to turn this far...
    var game = new GameMocks.MockGame_CalculationInterval(1.0);
    player.updatePosition(game);

    // We confirm that we're facing the new direction...
    test.approx(player._dynamicState.direction, 55.0);

    // We update and test again, to make sure the player has not
    // turned any more...
    player.updatePosition(game);
    test.approx(player._dynamicState.direction, 55.0);

    test.done();
};

// TODO: turn left
// TODO: turn right past 360
// TODO: turn left past 0

