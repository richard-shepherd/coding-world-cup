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
 * vectorFromDirection
 * -------------------
 * Returns a vector (with unit length) that points
 * in the direction passed in.
 *
 * The direction is in degrees measured clockwise from vertical.
 */
function vectorFromDirection(direction) {
    var angle = 90.0 - direction;
    var radians = angle * Math.PI / 180.0;
    var x = Math.cos(radians);
    var y = -1.0 * Math.sin(radians);
    return new Vector(x, y);
}

/**
 * replacer
 * --------
 * Returns a function that can be used with JSON.stringify
 * to replace numbers with versions truncated to the
 * specified number of decimal places.
 */
function decimalPlaceReplacer(decimalPlaces) {
    return function(key, value) {
        return value.toFixed ? Number(value.toFixed(decimalPlaces)) : value;
    };
}

/**
 * secondsSince
 * ------------
 * Returns the number of seconds since 'time', which should be a
 * time returned by process.hrtime().
 */
function secondsSince(time) {
    var elapsed = process.hrtime(time);
    var elapsedSeconds = elapsed[0] + elapsed[1] / 1000000000.0;
    return elapsedSeconds;
}

/**
 * callIfExists
 * ------------
 * Calls target.function(args) if functionName is the name of
 * a function on target. Any arguments after target and functionName
 * are passed to the function.
 *
 * You call it like this:
 *   callIfExists(this, 'go', 3, 4, 5);
 */
function callIfExists(target, functionName) {
    // We get the array of arguments...
    var args = Array.prototype.slice.call(arguments);

    // We remove target and functionName...
    args.shift();
    args.shift();

    // We check if the function exists...
    var fn = target[functionName];
    if (typeof fn === 'function') {
        fn.apply(target, args);
    }
}

// Exports...
exports.setApproxTolerance = setApproxTolerance;
exports.approxEqual = approxEqual;
exports.angleBetween = angleBetween;
exports.vectorFromDirection = vectorFromDirection;
exports.decimalPlaceReplacer = decimalPlaceReplacer;
exports.secondsSince = secondsSince;
exports.callIfExists = callIfExists;
