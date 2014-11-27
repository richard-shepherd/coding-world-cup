/**
 * Player
 * ------
 * Information about one player and methods to control them.
 *
 * The information includes dynamic data such as the player's current
 * position and speed, as well as more static / config data such as
 * the player's skills and abilities.
 *
 * Most, but not all, of the data is serialized and passed to AIs
 * each turn of the game. Some data - in particular the player's
 * actions are private, and are only available to the AI which
 * is controlling the player.
 */
var PlayerState_Dynamic = require('./PlayerState_Dynamic');
var PlayerState_Static = require('./PlayerState_Static');
var PlayerState_Action = require('./PlayerState_Action');
var UtilsLib = require('../utils');
var Utils = UtilsLib.Utils;
var CWCError = UtilsLib.CWCError;
var Random = UtilsLib.Random;
var Ball = require('./Ball');
var Team = require('./Team');


/**
 * @constructor
 */
function Player(playerNumber, playerType) {
    // Dynamic state (position etc)...
    this.dynamicState = new PlayerState_Dynamic();

    // Static state (skills, abilities etc)...
    this.staticState = new PlayerState_Static(playerNumber, playerType);

    // Current action (moving, kicking etc)...
    this.actionState = new PlayerState_Action();

    // Generates random numbers for some actions...
    this._random = new Random();
}

/**
 * Maximum running speed, in metres/second.
 * If a player has runningAbility of 100.0 and chooses to run at
 * 100% speed, they will run at this rate.
 */
Player.MAX_SPEED = 10.0;

/**
 * The maximum energy that any player can have. All players start with
 * this energy. (Though players recuperate at different rates depending
 * on their stamina.)
 */
Player.MAX_ENERGY = 100.0;

/**
 * The maximum rate at which players turn, in degrees/second.
 */
Player.MAX_TURNING_RATE = 600.0;

/**
 * isPlayer
 * --------
 * Returns true if this player is a player (ie, not a goalkeeper).
 */
Player.prototype.isPlayer = function() {
    return this.staticState.playerType === PlayerState_Static.PlayerType.PLAYER;
};

/**
 * isGoalkeeper
 * ------------
 * Returns true if this player is a goalkeeper.
 */
Player.prototype.isGoalkeeper = function() {
    return this.staticState.playerType === PlayerState_Static.PlayerType.GOALKEEPER;
};

/**
 * processAction
 * -------------
 * Processes the current action for this player, including moving,
 * turning, kicking the ball etc.
 */
Player.prototype.processAction = function(game) {
    // Is there a current action for this player?
    var action = this.actionState.action;
    if(action === PlayerState_Action.Action.NONE) {
        return;
    }

    // We call the function for this action. They have names like
    //     _processAction_MOVE(game, resetWhenComplete)
    var functionName = '_processAction_' + this.actionState.action;
    this[functionName](game, true);
};

/**
 * _processAction_TURN
 * -------------------
 * Turns the player towards their desired direction.
 */
Player.prototype._processAction_TURN = function(game, resetActionWhenComplete) {
    // We work out whether we should be turning left or right...
    var currentDirection = this.dynamicState.direction;
    var desiredDirection = this.actionState.direction;

    var angleToTurn = desiredDirection - currentDirection;
    if(angleToTurn > 180) {
        // We are turning more than 180 degrees to the right,
        // so this is really a turn to the left...
        angleToTurn = angleToTurn - 360;
    }
    if(angleToTurn < -180) {
        // We are turning more than 180 degrees to the left,
        // so this is really a turn to the right...
        angleToTurn = 360 + angleToTurn;
    }

    // We change this to an abs(angle) and a direction...
    var directionToTurn = 1.0;
    if(angleToTurn < 0) {
        angleToTurn = -1.0 * angleToTurn;
        directionToTurn = -1.0;
    }

    // We find the maximum angle that can be turned in the interval
    // since the last update. We may need to cap the angle we move...
    var maxAngle = Player.MAX_TURNING_RATE * game.getCalculationIntervalSeconds();
    if(angleToTurn > maxAngle) {
        angleToTurn = maxAngle;
    }

    // We turn by the amount, and check if we've gone past 360 degrees...
    var newDirection = currentDirection + angleToTurn * directionToTurn;
    if(newDirection > 360.0) {
        newDirection -= 360.0;
    }
    if(newDirection < 0) {
        newDirection += 360.0;
    }

    // We set the new direction...
    this.dynamicState.direction = newDirection;

    // If we are now facing in the desired direction, we stop turning...
    if(Utils.approxEqual(newDirection, desiredDirection) && resetActionWhenComplete === true) {
        this.actionState.action = PlayerState_Action.Action.NONE;
    }
};

