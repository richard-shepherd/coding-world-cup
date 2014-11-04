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
function createPlayerAt(x, y, direction) {
    var player = new Player();
    player._dynamicState.position.x = x;
    player._dynamicState.position.y = y;
    player._dynamicState.direction = direction;
    player._dynamicState.energy = Player.MAX_ENERGY;
    player._staticState.passingAbility = 100.0;
    player._staticState.runningAbility = 100.0;
    return player;
};


// Exports...
exports.createPlayerAt = createPlayerAt;


