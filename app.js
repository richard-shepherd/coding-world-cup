var UtilsLib = require('./utils');
var Logger = UtilsLib.Logger;
var LogHandler_Console = UtilsLib.LogHandler_Console;
var LogHandler_File = UtilsLib.LogHandler_File;
var GameLib = require('./game');
var Game = GameLib.Game;
var GUIWebSocket = UtilsLib.GUIWebSocket;
var AIUtilsLib = require('./ai_utils');
var AIManager = AIUtilsLib.AIManager;


// We set up logging...
Logger.addHandler(new LogHandler_Console(Logger.LogLevel.INFO));
Logger.addHandler(new LogHandler_File('./log/log.txt', Logger.LogLevel.DEBUG));

// The AI manager finds the available AIs...
var aiManager = new AIManager();

// We create two AIs...
var ai1 = aiManager.getAIWrapperFromName('BootAndShoot');
var ai2 = aiManager.getAIWrapperFromName('BootAndShoot');

// We create a game...
var game = new Game(ai1, ai2);

// Simulation-mode: true plays the game as fast as possible, false plays close to real-time...
game.setSimulationMode(false);

// Set if you want to play with a GUI. If true, this creates a WebSocket and waits for a client connection...
var playWithGUI = true;
if(playWithGUI) {
    // We're playing with a GUI, so we wait for a client connection...
    var guiWebSocket = new GUIWebSocket(12345);
    guiWebSocket.connect(function() {
        game.guiWebSocket = guiWebSocket;
        game.play();
    });
} else {
    // We're playing without a GUI, so we play straight away...
    game.play();
}

