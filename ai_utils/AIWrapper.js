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
 * sendUpdate
 * ----------
 * Sends an to the AI we're managing.
 */
AIWrapper.prototype.sendUpdate = function(data) {

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
        // Something went wrong with the parsing or processing
        // of the response. We log the error and send the error
        // message to the AI...
        Logger.log(ex.message, Logger.LogLevel.ERROR);

        var errorMessage = {
            messageType: "ERROR",
            error: ex.message
        };
        var errorMessageJSON = JSON.stringify(errorMessage);
        this.sendUpdate(errorMessageJSON);
    }
};


