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
 * Scales the vector by the factor provided.
 */
Vector.prototype.scale = function(factor) {
    this.x *= factor;
    this.y *= factor;
};

// Exports...
module.exports = Vector;


