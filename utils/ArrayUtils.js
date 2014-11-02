/**
 * ArrayUtils
 * ----------
 * Adds some helper functions to the Array class.
 */

/**
 * countIf
 * -------
 * Returns the number of items in the array which match the condition,
 * which should be a function like:
 *   function(item) { return item === whatever; }
 */
Array.prototype.countIf = function(condition) {
    return this.reduce(function(count, item) {
        return condition(item) ? count+1 : count;
    }, 0);
};
