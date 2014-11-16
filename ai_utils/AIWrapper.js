/**
 * AIWrapper
 * ---------
 * Helps manage an AI.
 *
 * Manages the link between the game, particularly the game state machine
 * (GSM) and the AIs themselves.
 */
var UtilsLib = require('../utils');
var Logger = UtilsLib.Logger;


/**
 * @constructor
 */
function AIWrapper(teamNumber, gsmManager) {
    // The team number...
    this._teamNumber = teamNumber;

    // The game state machine...
    this._gsmManager = gsmManager;
}
module.exports = AIWrapper;

/**
 * sendRequest
 * -----------
 * Sends a request to the AI we're managing.
 */
AIWrapper.prototype.sendRequest = function(request) {
    // We add the REQUEST type to the message...
    request.messageType = "REQUEST";
    this.sendData(request);
};

/**
 * sendError
 * ---------
 * Sends an error message to the AI.
 */
AIWrapper.prototype.sendError = function(message) {
    // We log the error...
    Logger.log(ex.message, Logger.LogLevel.ERROR);

    // We create an error message to send to the AI...
    var errorMessage = {
        messageType: "ERROR",
        error: ex.message
    };
    var errorMessageJSON = JSON.stringify(errorMessage);
    this.sendData(errorMessageJSON);
};

/**
 * sendData
 * --------
 * Sends the data passed in to the AI.
 * The data should be a JSON string.
 */
AIWrapper.prototype.sendData = function(data) {
    // TODO: Write this!
};

/**
 * onResponseReceived
 * ------------------
 * Called when we receive a response from the AI.
 */
AIWrapper.prototype.onResponseReceived = function(data) {
    try {
        // We forward the response to the GSM...
        switch(this._teamNumber) {
            case 1:
                this._gsmManager.onResponse_AI1(data);
                break;

            case 2:
                this._gsmManager.onResponse_AI2(data);
                break;
        }
    } catch(ex) {
        // Something went wrong processing the responses...
        this.sendError(ex.message);
    }
};


