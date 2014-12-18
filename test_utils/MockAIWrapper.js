/**
 * AIWrapperMocks
 * --------------
 * Mocks of AIWrapper for testing.
 *
 * Most override the sendData() method so that no data is sent
 * externally to a real AI.
 */
var AIUtilsLib = require('../ai_utils');
var AIWrapper = AIUtilsLib.AIWrapper;
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;

/**
 * @constructor
 */
function MockAIWrapper() {
    // We call the base class constructor...
    AIWrapper.call(this, 'mock-ai');
}
Utils.extend(AIWrapper, MockAIWrapper); // Derives from AIWrapper.

/**
 * sendData
 * --------
 * We override sendData(). No data is sent to an external AI.
 */
MockAIWrapper.prototype.sendData = function(jsonData) {
};


// Exports...
module.exports = MockAIWrapper;

