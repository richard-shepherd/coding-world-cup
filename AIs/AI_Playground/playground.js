/**
 * Playground
 * ----------
 * Plays football like children in a playground.
 *
 * All players (except the goalkeeper) run for the ball. If they get it
 * they kick it towards the goal.
 *
 * The goalkeeper tries to save the ball.
 */
var readline = require('readline');


var playGround = new Playground();

/**
 * @constructor
 */
function Playground() {
    // We hook up stdin and stdout, and register onGameData
    // to be called when data is received over stdin...
    this._io = readline.createInterface({
        input: process.stdin,
        output: process.stdout
    });
    var that = this;
    this._io.on('line', function(line){
        that.onGameData(line);
    });

    // Data about the pitch (width, height, position of goals)...
    this._pitch = {width:0, height:0};

    // The direction we're playing...
    this._direction = "";

    // The collection of players {playerNumber, playerType}...
    this._players = [];

    // The game state for the current turn...
    this._gameState = {};

    // The last time (in game-time) that we changed the movement of
    // players. We only update every few seconds...
    this._lastTimeWeChangedMovements = -1.0;
    this._changeInterval = 10.0;
}

/**
 * onGameData
 * ----------
 * Called when the game sends data to us.
 */
Playground.prototype.onGameData = function(jsonData) {
    // We get the object...
    var data = JSON.parse(jsonData);

    // And call a function to handle it...
    var messageHandler = '_on' + data.messageType;
    this[messageHandler](data);
};

/**
 * _onEVENT
 * --------
 * Called when we receive an event message.
 */
Playground.prototype._onEVENT = function(data) {
    // We call different functions depending on the event...
    var eventHandler = '_onEVENT_' + data.event;
    this[eventHandler](data);
};

/**
 * _onREQUEST
 * ----------
 * Called when we receive a request message.
 */
Playground.prototype._onREQUEST = function(data) {
    // We call different functions depending on the request...
    var requestHandler = '_onREQUEST_' + data.request;
    this[requestHandler](data);
};

/**
 * _onEVENT_GAME_START
 * -------------------
 * Called at the start of a game with general game info.
 */
Playground.prototype._onEVENT_GAME_START = function(data) {
    // We reset the last update time...
    this._lastTimeWeChangedMovements = -1.0;

    // We get data about the pitch...
    this._pitch = data.pitch;
};

/**
 * _onEVENT_TEAM_INFO
 * ------------------
 * Called when we receive the TEAM_INFO event.
 */
Playground.prototype._onEVENT_TEAM_INFO = function(data) {
    // We store our team number, direction and player info...
    this._teamNumber = data.teamNumber;
    this._direction = data.direction;
    this._players = data.players;
};

/**
 * _onEVENT_START_OF_TURN
 * ----------------------
 * Called at the start of turn with the current game state.
 */
Playground.prototype._onEVENT_START_OF_TURN = function(data) {
    // We store the current game state, to use later when we get requests...
    this._gameState = data.game;
};

/**
 * _onREQUEST_PLAY
 * ---------------
 * Called when we receive a request for a PLAY update, ie instructions
 * to move players, kick etc.
 */
Playground.prototype._onREQUEST_PLAY = function(data) {
    // We create an object for the reply...
    var reply = {};
    reply.request = "PLAY";
    reply.actions = [];

    // We only update player movements if some time has elapsed...
    var nextChange = this._lastTimeWeChangedMovements + this._changeInterval;
    if(this._gameState.currentTimeSeconds >= nextChange ||
        this._lastTimeWeChangedMovements === -1.0) {
        // We change the movements of our players.
        // For each player, we choose a random place on the pitch
        // for them to move towards...
        this._players.forEach(function(player) {
            var action = {};
            action.playerNumber = player.playerNumber;
            action.action = "MOVE";
            action.destination = {x:Math.random() * this._pitch.width, y:Math.random() * this._pitch.height};
            action.speed = 100.0;
            reply.actions.push(action);
        }, this);

        // We store the current time...
        this._lastTimeWeChangedMovements = this._gameState.currentTimeSeconds;
    }

    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
    console.log(jsonReply);
};

/**
 * _onREQUEST_KICKOFF
 * ------------------
 * Called when we get the request to place players for the kickoff.
 */
Playground.prototype._onREQUEST_KICKOFF = function(data) {
};




