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

    // Whether the player has the ball or not...
    this.hasBall = false;
}

// Exports...
module.exports = PlayerState_Dynamic;