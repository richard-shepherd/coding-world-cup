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
var util = require('util');
var Team = require('./Team');
var TeamState = require('./TeamState');
var Player = require('./Player');
var PlayerState_Static = require('./PlayerState_Static');
var Ball = require('./Ball');
var GameState = require('./GameState');
var Pitch = require('./Pitch');
var GSM_Manager = require('./GSM_Manager');
var GSM_ConfigureAbilities = require('./GSM_ConfigureAbilities');
var GSM_Kickoff = require('./GSM_Kickoff');
var UtilsLib = require('../utils');
var Logger = UtilsLib.Logger;
var Utils = UtilsLib.Utils;
var NanoTimer = require('nanotimer');
var Random = UtilsLib.Random;
var AIUtilsLib = require('../ai_utils');
var MessageUtils = AIUtilsLib.MessageUtils;


/**
 * @constructor
 */
function Game(ai1, ai2) {
    // The AI-wrappers...
    this._ai1 = ai1;
    this._ai2 = ai2;

    // The GUIWebSocket, to send updates to the GUI...
    this.guiWebSocket = null;

    // If we are in simulation mode, we run the game loop as a
    // tight(ish) loop. If it is false, we use a timer so the game
    // runs more in real time...
    this.simulationMode = false;

}

/**
 * play
 * ----
 * Starts a new game.
 */
Game.prototype.play = function() {
    // A timer for use with the game loop...
    this._timer = new NanoTimer();

    // Used with decisions that have a random element...
    this._random = new Random();

    // The game state...
    this.state = new GameState();

    // Holds the dimensions of the pitch...
    this.pitch = new Pitch();

    // The ball...
    this.ball = new Ball();

    // The interval in seconds between calculation updates.
    // This includes the 'physics' of player and ball movement
    // as well as player interactions (such as tackling) and other
    // game events...
    this._calculationIntervalSeconds = 0.01;

    // The interval in seconds between updates / requests being
    // sent to the AIs...
    this._aiUpdateIntervalSeconds = 0.1;

    // The length of the game in seconds...
    this._gameLengthSeconds = 30.0 * 60.0;
    this._halfTimeSeconds = this._gameLengthSeconds / 2.0;

    // The maximum total ability in each category...
    this.maxTotalAbility = 400;

    // The time from the previous time we processes the game-loop.
    // This is used to determine when half-time has occurred.
    this._previousCalculationTimeSeconds = 0.0;

    // We create the teams and the _players...
    this.createTeams(this._ai1, this._ai2);

    // The game-state-machine (GSM) that manages game events and transitions.
    // Note: This has to be done after creating the teams.
    this._gsmManager = new GSM_Manager();
    this._ai1.setGSMManager(this._gsmManager);
    this._ai2.setGSMManager(this._gsmManager);

    // We send some events to the AIs at the start of the game...
    this.sendEvent_GameStart();
    this._sendEvent_TeamInfo();

    // We set the initial game state...
    this._gsmManager.setState(new GSM_ConfigureAbilities(this));

    // We start to play...
    this.onTurn();
};

/**
 * createTeams
 * -----------
 * Creates the teams and the players.
 */
Game.prototype.createTeams = function(ai1, ai2) {
    // We create the two teams...
    this._team1 = new Team(ai1, 1);
    this._team2 = new Team(ai2, 2);
    ai1.setTeamNumber(1);
    ai2.setTeamNumber(2);

    // We set the direction they are playing...
    this._team1.setDirection(TeamState.Direction.RIGHT);
    this._team2.setDirection(TeamState.Direction.LEFT);

    // We create the _players and assign them to the teams...
    this._players = [];
    var playerNumber = {value: 0};
    this.addPlayersToTeam(this._team1, playerNumber);
    this.addPlayersToTeam(this._team2, playerNumber);
};

/**
 * addPlayersToTeam
 * ----------------
 * Adds players and the goalkeeper to the team passed in.
 */
Game.prototype.addPlayersToTeam = function(team, playerNumber) {
    // We add the _players...
    for(var i=0; i<Team.NUMBER_OF_PLAYERS; ++i) {
        var player = new Player(playerNumber.value, PlayerState_Static.PlayerType.PLAYER, team);
        this._players.push(player);
        team.addPlayer(player);
        playerNumber.value++;
    }

    // And the goalkeeper...
    player = new Player(playerNumber.value, PlayerState_Static.PlayerType.GOALKEEPER, team);
    this._players.push(player);
    team.addPlayer(player);
    playerNumber.value++;
};

/**
 * getPlayer
 * ---------
 * Returns the player with the number passed in.
 */
Game.prototype.getPlayer = function(playerNumber) {
    return this._players[playerNumber];
};

/**
 * onTurn
 * ------
 * This is the main function of the "game loop", and is called for
 * each turn or time-slice of the game.
 */
