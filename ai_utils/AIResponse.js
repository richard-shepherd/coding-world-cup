/**
 * AIResponse
 * ----------
 * Holds the data sent to us  by an AI, along with other data
 * associated with it.
 */

/**
 * @constructor
 */
function AIResponse() {
    // The jsonData sent to us by the AI. This is a JSON string...
    this.jsonData = "";

    // The data parsed into an object...
    this.data = {};

    // The time in seconds that the AI took to respond...
    this.processingTimeSeconds = 0.0;
}

// Exports...
module.exports = AIResponse;