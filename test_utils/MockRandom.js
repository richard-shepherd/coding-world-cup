/**
 * MockRandom
 * ----------
 * A mock for the Utils.Random class which allows you to specify
 * the next random number that will be returned.
 */
var UtilsLib = require('../utils');
var CWCError = UtilsLib.CWCError;


/**
 * @constructor
 * 'values' should be an array of doubles between 0.0 - 1.0 that
 * will be returned in order from calls to nextDouble.
 */
function MockRandom(values) {
    this._values = values;
    this._index = 0;
}

/**
 * nextDouble
 * ----------
 * Returns the next value from 'values' passed to the constructor.
 */
MockRandom.prototype.nextDouble = function() {
    if(this._index >= this._values.length) {
        this._index = 0;
    }
    var value = this._values[this._index];
    this._index++;
    return value;
};

// Exports...
module.exports = MockRandom;



