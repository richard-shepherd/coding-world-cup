/**
 * PlayerState_Intentions
 * ----------------------
 * Holds the current intentions of the player.
 *
 * These include:
 * - Running towards a given point on the pitch.
 * - Tackling or fouling a player.
 *
 * Some actions - such as tackling - can only be initiated if you
 * are within a certain range of their target.
 */

/**
 * @constructor
 */
function PlayerState_Intentions() {
}

// Exports...
module.exports = PlayerState_Intentions;
