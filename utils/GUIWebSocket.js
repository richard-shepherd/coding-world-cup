/**
 * GUIWebSocket
 * ------------
 * Manages a WebSocket for sending data to the GUI.
 */
var WebSocketLib = require('ws');
var WebSocketServer = WebSocketLib.Server;
var Logger = require('./Logger');


/**
 * @constructor
 */
function GUIWebSocket(port) {
    // We store the port...
    this._port = port;
}

/**
 * connect
 * -------
 * Connects and calls onConnect when we have received a message from a client.
 */
GUIWebSocket.prototype.connect = function(onConnect) {
    // We create the server...
    this.server = new WebSocketServer({port:this._port});

    // We call back when we get a connection...
    Logger.log("Waiting for GUI connection...", Logger.LogLevel.INFO);
    this.server.on('connection', function(ws) {
        // We've received a connection...
        Logger.log("Got a GUI connection!", Logger.LogLevel.INFO);
        onConnect();
    });
};

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

