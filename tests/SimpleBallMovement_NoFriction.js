var GameLib = require('../game');
var TestUtilsLib = require('../test_utils');
var Ball = GameLib.Ball;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;

/**
 * Tests simple ball movement without friction.
 */
exports['Move left'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set the ball's position...
    var position = ball._state.position;
    position.x = 30.0;
    position.y = 30.0;

    // Set its movement vector and speed, in m/s...
    ball._state.vector.x = -1;
    ball._state.vector.y = 0.0;
    ball._state.speed = 7.0;

    // We move it over 0.5 second...
    var game = new MockGame_CalculationInterval(0.5);
    ball.updatePosition(game);

    test.approx(position.x, 26.5);
    test.approx(position.y, 30.0);

    test.done();
};

/**
 * Tests simple ball movement without friction.
 */
exports['Move right'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set the ball's position...
    var position = ball._state.position;
    position.x = 30.0;
    position.y = 30.0;

    // Set its movement vector and speed, in m/s...
    ball._state.vector.x = 1.0;
    ball._state.vector.y = 0.0;
    ball._state.speed = 4.0;

    // We move it over 0.5 second...
    var game = new MockGame_CalculationInterval(0.6);
    ball.updatePosition(game);

    test.approx(position.x, 32.4);
    test.approx(position.y, 30.0);

    test.done();
};

/**
 * Tests simple ball movement without friction.
 */
exports['Move up'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set the ball's position...
    var position = ball._state.position;
    position.x = 30.0;
    position.y = 30.0;

    // Set its movement vector and speed, in m/s...
    ball._state.vector.x = 0.0;
    ball._state.vector.y = -1.0;
    ball._state.speed = 9.0;

    // We move it over 0.5 second...
    var game = new MockGame_CalculationInterval(0.8);
    ball.updatePosition(game);

    test.approx(position.x, 30.0);
    test.approx(position.y, 22.8);

    test.done();
};

/**
 * Tests simple ball movement without friction.
 */
exports['Move down'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set the ball's position...
    var position = ball._state.position;
    position.x = 30.0;
    position.y = 30.0;

    // Set its movement vector and speed, in m/s...
    ball._state.vector.x = 0.0;
    ball._state.vector.y = 1.0;
    ball._state.speed = 3.0;

    // We move it over 0.5 second...
    var game = new MockGame_CalculationInterval(0.2);
    ball.updatePosition(game);

    test.approx(position.x, 30.0);
    test.approx(position.y, 30.6);

    test.done();
};





