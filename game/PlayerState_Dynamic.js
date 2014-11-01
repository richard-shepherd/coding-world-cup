/**
 * PlayerState_Dynamic
 * -------------------
 * Player state - such as position - which changes as the game
 * is played.
 */
var Position = require('./Position');

/**
 * @constructor
 */
function PlayerState_Dynamic() {
    // The player's position...
    this.position = new Position();
}

// Exports...
module.exports = PlayerState_Dynamic;