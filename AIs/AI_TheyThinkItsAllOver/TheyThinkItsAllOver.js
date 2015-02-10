/**
 * TheyThinkItsAllOver
 * --------------
 * 
 * To DO:
 * - position keeper - done
 * - dont pass so far backwards - done
 * - try marking attackers - done
 * - players move into space when we have ball
 * - when opposing goalkeeper has ball, try having a striker stand in front - done
 * - try dragging defenders
 * - safe path for pass - done
 * - strikers need to be better at shooting/running - done
 * - if shooting for center doesn;t work, try shotting for corner - done
 * - set up strategy object (strategy v time)
 * - set up statistics, number of shots in center v number at conrner etc - sort of done
 defender only goes to 50 - done
 fix angle of defender vs ball - done
 
 */
var readline = require('readline');
var UtilsLib = require('../../utils');

var Logger = UtilsLib.Logger;
var Position = UtilsLib.Position;
var Utils = UtilsLib.Utils;
var Random = UtilsLib.Random;
var LogHandler_File = UtilsLib.LogHandler_File;

// We set up logging...
Logger.addHandler(new LogHandler_File('./log/TheyThinkItsAllOver.log', Logger.LogLevel.DEBUG));

// We create the AI...
var ai = new TheyThinkItsAllOver();

/**
 * @constructor
 */
function TheyThinkItsAllOver() {
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

	this.DRIBBLE_SPEED = 35;
	this.TAKE_POSSESSION_DIST = 1.0;
	
    // Data about the pitch (width, height, position of goals)...
    this._pitch = {width:0, height:0};

    // The direction we're playing...
    this._direction = "";
	this._origDirection	= ""; // set only when it changes (start and half time)
	this._teamNumber = 0;
	this._playerNumOffset = 0;
	
	
	// exploits
	this._exploit1 = {'on':1,'attempts':0};
	this._numGoalsFromExploit1 = 0;
	this._startExploit1 = true;
	this._exploit1Time = 0.0;
	
	// move goalkeeper on path - start at 0
	this._nextGoalkeeperWaypoint = 0;
	// when dod keeper get ball
	this._keeperGotBall = -1;	
	
	// how do we shoot
	this._randomShots = 0;
	this._numShots = 0;

	
	
	// for defenders
	this._mostForwardOtherPlayer = 9999;
	
	// style of defending
	this._markedList = [];
	this._manToManMarking = true;//false;

    // The collection of players {playerNumber, playerType}...
    this._players = [];
	
	this._playersHash = {};

    // The game state for the current turn...
    this._gameState = {};
	this._ball = {}
	this._weHaveBall = 0;
	this._theyHaveBall = 0;
	// we don't have ball but are closer than other team
	this._weHaveControlOfBall = false;
	this._myTeam = {}
	this._otherTeam = {}
	
	this._ourScore = 0;
	this._otherScore = 0;
	
	// if we go behind, we change to dribbling by not recording distance to 
	// other players. If we go to far behind, we switch back to passing for rest
	// of game
	true._forcePassing = false;
	
    // Kickoff info, including the direction we're playing...
    this._kickoffInfo = {};


    // The last time (in game-time) that we changed the movement of
    // players. We only update every few seconds...
    this._lastTimeWeChangedMovements = 0.0;
    this._changeInterval = 10.0;
	
	this._timeNow = 0.0;
}


TheyThinkItsAllOver.prototype.convertX = function(x) {
		var outX = x;
		Logger.log("X in = " + x + " for direction " + this._direction + "\n\r",Logger.LogLevel.DEBUG);

		if(this._direction == "LEFT")
		{
			// invert
			outX = 100 - x;
		}
		Logger.log("X out = " + outX + "\n\r",Logger.LogLevel.DEBUG);

		return outX;
}

TheyThinkItsAllOver.prototype.convertY = function(y) {
		var outY = y;
		if(this._direction == "LEFT")
		{
			// invert
			outY = 50 - y;
		}
		return outY;
}

TheyThinkItsAllOver.prototype.convertDir = function(dir) {
		var outDir = dir;
		if(this._direction == "LEFT")
		{
			// invert
			outDir = dir + 180;
			if(outDir > 360)
				outDir -= 360;
		}
		return outDir;
}


TheyThinkItsAllOver.prototype.swapX = function(x) {
	return 100 - x;
}

TheyThinkItsAllOver.prototype.swapY = function(y) {
	return 50 - y;
}



// converts a x = x+70 into a -70
TheyThinkItsAllOver.prototype.convertInc = function(inc) {
		var outInc = inc;
		if(this._direction == "LEFT")
		{
			// invert
			outInc = inc * -1;
		}
		return outInc;
}

// use when comparing two variables that are already correct
TheyThinkItsAllOver.prototype.lessThan = function(val1,val2) {
		var outBool = true;
		if(this._direction == "LEFT")
		{
			// invert
			outBool = val1 > val2;
		}
		else
		{
			outBool = val1 < val2;
		}
		return outBool;
	
}


// use when comparing two variables that are already correct
TheyThinkItsAllOver.prototype.greaterThan = function(val1,val2) {
		var outBool = true;
		if(this._direction == "LEFT")
		{
			// invert
			outBool = val1 < val2;
		}
		else
		{
			outBool = val1 > val2;
		}
		return outBool;
	
}


// is val1 less than val2.  If we kick left then its greater than!!!
// use when second arg is a magic number
TheyThinkItsAllOver.prototype.lessThanX = function(val1,val2) {
		var outBool = true;
		if(this._direction == "LEFT")
		{
			// invert
			outBool = val1 > 100-val2;
		}
		else
		{
			outBool = val1 < val2;
		}
		return outBool;
	
}
// ... and same for >
TheyThinkItsAllOver.prototype.greaterThanX = function(val1,val2) {
		var outBool = true;
		if(this._direction == "LEFT")
		{
			// invert
			outBool = val1 < 100-val2;
		}
		else
		{
			outBool = val1 > val2;
		}
		return outBool;
	
}


TheyThinkItsAllOver.prototype.lessThanY = function(val1,val2) {
		var outBool = true;
		if(this._direction == "LEFT")
		{
			// invert
			outBool = val1 > 50-val2;
		}
		else
		{
			outBool = val1 < val2;
		}
		return outBool;
	
}
// ... and same for >
TheyThinkItsAllOver.prototype.greaterThanY = function(val1,val2) {
		var outBool = true;
		if(this._direction == "LEFT")
		{
			// invert
			outBool = val1 < 50-val2;
		}
		else
		{
			outBool = val1 > val2;
		}
		return outBool;
	
}



TheyThinkItsAllOver.prototype.min = function(val1,val2) {
		var outVal = 0;;
		if(this._direction == "LEFT")
		{
			// invert
			outVal = Math.max(val1,val2);
		}
		else
		{
			outVal = Math.min(val1,val2);
		}
		return outVal;
}

TheyThinkItsAllOver.prototype.max = function(val1,val2) {
		var outVal = 0;;
		if(this._direction == "LEFT")
		{
			// invert
			outVal = Math.min(val1,val2);
		}
		else
		{
			outVal = Math.max(val1,val2);
		}
		return outVal;
}




/**
 * onGameData
 * ----------
 * Called when the game sends data to us.
 */