/**
 * _processAction_MOVE
 * -------------------
 * Moves the player towards their desired position.
 */
Player.prototype._processAction_MOVE = function(game, resetActionWhenComplete) {
    var position = this.dynamicState.position;
    var destination = this.actionState.moveDestination;

    // We check if the player is facing the right way...
    var currentDirection = this.dynamicState.direction;
    var directionToDestination = Utils.angleBetween(position, destination);
    if(!Utils.approxEqual(currentDirection, directionToDestination)) {
        // We are not currently facing the right way, so we turn first...
        this.actionState.direction = directionToDestination;
        this._processAction_TURN(game, false);
        return;
    }

    // We are facing the right direction, so we can move towards
    // the destination at the player's current speed...
    var distanceToDestination = position.distanceTo(destination);
    var distanceToMove = this.getSpeed() * game.getCalculationIntervalSeconds();
    if(distanceToDestination < distanceToMove) {
        distanceToMove = distanceToDestination;
    }

    // We find the vector to the destination, and scale it by the
    // distance to move...
    var vectorToDestination = position.vectorTo(destination);
    if(Utils.approxEqual(distanceToDestination, 0.0)) {
        var scaleFactor = 0.0;
    } else {
        var scaleFactor = distanceToMove / distanceToDestination;
    }
    var scaledVector = vectorToDestination.scale(scaleFactor);

    // We move the player...
    position.addVector(scaledVector);

    // If the player is now at the destination, we stop him moving...
    if(position.approxEqual(destination) && resetActionWhenComplete === true) {
        this.actionState.action = PlayerState_Action.Action.NONE;
    }
};

/**
 * _processAction_KICK
 * -------------------
 * The player kicks the ball in the desired direction.
 * How accurate the kick is depends on the passing-ability of the player.
 */
Player.prototype._processAction_KICK = function(game, resetActionWhenComplete) {
    var dynamicState = this.dynamicState;
    if(dynamicState.hasBall === false) {
        // The player does not have the ball, so can't kick it...
        this.actionState.action = PlayerState_Action.Action.NONE;
        return;
    }

    // We find the direction to the desired destination for the ball...
    var position = dynamicState.position;
    var actionState = this.actionState;
    var desiredBallDestination = actionState.kickDestination;
    var desiredDirection = Utils.angleBetween(position, desiredBallDestination);

    // The player may not kick the ball in exactly the direction requested.
    // This depends on the angle to the destination and the skill of the player.
    //
    // 1. Skill of player
    // ------------------
    // If the player has 100% passing-ability, we have zero variation
    // If the player has 0% passing-ability we have up to 360-degrees of variation
    // The actual variation is a random number up to the maximum variation.
    //
    // 2. Angle to destination
    // -----------------------
    // We find the difference in angle between the angle-to-ball-destination
    // and the angle the player is currently facing.
    // If there is 0 difference, we have zero variation.
    // If there is 180-degrees difference, we have up to 90-degrees of variation.
    // The actual variation is a random number up to the maximum variation.

    // 1. Skill...
    var maxSkillVariation = (100.0 - this.staticState.passingAbility) / 100.0 * 360.0;
    var skillVariation = this._random.nextDouble() * maxSkillVariation - maxSkillVariation / 2.0;

    // 2. Angle...
    var differenceInAngle = Math.abs(desiredDirection - dynamicState.direction);
    var maxAngleVariation = differenceInAngle / 180.0 * 90.0;
    var angleVariation = this._random.nextDouble() * maxAngleVariation - maxAngleVariation / 2.0;

    // We add the variations to the requested direction, and convert it to a unit vector...
    var direction = desiredDirection + skillVariation + angleVariation;
    var vector = Utils.vectorFromDirection(direction);

    // We set the ball's vector and speed, and update its position...
    var ball = game.ball;
    var ballState = ball.state;
    ballState.vector = vector;
    ballState.speed = actionState.kickSpeed / 100.0 * ball.getMaxSpeed();
    ballState.controllingPlayerNumber = -1;
    ball.updatePosition(game);

    // We're no longer managing the ball...
    dynamicState.hasBall = false;
    actionState.action = PlayerState_Action.Action.NONE;
};

/**
 * _processAction_TAKE_POSSESSION
 * ------------------------------
 * The player can attempt to take possession of the ball if he is close
 * enough to it.
 *
 * This action only *moves* the player towards the ball (at full speed).
 *
 * Taking possession itself is managed by the Game, as there may be multiple
 * players trying to take possession at the same time.
 */
