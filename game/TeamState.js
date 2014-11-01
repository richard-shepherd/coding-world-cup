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
    // The goals scored by this team...
    this.score = 0;
}

// Exports...
module.exports = TeamState;


