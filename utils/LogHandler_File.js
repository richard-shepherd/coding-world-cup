/**
 * LogHandler_File
 * ---------------
 * A log-handler for use with the Logger class which logs to a file.
 *
 * Writing is done synchronously.
 */
var fs = require('fs');
var filendir = require('filendir');

/**
 * @constructor
 */
function LogHandler_File(fileName, minimumLogLevel) {
    // We create the file. (The filendir library will create the
    // path to the file if it does not already exist.)
    filendir.ws(fileName, '');
    this._file = fs.openSync(fileName, 'w');
    this._minimumLogLevel = minimumLogLevel;
}

/**
 * Logs the message to the file.
 */
LogHandler_File.prototype.log = function(message, level) {
    if(level[0] >= this._minimumLogLevel[0]) {
        fs.writeSync(this._file, message + '\n');
    }
};

// Exports...
module.exports = LogHandler_File;