TheyThinkItsAllOver.prototype.onGameData = function(jsonData) {
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
TheyThinkItsAllOver.prototype._onEVENT = function(data) {
    // We call different functions depending on the event...
    var eventHandler = '_onEVENT_' + data.eventType;
    this[eventHandler](data);
};

/**
 * _onREQUEST
 * ----------
 * Called when we receive a request message.
 */
TheyThinkItsAllOver.prototype._onREQUEST = function(data) {
    // We call different functions depending on the request...
    var requestHandler = '_onREQUEST_' + data.requestType;
    this[requestHandler](data);
};

/**
 * _onEVENT_GAME_START
 * -------------------
 * Called at the start of a game with general game info.
 */
TheyThinkItsAllOver.prototype._onEVENT_GAME_START = function(data) {
    // We reset the last update time...
    this._lastTimeWeChangedMovements = 0.0;
	

    // We get data about the pitch...
    this._pitch = data.pitch;
};

/**
 * _onEVENT_TEAM_INFO
 * ------------------
 * Called when we receive the TEAM_INFO event.
 */
 /*
 SENT EVENT: {"eventType":"TEAM_INFO","teamNumber":1,"players":[{"playerNumber":0,"playerType":"P"},{"playerNumber":1,"playerType":"P"},{"playerNumber":2,"playerType":"P"},{"playerNumber":3,"playerType":"P"},{"playerNumber":4,"playerType":"P"},{"playerNumber":5,"playerType":"G"}],"messageType":"EVENT"}
 */
TheyThinkItsAllOver.prototype._onEVENT_TEAM_INFO = function(data) {
	Logger.log("---------------------------------------------------------- \n\r",Logger.LogLevel.DEBUG);
	Logger.log("------------------Event Team Info------------------------------- \n\r",Logger.LogLevel.DEBUG);
	Logger.log("data = " + JSON.stringify(data) + "\n\r",Logger.LogLevel.DEBUG);
	Logger.log("---------------------------------------------------------- \n\r",Logger.LogLevel.DEBUG);

    // We store our team number, direction and player info...
    this._teamNumber = data.teamNumber;
	Logger.log("We are team number: " + this._teamNumber +"\n\r",Logger.LogLevel.DEBUG);
	
    this._players = data.players;
	
	// 		reply.players = [{{"position":{"x":7,"y":25},"direction":90},"playerNumber":player.playerNumber},{"playerNumber":1,"position":{"x":25,"y":35},"direction":90},{"playerNumber":2,"position":{"x":37,"y":25},"direction":90},{"playerNumber":3,"position":{"x":49,"y":14},"direction":90},{"playerNumber":4,"position":{"x":49,"y":36},"direction":90},{"playerNumber":5,"position":{"x":7,"y":25},"direction":90}]

	var count = 0;
	playersHash = {};

	Logger.log("PlayersHash looks like: " + JSON.stringify(this._playersHash) +"\n\r",Logger.LogLevel.DEBUG);
	
	this._players.forEach(function(player) {

		 // we call convert funcs but they will do nothing until we set up _direction in kickoff msg
		 if(player.playerType == "G")
		 {
			playersHash[String(player.playerNumber)] = {"playerNumber":player.playerNumber,"position":{"x":this.convertX(7),"y":this.convertY(25)},"startPosition":{"x":this.convertX(7),"y":this.convertY(25)},"direction":this.convertDir(90),"type":"G","strategy":"goalkeeper","side":"","hasBall":false,"distToOpponent":9999,"markedBy":-1}
		 }
		 else if(count == 0)
		 {		 
			playersHash[String(player.playerNumber)] = {"playerNumber":player.playerNumber,"position":{"x":this.convertX(25),"y":this.convertY(15)},"startPosition":{"x":this.convertX(25),"y":this.convertY(15)},"direction":this.convertDir(90),"type":"D1","strategy":"defender","side":"left","hasBall":false,"distToOpponent":9999,"markedBy":-1}
			count++;
		 }	
		 else if(count == 1)
		 {		 
			playersHash[String(player.playerNumber)] = {"playerNumber":player.playerNumber,"position":{"x":this.convertX(25),"y":this.convertY(35)},"startPosition":{"x":25,"y":this.convertY(35)},"direction":this.convertDir(90),"type":"D2","strategy":"defender","side":"right","hasBall":false,"distToOpponent":9999,"markedBy":-1}
			count++;
		 }	
		 else if(count == 2)
		 {		 
			playersHash[String(player.playerNumber)] = {"playerNumber":player.playerNumber,"position":{"x":this.convertX(37),"y":this.convertY(25)},"startPosition":{"x":37,"y":this.convertY(25)},"direction":this.convertDir(90),"type":"M","strategy":"midfielder","side":"middle","hasBall":false,"distToOpponent":9999,"markedBy":-1}
			count++;
		 }	
		 else if(count == 3)
		 {		 
			playersHash[String(player.playerNumber)] = {"playerNumber":player.playerNumber,"position":{"x":this.convertX(49),"y":this.convertY(15)},"startPosition":{"x":49,"y":this.convertY(15)},"direction":this.convertDir(90),"type":"S1","strategy":"striker","side":"left","hasBall":false,"distToOpponent":9999,"markedBy":-1}
			count++;
		 }	
		 else if(count == 4)
		 {		 
			playersHash[String(player.playerNumber)] = {"playerNumber":player.playerNumber,"position":{"x":this.convertX(49),"y":this.convertY(35)},"startPosition":{"x":49,"y":this.convertY(35)},"direction":this.convertDir(90),"type":"S2","strategy":"striker","side":"right","hasBall":false,"distToOpponent":9999,"markedBy":-1}
			count++;
		 }	
	}, this);

	Logger.log("PlayersHash looks like: " + JSON.stringify(playersHash) +"\n\r",Logger.LogLevel.DEBUG);
	
    this._playersHash = playersHash;
	
	Logger.log("this._PlayersHash looks like: " + JSON.stringify(this._playersHash) +"\n\r",Logger.LogLevel.DEBUG);
	
	// which set of players are we?
	// 0 or 6
	this._playerNumOffset = data.players[0].playerNumber;
};



//TheyThinkItsAllOver.prototype._whichTeamClosestToBall = function(myPlayer,x,y,shoot) {
//	Logger.log("****playerNum " + myPlayer.playerNumber + " facing " + myPlayer.direction + "\n\r",Logger.LogLevel.DEBUG);
//}

/**
 * _onEVENT_START_OF_TURN
 * ----------------------
 * Called at the start of turn with the current game state.
 * This is where we analyse where the players are, who has ball etc!!!!!!!!!!!!!!
 */
TheyThinkItsAllOver.prototype._onEVENT_START_OF_TURN = function(data) {
    // We store the current game state, to use later when we get requests...
	Logger.log("----------------------------------------------------------\n\r",Logger.LogLevel.DEBUG);
	Logger.log("------- START OF TURN------------------------------------- \n\r",Logger.LogLevel.DEBUG);
	Logger.log("Start of turn data = " + JSON.stringify(data) + "\n\r",Logger.LogLevel.DEBUG);

	Logger.log("---------------------------------------------------------- \n\r",Logger.LogLevel.DEBUG);
	Logger.log("-------" + data.game.currentTimeSeconds + "------------------------------------\n\r",Logger.LogLevel.DEBUG);


    this._gameState = data.game;
	this._timeNow = data.game.currentTimeSeconds;
	this._ball = data.ball;
	
	this._weHaveBall = 0;
	this._theyHaveBall = 0;
	
	// we are clostest to the ball out of the two teams
	this._weHaveControlOfBall = false;
	Logger.log("Start of turn: ball = " + JSON.stringify(this._ball) +"\n\r",Logger.LogLevel.DEBUG);
	
	if(this._teamNumber == 1)
	{
		this._myTeam = data.team1;
		this._otherTeam = data.team2;
		this._ourScore = data.team1.team.score;
		this._otherScore = data.team2.team.score;
		
		if(this._ball.controllingPlayerNumber >= 0 && this._ball.controllingPlayerNumber <= 5)
		{
			this._weHaveBall = 1;
		}
		else if(this._ball.controllingPlayerNumber != -1)
		{
		
			this._theyHaveBall = 1;
			Logger.log("They have ball. this._ball.controllingPlayerNumber = " + this._ball.controllingPlayerNumber + " \n\r",Logger.LogLevel.DEBUG);

		}
		else
		{
			Logger.log("Nobody ball\n\r",Logger.LogLevel.DEBUG);
			this._weHaveBall = 0;
			this._theyHaveBall = 0;
		
		}
	}
	else
	{
		this._myTeam = data.team2;
		this._otherTeam = data.team1;
		this._ourScore = data.team2.team.score;
		this._otherScore = data.team1.team.score;
		
		if(this._ball.controllingPlayerNumber >= 6 && this._ball.controllingPlayerNumber <= 11)
		{
			this._weHaveBall = 1;
		}
		else if(this._ball.controllingPlayerNumber != -1)
		{		
			this._theyHaveBall = 1;
			Logger.log("They have ball. this._ball.controllingPlayerNumber = " + this._ball.controllingPlayerNumber + " \n\r",Logger.LogLevel.DEBUG);

		}
		else
		{
			Logger.log("Nobody ball\n\r",Logger.LogLevel.DEBUG);
			this._weHaveBall = 0;
			this._theyHaveBall = 0;
		
		}
	}

	//Logger.log("-------------------------------------------------------------" +"\n\r",Logger.LogLevel.DEBUG);	
	//Logger.log("this._playersHash = " + JSON.stringify(this._playersHash) +"\n\r",Logger.LogLevel.DEBUG);
	
	this._myTeam.players.forEach(function(p)
	{
		var playerNumStr = String(p.staticState.playerNumber);
		
		this._playersHash[playerNumStr].position = p.dynamicState.position;
		this._playersHash[playerNumStr].direction = p.dynamicState.direction;
		if(this._ball.controllingPlayerNumber == p.staticState.playerNumber)
		{
			this._playersHash[playerNumStr].hasBall = true;
		}
		else
		{
			this._playersHash[playerNumStr].hasBall = false;
		}	
	}, this);		
	
	
	// enrich the data
	var closestPlayerToBall = null;
	var ourDistToBall = 9999;
	
	// enrich the playersHash data
	for(i in this._playersHash)
	{	
		var myPlayer = this._playersHash[i];		
		var tempDistToBall = this._getDistToBall(myPlayer);
		// reset them
		myPlayer.closestToBall = false;
		if(tempDistToBall < ourDistToBall && myPlayer.type != "G")
		{
			ourDistToBall = tempDistToBall;
			closestPlayerToBall = myPlayer;
		}
	
		
		// if we're to many goals behind then don't set this which means we just attack
		// hacky way to change strategy
		myPlayer.distToOpponent = 9999;
		if(this._ourScore >= this._otherScore-1 || this._forcePassing)
		{
			myPlayer.distToOpponent = this._getDistToOpponents(myPlayer);
		}	
		else
		{
			if(this._ourScore + 4 < this._otherScore)
			{
				// dribbling isn;t working so go back to passing
				this._forcePassing = true;
				Logger.log("dribbling not working so bo back to passing = " + JSON.stringify(closestPlayerToBall) + ", dist = " + ourDistToBall + "\n\r",Logger.LogLevel.DEBUG);

			}
		}
	
	}
	
	Logger.log("my closest (non goalkeeper) player to ball = " + JSON.stringify(closestPlayerToBall) + ", dist = " + ourDistToBall + "\n\r",Logger.LogLevel.DEBUG);

	closestPlayerToBall.closestToBall = true;

	
	// if neither team has the ball, we need to chck the other team as 
	// we want to know which team is closest as this will effect supporting
	// players behaviour.  We may be dribbling!
//	if(!this._weHaveBall && !this.theyHaveBall)
//	{
		var otherTeamDistToBall = 9999;
		this._mostForwardOtherPlayer = 9999;			
		if(this._direction == "LEFT")
		{
			this._mostForwardOtherPlayer = -1;
		}
		
		this._otherTeam.players.forEach(function(p)
		{
			var tempDistToBall = this._getDistToBallFromPosition(new Position(p.dynamicState.position.x,p.dynamicState.position.y));
			otherTeamDistToBall = Math.min(tempDistToBall,otherTeamDistToBall);
			Logger.log("Other players closest dist to ball = " + otherTeamDistToBall + "\n\r",Logger.LogLevel.DEBUG);			
			
			// used to position defender
			this._mostForwardOtherPlayer = this.min(p.dynamicState.position.x,this._mostForwardOtherPlayer);	
			Logger.log("Most forward player is " + this._mostForwardOtherPlayer + "\n\r",Logger.LogLevel.DEBUG);			
		},this);

		this._weHaveControlOfBall = false;
		if(ourDistToBall < otherTeamDistToBall) 
			this._weHaveControlOfBall = true;

	// empty mark list so we start picking targets again - hopefully same targets
	this._markedList = [];		
};

/**
 * _onEVENT_KICKOFF
 * ----------------
 * Called when we get the KICKOFF event.
 */
TheyThinkItsAllOver.prototype._onEVENT_KICKOFF = function(data) {
	Logger.log("---------------------------------------------------------- \n\r",Logger.LogLevel.DEBUG);
	Logger.log("------------------Event Team Info------------------------------- \n\r",Logger.LogLevel.DEBUG);
	Logger.log("data = " + JSON.stringify(data) + "\n\r",Logger.LogLevel.DEBUG);

	Logger.log("---------------------------------------------------------- \n\r",Logger.LogLevel.DEBUG);


    this._kickoffInfo = data;

	if(this._teamNumber == 1)
	{
		this._direction = data.team1.direction;
	}
	else
	{
		this._direction = data.team2.direction;	
	}
	
	var swap = false;
	
	if(this._origDirection == "")
	{
		// on start up
		this._origDirection = this._direction;
		// swap on start up if we are left as we added players too early so thet will
		// be shooting right
		if(this._direction == "LEFT")
			swap = true;	
	}
	
		
	// on change of sides
	if(this._direction != this._origDirection)
	{
		this._origDirection = this._direction;
		swap = true;
	}
	
	// we find out direction after setting up player info.  We set up player info
	// early bec we need it for configuration msg
	for(i in this._playersHash)
	{	
		var player = this._playersHash[i];		
		if(swap)
		{
			Logger.log("swaps sides\n\r",Logger.LogLevel.DEBUG);			
		
			//reverse at half time
			Logger.log("x before = " + player.startPosition.x + "\n\r",Logger.LogLevel.DEBUG);			

			player.startPosition.x = this.swapX(player.startPosition.x);
			player.startPosition.y = this.swapY(player.startPosition.y);
		}
		player.position.x = player.startPosition.x;
		player.position.y = player.startPosition.y;
		
	}		


	
	Logger.log("We are shooting: " + this._direction +"\n\r",Logger.LogLevel.DEBUG);
	
};

/**
 * _onEVENT_GOAL
 * -------------
 * Called when we get the GOAL event.
 */
TheyThinkItsAllOver.prototype._onEVENT_GOAL = function(data) {
	Logger.log("GOAL.... data = " + JSON.stringify(data) + "\n\r",Logger.LogLevel.DEBUG);
	
	var theirScore = 0;
	
	// exploits
	if(this._exploit1.on)
	{
		Logger.log("Exploit 1 on\n\r",Logger.LogLevel.DEBUG);
		var ourScore = 0;
		if(this._teamNumber == 1)
		{
			if(this._timeNow < this._exploit1Time + 3)
			{
				// we scored within 3 secs of it being in area so assume
				// exploit worked
				
				this._numGoalsFromExploit1 = data.team1.score;
				Logger.log("Goal caused by Exploit, this._numGoalsFromExploit1 = " + this._numGoalsFromExploit1 + " attempts = " +   + "\n\r",Logger.LogLevel.DEBUG);
				
			}	
			ourScore = data.team1.score;
			theirScore = data.team2.score;
		}
		else
		{
			if(this._timeNow < this._exploit1Time + 3)
			{
				// we scored within 3 secs of it being in area so assume
				// exploit worked
				Logger.log("Exploit 1 on\n\r",Logger.LogLevel.DEBUG);
				this._numGoalsFromExploit1 = data.team2.score;
			}	
			ourScore = data.team2.score;
			theirScore = data.team1.score;
		}
		Logger.log("Our score = " + ourScore + "\n\r",Logger.LogLevel.DEBUG);
		
		// need to get it to work over 1 in 4 times
		if(this._numGoalsFromExploit1 > 0)
		{
			if((this._exploit1.attempts / this._numGoalsFromExploit1) > 6)
			{
				Logger.log("Switch off Exploit 1\n\r",Logger.LogLevel.DEBUG);
			
				this._exploit1.on = false;
			}
		}
		else if(this._exploit1.attempts > 6)
		{
			Logger.log("Switch off Exploit 1\n\r",Logger.LogLevel.DEBUG);
			this._exploit1.on = false;
		}
		
		
		// if we're scoring less than 1 in 3 shots, change strategy
		if(ourScore > 0 && this._numShots / ourScore > 5)
		{
			Logger.log("Change shooting strategy\n\r",Logger.LogLevel.DEBUG);
			this._randomShots = 1;
		}	
	}
	
	for(i in this._playersHash)
	{	
		var player = this._playersHash[i];		
		player.position = player.startPosition;
	}		
	
	if(theirScore > 10)
	{
		this._manToManMarking = false;		
		Logger.log("****Man marking OFF\n\r",Logger.LogLevel.DEBUG);

	}
	else if(theirScore > 5)  
	{
		Logger.log("****Man marking ON\n\r",Logger.LogLevel.DEBUG);
	
		this._manToManMarking = true;	
	}	
};

/**
 * _onEVENT_HALF_TIME
 * ------------------
 * Called when we get the HALF_TIME event.
 */
TheyThinkItsAllOver.prototype._onEVENT_HALF_TIME = function(data) {
};

/**
 * _onREQUEST_CONFIGURE_ABILITIES
 * ------------------------------
 * Called when we receive the request to configure abilities for our players.
 {"requestType":"CONFIGURE_ABILITIES","totalKickingAbility":400,"totalRunningAbility":400,
 "totalBallControlAbility":400,"totalTacklingAbility":400,"messageType":"REQUEST"}

 */
TheyThinkItsAllOver.prototype._onREQUEST_CONFIGURE_ABILITIES = function(data) {
    var reply = {};
    reply.requestType = "CONFIGURE_ABILITIES";
    reply.players = [];
	var totalKickingAbility = data.totalKickingAbility;
	var totalRunningAbility = data.totalRunningAbility;
	var totalBallControlAbility = data.totalBallControlAbility;
	var totalTacklingAbility = data.totalTacklingAbility;

	// split total into 24 (4*6) segments.  We can then share them out depending
	// upon player type
	var kickingAbility = totalKickingAbility/24;
	var runningAbility = totalRunningAbility/24;
	var ballControlAbility = totalBallControlAbility/24;
	var tacklingAbility = totalTacklingAbility/24;
	
	// goal keeper takes the rest
	
	this._players.forEach(function(player) {
			// share out the points
		var type = this._playersHash[String(player.playerNumber)].type;
	
		if(type == "S1" || type == "S2")
		{
			var info = {
					// *4 would be equal across all
					playerNumber:player.playerNumber,
					kickingAbility:kickingAbility*5, // 10
					runningAbility:runningAbility*4, // 8
					ballControlAbility:ballControlAbility*3, //6
					tacklingAbility:tacklingAbility*3 //6
			};
		}	
		else if(type == "D1" || type == "D2")
		{
			var info = {
					// *4 would be equal across all
					playerNumber:player.playerNumber,
					kickingAbility:kickingAbility*3, //16
					runningAbility:runningAbility*4, // 16
					ballControlAbility:ballControlAbility*4, // 14
					tacklingAbility:tacklingAbility*6 // 18
			};
		}	
		else if(type == "M")
		{
			var info = {
					// *4 would be equal across all
					playerNumber:player.playerNumber,
					kickingAbility:kickingAbility*4, // 20
					runningAbility:runningAbility*4, // 20
					ballControlAbility:ballControlAbility*4, // 18
					tacklingAbility:tacklingAbility*5 // 23
			};
		}	
		else if(type == "G")
		{
			var info = {
					// *4 would be equal across all
					playerNumber:player.playerNumber,
					kickingAbility:kickingAbility*4, // 24
					runningAbility:runningAbility*4, // 24
					ballControlAbility:ballControlAbility*6, //24
					tacklingAbility:tacklingAbility*1 //24
			};
		}	
		

		//totalKickingAbility = totalKickingAbility - kickingAbility;
		//totalRunningAbility = totalRunningAbility - runningAbility;
		//totalBallControlAbility = totalBallControlAbility - ballControlAbility;
		//totalTacklingAbility = totalTacklingAbility - tacklingAbility;

	reply.players.push(info);
	}, this);	

    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
    console.log(jsonReply);
};

/**
 * _onREQUEST_KICKOFF
 * ------------------
 * Called when we receive a request for player positions at kickoff.
 */
TheyThinkItsAllOver.prototype._onREQUEST_KICKOFF = function(data) {
    // We return an empty collection of positions, so we get the defaults...
	Logger.log("---------------------------------------------------------- \n\r",Logger.LogLevel.DEBUG);
	Logger.log("----------------KICKOFF---------------------------------- \n\r",Logger.LogLevel.DEBUG);
	Logger.log("data = " + JSON.stringify(data) + "\n\r",Logger.LogLevel.DEBUG);
	Logger.log("---------------------------------------------------------- \n\r",Logger.LogLevel.DEBUG);


    var reply = {};
    reply.requestType = "KICKOFF";

	reply.players = [];
	// this is a bit clunky to loop through
	for(i in this._playersHash)
	{	
		var p = this._playersHash[i];
	
		Logger.log("p.playerNumber " + JSON.stringify(p.playerNumber) + "\n\r",Logger.LogLevel.DEBUG);
		Logger.log("p.position " + JSON.stringify(p.position) + "\n\r",Logger.LogLevel.DEBUG);
		Logger.log("p.direction " + JSON.stringify(p.direction) + "\n\r",Logger.LogLevel.DEBUG);

		var playerOut = {"playerNumber":p.playerNumber,"position":p.position,"direction":p.direction};
		Logger.log("playerOut " + JSON.stringify(playerOut) + "\n\r",Logger.LogLevel.DEBUG);

	    reply.players.push(playerOut);
	};
	
    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
	
    console.log(jsonReply);
};

// turn if necessary otherwise kick. hopefully if we turn then next turn we will
// be able to kick. Shoot is boolean and if true, kick at max

TheyThinkItsAllOver.prototype._turnAndKick = function(myPlayer,x,y,shoot,opponentDist) {
	Logger.log("****playerNum " + myPlayer.playerNumber + " facing " + myPlayer.direction + "\n\r",Logger.LogLevel.DEBUG);

	
	var direction = Utils.angleBetween(new Position(myPlayer.position.x,myPlayer.position.y), 
	                   new Position(x,y));
	Logger.log("****Angle to target is " + direction + "\n\r",Logger.LogLevel.DEBUG);
					   
	var absDirectionDiff = Math.abs(direction-myPlayer.direction);

	// accuracy is dependent upon if we are going to be tackled
	// its how close do we need to be pointing to actual dir before kicking
	var accuracy = 10;
	if(myPlayer.type == 'G')
	{
		accuracy = 2;
	}
	else if(opponentDist < 0.5)
	{
		Logger.log("****Angle doesn't matter, just kick it (opponent dist = " + opponentDist + ") \n\r",Logger.LogLevel.DEBUG);
	
		accuracy = 180;
	}
	else if(opponentDist < 1)
	{
		accuracy = 60;
	}
	
	else if(opponentDist < 5)
	{
		accuracy = 20;
	}
	
	if(absDirectionDiff < accuracy || absDirectionDiff > (360 - accuracy))
	{
		// we're in the right direction, so kick it
		var tempDist = Utils.distanceBetween(new Position(myPlayer.position.x,myPlayer.position.y),new Position(x,y));
		Logger.log("****Direction of my player = " + myPlayer.direction + "\n\r",Logger.LogLevel.DEBUG);
		Logger.log("****Kick it, dist from player to target = " + tempDist + "\n\r",Logger.LogLevel.DEBUG);

		var action = {};
		action.playerNumber = myPlayer.playerNumber;
		action.action = "KICK";
		action.destination = {"x":x, "y":y};
		var speed = 100;
		if(!shoot)
		{
			// try to get the perfect speed.  Less than 50 tends to turn into
			// a dribble.
			var speedFactor = 2.95;
			if(myPlayer.type.substring(0,1) == "S")
			{
				speedFactor = 2.0;
			}
			speed = Math.max(50,tempDist*speedFactor);
		}
		Logger.log("***speed = " + speed + "\n\r",Logger.LogLevel.DEBUG);
			
		action.speed = speed;
		
		// in case we are goalkeeper on path or wasting time - reset for next time
		this._nextGoalkeeperWaypoint = 0;
		this._keeperGotBall = -1;

		return action;		
	}
	else
	{
		Logger.log("****playerNum " + myPlayer.playerNumber + " turning to " + direction + "\n\r",Logger.LogLevel.DEBUG);
		var action = {};
		action.playerNumber = myPlayer.playerNumber;
		action.action = "TURN";
		action.direction = direction;
		action.speed = 100.0;
		return action;			
	}
};



// what is atual diff between two angle, order does not matter
TheyThinkItsAllOver.prototype._getAngleDiff = function(angle1,angle2)
{
	// 50 - 30 = 20
	// 300 - 20 = 280 = 100
	// 30 - 350 = -320 = 320 = 40
	var angle = Math.abs(angle1-angle2);
	if(angle > 180)
		angle = 360 - angle;
	Logger.log("                 Diff in angle is " + angle + "\n\r",Logger.LogLevel.DEBUG);					
		
	return angle;
}





TheyThinkItsAllOver.prototype._checkDribbleAngle = function(myPlayer,forwardAngle) {

		var directionGood = true;
		
		this._otherTeam.players.forEach(function(otherPlayer) 
		{
			var myPos = new Position(myPlayer.position.x,myPlayer.position.y);
			var otherPos = new Position(otherPlayer.dynamicState.position.x,otherPlayer.dynamicState.position.y);

			var direction2 = Utils.angleBetween(myPos, otherPos);
			var distToOther = Utils.distanceBetween(myPos,otherPos);
		
			// we struggle with close players so be extra careful
			var safeAngle = 20;
			if(distToOther < 5)
				safeAngle = safeAngle * 2;
				
			if(distToOther < 25 && this._getAngleDiff(forwardAngle, direction2) < safeAngle)
			{
				directionGood = false;
				return directionGood;
			}
		}, this);
		
		return directionGood;
}		


		
TheyThinkItsAllOver.prototype._dribbleSafelyPast = function(myPlayer) {
	// keep looking around until we find a direction we can dribble away 
	// from players
	Logger.log("Will try to dribble\n\r",Logger.LogLevel.DEBUG);
	
	var forwardAngle = this.convertDir(90);	
	var startAngle = forwardAngle;
	
	// full circle
	for(; forwardAngle < startAngle + 90; forwardAngle+=10)
	{
		Logger.log("Can we dribble in direction " + forwardAngle + "\n\r",Logger.LogLevel.DEBUG);

		if(this._checkDribbleAngle(myPlayer,forwardAngle))
		{

			var newPosition = Utils.pointFromDirection(new Position(myPlayer.position.x,myPlayer.position.y), forwardAngle, 20);  
			Logger.log("  Yes we can: " + JSON.stringify(newPosition) + "\n\r",Logger.LogLevel.DEBUG);		
			
			var action = {};
            action.playerNumber = myPlayer.playerNumber;
			action.action = "KICK";
			action.destination = {x:newPosition.x, y:newPosition.y};
			action.speed = this.DRIBBLE_SPEED;
			return action;
			
		}
	}
	
		
	// full circle
	
	for(forwardAngle = this.convertDir(90); forwardAngle > startAngle -90; forwardAngle-=10)
	{
		Logger.log("...Or can we dribble in direction " + forwardAngle + "\n\r",Logger.LogLevel.DEBUG);

		if(this._checkDribbleAngle(myPlayer,forwardAngle))
		{
		
			var newPosition = Utils.pointFromDirection(new Position(myPlayer.position.x,myPlayer.position.y), forwardAngle, 20);  

			Logger.log("  Yes we can: " + JSON.stringify(newPosition) + "\n\r",Logger.LogLevel.DEBUG);		
			
			var action = {};
            action.playerNumber = myPlayer.playerNumber;
			action.action = "KICK";
			action.destination = {x:newPosition.x, y:newPosition.y};
			action.speed = this.DRIBBLE_SPEED;
			return action;
			
		}
	}
	
	
	// ok so check backwards, loop from bottom to top
	var forwardAngle = this.convertDir(180);	
	var startAngle = forwardAngle;
	
	for(;forwardAngle < startAngle + 180; forwardAngle+=10)
	{
		Logger.log("So can we dribble backwards in direction " + forwardAngle + "\n\r",Logger.LogLevel.DEBUG);

		if(this._checkDribbleAngle(myPlayer,forwardAngle))
		{
			var newPosition = Utils.pointFromDirection(new Position(myPlayer.position.x,myPlayer.position.y), forwardAngle, 20);  

			Logger.log("  Yes we can: " + JSON.stringify(newPosition) + "\n\r",Logger.LogLevel.DEBUG);		
			
			// slow dribble
			var action = {};
            action.playerNumber = myPlayer.playerNumber;
			action.action = "MOVE";
			action.destination = {x:newPosition.x, y:newPosition.y};
			action.speed = 100.0;
			return action;
		}
	}

	
		
	// surrounded, stay still and hope
	Logger.log("Surrounded...\n\r",Logger.LogLevel.DEBUG);		

	var action = {};
	action.playerNumber = myPlayer.playerNumber;
	action.action = "MOVE";
	action.destination = {x:this._ball.position.x, y:this._ball.position.y};
	action.speed = 0;
	return action;
	
}


// based on y
TheyThinkItsAllOver.prototype._ballOnMySideOfPitch = function(myPlayer) {
	var rv = false;
	if((this._ball.position.y < 25 && myPlayer.startPosition.y < 25) ||
	   (this._ball.position.y >= 25 && myPlayer.startPosition.y >= 25))
	{
		rv = true;
	}
	
	return rv;
}				

TheyThinkItsAllOver.prototype._getDistToBall = function(player) {
	var myX = player.position.x;
	var myY = player.position.y;
	var distTemp = Utils.distanceBetween(new Position(myX,myY),new Position(this._ball.position.x,this._ball.position.y));	
	return distTemp;
};

TheyThinkItsAllOver.prototype._getDistToBallFromPosition = function(position) {
	var myX = position.x;
	var myY = position.y;
	var distTemp = Utils.distanceBetween(new Position(myX,myY),new Position(this._ball.position.x,this._ball.position.y));	
	return distTemp;
};





TheyThinkItsAllOver.prototype._getDistToOpponents = function(myPlayer) {
			var inFront = false;
			var distTemp = 9999;
			var dist = 9999;
			
			this._otherTeam.players.forEach(function(otherPlayer) 
			{
				// short hand
				var myX = myPlayer.position.x;
				var myY = myPlayer.position.y;
				var otherX = otherPlayer.dynamicState.position.x;
				var otherY = otherPlayer.dynamicState.position.y;
				var distTemp = Utils.distanceBetween(new Position(myX,myY),new Position(otherX,otherY));
				
				if(this.lessThan(myX,otherX))
				{
					inFront = true;
				}   
				
				// we only care about other players in front or who are really close
				if(inFront || distTemp < 5)
				{
					dist = Math.min(distTemp,dist);
				}
			}, this);  // end opponents loop

			return dist;
};

// ---------------------------------------------------
// player strategies


/*TheyThinkItsAllOver.prototype._isLeft = function(linePt1,linePt2,pt)
{
	return((linePt2.x-linePt1.x)*(pt.y-linePt1.y)-(linePt2.y-linePt1.y)*(pt.x-linePt1.x));
}
TheyThinkItsAllOver.prototype._onSameSide = function(pt1,pt2,linePt1,linePt2)
{
			var side1 = this._isLeft(linePt1,linePt2,pt1);
			var side2 = this._isLeft(linePt1,linePt2,pt2);
			if(side1 * side2 >= 0)
				return true;
			else
				return false;
}
TheyThinkItsAllOver.prototype._ptInTriangle = function(pt,a,b,c)
{
	if(this._onSameSide(pt,a,  b,c) && this._onSameSide(pt,b,  a,c) &&
	   this._onSameSide(pt,c,  a,b))
	{
		return true;
	}
	else
	{
		return false;
	}
}


// check pass is safe - i.e. we have a corridor to pass it down
TheyThinkItsAllOver.prototype._passIsSafe = function(from,to)
{
	Logger.log("start safeToPass function\n\r",Logger.LogLevel.DEBUG);					

	var direction = Utils.angleBetween(from, to);
	var rv = true;
	this._otherTeam.players.forEach(function(player)
	{
		Logger.log("      check other sides player " + player.staticState.playerNumber + "\n\r",Logger.LogLevel.DEBUG);					
	
		if(direction >315 || direction < 45 || (direction > 135 && direction < 225))
		{
			if(this._ptInTriangle(new Position(player.dynamicState.position.x,player.dynamicState.position.y),
		                      new Position(from.x,from.y),
							  new Position(to.x-8,to.y),
							  new Position(to.x+8,to.y)))	
			{
				Logger.log("safeToPass: is NOT safe to pass\n\r",Logger.LogLevel.DEBUG);					
				rv = false;
			}
		}
		else
		{
			if(this._ptInTriangle(new Position(player.dynamicState.position.x,player.dynamicState.position.y),
		                      new Position(from.x,from.y),
							  new Position(to.x,to.y-8),
							  new Position(to.x,to.y+8)))	
			{
				Logger.log("safeToPass: is NOT safe to pass\n\r",Logger.LogLevel.DEBUG);					
				rv = false;
			}
		}
	},this);

	Logger.log("end safeToPass function\n\r",Logger.LogLevel.DEBUG);					

	return rv;
}
*/




// check pass is safe - i.e. we have a corridor to pass it down.  v2 uses 
// angles to tell if its safe
/*TheyThinkItsAllOver.prototype._passIsSafe = function(from,to)
{
	return this._passIsSafe2(from,to,10); // 10 degrees
}
*/
TheyThinkItsAllOver.prototype._passIsSafe2 = function(from,to,safetyAngle) // degrees
{
	Logger.log("Min safe angle is " + safetyAngle + "\n\r",Logger.LogLevel.DEBUG);					

	var direction = Utils.angleBetween(from, to);
	var distToPlayer = Utils.distanceBetween(to,from);
	Logger.log("start safeToPass function for angle " + direction + ", distance = " + distToPlayer + "\n\r",Logger.LogLevel.DEBUG);					
	
	
	var rv = true;
	this._otherTeam.players.forEach(function(player)
	{
		var direction2 = Utils.angleBetween(from, new Position(player.dynamicState.position.x,player.dynamicState.position.y));

		var distToOther = Utils.distanceBetween(new Position(player.dynamicState.position.x,player.dynamicState.position.y),from);
		Logger.log("      check other sides player " + player.staticState.playerNumber + ", angle = " + direction2 + ", distToOther = " + distToOther + "\n\r",Logger.LogLevel.DEBUG);					

		// be careful of very close players
		var tempSafetyAngle = safetyAngle;
		if(distToOther < 7)
			tempSafetyAngle *= 1.5;
		else if(distToOther < 2)
			tempSafetyAngle *= 2;
			
		// add 7 in case of over shoot
		if(distToOther < distToPlayer + 7)		
		{	
			var diff = this._getAngleDiff(direction, direction2);
			if(diff < tempSafetyAngle)
			{
				Logger.log("safeToPass: is NOT safe to pass as " + diff + " < " + tempSafetyAngle + "\n\r",Logger.LogLevel.DEBUG);					
				rv = false;
			}	
		}
	},this);

	Logger.log("end safeToPass function\n\r",Logger.LogLevel.DEBUG);					

	return rv;
}



// pass in position of ball
TheyThinkItsAllOver.prototype._positionInFrontOfGoal = function(ballPosition,dist)
{
	var centreGoal = new Position(this.convertX(0),this.convertY(25));
	var direction = Utils.angleBetween(centreGoal, ballPosition);
	var position = Utils.pointFromDirection(centreGoal, direction, dist);  // 7 is dist 
	// now cope with ball in corner
	position.x = this.min(position.x,ballPosition.x);
	return position;
}

// move around area looking for a gap or using up time

TheyThinkItsAllOver.prototype._goalkeeperMoveOnPath = function(player){
	var waypoints = [{x:this.convertX(7),y:this.convertY(12)},
					{x:this.convertX(4),y:this.convertY(33)},
					{x:this.convertX(11),y:this.convertY(25)},
					{x:this.convertX(7),y:this.convertY(14)}];
					
	var waypoint = waypoints[this._nextGoalkeeperWaypoint];				
	
	if(Utils.approxEqual(player.position.x,waypoint.x) &&
       Utils.approxEqual(player.position.y,waypoint.y))
	{	
		this._nextGoalkeeperWaypoint++;
		Logger.log("  Next way point: " + this._nextGoalkeeperWaypoint + "\n\r",Logger.LogLevel.DEBUG);		
		
	}
	
	if(this._nextGoalkeeperWaypoint < waypoints.length)
	{
		var action = {};
		action.playerNumber = player.playerNumber;
		action.action = "MOVE";
		// already converted
		action.destination = {x:waypoint.x, y:waypoint.y};
		action.speed = 100.0;
		return action;
	}
	return null;
}

TheyThinkItsAllOver.prototype._strategy_goalkeeper_attack = function(player) {

	// use up some time
	// when did keeper get the ball
/*	if(this._keeperGotBall == -1)
		this._keeperGotBall = this._timeNow;
		
	var DELAY = 5; //secs
	if(this.otherScore + 3 < this.ourScore)
	{
		if(this._keeperGotBall < this._timeNow + DELAY)
		{
			// do nothing
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:player.position.x, y:player.position.y};
			action.speed = 0.0;
			return action;	
		}
	}
*/	
	// move to front of area before kicking it
	// we move at the end now
/*	if(this.lessThanX(player.position.x,10))
	{
		var action = {};
		action.playerNumber = player.playerNumber;
		action.action = "MOVE";
		action.destination = {x:this.convertX(10), y:this.convertY(25)};
		action.speed = 100.0;
		return action;	
	}*/

	var bestSafePlayer = null;
	var maxDist = 0;
	
	for(i in this._playersHash)
	{	
		var playerWithoutBall = this._playersHash[i];		
			
		Logger.log("   Check player " + playerWithoutBall.playerNumber + "\n\r",Logger.LogLevel.DEBUG);		

		if(playerWithoutBall.distToOpponent > maxDist && 
		   playerWithoutBall.type != "G" && 
		   this.greaterThanX(playerWithoutBall.position.x,30))
		{
			maxDist = playerWithoutBall.distToOpponent;
			if(this._passIsSafe2(new Position(player.position.x,player.position.y),
			                    new Position(playerWithoutBall.position.x,playerWithoutBall.position.y),25)) // 25 degrees
			{
			
				bestSafePlayer = playerWithoutBall;
			}
		}
	}	

	if(bestSafePlayer != null)
	{
		Logger.log("   Best player = " + JSON.stringify(bestSafePlayer) + "\n\r",Logger.LogLevel.DEBUG);		
		return this._turnAndKick(player,bestSafePlayer.position.x, bestSafePlayer.position.y,true,100);
	}
	else
	{
		// random place up the pitch
		Logger.log("Goalkeeper will just kick the ball\n\r",Logger.LogLevel.DEBUG);		

		for(i = 0; i < 50; i+=5)
		{
			if(this._passIsSafe2(new Position(player.position.x,player.position.y),
			                    new Position(player.position.x+this.convertInc(40),i),25)) // degrees
			{
				Logger.log("    i = " + i + "\n\r",Logger.LogLevel.DEBUG);		

				return this._turnAndKick(player,player.position.x+this.convertInc(40), i,true,100);
			}	
		}
		
		Logger.log("Goalkeeper will move to get better angle\n\r",Logger.LogLevel.DEBUG);		
		
		// nowhere to kick so move
		var rv = this._goalkeeperMoveOnPath(player);
		if(rv)
			return rv;

		Logger.log("Just kick it as I'm out of ideas\n\r",Logger.LogLevel.DEBUG);		

		var action = {};
        action.playerNumber = player.playerNumber;
		action.action = "KICK";
		action.destination = {x:this.convertX(20), y:this.convertY(50)/* + new Random().nextDouble()*10 */};
		action.speed = 100.0;
		return action;
		
	}
}
TheyThinkItsAllOver.prototype._strategy_goalkeeper_support = function(player) {
		return this._strategy_goalkeeper_defend(player);
}



TheyThinkItsAllOver.prototype._ballHeadingForGoal = function() {
	if(this._ball.speed > 0.1)
	{
		var vector = this._ball.vector;
		var direction = Utils.angleBetween(new Position(this._ball.position.x,this._ball.position.y),
										   new Position(this._ball.position.x+vector.x,this._ball.position.y+vector.y));
		Logger.log("direction of ball = " + direction + "\n\r",Logger.LogLevel.DEBUG);
		
		var dirToPost1 = Utils.angleBetween(new Position(this._ball.position.x,this._ball.position.y),
											new Position(this.convertX(0),this._pitch.goalY1));

		var dirToPost2 = Utils.angleBetween(new Position(this._ball.position.x,this._ball.position.y),
											new Position(this.convertX(0),this._pitch.goalY2));
											
		// dirToPost1 and 2 will never straddle 0
		var minDir = Math.min(dirToPost1,dirToPost2);
		var maxDir = Math.max(dirToPost1,dirToPost2);

		Logger.log("check if " + direction + " between " + minDir + " and " + maxDir + "\n\r",Logger.LogLevel.DEBUG);
	
		if(direction >= minDir && direction <= maxDir)
		{
			return true;
		}
	}
	return false;
	
}


TheyThinkItsAllOver.prototype._strategy_goalkeeper_defend = function(player) {
		var tempDist = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(this._ball.position.x,this._ball.position.y));
		Logger.log("dist between goalie and ball = " + tempDist + "\n\r",Logger.LogLevel.DEBUG);

		if(tempDist <= this.TAKE_POSSESSION_DIST)
		{
			Logger.log("Player " + player.playerNumber + " will steal ball\n\r",Logger.LogLevel.DEBUG);		
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "TAKE_POSSESSION";
			return action;
		}
		// its flying by us - beside us, take a leap
		/*else if(tempDist < 5.0001 && 
				this._ball.speed > 0.01 &&
				this._headingForGoal(this._vectorToDegrees(this._ball.vector)))*/
		else if(this._ballHeadingForGoal())		
		{
				// move across in front of ball
				var newPosition = this._positionInFrontOfGoal(new Position(this._ball.position.x,this._ball.position.y),7); // yards in front of goal
				var action = {};
				action.playerNumber = player.playerNumber;
				action.action = "MOVE";
				action.destination = {x:newPosition.x, y:newPosition.y};
				Logger.log("Try to get across in front of ball.  New pos = " + JSON.stringify(action.destination) + "\n\r",Logger.LogLevel.DEBUG);		
				Logger.log("Was at " + JSON.stringify(player.position) + "\n\r",Logger.LogLevel.DEBUG);		
				
				action.speed = 100.0;
				return action;
		}
				// is it in area - go to it
		else if(Utils.distanceBetween(new Position(this.convertX(0),this.convertY(25)),new Position(this._ball.position.x,this._ball.position.y)) < 17)
		{
			Logger.log("Goalkeeper has dist under 16 so move to ball\n\r",Logger.LogLevel.DEBUG);		
				var action = {};
				action.playerNumber = player.playerNumber;
				action.action = "MOVE";
				action.destination = {x:this._ball.position.x, y:this._ball.position.y};
				action.speed = 100.0;
				return action;
		}	
		else
		{
			Logger.log("Keeper move to ideal position\n\r",Logger.LogLevel.DEBUG);		
				var newPosition = this._positionInFrontOfGoal(new Position(this._ball.position.x,this._ball.position.y),7); // yards in front of goal
				var action = {};
				action.playerNumber = player.playerNumber;
				action.action = "MOVE";
				action.destination = {x:newPosition.x, y:newPosition.y};
				action.speed = 100.0;
				return action;
		}
}
TheyThinkItsAllOver.prototype._strategy_goalkeeper_freeball = function(player) {
		return this._strategy_goalkeeper_defend(player);
}



