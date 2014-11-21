/**
 * GSM_Manager
 * -----------
 * Manages the game state machine (GSM).
 *
 * Each state manages one section of the game, such as kick-off,
 * regular play, free-kick and so on.
 *
 * The objects that manage each AI have a reference to this manager
 * and can send messages to the current state.
 *
 * The Game.onTurn function calls checkState() each turn to check the
 * state. This method returns the new state if the state has changed,
 * or 'this' to remain in the current state.
 */

/**
 * @constructor
 */
function GSM_Manager() {
    // The current state...
    this._state = null;
}
module.exports = GSM_Manager;

/**
 * setState
 * --------
 * Sets the current state of the game.
 *
 * In the normal course of events, the state (and transitions between
 * states) is managed by the GSM itself. But sometimes it can be set
 * externally, mostly by the Game object.
 */
GSM_Manager.prototype.setState = function(state) {
    this._state = state;
};

/**
 * onResponse_AI1
 * --------------
 * Called when we get a response from AI1.
 */
GSM_Manager.prototype.onResponse_AI1 = function(jsonData) {
    this._state.onResponse_AI1(jsonData);
};

/**
 * onResponse_AI2
 * --------------
 * Called when we get a response from AI2.
 */
GSM_Manager.prototype.onResponse_AI2 = function(data) {
    this._state.onResponse_AI2(data);
};

/**
 * checkState
 * ----------
 * Called by the game loop after updates have been made. The specific
 * State object checks whether it needs to transition the state.
 */
GSM_Manager.prototype.checkState = function() {
    this._state = this._state.checkState();
};

/**
 * onTurn
 * ------
 * Called by the game loop after the game-state update has been sent to
 * the AIs. The specific State can request actions from the AIs.
 */
GSM_Manager.prototype.onTurn = function() {
    this._state.onTurn();
};


