require('../utils');
var GameLib = require('../game');
var Game = GameLib.Game;
var PlayerState_Static = GameLib.PlayerState_Static;


/**
 * Tests that a newly created game has the expected number of players.
 */
exports['Expected number of players created'] = function(test) {
    // We create a new game...
    var game = new Game();

    // We check the number of players...
    var numberOfPlayers = game.players.countIf(function(player) {
        return player.staticState.playerType === PlayerState_Static.PlayerType.PLAYER;
    });
    test.equal(numberOfPlayers , 10);

    // We check that there are two goalkeepers...
    var numberOfKeepers = game.players.countIf(function(player) {
        return player.staticState.playerType === PlayerState_Static.PlayerType.PLAYER;
    });
    test.equal(numberOfKeepers, 2);
};

