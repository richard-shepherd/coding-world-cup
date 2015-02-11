var TestUtilsLib = require('../test_utils');
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var GameLib = require('../game');
var GameUtils = GameLib.GameUtils;
var GSM_Play = GameLib.GSM_Play;

/**
 * Tests that timewasting detection is working if the ball has been in
 * the goal area for more than 20 seconds.
 */
exports['time wasting'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);
    game.goalAreaTimewastingLimitSeconds = 20.0;

    // We put the ball in the goal area...
    var ballState = game.ball.state;
    var ballPosition = ballState.position;
    ballPosition.x = 4.0;
    ballPosition.y = 25.0;

    // We put the game into 'play' mode...
    game._gsmManager.setState(new GSM_Play(game));

    // We play for 15 seconds...
    for(var i=0; i<150; ++i) {
        game.calculate();
    }

    // We should be in the GSM_BallInGoalArea state...
    test.equal(game._gsmManager._state.constructor.name, "GSM_BallInGoalArea");

    // We play for another 10 seconds...
    for(var i=0; i<100; ++i) {
        game.calculate();
    }

    // The ball should have been teleported outside the goal area...
    var ballInGoalArea = GameUtils.positionIsInGoalArea(ballPosition);
    test.equal(ballInGoalArea, false)

    // We should be back in the GSM_Play state...
    test.equal(game._gsmManager._state.constructor.name, "GSM_Play");

    test.done();
};

/**
 * The ball moves out of the area before triggering timewasting.
 * We check that the GSM states are handled correctly.
 */
exports['ball moves before time wasting'] = function(test) {
    // The mock game...
    var game = new MockGame_CalculationInterval(0.1);
    game.goalAreaTimewastingLimitSeconds = 20.0;

    // We put the ball in the goal area...
    var ballState = game.ball.state;
    var ballPosition = ballState.position;
    ballPosition.x = 4.0;
    ballPosition.y = 25.0;

    // We put the game into 'play' mode...
    game._gsmManager.setState(new GSM_Play(game));

    // We play for 10 seconds...
    for(var i=0; i<100; ++i) {
        game.calculate();
    }

    // We should be in the GSM_BallInGoalArea state...
    test.equal(game._gsmManager._state.constructor.name, "GSM_BallInGoalArea");

    // We move the ball outside the goal area...
    ballPosition.x = 34.0;
    ballPosition.y = 25.0;
    game.calculate();

    // We should be back in the Play state...
    test.equal(game._gsmManager._state.constructor.name, "GSM_Play");

    test.done();
};


