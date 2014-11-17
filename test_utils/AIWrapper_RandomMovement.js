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

    // The team number...
    this._teamNumber = -1;

    // The direction we're playing...
    this._direction = "";

    // The collection of players {playerNumber, playerType}...
    this._players = [];
}
AIWrapper_RandomMovement.prototype = new AIWrapper(); // Derived from AIWrapper.

/**
 * sendData
 * --------
 * This is called when the game sends data to the AI, so in this
 * dummy AI it is where we receive updates.
 */
AIWrapper_RandomMovement.prototype.sendData = function(jsonData) {
    // We process the data next time around the event loop as if
    // the AI was calling back asynchronously (and to make sure the
    // code is not re-entrant)...
    process.nextTick(function() {
        // We convert the JSON to an object, and call functions
        // depending on the message-type...
        var data = JSON.parse(jsonData);
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
};

