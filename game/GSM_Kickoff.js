/**
 * GSM_Kickoff
 * -----------
 * Manages play at kickoff.
 *
 * Requests player positions for he kickoff from the two teams, and
 * starts play when it has received them.
 */
var GSM_Base = require('./GSM_Base');
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;


/**
 * @constructor
 */
function GSM_Kickoff(game, teamToKickOff) {
    // We call the base class constructor...
    GSM_Base.call(this, game);

    // We send the KICKOFF request...
    var request = {
        request:'KICKOFF',
        team1:game.getTeam1().state,
        team2:game.getTeam2().state,
        team:teamToKickOff.getTeamID()
    };
    this.sendRequestToBothAIs(request);
}
Utils.extend(GSM_Base, GSM_Kickoff); // Derived from GSM_Base.
module.exports = GSM_Kickoff;



