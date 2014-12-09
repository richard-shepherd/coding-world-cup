/**
 * RandomMovement
 * --------------
 * Moves the players randomly.
 */
var readline = require('readline');
var UtilsLib = require('../../utils');
var Logger = UtilsLib.Logger;
var LogHandler_File = UtilsLib.LogHandler_File;

// We set up logging...
Logger.addHandler(new LogHandler_File('./log/RandomMovement.log', Logger.LogLevel.DEBUG));

// We create the AI...
var ai = new RandomMovement();

/**
 * @constructor
 */
function RandomMovement() {
    // We hook up stdin and stdout, and register onGameData
    // to be called when data is received over stdin...
    this._io = readline.createInterface({
        input: process.stdin,
        output: process.stdout
    });
    var that = this;
    this._io.on('line', function(line){
        that.onGameData(line);
    });

    // Data about the pitch (width, height, position of goals)...
    this._pitch = {width:0, height:0};

    // The direction we're playing...
    this._direction = "";

    // The collection of players {playerNumber, playerType}...
    this._players = [];

    // The game state for the current turn...
    this._gameState = {};

    // Kickoff info, including the direction we're playing...
    this._kickoffInfo = {};

    // The last time (in game-time) that we changed the movement of
    // players. We only update every few seconds...
    this._lastTimeWeChangedMovements = 0.0;
    this._changeInterval = 10.0;
}

/**
 * onGameData
 * ----------
 * Called when the game sends data to us.
 */
RandomMovement.prototype.onGameData = function(jsonData) {
    // We get the object...
    var data = JSON.parse(jsonData);

    // And call a function to handle it...
    var messageHandler = '_on' + data.messageType;
    this[messageHandler](data);
};

/**
 * _onEVENT
 * --------
 * Called when we receive an event message.
 */
RandomMovement.prototype._onEVENT = function(data) {
    // We call different functions depending on the event...
    var eventHandler = '_onEVENT_' + data.eventType;
    this[eventHandler](data);
};

/**
 * _onREQUEST
 * ----------
 * Called when we receive a request message.
 */
RandomMovement.prototype._onREQUEST = function(data) {
    // We call different functions depending on the request...
    var requestHandler = '_onREQUEST_' + data.requestType;
    this[requestHandler](data);
};

/**
 * _onEVENT_GAME_START
 * -------------------
 * Called at the start of a game with general game info.
 */
RandomMovement.prototype._onEVENT_GAME_START = function(data) {
    // We reset the last update time...
    this._lastTimeWeChangedMovements = 0.0;

    // We get data about the pitch...
    this._pitch = data.pitch;
};

/**
 * _onEVENT_TEAM_INFO
 * ------------------
 * Called when we receive the TEAM_INFO event.
 */
RandomMovement.prototype._onEVENT_TEAM_INFO = function(data) {
    // We store our team number, direction and player info...
    this._teamNumber = data.teamNumber;
    this._direction = data.direction;
    this._players = data.players;
};

/**
 * _onEVENT_START_OF_TURN
 * ----------------------
 * Called at the start of turn with the current game state.
 */
RandomMovement.prototype._onEVENT_START_OF_TURN = function(data) {
    // We store the current game state, to use later when we get requests...
    this._gameState = data.game;
};

/**
 * _onEVENT_KICKOFF
 * ----------------
 * Called when we get the KICKOFF event.
 */
RandomMovement.prototype._onEVENT_KICKOFF = function(data) {
    this._kickoffInfo = data;
};

/**
 * _onEVENT_GOAL
 * -------------
 * Called when we get the GOAL event.
 */
RandomMovement.prototype._onEVENT_GOAL = function(data) {
};

/**
 * _onEVENT_HALF_TIME
 * ------------------
 * Called when we get the HALF_TIME event.
 */
RandomMovement.prototype._onEVENT_HALF_TIME = function(data) {
};

/**
 * _onREQUEST_CONFIGURE_ABILITIES
 * ------------------------------
 * Called when we receive the request to configure abilities for our players.
 */
RandomMovement.prototype._onREQUEST_CONFIGURE_ABILITIES = function(data) {
    var reply = {};
    reply.requestType = "CONFIGURE_ABILITIES";
    reply.players = [];

    // We try to give each player 75% ability in each category, regardless
    // of the max ability specified in the request. The game may not give us this...
    this._players.forEach(function(player) {
        var info = {
            playerNumber:player.playerNumber,
            kickingAbility:75,
            runningAbility:75,
            ballControlAbility:75,
            tacklingAbility:75
        };
        reply.players.push(info);
    });

    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
    console.log(jsonReply);
};

/**
 * _onREQUEST_KICKOFF
 * ------------------
 * Called when we receive a request for player positions at kickoff.
 */
RandomMovement.prototype._onREQUEST_KICKOFF = function(data) {
    // We return an empty collection of positions, so we get the defaults...
    var reply = {};
    reply.requestType = "KICKOFF";
    reply.players = [];

    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
    console.log(jsonReply);
};

/**
 * _onREQUEST_PLAY
 * ---------------
 * Called when we receive a request for a PLAY update, ie instructions
 * to move players, kick etc.
 */
RandomMovement.prototype._onREQUEST_PLAY = function(data) {
    // We create an object for the reply...
    var reply = {};
    reply.requestType = "PLAY";
    reply.actions = [];

    // We only update player movements if some time has elapsed...
    var nextChange = this._lastTimeWeChangedMovements + this._changeInterval;
    if(this._gameState.currentTimeSeconds >= nextChange) {
        // We change the movements of our players.
        // For each player, we choose a random place on the pitch
        // for them to move towards...
        this._players.forEach(function(player) {
            var action = {};
            action.playerNumber = player.playerNumber;
            action.action = "MOVE";
            action.destination = {x:Math.random() * this._pitch.width, y:Math.random() * this._pitch.height};
            action.speed = 100.0;
            reply.actions.push(action);
        }, this);

        // We store the current time...
        this._lastTimeWeChangedMovements = this._gameState.currentTimeSeconds;
    }

    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
    console.log(jsonReply);
};



