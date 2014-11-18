/**
 * AIWrapper_RandomMovement
 * ------------------------
 * A class derived from AIWrapper that randomly moves the players
 * in the team it controls.
 *
 * This is derived from AIWrapper and overrides the sendData method.
 * This intercepts the data sent to the AI and processes it locally
 * instead of it being sent to an external AI.
 */
var AIUtilsLib = require('../ai_utils');
var AIWrapper = AIUtilsLib.AIWrapper;

/**
 * @constructor
 */
function AIWrapper_RandomMovement(teamNumber, gsmManager) {
    // We call the base class constructor...
    AIWrapper.call(this, teamNumber, gsmManager);

    // Data about the pitch (width, height, position of goals)...
    this._pitch = {width:0, height:0};

    // The team number...
    this._teamNumber = -1;

    // The direction we're playing...
    this._direction = "";

    // The collection of players {playerNumber, playerType}...
    this._players = [];

    // The last time (in game-time) that we changed the movement of
    // players. We only update every few seconds...
    this._lastTimeWeChangedMovements = 0.0;
    this._changeInterval = 10.0;
}
AIWrapper_RandomMovement.prototype = new AIWrapper(); // Derived from AIWrapper.

/**
 * sendData
 * --------
 * This is called when the game sends data to the AI, so in this
 * dummy AI it is where we receive updates.
 */
AIWrapper_RandomMovement.prototype.sendData = function(data) {
    // Note re. JSON
    // -------------
    // Normally AIs would receive data as JSON strings. Here, though,
    // we are intercepting the original data before it has been stringified
    // into JSON, so we do not have to convert it back to an object.

    // We process the data next time around the event loop as if
    // the AI was calling back asynchronously (and to make sure the
    // code is not re-entrant)...
    process.nextTick(function() {
        // We call functions depending on the message-type...
        var messageHandler = '_on' + data.messageType;
        this[messageHandler](data);
    });
};

/**
 * _onEVENT
 * --------
 * Called when we receive an event message.
 */
AIWrapper_RandomMovement.prototype._onEVENT = function(data) {
    // We call different functions depending on the event...
    var eventHandler = '_onEVENT_' + data.event;
    this[eventHandler](data);
};

/**
 * _onREQUEST
 * ----------
 * Called when we receive a request message.
 */
AIWrapper_RandomMovement.prototype._onREQUEST = function(data) {
    // We call different functions depending on the request...
    var requestHandler = '_onREQUEST_' + data.request;
    this[requestHandler](data);
};

/**
 * _onEVENT_GAME_START
 * -------------------
 * Called at the start of a game with general game info.
 */
AIWrapper_RandomMovement.prototype._onEVENT_GAME_START = function(data) {
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
AIWrapper_RandomMovement.prototype._onEVENT_TEAM_INFO = function(data) {
    // We store our team number, direction and player info...
    this._teamNumber = data.teamNumber;
    this._direction = data.direction;
    this._players = data.players;
};

/**
 * _onREQUEST_PLAY
 * ---------------
 * Called when we receive a request for a PLAY update, ie instructions
 * to move players, kick etc.
 */
AIWrapper_RandomMovement.prototype._onREQUEST_PLAY = function(data) {
    // We create an object for the reply...
    var reply = {};
    reply.request = "PLAY";
    reply.actions = [];

    // We only update player movements if some time has elapsed...
    var nextChange = this._lastTimeWeChangedMovements + this._changeInterval;
    if(data.game.currentTimeSeconds >= nextChange) {
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
        });

        // We store the current time...
        this._lastTimeWeChangedMovements = data.game.currentTimeSeconds;
    }

    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
    this.onResponseReceived(jsonReply);
};

