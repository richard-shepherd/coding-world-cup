/**
 * GameState
 * ---------
 * Information about the game, such as the current time and so on.
 *
 * Note: this data is just data at the game level. Other state
 *       (such as the teams and players) is held by the Game object.
 */

/**
 * @constructor
 */
function Game() {
    // The current time in the game...
    this.currentTimeSeconds = 0.0;
}

// Exports...
module.exports = Game;


