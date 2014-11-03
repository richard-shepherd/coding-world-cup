var TestUtilsLib = require('../test_utils');
var CreatePlayers = TestUtilsLib.CreatePlayers;
var GameMocks = TestUtilsLib.GameMocks;

/**
 * Tests that a player can move to the right.
 */
exports['Simple movement right'] = function(test) {
    // We create a player at the centre of the pitch...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0);

    // We set the player's intention to move to the right at full speed...
    player._intentionsState.destination.x = 100.0;
    player._intentionsState.destination.y = 25.0;
    player._intentionsState.speed = 100.0;

    // We set the calculation interval to 1 second...
    var game = new GameMocks.MockGame_CalculationInterval(1.0);
    player.updatePosition(game);

    // The player should be moving at 10m/s, so should have moved
    // 10 metres to the right...
    test.approx(player._dynamicState.position.x, 60.0);
    test.approx(player._dynamicState.position.y, 25.0);

    test.done();
};



