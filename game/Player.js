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
var Position = UtilsLib.Position;
var Ball = require('./Ball');
var Team = require('./Team');
var TeamState = require('./TeamState');
var GameUtils = require('./GameUtils');


/**
 * @constructor
 */
function Player(playerNumber, playerType, team) {
    // Dynamic state (position etc)...
    this.dynamicState = new PlayerState_Dynamic();

    // Static state (skills, abilities etc)...
    this.staticState = new PlayerState_Static(playerNumber, playerType);

    // The team the player is a member of...
    this._team = team;

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
 * The maximum rate at which players turn, in degrees/second.
 */
Player.MAX_TURNING_RATE = 600.0;

/**
 * The player moves slower when he has the ball.
 */
Player.SPEED_WITH_BALL_FACTOR = 0.4;

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
        this.clearAction();
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
    var scaleFactor = 0.0;
    if(!Utils.approxEqual(distanceToDestination, 0.0)) {
        scaleFactor = distanceToMove / distanceToDestination;
    }
    var scaledVector = vectorToDestination.scale(scaleFactor);

    // We move the player...
    var newPosition = new Position(position.x, position.y);
    newPosition.addVector(scaledVector);

    // If the player has moved into a goal area, we "slide" around it...
    this._slideAroundGoalArea(game.pitch, newPosition);

    // Is the new position a valid one for this player?
    var validPosition = this.validatePosition(newPosition, this._team.getDirection(), false, false);
    if(!validPosition) {
        // The player tried to move to an invalid position...
        this.clearAction();
        return;
    }

    // We update the position...
    position.copyFrom(newPosition);

    // If the player has the ball, we move the ball as well...
    if(this.dynamicState.hasBall) {
        game.ball.state.position.copyFrom(position);
    }

    // If the player is now at the destination, we stop him moving...
    if(position.approxEqual(destination) && resetActionWhenComplete === true) {
        this.clearAction();
    }
};

/**
 * _slideAroundGoalArea
 * --------------------
 * 'Slides' the payer around a goal area, if he has gone into one of them.
 * This preserves the y-coordinate, and changes the x-coordinate so that
 * the point is on the goal-area line.
 */
Player.prototype._slideAroundGoalArea = function(pitch, position) {
    // Only players (not goalkeepers) slide around the area...
    if(this.staticState.playerType === PlayerState_Static.PlayerType.GOALKEEPER) {
        return;
    }

    if(GameUtils.positionIsInLeftHandGoalArea(position)) {
        // The player has moved into the left-hand goal area...
        this._slideAroundGoalArea_Left(pitch, position);
    }

    if(GameUtils.positionIsInRightHandGoalArea(position)) {
        // The player has moved into the right-hand goal area...
        this._slideAroundGoalArea_Right(pitch, position);
    }
};

/**
 * _slideAroundGoalArea_Left
 * -------------------------
 * (See comments for "_slideAroundGoalArea" above.)
 */
Player.prototype._slideAroundGoalArea_Left = function(pitch, position) {
    // We use the circle formula: x2 + y2 = r2, with values relative to the goal-centre.
    var y = position.y - pitch.goalCentre;
    var ySquared = y * y;
    var rSquared = pitch.goalAreaRadius * pitch.goalAreaRadius;
    var xSquared = rSquared - ySquared;
    var x = Math.sqrt(xSquared);
    position.x = x + 0.001; // +0.001 to make sure we are outside the area
};

/**
 * _slideAroundGoalArea_Right
 * --------------------------
 * (See comments for "_slideAroundGoalArea" above.)
 */
