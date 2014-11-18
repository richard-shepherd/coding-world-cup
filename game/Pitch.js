/**
 * Pitch
 * -----
 * Data about the pitch.
 */

/**
 * @constructor
 */
function Pitch() {
}

/**
 * The width of the pitch in metres.
 */
Pitch.WIDTH = 100.0;

/**
 * The height of the pitch in metres.
 */
Pitch.HEIGHT = 50.0;

/**
 * The position of the 'top' goalpost.
 */
Pitch.GOAL_Y1 = Pitch.HEIGHT/2.0 - 4.0;

/**
 * The position of the 'bottom' goalpost.
 */
Pitch.GOAL_Y2 = Pitch.HEIGHT/2.0 + 4.0;


// Exports...
module.exports = Pitch;

