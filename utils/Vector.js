/**
 * Vector
 * ------
 * Manages a vector. In the coding-world-cup this is usually the
 * difference between two positions on the pitch.
 */

/**
 * @constructor
 */
function Vector(x, y) {
    this.x = x;
    this.y = y;
}

/**
 * scale
 * -----
 * Returns a new vector, from this one scaled by the factor provided.
 */
Vector.prototype.scale = function(factor) {
    return new Vector(this.x * factor, this.y * factor);
};

// Exports...
module.exports = Vector;


