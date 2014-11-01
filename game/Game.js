/**
 * Game
 * ----
 * Manages a game, including the two teams, their AIs and players.
 *
 * Calculation and AI-update intervals
 * -----------------------------------
 * These intervals are stored in seconds, though these are really
 * 'virtual' intervals in game-time, and may be run much faster than
 * real time.
 *
 * Calculations may be done more frequently than player-AI updates, to
 * make sure that no events are missed.
 */
var Team = require('./Team');
var Array = require('collections/shim-array');

/**
 * @constructor
 */
function Game() {
    // The two teams...
    this.teams = new Array();
    this.teams.push(new Team());
    this.teams.push(new Team());

    // The interval in seconds between calculation updates.
    // This includes the 'physics' of player and ball movement
    // as well as player interactions (such as tackling) and other
    // game events...
    this.calculationIntervalSeconds = 0.1;

    // The interval in seconds between updates / requests being
    // sent to the AIs...
    this.aiUpdateIntervalSeconds = 1.0;

    // The length of the game in seconds...
    this.gameLengthSeconds = 90.0 * 60.0;
}

/**
 * calculate
 * ---------
 * Calculates new positions of players and takes actions, based
 * on the current game time.
 */
Game.prototype.calculate = function() {

}

/**
 * updatePositions
 * ---------------
 * Updates the positions of the players, based on current game time
 * and the directions, speed and intentions of the players.
 */
Game.prototype.updatePositions = function() {

}

// Exports...
module.exports = Game;

