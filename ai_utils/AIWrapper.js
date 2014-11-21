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
function AIWrapper() {
    // The team number...
    this._teamNumber = -1;

    // The game state machine...
    this._gsmManager = null;
}
module.exports = AIWrapper;

/**
 * setGSMManager
 * -------------
 */
AIWrapper.prototype.setGSMManager = function(gsmManager) {
    this._gsmManager = gsmManager;
};

/**
 * sendRequest
 * -----------
 * Sends a request message to the AI we're managing.
 */
AIWrapper.prototype.sendRequest = function(request) {
    // We add the REQUEST type to the message...
    request.messageType = "REQUEST";
    this.sendData(request);
};

/**
 * sendEvent
 * ---------
 * Sends an event message to the AI we're managing.
 */
AIWrapper.prototype.sendEvent = function(event) {
    // We add the EVENT type to the message...
    event.messageType = "EVENT";
    this.sendData(event);
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
    // TODO: Write this! (Convert to JSON and send to AI.)
};

/**
 * onResponseReceived
 * ------------------
 * Called when we receive a response from the AI.
 * 'data' is an object, ie it has already been parsed from the JSON
 * string sent by the AI.
 */
AIWrapper.prototype.onResponseReceived = function(jsonData) {
    try {
        // We forward the response to the GSM...
        switch(this._teamNumber) {
            case 1:
                this._gsmManager.onResponse_AI1(jsonData);
                break;

            case 2:
                this._gsmManager.onResponse_AI2(jsonData);
                break;
        }
    } catch(ex) {
        // Something went wrong processing the responses...
        this.sendError(ex.message);
    }
};


