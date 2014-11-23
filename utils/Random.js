/**
 * Random
 * ------
 * A class to provide random numbers.
 *
 * The main purpose of having this class is to allow it to be mocked,
 * so that we can control the 'random' numbers when creating tests.
 */

/**
 * @constructor
 */
function Random() {
}

/**
 * nextDouble
 * ----------
 * Returns a random double between 0.0 - 1.0
 */
Random.prototype.nextDouble = function() {
    return Math.random();
};

// Exports...
module.exports = Random;


