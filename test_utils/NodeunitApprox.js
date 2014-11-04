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


