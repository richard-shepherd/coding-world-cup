require('../test_utils');
var UtilsLib = require('../utils');
var Position = UtilsLib.Position;
var Utils = UtilsLib.Utils;

/**
 * Tests that the angle between two points (from vertical)
 * is calculated correctly.
 */
exports['Angle between #1'] = function(test) {
    var p1 = new Position(50, 25);
    var p2 = new Position(50, 15);
    var angle = Utils.angleBetween(p1, p2);
    test.approx(angle, 0.0);
    test.done();
};

/**
 * Tests that the angle between two points (from vertical)
 * is calculated correctly.
 */
exports['Angle between #2'] = function(test) {
    var p1 = new Position(50, 25);
    var p2 = new Position(60, 25);
    var angle = Utils.angleBetween(p1, p2);
    test.approx(angle, 90.0);
    test.done();
};

/**
 * Tests that the angle between two points (from vertical)
 * is calculated correctly.
 */
exports['Angle between #3'] = function(test) {
    var p1 = new Position(50, 25);
    var p2 = new Position(50, 45);
    var angle = Utils.angleBetween(p1, p2);
    test.approx(angle, 180.0);
    test.done();
};

/**
 * Tests that the angle between two points (from vertical)
 * is calculated correctly.
 */
exports['Angle between #4'] = function(test) {
    var p1 = new Position(50, 25);
    var p2 = new Position(30, 25);
    var angle = Utils.angleBetween(p1, p2);
    test.approx(angle, 270.0);
    test.done();
};

/**
 * Tests that the angle between two points (from vertical)
 * is calculated correctly.
 */
exports['Angle between #4'] = function(test) {
    var p1 = new Position(50, 25);
    var p2 = new Position(30, 5);
    var angle = Utils.angleBetween(p1, p2);
    test.approx(angle, 315.0);
    test.done();
};

/**
 * Tests that we correctly calculate a unit vector from an angle.
 */
exports['Direction to vector #1'] = function(test) {
    var vector = Utils.vectorFromDirection(0);
    test.approx(vector.x, 0.0);
    test.approx(vector.y, -1.0);
    test.done();
};

/**
 * Tests that we correctly calculate a unit vector from an angle.
 */
exports['Direction to vector #2'] = function(test) {
    var vector = Utils.vectorFromDirection(90);
    test.approx(vector.x, 1.0);
    test.approx(vector.y, 0.0);
    test.done();
};

/**
 * Tests that we correctly calculate a unit vector from an angle.
 */
exports['Direction to vector #3'] = function(test) {
    var vector = Utils.vectorFromDirection(180);
    test.approx(vector.x, 0.0);
    test.approx(vector.y, 1.0);
    test.done();
};

/**
 * Tests that we correctly calculate a unit vector from an angle.
 */
exports['Direction to vector #4'] = function(test) {
    var vector = Utils.vectorFromDirection(270);
    test.approx(vector.x, -1.0);
    test.approx(vector.y, 0.0);
    test.done();
};

/**
 * Tests that we correctly calculate a unit vector from an angle.
 */
exports['Direction to vector #5'] = function(test) {
    var vector = Utils.vectorFromDirection(225);
    var rootHalf = Math.sqrt(0.5);
    test.approx(vector.x, -rootHalf);
    test.approx(vector.y, rootHalf);
    test.done();
};