Game.prototype.onTurn = function() {
    // We log the game time...
    var message = util.format('Time: %d\tscore: %d:%d',
        this.state.currentTimeSeconds.toFixed(4),
        this._team1.state.score,
        this._team2.state.score);
    Logger.log(message, Logger.LogLevel.INFO_PLUS);
    Logger.indent();

    // We update the game state - kicking, moving the ball and players etc...
    this.calculate();

    // We check game events - goals, end-of-half etc...
    this._checkGameEvents();

    // Now that we've updated positions and events, we see if this
    // has changed the game state...
    this._gsmManager.checkState();

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
    Logger.dedent();

    // Has the game ended?
    if(this.state.currentTimeSeconds >= this._gameLengthSeconds) {
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
    var calculationIntervalSeconds = this.getCalculationIntervalSeconds();
    while(calculationTime < this._aiUpdateIntervalSeconds-0.000001) {
        // We update the game time and calculation time...
        this.state.currentTimeSeconds += calculationIntervalSeconds;
        calculationTime += calculationIntervalSeconds;

        // We see if any players can take possession of the ball or tackle...
        if(this.ball.state.controllingPlayerNumber === -1) {
            // No-one has the ball, so players can try to take possession...
            this.calculate_takePossession();
        } else {
            // Someone has the ball, so players can try to tackle...
            this.calculate_tackle();
        }

        // We move the players...
        this._team1.processActions(this);
        this._team2.processActions(this);

        // We move the ball...
        this.ball.updatePosition(this);

        // We check for game events...
        this._checkGameEvents();
    }
};

/**
 * calculate_takePossession
 * ------------------------
 * Checks if any of the players can take possession of the ball.
 *
 * If more than one wants to take possession, we have to decide
 * between them.
 */
Game.prototype.calculate_takePossession = function() {
    // We look through the players to find whether they want to take
    // possession, and their probability of doing so...
    var players = [];
    this._players.forEach(function(player) {
        // We get the probability of taking possession...
        var probability = player.getProbabilityOfTakingPossession(this);

        // And see if they actually could get it...
        var random = this._random.nextDouble();
        if(random <= probability) {
            players.push(player);
        }
    }, this);

    // Are there any players who can take possession?
    if(players.length === 0) {
        return;
    }

    // We've now got a collection of players who could take possession
    // of the ball, so we pick one who actually gets it...
    var index = Math.floor(this._random.nextDouble() * players.length);
    var playerWhoGetsBall = players[index];
    this.giveBallToPlayer(playerWhoGetsBall);
    playerWhoGetsBall.clearAction();

    // We stop further players taking possession this turn...
    this._clearTakePossessionActions();
};

/**
 * calculate_tackle
 * ----------------
 * Checks if any player can tackle the player with the ball.
 */
Game.prototype.calculate_tackle = function() {
    // We look through the players to find whether they want to tackle,
    // and their probability of successfully doing so...
    var playerWithBall = this.getPlayer(this.ball.state.controllingPlayerNumber);
    var players = [];
    this._players.forEach(function(player) {
        // We get the probability of successfully tackling...
        var probability = player.getProbabilityOfSuccessfulTackle(this, playerWithBall);

        // And see if they actually could get it...
        var random = this._random.nextDouble();
        if(random <= probability) {
            players.push(player);
        }
    }, this);

    // Are there any players who can successfully tackle?
    if(players.length === 0) {
        return;
    }

    // We've now got a collection of players who could successfully tackle,
    // so we pick one who actually gets it...
    var index = Math.floor(this._random.nextDouble() * players.length);
    var playerWhoGetsBall = players[index];
    this.giveBallToPlayer(playerWhoGetsBall);
    playerWhoGetsBall.clearAction();

    // We stop further players taking possession this turn...
    this._clearTakePossessionActions();
};

/**
 * _clearTakePossessionActions
 * ---------------------------
 * Clears the TAKE_POSSESSION action from all players.
 */
Game.prototype._clearTakePossessionActions = function() {
    this._players.forEach(function(player) {
        player.clearTakePossessionAction();
    });
};

/**
 * checkForGoal
 * ------------
 * Called when the ball has bounced off one of the goal lines. We check if
 * a goal has been scored.
 *
 * 'goalLine' is the x-value of the goal line that the ball bounced off.
 */
Game.prototype.checkForGoal = function(position1, position2, goalLine) {
    // We find the crossing point (y-value) where the ball bounced...
    var pitch = this.pitch;
    var crossingPoint = Utils.crossingPoint(position1, position2, goalLine);
    if(crossingPoint < pitch.goalY1 || crossingPoint > pitch.goalY2) {
        // The crossing point was outside the goalposts...
        return;
    }

    // A goal was scored. We need to work out which team scored it...
    var team1Direction = this._team1.state.direction;
    var team2Direction = this._team2.state.direction;
    var scoringTeam = null;
    if(goalLine === 0.0 && team1Direction === TeamState.Direction.LEFT) {
        scoringTeam = this._team1;
    } else
    if(goalLine === 0.0 && team2Direction === TeamState.Direction.LEFT) {
        scoringTeam = this._team2;
    } else
    if(goalLine === pitch.width && team1Direction === TeamState.Direction.RIGHT) {
        scoringTeam = this._team1;
    } else
    if(goalLine === pitch.width && team2Direction === TeamState.Direction.RIGHT) {
        scoringTeam = this._team2;
    }

    // We update the score, send an event, and start the kickoff...
    scoringTeam.state.score++;
    var teamToKickOff = (scoringTeam === this._team1) ? this._team2 : this._team1;
    this.sendEvent_Goal();
    this._gsmManager.setState(new GSM_Kickoff(this, teamToKickOff))
};

/**
 * _checkGameEvents
 * ----------------
 * Checks game events such as end of half-time and so on,
 * and updates the game-state accordingly.
 */
Game.prototype._checkGameEvents = function() {
    // We check for half-time...
    if(this._previousCalculationTimeSeconds < this._halfTimeSeconds && this.state.currentTimeSeconds >= this._halfTimeSeconds) {
        // We tell the AIs that it's half time...
        this.sendEvent_HalfTime();

        // TODO: If we have energy, players should recuperate at half-time.
        // Team 2 kicks off...
        this._team1.setDirection(TeamState.Direction.LEFT);
        this._team2.setDirection(TeamState.Direction.RIGHT);
        this._gsmManager.setState(new GSM_Kickoff(this, this._team2));
    }
    this._previousCalculationTimeSeconds = this.state.currentTimeSeconds;
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
        game: this.state,
        ball: this.ball.state,
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
        player.staticState.kickingAbility = 100.0;
        player.staticState.ballControlAbility = 100.0;
        player.staticState.tacklingAbility = 100.0;
        player.dynamicState.energy = 100.0;
    });
};

