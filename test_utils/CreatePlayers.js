/**
 * TestUtils
 * ---------
 * Some utility functions and classes used by tests.
 */
var GameLib = require('../game/index');
var Player = GameLib.Player;

/**
 * createPlayerAt
 * --------------
 * Returns a Player at the position passed in.
 */
function createPlayerAt(x, y) {
    var player = new Player();
    player._dynamicState.position.x = x;
    player._dynamicState.position.y = y;
    player._dynamicState.energy = Player.MAX_ENERGY;
    return player;
};

/**
 * createPlayerFacing
 * ------------------
 * Creates a player facing the direction passed in.
 */
function createPlayerFacing(direction) {
    var player = createPlayerAt(50.0, 25.0);
    player._dynamicState.direction = direction;
    return player;
};

// Exports...
exports.createPlayerAt = createPlayerAt;
exports.createPlayerFacing = createPlayerFacing;


