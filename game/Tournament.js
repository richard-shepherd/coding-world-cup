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
var Utils = UtilsLib.Utils;
var Logger = UtilsLib.Logger;
var fs = require('fs');
var filendir = require('filendir');


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
            gamesDrawn: 0,
            gamesLost: 0,
            points: 0,
            processingTimeSeconds: 0.0,
            goalsScored: 0
        };
    }, this);

    // We create a log file for the tournament...
    filendir.ws('tournament.log', '');
    this._file = fs.openSync('tournament.log', 'w');
}

/**
 * play
 * ----
 */
Tournament.prototype.play = function() {
    this._playNextRound(1);
};

/**
 * playNextRound
 * -------------
 * Plays one round of the tournament, ie all players against all other players.
 */
Tournament.prototype._playNextRound = function(roundNumber) {
    var that = this;
    setImmediate(function() {
        that._playNextGame(roundNumber, 0, 0, function() {
            that._playNextRound(roundNumber+1);
        });
    });
};

/**
 * playNextGame
 * ------------
 * Plays a game between the players with the indexes passed in.
 */
Tournament.prototype._playNextGame = function(roundNumber, player1Index, player2Index, roundFinishedCallback) {
    var numberOfPlayers = this._aiNames.length;
    if(player2Index === numberOfPlayers) {
        // We've played all games for player1, so we move to the next player...
        player1Index += 1;
        player2Index = 0;
    }
    if(player1Index === numberOfPlayers) {
        // We've played all the games in this round...
        this._showScores(roundNumber);
        roundFinishedCallback();
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
    this.log("Round: " + roundNumber + ", " + player1Name + " vs. " + player2Name);

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
            // Team 1 has won...
            team1Info.gamesWon += 1;
            team1Info.points += 3;
            team2Info.gamesLost += 1;
        } else if(team2Score > team1Score) {
            // Team 2 has won...
            team2Info.gamesWon += 1;
            team2Info.points += 3;
            team1Info.gamesLost += 1;
        } else {
            // It was a draw...
            team1Info.gamesDrawn += 1;
            team2Info.gamesDrawn += 1;
            team1Info.points += 1;
            team2Info.points += 1;
        }

        // We update the processing time...
        team1Info.processingTimeSeconds += ai1.processingTimeSeconds;
        team2Info.processingTimeSeconds += ai2.processingTimeSeconds;

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
Tournament.prototype._showScores = function(roundNumber) {
    this.log("Scores after round " + roundNumber);
    for(var name in this._scores) {
        var score = this._scores[name];
        var jsonScore = JSON.stringify(score, Utils.decimalPlaceReplacer(4));
        this.log(name + ": " + jsonScore);
    }
};

/**
 * log
 * ---
 * Writes a message to the log file.
 */
Tournament.prototype.log = function(message) {
    fs.writeSync(this._file, message + '\n');
};

// Exports...
module.exports = Tournament;
