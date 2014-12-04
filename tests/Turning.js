var TestUtilsLib = require('../test_utils');
var PlayerTestUtils = TestUtilsLib.PlayerTestUtils;
var MockGame_CalculationInterval = TestUtilsLib.MockGame_CalculationInterval;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;


/**
 * Tests that we can make a short turn to the right, and
 * not overshoot.
 */
exports['Short turn to the right'] = function(test) {
    // We simulate a 0.1 second interval, which should be more
    // than enough to turn this far...
    var game = new MockGame_CalculationInterval(0.1);
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 50.0, 25.0, 20.0);

    // We set the intention to turn...
    player._setAction_TURN({direction:55.0});
    player.processAction(game);

    // We confirm that we're facing the new direction...
    test.approx(player.dynamicState.direction, 55.0);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    // We update and test again, to make sure the player has not
    // turned any more...
    player.processAction(game);
    test.approx(player.dynamicState.direction, 55.0);

    test.done();
};

/**
 * Tests that we can make a short turn to the left, and
 * not overshoot.
 */
exports['Short turn to the left'] = function(test) {
    // We simulate a 0.1 second interval, which should be more
    // than enough to turn this far...
    var game = new MockGame_CalculationInterval(0.1);
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 50.0, 25.0, 155.0);

    // We set the intention to turn...
    player._setAction_TURN({direction:120.0});
    player.processAction(game);

    // We confirm that we're facing the new direction...
    test.approx(player.dynamicState.direction, 120.0);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Tests that we can make a long turn to the right, which takes
 * more than one calculation cycle.
 */
exports['Long turn to the right'] = function(test) {
    // We simulate 0.1 second intervals and check how far we've turned
    // each time....
    var game = new MockGame_CalculationInterval(0.1);
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 50.0, 25.0, 20.0);

    // We set the intention to turn...
    player._setAction_TURN({direction:165.0});

    player.processAction(game);
    test.approx(player.dynamicState.direction, 80.0);
    player.processAction(game);
    test.approx(player.dynamicState.direction, 140.0);
    player.processAction(game);
    test.approx(player.dynamicState.direction, 165.0);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Tests that we can make a long turn to the right, which takes
 * more than one calculation cycle and goes past 360 degrees.
 */
exports['Long turn to the right past 360'] = function(test) {
    // We simulate 0.1 second intervals and check how far we've turned
    // each time....
    var game = new MockGame_CalculationInterval(0.1);
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 50.0, 25.0, 280.0);

    // We set the intention to turn...
    player._setAction_TURN({direction:48.0});

    player.processAction(game);
    test.approx(player.dynamicState.direction, 340.0);
    player.processAction(game);
    test.approx(player.dynamicState.direction, 40.0);
    player.processAction(game);
    test.approx(player.dynamicState.direction, 48.0);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Tests that we can make a long turn to the left, which takes
 * more than one calculation cycle.
 */
exports['Long turn to the left'] = function(test) {
    // We simulate 0.1 second intervals and check how far we've turned
    // each time....
    var game = new MockGame_CalculationInterval(0.1);
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 50.0, 25.0, 255.0);

    // We set the intention to turn...
    player._setAction_TURN({direction:80.0});

    player.processAction(game);
    test.approx(player.dynamicState.direction, 195.0);
    player.processAction(game);
    test.approx(player.dynamicState.direction, 135.0);
    player.processAction(game);
    test.approx(player.dynamicState.direction, 80.0);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Tests that we can make a short turn to the right, going past 360-degrees.
 */
exports['Short turn to the right past 360'] = function(test) {
    // We simulate a 0.1 second interval, which should be more
    // than enough to turn this far...
    var game = new MockGame_CalculationInterval(0.1);
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 50.0, 25.0, 350.0);

    // We set the intention to turn...
    player._setAction_TURN({direction:33.0});

    player.processAction(game);

    // We confirm that we're facing the new direction...
    test.approx(player.dynamicState.direction, 33.0);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};

/**
 * Tests that we can make a short turn to the left, going past 0-degrees.
 */
exports['Short turn to the left past 0'] = function(test) {
    // We simulate a 0.1 second interval, which should be more
    // than enough to turn this far...
    var game = new MockGame_CalculationInterval(0.1);
    var player = game.getPlayer(1);
    PlayerTestUtils.setPlayerPosition(player, 50.0, 25.0, 5.0);

    // We set the intention to turn...
    player._setAction_TURN({direction:354.0});

    player.processAction(game);

    // We confirm that we're facing the new direction...
    test.approx(player.dynamicState.direction, 354.0);
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    test.done();
};


