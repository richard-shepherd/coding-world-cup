var CreatePlayers = require('../test_utils').CreatePlayers;

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

    test.done();
};



