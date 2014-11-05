/**
 * BallState
 * ---------
 * Data about the ball: its position, speed, owner etc.
 */
var UtilsLib = require('../utils');
var Position = UtilsLib.Position;
var Vector = UtilsLib.Vector;

/**
 * @constructor
 */
function BallState() {
    // The ball's position...
    this.position = new Position(0.0, 0.0);

    // The direction the ball is travelling.
    // Note: The size of the vector is not important, as this
    //       is controlled by the speed. It is just holding the
    //       direction of travel.
    this.vector = new Vector();

    // The speed in m/s...
    this.speed = 0.0;
}

// Exports...
module.exports = BallState;

