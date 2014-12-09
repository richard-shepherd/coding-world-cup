/**
 * PlayerTestUtils
 * ---------------
 * Some utility functions, used in tests, for setting up players.
 */

var GameLib = require('../game/index');
var Player = GameLib.Player;

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
exports.setPlayerPosition = setPlayerPosition;


