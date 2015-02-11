/**
 * GSM_BallInGoalArea
 * ------------------
 * Checks for timewasting when the ball is in the goal area.
 *
 * This is derived from GSM_Play as most of the
 */

var GSM_Play = require('./GSM_Play');

/**
 * @constructor
 */
function GSM_BallInGoalArea(game) {

}
Utils.extend(GSM_Play, GSM_BallInGoalArea); // Derived from GSM_Play.
module.exports = GSM_BallInGoalArea;
