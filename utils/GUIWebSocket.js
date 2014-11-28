/**
 * GUIWebSocket
 * ------------
 * Manages a WebSocket for sending data to the GUI.
 */
var WebSocketLib = require('ws');
var WebSocketServer = WebSocketLib.Server;


/**
 * @constructor
 */
function GUIWebSocket(port) {
    // We create the server...
    this.server = new WebSocketServer({port:port});
}

/**
 * broadcast
 * ---------
 * Sends the data to all clients attached to the WebSocket.
 */
GUIWebSocket.prototype.broadcast = function(jsonData) {
    for(var i in this.server.clients) {
        this.server.clients[i].send(jsonData);
    }
};

// Exports...
module.exports = GUIWebSocket;

