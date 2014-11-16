/**
 * GSM_Play
 * --------
 * Manages standard play - players moving, kicking etc.
 */
var GSM_Base = require('./GSM_Base');
var UtilsLib = require('../utils');
var Logger = UtilsLib.Logger;
var util = require('util');


/**
 * @constructor
 */
function GSM_Play(game) {
    // We call the base class constructor...
    GSM_Base.call(this, game);

    // We send the current state to the AIs, and wait for responses...
    this.sendPlayUpdateToBothAIs();
}
GSM_Play.prototype = new GSM_Base(); // Inherits from GSM_Base...
module.exports = GSM_Play;

/**
 * onAIResponsesReceived
 * ---------------------
 * Called when we have received responses from both AIs.
 */
GSM_Play.prototype.onAIResponsesReceived = function() {
    // We process the two responses...
    this._processResponse(this._aiResponses.AI1.data, this._team1);
    this._processResponse(this._aiResponses.AI2.data, this._team2);
};

/**
 * _processResponse
 * ----------------
 * Checks that the response is the right type, and passes it to the
 * team to process.
 */
GSM_Play.prototype._processResponse = function(data, team) {
    if(data.request === 'PLAY') {
        // We got a PLAY response, so we pass it to the Team to process...
        team.processPlayResponse(data);
    } else {
        // We got an unexpected response...
        Logger.log(util.format('Unexpected response from team: %s, data:%s', team.getName(), data), Logger.LogLevel.ERROR);
    }
};




