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
var GSM_Manager = require('./GSM_Manager');
var GSM_Play = require('./GSM_Play');
var UtilsLib = require('../utils');
var Logger = UtilsLib.Logger;
var Utils = UtilsLib.Utils;
var NanoTimer = require('nanotimer');


/**
 * @constructor
 */
function Game(ai1, ai2, guiWebSocket) {
    // We store the GUIWebSocket, to send updates to the GUI...
    this._guiWebSocket = guiWebSocket;

    // A timer for use with the game loop...
    this._timer = new NanoTimer();
    
    // The game state...
    this._state = new GameState();

    // The ball...
    this._ball = new Ball();

    // We create the teams and the _players...
    this.createTeams(ai1, ai2);

    // The game-state-machine (GSM) that manages game events and transitions.
    // Note: This has to be done after creating the teams.
    this._gsmManager = new GSM_Manager();
    this._gsmManager.setState(new GSM_Play(this));
    ai1.setGSMManager(this._gsmManager);
    ai2.setGSMManager(this._gsmManager);

    // The interval in seconds between calculation updates.
    // This includes the 'physics' of player and ball movement
    // as well as player interactions (such as tackling) and other
    // game events...
    this._calculationIntervalSeconds = 0.01;

    // The interval in seconds between updates / requests being
    // sent to the AIs...
    this._aiUpdateIntervalSeconds = 0.0999999;

    // The length of the game in seconds...
    this._gameLengthSeconds = 30.0 * 60.0;

    // If we are in simulation mode, we run the game loop as a
    // tight(ish) loop. If it is false, we use a timer so the game
    // runs more in real time...
    this.simulationMode = false;

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
    player = new Player(playerNumber.value, PlayerState_Static.PlayerType.GOALKEEPER);
    this._players.push(player);
    team.addPlayer(player);
    playerNumber.value++;
};

/**
 * onTurn
 * ------
 * This is the main function of the "game loop", and is called for
 * each turn or time-slice of the game.
 */
Game.prototype.onTurn = function() {
    // We log the game time...
    Logger.log("Time (seconds): " + this._state.currentTimeSeconds.toFixed(4), Logger.LogLevel.INFO_PLUS);

    // We update the game state - kicking, moving the ball and players etc...
    this.calculate();

    // We check game events - goals, end-of-half etc...
    this._checkGameEvents();

    // Now that we've updated positions and events, we see if this
    // has changed the game state...
    this._gsmManager.checkState();

    // We send an update the the GUI...
    this._sendUpdateToGUI();

    // We send the start-of-turn event to the AIs. This includes
    // the game-state (player positions, ball position etc)...
    this._sendEvent_StartOfTurn();

    // We perform actions specific to the current state.
    // This includes sending requests to the AIs...
    this._gsmManager.onTurn();
};

/**
 * playNextTurn
 * ------------
 * Called (usually by one of the GSM states) when we can play the next turn.
 */
Game.prototype.playNextTurn = function () {
    // Has the game ended?
    if(this._state.currentTimeSeconds >= this._gameLengthSeconds) {
        Logger.log("Game over!", Logger.LogLevel.INFO);
        return;
    }

    var that = this;
    if(this.simulationMode) {
        // We are in simulation mde, so we play the next turn
        // as soon as possible...
        setImmediate(function() {
            that.onTurn();
        });
    } else {
        // We are in real-time mode, so we play the next turn after an interval...
        var timeout = this._aiUpdateIntervalSeconds + 's';
        this._timer.setTimeout(function () {
            that.onTurn();
        }, '', timeout);
    }
};

/**
 * _sendUpdateToGUI
 * ----------------
 * Sends an update of the current game state to the GUI.
 */
Game.prototype._sendUpdateToGUI = function() {
    if(this._guiWebSocket === null) {
        return;
    }

    // We get the DTO, and send it to the GUI...
    var dto = this.getDTO();
    var jsonDTO = JSON.stringify(dto, Utils.decimalPlaceReplacer(4));
    this._guiWebSocket.broadcast(jsonDTO);
};

/**
 * calculate
 * ---------
 * Calculates new positions of _players and takes actions, based
 * on the current game time.
 */
Game.prototype.calculate = function() {
    // To calculate the next game state, we:
    // - Perform actions (tackling, kicking).
    // - Move the ball.
    // - Turn and/or move players to their new positions.
    // - Check game event (goals, half-time etc)

    // We calculate multiple times to smooth out the movement, and
    // make it more likely for players to be able to tackle and take
    // possession of the ball...
    var calculationTime = 0.0;
    while(calculationTime < this._aiUpdateIntervalSeconds) {
        // We update the game time and calculation time...
        this._state.currentTimeSeconds += this._calculationIntervalSeconds;
        calculationTime += this._calculationIntervalSeconds;

        // We move the ball...
        this._ball.updatePosition(this);

        // We move the players...
        this._team1.updatePositions(this);
        this._team2.updatePositions(this);

        // We check for game events...
        this._checkGameEvents();
    }
};

/**
 * _checkGameEvents
 * ----------------
 * Checks game events such as goals being scored, end of half-time
 * and so on, and updates the game-state accordingly.
 */
Game.prototype._checkGameEvents = function() {
    // TODO: Write this!
};

/**
 * getCalculationIntervalSeconds
 * -----------------------------
 * Returns the time in (game) seconds since the previous calculation.
 */
Game.prototype.getCalculationIntervalSeconds = function() {
    return this._calculationIntervalSeconds;
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
 * giveAllPlayersMaxAbilities
 * --------------------------
 * Used when testing.
 */
Game.prototype.giveAllPlayersMaxAbilities = function() {
    this._players.forEach(function(player) {
        player.staticState.runningAbility = 100.0;
        player.staticState.passingAbility = 100.0;
        player.dynamicState.energy = 100.0;
    });
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

/**
 * _sendEvent_StartOfTurn
 * ----------------------
 * Sends the game-state to the AIs.
 */
Game.prototype._sendEvent_StartOfTurn = function() {
    // We get the DTO and pass it to the AIs...
    var info = this.getDTO(true);
    info.event = "START_OF_TURN";
    this._team1.getAI().sendEvent(info);
    this._team2.getAI().sendEvent(info);
};



// Exports...
module.exports = Game;

