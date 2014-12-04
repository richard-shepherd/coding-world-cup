/**
 * GSM_Play
 * --------
 * Manages standard play - players moving, kicking etc.
 */
var GSM_Base = require('./GSM_Base');
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;
var CWCError = UtilsLib.CWCError;


/**
 * @constructor
 */
function GSM_Play(game) {
    // We call the base class constructor...
    GSM_Base.call(this, game);
}
Utils.extend(GSM_Base, GSM_Play); // Derived from GSM_Base.
module.exports = GSM_Play;

/**
 * checkState
 * ----------
 * Called by the game loop after updates have been made, to check
 * if we want to change the state.
 */
GSM_Play.prototype.checkState = function() {
    return this;
};

/**
 * onTurn
 * ------
 * Called by the game loop after the game-state update has been sent to
 * the AIs. We request PLAY updates from the AIs.
 */
GSM_Play.prototype.onTurn = function() {
    // We send the start-of-turn event to the AIs. This includes
    // the game-state (player positions, ball position etc)...
    this._game.sendEvent_StartOfTurn();

    // We request PLAY responses from the AIs...
    this.sendRequestToBothAIs({requestType:"PLAY"});
};

/**
 * onAIResponsesReceived
 * ---------------------
 * Called when we have received responses from both AIs.
 */
GSM_Play.prototype.onAIResponsesReceived = function() {
    // We process the two responses...
    this._processResponse(this._aiResponses.AI1.data, this._team1);
    this._processResponse(this._aiResponses.AI2.data, this._team2);

    // We play the next turn...
    this._game.playNextTurn();
};

/**
 * _processResponse
 * ----------------
 * Checks that the response is the right type, and passes it to the
 * team to process.
 */
GSM_Play.prototype._processResponse = function (data, team) {
    // TODO: Put try...catch back later.
    //try {
        // We check that we got a PLAY response...
        if(data.requestType !== 'PLAY') {
            throw new CWCError('Expected a PLAY response.')
        }

        // We got a PLAY response, so we pass it to the Team to process...
        team.processPlayResponse(data);
    //} catch(ex) {
    //    // We log the error and report it back to the AI...
    //    team.getAI().sendError(ex.message);
    //}
};