TheyThinkItsAllOver.prototype._strategy_defender_attack = function(player) {
	return this._strategy_striker_attack(player);
}
TheyThinkItsAllOver.prototype._strategy_defender_support = function(player) {
			var x = player.position.x;
			if(this.lessThanX(x,50))
			{
				x = this.convertX(50);
			}
           var action = {};
            action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:x, y:player.startPosition.y};
			action.speed = 100.0;
			return action;
}
TheyThinkItsAllOver.prototype._strategy_defender_defend = function(player) {
	return this._strategy_defender_freeball(player);
/*		var tempDist = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(this._ball.position.x,this._ball.position.y));
		Logger.log("tempDist = " + tempDist + "\n\r",Logger.LogLevel.DEBUG);

		if(tempDist <= 5)
		{
			Logger.log("Player " + player.playerNumber + " will steal ball\n\r",Logger.LogLevel.DEBUG);		
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "TAKE_POSSESSION";
			return action;
		}
		// is it on our side of the pitch
		else if(this._ballOnMySideOfPitch(player))
		{
           var action = {};
            action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:this._ball.position.x, y:this._ball.position.y};
			action.speed = 100.0;
			return action;
		}	
		else
		{
           var action = {};
            action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:this._ball.position.x, y:player.startPosition.y};
			action.speed = 100.0;
			return action;
		}	
*/
}

