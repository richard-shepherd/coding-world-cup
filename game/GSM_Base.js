/**
 * GSM_Base
 * --------
 * A base class for state in the game state machine (GSM).
 *
 * Game states often interact with the AIs by sending them updates
 * about the current game state and expecting a response from them.
 *
 * This base class manages the mechanics of some of the common
 * interactions, including timing how long the response takes.
 *
 * Responses
 * ---------
 * We hold responses from AIs in the _aiResponses object. This acts
 * like a map of AI->AIResponse, and helps us determine when we have
 * responses from both AIs.
 *
 * Events
 * ------
 * These functions can be optionally implemented in the derived GSM
 * classes to handle certain events.
 * - onAI1Response
 * - onAI2Response
 * - onBothAIResponses
 */

/**
 * @constructor
 */
function GSM_Base(game) {
    // The game...
    this._game = game;

    // The two AIs...
    this._AI1 = game.getTeam1().getAI();
    this._AI2 = game.getTeam2().getAI();

    // Responses from AIs...
    this._aiResponses = {};
}

/**
 * sendPlayUpdateToBothAIs
 * -----------------------
 * Sends a standard in-play update of the game state to the AIs.
 */
GSM_Base.prototype.sendPlayUpdateToBothAIs = function() {
    // We note the time before sending the update, so that we
    // an time how long the AIs take to process it...
    this._updateSentTime = process.hrtime();

    // We get the DTO and pass it to the AIs...
    var data = this._game.getDTO(true);
    this._AI1.sendUpdate(data);
    this._AI2.sendUpdate(data);
};



// Exports...
module.exports = GSM_Base;


