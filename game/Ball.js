/**
 * Ball
 * ----
 * Manages the movement of the ball.
 */
var BallState = require('./BallState');
var Pitch = require('./Pitch');
var UtilsLib = require('../utils');
var Position = UtilsLib.Position;
var Utils = UtilsLib.Utils;


/**
 * @constructor
 */
function Ball() {
    // Data about the ball...
    this.state = new BallState();

    // How much the ball slows down as it moves, in m/s/s...
    this.friction = 10.0;
}

/**
 * The maximum speed the ball can travel, in m/s.
 */
Ball.MAX_SPEED = 30.0;

/**
 * setPosition
 * -----------
 */
Ball.prototype.setPosition = function(position) {
    this.state.position.copyFrom(position);
};

/**
 * updatePosition
 * --------------
 * Updates the position of the ball when it is has been kicked.
 */
Ball.prototype.updatePosition = function (game) {
    if(this.state.controllingPlayerNumber !== -1) {
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
    var speedAtStart = this.state.speed;
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

    // We hold the initial position to help check whether a goal
    // has been scored...
    var position = this.state.position;
    var initialPosition = new Position(position.x, position.y);

    // We find the distance travelled, and move the ball...
    var distance = averageSpeed * intervalSeconds;
    var vector = this.state.vector;
    var vectorMoved = this.state.vector.scale(distance);
    position.addVector(vectorMoved);

    // Did the ball bounce?
    var pitch = game.pitch;
    if(position.x < 0.0) {
        game.checkForGoal(initialPosition, position, 0.0);
        position.x *= -1.0;
        vector.x *= -1.0;
    }
    if(position.x > pitch.width) {
        game.checkForGoal(initialPosition, position, pitch.width);
        position.x = pitch.width - (position.x - pitch.width);
        vector.x *= -1.0;
    }

    if(position.y < 0.0) {
        position.y *= -1.0;
        vector.y *= -1.0;
    }

    if(position.y > pitch.height) {
        position.y = pitch.height - (position.y - pitch.height);
        vector.y *= -1.0;
    }

    // We change the speed of the ball...
    this.state.speed = speedAtEnd;
};

/**
 * getMaxSpeed
 * -----------
 * Returns the maximum speed the ball can travel at.
 */
Ball.prototype.getMaxSpeed = function() {
    return Ball.MAX_SPEED;
};

// Exports...
module.exports = Ball;


