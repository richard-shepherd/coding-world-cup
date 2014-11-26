var assert = require('nodeunit').assert;
var Utils = require('../utils').Utils;

/**
 * approx
 * ------
 * Adds an approx method to nodeunit tests for testing double values.
 * This tests that the actual and expected values are the same to within
 * eight decimal places.
 */
assert.approx = function approx(actual, expected, message) {
    if(!Utils.approxEqual(actual, expected)) {
        assert.fail(actual, expected, message, "~=");
    }
};

/**
 * lessThanOrEqual
 * ---------------
 * Adds a less-than-or-equal test.
 */
assert.lessThanOrEqual = function approx(actual, expected, message) {
    if(!(actual <= expected)) {
        assert.fail(actual, expected, message, "<=");
    }
};

/**
 * lessThan
 * --------
 * Adds a less-than test.
 */
assert.lessThan = function approx(actual, expected, message) {
    if(!(actual < expected)) {
        assert.fail(actual, expected, message, "<");
    }
};

/**
 * greaterThanOrEqual
 * ------------------
 * Adds a greater-than-or-equal test.
 */
assert.greaterThanOrEqual = function approx(actual, expected, message) {
    if(!(actual >= expected)) {
        assert.fail(actual, expected, message, ">=");
    }
};

/**
 * greaterThan
 * -----------
 * Adds a greater-than test.
 */
assert.greaterThan = function approx(actual, expected, message) {
    if(!(actual > expected)) {
        assert.fail(actual, expected, message, ">");
    }
};




