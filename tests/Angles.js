var TestUtilsLib = require('../test_utils');
var UtilsLib = require('../utils');
var Position = UtilsLib.Position;
var Utils = UtilsLib.Utils;

/**
 * Tests that the angle between two points (from vertical)
 * is calculated correctly.
 */
exports['Angle between #1'] = function(test) {
    // p2 is directly above p1...
    var p1 = new Position(50, 25);
    var p2 = new Position(50, 15);
    var angle = Utils.angleBetween(p1, p2);
    test.approx(angle, 0.0);
    test.done();
}



