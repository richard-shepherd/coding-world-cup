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
    this._dynamicState = new PlayerState_Dynamic();

    // Static state (skills, abilities etc)...
    this._staticState = new PlayerState_Static(playerNumber, playerType);

    // Intentions (direction, speed etc)...
    this._intentionsState = new PlayerState_Intentions();
}

/**
 * Maximum running speed, in metres/second.
 * If a player has runningAbility of 100.0 and chooses to run at
 * 100% speed, they will run at this rate.
 */
Player.MAX_SPEED = 10.0;

/**
 * The maximum energy that any player can have. All players start with
 * this energy. (Though players recuperate at different rates depending
 * on their stamina.)
 */
Player.MAX_ENERGY = 100.0;

/**
 * isPlayer
 * --------
 * Returns true if this player is a player (ie, not a goalkeeper).
 */
Player.prototype.isPlayer = function() {
    return this._staticState.playerType === PlayerState_Static.PlayerType.PLAYER;
};

/**
 * isGoalkeeper
 * ------------
 * Returns true if this player is a goalkeeper.
 */
Player.prototype.isGoalkeeper = function() {
    return this._staticState.playerType === PlayerState_Static.PlayerType.GOALKEEPER;
};

/**
 * updatePosition
 * --------------
 * Moves the player based on their current intentions, speed and so on, and
 * on the time elapsed since the previous update.
 */
Player.prototype.updatePosition = function(game) {

};

// Exports...
module.exports = Player;

