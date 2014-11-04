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
var Position = require('./../utils/Position');

/**
 * @constructor
 */
function PlayerState_Intentions() {
    // The currently intention...
    this.action = PlayerState_Intentions.Action.NONE;

    // For the MOVE intention...
    this.destination = new Position();
    this.speed = 0.0; // As a percentage of the player's max speed.

    // For the TURN intention...
    this.direction = 0.0;
}

/**
 * Action
 * ------
 * An enum for the desired action.
 */
PlayerState_Intentions.Action = {
    NONE: 0,
    TURN: 1,
    MOVE: 2
};

// Exports...
module.exports = PlayerState_Intentions;
