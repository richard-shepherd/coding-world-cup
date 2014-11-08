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
var Player = require('./Player');
var PlayerState_Static = require('./PlayerState_Static');
var Ball = require('./Ball');

/**
 * @constructor
 */
function Game() {
    // The ball...
    this._ball = new Ball();

    // We create the teams and the _players...
    this.createTeams();

    // The interval in seconds between calculation updates.
    // This includes the 'physics' of player and ball movement
    // as well as player interactions (such as tackling) and other
    // game events...
    this._calculationIntervalSeconds = 0.1;

    // The interval in seconds between updates / requests being
    // sent to the AIs...
    this._aiUpdateIntervalSeconds = 1.0;

    // The length of the game in seconds...
    this._gameLengthSeconds = 90.0 * 60.0;
}

/**
 * Creates the teams and the _players.
 */
Game.prototype.createTeams = function() {
    // We create the two teams...
    this._team1 = new Team();
    this._team2 = new Team();

    // We create the _players and assign them to the teams...
    this._players = [];
    var playerNumber = {value: 0};
    this.addPlayersToTeam(this._team1, playerNumber);
    this.addPlayersToTeam(this._team2, playerNumber);
};

/**
 * Adds _players and the goalkeeper to the team passed in.
 */
Game.prototype.addPlayersToTeam = function(team, playerNumber) {
    // We add the _players...
    for(var i=0; i<Team.NUMBER_OF_PLAYERS; ++i) {
        var player = new Player(playerNumber.value, PlayerState_Static.PlayerType.PLAYER);
        this._players.push(player);
        team.addPlayer(player);
        playerNumber.value++;
    }

    // And the goalkeeper...
    var player = new Player(playerNumber.value, PlayerState_Static.PlayerType.GOALKEEPER);
    this._players.push(player);
    team.addPlayer(player);
    playerNumber.value++;
};

/**
 * calculate
 * ---------
 * Calculates new positions of _players and takes actions, based
 * on the current game time.
 */
Game.prototype.calculate = function() {
    // To calculate the next game state, we:
    // 1. Move the ball.
    // 2. Turn and/or move players to their new positions.
    // 3. Perform actions (tackling, kicking).

    // 1. We move the ball...
    this._ball.updatePosition(this);

    // 2. We move the players...
    this._team1.updatePositions(this);
    this._team2.updatePositions(this);
};

/**
 * getCalculationIntervalSeconds
 * -----------------------------
 * Returns the time in (game) seconds since the previous calculation.
 */
Game.prototype.getCalculationIntervalSeconds = function() {
    // TODO: Write this properly!
    return 0.01;
};


// Exports...
module.exports = Game;

