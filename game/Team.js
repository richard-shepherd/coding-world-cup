/**
 * Team
 * ----
 * Manages one team, including the collection of players in it.
 */
var TeamState = require('./TeamState');
var UtilsLib = require('../utils');
var Logger = UtilsLib.Logger;
var CWCError = UtilsLib.CWCError;
var Utils = UtilsLib.Utils;
var AIUtilsLib = require('../ai_utils');
var MessageUtils = AIUtilsLib.MessageUtils;
var PlayerState_Static = require('./PlayerState_Static');
var GameUtils = require('./GameUtils');


/**
 * @constructor
 */
function Team(ai, teamNumber) {
    // The team's number (1 or 2)...
    this._teamNumber = teamNumber;

    // The AIWrapper for the AI managing this team...
    this._ai = ai;

    // The team state (score etc)...
    this.state = new TeamState();

    // The collection of _players.
    // (The player objects are created in the Game, and passed to us later)...
    this._players = [];
}

// The number of _players on each team (not including the goalkeeper)...
Team.NUMBER_OF_PLAYERS = 5;

/**
 * addPlayer
 * ---------
 * Adds a player to this team.
 */
Team.prototype.addPlayer = function(player) {
    this._players.push(player);
};

/**
 * getTeamNumber
 * -------------
 */
Team.prototype.getTeamNumber = function() {
    return this._teamNumber;
};

/**
 * getDirection
 * ------------
 */
Team.prototype.getDirection = function() {
    return this.state.direction;
};

/**
 * setDirection
 * ------------
 * Sets the direction the team is playing.
 */
Team.prototype.setDirection = function(direction) {
    this.state.direction = direction;
};

/**
 * processActions
 * --------------
 * Processes the actions (moving, kicking etc) for each player
 * in the team.
 */
Team.prototype.processActions = function(game) {
    this._players.forEach(function(player){
        player.processAction(game);
    }, this);
};

/**
 * getDTO
 * ------
 * Returns the state of this object and its players.
 *
 * If publicOnly is true, then only public (dynamic) data about
 * the players will be included.
 */
Team.prototype.getDTO = function(publicOnly) {
    var state = {};
    state.team = this.state;
    state.players = this._players.map(function(player) {
        return player.getDTO(publicOnly);
    });
    return state;
};

/**
 * Returns the AI (wrapper) for this team.
 */
Team.prototype.getAI = function() {
    return this._ai;
};

/**
 * getName
 * -------
 * Helper function to get the team's name.
 */
Team.prototype.getName = function() {
    return this.state.name;
};

/**
 * processPlayResponse
 * -------------------
 * Processes a response from an AI for general 'play' updates
 * involving moving of players, kicking and so on.
 */
Team.prototype.processPlayResponse = function(data) {
    // The data should have an 'actions' section...
    if(!('actions' in data)) {
        throw new CWCError('Expected an "actions" array in response.');
    }

    // We process each action...
    data.actions.forEach(function(action) {
        var player = this._getPlayer(action.playerNumber);
        player.setAction(action);
    }, this);
};

/**
 * _getPlayer
 * ----------
 * Returns the Player object for the player-number passed in.
 * Throws an exception if the player is not part of this team.
 */
Team.prototype._getPlayer = function(playerNumber) {
    // The player numbers are consecutive, so we can do a bit
    // of index arithmetic to find the player for the number
    // passed in...
    var players = this._players;
    if(players.length === 0) {
        throw new CWCError('No players in team.');
    }

    var firstPlayerNumber = players[0].getPlayerNumber();
    var index = playerNumber - firstPlayerNumber;
    if(index >= players.length) {
        throw new CWCError('Player not found: ' + playerNumber);
    }
    return players[index];
};

/**
 * sendEvent_TeamInfo
 * ------------------
 * Sends information about this team to the team's AI.
 */
