/**
 * Position
 * --------
 * (x, y) coordinates.
 *
 * In the coding-world-cup (0.0, 0.0) is the top-left corner of the pitch and
 * (100.0, 50.0) is the bottom right.
 */
var Utils = require('./Utils');
var Vector = require('./Vector');

/**
 * @constructor
 */
function Position(x, y) {
    this.x = x;
    this.y = y;
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

/**
 * vectorTo
 * --------
 * Returns the vector from this position to the one passed in.
 */
Position.prototype.vectorTo = function(position) {
    return new Vector(position.x - this.x, position.y - this.y);
};

/**
 * addVector
 * ---------
 * Adds the vector to the position.
 */
Position.prototype.addVector = function(vector) {
    this.x += vector.x;
    this.y += vector.y;
};


// Exports...
module.exports = Position;