TheyThinkItsAllOver.prototype._strategy_defender_freeball = function(player) {
	var tempDist = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(this._ball.position.x,this._ball.position.y));
	Logger.log("dist from defender to ball = " + tempDist + "\n\r",Logger.LogLevel.DEBUG);

	if(tempDist <= this.TAKE_POSSESSION_DIST)
	{
		Logger.log("Player " + player.playerNumber + " will steal ball\n\r",Logger.LogLevel.DEBUG);		
		var action = {};
		action.playerNumber = player.playerNumber;
		action.action = "TAKE_POSSESSION";
		return action;
	}
	else if(player.closestToBall)	
	{
		var x = player.position.x;
	    var action = {};
        action.playerNumber = player.playerNumber;
		action.action = "MOVE";
		action.destination = {x:this._ball.position.x, y:this._ball.position.y};
		action.speed = 100.0;
		return action;
	}
	else if(this._weHaveControlOfBall)
	{
		var x = this.min(this._ball.position.x,this._mostForwardOtherPlayer) - this.convertInc(5);
        var action = {};
        action.playerNumber = player.playerNumber;
		action.action = "MOVE";
		action.destination = {x:x, y:player.startPosition.y};
		action.speed = 100.0;
		return action;
	
	}
	// force marking for time being for test
	else if(this._manToManMarking)
	{
		// mark
		return this._strategy_defender_freeball_mark(player);
	}	
	else if(this._ballOnMySideOfPitch(player) && this.lessThanX(this._ball.position.x,50))
	{
		// don't move to ball as they may dribble past, move in front of ball
		var x = player.position.x;
		var y = player.position.y;
		if(this.greaterThan(x,this._ball.position.x))
		{
			// chase it as its behind us
			x = this._ball.position.x;
			y = this._ball.position.y;
		}	
		else
		{
			var newPosition = this._positionInFrontOfGoal(new Position(this._ball.position.x,this._ball.position.y),
		                                              this.convertX(x)); // x yards in front of goal
			y = newPosition.y;
		}	
        var action = {};
        action.playerNumber = player.playerNumber;
		action.action = "MOVE";
		action.destination = {x:x, y:y};
		action.speed = 100.0;
		return action;
	}	
	else
	{
		// its on the other side but we can move back as necessary, and into the middle
		var x = player.startPosition.x;
		if(this.lessThanX(this._ball.position.x,50))
		{
			// move with ball
			x = this.min(this._ball.position.x,this._mostForwardOtherPlayer) - this.convertInc(5);
		}	
	    var action = {};
        action.playerNumber = player.playerNumber;
		action.action = "MOVE";
//		action.destination = {x:x, y:player.startPosition.y};
		// move to near the center of y but still on our side
		var y = 28;
		if(player.startPosition.y < 25)
		{
			y = 22;
		}	
		action.destination = {x:x, y:y};

		action.speed = 100.0;
		return action;
	}	
}



