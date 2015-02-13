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
var readline = require('readline');


/**
 * @constructor
 */
function AIWrapper(name) {
    // The team's name...
    this.name = name;

    // The team number...
    this._teamNumber = -1;

    // The game state machine...
    this._gsmManager = null;

    // The AI process...
    this._aiProcess  = null;

    // Whether this object has been disposed...
    this._disposed = false;

    // The processing time taken...
    this.processingTimeSeconds = 0.0;
}
module.exports = AIWrapper;

/**
 * dispose
 * -------
 */
AIWrapper.prototype.dispose = function() {
    if(this._disposed) {
        return;
    }

    // We kill the AI process...
    if(this._aiProcess !== null) {
        this._aiProcess.kill();
        this._aiProcess = null;
    }

    this._disposed = true;
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

    // We listen for the process exiting unexpectedly...
    var that = this;
    this._aiProcess.on('exit', function(code, signal) {
        that._onExit();
    });

    // We hook up to stdout from the AI...
    this._io = readline.createInterface({
        input: this._aiProcess.stdout,
        output: this._aiProcess.stdin
    });
    var that = this;
    this._io.on('line', function(line){
        that.onResponseReceived(line);
    });

    // And to stderr...
    this._aiProcess.stderr.on('data', function(data) {
        var line = data.toString();
        Logger.log(line, Logger.LogLevel.ERROR);
    });
};

/**
 * onExit
 * ------
 * Called when the process exits.
 */
AIWrapper.prototype._onExit = function() {
    // If this object has been disposed, then the process exited as expected...
    if(this._disposed) {
        return;
    }

    // TODO The process exited unexpectedly...
    Logger.log(this.name +  ' EXITED UNEXPECTEDLY!', Logger.LogLevel.ERROR);
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
    try {
        // We log the error...
        Logger.log(message, Logger.LogLevel.ERROR);

        // We create an error message to send to the AI...
        var errorMessage = {
            messageType: "ERROR",
            error: message
        };

        var jsonErrorMessage = JSON.stringify(errorMessage);
        this.sendData(jsonErrorMessage);
    } catch(ex) {
        // The process may have exited unexpectedly...
        Logger.log(ex.message, Logger.LogLevel.ERROR);
        this._onExit();
    }
};

/**
 * sendData
 * --------
 * Sends the data passed in to the AI.
 */
AIWrapper.prototype.sendData = function(jsonData) {
    try {
        if(this._aiProcess !== null) {
            this._aiProcess.stdin.write(jsonData);
            this._aiProcess.stdin.write('\n');
        }
    } catch(ex) {
        // The process may have exited unexpectedly...
        Logger.log(ex.message, Logger.LogLevel.ERROR);
        this._onExit();
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


