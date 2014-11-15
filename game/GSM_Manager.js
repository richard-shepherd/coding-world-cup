/**
 * GSM_Manager
 * -----------
 * Manages the game state machine (GSM).
 *
 * Each state manages one section of the game, such as kick-off,
 * regular play, free-kick and so on.
 *
 * The objects that manage each AI have a reference to this manager
 * and can send messages to update the state. Updates may change the
 * state.
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

/**
 * onUpdate_AI1
 * ------------
 * Called when we get an update from AI1.
 */
GSM_Manager.prototype.onUpdate_AI1 = function(data) {
    this._state = this._state.onUpdate_AI1(data);
};

/**
 * onUpdate_AI2
 * ------------
 * Called when we get an update from AI2.
 */
GSM_Manager.prototype.onUpdate_AI2 = function(data) {
    this._state = this._state.onUpdate_AI2(data);
};



