/**
 * GSM_Play
 * --------
 * Manages standard play - players moving, kicking etc.
 */
var GSM_Base = require('./GSM_Base');


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
    //
};




