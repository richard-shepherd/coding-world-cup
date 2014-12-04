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
var child_process = require('child_process');


/**
 * @constructor
 */
function AIWrapper() {
    // The team number...
    this._teamNumber = -1;

    // The game state machine...
    this._gsmManager = null;

    // The AI process...
    this._aiProcess  = null;
}
module.exports = AIWrapper;

/**
 * dispose
 * -------
 */
AIWrapper.prototype.dispose = function() {
    // We kill the AI process...
    if(this._aiProcess !== null) {
        this._aiProcess.kill();
        this._aiProcess = null;
    }
};

/**
 * setTeamNumber
 * -------------
 */
AIWrapper.prototype.setTeamNumber = function(teamNumber) {
    this._teamNumber = teamNumber;
};

/**
 * wrap
 * ----
 * Launches the AI from the info provided.
 */
AIWrapper.prototype.wrap = function(aiInfo) {
    // We launch the AI process...
    this._aiProcess = child_process.spawn(aiInfo.executable, aiInfo.args, {cwd:aiInfo.absoluteFolder});

    // We hook up to stdout from the AI...
    var that = this;
    this._aiProcess.stdout.on('data', function(data) {
        var line = data.toString();
        that.onResponseReceived(line);
    });

    // And to stderr...
    this._aiProcess.stderr.on('data', function(data) {
        var line = data.toString();
        Logger.log(line, Logger.LogLevel.ERROR);
    });
};

/**
 * setGSMManager
 * -------------
 */
AIWrapper.prototype.setGSMManager = function(gsmManager) {
    this._gsmManager = gsmManager;
};

/**
 * sendError
 * ---------
 * Sends an error message to the AI.
 */
AIWrapper.prototype.sendError = function(message) {
    // We log the error...
    Logger.log(message, Logger.LogLevel.ERROR);

    // We create an error message to send to the AI...
    var errorMessage = {
        messageType: "ERROR",
        error: message
    };

    var jsonErrorMessage = JSON.stringify(errorMessage);
    this.sendData(jsonErrorMessage);
};

/**
 * sendData
 * --------
 * Sends the data passed in to the AI.
 */
AIWrapper.prototype.sendData = function(jsonData) {
    if(this._aiProcess !== null) {
        this._aiProcess.stdin.write(jsonData);
        this._aiProcess.stdin.write('\n');
    }
};

/**
 * onResponseReceived
 * ------------------
 * Called when we receive a response from the AI.
 * 'data' is an object, ie it has already been parsed from the JSON
 * string sent by the AI.
 */
AIWrapper.prototype.onResponseReceived = function (jsonData) {
    // TODO: Put the try...catch back
    //try {
        // We forward the response to the GSM...
        switch(this._teamNumber) {
            case 1:
                this._gsmManager.onResponse_AI1(jsonData);
                break;

            case 2:
                this._gsmManager.onResponse_AI2(jsonData);
                break;
        }
    //} catch(ex) {
    //    // Something went wrong processing the responses...
    //    this.sendError(ex.message);
    //}
};


