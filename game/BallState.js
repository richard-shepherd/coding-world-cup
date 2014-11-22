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
    // Note: This is a unit vector.
    this.vector = new Vector(0.0, 0.0);

    // The speed in m/s...
    this.speed = 0.0;

    // The number of the player controlling the ball, or -1 if
    // no player is controlling it. The ball does not move independently
    // if it is being controlled by a player...
    this.controllingPlayerNumber = -1;
}

// Exports...
module.exports = BallState;

