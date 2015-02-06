/**
 * Tournament
 * ----------
 * Plays a tournament between all available AIs, playing each one against
 * all the others.
 */
var AIUtilsLib = require('../ai_utils');
var AIManager = AIUtilsLib.AIManager;
var Game = require('./Game');
var UtilsLib = require('../utils');
var Logger = UtilsLib.Logger;


/**
 * @constructor
 */
function Tournament() {
    // We find all the AIs and create the score objects for them...
    this._aiManager = new AIManager();
    this._aiNames = this._aiManager.getAINames();
    this._scores = {};
    this._aiNames.forEach(function(aiName) {
        this._scores[aiName] = {
            gamesWon: 0,
            goalsScored: 0
        };
    }, this);
}

/**
 * play
 * ----
 */
Tournament.prototype.play = function() {
    this._playOneRound();
};

/**
 * playOneRound
 * ------------
 * Plays one round of all AIs against all others.
 */
Tournament.prototype._playOneRound = function() {
    this._playNextGame(0, 0);
};

/**
 * playNextGame
 * ------------
 * Plays a game between the players with the indexes passed in.
 */
Tournament.prototype._playNextGame = function(player1Index, player2Index) {
    var numberOfPlayers = this._aiNames.length;
    if(player2Index === numberOfPlayers) {
        // We've played all games for player1, so we move to the next player...
        player1Index += 1;
        player2Index = 0;
    }
    if(player1Index === numberOfPlayers) {
        // We've played all the games in this round...
        return;
    }

    // We find the AIs...
    var that = this;
    var player1Name = this._aiNames[player1Index];
    var player2Name = this._aiNames[player2Index];
    if(player1Name === player2Name) {
        player2Index += 1;
        setImmediate(function() {
            that._playNextGame(player1Index, player2Index);
        });
        return;
    }
    var ai1 = this._aiManager.getAIWrapperFromName(player1Name);
    var ai2 = this._aiManager.getAIWrapperFromName(player2Name);

    // We log who's playing...
    Logger.log("---", Logger.LogLevel.INFO_PLUS);
    Logger.log(player1Name + " vs. " + player2Name, Logger.LogLevel.INFO_PLUS);

    // We play the game...
    var game = new Game(ai1, ai2, function() {
        // Called when the game has completed...

        // We update the scores...
        var team1Score = game.getTeam1().state.score;
        var team1Info = that._scores[player1Name];
        team1Info.goalsScored += team1Score;

        var team2Score = game.getTeam2().state.score;
        var team2Info = that._scores[player2Name];
        team2Info.goalsScored += team2Score;

        if(team1Score > team2Score) {
            team1Info.gamesWon += 1;
        }
        if(team2Score > team1Score) {
            team2Info.gamesWon += 1;
        }

        // We show the scores...
        that._showScores();

        // We play the next game...
        player2Index += 1;
        setImmediate(function() {
            that._playNextGame(player1Index, player2Index);
        });
    });
    game.setTurnRate(0.0);
    game.play();
};

/**
 * showScores
 * ----------
 */
Tournament.prototype._showScores = function() {
    for(var name in this._scores) {
        var score = this._scores[name];
        Logger.log(name + ": Games=" + score.gamesWon + ", Goals=" + score.goalsScored, Logger.LogLevel.INFO_PLUS);
    }
};

// Exports...
module.exports = Tournament;