TheyThinkItsAllOver.prototype._closestOtherPlayerNotMarked = function(player) {
//xxxxx
	var minDist = 9999;
	var closestUnmarkedPlayer = null;
	
	this._otherTeam.players.forEach(function(otherPlayer) 
	{
		var tempDist = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(otherPlayer.dynamicState.position.x,otherPlayer.dynamicState.position.y));
		if(tempDist < minDist && this._markedList.indexOf(otherPlayer.playerNumber) == -1 && this.lessThanX(otherPlayer.dynamicState.position.x,50))
		{
			// found a player to mark
			closestUnmarkedPlayer = otherPlayer;
			minDist = tempDist;
		}
	},this);
	
	if(closestUnmarkedPlayer != null)
	{
		Logger.log("Player " + player.playerNumber + " will mark player " + closestUnmarkedPlayer.playerNumber + "\n\r",Logger.LogLevel.DEBUG);		
		this._markedList.push(closestUnmarkedPlayer.playerNumber);
        var action = {};
        action.playerNumber = player.playerNumber;
		action.action = "MOVE";
		var x = closestUnmarkedPlayer.dynamicState.position.x - this.convertInc(3);
		
		action.destination = {x:x,y:closestUnmarkedPlayer.dynamicState.position.y};
		action.speed = 100.0;
		return action;
	}
	
}


