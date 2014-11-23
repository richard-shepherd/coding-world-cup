/**
 * MockGame_CalculationInterval
 * ----------------------------
 * A mock that lets you set the interval since the previous calculation.
 */
var GameLib = require('../game');
var Game = GameLib.Game;
var MockAIWrapper = require('./MockAIWrapper');
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;


/**
 * @constructor
 */
function MockGame_CalculationInterval(interval) {
    var ai1 = new MockAIWrapper();
    var ai2 = new MockAIWrapper();
    Game.call(this, ai1, ai2, null);
    this._interval = interval;
}
Utils.extend(Game, MockGame_CalculationInterval); // Derives from Game.

/**
 * getCalculationIntervalSeconds
 * -----------------------------
 * Overridden to return the interval specified in the constructor.
 */
MockGame_CalculationInterval.prototype.getCalculationIntervalSeconds = function() {
    return this._interval;
};


// Exports...
module.exports = MockGame_CalculationInterval;


