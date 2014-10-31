/**
 * Logger
 * ------
 * The "logger" is really a log-broker.
 *
 * You register log-handler classes with it, which each implement
 * a log(message) method. When you log to this class, it forwards
 * the message to the registered handlers.
 *
 * This class manages log levels, formatting and indenting as well
 * as the collection of log-handlers.
 */

/**
 * @constructor
 */
function Logger() {
}

/**
 * An enum for log levels.
 */
Logger.LogLevel = {
    DEBUG: [0, "Debug"],
    INFO: [1, "Info"],
    INFO_PLUS: [3, "InfoPlus"],
    WARNING: [4, "Warning"],
    ERROR: [5, "Error"],
    FATAL: [6, "Fatal"]
};

// Indent level (sections of the log can be indented)...
Logger._indent_level = 0;

// The collection of log-handlers...
var set = require('collections/set');
Logger._handlers = new set();

/**
 * Adds a log-handler. This is an object implementing:
 * handle_log_message(message, level, indent_level)
 */
Logger.addHandler = function(handler) {
    Logger._handlers.add(handler);
};

/**
 * Removes a log handler.
 */
Logger.removeHandler = function(handler) {
    Logger._handlers.delete(handler);
};

/**
 * Logs the message.
 */
Logger.log = function(message, level) {
    // We find the indent...
    var indent = '';
    if(Logger._indent_level > 0) {
        indent = new Array(Logger._indent_level).join('  ') + '- ';
    }

    // We add the log-level if it is high enough...
    var prefix  = '';
    if(level[0] >= Logger.LogLevel.WARNING[0]) {
        prefix = level[1] + ': ';
    }

    // We log the formatted message...
    var formattedMessage = indent + prefix + message;
    Logger._handlers.forEach(function(handler) {
        handler.log(formattedMessage, level);
    });
};

/**
 * Increases the indent level.
 */
Logger.indent = function() {
    Logger._indent_level += 1;
};

/**
 * Decreases the indent level.
 */
Logger.dedent = function() {
    if(Logger._indent_level > 0) {
        Logger._indent_level -= 1;
    }
};

// Exports...
module.exports = Logger;
