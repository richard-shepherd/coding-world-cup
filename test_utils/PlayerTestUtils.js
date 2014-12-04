/**
 * PlayerTestUtils
 * ---------------
 * Some utility functions, used in tests, for setting up players.
 */

var GameLib = require('../game/index');
var Player = GameLib.Player;

// TODO: Remove this
///**
// * createPlayerAt
// * --------------
// * Returns a Player with maximum abilities at the position passed in.
// */
//function createPlayerAt(x, y, direction) {
//
//
//    var player = new Player();
//    player.staticState.playerNumber = 123;
//    setPlayerPosition(player, x, y, direction);
//    return player;
//};

/**
 * setPlayerPosition
 * -----------------
 * Puts the player at the point specified, with max abilities.
 */
function setPlayerPosition(player, x, y, direction) {
    player.dynamicState.position.x = x;
    player.dynamicState.position.y = y;
    player.dynamicState.direction = direction;
    player.dynamicState.energy = 100.0;
    player.staticState.kickingAbility = 100.0;
    player.staticState.runningAbility = 100.0;
    player.staticState.ballControlAbility = 100.0;
    player.staticState.tacklingAbility = 100.0;
}

// Exports...
// TODO: Remove this
//exports.createPlayerAt = createPlayerAt;
exports.setPlayerPosition = setPlayerPosition;


