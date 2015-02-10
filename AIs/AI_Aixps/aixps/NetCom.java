package aixps;

import aixps.GameState;
import aixps.AI;

import javax.json.*;
import javax.json.stream.JsonParser;
import javax.json.stream.JsonParser.Event;

import org.json.simple.JSONArray;
import org.json.simple.JSONObject;
import org.json.simple.parser.JSONParser;
import org.json.simple.parser.ParseException;

import java.io.IOException;
import java.io.StringReader;
import java.io.InputStreamReader;

import java.lang.Math;

import java.util.Scanner;

//import java.util.StringTokenizer;

class NetCom
{

	GameState	xGameState = null;
	AI 			xAI = null;
	JsonReaderFactory factory = null;

	String sMessageType = null;
	String sGeneralType = null;

	public NetCom()
	{
	 	factory = Json.createReaderFactory(new java.util.HashMap());			
	}

	public void setGameState(GameState xGameState)
	{
		this.xGameState = xGameState;
	}

	public void setAI(AI xAI)
	{
		this.xAI = xAI;
	}

	// A fake response
	public String buildResponseBeep()
	{
		String sResponse;

		sResponse = "{\"requestType\":\"BEEP\"}";

		return sResponse;
	}	

	public String buildResponseConfigureAbilities()
	{
		String sResponse;
		int iNoPlayer = 0;
		int iPlayerCnt = 0;

		sResponse = "{\"requestType\":\"CONFIGURE_ABILITIES\",\"players\":[";

		for (iNoPlayer = xGameState.iFirstPlayer; iNoPlayer <= xGameState.iLastPlayer; iNoPlayer++)
		{
			Player xPlayer = null;
			// Replace general table with AI players
			if (xAI != null)
			{
				xAI.xAIPlayersTable[iPlayerCnt].iPlayerNo = iNoPlayer;
				xGameState.xPlayersTable[iNoPlayer] = xAI.xAIPlayersTable[iPlayerCnt];
			}

			String sPlayerCommand = xGameState.xPlayersTable[iNoPlayer].buildConfigureAbilities();
			String sSeparator = ",";
			if (iNoPlayer == xGameState.iFirstPlayer)
				sSeparator = "";
			if (sPlayerCommand != null)
				sResponse = sResponse + sSeparator + sPlayerCommand;

			iPlayerCnt++;
		}

		sResponse = sResponse + "]}";

		return sResponse;
	}	

	public String buildResponseKickOff()
	{
		String sResponse;
		int iNoPlayer = 0;
//		sResponse = 
//		"{\"requestType\":\"KICKOFF\",\"players\":[{\"playerNumber\":6,\"position\":{\"x\":75,\"y\":40},\"direction\":270},{\"playerNumber\":7,\"position\":{\"x\":75,\"y\":25},\"direction\":270},{\"playerNumber\":8,\"position\":{\"x\":75,\"y\":10},\"direction\":270},{\"playerNumber\":9,\"position\":{\"x\":51,\"y\":36},\"direction\":270},{\"playerNumber\":10,\"position\":{\"x\":51,\"y\":14},\"direction\":270}]}";

		sResponse = "{\"requestType\":\"KICKOFF\",\"players\":[";

		for (iNoPlayer = xGameState.iFirstPlayer; iNoPlayer <= xGameState.iLastPlayer; iNoPlayer++)
		{
			String sPlayerCommand = xGameState.xPlayersTable[iNoPlayer].buildPositionCommand();
			String sSeparator = ",";
			if (iNoPlayer == xGameState.iFirstPlayer)
				sSeparator = "";
			if (sPlayerCommand != null)
				sResponse = sResponse + sSeparator + sPlayerCommand;
		}

		sResponse = sResponse + "]}";

		if (xAI != null)
			xAI.bActive = true;

		return sResponse;
	}

