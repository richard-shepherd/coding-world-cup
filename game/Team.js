/**
 * Team
 * ----
 * Manages one team, including the collection of players in it.
 */
var TeamState = require('./TeamState');

/**
 * @constructor
 */
function Team() {
    // The team state (score etc)...
    this.state = new TeamState();

    // The collection of players.
    // (The player objects are created in the Game, and passed to us later)...
    this.players = [];
}

/**
 * Adds a player to this team.
 */
Team.prototype.addPlayer = function(player) {
    this.players.push(player);
};

// The number of players on each team (not including the goalkeeper)...
Team.NUMBER_OF_PLAYERS = 5;

// Exports...
module.exports = Team;