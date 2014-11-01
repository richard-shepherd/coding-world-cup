/**
 * TeamState
 * ---------
 * Holds all the state for one team, in particular the state of
 * each player in the team.
 */
var Array = require('collections/shim_array');
var Player = require('./Player');

/**
 * @constructor
 */
function TeamState() {
    // The players...
    this.players = [];
    for(var i=0; i<TeamState.NUMBER_OF_PLAYERS; ++i) {
        this.players.push(new Player());
    }

    // The goalkeeper...
    this.goalkeeper = new Player();
}

// The number of players on each team (not including the goalkeeper)...
TeamState.NUMBER_OF_PLAYERS = 5;

// Exports...
module.exports = TeamState;