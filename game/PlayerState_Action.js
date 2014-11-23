/**
 * PlayerState_Action
 * ------------------
 * Holds the current action (intention) of the player.
 *
 * Types of action include:
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
function PlayerState_Action() {
    // The currently action...
    this.action = PlayerState_Action.Action.NONE;

    // For the MOVE action...
    this.moveDestination = new Position(0.0, 0.0);
    this.moveSpeed = 0.0; // As a percentage of the player or ball's max speed.

    // For the TURN action...
    this.direction = 0.0;

    //// For the MOVE & KICK actions...
    //this.moveDestination = new Position(0.0, 0.0);
    //this.speed = 0.0; // As a percentage of the player or ball's max speed.

}

/**
 * Action
 * ------
 * An enum for the desired action.
 */
PlayerState_Action.Action = {
    NONE: 'NONE',
    TURN: 'TURN',
    MOVE: 'MOVE',
    KICK: 'KICK'
};

// Exports...
module.exports = PlayerState_Action;
