var TestUtilsLib = require('../test_utils');
var CreatePlayers = TestUtilsLib.CreatePlayers;
var GameMocks = TestUtilsLib.GameMocks;
var GameLib = require('../game');
var PlayerState_Intentions = GameLib.PlayerState_Intentions;


/**
 * Tests that a player can move to the right.
 */
exports['Simple movement right'] = function(test) {
    // We create a player at the centre of the pitch...
    var player = CreatePlayers.createPlayerAt(50.0, 25.0, 90.0);

    // We set the player's intention to move to the right at full speed...
    player._intentionsState.action = PlayerState_Intentions.Action.MOVE;
    player._intentionsState.destination.x = 73.0;
    player._intentionsState.destination.y = 25.0;
    player._intentionsState.speed = 100.0;

    // We set the calculation interval to 1 second...
    var game = new GameMocks.MockGame_CalculationInterval(1.0);

    // The player should be moving at 10m/s, so should have moved
    // 10 metres to the right...
    player.updatePosition(game);
    test.approx(player._dynamicState.position.x, 60.0);
    test.approx(player._dynamicState.position.y, 25.0);

    // 10 more metres...
    player.updatePosition(game);
    test.approx(player._dynamicState.position.x, 70.0);
    test.approx(player._dynamicState.position.y, 25.0);

    // The remaining 3 metres...
    player.updatePosition(game);
    test.approx(player._dynamicState.position.x, 73.0);
    test.approx(player._dynamicState.position.y, 25.0);

    // The player should not move any more...
    player.updatePosition(game);
    test.approx(player._dynamicState.position.x, 73.0);
    test.approx(player._dynamicState.position.y, 25.0);


    test.done();
};



