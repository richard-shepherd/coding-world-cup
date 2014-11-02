var assert = require('nodeunit').assert;

/**
 * approx
 * ------
 * Adds an approx method to nodeunit tests for testing double values.
 * This tests that the actual and expected values are the same to within
 * eight decimal places.
 */
assert.approx = function approx(actual, expected, message) {
    var epsilon = 0.00000001;
    if (actual < expected - epsilon || actual > expected + epsilon) {
        assert.fail(actual, expected, message, "~=");
    }
};


