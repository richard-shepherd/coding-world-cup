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

    // The player's energy. (Between 0.0 and Player.MAX_ENERGY.)
    this.energy = 0.0;
}

// Exports...
module.exports = PlayerState_Dynamic;