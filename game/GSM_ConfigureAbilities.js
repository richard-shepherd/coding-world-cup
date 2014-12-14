/**
 * GSM_ConfigureAbilities
 * ----------------------
 * Sends a request to the AIs to chose the abilities for each player.
 */
var GSM_Base = require('./GSM_Base');
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;
var CWCError = UtilsLib.CWCError;
var GSM_Kickoff = require('./GSM_Kickoff');


/**
 * @constructor
 */
function GSM_ConfigureAbilities(game) {
    // We call the base class constructor...
    GSM_Base.call(this, game);

    // We send the CONFIGURE_ABILITIES request...
    var request = {
        requestType:'CONFIGURE_ABILITIES',
        totalKickingAbility:game.maxTotalAbility,
        totalRunningAbility:game.maxTotalAbility,
        totalBallControlAbility:game.maxTotalAbility,
        totalTacklingAbility:game.maxTotalAbility
    };
    this.sendRequestToBothAIs(request);

}
Utils.extend(GSM_Base, GSM_ConfigureAbilities); // Derived from GSM_Base.
module.exports = GSM_ConfigureAbilities;

/**
 * onAIResponsesReceived
 * ---------------------
 * Called when we have received responses from both AIs.
 */
GSM_ConfigureAbilities.prototype.onAIResponsesReceived = function() {
    // We process the two responses...
    this._processResponse(this._aiResponses.AI1.data, this._team1);
    this._processResponse(this._aiResponses.AI2.data, this._team2);

    // We kick-off...
    this._game.setGameState(new GSM_Kickoff(this._game, this._game.getTeam1()));
};

/**
 * _processResponse
 * ----------------
 * Checks that the response is the right type, and passes it to the
 * team to process.
 */
GSM_ConfigureAbilities.prototype._processResponse = function (data, team) {
    try {
    // We check that we got a PLAY response...
    if(data.requestType !== 'CONFIGURE_ABILITIES') {
        throw new CWCError('Expected a CONFIGURE_ABILITIES response.')
    }

    // We got a CONFIGURE_ABILITIES response, so we pass it to the Team to process...
    team.processConfigureAbilitiesResponse(data, this._game.maxTotalAbility);
    } catch(ex) {
        // We log the error and report it back to the AI...
        team.getAI().sendError(ex.message);
    }
};
