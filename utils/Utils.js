/**
 * Utils
 * -----
 * Miscellaneous utility functions.
 */

/**
 * The tolerance for the approxEqual function.
 */
var _APPROX_TOLERANCE = 0.00000001;

/**
 * setApproxTolerance
 * ------------------
 * Sets the tolerance used for approxEqual.
 */
function setApproxTolerance(tolerance) {
    _APPROX_TOLERANCE = tolerance;
}

/**
 * approxEqual
 * -----------
 * True if the values passed in are equal to within the
 * _APPROX_TOLERANCE.
 */
function approxEqual(a, b) {
    var difference = Math.abs(a - b);
    if(difference < _APPROX_TOLERANCE) {
        return true;
    } else {
        return false;
    }
}

// Exports...
exports.setApproxTolerance = setApproxTolerance;
exports.approxEqual = approxEqual;

