/**
 * PandaAi
 * --------------
 * Moves the players randomly.
 */


var readline = require('readline');
var UtilsLib = require('../../utils');
var Utils = UtilsLib.Utils;
var GameUtils = require('../../game/GameUtils');
var PandaAifs = require('fs');
var Position = require('./../../utils/Position');
var DeepQ = require('./deepqlearn.js')
var Logger = UtilsLib.Logger;
var LogHandler_File = UtilsLib.LogHandler_File;


// We set up logging...
var dateForLogFilePandaAi  = new Date();
var PandaAiLogFile='./log/PandAi' +dateForLogFilePandaAi.getFullYear()+'_'+dateForLogFilePandaAi.getMonth()+'_'+dateForLogFilePandaAi.getDate()+'_'+dateForLogFilePandaAi.getHours()+dateForLogFilePandaAi.getMinutes()+dateForLogFilePandaAi.getMilliseconds() + '.log';
Logger.addHandler(new LogHandler_File(PandaAiLogFile, Logger.LogLevel.DEBUG));


	 	 

// We create the AI...
var ai = new PandaAi();

/**
 * @constructor
 */
function PandaAi() {
    // We hook up stdin and stdout, and register onGameData
    // to be called when data is received over stdin...
    this._GamesPlayed=0;
    this._io = readline.createInterface({
        input: process.stdin,
        output: process.stdout
    });
    var that = this;
    this._io.on('line', function (line) {
        that.onGameData(line);
    });
   
    num_inputs = 14; //temporarily



    num_actions = 5;
        this._brain = new DeepQ.Brain(num_inputs, num_actions)
    this._brainInput=[]; //Current Brain Input except player number
    // Data about the pitch (width, height, position of goals)...
    this._pitch = {width:0, height:0};

    // The direction we're playing...
    this._direction = "";

    // The collection of players {playerNumber, playerType}...
    this._players = [];

    // The game state for the current turn...
    this._gameState = {};

    // Kickoff info, including the direction we're playing...
    this._kickoffInfo = {};

    // The last time (in game-time) that we changed the movement of
    // players. We only update every few seconds...
    this._lastTimeWeChangedMovements = 0.0;
    this._changeInterval = 10.0;
    this._GamesPlayed=0;
    this.brainLearningEnabled=true;
    this.previousGameInfo={};
}

/**
 * onGameData
 * ----------
 * Called when the game sends data to us.
 */
