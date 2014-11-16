/**
 * GSM_Play
 * --------
 * Manages standard play - players moving, kicking etc.
 */
var GSM_Base = require('./GSM_Base');
var UtilsLib = require('../utils');
var CWCError = UtilsLib.CWCError;
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
GSM_Play.prototype = new GSM_Base(); // Derived from GSM_Base.
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
    try {
        // We check that we got a PLAY response...
        if(data.request !== 'PLAY') {
            throw new CWCError('Expected a PLAY response.')
        }
        // We got a PLAY response, so we pass it to the Team to process...
        team.processPlayResponse(data);
    } catch(ex) {
        // We log the error and report it back to the AI...
        team.getAI().sendError(ex.message);
    }
};




