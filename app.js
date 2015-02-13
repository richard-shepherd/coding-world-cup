var UtilsLib = require('./utils');
var Logger = UtilsLib.Logger;
var LogHandler_Console = UtilsLib.LogHandler_Console;
var LogHandler_File = UtilsLib.LogHandler_File;
var GameLib = require('./game');
var Game = GameLib.Game;
var Tournament = GameLib.Tournament;
var GUIWebSocket = UtilsLib.GUIWebSocket;
var AIUtilsLib = require('./ai_utils');
var AIManager = AIUtilsLib.AIManager;

// Comment out one of the lines below to either play a
// single game or play a tournament...
//playGame('ShootingStars', 'Rimpo');
playTournament();

/**
 * playTournament
 * --------------
 * Plays a tournament of a number of rounds between all combinations of players.
 */
function playTournament() {
    // We set up logging...
    Logger.addHandler(new LogHandler_Console(Logger.LogLevel.INFO_PLUS));
    Logger.addHandler(new LogHandler_File('./log/log.txt', Logger.LogLevel.DEBUG));

    // We play the tournament...
    var tournament = new Tournament();
    tournament.play();
}

/**
 * playGame
 * --------
 * Plays a game between two players.
 */
function playGame(player1Name, player2Name) {
    // We set up logging...
    Logger.addHandler(new LogHandler_Console(Logger.LogLevel.INFO));
    Logger.addHandler(new LogHandler_File('./log/log.txt', Logger.LogLevel.DEBUG));

    // The AI manager finds the available AIs...
    var aiManager = new AIManager();

    // We create two AIs...
    var ai1 = aiManager.getAIWrapperFromName(player1Name);
    var ai2 = aiManager.getAIWrapperFromName(player2Name);

    // We create a game...
    var game = new Game(ai1, ai2);

    // 1.0 plays at real-time, 0.5 plays twice as fast, 0.0 plays as fast as possible...
    game.setTurnRate(0.5);

    // Set if you want to play with a GUI. If true, this creates a WebSocket and waits for a client connection...
    var playWithGUI = false;
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
}

