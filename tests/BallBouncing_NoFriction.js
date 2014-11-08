var GameLib = require('../game');
var TestUtils = require('../test_utils');
var Ball = GameLib.Ball;
var GameMocks = TestUtils.GameMocks;

/**
 * Tests the ball bouncing off the side of the pitch.
 */
exports['Bounce off top - straight'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set its position...
    var position = ball._state.position;
    position.x = 60.0;
    position.y = 4.0;

    // We set it's direction and speed...
    var vector = ball._state.vector;
    vector.x = 0.0;
    vector.y = -1.0;
    ball._state.speed = 10.0;

    // We move it...
    var game = new GameMocks.MockGame_CalculationInterval(1.0);
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
exports['Bounce off top - diagonal'] = function(test) {
    // We create a ball...
    var ball = new Ball();
    ball.friction = 0.0;

    // Set its position...
    var position = ball._state.position;
    position.x = 20.0;
    position.y = 3.0;

    // We set it's direction and speed...
    var vector = ball._state.vector;
    var sqrtHalf = Math.sqrt(0.5);
    vector.x = -1.0 * sqrtHalf;
    vector.y = -1.0 * sqrtHalf;
    ball._state.speed = 10.0;

    // We move it...
    var game = new GameMocks.MockGame_CalculationInterval(1.0);
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

