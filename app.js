var UtilsLib = require('./utils');
var Logger = UtilsLib.Logger;
var LogHandler_Console = UtilsLib.LogHandler_Console;
var LogHandler_File = UtilsLib.LogHandler_File;
var Utils = UtilsLib.Utils;
var GameLib = require('./game');
var Game = GameLib.Game;
var NanoTimer = require('nanotimer');
var util = require('util');

// We set up logging...
Logger.addHandler(new LogHandler_Console(Logger.LogLevel.INFO_PLUS));
Logger.addHandler(new LogHandler_File('./log/log.txt', Logger.LogLevel.INFO));

// We create a game, and set up the teams...
var game = new Game();
game._team1._state.name = 'Robots United';
game._team2._state.name = 'AI City';

// We set the ball moving...
var ball = game._ball;
ball._state.position.x = 50;
ball._state.position.y = 25;
ball._state.speed = 10;
ball.friction = 0.0;
ball._state.vector.x = -1.0 * Math.sqrt(0.5);
ball._state.vector.y = -1.0 * Math.sqrt(0.5);

// We run a game loop...
var start = process.hrtime();
for(var i=0; i<100; ++i) {
    // We update the game state...
    game.calculate();

    // We log the game state...
    var dto = JSON.stringify(game.getStateForDTO(), Utils.decimalPlaceReplacer(4));
    Logger.log(dto, Logger.LogLevel.INFO);
}

// We log the time taken...
Logger.log("Time taken (s)=" + Utils.secondsSince(start), Logger.LogLevel.INFO);

