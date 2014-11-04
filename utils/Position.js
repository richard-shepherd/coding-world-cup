/**
 * Position
 * --------
 * (x, y) coordinates.
 *
 * In the coding-world-cup (0.0, 0.0) is the top-left corner of the pitch and
 * (100.0, 50.0) is the bottom right.
 */
var Utils = require('./Utils');

/**
 * @constructor
 */
function Position() {
    this.x = 0.0;
    this.y = 0.0;
}

/**
 * approxEqual
 * -----------
 * True if this position is the same as 'other' to the precision
 * limit we are interested in.
 */
Position.prototype.approxEqual = function(other) {
    if(!Utils.approxEqual(this.x, other.x)) return false;
    if(!Utils.approxEqual(this.y, other.y)) return false;
    return true;
};

/**
 * distanceTo
 * ----------
 * Returns the distance to the 'other' position passed in.
 */
Position.prototype.distanceTo = function(other) {
    var x = other.x - this.x;
    var y = other.y - this.y;
    var x2 = x * x;
    var y2 = y * y;
    var distance = Math.sqrt(x2 + y2);
    return distance;
};

// Exports...
module.exports = Position;

