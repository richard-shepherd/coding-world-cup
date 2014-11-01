/**
 * PlayerState_Intentions
 * ----------------------
 * Holds the current intentions of the player.
 *
 * These include:
 * - Running towards a given point on the pitch.
 * - The speed the player should move at.
 * - Tackling or fouling a player.
 *
 * Some actions - such as tackling - can only be initiated if you
 * are within a certain range of their target.
 *
 * Players may not always want to move at maximum speed as this will
 * use up their energy faster.
 */
var Position = require('./Position');

/**
 * @constructor
 */
function PlayerState_Intentions() {
    // The point on the pitch the player is moving towards...
    this.destination = new Position();

    // The speed the player is moving towards the destination, as
    // a percentage of their current maximum speed...
    this.speed = 0.0;
}

// Exports...
module.exports = PlayerState_Intentions;
