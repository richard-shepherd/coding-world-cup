/**
 * Player
 * ------
 * Information about one player and methods to control them.
 *
 * The information includes dynamic data such as the player's current
 * position and speed, as well as more static / config data such as
 * the player's skills and abilities.
 *
 * Most, but not all, of the data is serialized and passed to AIs
 * each turn of the game. Some data - in particular the player's
 * "intentions" are private, and are only available to the AI which
 * is controlling the player.
 */
var PlayerState_Dynamic = require('./PlayerState_Dynamic');
var PlayerState_Static = require('./PlayerState_Static');
var PlayerState_Intentions = require('./PlayerState_Intentions');

/**
 * @constructor
 */
function Player(playerNumber, playerType) {
    // Dynamic state (position etc)...
    this.dynamicState = new PlayerState_Dynamic();

    // Static state (skills, abilities etc)...
    this.staticState = new PlayerState_Static(playerNumber, playerType);

    // Intentions (direction, speed etc)...
    this.intentionsState = new PlayerState_Intentions();
}

/**
 * isPlayer
 * --------
 * Returns true if this player is a player (ie, not a goalkeeper).
 */
Player.prototype.isPlayer = function() {
    return this.staticState.playerType === PlayerState_Static.PlayerType.PLAYER;
};

/**
 * isGoalkeeper
 * ------------
 * Returns true if this player is a goalkeeper.
 */
Player.prototype.isGoalkeeper = function() {
    return this.staticState.playerType === PlayerState_Static.PlayerType.GOALKEEPER;
};

// Exports...
module.exports = Player;

