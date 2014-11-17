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
    AIWrapper.call(this, teamNumber, gsmManager);
}
AIWrapper_RandomMovement.prototype = new AIWrapper(); // Derived from AIWrapper.

/**
 * sendData
 * --------
 * This is called when the game sends data to the AI, so in this
 * dummy AI it is where we receive updates.
 */
AIWrapper_RandomMovement.prototype.sendData = function(data) {
    // We call functions depending on the message-type...
    var messageHandler = '_onMessage_' + data.messageType;
    this[messageHandler](data);
};

/**
 * _onMessage_REQUEST
 * ------------------
 * Called when we receive a
 * @param data
 * @private
 */
AIWrapper_RandomMovement.prototype._onMessage_REQUEST = function(data) {

};