	public String buildResponsePlay()
	{
		String sResponse = null;
		int iNoPlayer = 0;
		//sResponse = 
		//"{\"requestType\":\"PLAY\",\"actions\":[{\"playerNumber\":6,\"action\":\"MOVE\",\"destination\":{\"x\":25,\"y\":10},\"speed\":100},{\"playerNumber\":7,\"action\":\"MOVE\",\"destination\":{\"x\":25,\"y\":25},\"speed\":100},{\"playerNumber\":8,\"action\":\"MOVE\",\"destination\":{\"x\":25,\"y\":40},\"speed\":100},{\"playerNumber\":9,\"action\":\"MOVE\",\"destination\":{\"x\":52.239,\"y\":19.314},\"speed\":100},{\"playerNumber\":10,\"action\":\"MOVE\",\"destination\":{\"x\":52.239,\"y\":19.314},\"speed\":100},{\"playerNumber\":11,\"action\":\"MOVE\",\"destination\":{\"x\":4.9706420413880341,\"y\":24.458966085734176},\"speed\":100}]}";


		sResponse = "{\"requestType\":\"PLAY\",\"actions\":[";

		if (xGameState != null)
		{
			for (iNoPlayer = xGameState.iFirstPlayer; iNoPlayer <= xGameState.iLastPlayer; iNoPlayer++)
			{
				String sPlayerCommand = null;
				String sSeparator = ",";
				Player xPlayer = null;
				
				xPlayer = xGameState.xPlayersTable[iNoPlayer];

				if (xPlayer != null)
					sPlayerCommand = xPlayer.buildActionCommand();
				
				if (iNoPlayer == xGameState.iFirstPlayer)
					sSeparator = "";
				if (sPlayerCommand != null)
					sResponse = sResponse + sSeparator + sPlayerCommand;
			}
		}

		sResponse = sResponse + "]}";

		return sResponse;
	}	

