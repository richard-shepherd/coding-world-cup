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
    GSM_Base.call(this, game);
    this.sendPlayUpdateToBothAIs();
}
GSM_Play.prototype = new GSM_Base(); // Inherits from GSM_Base...
module.exports = GSM_Play;