/**
 * sendEvent
 * ---------
 * Sends the event to both AIs and to the GUI.
 */
Game.prototype.sendEvent = function(event) {
    // We get the JSON version of the event...
    var jsonEvent = MessageUtils.getEventJSON(event);

    // We send the event to both AIs...
    this._team1.getAI().sendData(jsonEvent);
    this._team2.getAI().sendData(jsonEvent);

    // And to the GUI...
    if(this.guiWebSocket !== null) {
        this.guiWebSocket.broadcast(jsonEvent);
    }

    Logger.log('SENT EVENT: ' + jsonEvent, Logger.LogLevel.DEBUG);
};

/**
 * sendEvent_GameStart
 * -------------------
 * Sends the game-start event to both AIs.
 */
Game.prototype.sendEvent_GameStart = function() {
    var event = {
        eventType:"GAME_START",
        pitch: this.pitch,
        gameLengthSeconds: this._gameLengthSeconds
    };
    this.sendEvent(event);
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
 * sendEvent_StartOfTurn
 * ---------------------
 * Sends the game-state to the AIs.
 */
Game.prototype.sendEvent_StartOfTurn = function() {
    // We get the DTO and pass it to the AIs...
    var event = this.getDTO(true);
    event.eventType = "START_OF_TURN";
    this.sendEvent(event);
};

/**
 * sendEvent_Goal
 * --------------
 */
Game.prototype.sendEvent_Goal = function() {
    var event = {
        eventType:"GOAL",
        team1:this._team1.state,
        team2:this._team2.state
    };
    this.sendEvent(event);
};

/**
 * sendEvent_HalfTime
 * ------------------
 */
Game.prototype.sendEvent_HalfTime = function() {
    this.sendEvent({eventType:"HALF_TIME"});
};

/**
 * giveBallToPlayer
 * ----------------
 * Sets data in the player and game to give the player the ball.
 */
Game.prototype.giveBallToPlayer = function(player) {
    var playerDynamicState = player.dynamicState;
    var ballState = this.ball.state;

    // We give the player the ball...
    playerDynamicState.hasBall = true;

    // We tell the ball that it is owned by the player...
    ballState.controllingPlayerNumber = player.getPlayerNumber();
    ballState.position.copyFrom(playerDynamicState.position);
};

/**
 * clearAllActions
 * ---------------
 * Sets the action for all players to NONE.
 */
Game.prototype.clearAllActions = function() {
    this._players.forEach(function(player) {
        player.clearAction();
    });
};

/**
 * setDefaultKickoffPositions
 * --------------------------
 */
Game.prototype.setDefaultKickoffPositions = function() {
    this.ball.setPosition(this.pitch.centreSpot);
    this._team1.setDefaultKickoffPositions(this.pitch);
    this._team2.setDefaultKickoffPositions(this.pitch);
};

/**
 * setGameState
 * ------------
 * Sets the (GSM) game state.
 */
Game.prototype.setGameState = function(gameState) {
    this._gsmManager.setState(gameState);
};

/**
 * setSimulationMode
 * -----------------
 * If simulationMode is true, the game runs as fast as possible.
 * If false, it runs closer to real-time.
 */
Game.prototype.setSimulationMode = function(simulationMode) {
    this.simulationMode = simulationMode;
};

// Exports...
module.exports = Game;

