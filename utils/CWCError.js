/**
 * CWCError
 * --------
 * An error class, derived from Error, used when throwing exceptions
 * in coding-world-cup (CWC) code.
 */

/**
 * @constructor
 */
function CWCError(message) {
    Error.call(this, message);
}
CWCError.prototype = new Error(); // Derived from Error.


// Exports...
module.exports = CWCError;