PandaAi.prototype.onGameData = function(jsonData) {
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
PandaAi.prototype._onERROR=function(data)
{
    PandaAiLog('Error occurred'+ JSON.stringify(data));
}

PandaAi.prototype._onEVENT = function(data) {
    // We call different functions depending on the event...
    var eventHandler = '_onEVENT_' + data.eventType;
    this[eventHandler](data);
};

/**
 * _onREQUEST
 * ----------
 * Called when we receive a request message.
 */
PandaAi.prototype._onREQUEST = function(data) {
    // We call different functions depending on the request...
    var requestHandler = '_onREQUEST_' + data.requestType;
    this[requestHandler](data);
};

//PandaAi.prototype._onERROR = function(data) {
//
//    PandaAiLog( "Error: " +data);
//};

/**
 * _onEVENT_GAME_START
 * -------------------
 * Called at the start of a game with general game info.
 */
PandaAi.prototype._onEVENT_GAME_START = function(data) {
    this._GamesPlayed=this._GamesPlayed+1;
    // We get data about the pitch...
    this._pitch = data.pitch;
    this._gameLengthSeconds=data.gameLengthSeconds;
    this._ourScore=0;
    this._theirScore=0;
    this.saveBrainToLog();
};

/**
 * _onEVENT_TEAM_INFO
 * ------------------
 * Called when we receive the TEAM_INFO event.
 */
PandaAi.prototype._onEVENT_TEAM_INFO = function(data) {
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
PandaAi.prototype._onEVENT_START_OF_TURN = function(data) {
    // We store the current game state, to use later when we get requests...
    //Reward the brain if we have possesion, punish if the other team do


    this._gameState = data.game;
    if (this._gameState.currentTimeSeconds >= this._gameLengthSeconds)
    {
        //Close to the end of the game - save the brain
        this.saveBrainToLog();
    }
    this._ball=data.ball;
    this._players=data.team1.players.concat(data.team2.players);
    this._nearestPlayerToBall=999;
        if (this.brainLearningEnabled) {

            if (this.isOpponent(this.playerWithBallIndex())) {
                this._brain.backward(-5);
            }
            else {
            	if (this.playerWithBallIndex>=0)
                {this._brain.backward(5);}
            }
        }
    this.savePreviousGameTurnInfo();
};

/**
 * _onEVENT_KICKOFF
 * ----------------
 * Called when we get the KICKOFF event.
 */
PandaAi.prototype._onEVENT_KICKOFF = function(data) {

    this._kickoffInfo = data;
    this._direction=data['team'+ this._teamNumber].direction;
};

PandaAi.prototype.saveBrainToLog=function()  {
    if(this._brain.value_net)
    {
        var brainJSON = this._brain.value_net.toJSON();
        var brainJSONString = JSON.stringify(brainJSON);
        //saveBrainToFile('Saving JSON for Brain after: ' +this._GamesPlayed + "\r\b");
//        if (!this._BrainLogSetup) {
//            Logger.addHandler(new LogHandler_File(PandaAiLogFile, Logger.LogLevel.DEBUG));
//            this._BrainLogSetup = true;
//        }
        this.saveBrainToFile (brainJSONString,this._GamesPlayed);
    }
    else
    {
        PandaAiLog('Trying to save brain - but it is empty');
    }
};

PandaAi.prototype.loadBrainFromLog=function(){
    var t = document.getElementById('tt').value;
    var j = JSON.parse(t);
    w.agents[0].brain.value_net.fromJSON(j);
    stoplearn(); // also stop learning
    gonormal();
};


/**
 * _onEVENT_GOAL
 * -------------
 * Called when we get the GOAL event.
 */
PandaAi.prototype._onEVENT_GOAL = function(data) {
    if (this.brainLearningEnabled) {
        if (data['team' + this._teamNumber].score > this._ourScore) {
            //We've scored
            this._brain.backward(50);
        }
        else {
            //They've scored
            this._brain.backward(-50);
        }
    }
};

/**
 * _onEVENT_HALF_TIME
 * ------------------
 * Called when we get the HALF_TIME event.
 */
PandaAi.prototype._onEVENT_HALF_TIME = function(data) {
};

/**
 * _onREQUEST_CONFIGURE_ABILITIES
 * ------------------------------
 * Called when we receive the request to configure abilities for our players.
 */
PandaAi.prototype._onREQUEST_CONFIGURE_ABILITIES = function(data) {
    var reply = {};
    reply.requestType = "CONFIGURE_ABILITIES";
    reply.players = [];

    this._players.forEach(function(player) {
        if (player.playerNumber==5||player.playerNumber==11)
        {
            var info = {
                playerNumber:player.playerNumber,
                kickingAbility:10,
                runningAbility:15,
                ballControlAbility:30,
                tacklingAbility:0
            };
        }
        //Odd players are attackers (2), even are defenders (3)
        else if (player.playerNumber % 2 === 0 )
        {
            var info = {
            playerNumber:player.playerNumber,
            kickingAbility:10,
            runningAbility:15,
            ballControlAbility:10,
            tacklingAbility:20
        };
        }

        else {
            var info = {
                playerNumber:player.playerNumber,
                kickingAbility:30,
                runningAbility:20,
                ballControlAbility:25,
                tacklingAbility:20
            };

        }
        reply.players.push(info);
    });

    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
    console.log(jsonReply);
};

/**
 * _onREQUEST_KICKOFF
 * ------------------
 * Called when we receive a request for player positions at kickoff.
 */
PandaAi.prototype._onREQUEST_KICKOFF = function(data) {
    // We return an empty collection of positions, so we get the defaults...
    var reply = {};
    reply.requestType = "KICKOFF";
    reply.players = [];

    // We send the data back to the game...
    var jsonReply = JSON.stringify(reply);
    console.log(jsonReply);
};

/**
 * _onREQUEST_PLAY
 * ---------------
 * Called when we receive a request for a PLAY update, ie instructions
 * to move players, kick etc.
 */
PandaAi.prototype._onREQUEST_PLAY = function(data) {
    // We create an object for the reply...
    var reply = {};
    reply.requestType = "PLAY";
    reply.actions = [];
    var brainActions = []; //Array of six numbers indicating which actions each of our players would like to take
    var gameActions = []; //The actions translated to real action decisions


        {
        for (var myBrains = 0; myBrains < 6; myBrains++) {
            if (this._teamNumber==1)
        {
                // Add characteristics for each of our player
             for(j=0;j<12;j++){
             this._brainInput[j]=this.distanceToOpponent(myBrains,j)
             }
           this._brainInput[12]=this.getDistanceToBall(myBrains);
            }
            else
            {
                 for(j=6;j<12;j++){
             this._brainInput[j-6]=this.distanceToOpponent(myBrains,j)
    this._brainInput[13]=this.getDistanceToOpponentGoalTop(myBrains);
         this._brainInput[14]=this.getDistanceToOpponentGoalBottom(myBrains);
            }
            for(j=0;j<6;j++){
             this._brainInput[j]=this.distanceToOpponent(myBrains,j)

            }
            this._brainInput[12]=this.getDistanceToBall(myBrains+6);
            this._brainInput[13]=this.getDistanceToOpponentGoalTop(myBrains+6);
         this._brainInput[14]=this.getDistanceToOpponentGoalBottom(myBrains+6);
            }
            if (this._shouldMoveTowardsBallOrTakePossession(this.getOurPlayerIndexFromPlayerNumber(myBrains)))
            {
                var actionIndex =5 ;
            }
            else
            {
//                if (this.hasGameTurnInfoChanged())
//                {
                    var actionIndex = this._brain.forward(this._brainInput);
//                }
            }


                brainActions.push(actionIndex);
        }
        //PandaAiLog("Brain Actions before storing:" + JSON.stringify(brainActions));
        //Store previous actions so we can see if they have completed before issuing another
       // this._storePreviousActionsAndInfo(brainActions);
        //PandaAiLog("Brain Actions AFTER storing:" + JSON.stringify(brainActions));
        //brainActions now contains a list of actionIndexes for each player
        //we need to transform them into JSON commands

        for (var myBrains = 0; myBrains < 6; myBrains++) {
            var playerIndex=this.getOurPlayerIndexFromPlayerNumber(myBrains);
            if (brainActions[myBrains])
            {
                gameActions.push(this.getJsonForAction(playerIndex,brainActions[myBrains]));
            }
            }

            reply.actions=gameActions;

        // We send the data back to the game...
        var jsonReply = JSON.stringify(reply);
        //PandaAiLog(jsonReply);
        console.log(jsonReply);
    }
};

function PandaAiLog(message){
    PandaAifs.appendFileSync(PandaAiLogFile,message + "\r\n");
};

PandaAi.prototype.getJsonForAction=function(playerIndex,actionIndex)
{

	var isGoalie=false;
	if (playerIndex==5||playerIndex==11)
	{isGoalie=true;}
	var hasBall=playerIndex==this.playerWithBallIndex();
    //PandaAiLog('PlayerIndex:' + playerIndex + ' ActionIndex:' + actionIndex + ' hasBall:' + hasBall + ' isGoalie:' + isGoalie);
	if ((!hasBall)&&(!isGoalie) )
	{
		//5 is take possession
        if (actionIndex==5)
            {return this.getJsonForTakePossession(playerIndex);}

        //Not goalie and don't have ball. Mark our opposite number
		if (actionIndex==0)
		{return this.getJsonForMarkingOppositePlayer(playerIndex);}
		
		if (actionIndex==1)
		{
			//move into space forwards
            if (this._direction=='RIGHT')
            {
			    return this.getJsonForMoveIntoSpace(playerIndex,0,180);
            }
            else
            {
                return this.getJsonForMoveIntoSpace(playerIndex,180,360);
            }
		}

        if (actionIndex==2)
        {
            //move into space backwards
            if (this._direction=='LEFT')
            {
                return this.getJsonForMoveIntoSpace(playerIndex,0,180);
            }
            else
            {
                return this.getJsonForMoveIntoSpace(playerIndex,180,360);
            }
        }

        if (actionIndex==3)
        {
            return this.getJsonForTakePossession(playerIndex);
        }

        if (actionIndex==4)
        {
            return this.getJsonForFaceBall(playerIndex);
        }
	}

    if ( !(hasBall) && (isGoalie) ) {
        // goalie and don't have ball.
						if (this.isBallInOurGoalArea())
						{return this.getJsonForTakePossession(playerIndex);
						}

        if (actionIndex==0||actionIndex==1)
        {
						if (this._direction=='LEFT')
						{
							var xPos=this._pitch.width-(this._pitch.goalAreaRadius/2);
						}
						else
						{
							var xPos=(this._pitch.goalAreaRadius/2);
						}
						return this.getJsonForMoveToPos(playerIndex,xPos,this._pitch.height/2);
        }


        if (actionIndex==2||actionIndex==3)
        {
						return this.getJsonForMoveBetweenCentreGoalAndBall(playerIndex);
        }

        if (actionIndex==4)
        {
            return this.getJsonForFaceBall(playerIndex);
        }
    }
    
   if ( (hasBall) && (!isGoalie) ) {
			if (actionIndex==0||actionIndex==1)
			{
				return this.getJsonForPass(this.nearestPlayerInSpace(playerIndex) );
			}				
			if (actionIndex==2)
			{return this.getJsonForMoveIntoSpace(playerIndex,0,180);}
		}
		
		if (actionIndex==3)
		{ return this.getJsonForMoveIntoSpace(playerIndex,180,360);}
		
		if (actionIndex==4)
		{return this.getJsonForShootAtBestGoalSpot();}

    if ( (hasBall) && (isGoalie) ) {
        return this.getJsonForPass(this.nearestPlayerInSpace(playerIndex) );

    }


};
PandaAi.prototype.getJsonForShootAtBestGoalSpot=function(){
	var playerIndex=this._ball.controllingPlayerNumber;
	 var xSpot=-1;
    var ySpot=-1;
	if (this._direction=='LEFT')
	{
		xSpot=0;
	}
	else
	{
		xSpot=this._pitch.width;
	}
	var goalPos=0;
	var bestGoalPos=4; //Centre goal
	var goalWidth=this._pitch.goalY2-this._pitch.goalY1;
 	var multiplier=goalWidth/8;
  var maxDist=-1;
  var playerInterceptingIndex= this._teamNumber==1 ? 11 : 5;
	for(goalPos=0;goalPos<=8;goalPos++)
	{
		var ySpot=this._pitch.goalY1+(multiplier*goalPos);
		var goalTarget=new Position(xSpot,ySpot);
		var calcDist=this.minInterceptDistance(playerInterceptingIndex,this._ball.position,goalTarget);
		if (calcDist>maxDist)
		{
			bestGoalPos=goalPos;
			maxDist=calcDist;
		}	
	}
	ySpot=this._pitch.goalY1+(multiplier*bestGoalPos);
	  var action ={};
    action.action="KICK";
    //PandaAiLog('Shoot at goal, controlling player is ' + this._ball.controllingPlayerNumber);
    action.playerNumber=this._ball.controllingPlayerNumber;
    

	action.destination={x: xSpot,y: ySpot};
    action.speed=100.0;
    return action;
    
};
PandaAi.prototype.getJsonForMoveBetweenCentreGoalAndBall=function(playerIndex)
{
	var goalXpos=this._pitch.width;
	if (this._direction=='LEFT')
	{
		goalXPos=0;
	}
	var centreGoalPos=new Position(goalXpos,this._pitch.height/2);
	var distanceToBall=this.getDistanceToBall(playerIndex);
	if (this._direction=='RIGHT')
	{
	var a=this._ball.position.x;
	var b=this._ball.position.y-this._pitch.height/2;
	}
	else
	{
	var a=this._pitch.width-this._ball.position.x;
	var b=Math.abs(this._ball.position.y-this._pitch.height/2);
		
	}
	var multiplier=(this._pitch.goalAreaRadius/2)/(Math.sqrt((a*a)+(b*b)));
	if (this._direction=='LEFT')
	{
		var newXpos=multiplier*a;
		var newYpos=multiplier*b + (this._pitch.height/2);
	}
	else
	{
		var newXpos=this._pitch.width-multiplier*a;
		var newYpos=multiplier*b + (this._pitch.height/2);
		
	}
	return this.getJsonForMoveToPos(playerIndex,newXpos,newYpos);
};

PandaAi.prototype.getJsonForFaceBall=function(playerIndex)
{
	var angleBetween=Utils.angleBetween(this._players[playerIndex].dynamicState.position,this._ball.position);
	
            var action = {};
            action.playerNumber = playerIndex;
            action.action = "TURN";
            action.direction=angleBetween;
            return action;
	
};
PandaAi.prototype.toRadians=function(angle){
	return (angle * Math.PI/180);
};

PandaAi.prototype.randomAngle=function(minAngle,maxAngle){
	
	return Math.random() * (maxAngle - minAngle) + minAngle;
	
};

PandaAi.prototype.possiblePosition=function(playerIndex,angle)
{
	var actSpeed=this.getPlayerSpeed(playerIndex);
	//x dist= hypot* sin(x)=0.01*actSpeed*sin(angle)
    //PandaAiLog('For player:' + playerIndex +' ActualMaxSpeed=' +actSpeed);
	var xMoveRight=0.01*actSpeed*Math.sin(this.toRadians(angle));
    //PandaAiLog('xMoveRgiht:'+xMoveRight);
	var yMoveDown=-0.01*actSpeed*Math.cos(this.toRadians(angle));
    //PandaAiLog('yMoveDown:'+yMoveDown);
	var newX=this._players[playerIndex].dynamicState.position.x+xMoveRight;
	var newY=this._players[playerIndex].dynamicState.position.y+yMoveDown;
    //PandaAiLog('For player:' + playerIndex + ' at angle:' + angle + '. Position will be:(x,y):('+newX+','+newY+')');
	return new Position (newX,newY);
	
};

PandaAi.prototype.minInterceptDistance=function(playerInterceptingIndex,startPointPos,endPointPos)
{
	//Minimum distance of a player to a line between two points
	var playerPosX=this._players[playerInterceptingIndex].dynamicState.position.x;
	var playerPosY=this._players[playerInterceptingIndex].dynamicState.position.y;
	var Dist=Math.abs(  (endPointPos.y-startPointPos.y )*playerPosX  - (endPointPos.x-startPointPos.x)*playerPosY + endPointPos.x*startPointPos.y-endPointPos.y*startPointPos.x   ) / Math.sqrt( (endPointPos.y-startPointPos.Y)*(endPointPos.y-startPointPos.Y) +  (endPointPos.x-startPointPos.y)* (endPointPos.x-startPointPos.y)  );
	return Dist;
};

PandaAi.prototype.nearestPlayerInSpace=function(playerIndex)
{
	//searches for the nearest player to us in space (i.e. more than 5m from opponents and ball intercept also more than 5m from opponent)
	//searchForward is a boolean telling us whether to prioritise those players ahead of us towards goal or behind
	 var firstBy=(function(){function e(f){f.thenBy=t;return f}function t(y,x){x=this;return e(function(a,b){return x(a,b)||y(a,b)})}return e})();

	var startIndex=0;
	if (this._teamNumber==2){startIndex=6;}
	var ourPlayers=[];
	for (i=0;i<6;i++)
	{
		if (i+startIndex!=playerIndex)
		{
			var checkIndex=i+startIndex;
			ourPlayers.push(
                {playerIndex:checkIndex
                ,distance:this.getDistanceToBall(checkIndex)
                ,inSpace:this.isPlayerInSpace(checkIndex),
                horizDistForward:this._direction=='LEFT'?this._players[checkIndex].dynamicState.position.x- this._ball.position.x: this._ball.position.x- this._players[checkIndex].dynamicState.position.x});
		}
	}
	//sort by inSpace,forward,nearest
	 var s = firstBy(function (v1, v2) { return v1.inSpace&&!(v2.inSpace) ? 1:0; }).thenBy(function (v1, v2) { return (v1.horizDistForward>0)&&(v2.horizDistForward<0) ? 1 : 0; }).thenBy(function (v1,v2){return v1.distance >v2.distance ? 1:0;  });
	
	 ourPlayers.sort(s);
	//PandaAiLog('List of our players ordered by inSpace:\r\n' + JSON.stringify(ourPlayers));
	return ourPlayers[0].playerIndex;

	
	};

PandaAi.prototype.isPlayerInSpace=function(playerIndex)
{
	 var oppStartIndex=6;
    if (this._teamNumber==2)
    {
        oppStartIndex=0;
    }
	 var minDist=999; //Minimum distance of opponents to the intercept
        //point of the ball at our new position
        var playerCount=0;
        while (playerCount<5&&minDist>5)
        {
            var newDist=this.distanceToOpponent(playerIndex,playerCount+oppStartIndex);
            if (newDist<minDist)
            {
                minDist=newDist;
            }
            playerCount++;
        }
        if (minDist>5)
        {return true;}
        return false;
};
	 
PandaAi.prototype.savePreviousGameTurnInfo=function()
{
    this.previousGameInfo.previousControllingPlayerNumber=this._ball.controllingPlayerNumber;
    this.previousGameInfo.inGoalArea=this.ballInWhichGoalArea();
};

PandaAi.prototype.ballInWhichGoalArea=function()
{
    if (GameUtils.positionIsInRightHandGoalArea({x:this._ball.position.x,y:this._ball.position.y}))
    {
        return 'RIGHT';
    }
    else if (GameUtils.positionIsInLeftHandGoalArea({x:this._ball.position.x,y:this._ball.position.y}))
    {
        return 'LEFT';
    }
    else
    {
        return 'NONE';
    }
};

PandaAi.prototype.hasGameTurnInfoChanged=function()
{
    PandaAiLog('Checking if game info has changed');

    if (this.previousGameInfo.previousControllingPlayerNumber!=this._ball.controllingPlayerNumber ||
        this.previousGameInfo.inGoalArea!=this.ballInWhichGoalArea() )
    {
        PandaAiLog('Game info has changed');
        return true;
    }
    PandaAiLog('Game info has NOT changed. ');
    return false;
};

PandaAi.prototype.getJsonForMoveIntoSpace=function(playerIndex,minAngle,maxAngle)
{
//What point to aim for	?
		var oppStartIndex=6;
    if (this._teamNumber==2)
    {
        oppStartIndex=0;
    }
    var startTime=Date.now();
    //Repeat while we're within 0.01 seconds /6
    
    //First try the angle we're facing
    var angle=-999;
    if ( (this._players[playerIndex].direction >=minAngle) && (this._players[playerIndex].direction <=maxAngle) )
    {
    		angle=this._players[playerIndex].direction;
    }
    else
    {
    		angle=this.randomAngle(minAngle,maxAngle);    	
    	}
    var possiblePosition=this.possiblePosition(playerIndex,angle); //In case we're already out of time
    while ((Date.now()-startTime)<0.01*1000/6)
    {
        
        var possiblePosition=this.possiblePosition(playerIndex,angle);
        var minDist=999; //Minimum distance of opponents to the intercept
        //point of the ball at our new position
        var playerCount=0;
        while (playerCount<5&&minDist>5)
        {
            var newDist=this.minInterceptDistance(playerCount+oppStartIndex,this._ball.position,possiblePosition);
            if (newDist<minDist)
            {
                minDist=newDist;
            }
            playerCount++;
        }
        if (minDist>5)
        {
            //Found a good position
            break;
        }
			var angle=this.randomAngle(minAngle,maxAngle);
    }
    //PandaAiLog('Trying to Move player:' + playerIndex + ' to pos:' + JSON.stringify(possiblePosition));

    return this.getJsonForMoveToPos(playerIndex,possiblePosition.x,possiblePosition.y)};

PandaAi.prototype.saveBrainToFile=function(encodedBrain,gamesPlayed){
    PandaAiLog('Writing brain to file' + gamesPlayed);
    PandaAifs.appendFileSync('./log/PandaAi_BrainAfter' +(gamesPlayed-1) + '_Games.txt',encodedBrain);
};

PandaAi.prototype._moveCompleted=function(playerNum)
{

    playerIndex=this.getOurPlayerIndexFromPlayerNumber(playerNum);
    //PandaAiLog('PlayerNum:' + playerNum + ' Player Index: '+playerIndex);
    //PandaAiLog(JSON.stringify(this._players));
    if (this._previousBrainActions[playerNum]==25||this._previousBrainActions[playerNum]) {
        //Check to see if the previous controlling player has changed.
        if (this._previousControllingPlayerIndex == this.playerWithBallIndex()) {
            return false;
        }
        else
        {
            return true;
        }
    }
    if (this._targetMovePosition&&(this._players[playerIndex].dynamicState.position.x==this._targetMovePosition[playerNum].x&&
            this._players[playerIndex].dynamicState.position.y==this._targetMovePosition[playerNum].y))
    {

        return true;
    }

    return false;
};
PandaAi.prototype.getJsonForTakePossession=function(playerIndex)
	{
		if (this.getDistanceToBall(playerIndex)>=5)
		{
            var action = {};
            action.playerNumber = playerIndex;
            action.action = "MOVE";
            action.destination = {x:this._ball.position.x,
                    y:this._ball.position.y};
            action.speed = 100.0;
            return action;
		}
		var action = {};
		action.playerNumber=playerIndex;
    	action.action="TAKE_POSSESSION";
    	return action;

	};

	PandaAi.prototype.getJsonForTackle=function(playerIndex)
	{
        // Tackling and taking possesion are now the same
        return this.getJsonForTakePossession(playerIndex);
	}

function actionMeaning(actionIndex)
{
  //Returns the meaning of each chosen action;
    var meaning=['Pos1','Pos2','Pos3','Pos4','Pos5','Pos5','Pos7.1','Pos8.1','Pos9.1','Pos10','Pos11','Pos12.1','Pos13.1','Pos14',
        'Pos15','Pos16.1','Pos17.1','Pos18.1','Pos19','Pos20','Pos21','Pos22','Pos23','Pos24','tackle','pass1','pass2','pass3',
    'pass4','pass5','passGoalie','goalPos1','goalPos2','goalPos3','goalPos4','goalPos5','goalPos6','goalPos7','goalPos8','possess','nothing'];
    // For movement,
    // xxxxx
    // xxxxx
    // xx0xx
    // xxxxx
    // xxxxx
    // = 24 "actions" N.B. We choose to move either 1m or 0.1m i.e. positions 7,8,9,12,13,16,17,18 are all 0.1m moves

    // We use the same brain for every player,but first input is the player number
    //24 movements, should tackle (1), player to pass to (5 including goalie), goal spot to aim for (8), should take possession (1),do nothing (1)
    // = 41 actions

    return meaning[actionIndex-1];
};

PandaAi.prototype._storePreviousActionsAndInfo=function(brainActionsToStore)
{
	//Store the previous brain actions and relevant info so we can check if they've completed or not
	this._previousBrainActions=brainActionsToStore;
    this._targetMovePosition=[];
	for(myPlayerCount=0; myPlayerCount<6;myPlayerCount++)
	{
		myActionIndex=brainActionsToStore[myPlayerCount];
        //PandaAiLog('Storing action:' + actionIndex);
		if (myActionIndex<=24)
		{
			playerIndex= this.getOurPlayerIndexFromPlayerNumber(myPlayerCount);
            playerPos=this._players[playerIndex].dynamicState.position;
	var xMoveRight=0;//negative if left
	var yMoveDown=0;//negative if up
  yMoveDown=this.getYMovement(myActionIndex);
	xMoveRight=this.getXMovement(myActionIndex);
	newPosX=playerPos.x+xMoveRight;
	newPosY=playerPos.y+yMoveDown;

            this._targetMovePosition[myPlayerCount]=new Position(newPosX, newPosY);

		}
		//25=Tackling so nothing to store)
		//26 to 39=nothing to store because instant
		//39=Take possession, nothing to store. ToDo:Just need to see if anyone has the ball yet. If not we REPEAT the command if we're still close enough rather than generating a new one,in case we tried to get possession and failed.

	}

};

PandaAi.prototype.getJsonForShootAtGoal=function(actionIndex)
	{
   var goalPos=actionIndex-32;
   //PandaAiLog('Shoot at Goal ActionIndex is:' + actionIndex + ' GoalY1 is:'+ this._pitch.goalY1 + ' goal Y2 is '+ this._pitch.goalY2 +' Goal Pos is ' + goalPos);
   var goalWidth=this._pitch.goalY2-this._pitch.goalY1;
   var multiplier=goalWidth/8;
   ySpot=this._pitch.goalY1+(multiplier*goalPos);
    var action ={};
    action.action="KICK";
    //PandaAiLog('Shoot at goal, controlling player is ' + this._ball.controllingPlayerNumber);
    action.playerNumber=this._ball.controllingPlayerNumber;
    var xSpot=-1;
    var ySpot=-1;
	if (this._direction=='LEFT')
	{
		xSpot=0;
	}
	else
	{
		xSpot=this._pitch.width;
	}

	action.destination={x: xSpot,y: ySpot};
    action.speed=100.0;
    return action;
	};

PandaAi.prototype.getJsonForPass=function(targetPlayerNum){
    //We'll return a message for passing to a player
    //6 means the goalie
    var action ={};
    action.action="KICK";
    action.playerNumber=this._ball.controllingPlayerNumber;
    PandaAiLog('For Pass: player wants to pass to targetplayernum:' + targetPlayerNum);
    action.destination=this._players[targetPlayerNum].dynamicState.position;
    action.speed=75.0;
    return action;
};

PandaAi.prototype.getJsonForMoveToPos=function(playerIndex,xPos,yPos)
{
    var action = {};
    action.playerNumber = playerIndex;
    action.action = "MOVE";
    action.destination = {x:xPos,
            y:yPos};
    action.speed = 100.0;
    return action;
};

PandaAi.prototype.getJsonForMove=function(playerIndex,actionIndex)
{
    var action = {};
    action.playerNumber = playerIndex;
    action.action = "MOVE";
    action.destination = {x:this._players[playerIndex].dynamicState.position.x+this.getXMovement(actionIndex),
            y:this._players[playerIndex].dynamicState.position.y+ this.getYMovement(actionIndex)};
    action.speed = 100.0;
    return action;
};

PandaAi.prototype.getJsonForMarkingOppositePlayer=function(playerIndex)
{
	  var action = {};
    action.playerNumber = playerIndex;
    action.action = "MOVE";
    if (this._teamNumber==1)
    {
    	var opponentIndex=playerIndex+6;
    	}
    else
    {
        var opponentIndex=playerIndex-6;
    }
    opponentDistToBall=this.getDistanceToBall(opponentIndex);
    var xMove=(5.1/opponentDistToBall)*(this._players[opponentIndex].dynamicState.position.x-this._ball.position.x);
    var xDest= this._players[opponentIndex].dynamicState.position.x + xMove;
    
    var yMove=(5.1/opponentDistToBall)*(this._players[opponentIndex].dynamicState.position.y-this._ball.position.y);
    var yDest= this._players[opponentIndex].dynamicState.position.y + yMove
    
    action.destination ={x:xDest,y:yDest};
    
    return action;
};

PandaAi.prototype.getYMovement=function(actionIndex)
{
    //PandaAiLog("getYMovement was passed actionIndex:" + actionIndex);
	var yMoveDown=0;
	if (actionIndex>=1&&actionIndex<=5)
	{
		yMoveDown=-this._largeMovementMetres;
	}
	if (actionIndex>=6&&actionIndex<=10)
	{
		yMoveDown=-this._smallMovementMetres;
	}

	if (actionIndex>=15&&actionIndex<=19)
	{
		yMoveDown=this._smallMovementMetres;
	}

	if (actionIndex>=20)
	{
		yMoveDown=this._largeMovementMetres;
	}
	return yMoveDown;

};
PandaAi.prototype.getXMovement=function(actionIndex)
{
    //PandaAiLog("getXMovement was passed actionIndex:" + actionIndex);
    var xMoveRight=0;
	if (actionIndex==2||actionIndex==7||actionIndex==12||actionIndex==16||actionIndex==21)
	{
		xMoveRight=-this._smallMovementMetres;
	}

	if (actionIndex==4||actionIndex==9||actionIndex==13||actionIndex==18||actionIndex==23)
	{
		xMoveRight=this._smallMovementMetres;
	}

	if (actionIndex==1||actionIndex==6||actionIndex==11||actionIndex==15||actionIndex==20)
	{
		xMoveRight=-this._largeMovementMetres;
	}

if (actionIndex==5||actionIndex==10||actionIndex==14||actionIndex==19||actionIndex==24)
	{
		xMoveRight=this._largeMovementMetres;
	}
	return xMoveRight;

};

PandaAi.prototype.isMoveAllowed=function(playerIndex,actionIndex)
{
	var playerPos=this._players[playerIndex].dynamicState.position;
	var xMoveRight=0;//negative if left
	var yMoveDown=0;//negative if up
    // xxxxx
    // xxxxx
    // xx0xx
    // xxxxx
    // xxxxx
  yMoveDown=this.getYMovement(actionIndex);
	xMoveRight=this.getXMovement(actionIndex);
	newPosX=playerPos.x+xMoveRight;
	newPosY=playerPos.y+yMoveDown;
	//Must be within pitch
    //PandaAiLog('Assessing move for player:'+ playerIndex);
	if (newPosX<0||newPosY<0||newPosX>this._pitch.width||newPosY>this._pitch.height)
	{
		return false;
	}

	if (playerIndex!=5&&playerIndex!=11)
	{
		if(GameUtils.positionIsInGoalArea({x:newPosX,y:newPosY})) {
        return false;
    	}
    	else
    	{
    		return true;
    	}
	}
	else
	{
		//If we're the goalie just check we're in the goal area.No need to check which one because we're already there
		if(GameUtils.positionIsInGoalArea({x:newPosX,y:newPosY})) {
        //PandaAiLog('Goalie tried to move inside goal area to pos:' + newPosX +',' + newPosY);
        return true;
    	}
    	else
    	{
            //PandaAiLog('Goalie tried to move outside goal area to pos:' + newPosX +',' + newPosY);
    		return false;
    	}
	}
};

PandaAi.prototype.getDistanceToBall = function(playerIndex) {
    //PandaAiLog('Player Index for distance to ball is:' + playerIndex);
    var playerPosition = this._players[playerIndex].dynamicState.position;
    var ballPosition = this._ball.position;
    var distance = Utils.distanceBetween(playerPosition, ballPosition);
    //PandaAiLog('Distance to ball for player:' + playerIndex + '='+ distance );
    return distance;
};

PandaAi.prototype.isActionValid=function(playerIndex,actionIndex)
{
    if (actionIndex==0)
    {
        //Zero index problem
        return false;
    }
    // playerIndex is already the right index in the players collection
    // Work out if the action is valid or not for this player (and this game state)
    //this._ballState;
    //this._gameState;
//ToDO: If we're the nearest player to the ball and it's stationary then only valid moves are when we move towards it or when
    //ToDo:we take possession (allowing for whether we are in goal area or not)
        if (this._shouldMoveTowardsBallOrTakePossession(playerIndex))
        {
            if (actionIndex==40||actionIndex==25)
            {
                return true;
            }

            	return false;
        }

		if (actionIndex>=1&&actionIndex<=24)
		{
			return this.isMoveAllowed(playerIndex,actionIndex);
		}

    if (actionIndex==25||actionIndex==40)
    {
        //This player wants to take possession or tavklr
      return this.canPlayerTakePossession(playerIndex);
    }
    if (actionIndex==41)
    {
        //Doing nothing is fine unless we have the ball
        //ToDo: Should we be allowed to "do nothing" even if we have the ball?
        return(this._players[playerIndex].dynamicState.hasBall!=true);
    }
    //Shooting action only allowed if we have the ball
    if (actionIndex>=32&&actionIndex<=39)
    {
        return (this._players[playerIndex].dynamicState.hasBall==true);
    }
    //Passing 26 to 31. Only allowed if it's not our own player AND we have the ball
    if (actionIndex>=26&&actionIndex<=31)
    {
        if(this._players[playerIndex].dynamicState.hasBall==false)
        {
            return false;
        }
        var targetPlayerIndex=this.getOurPlayerIndexFromPlayerNumber(actionIndex-26);
        //PandaAiLog('PlayerIndex:' + playerIndex+ ' wants to pass to playerIndex:' +targetPlayerIndex + 'ActionAllowed:' + (targetPlayerIndex!=playerIndex));

        return targetPlayerIndex!=playerIndex;
    }
    
    return true;
};

PandaAi.prototype.getOurPlayerIndexFromPlayerNumber=function(playerNumber)
{
    var startPlayerIndex=0;
    if (this._teamNumber==2)
    {
        startPlayerIndex=6;
    }
    return (startPlayerIndex+playerNumber);
};

    PandaAi.prototype._shouldMoveTowardsBallOrTakePossession=function(playerIndex) {
        //Work out if we should be moving towards the ball or taking possession
        // i.e. if we are the nearest of our players to the ball
        // it is stationary
        // and if we're goalie then it's in our goal area.
        if (this.playerWithBallIndex() != -1) {
            //Ball is controlled by someone
            //PandaAiLog('Ball is controlled by:' + this.playerWithBallIndex() );
            return false;
        }
        //PandaAiLog('Ball speed is:' + this._ball.speed );
//        if (this._ball.speed > 1) {
//            //Ball is not stationary
//
//            return false;
//        }
        if (this.isBallInOurGoalArea()) {
            //PandaAiLog('Ball is in goal area')
            if (playerIndex == 5 || playerIndex == 11 ) {
                //We're the goalie and it's in our goal area
                //PandaAiLog('We are the goalie so get the ball');
                return true;
            }
            else
            {
                //PandaAiLog('We are NOT the goalie');
                return false;
            }
        }
        else
        {
            if (this.getNearestOfOurPlayersToBall()==playerIndex)
            {
                //PandaAiLog('Ball is stationery and nearest player to the ball is:' +this.getNearestOfOurPlayersToBall() );
                return true;
            }
            else
            {
                return false;
            }
        }
    };

PandaAi.prototype.getNearestOfOurPlayersToBall=function()
    {
        //Get the nearest of our players to the ball (excludes goalie)
        var playerNearest=-1;
        var minDistance=999;
        if (this._nearestPlayerToBall!=999)
        {
            //Have we previously worked this out this turn?
            return this._nearestPlayerToBall
        }
        for(var i=0;i<5;i++)
        {
            var currDistanceToBall=this.getDistanceToBall(this.getOurPlayerIndexFromPlayerNumber(i));
            //PandaAiLog('Distance to the ball for player:' + i + ' is ' + currDistanceToBall);
            if (currDistanceToBall<minDistance)
            {
                minDistance=currDistanceToBall;
                playerNearest=this.getOurPlayerIndexFromPlayerNumber(i);
            }
        }
        //PandaAiLog('Nearest player to the ball is:'+ playerNearest);
        this._nearestPlayerToBall=playerNearest;
        return playerNearest;
    };

PandaAi.prototype.isBallInOurGoalArea=function()
    {
        //PandaAiLog('Team Direction is:' + this._direction);
        if (this._direction=="LEFT" &&
            GameUtils.positionIsInRightHandGoalArea({x:this._ball.position.x,y:this._ball.position.y}))
        {
            //PandaAiLog('Ball is IN our goal area.');
                return true;
        }
        else if (this._direction=="RIGHT" &&
            GameUtils.positionIsInLeftHandGoalArea({x:this._ball.position.x,y:this._ball.position.y})
            )
        {
            //PandaAiLog('Ball is IN our goal area.');
            return true;
        }
        return false;
    };

PandaAi.prototype.canPlayerTakePossession=function (playerIndex)
{

  var _playerWithBallIndex=this.playerWithBallIndex();
  if (_playerWithBallIndex==5||_playerWithBallIndex==11)
  {
  		return false; //Can't tackle goalie and ball must be controlled by someone
  }
  if (_playerWithBallIndex!=-1)
  {
  	if (!this.isOpponent(_playerWithBallIndex))
  	{
  			return false;
  	}
  }
  if(this.isBallInOurGoalArea())
  {
  	if(playerIndex==5||playerIndex==11)
			  {
			  	return true;
			  }
		else {
			  	return false;
			  }
		}
		else if (playerIndex==5||playerIndex==11)
		{
			return false;
		}
    return true;

};

PandaAi.prototype.getDistanceToOpponentGoalTop=function (playerIndex)
{
		if (this._direction=="RIGHT" )
	{
		var horizDist=this._pitch.width-this._players [playerIndex].dynamicState.position.x;
		}
		else 
		{
			 var horizDist=this._players [playerIndex].dynamicState.position.x;			
		}
		var verDist=this._pitch.goalY1- this._players [playerIndex].dynamicState.position.y;
		return Math.sqrt((horizDist*horizDist)+(verDist*verDist));
	
};

PandaAi.prototype.getDistanceToOpponentGoalBottom=function (playerIndex)
{
		if (this._direction=="RIGHT" )
	{
		var horizDist=this._pitch.width-this._players [playerIndex].dynamicState.position.x;
		}
		else 
		{
			 var horizDist=this._players [playerIndex].dynamicState.position.x;			
		}
		var verDist=this._pitch.goalY2- this._players [playerIndex].dynamicState.position.y;
		return Math.sqrt((horizDist*horizDist)+(verDist*verDist));
	
};


PandaAi.prototype.getDistanceToOpponentWithBall=function(playerIndex)
{
	i=0;
	if (this._teamNumber==2){i=5;}
	var playerWithBall=this.playerWithBallIndex();
	if ( playerWithBall!=-1)
	{
		if (this.isOpponent(playerWithBall)==false)
		{
			return -1;
		}
		return this.distanceToOpponent(playerIndex,playerWithBall);
	}
	else
	{
		return -1;
	}
};

PandaAi.prototype.isOpponent=function(playerIndex)
{
	if (this._teamNumber==1)
	{
		return (playerIndex>=6&&playerIndex<=11);
	}
	else
	{
		return (playerIndex>=0&&playerIndex<=5);
	}
};

PandaAi.prototype.playerWithBallIndex=function()
{
	return this._ball.controllingPlayerNumber;
};

PandaAi.prototype.distanceToOpponent=function(playerIndex,opponentIndex)
{
	var ourPos=this._players[playerIndex].dynamicState.position;
	var theirPos=this._players[opponentIndex].dynamicState.position;
	return Utils.distanceBetween(ourPos, theirPos);
};

PandaAi.prototype.getPlayerSpeed=function(playerIndex){

    var myPlayer=this._players[playerIndex];
    var runningAbility = myPlayer.staticState.runningAbility / 100.0;
    var speed = runningAbility * 10;

    // If we have the ball, we move slower...
    if(playerIndex==this._ball.controllingPlayerNumber) {
        speed *= 0.4;
    }

    return speed;
}