var UtilsLib = require('./utils');
var GameLib = require('./game');
var Game = GameLib.Game;
var Logger = UtilsLib.Logger;
var NanoTimer = require('nanotimer');
var util = require('util');

// We set up logging...
Logger.addHandler(new UtilsLib.LogHandler_Console(Logger.LogLevel.INFO_PLUS));
Logger.addHandler(new UtilsLib.LogHandler_File('./log/log.txt', Logger.LogLevel.INFO));

// We create a game, and set the ball moving...
var game = new Game();
var ball = game._ball;
ball._state.position.x = 50;
ball._state.position.y = 25;
ball._state.speed = 10;
ball.friction = 0.0;
ball._state.vector.x = -1.0 * Math.sqrt(0.5);
ball._state.vector.y = -1.0 * Math.sqrt(0.5);

// We run a game loop...
var start = process.hrtime();
for(var i=0; i<240000; ++i) {
    // We update the game state...
    game.calculate();

    // We log the time, and the ball position...
    var interval = process.hrtime(start);
    var intervalSeconds = interval[0] + interval[1] / 1000000000.0;
    var ballJSON = JSON.stringify(ball._state.position);
    var message = util.format('%d: %s', intervalSeconds.toFixed(4), ballJSON);
    Logger.log(message, Logger.LogLevel.INFO);
}

