/**
 * MessageUtils
 * ------------
 * Some helper functions for creating messages to send to AIs and to the GUI.
 */


/**
 * getEventJSON
 * ------------
 */
function getEventJSON(event) {
    // We add the EVENT type to the message and convert to JSON...
    event.messageType = "EVENT";
    var jsonEvent = JSON.stringify(event);
    return jsonEvent;
}

/**
 * getRequestJSON
 * --------------
 */
function getRequestJSON(request) {
    // We add the REQUEST type to the message...
    request.messageType = "REQUEST";
    var jsonRequest = JSON.stringify(request);
    return jsonRequest;
}

// Exports...
exports.getEventJSON = getEventJSON;
exports.getRequestJSON = getRequestJSON;