// marking for defenders
TheyThinkItsAllOver.prototype._strategy_defender_freeball_mark = function(player) {
	var tempDist = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(this._ball.position.x,this._ball.position.y));
	Logger.log("Mark attacke if possible\n\r",Logger.LogLevel.DEBUG);

	// find players on this end of pitch
	// find closest player not marked
	// move to him
	var rv = this._closestOtherPlayerNotMarked(player);
	if(rv != null)
	{
		return rv;
	}
	else	
	{
		// don't move to ball as they may dribble past, move in front of ball
		var x = player.position.x;
		var y = player.position.y;
		if(this.greaterThan(x,this._ball.position.x))
		{
			// chase it as its behind us
			x = this._ball.position.x;
			y = this._ball.position.y;
		}	
		else
		{
			var newPosition = this._positionInFrontOfGoal(new Position(this._ball.position.x,this._ball.position.y),
		                                              this.convertX(x)); // x yards in front of goal
			y = newPosition.y;
		}	
        var action = {};
        action.playerNumber = player.playerNumber;
		action.action = "MOVE";
		action.destination = {x:x, y:y};
		action.speed = 100.0;
		return action;
	}	
}



TheyThinkItsAllOver.prototype._strategy_midfielder_attack = function(player) {
	return this._strategy_striker_attack(player);
}

