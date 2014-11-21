/**
 * CWCError
 * --------
 * An error class, derived from Error, used when throwing exceptions
 * in coding-world-cup (CWC) code.
 */
var Utils = require('./Utils');


/**
 * @constructor
 */
function CWCError(message) {
    Error.call(this, message);
}
Utils.extend(Error, CWCError); // Derived from Error.


// Exports...
module.exports = CWCError;