Player.prototype._processAction_TAKE_POSSESSION = function(game, resetActionWhenComplete) {
    // We check if a player (including ourself) already has possession...
    var ballState = game.ball.state;
    if(ballState.controllingPlayerNumber !== -1) {
        // The ball is already in a player's possession...
        this.actionState.action = PlayerState_Action.Action.NONE;
        return;
    }

    // We check if the player is close enough to the ball...
    var distance = this.getDistanceToBall(game.ball);
    if(distance > 5.0) {
        // The player is too far from the ball to take possession,
        // so we cancel the action...
        this.actionState.action = PlayerState_Action.Action.NONE;
        return;
    }

    // We are close enough and the ball is not owned, so we move towards
    // the ball...
    this.actionState.moveDestination.copyFrom(ballState.position);
    this.actionState.moveSpeed = 100.0;
    this._processAction_MOVE(game, false);
};

/**
 * _processAction_TACKLE
 * ---------------------
 * The player just moves towards the player to tackle when processing this
 * action. The decision about who gets the ball is taken by the Game.
 */
Player.prototype._processAction_TACKLE = function(game, resetActionWhenComplete) {
    // Is the target player in the same team?
    var playerNumber = this.staticState.playerNumber;
    var otherPlayerNumber = this.actionState.tacklePlayerNumber;
    if((playerNumber <= Team.NUMBER_OF_PLAYERS && otherPlayerNumber <= Team.NUMBER_OF_PLAYERS) ||
        (playerNumber > Team.NUMBER_OF_PLAYERS && otherPlayerNumber > Team.NUMBER_OF_PLAYERS)) {
        // The players are on the same team...
        this.actionState.action = PlayerState_Action.Action.NONE;
        return;
    }

    // Does the other player have the ball?
    var otherPlayer = game.getPlayer(otherPlayerNumber);
    var otherPlayerDynamicState = otherPlayer.dynamicState;
    if(otherPlayerDynamicState.hasBall === false) {
        // The other player does not have the ball...
        this.actionState.action = PlayerState_Action.Action.NONE;
        return;
    }

    // You cannot tackle the goalkeeper...
    if(otherPlayer.isGoalkeeper()) {
        this.actionState.action = PlayerState_Action.Action.NONE;
        return;
    }

    // The goalkeeper cannot tackle another player...
    if(this.isGoalkeeper()) {
        this.actionState.action = PlayerState_Action.Action.NONE;
        return;
    }

    // Is the other player close enough?
    var position = this.dynamicState.position;
    var otherPlayerPosition = otherPlayerDynamicState.position;
    var distance = Utils.distanceBetween(position, otherPlayerPosition);
    if(distance > 5.0) {
        // The player is too far away to tackle...
        this.actionState.action = PlayerState_Action.Action.NONE;
        return;
    }

    // We move towards the other player...
    this.actionState.moveDestination.copyFrom(otherPlayerPosition);
    this.actionState.moveSpeed = 100.0;
    this._processAction_MOVE(game, false);
};

/**
 * getProbabilityOfTakingPossession
 * --------------------------------
 * Returns the probability of this player taking possession of the ball.
 */
Player.prototype.getProbabilityOfTakingPossession = function(game) {
    // Is this player trying to take possession?
    if(this.actionState.action !== PlayerState_Action.Action.TAKE_POSSESSION) {
        return 0.0;
    }

    // Is the player near enough?
    var ball = game.ball;
    var distance = this.getDistanceToBall(ball);
    if(distance > 0.5) {
        return 0.0;
    }

    // The player is close enough and wants to take possession. The probability
    // depends on:
    // - How close he is to the ball.
    // - His ball-control ability.
    // - The speed the ball is travelling
    //
    // This is calculated as 1 - probability-of-failing
    //
    // probability-of-failing is higher when:
    // - The distance is greater
    // - The ball is moving faster
    // - The player's ability is low
    var distanceFactor = distance / 0.5;
    var ballControlFactor = 1.0 - this.staticState.ballControlAbility / 100.0;
    var speedFactor = ball.state.speed / Ball.MAX_SPEED;
    var probabilityOfFailing = distanceFactor * ballControlFactor * speedFactor;
    return 1.0 - probabilityOfFailing;
};

/**
 * getProbabilityOfSuccessfulTackle
 * --------------------------------
 * Returns the probability that this player gets the ball by tackling
 * the currently selected target player.
 */
Player.prototype.getProbabilityOfSuccessfulTackle = function(game, playerWithBall) {
    // Is this player trying to tackle?
    var actionState = this.actionState;
    if(actionState.action !== PlayerState_Action.Action.TACKLE) {
        return 0.0;
    }

    // Is the player near enough?
    var position = this.dynamicState.position;
    var playerWithBallPosition = playerWithBall.dynamicState.position;
    var distance = Utils.distanceBetween(position, playerWithBallPosition);
    if(distance > 0.5) {
        return 0.0;
    }

    // TODO: Write this!
    return 0.0;
};

