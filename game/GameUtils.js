/**
 * GameUtils
 * ---------
 * Utility functions for the game. These are more specific to the
 * football game than the functions in the Utils library.
 */
var Pitch = require('./Pitch');
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;


// File-scope pitch object...
var pitch = new Pitch();

/**
 * positionIsOnPitch
 * -----------------
 */
function positionIsOnPitch(position) {
    if(position.x < 0.0) return false;
    if(position.x > pitch.width) return false;
    if(position.y < 0.0) return false;
    if(position.y > pitch.height) return false;
    return true;
}

/**
 * positionIsInCentreCircle
 * ------------------------
 */
function positionIsInCentreCircle(position) {
    var distance = Utils.distanceBetween(position, pitch.centreSpot);
    return distance < pitch.centreCircleRadius;
}

/**
 * positionIsInLeftHandGoalArea
 * ----------------------------
 */
function positionIsInLeftHandGoalArea(position) {
    if(position.x < 0) {
        return false;
    }
    var distance = Utils.distanceBetween(position, {x:0, y:pitch.goalCentre});
    return distance < pitch.goalAreaRadius;
}

/**
 * positionIsInRightHandGoalArea
 * -----------------------------
 */
function positionIsInRightHandGoalArea(position) {
    if(position.x > pitch.width) {
        return false;
    }
    var distance = Utils.distanceBetween(position, {x:pitch.width, y:pitch.goalCentre});
    return distance < pitch.goalAreaRadius;
}

/**
 * positionIsInGoalArea
 * --------------------
 */
function positionIsInGoalArea(position) {
    return (positionIsInLeftHandGoalArea(position) || positionIsInRightHandGoalArea(position));
}

/**
 * positionIsOnLeftSideOfPitch
 * ---------------------------
 */
function positionIsOnLeftSideOfPitch(position) {
    return position.x < pitch.centreSpot.x;
}

/**
 * positionIsOnRightSideOfPitch
 * ----------------------------
 */
function positionIsOnRightSideOfPitch(position) {
    return position.x > pitch.centreSpot.x;
}


// Exports...
exports.positionIsOnPitch = positionIsOnPitch;
exports.positionIsInCentreCircle = positionIsInCentreCircle;
exports.positionIsInLeftHandGoalArea = positionIsInLeftHandGoalArea;
exports.positionIsInRightHandGoalArea = positionIsInRightHandGoalArea;
exports.positionIsInGoalArea = positionIsInGoalArea;
exports.positionIsOnLeftSideOfPitch = positionIsOnLeftSideOfPitch;
exports.positionIsOnRightSideOfPitch = positionIsOnRightSideOfPitch;



