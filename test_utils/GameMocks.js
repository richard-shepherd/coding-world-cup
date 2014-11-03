/**
 * GameMocks
 * ---------
 * Mocks of the Game object for various purposes.
 */

/**
 * MockGame_CalculationInterval
 * ----------------------------
 * A mock that lets you set the interval since the previous calculation.
 * @constructor
 */
function MockGame_CalculationInterval(interval) {
    this._interval = interval;
}
MockGame_CalculationInterval.prototype.getCalculationInterval = function() {
    return this._interval;
};


// Exports...
exports.MockGame_CalculationInterval = MockGame_CalculationInterval;


