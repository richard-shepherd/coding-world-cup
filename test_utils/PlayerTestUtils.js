/**
 * PlayerTestUtils
 * ---------------
 * Some utility functions, used in tests, for setting up players.
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
    player.staticState.playerNumber = 123;
    player.staticState.passingAbility = 100.0;
    player.staticState.runningAbility = 100.0;
    return player;
};



// Exports...
exports.createPlayerAt = createPlayerAt;


