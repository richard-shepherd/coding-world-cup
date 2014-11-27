/**
 * TeamState
 * ---------
 * Information related to the team. This data will be serialized
 * and sent to the AIs.
 *
 * Note: This is just data relating to the team level itself, and does
 *       not include the state of the players in the team. (This is held
 *       in the Team object.)
 */

/**
 * @constructor
 */
function TeamState() {
    // The team's name...
    this.name = '';

    // The goals scored by this team...
    this.score = 0;

    // The direction the team is playing...
    this.direction = TeamState.Direction.NONE;
}

/**
 * Direction
 * ---------
 * An enum for the direction the team is playing.
 */
TeamState.Direction = {
    // Default (invalid) value...
    NONE: 'NONE',

    // Shooting for the left-hand goal...
    LEFT: 'LEFT',

    // Shooting for the right-hand goal...
    RIGHT: 'RIGHT'
};


// Exports...
module.exports = TeamState;