TheyThinkItsAllOver.prototype._strategy_midfielder_support = function(player) {
			var x = player.position.x;
			if(this.lessThanX(player.position.x,60))
			{
				x = player.position.x+this.convertInc(20);
			}
			
			var action = {};
            action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:x, y:player.position.y};
			action.speed = 100.0;
			return action;
}
TheyThinkItsAllOver.prototype._strategy_midfielder_defend = function(player) {
		var tempDist = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(this._ball.position.x,this._ball.position.y));
		Logger.log("tempDist = " + tempDist + "\n\r",Logger.LogLevel.DEBUG);

		if(tempDist <= this.TAKE_POSSESSION_DIST)
		{
			Logger.log("Player " + player.playerNumber + " will steal ball\n\r",Logger.LogLevel.DEBUG);		
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "TAKE_POSSESSION";
			return action;
		}
		else if(player.closestToBall)
		{
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:this._ball.position.x, y:this._ball.position.y};
			action.speed = 100.0;
			return action;
		}
		else
		{
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:player.startPosition.x, y:player.startPosition.y};
			action.speed = 100.0;
			return action;
		}
}


TheyThinkItsAllOver.prototype._strategy_midfielder_freeball = function(player) {
	var tempDist = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(this._ball.position.x,this._ball.position.y));
	Logger.log("tempDist = " + tempDist + "\n\r",Logger.LogLevel.DEBUG);

	if(tempDist <= this.TAKE_POSSESSION_DIST)
	{
			Logger.log("Player " + player.playerNumber + " will steal ball\n\r",Logger.LogLevel.DEBUG);		
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "TAKE_POSSESSION";
			return action;
	}
	else if(player.closestToBall)
	{
			var action = {};
            action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:this._ball.position.x, y:this._ball.position.y};
			action.speed = 100.0;
			return action;
	}
	else if(this._weHaveControlOfBall) // we're dribbling
	{
			Logger.log("We have ball, move up to 70\n\r",Logger.LogLevel.DEBUG);		

			var x = player.position.x;
			if(this.lessThanX(player.position.x,60))
				x = player.position.x+this.convertInc(20)
				
			var action = {};
            action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:x, y:this._ball.position.y};
			action.speed = 100.0;
			return action;
	}
	else
	{
			var action = {};
            action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:player.startPosition.x, y:player.startPosition.y};
			action.speed = 100.0;
			return action;
	
	}
}


// TO DO: turn this into func. Defenders need to only go to 50 then pass...

TheyThinkItsAllOver.prototype._strategy_striker_attack = function(player) {
// either I have the ball, we have the ball, noone has the ball or they have the ball
		var SHOOTING_DIST = 75;
		if(this.greaterThanX(player.position.x,SHOOTING_DIST) && this.greaterThanY(player.position.y,10) && this.lessThanY(player.position.y,40))
		{
			// shoot for goal.  
			Logger.log("Shoot for goal\n\r",Logger.LogLevel.DEBUG);
			this._numShots++;
			Logger.log("Num shots = " + this._numShots + "\n\r",Logger.LogLevel.DEBUG);			
			var y = this.convertY(25);
			if(this._randomShots)
			{
				y = y + (new Random().nextDouble() * 6) - 3;
			}
			Logger.log("Shooting at y = " + y + "\n\r",Logger.LogLevel.DEBUG);
			
			return this._turnAndKick(player,this.convertX(100),y,true,player.distToOpponent);
		
		}
		else if(player.distToOpponent > 10)
		{
			// dribble it to goal
			var x = player.position.x;
			var y = player.position.y;
			if(player.position.x > SHOOTING_DIST)
			{
				// to far out (first if statement saw to this), move to center
				y = this.convertY(25);
			}
			else
			{
				if(this.greaterThanY(y,35))
					y = this.convertY(35);
				else if(this.lessThanY(y,15))
					y = this.convertY(15);				
			}
			// continue to move forward (or back if we're stuck in corner)	
			if(this.greaterThanX(player.position.x,90))
			{
				x = player.position.x-this.convertInc(20);
			}
			else
			{
				x = player.position.x+this.convertInc(20);
			}
			var action = {};
            action.playerNumber = player.playerNumber;
			action.action = "KICK";
			action.destination = {x:x, y:y};
			action.speed = this.DRIBBLE_SPEED;
			return action;
		}
		else
		{
			Logger.log("Opponent is too close to player: " + player.playerNumber + "\n\r",Logger.LogLevel.DEBUG);		

			// we need to pass
			// find best player to pass to
			var bestPlayer = null;
			var mostForwardPlayer = null;
			var x = 0
			var dist = 0;
			var distToPlayer = 0;
			
			// two passes, look for a forward player who is not marked
			// if not then look for a player that's not marked

			for(i in this._playersHash)
			{	
				var playerWithoutBall = this._playersHash[i];		
				
				var safeAngle = 15;
				if(this.greaterThanX(player.position.x,50))
				{
					// we're in other half so can take some risks
					safeAngle = 10;
				}
			
				Logger.log("  First check - can we pass to " + playerWithoutBall.playerNumber + "\n\r",Logger.LogLevel.DEBUG);		
				if(!playerWithoutBall.hasBall && 
				   playerWithoutBall.distToOpponent > 8 && 
					   (this.greaterThan(playerWithoutBall.position.x,player.position.x)) &&
				   playerWithoutBall.type != "G" &&
				   this._passIsSafe2(new Position(player.position.x,player.position.y),
			                        new Position(playerWithoutBall.position.x,playerWithoutBall.position.y),safeAngle))
				{

					x = playerWithoutBall.position.x + this.convertInc(3);
					bestPlayer = playerWithoutBall;
					distToPlayer = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(bestPlayer.position.x,bestPlayer.position.y));
				}
			}
		
			if(bestPlayer == null)
			{
				// if less marked and not too far behind ball player
				for(i in this._playersHash)
				{	
					var playerWithoutBall = this._playersHash[i];		
				
					// if less marked and not too far behind ball player
					Logger.log("  Second check - can we pass to " + playerWithoutBall.playerNumber + "\n\r",Logger.LogLevel.DEBUG);		
					
					if(!playerWithoutBall.hasBall && 
					   playerWithoutBall.type != "G" &&
					   playerWithoutBall.distToOpponent > dist && 
					   // don't pass back in our half
					   (this.lessThanX(player.position.x,50) && this.greaterThan(playerWithoutBall.position.x,player.position.x) ||
					    (this.greaterThanX(playerWithoutBall.position.x,50))) &&
					   this._passIsSafe2(new Position(player.position.x,player.position.y),
			                            new Position(playerWithoutBall.position.x,playerWithoutBall.position.y),15))

					{
						dist = player.playerWithoutBall;
						bestPlayer = playerWithoutBall;
					}
				}
			}
			
			
			if(bestPlayer != null)
			{
				Logger.log("bestPlayer = " + JSON.stringify(bestPlayer) + "\n\r",Logger.LogLevel.DEBUG);		
				Logger.log("Pass to best player\n\r",Logger.LogLevel.DEBUG);		
				return this._turnAndKick(player, bestPlayer.position.x, bestPlayer.position.y,false,10);
/*				var action = {};
				action.playerNumber = player.playerNumber;
				action.action = "KICK";
				action.destination = {x:bestPlayer.position.x, y:bestPlayer.position.y};
				action.speed = Math.max(50,distToPlayer*2);
				return action;		
*/				
			}
			else
			{
				return this._dribbleSafelyPast(player);
				// TO DO: we should try to dribble round defender as nobody to pass to
				// dribble
/*				var action = {};
				action.playerNumber = player.playerNumber;
				action.action = "MOVE";
				action.destination = {x:player.position.x+this.convertInc(20), y:player.position.y};
				action.speed = 100.0;
				return action;
*/				
			}
		}
}
	
	
TheyThinkItsAllOver.prototype._strategy_striker_support = function(player) {
		// our team has the ball but not me
		// TO DO: we should try to move away from defenders or move forward
		Logger.log("Striker - we have the ball\n\r",Logger.LogLevel.DEBUG);		
		var action = {};
        action.playerNumber = player.playerNumber;
		action.action = "MOVE";
		action.destination = {x:player.position.x+this.convertInc(20), y:player.position.y};
		action.speed = 100.0;
		return action;
}

