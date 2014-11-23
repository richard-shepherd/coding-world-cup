var GameLib = require('../game');
var TestUtilsLib = require('../test_utils');
var Ball = GameLib.Ball;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;

/**
 * Tests ball movement with friction.
 */
exports['Move left'] = function(test) {
    // We create a ball...
    var ball = new Ball();

    // Set the ball's position...
    var position = ball.state.position;
    position.x = 30.0;
    position.y = 30.0;

    // Set its movement vector and speed, in m/s...
    ball.state.vector.x = -1.0;
    ball.state.vector.y = 0.0;
    ball.state.speed = 7.0;

    // We move it over 0.5 second...
    // Slow from 7 m/s to 2 m/s over 0.5 sec, so average of 4.5 m/s...
    var game = new MockGame_CalculationInterval(0.5);
    ball.updatePosition(game);
    test.approx(position.x, 27.75);
    test.approx(position.y, 30.0);

    // Slow from 2 m/s to 0m/s over 0.5 sec, so average of 1 m/s...
    ball.updatePosition(game);
    test.approx(position.x, 27.25);
    test.approx(position.y, 30.0);

    // Ball should now be stopped...
    ball.updatePosition(game);
    test.approx(position.x, 27.25);
    test.approx(position.y, 30.0);

    test.done();
};