Team.prototype.sendEvent_TeamInfo = function() {
    // We create the info for the event...
    var event = {
        eventType:"TEAM_INFO",
        teamNumber:this._teamNumber,
        players:[]
    };

    // Add the players...
    this._players.forEach(function(player) {
        var playerInfo = {
            playerNumber:player.staticState.playerNumber,
            playerType:player.staticState.playerType
        };
        event.players.push(playerInfo);
    }, this);

    // And send it to the team's AI...
    var jsonEvent = MessageUtils.getEventJSON(event);
    this._ai.sendData(jsonEvent);

    Logger.log('SENT EVENT: ' + jsonEvent, Logger.LogLevel.DEBUG);

};

/**
 * setDefaultKickoffPositions
 * --------------------------
 * Sets the kickoff positions to defaults used if the AI does
 * not specify valid positions.
 */
Team.prototype.setDefaultKickoffPositions = function(pitch) {
    var playerX = 0.0;
    var playerY = pitch.goalCentre;
    var goalkeeperX = 0.0;
    var direction = 0.0;

    // Is the team playing right or left?
    switch(this.state.direction) {
        case TeamState.Direction.LEFT:
            playerX = pitch.width * 0.75;
            goalkeeperX = pitch.width - 0.5;
            direction = 270.0;
            break;

        case TeamState.Direction.RIGHT:
            playerX = pitch.width * 0.25;
            goalkeeperX = 0.5;
            direction = 90.0;
            break;

        default:
            throw new CWCError('Unexpected team direction');
    }

    // We set the positions for each player...
    this._players.forEach(function(player) {

        // We set the direction the player is facing...
        player.setDirection(direction);

        // The position of the player depends on whether he is a
        // player or the goalkeeper...
        switch(player.staticState.playerType) {

            case PlayerState_Static.PlayerType.PLAYER:
                player.setPosition(playerX, playerY);
                break;

            case PlayerState_Static.PlayerType.GOALKEEPER:
                player.setPosition(goalkeeperX, playerY);
                break;
        }
    });
};

/**
 * processKickoffResponse
 * ----------------------
 * Updates player positions in preparation for kickoff.
 *
 * 'isTeamKickingOff' is true if this team is the one kicking off,
 * false if not.
 */
Team.prototype.processKickoffResponse = function(game, data, isTeamKickingOff) {
    // We're expecting a 'players' field...
    if(!('players' in data)) {
        throw new CWCError('Expected a "players" field in KICKOFF response');
    }

    // We set the players' positions...
    this.processKickoffResponse_PositionPlayers(data, isTeamKickingOff);

    // We make sure that a player is in the centre spot, and has the ball...
    this.processKickoffResponse_GiveBallToCentrePlayer(game, isTeamKickingOff);
};

/**
 * processKickoffResponse_GiveBallToCentrePlayer
 * ---------------------------------------------
 * We find the player nearest the centre spot, put him on the centre spot
 * and give him the ball.
 */
Team.prototype.processKickoffResponse_GiveBallToCentrePlayer = function(game, isTeamKickingOff) {
    // We only put a player on the centre spot if we are the team kicking off...
    if(!isTeamKickingOff) {
        return;
    }

    // We find the player closest to the centre...
    var closestPlayer = null;
    var closestDistance = 1000.0;
    var centreSpot = game.pitch.centreSpot;
    this._players.forEach(function(player) {
        // We don't move the goalkeeper...
        if(player.isGoalkeeper()) {
            return;
        }

        // We find the distance to the centre spot for this player...
        var distance = Utils.distanceBetween(player.dynamicState.position, centreSpot);
        if(distance < closestDistance) {
            // This player is the closest we've seen so far...
            closestPlayer = player;
            closestDistance = distance;
        }
    }, this);

    // We move the closest player to the centre spot and give him the ball...
    closestPlayer.dynamicState.position.copyFrom(centreSpot);
    game.giveBallToPlayer(closestPlayer);
};

/**
 * processKickoffResponse_PositionPlayers
 * --------------------------------------
 * Puts players in the positions specified by 'data'.
 */