/**
 * getDistanceToBall
 * -----------------
 */
Player.prototype.getDistanceToBall = function(ball) {
    var playerPosition = this.dynamicState.position;
    var ballPosition = ball.state.position;
    var distance = Utils.distanceBetween(playerPosition, ballPosition);
    return distance;
};

/**
 * getSpeed
 * --------
 * Returns the current speed the player will move at in m/s.
 * This is a function of the player's max speed and current energy.
 */
Player.prototype.getSpeed = function() {
    var runningAbility = this.staticState.runningAbility / 100.0;
    var energy = this.dynamicState.energy / 100.0;
    var speed = runningAbility * energy * Player.MAX_SPEED;
    return speed;
};

/**
 * getDTO
 * --------------
 * Returns an object holding the player's state.
 *
 * If publicOnly is true, then only the dynamic state is
 * returned. If false, all the state is returned.
 */
Player.prototype.getDTO = function(publicOnly) {
    var state = {};
    state.dynamic = this.dynamicState;
    state.config = this.staticState;
    if(!publicOnly) {
        // We want to include the private jsonData as well...
        state.action = this.actionState;
    }
    return state;
};

/**
 * getPlayerNumber
 * ---------------
 * Helper function to get the player number.
 */
Player.prototype.getPlayerNumber = function() {
    return this.staticState.playerNumber;
};

/**
 * setAction
 * ---------
 * Sets the current action from the data passed in (which usually
 * originated from an AI).
 */
Player.prototype.setAction = function(action) {
    // The action should have an "action" member specifying
    // which action to perform. We look for a function called
    // _setAction_[action] to parse the specific parameters for
    // this action...
    if(!('action' in action)) {
        throw new CWCError('Expected "action" field in response');
    }
    var setActionMethodName = '_setAction_' + action.action;
    if(!(setActionMethodName in this)) {
        throw new CWCError('No method found to process action: ' + action.action);
    }
    this[setActionMethodName](action);
};

/**
 * _setAction_MOVE
 * ---------------
 * Processes a MOVE action.
 */
Player.prototype._setAction_MOVE = function(action) {
    // We expect the action to have "destination" and "speed" fields...
    if(!('destination' in action)) {
        throw new CWCError('Expected "destination" field in MOVE action');
    }
    if(!('speed' in action)) {
        throw new CWCError('Expected "speed" field in MOVE action');
    }
    this.actionState.action = PlayerState_Action.Action.MOVE;
    this.actionState.moveDestination.copyFrom(action.destination);
    this.actionState.moveSpeed = action.speed;
};

/**
 * _setAction_TURN
 * ---------------
 * Processes a TURN action.
 */
Player.prototype._setAction_TURN = function(action) {
    // We expect the action to have a "direction" field...
    if(!('direction' in action)) {
        throw new CWCError('Expected "direction" field in TURN action');
    }
    this.actionState.action = PlayerState_Action.Action.TURN;
    this.actionState.direction = action.direction;
};

/**
 * _setAction_KICK
 * ---------------
 * Sets the action to kick the ball.
 */
Player.prototype._setAction_KICK = function(action) {
    // We expect the action to have "destination" and "speed" fields...
    if(!('destination' in action)) {
        throw new CWCError('Expected "destination" field in KICK action');
    }
    if(!('speed' in action)) {
        throw new CWCError('Expected "speed" field in KICK action');
    }
    this.actionState.action = PlayerState_Action.Action.KICK;
    this.actionState.kickDestination.copyFrom(action.destination);
    this.actionState.kickSpeed = action.speed;
};

/**
 * _setAction_TAKE_POSSESSION
 * --------------------------
 * Sets the action to take possession of the ball when it is nearby.
 */
Player.prototype._setAction_TAKE_POSSESSION = function(action) {
    this.actionState.action = PlayerState_Action.Action.TAKE_POSSESSION;
};

/**
 * _setAction_TACKLE
 * -----------------
 * Sets the action to tackle another player.
 */
Player.prototype._setAction_TACKLE = function(action) {
    // We expect the action to have "player" and "strength" fields...
    if(!('player' in action)) {
        throw new CWCError('Expected "player" field in TACKLE action');
    }
    if(!('strength' in action)) {
        throw new CWCError('Expected "strength" field in TACKLE action');
    }
    this.actionState.action = PlayerState_Action.Action.TACKLE;
    this.actionState.tacklePlayerNumber = action.player;
    this.actionState.tackleStrength = action.strength;
};


// Exports...
module.exports = Player;

