var UtilsLib = require('./utils');
var Logger = UtilsLib.Logger;
var LogHandler_Console = UtilsLib.LogHandler_Console;
var LogHandler_File = UtilsLib.LogHandler_File;
var GameLib = require('./game');
var Game = GameLib.Game;
var TestUtilsLib = require('./test_utils');
var AIWrapper_RandomMovement = TestUtilsLib.AIWrapper_RandomMovement;


// We set up logging...
Logger.addHandler(new LogHandler_Console(Logger.LogLevel.DEBUG));
Logger.addHandler(new LogHandler_File('./log/log.txt', Logger.LogLevel.DEBUG));

// We create two RandomMovement AIs for the teams...
var ai1 = new AIWrapper_RandomMovement();
var ai2 = new AIWrapper_RandomMovement();

// We create a new game...
var game = new Game(ai1, ai2);

// And we start to play...
game.onTurn();



