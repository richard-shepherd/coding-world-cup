var TestUtilsLib = require('../test_utils');
var CreatePlayers = TestUtilsLib.CreatePlayers;
var GameMocks = TestUtilsLib.GameMocks;
var GameLib = require('../game');
var PlayerState_Action = GameLib.PlayerState_Action;


/**
 * Tests that a player can move to the right.
 */
exports['Simple movement right'] = function(test) {
    // We create a player at the centre of the pitch...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 90.0);

    // We set the player's intention to move to the right at full speed...
    player._setAction_MOVE({destination:{x:73.0, y:25.0}, speed:100.0});

    // We set the calculation interval to 1 second...
    var game = new GameMocks.MockGame_CalculationInterval(1.0);

    // The player should be moving at 10m/s, so should have moved 10 metres to the right...
    player.updatePosition(game);
    test.approx(player.dynamicState.position.x, 60.0);
    test.approx(player.dynamicState.position.y, 25.0);

    // 10 more metres...
    player.updatePosition(game);
    test.approx(player.dynamicState.position.x, 70.0);
    test.approx(player.dynamicState.position.y, 25.0);

    // The remaining 3 metres...
    player.updatePosition(game);
    test.approx(player.dynamicState.position.x, 73.0);
    test.approx(player.dynamicState.position.y, 25.0);

    // The player should have stopped moving...
    test.equal(player.actionState.action, PlayerState_Action.Action.NONE);

    // The player should not move any more...
    player.updatePosition(game);
    test.approx(player.dynamicState.position.x, 73.0);
    test.approx(player.dynamicState.position.y, 25.0);

    test.done();
};

/**
 * Tests that a player can move upwards.
 */
exports['Simple movement up'] = function(test) {
    // We create a player at the centre of the pitch...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 0.0);

    // We set the player's intention to move upwards at full speed...
    player._setAction_MOVE({destination:{x:50.0, y:5.0}, speed:100.0});

    // We set the calculation interval to 1 second...
    var game = new GameMocks.MockGame_CalculationInterval(1.0);

    // The player should be moving at 10m/s, so should have moved 10 metres to the right...
    player.updatePosition(game);
    test.approx(player.dynamicState.position.x, 50.0);
    test.approx(player.dynamicState.position.y, 15.0);

    // The player has not reached the destination, so should still be moving...
    test.equal(player.actionState.action, PlayerState_Action.Action.MOVE);

    test.done();
};

/**
 * Tests that a player can move downwards.
 */
exports['Simple movement down'] = function(test) {
    // We create a player at the centre of the pitch...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 180.0);

    // We set the player's intention to move downwards at full speed...
    player._setAction_MOVE({destination:{x:50.0, y:45.0}, speed:100.0});

    // We set the calculation interval to 1 second...
    var game = new GameMocks.MockGame_CalculationInterval(1.0);

    // The player should be moving at 10m/s, so should have moved 10 metres downwards...
    player.updatePosition(game);
    test.approx(player.dynamicState.position.x, 50.0);
    test.approx(player.dynamicState.position.y, 35.0);

    // The player has not reached the destination, so should still be moving...
    test.equal(player.actionState.action, PlayerState_Action.Action.MOVE);

    test.done();
};

/**
 * Tests that a player can move left.
 */
exports['Simple movement left'] = function(test) {
    // We create a player at the centre of the pitch...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 270.0);

    // We set the player's intention to move left at full speed...
    player._setAction_MOVE({destination:{x:30.0, y:25.0}, speed:100.0});

    // We set the calculation interval to 1 second...
    var game = new GameMocks.MockGame_CalculationInterval(1.0);

    // The player should be moving at 10m/s, so should have moved 10 metres to the left...
    player.updatePosition(game);
    test.approx(player.dynamicState.position.x, 40.0);
    test.approx(player.dynamicState.position.y, 25.0);

    // The player has not reached the destination, so should still be moving...
    test.equal(player.actionState.action, PlayerState_Action.Action.MOVE);

    test.done();
};



