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
 * received responses from both AIs.
 *
 * Events
 * ------
 * This function should be implemented in the derived GSM classes to handle
 * updates from AIs:
 * - onAIResponsesReceived
 * This function should return the new state (which can be 'this' to
 * remain in the current state).
 */
var AIUtilsLib = require('../ai_utils');
var AIResponse = AIUtilsLib.AIResponse;
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;


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

/**
 * onResponse_AI1
 * --------------
 * Called when we get a response from AI1.
 */
GSM_Base.prototype.onResponse_AI1 = function(jsonData) {
    // We store the jsonData and check whether we have received both updates...
    this._aiResponses.AI1 = this._getAIResponse(jsonData);
    return this._checkResponses();
};

/**
 * onResponse_AI2
 * --------------
 * Called when we get a response from AI2.
 */
GSM_Base.prototype.onResponse_AI2 = function(jsonData) {
    // We store the jsonData and check whether we have received both updates...
    this._aiResponses.AI2 = this._getAIResponse(jsonData);
    return this._checkResponses();
};

/**
 * _checkResponses
 * ---------------
 * Checks whether we have received responses from both AIs.
 * Returns the new state to move to (or 'this' to remain
 * in the current state).
 */
GSM_Base.prototype._checkResponses = function() {
    // We check if we have jsonData from both AIs in our
    // collection of responses...
    if(!('AI1' in this._aiResponses)) return this;
    if(!('AI2' in this._aiResponses)) return this;

    // We've got updates from both AIs.
    var ai1 = this._aiResponses.AI1;
    var ai2 = this._aiResponses.AI2;

    // We convert the JSON data to objects...
    ai1.data = JSON.parse(ai1.jsonData);
    ai2.data = JSON.parse(ai2.jsonData);

    // We call into the derived class to handle the responses...
    return this.onAIResponsesReceived();
};

/**
 * _getAIResponse
 * --------------
 * Creates an AIResponse object to hold the jsonData passed in by an AI,
 * along with other associated info.
 */
GSM_Base.prototype._getAIResponse = function(jsonData) {
    var response = new AIResponse();
    response.jsonData = jsonData;
    response.processingTimeSeconds = this._getProcessingTimeSeconds();
    return response;
};

/**
 * _getProcessingTimeSeconds
 * -------------------------
 * Returns the time in seconds between the update-sent-time
 * and now.
 */
GSM_Base.prototype._getProcessingTimeSeconds = function() {
    var diff = process.hrtime(this._updateSentTime);
    var diffSeconds = diff[0] + diff[1] / 1000000000.0;
    return diffSeconds;
};


// Exports...
module.exports = GSM_Base;