	public void readMessage(String sMessage)
	{
      //JSONParser jsonParser = new JSONParser();
      //JSONObject jsonObject = (JSONObject) jsonParser.parse(sMessage);

  		JsonReader jsonReader = factory.createReader(new StringReader(sMessage));
  		JsonObject jsonObject = jsonReader.readObject();

      //System.err.println("Message object: " + jsonObject);
  		/*String*/ sMessageType = jsonObject.getString("messageType");
  		//String sMessageType = jsonObject.getString("Type");
  		//System.err.println("Type: " + sMessageType);
  		/*String*/ sGeneralType = null;
  		if (sMessageType.equals("EVENT"))
  			sGeneralType = jsonObject.getString("eventType");
  		else if (sMessageType.equals("REQUEST"))
  			sGeneralType = jsonObject.getString("requestType");
  		
  		//System.err.println("Message:" + sMessageType + " General: " + sGeneralType);
  		//System.err.println(jsonObject);

  		if (sMessageType.equals("EVENT") && sGeneralType.equals("GAME_START"))
  		{
  			// GAME_START
  			//	pitch
  			//		width
  			//		height
  			//		goalCentre
  			//		goalY1
  			//		goalY2
  			//		centreSpot
  			//			x
  			//			y
  			//		centreCircleRadius
  			//		goalAreaRadius
  			//	gameLengthSeconds

  			int iGameLengthSeconds = 0;
			int iWidth = 0;
			int iHeight = 0;
			int iGoalCentre = 0;
			int iGoalY1 = 0;
			int iGoalY2 = 0;
			int iCentreX = 0;
			int iCentreY = 0;
			int iCentreRadius = 0;
			int iGoalAreaRadius = 0;

  			if (jsonObject != null)
  			{
	  			iGameLengthSeconds = jsonObject.getInt("gameLengthSeconds");

				{
					JsonObject jPitch = jsonObject.getJsonObject("pitch");
					if (jPitch != null)
					{
						iWidth = jPitch.getInt("width");
						iHeight = jPitch.getInt("height");
						iGoalCentre = jPitch.getInt("goalCentre");
						iGoalY1 = jPitch.getInt("goalY1");
						iGoalY2 = jPitch.getInt("goalY2");
						JsonObject jCentreSpot = jPitch.getJsonObject("centreSpot");
						if (jCentreSpot != null)
						{
							iCentreX = jCentreSpot.getInt("x");
							iCentreY = jCentreSpot.getInt("y");	
						}					
						iCentreRadius = jPitch.getInt("centreCircleRadius");
						iGoalAreaRadius = jPitch.getInt("goalAreaRadius");
					}

				}
			}

			xGameState.setInitialTime(iGameLengthSeconds);
			xGameState.setPitch(iWidth, iHeight, iGoalCentre, iGoalY1, iGoalY2, iCentreX, iCentreY, iCentreRadius, iGoalAreaRadius);
  			xGameState.setInitialState();
			
  			
  		}

		if (sMessageType.equals("EVENT") && sGeneralType.equals("TEAM_INFO"))
		{
			// {"eventType":"TEAM_INFO","teamNumber":1,"players":[{"playerNumber":0,"playerType":"P"},{"playerNumber":1,"playerType":"P"},{"playerNumber":2,"playerType":"P"},{"playerNumber":3,"playerType":"P"},{"playerNumber":4,"playerType":"P"},{"playerNumber":5,"playerType":"G"}],"messageType":"EVENT"}
			//	teamnumber
			//	players[]
			//		playerNumber
			//		playerType

			int iTeamNumber = 0;
			JsonArray jPlayers = null;
			int iSize = 0;
			iTeamNumber = jsonObject.getInt("teamNumber");
			xGameState.setControlledTeam(iTeamNumber);
			jPlayers = jsonObject.getJsonArray("player");
			if (jPlayers != null)
			{
				iSize = jPlayers.size();

				for (int iIndex = 0; iIndex < iSize; iIndex++)
				{
					JsonObject jPlayer = jPlayers.getJsonObject(iIndex);
					if (jPlayer != null)
					{
			  			int iPlayerNo = jPlayer.getInt("playerNumber");
			  			String sPlayerType = jPlayer.getString("playerType");
			  			xGameState.setPlayer(iPlayerNo, "AI", sPlayerType);
			  		}
				}
			}	
		}  		

		if (sMessageType.equals("EVENT") && sGeneralType.equals("HALF_TIME"))
		{
			xGameState.setHalfTime();
		}  		

		if (sMessageType.equals("EVENT") && sGeneralType.equals("GOAL"))
		{
			;	// can be ignored and use START_OF_TURN for all infos
		}  		

		if (sMessageType.equals("EVENT") && sGeneralType.equals("KICKOFF"))
		{
			JsonObject jTeam = null;
			String sDirection = null;
			String sName = null;
			int iScore = 0;
			int iTeamKickingOff = 0;

			// {"eventType":"KICKOFF","team1":{"name":"","score":0,"direction":"RIGHT"},"team2":{"name":"","score":0,"direction":"LEFT"},"teamKickingOff":1,"messageType":"EVENT"}
			// team1
			//		name : string
			//		score : number
			//		direction : string
			//	team2
			//		name
			//		score
			//		direction
			//	teamKickingOff : number

			if (jsonObject != null)
			{
				int iTeamIndex = 1;
				for (iTeamIndex = 1; iTeamIndex <= 2; iTeamIndex++)
				{
					int iSize = 0;				
					String sTeam = "team" + iTeamIndex;
					
					jTeam = jsonObject.getJsonObject(sTeam);
					if (jTeam != null)
					{
						sDirection = jTeam.getString("direction");
						sName = jTeam.getString("name");
						iScore = jTeam.getInt("score");
						xGameState.setTeamInfo(iTeamIndex, sName, iScore, sDirection);
					}

				}
				iTeamKickingOff = jsonObject.getInt("teamKickingOff");
			}

			if (xAI != null)
			{
				xAI.setupKickoffPositions(iTeamKickingOff);
			}
		}  		


		//--- START OF TURN : General info about ball & players positions
		if (sMessageType.equals("EVENT") && sGeneralType.equals("START_OF_TURN"))
		{
			// START_OF_TURN is composed like this:
			//	game
			//		currentTimeSeconds : decimal
			//	ball
			//		position
			//			x : double
			//			y : double
			//		vector
			//			x : double
			//			y : double
			//		speed : number
			//		controllingPlayerNumber : number (-1 if not controlled)
			//	team1
			//		team
			//			name : string
			//			score : number
			//			direction : string (RIGHT or LEFT) then this team controls the opposite side of this direction
			//		players [table]
			//			staticState
			//				playerNumber : number
			//				playerType : string (P or G)
			//				kickingAbility : number
			//				runningAbility : number
			//				ballControlAbility : number
			//				tacklingAbility : number
			//			dynamicState
			//				position
			//					x : double
			//					y : double			
			//				hasBall : boolean
			//				energy : number
			//				direction : double

			{
				JsonObject jGame = jsonObject.getJsonObject("game");
				double dCurrentTimeSeconds = 0;
				JsonNumber jCurrentTimeSeconds = jGame.getJsonNumber("currentTimeSeconds");
				dCurrentTimeSeconds = jCurrentTimeSeconds.doubleValue();
			}

			int iNoPlayer = 0;
			//System.err.println("START_OF_TURN: Ball");
			JsonObject jBall = jsonObject.getJsonObject("ball");
			if (jBall != null)
			{
				JsonObject jBallPosition = jBall.getJsonObject("position");
				
				if (jBallPosition != null)
				{
					JsonNumber jBallPosX = jBallPosition.getJsonNumber("x");
					JsonNumber jBallPosY = jBallPosition.getJsonNumber("y");
					double fBallPosX = jBallPosX.doubleValue();
					double fBallPosY = jBallPosY.doubleValue();
					xGameState.setBallPosition(fBallPosX, fBallPosY);

				}
				
				JsonObject jBallVector = jBall.getJsonObject("vector");
				if (jBallVector != null)
				{
					JsonNumber jBallVectorX = jBallVector.getJsonNumber("x");
					JsonNumber jBallVectorY = jBallVector.getJsonNumber("y");
					double fBallVectorX = jBallVectorX.doubleValue();
					double fBallVectorY = jBallVectorY.doubleValue();
					xGameState.setBallVector(fBallVectorX, fBallVectorY);
				}
				JsonNumber jBallSpeed = jBall.getJsonNumber("speed");
				if (jBallSpeed != null)
				{
					double dSpeed = jBallSpeed.doubleValue();
					xGameState.setBallSpeed(dSpeed);
				}
				int iControlPlayer = jBall.getInt("controllingPlayerNumber");

			}
			
			//--- Get team & player information!
			//System.err.println("START_OF_TURN: Teams");
			int iTeamIndex = 1;
			for (iTeamIndex = 1; iTeamIndex <= 2; iTeamIndex++)
			{
				int iSize = 0;				
				String sTeam = "team" + iTeamIndex;
				//System.err.println("Reading team: " + sTeam);
				
				JsonObject jTeamX = null;
				JsonArray jPlayers = null;
				JsonObject jTeam = null;
				String sDirection = null;
				String sName = null;
				String sScore = null;
				int iScore = -1;

				jTeamX = jsonObject.getJsonObject(sTeam);
				if (jTeamX != null)
				{
					//System.err.println(sTeam + "json object found!");
					jPlayers = jTeamX.getJsonArray("players");
					jTeam = jTeamX.getJsonObject("team");
				}


				//if (xGameState.iTeamNumber == iTeamIndex)
				if (jTeam != null)
				{
					sDirection = jTeam.getString("direction");
					sName = jTeam.getString("name");
					iScore = jTeam.getInt("score");
				}

				xGameState.setTeamInfo(iTeamIndex, sName, iScore, sDirection);

				if (jPlayers != null)
					iSize = jPlayers.size();
				
				//System.err.println("START_OF_TURN: Players");
				for (int iIndex = 0; iIndex < iSize; iIndex++)
				{
					Player xPlayer = null;
					JsonObject jPlayer = jPlayers.getJsonObject(iIndex);
			  		JsonObject jStaticState = jPlayer.getJsonObject("staticState");
			  		int 	iStaticPlayerNo = jStaticState.getInt("playerNumber");
			  		JsonObject jDynamicState = jPlayer.getJsonObject("dynamicState");
			  		JsonObject jPlayerPosition = jDynamicState.getJsonObject("position");
			  		double fPlayerPosX = jPlayerPosition.getJsonNumber("x").doubleValue();
			  		double fPlayerPosY = jPlayerPosition.getJsonNumber("y").doubleValue();
			  		boolean bHasBall = jDynamicState.getBoolean("hasBall");
//			  		double fEnergy = jDynamicState.getJsonNumber("energy").doubleValue();
//			  		double fDirection = jDynamicState.getJsonNumber("direction").doubleValue();
			  		//int iEnergy = jDynamicState.getInt("energy");
			  		int iDirection = jDynamicState.getInt("direction");
//			  		System.err.println("Energy: " + iEnergy + " Direction: " + iDirection);

			  		xPlayer = xGameState.xPlayersTable[iStaticPlayerNo];
			  		if (xPlayer == null)	// This should create only players for opponent team
			  			xPlayer = new Player();
			  		if (xPlayer != null)
			  		{
						xPlayer.fPosX = (float) fPlayerPosX;
						xPlayer.fPosY = (float) fPlayerPosY;
						xPlayer.bHasBall = bHasBall;
						xPlayer.iDirection = iDirection;
			  		}
			  		xGameState.xPlayersTable[iStaticPlayerNo] = xPlayer;
			  	}
			}
		}
		//System.err.println("START_OF_TURN: Ended.");

		

	}

	public void respond()
	{
		String sResponse = null;
		
		if (sMessageType.equals("REQUEST") && sGeneralType.equals("CONFIGURE_ABILITIES"))
			sResponse = buildResponseConfigureAbilities();
		if (sMessageType.equals("REQUEST") && sGeneralType.equals("KICKOFF"))
			sResponse = buildResponseKickOff();
		if (sMessageType.equals("REQUEST") && sGeneralType.equals("PLAY"))
			sResponse = buildResponsePlay();

		if (sResponse != null)
		{
			//System.err.println(sResponse);
			System.out.println(sResponse);
		}
	}

};