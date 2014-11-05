/**
 * Ball
 * ----
 * Manages the movement of the ball.
 */
var BallState = require('./BallState');

/**
 * @constructor
 */
function Ball() {
    // Data about the ball...
    this._ballState = new BallState();
}



// Exports...
module.exports = Ball;


