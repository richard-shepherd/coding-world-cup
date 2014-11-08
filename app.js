var UtilsLib = require('./utils');
var GameLib = require('./game');
var Game = GameLib.Game;
var Logger = UtilsLib.Logger;
var NanoTimer = require('nanotimer');

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

// We run a game loop every 100ms...
var gameTimer = new NanoTimer();
gameTimer.setInterval(function() {
    game.calculate();
    Logger.log(JSON.stringify(ball._state.position), Logger.LogLevel.INFO_PLUS);
}, '', '100m');

