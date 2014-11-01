/**
 * PlayerState
 * -----------
 * Information about one player.
 *
 * This includes dynamic data such as the player's current position
 * and speed, as well as more static / config data such as the player's
 * skills and abilities.
 *
 * Most, but not all, of the data is serialized and passed to AIs
 * each turn of the game. Some data - in particular the player's
 * "intentions" are private, and are only available to the AI which
 * is controlling the player.
 */
var PlayerState_Dynamic = require('./PlayerState_Dynamic');

/**
 * @constructor
 */
function PlayerState() {
    // Dynamic state, such as position...
    this.dynamicState = new PlayerState_Dynamic();
}

// Exports...
module.exports = PlayerState;


