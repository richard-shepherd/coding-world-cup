/**
 * MessageUtils
 * ------------
 * Some helper functions for creating messages to send to AIs and to the GUI.
 */
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;


/**
 * getEventJSON
 * ------------
 */
function getEventJSON(event) {
    // We add the EVENT type to the message and convert to JSON...
    event.messageType = "EVENT";
    var jsonEvent = JSON.stringify(event, Utils.decimalPlaceReplacer(3));
    return jsonEvent;
}

/**
 * getRequestJSON
 * --------------
 */
function getRequestJSON(request) {
    // We add the REQUEST type to the message...
    request.messageType = "REQUEST";
    var jsonRequest = JSON.stringify(request, Utils.decimalPlaceReplacer(3));
    return jsonRequest;
}

// Exports...
exports.getEventJSON = getEventJSON;
exports.getRequestJSON = getRequestJSON;
