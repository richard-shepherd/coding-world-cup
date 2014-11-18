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
 * onResponse_AI1
 * --------------
 * Called when we get a response from AI1.
 */
GSM_Manager.prototype.onResponse_AI1 = function(jsonData) {
    this._state = this._state.onResponse_AI1(jsonData);
};

/**
 * onResponse_AI2
 * --------------
 * Called when we get a response from AI2.
 */
GSM_Manager.prototype.onResponse_AI2 = function(data) {
    this._state = this._state.onResponse_AI2(data);
};



