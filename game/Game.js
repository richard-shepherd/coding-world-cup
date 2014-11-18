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
var GameState = require('./GameState');
var Pitch = require('./Pitch');


/**
 * @constructor
 */
function Game(ai1, ai2) {
    // The game state...
    this._state = new GameState();

    // The ball...
    this._ball = new Ball();

    // We create the teams and the _players...
    this.createTeams(ai1, ai2);

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

    // We send some events to the AIs at the start of the game...
    this._sendEvent_GameStart();
    this._sendEvent_TeamInfo();
}

/**
 * Creates the teams and the _players.
 */
Game.prototype.createTeams = function(ai1, ai2) {
    // We create the two teams...
    this._team1 = new Team(ai1, 1);
    this._team2 = new Team(ai2, 2);

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

/**
 * getDTO
 * --------------
 * Gets the DTO which we pass to the GUI.
 */
Game.prototype.getDTO = function(publicOnly) {
    var DTO = {
        game: this._state,
        ball: this._ball._state,
        team1: this._team1.getDTO(publicOnly),
        team2: this._team2.getDTO(publicOnly)
    };
    return DTO;
};

/**
 * getTeam1
 * --------
 */
Game.prototype.getTeam1 = function() {
    return this._team1;
};

/**
 * getTeam2
 * --------
 */
Game.prototype.getTeam2 = function() {
    return this._team2;
};

/**
 * _sendEvent_GameStart
 * --------------------
 * Sends the game-start event to both AIs.
 */
Game.prototype._sendEvent_GameStart = function() {
    var info = {
        event:"GAME_START",
        pitch: {
            width:Pitch.WIDTH,
            height:Pitch.HEIGHT,
            goalY1:Pitch.GOAL_Y1,
            goalY2:Pitch.GOAL_Y2
        }
    };
    this._team1.getAI().sendEvent(info);
    this._team2.getAI().sendEvent(info);
};

/**
 * _sendEvent_TeamInfo
 * -------------------
 * Sends team-info to the AIs. Each AI gets info about their own team.
 */
Game.prototype._sendEvent_TeamInfo = function() {
    this._team1.sendEvent_TeamInfo();
    this._team2.sendEvent_TeamInfo();
};


// Exports...
module.exports = Game;

