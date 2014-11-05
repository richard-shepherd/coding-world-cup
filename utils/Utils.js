/**
 * Utils
 * -----
 * Miscellaneous utility functions.
 */
var Vector = require('./Vector');


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
    return difference < _APPROX_TOLERANCE;
}

/**
 * angleBetween
 * ------------
 * Returns the angle of the line from p1 to p2 in degrees,
 * measured clockwise from a vertical line. p1 and p2 should
 * be Position objects.
 *
 * In other words, the angle is the direction a player at p1
 * should face to be pointing towards p2.
 */
function angleBetween(p1, p2) {
    var deltaY = p1.y - p2.y;
    var deltaX = p1.x - p2.x;
    var angle = Math.atan2(deltaY, deltaX) * 180.0 / Math.PI;
    angle += 270;
    angle %= 360;
    return angle;
}

/**
 * vectorTo
 * --------
 * Returns a vector (with unit length) that points
 * in the direction passed in.
 *
 * The direction is in degrees measured clockwise from vertical.
 */
function vectorTo(direction) {
    var angle = 90.0 - direction;
    var radians = angle * Math.PI / 180.0;
    var x = Math.sin(radians);
    var y = Math.cos(radians);
    return new Vector(x, y);
}

// Exports...
exports.setApproxTolerance = setApproxTolerance;
exports.approxEqual = approxEqual;
exports.angleBetween = angleBetween;

