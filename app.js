var UtilsLib = require('./utils');
var Logger = UtilsLib.Logger;
var LogHandler_Console = UtilsLib.LogHandler_Console;
var LogHandler_File = UtilsLib.LogHandler_File;
var GameLib = require('./game');
var Game = GameLib.Game;
var TestUtilsLib = require('./test_utils');
var AIWrapper_RandomMovement = TestUtilsLib.AIWrapper_RandomMovement;
var GUIWebSocket = UtilsLib.GUIWebSocket;
var AIUtilsLib = require('./ai_utils');
var AIManager = AIUtilsLib.AIManager;


// We set up logging...
Logger.addHandler(new LogHandler_Console(Logger.LogLevel.INFO));
Logger.addHandler(new LogHandler_File('./log/log.txt', Logger.LogLevel.DEBUG));

// The AI manager finds the available AIs...
var aiManager = new AIManager();

// We create two AIs...
var ai1 = aiManager.getAIWrapperFromName('RandomMovement');
var ai2 = aiManager.getAIWrapperFromName('RandomMovement');

// We create a new game...
var guiWebSocket = new GUIWebSocket(12345);
var game = new Game(ai1, ai2, guiWebSocket);
game.giveAllPlayersMaxAbilities();

// And we start to play...
game.onTurn();

