/**
 * GSM_BallInGoalArea
 * ------------------
 * Checks for timewasting when the ball is in the goal area.
 *
 * This is derived from GSM_Play. Most of the functionality is
 * inherited from there, with just the timewasting detection in
 * this class.
 */
var GSM_Play = require('./GSM_Play');
var GameUtils = require('./GameUtils');
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;
var Logger = UtilsLib.Logger;


/**
 * @constructor
 */
function GSM_BallInGoalArea(game) {
    // We call the base class constructor...
    GSM_Play.call(this, game);

    // We work out the time limit that the ball is allowed to be in the goal area...
    var now = game.state.currentTimeSeconds;
    this._limit = now + game.goalAreaTimewastingLimitSeconds;
}
Utils.extend(GSM_Play, GSM_BallInGoalArea); // Derived from GSM_Play.
module.exports = GSM_BallInGoalArea;

/**
 * checkState
 * ----------
 * Called by the game loop after updates have been made, to check
 * if we want to change the state.
 */
GSM_BallInGoalArea.prototype.checkState = function() {
    if(!this._game.ballIsInGoalArea()) {
        // The ball is no longer in the goal area, so we switch to the GSM_Play state...
        return new GSM_Play(this._game);
    }

    // The ball is still in the goal area, but has the time limit expired?
    var now = this._game.state.currentTimeSeconds;
    if(now < this._limit) {
        // We are within the time limit, so we carry on...
        return this;
    }

    // The time limit has expired. We teleport the ball outside the goal area...
    var ballPosition = this._game.ball.state.position;
    var pitch = this._game.pitch;
    var centerSpot = pitch.centreSpot;
    var offset = pitch.goalAreaRadius + 5.0;
    if(ballPosition.x < centerSpot.x) {
        ballPosition.x += offset;
    } else {
        ballPosition.x -= offset;
    }

    Logger.log("Ball moved because of timewasting", Logger.LogLevel.WARNING);

    // And switch back to the Play state...
    return new GSM_Play(this._game);
};