// is the ball in the other area? if so, then the goalkeeper will need to kick it
TheyThinkItsAllOver.prototype._ballInOtherArea = function(player) {
	
		var tempDist = Utils.distanceBetween(new Position(this.convertX(100),25),new Position(this._ball.position.x,this._ball.position.y));
		return tempDist < 14;
}


TheyThinkItsAllOver.prototype._startExploit1Timer = function(player) {
	this._exploit1Time = this._timeNow;
}

TheyThinkItsAllOver.prototype._exploit1Helper = function(player) {
			Logger.log("Attacker will do exploit\n\r",Logger.LogLevel.DEBUG);		
			if(this._startExploit1 == true && player.type == 'S1')
			{
				// only count once for both players
				this._exploit1.attempt++;
				this._startExploit1 = false;
			}
			
			// try to stand in front of ball
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			// end equation comes out as -1 of +1 to offset slightly
			var y = this._ball.position.y + ((25-player.startPosition.y)/10);
			Logger.log("dest y = " + y + "\n\r",Logger.LogLevel.DEBUG);

			action.destination = {x:this.convertX(82), y:y};
			action.speed = 100.0;
			return action;

}

TheyThinkItsAllOver.prototype._strategy_striker_defend = function(player) {
		var tempDist = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(this._ball.position.x,this._ball.position.y));
		Logger.log("dist from player to ball = " + tempDist + "\n\r",Logger.LogLevel.DEBUG);

		if(this._exploit1.on == true && this._ballInOtherArea() == true)
		{
			this._startExploit1Timer();
			return this._exploit1Helper(player);
		}
		else
		{
			// reset when we stop doing exploit
			this._startExploit1 = true;
		}	
		
		if(tempDist <= this.TAKE_POSSESSION_DIST)
		{
			Logger.log("Player " + player.playerNumber + " will steal ball\n\r",Logger.LogLevel.DEBUG);		
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "TAKE_POSSESSION";
			return action;
		}
		// in case they dribble
		else if(player.closestToBall) 
		{
			Logger.log("_strategy_striker_defend: position.x = " + player.position.x + "\r\n",Logger.LogLevel.DEBUG);
			Logger.log("this._ball = " + JSON.stringify(this._ball) + "\r\n",Logger.LogLevel.DEBUG);
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			
			action.destination = {x:this._ball.position.x, y:this._ball.position.y};
			action.speed = 100.0;
			return action;
			
		}	
		else if(tempDist <= 20 && this.greaterThanX(player.position.x,50))
		{
			// TO DO: Turn
			// move to the ball
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:this._ball.position.x,y:this._ball.position.y};
			action.speed = 100.0;
			return action;
		}	
		else
		{
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "MOVE";
			action.destination = {x:player.startPosition.x, y:player.startPosition.y};
			action.speed = 100.0;
			return action;	

		}
}

TheyThinkItsAllOver.prototype._strategy_striker_freeball = function(player) {
		var action = {};
		action.playerNumber = player.playerNumber;
		action.action = "MOVE";

		var distToBall = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(this._ball.position.x,this._ball.position.y));
		Logger.log("distToBall = " + distToBall + "\n\r",Logger.LogLevel.DEBUG);

		if(this._exploit1.on == true && this._ballInOtherArea() == true)
		{
			return this._exploit1Helper(player);
		}
		else
		{
			// reset when we stop doing exploit
			this._startExploit1 = true;
		}	

		
		if(distToBall <= this.TAKE_POSSESSION_DIST)
		{
			Logger.log("Player " + player.playerNumber + " will steal ball\n\r",Logger.LogLevel.DEBUG);		
			var action = {};
			action.playerNumber = player.playerNumber;
			action.action = "TAKE_POSSESSION";
			return action;
		}
		else if(player.closestToBall) 
		{
			Logger.log("_strategy_striker_freeball: position.x = " + player.position.x + "\r\n",Logger.LogLevel.DEBUG);
			Logger.log("this._ball = " + JSON.stringify(this._ball) + "\r\n",Logger.LogLevel.DEBUG);
			
			action.destination = {x:this._ball.position.x, y:this._ball.position.y};
			action.speed = 100.0;
		}	
		else if(this._weHaveControlOfBall) // we are dribbling or similar
		{
			// our player is closest to ball so assume he will pass
			var x = player.position.x+this.convertInc(20);

			if(this.greaterThanX(player.position.x,85))
				x = player.startPosition.x;
				
			// if other striker in corner, come into middle for cross
			var y = player.startPosition.y;
			if(this.greaterThanY(this._ball.position.y,37))
			{	
				y = 25;
			}	
			
			
			var hidden = false;
			// can other player with ball see me?
			var dirToBall = Utils.angleBetween(new Position(player.position.x,player.position.y), 
	                   new Position(this._ball.position.x,this._ball.position.y));
				
			//distToBall
			this._otherTeam.players.forEach(function(otherPlayer) 
			{
				// short hand
				var otherX = otherPlayer.dynamicState.position.x;
				var otherY = otherPlayer.dynamicState.position.y;
				var distToOther = Utils.distanceBetween(new Position(player.position.x,player.position.y),new Position(otherX,otherY));
				
				if(distToOther < distToBall)
				{
					var dirToOther = Utils.angleBetween(new Position(player.position.x,player.position.y), 
								new Position(otherX,otherY));

					// could be in the way\		
					var diff = this._getAngleDiff(dirToBall,dirToOther);
					if(diff < 15)
					{
						hidden = true;
						Logger.log("hidden = true (angle = " + diff + ")\r\n",Logger.LogLevel.DEBUG);

					}	
				}
			},this);// while	
			
			if(hidden)
			{
				x += this.convertInc(5);
			}
					
			Logger.log("_strategy_striker_freeball: move up as we will have ball\r\n",Logger.LogLevel.DEBUG);
			action.destination = {x:x, y:y};
			action.speed = 100.0;
			
		}
		else if(this.greaterThanX(player.position.x,50))
		{
			Logger.log("_strategy_striker_freeball: > 50: position.x = " + player.position.x + "\r\n",Logger.LogLevel.DEBUG);
			action.destination = {x:player.startPosition.x, y:player.startPosition.y};
			action.speed = 100.0;
		}	
		else
		{
			// stay still near half way line
			action.destination = {x:player.position.x, y:player.startPosition.y};
			action.speed = 0.0;
		}
		return action;
}

//-------------------------------------------------------------------------------
/**
 * _onREQUEST_PLAY
 * ---------------
 * Called when we receive a request for a PLAY update, ie instructions
 * to move players, kick etc.
 * This is where we do the hard work!!!!!!!!!!!!!!!!!!!!!!
 */
TheyThinkItsAllOver.prototype._onREQUEST_PLAY = function(data) {
    // We create an object for the reply...
    var reply = {};
    reply.requestType = "PLAY";
    reply.actions = [];

	
	for(i in this._playersHash)
	{	
		var myPlayer = this._playersHash[i];

		if(myPlayer.hasBall == true)
		{
			Logger.log("Player " + myPlayer.playerNumber + " has ball = \n\r",Logger.LogLevel.DEBUG);

			var action = this["_strategy_" + myPlayer.strategy + "_attack"](myPlayer);
			reply.actions.push(action);
		}
		else if(this._weHaveBall == true)
		{
			Logger.log("Player " + myPlayer.playerNumber + " is supporting\n\r",Logger.LogLevel.DEBUG);

			var action = this["_strategy_" + myPlayer.strategy + "_support"](myPlayer);
			reply.actions.push(action);
		}
		else if(this._theyHaveBall == 1)
		{
			Logger.log("Player " + myPlayer.playerNumber + " is defending\n\r",Logger.LogLevel.DEBUG);

			var action = this["_strategy_" + myPlayer.strategy + "_defend"](myPlayer);
			reply.actions.push(action);
		}
		else
		{
			Logger.log("Player " + myPlayer.playerNumber + " is looking for freeball = \n\r",Logger.LogLevel.DEBUG);

			var action = this["_strategy_" + myPlayer.strategy + "_freeball"](myPlayer);
			reply.actions.push(action);		
		}
    };

    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
    console.log(jsonReply);
};
