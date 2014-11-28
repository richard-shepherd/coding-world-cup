/**
 * GoalsDetected
 * -------------
 * Tests that goals are detected, including when the ball bounces
 * back into the pitch.
 */
var TestUtilsLib = require('../test_utils');
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var GameLib = require('../game');
var TeamState = GameLib.TeamState;
var Pitch = GameLib.Pitch;
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;

/**
 * Straight bounce left
 * --------------------
 * The ball is travelling straight (horizontally) and bounces
 * out of the left-hand goal during the calculation cycle.
 */
exports['Straight bounce left'] = function(test) {
    // We set up the game...
    var game = new MockGame_CalculationInterval(1.0);
    game._aiUpdateIntervalSeconds = 1.0;

    // We set the team directions...
    var team1 = game.getTeam1();
    team1.setDirection(TeamState.Direction.RIGHT);
    var team2 = game.getTeam2();
    team2.setDirection(TeamState.Direction.LEFT);

    // We put the ball near the left-hand goal, heading towards it...
    var ball = game.ball;
    var ballState = ball.state;
    var ballPosition = ballState.position;
    var ballVector = ballState.vector;
    ball.friction = 0.0;
    ballPosition.x = 7.0;
    ballPosition.y = 25.0;
    ballVector.x = -1.0;
    ballVector.y = 0.0;
    ballState.speed = 10.0;

    // We calculate...
    game.calculate();

    // The ball should have travelled 10m and team2 should have scored...
    test.approx(ballPosition.x, 3.0);
    test.approx(ballPosition.y, 25.0);
    test.equal(team1.state.score, 0);
    test.equal(team2.state.score, 1);

    test.done();
};

/**
 * Diagonal right
 * --------------
 * The ball travels at an angle into the right goal.
 */
exports['Diagonal right'] = function(test) {
    // We set up the game...
    var game = new MockGame_CalculationInterval(1.0);
    game._aiUpdateIntervalSeconds = 1.0;

    // We set the team directions...
    var team1 = game.getTeam1();
    team1.setDirection(TeamState.Direction.RIGHT);
    var team2 = game.getTeam2();
    team2.setDirection(TeamState.Direction.LEFT);

    // We put the ball near the left-hand goal, heading towards it...
    var ball = game.ball;
    var ballState = ball.state;
    var ballPosition = ballState.position;
    var pitch = game.pitch;
    ball.friction = 0.0;
    ballPosition.x = pitch.width -  4.0;
    ballPosition.y = pitch.goalY2 + 3;
    ballState.vector = Utils.vectorFromDirection(45);
    ballState.speed = 10.0;

    // We calculate...
    game.calculate();

    // Team1 should have scored...
    test.equal(team1.state.score, 1);
    test.equal(team2.state.score, 0);

    test.done();
};


