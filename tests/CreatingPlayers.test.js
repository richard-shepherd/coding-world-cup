require('../utils');
var GameLib = require('../game');
var Game = GameLib.Game;

/**
 * Tests that a newly created game has the expected number of players.
 */
exports['Expected number of players created'] = function(test) {
    // We create a new game...
    var game = new Game();

    // We check the number of players...
    var numberOfPlayers = game.players.countIf(function(player) { return player.isPlayer(); });
    test.equal(numberOfPlayers , 10);

    // We check that there are two goalkeepers...
    var numberOfKeepers = game.players.countIf(function(player) { return player.isGoalkeeper(); });
    test.equal(numberOfKeepers, 2);
    test.done();
};

