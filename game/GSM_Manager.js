/**
 * GSM_Manager
 * -----------
 * Manages the game state machine (GSM).
 *
 * Each state manages one section of the game, such as kick-off,
 * regular play, free-kick and so on.
 *
 * The objects that manage each AI have a reference to this manager
 * and can send messages to update the state.
 */
//var GSM_Kickoff = require('./GSM_Kickoff');
var GSM_Play = require('./GSM_Play');

/**
 * @constructor
 */
function GSM_Manager(game) {
    // The current state...
    //this._state = new GSM_Kickoff(game);
    this._state = new GSM_Play(game);
}
module.exports = GSM_Manager;