Player.prototype._slideAroundGoalArea_Right = function(pitch, position) {
    // We use the circle formula: x2 + y2 = r2, with values relative to the goal-centre.
    var y = position.y - pitch.goalCentre;
    var ySquared = y * y;
    var rSquared = pitch.goalAreaRadius * pitch.goalAreaRadius;
    var xSquared = rSquared - ySquared;
    var x = pitch.width - Math.sqrt(xSquared);
    position.x = x - 0.001; // -0.001 to make sure we are outside the area
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
        this.clearAction();
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
    var maxSkillVariation = (100.0 - this.staticState.kickingAbility) / 100.0 * 360.0;
    var skillVariation = this._random.nextDouble() * maxSkillVariation - maxSkillVariation / 2.0;

    // 2. Angle...
    var differenceInAngle = Math.abs(desiredDirection - dynamicState.direction);
    var maxAngleVariation = differenceInAngle / 180.0 * 90.0;
    var angleVariation = this._random.nextDouble() * maxAngleVariation - maxAngleVariation / 2.0;

    // We add the variations to the requested direction, and convert it to a unit vector...
    var direction = desiredDirection + skillVariation + angleVariation;
    var vector = Utils.vectorFromDirection(direction);

    // We set the ball's vector and speed...
    var ball = game.ball;
    var ballState = ball.state;
    ballState.vector = vector;
    ballState.speed = actionState.kickSpeed / 100.0 * ball.getMaxSpeed();
    ballState.controllingPlayerNumber = -1;

    // We're no longer managing the ball...
    dynamicState.hasBall = false;
    this.clearAction();
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
    // If we already have possession, we clear the action...
    if(this.dynamicState.hasBall) {
        this.clearAction();
        return;
    }

    // We check if the player is close enough to the ball...
    var ball = game.ball;
    var distance = this.getDistanceToBall(ball);
    if(distance > 5.001) {
        // The player is too far from the ball to take possession,
        // so we cancel the action...
        this.clearAction();
        return;
    }

    // We are close enough and the ball is not owned, so we move towards the ball...
    this.actionState.moveDestination.copyFrom(ball.state.position);
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
    var ball = game.ball;
    if(!this.isTakingPossession(ball)) {
        return 0.0;
    }

    // Is the player near enough?
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
Player.prototype.getProbabilityOfSuccessfulTackle = function(game, opponent) {
    // Is this player trying to tackle?
    if(!this.isTackling(game.ball, opponent)) {
        return 0.0;
    }

    // Is the player near enough?
    var position = this.dynamicState.position;
    var opponentPosition = opponent.dynamicState.position;
    var distance = Utils.distanceBetween(position, opponentPosition);
    if(distance > 0.5) {
        return 0.0;
    }

    // The player is near enough to tackle, so we calculate the probability
    // that he gets the ball. This depends on the relative tackling ability
    // of the two players...
    var playerAbility = this.staticState.tacklingAbility;
    var opponentAbility = opponent.staticState.tacklingAbility;
    var totalAbility = playerAbility + opponentAbility;
    var probability = playerAbility / totalAbility;
    return probability;
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

    // If we have the ball, we move slower...
    if(this.dynamicState.hasBall) {
        speed *= Player.SPEED_WITH_BALL_FACTOR;
    }

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
    state.staticState = this.staticState;
    state.dynamicState = this.dynamicState;
    if(!publicOnly) {
        // We want to include the private data as well...
        state.actionState = this.actionState;
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
 * clearAction
 * -----------
 * Sets the action to NONE.
 */
Player.prototype.clearAction = function() {
    this.actionState.action = PlayerState_Action.Action.NONE;
};

/**
 * clearTakePossessionAction
 * -------------------------
 * Sets the action to NONE if it was previously TAKE_POSSESSION.
 */
Player.prototype.clearTakePossessionAction = function() {
    if(this.actionState.action === PlayerState_Action.Action.TAKE_POSSESSION) {
        this.clearAction();
    }
};

/**
 * setPosition
 * -----------
 */
Player.prototype.setPosition = function(x, y) {
    this.dynamicState.position.x = x;
    this.dynamicState.position.y = y;
};

/**
 * setDirection
 * ------------
 */
Player.prototype.setDirection = function(direction) {
    this.dynamicState.direction = direction;
};

/**
 * validatePosition
 * ----------------
 * Returns true if 'position' is a valid one for the player, false if not.
 * The position may be valid under some circumstance, but not others (such as kickoff).
 */
Player.prototype.validatePosition = function(position, playDirection, isKickoff, isMemberOfKickoffTeam) {
    // Is the player on the pitch?
    if(!GameUtils.positionIsOnPitch(position)) {
        return false;
    }

    // We validate a goalkeeper's position differently from a player's...
    switch(this.staticState.playerType) {
        case PlayerState_Static.PlayerType.PLAYER:
            return this._validatePosition_Player(position, playDirection, isKickoff, isMemberOfKickoffTeam);
            break;

        case PlayerState_Static.PlayerType.GOALKEEPER:
            return this._validatePosition_Goalkeeper(position, playDirection);
            break;

        default:
            throw new CWCError('Unexpected playerType');
    }
};

/**
 * _validatePosition_Player
 * ------------------------
 * Validates that 'position is a valid position for a (non-goalkeeper) player.
 */
Player.prototype._validatePosition_Player = function(position, playDirection, isKickoff, isMemberOfKickoffTeam) {
    // The player is not allowed in either of the goal-areas...
    if(GameUtils.positionIsInGoalArea(position)) {
        return false;
    }

    // If we're not at kickoff, then the position is looking good...
    if(isKickoff === false) {
        return true;
    }

    // We are at kickoff, so we need to do some extra checks.

    // Is the player in the right half?
    switch(playDirection) {
        case TeamState.Direction.RIGHT:
            // The player should be in the left side of the pitch...
            if(GameUtils.positionIsOnRightSideOfPitch(position)) {
                return false;
            }
            break;

        case TeamState.Direction.LEFT:
            // The player should be in the right side of the pitch...
            if(GameUtils.positionIsOnLeftSideOfPitch(position)) {
                return false;
            }
            break;

        default:
            throw new CWCError('Unexpected playDirection');
    }

    // If the player is not in the kicking-off team, he is not allowed
    // in the centre circle...
    if(!isMemberOfKickoffTeam && GameUtils.positionIsInCentreCircle(position)) {
        return false;
    }

    // Everything looks OK...
    return true;
};

/**
 * _validatePosition_Goalkeeper
 * ----------------------------
 * Returns true if 'position' is a valid position for the goalkeeper,
 * ie, in his goal-area.
 */
Player.prototype._validatePosition_Goalkeeper = function(position, playDirection) {
    switch(playDirection) {
        case TeamState.Direction.RIGHT:
            return GameUtils.positionIsInLeftHandGoalArea(position);
            break;

        case TeamState.Direction.LEFT:
            return GameUtils.positionIsInRightHandGoalArea(position);
            break;

        default:
            throw new CWCError('Unexpected playDirection');
    }
};

/**
 * isTakingPossession
 * ------------------
 * True if the player is attempting to "take possession", ie trying
 * to get the ball when it is not controlled by any player.
 */
Player.prototype.isTakingPossession = function(ball) {
    // Are we trying to take possession?
    if(this.actionState.action != PlayerState_Action.Action.TAKE_POSSESSION) {
        return false;
    }

    // Is the ball controlled by any player?
    return (ball.state.controllingPlayerNumber === -1);
};

/**
 * isTackling
 * ----------
 * True if the player is tackling, ie trying to take possession when the
 * ball is controlled by another player.
 */
Player.prototype.isTackling = function(ball, opponent) {
    // Are we trying to take possession?
    if(this.actionState.action != PlayerState_Action.Action.TAKE_POSSESSION) {
        return false;
    }

    // The goalkeepers cannot tackle...
    if(this.staticState.playerType === PlayerState_Static.PlayerType.GOALKEEPER) {
        return false;
    }

    // Players cannot tackle the goalkeeper...
    if(opponent.staticState.playerType === PlayerState_Static.PlayerType.GOALKEEPER) {
        return false;
    }

    // Is the ball controlled by any player?
    return (ball.state.controllingPlayerNumber !== -1);
};

// Exports...
module.exports = Player;

