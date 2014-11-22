/**
 * Team
 * ----
 * Manages one team, including the collection of players in it.
 */
var TeamState = require('./TeamState');
var UtilsLib = require('../utils');
var CWCError = UtilsLib.CWCError;


/**
 * @constructor
 */
function Team(ai, teamNumber) {
    // The team's number (1 or 2)...
    this._teamNumber = teamNumber;

    // The AIWrapper for the AI managing this team...
    this._ai = ai;

    // The team state (score etc)...
    this._state = new TeamState();

    // The collection of _players.
    // (The player objects are created in the Game, and passed to us later)...
    this._players = [];
}

// The number of _players on each team (not including the goalkeeper)...
Team.NUMBER_OF_PLAYERS = 5;

/**
 * Adds a player to this team.
 */
Team.prototype.addPlayer = function(player) {
    this._players.push(player);
};

/**
 * Updates the positions of the _players using the current time
 * from the game.
 */
Team.prototype.updatePositions = function(game) {
    this._players.forEach(function(player){
        player.updatePosition(game);
    }, this);
};

/**
 * getDTO
 * --------------
 * Returns the state of this object and its players.
 *
 * If publicOnly is true, then only public (dynamic) jsonData about
 * the players will be included.
 */
Team.prototype.getDTO = function(publicOnly) {
    var state = {};
    state.team = this._state;
    state.players = this._players.map(function(player) {
        return player.getDTO(publicOnly);
    });
    return state;
};

/**
 * Returns the AI (wrapper) for this team.
 */
Team.prototype.getAI = function() {
    return this._ai;
};

/**
 * getName
 * -------
 * Helper function to get the team's name.
 */
Team.prototype.getName = function() {
    return this._state.name;
};

/**
 * processPlayResponse
 * -------------------
 * Processes a response from an AI for general 'play' updates
 * involving moving of players, kicking and so on.
 */
Team.prototype.processPlayResponse = function(data) {
    // The data should have an 'actions' section...
    if(!('actions' in data)) {
        throw new CWCError('Expected an "actions" array in response.');
    }

    // We process each action...
    data.actions.forEach(function(action) {
        var player = this._getPlayer(action.playerNumber);
        player.setAction(action);
    }, this);
};

/**
 * _getPlayer
 * ----------
 * Returns the Player object for the player-number passed in.
 * Throws an exception if the player is not part of this team.
 */
Team.prototype._getPlayer = function(playerNumber) {
    // The player numbers are consecutive, so we can do a bit
    // of index arithmetic to find the player for the number
    // passed in...
    var players = this._players;
    if(players.length === 0) {
        throw new CWCError('No players in team.');
    }

    var firstPlayerNumber = players[0].getPlayerNumber();
    var index = playerNumber - firstPlayerNumber;
    if(index >= players.length) {
        throw new CWCError('Player not found: ' + playerNumber);
    }
    return players[index];
};

/**
 * sendEvent_TeamInfo
 * ------------------
 * Sends information about this team to the team's AI.
 */
Team.prototype.sendEvent_TeamInfo = function() {
    // We create the info for the event...
    var info = {
        event:"TEAM_INFO",
        teamNumber:this._teamNumber,
        players:[]
    };

    // Add the players...
    this._players.forEach(function(player) {
        var playerInfo = {
            playerNumber:player.staticState.playerNumber,
            playerType:player.staticState.playerType
        };
        info.players.push(player);
    }, this);

    // And send it to the team's AI...
    this._ai.sendEvent(info);
};

// Exports...
module.exports = Team;