/**
 * ConsoleLogHandler
 * -----------------
 * Logs messages to the console.
 *
 * This is a log-handler class which can be registered
 * with the Logger.
 *
 * @constructor
 */
function ConsoleLogHandler(minimumLogLevel) {
    this._minimumLogLevel = minimumLogLevel;
}

/**
 * Logs to the console.
 */
ConsoleLogHandler.prototype.log = function(message, level) {
    if(level[0] >= this._minimumLogLevel[0]) {
        console.log(message);
    }
};

// Exports...
module.exports = ConsoleLogHandler;