/**
 * Logger
 * ------
 * This "logger" is really a log-broker.
 *
 * You register logging classes with it, which each implement
 * a log(message) method. When you log to this class, it forwards
 * the message to the registered brokers.
 *
 * This class manages log levels, formatting and indenting as well
 * as the collection of loggers.
 */

/**
 * Constructor.
 */
Logger = new function() {
};

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

// Sections of the log can be indented...
Logger._indent_level = 0;

// The collection of loggers...




// Exports...
exports.Logger = Logger;
