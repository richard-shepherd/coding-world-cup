/**
 * Team
 * ----
 * Manages one team, including the collection of players in it.
 */
var Array = require('collections/shim-array');
var Player = require('./Player');
var TeamState = require('./TeamState');

/**
 * @constructor
 */
function Team() {
    // The team state (score etc)...
    this.state = new TeamState();

    // The players...
    this.players = new Array();
    for(var i=0; i<Team.NUMBER_OF_PLAYERS; ++i) {
        this.players.push(new Player());
    }

    // The goalkeeper...
    this.goalkeeper = new Player();
}

// The number of players on each team (not including the goalkeeper)...
Team.NUMBER_OF_PLAYERS = 5;

// Exports...
module.exports = Team;