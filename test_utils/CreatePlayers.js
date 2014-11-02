/**
 * TestUtils
 * ---------
 * Some utility functions and classes used by tests.
 */
var GameLib = require('../game/index');
var Player = GameLib.Player;

/**
 * createPlayerInCentreOfPitch
 * ---------------------------
 * Returns a Player in the centre of the pitch, with maximum
 * energy, stamina etc.
 */
exports.createPlayerAt = function(x, y) {
    var player = new Player();
    player._dynamicState.position.x = x;
    player._dynamicState.position.x = y;
    player._dynamicState.energy = Player.MAX_ENERGY;
    return player;
}


