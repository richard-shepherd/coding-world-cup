var GameLib = require('../game');
var TestUtilsLib = require('../test_utils');
var Ball = GameLib.Ball;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval

/**
 * Tests the ball bouncing off the side of the pitch.
 */
exports['Bounce off top - straight'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set its position...
    var position = ball.state.position;
    position.x = 60.0;
    position.y = 4.0;

    // We set it's direction and speed...
    var vector = ball.state.vector;
    vector.x = 0.0;
    vector.y = -1.0;
    ball.state.speed = 10.0;

    // We move it...
    var game = new MockGame_CalculationInterval(1.0);
    ball.updatePosition(game);
    test.approx(position.x, 60.0);
    test.approx(position.y, 6.0);
    test.approx(vector.x, 0.0);
    test.approx(vector.y, 1.0);

    // Move again...
    ball.updatePosition(game);
    test.approx(position.x, 60.0);
    test.approx(position.y, 16.0);
    test.approx(vector.x, 0.0);
    test.approx(vector.y, 1.0);

    test.done();
};

/**
 * Tests the ball bouncing off the side of the pitch.
 */
exports['Bounce off left - straight'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set its position...
    var position = ball.state.position;
    position.x = 3.0;
    position.y = 20.0;

    // We set it's direction and speed...
    var vector = ball.state.vector;
    vector.x = -1.0;
    vector.y = 0.0;
    ball.state.speed = 10.0;

    // We move it...
    var game = new MockGame_CalculationInterval(1.0);
    ball.updatePosition(game);
    test.approx(position.x, 7.0);
    test.approx(position.y, 20.0);
    test.approx(vector.x, 1.0);
    test.approx(vector.y, 0.0);

    // Move again...
    ball.updatePosition(game);
    test.approx(position.x, 17.0);
    test.approx(position.y, 20.0);
    test.approx(vector.x, 1.0);
    test.approx(vector.y, 0.0);

    test.done();
};

/**
 * Tests the ball bouncing off the side of the pitch.
 */
exports['Bounce off right - straight'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set its position...
    var position = ball.state.position;
    position.x = 93.0;
    position.y = 35.0;

    // We set it's direction and speed...
    var vector = ball.state.vector;
    vector.x = 1.0;
    vector.y = 0.0;
    ball.state.speed = 10.0;

    // We move it...
    var game = new MockGame_CalculationInterval(1.0);
    ball.updatePosition(game);
    test.approx(position.x, 97.0);
    test.approx(position.y, 35.0);
    test.approx(vector.x, -1.0);
    test.approx(vector.y, 0.0);

    // Move again...
    ball.updatePosition(game);
    test.approx(position.x, 87.0);
    test.approx(position.y, 35.0);
    test.approx(vector.x, -1.0);
    test.approx(vector.y, 0.0);

    test.done();
};

/**
 * Tests the ball bouncing off the side of the pitch.
 */
exports['Bounce off bottom - straight'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set its position...
    var position = ball.state.position;
    position.x = 44.0;
    position.y = 48.0;

    // We set it's direction and speed...
    var vector = ball.state.vector;
    vector.x = 0.0;
    vector.y = 1.0;
    ball.state.speed = 10.0;

    // We move it...
    var game = new MockGame_CalculationInterval(1.0);
    ball.updatePosition(game);
    test.approx(position.x, 44.0);
    test.approx(position.y, 42.0);
    test.approx(vector.x, 0.0);
    test.approx(vector.y, -1.0);

    // Move again...
    ball.updatePosition(game);
    test.approx(position.x, 44.0);
    test.approx(position.y, 32.0);
    test.approx(vector.x, 0.0);
    test.approx(vector.y, -1.0);

    test.done();
};

/**
 * Tests the ball bouncing off the side of the pitch.
 */
exports['Bounce off top - diagonal'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set its position...
    var position = ball.state.position;
    position.x = 20.0;
    position.y = 3.0;

    // We set it's direction and speed...
    var vector = ball.state.vector;
    var sqrtHalf = Math.sqrt(0.5);
    vector.x = -1.0 * sqrtHalf;
    vector.y = -1.0 * sqrtHalf;
    ball.state.speed = 10.0;

    // We move it...
    var game = new MockGame_CalculationInterval(1.0);
    ball.updatePosition(game);

    // By the time we hit the wall, we've moved 3 up, 3 to the left.
    // ie, travelled sqrt(18) ~ 4.24. So the ball will travel ~5.76
    // further after bouncing, ie ~4.07 in each direction...
    test.approx(position.x, 12.928932188134);
    test.approx(position.y, 4.0710678118654);
    test.approx(vector.x, -1.0 * sqrtHalf);
    test.approx(vector.y, sqrtHalf);

    test.done();
};

