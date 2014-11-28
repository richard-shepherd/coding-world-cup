/**
 * Pitch
 * -----
 * Data about the pitch.
 */

/**
 * @constructor
 */
function Pitch() {
    // All measurements in metres...
    this.width = 100.0;
    this.height = 50.0;
    this.goalCentre = this.height / 2.0;
    this.goalY1 = this.goalCentre - 4.0;
    this.goalY2 = this.goalCentre + 4.0;
}


// Exports...
module.exports = Pitch;

