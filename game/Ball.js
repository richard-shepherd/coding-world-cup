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
    this._state = new BallState();

    // How much the ball slows down as it moves, in m/s/s...
    this.friction = 10.0;
}

/**
 * The maximum speed the ball can travel, in m/s.
 */
Ball.MAX_SPEED = 30.0;

/**
 * updatePosition
 * --------------
 * Updates the position of the ball when it is has been kicked.
 */
Ball.prototype.updatePosition = function (game) {
    if(this._state.controllingPlayerNumber !== -1) {
        // The ball is being controlled by a player so we
        // don't move it independently...
        return;
    }

    // The ball is travelling under its own steam.
    //
    // The ball slows down as it moves. So we find its speed at
    // the start and at the end of the interval we are calculating
    // and move at the average speed...
    var intervalSeconds = game.getCalculationIntervalSeconds();
    var speedAtStart = this._state.speed;
    var speedAtEnd = speedAtStart - this.friction * intervalSeconds;
    if(speedAtEnd < 0.0) {
        // The ball has come to a stop during this interval.
        // I suppose that this really means that we should adjust
        // the interval as well, as the ball has stopped before
        // the end of it. But maybe the ball just rolls a bit at
        // the end :-)
        speedAtEnd = 0.0;
    }
    var averageSpeed = (speedAtStart + speedAtEnd) / 2.0;

    // We find the distance travelled, and move the ball...
    var distance = averageSpeed * intervalSeconds;
    var vector = this._state.vector.scale(distance);
    this._state.position.addVector(vector);

    // We change the speed of the ball...
    this._state.speed = speedAtEnd;
};

// Exports...
module.exports = Ball;