Team.prototype.processKickoffResponse_PositionPlayers = function(data, isTeamKickingOff) {
    // A maximum of two players are allowed in the centre circle...
    var numberPlayersInCentreCircle = 0;

    // We update the position for each player...
    data.players.forEach(function(playerInfo) {
        // There should be a position and direction for each player...
        if(!('playerNumber' in playerInfo)) {
            throw new CWCError('Expected a "playerNumber" field in KICKOFF response');
        }
        if(!('position' in playerInfo)) {
            throw new CWCError('Expected a "position" field in KICKOFF response');
        }
        if(!('direction' in playerInfo)) {
            throw new CWCError('Expected a "direction" field in KICKOFF response');
        }

        // We validate the position...
        var player = this._getPlayer(playerInfo.playerNumber);
        var isValidPosition = player.validatePosition(
            playerInfo.position,
            this.state.direction,
            true, // isKickoff
            isTeamKickingOff);
        if(!isValidPosition) {
            return;
        }

        // We check that there are not too many players in the centre circle...
        var playerInCentreCircle = GameUtils.positionIsInCentreCircle(playerInfo.position);
        if(playerInCentreCircle && numberPlayersInCentreCircle >= 2) {
            return;
        }

        // We set the player's position and direction...
        player.setPosition(playerInfo.position.x, playerInfo.position.y);
        player.setDirection(playerInfo.direction);
        numberPlayersInCentreCircle += playerInCentreCircle ? 1 : 0;
    }, this);
};

/**
 * processConfigureAbilitiesResponse
 * ---------------------------------
 * Assigns abilities to player. The team cannot have more than
 * 'maxTotalAbility' in any one category.
 */
Team.prototype.processConfigureAbilitiesResponse = function(data, maxTotalAbility) {
    // We're expecting a 'players' field...
    if(!('players' in data)) {
        throw new CWCError('Expected a "players" field in KICKOFF response');
    }

    // We keep a note of the total remaining ability in each category, to
    // make sure that the team cannot be allocated more than this...
    var remainingKickingAbility = maxTotalAbility;
    var remainingRunningAbility = maxTotalAbility;
    var remainingBallControlAbility = maxTotalAbility;
    var remainingTacklingAbility = maxTotalAbility;

    // We update the abilities for each player...
    data.players.forEach(function(playerInfo) {
        // We check the fields...
        if(!('playerNumber' in playerInfo)) {
            throw new CWCError('Expected a "playerNumber" field in CONFIGURE_ABILITIES response');
        }
        if(!('kickingAbility' in playerInfo)) {
            throw new CWCError('Expected a "kickingAbility" field in CONFIGURE_ABILITIES response');
        }
        if(!('runningAbility' in playerInfo)) {
            throw new CWCError('Expected a "runningAbility" field in CONFIGURE_ABILITIES response');
        }
        if(!('ballControlAbility' in playerInfo)) {
            throw new CWCError('Expected a "ballControlAbility" field in CONFIGURE_ABILITIES response');
        }
        if(!('tacklingAbility' in playerInfo)) {
            throw new CWCError('Expected a "tacklingAbility" field in CONFIGURE_ABILITIES response');
        }

        // We get the player, and set his abilities...
        var player = this._getPlayer(playerInfo.playerNumber);
        var kickingAbility = (playerInfo.kickingAbility <= remainingKickingAbility) ? playerInfo.kickingAbility : remainingKickingAbility;
        var runningAbility = (playerInfo.runningAbility <= remainingRunningAbility) ? playerInfo.runningAbility : remainingRunningAbility;
        var ballControlAbility = (playerInfo.ballControlAbility <= remainingBallControlAbility) ? playerInfo.ballControlAbility : remainingBallControlAbility;
        var tacklingAbility = (playerInfo.tacklingAbility <= remainingTacklingAbility) ? playerInfo.tacklingAbility : remainingTacklingAbility;

        player.staticState.kickingAbility = kickingAbility;
        player.staticState.runningAbility = runningAbility;
        player.staticState.ballControlAbility = ballControlAbility;
        player.staticState.tacklingAbility = tacklingAbility;

        remainingKickingAbility -= kickingAbility;
        remainingRunningAbility -= runningAbility;
        remainingBallControlAbility -= ballControlAbility;
        remainingTacklingAbility -= tacklingAbility;

    }, this);
};

// Exports...
module.exports = Team;