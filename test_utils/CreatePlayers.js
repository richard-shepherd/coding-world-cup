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
    player.dynamicState.position.x = x;
    player.dynamicState.position.y = y;
    player.dynamicState.direction = direction;
    player.dynamicState.energy = Player.MAX_ENERGY;
    player.staticState.passingAbility = 100.0;
    player.staticState.runningAbility = 100.0;
    return player;
};


// Exports...
exports.createPlayerAt = createPlayerAt;


