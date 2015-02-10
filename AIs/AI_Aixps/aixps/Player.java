package aixps;

class Player
{
	static int iCreatedPlayer = 0;
	static String sPlayerNames[] = { "Andres", "Bobby", "Cristiano", "David", "Eusebio", "Frank", "Gianluigi", "Hristo"
									,"Ian", "Johann", "Kaka", "Lionel", "Manuel", "Neymar",
									"Osvaldo", "Pele", "Quentin", "Ronaldo", "Steven", "Totao"
									,"Ulrich", "Victor", "Wesley", "Xavi", "Yaya", "Zinedine" };
	//---------------------------
	int iInternalNo;

	int iPlayerNo;
	int iPlayerTeam;
	boolean bAIControlled;
	String sPlayerName;

	String sPlayerType;	// P/G
	String sPlayerRole;	// A/C/D/G
	String sPlayerTempRole;

	boolean	bHasBall;

	double fKickingAbility;
	double fRunningAbility;
	double fBallControlAbility;
	double fTacklingAbility;

	double fPosX;
	double fPosY;

	double fGuardPosX;
	double fGuardPosY;

	int	   iDirection;	// 0- 360 degrees
	int    iWantedDirection;
	double fDirection;

	double fSpeed;
	double fMoveX;
	double fMoveY;

	double fDestX;
	double fDestY;

	double fKickDestX = 0;
	double fKickDestY = 0;
	double fKickSpeed = 25;
	int 	iKickCooldown = 0;

	String sAction;

	String sAIAction = "";

	//---- Possible actions
	boolean bActionKick = false;
	boolean bActionTake = false;
	boolean bActionTurn = false;
	//boolean bMoveToGoal = false;


	//----------------------------------
	String sAIMoveMode;
	String sAIStatus;
	String sAIOrder;

	//----------------------------------

	public Player()
	{
		bHasBall = false;

		sAction = "MOVE";

		fSpeed = 100;

		fPosX = 0;
		fPosY = 0;

		fDestX = 0;
		fDestY = 0;

		bAIControlled = false;

		sPlayerType = "P";

		//----------------------------
		sAIMoveMode = "PERSONAL_SMALL";
		sAIStatus = "SAFE";
		sAIOrder = "NONE";
		//----------------------------
		iInternalNo = iCreatedPlayer;
		sPlayerName = sPlayerNames[iInternalNo];
		iCreatedPlayer++;
		//System.err.println("Created player named " + sPlayerName);
	}
	
	//{\"playerNumber\":6,\"action\":\"MOVE\",\"destination\":{\"x\":25,\"y\":10},\"speed\":100}

	public void Setup(int iNoPlayer)
	{
		this.iPlayerNo = iNoPlayer;
		iPlayerTeam = 0;

		fPosX = 0;
		fPosY = 0;

		iDirection = 0;
		fKickingAbility = 66;
		fRunningAbility = 66;
		fBallControlAbility = 66;
		fTacklingAbility = 66;

		sPlayerType = "P";
		if (iNoPlayer%6 == 5)
			sPlayerType = "G";

		bHasBall = false;

		sAction = "MOVE";

		fSpeed = 100;
		fDirection = 0;

		fDestX = 0;
		fDestY = 0;		

	}

	public String buildConfigureAbilities()
	{
		String sInstruction = null;

		sInstruction = "{\"playerNumber\":" + iPlayerNo + ",\"kickingAbility\":" + fKickingAbility + ",\"runningAbility\":" + fRunningAbility + ",\"ballControlAbility\":" + fBallControlAbility + ",\"tacklingAbility\":" + fTacklingAbility + "}";	

		return sInstruction;
	}
	

	public String buildPositionCommand()
	{
		String sCommand = null;

		//{\"playerNumber\":6,\"position\":{\"x\":75,\"y\":40},\"direction\":270}

		sCommand = "{\"playerNumber\":" + iPlayerNo + ",\"position\":{\"x\":" + fPosX + ",\"y\":" + fPosY + "},\"direction\":" + fDirection + "}";
		bActionKick = false;
		bActionTake = false;
		bActionTurn = false;
		
		return sCommand;
	}

	public String buildActionMoveCommand()
	{
		String sCommand = null;
		sCommand = "{\"playerNumber\":" + iPlayerNo + ",\"action\":\"" + "MOVE" + "\",\"destination\":{\"x\":" + fDestX + ",\"y\":" + fDestY + "},\"speed\":" + fSpeed + "}";
		return sCommand;

	}

	public String buildActionTurnCommand()
	{
		String sCommand = null;
		sCommand = "{\"playerNumber\":" + iPlayerNo + ",\"action\":\"" + "TURN" + "\",\"direction\":" + iWantedDirection + "}";
		return sCommand;

	}	

	public String buildNoActionCommand()
	{
		String sCommand = null;
		sCommand = "{\"playerNumber\":" + iPlayerNo + ",\"action\":\"" + sAction + "\",\"destination\":{\"x\":" + fPosX + ",\"y\":" + fPosY + "},\"speed\":" + fSpeed + "}";
		return sCommand;
	}

	public String buildActionCommand()
	{
		String sCommand = null;
		boolean bPrintDebug = false;

		// Default action is MOVE
		//	A null action should be a MOVE to the current position
		if (bPrintDebug)
			System.err.println("Building command for " + getPlayerName());
		
		if (sAIOrder.equals("MOVE"))
		{
			bActionKick = false;
			bActionTake = false;
			bActionTurn = false;			
		}

		if (bActionKick == true && bHasBall == true)
		{
			sAction = "KICK";
			//{"action":"KICK","playerNumber":6,"destination":{"x":25,"y":15},"speed":100}
			sCommand = "{\"playerNumber\":" + iPlayerNo + ",\"action\":\"" + "KICK" + "\",\"destination\":{\"x\":" + fKickDestX + ",\"y\":" + fKickDestY + "},\"speed\":" + fKickSpeed + "}";

			if (bPrintDebug)
			{
				System.err.println("*********************************************");
				System.err.println("Player " + iPlayerNo + " KICKS! @" + fKickSpeed + " to " + fKickDestX + "/" + fKickDestY);
				System.err.println("*********************************************");
			}
		}

		else if (bActionTake == true || bActionKick == true)
		{
			//{"playerNumber":7,"action":"TAKE_POSSESSION"}
			sAction = "TAKE_POSSESSION";
			sCommand = "{\"playerNumber\":" + iPlayerNo + ",\"action\":\"" + "TAKE_POSSESSION" + "\"}";

			if (bPrintDebug)
				System.err.println("Player " + iPlayerNo + " asks for TAKE BALL!");
			
			bActionTake = false;
		}
		else if (bActionTurn == true)
		{
			sAction = "TURN";
			sCommand = buildActionTurnCommand();
		}
		else
		{
			sAction = "MOVE";
			sCommand = buildActionMoveCommand();
		}
		
		if (bPrintDebug)
			System.err.println("Command successfully built!");

		bActionKick = false;
		bActionTake = false;
		bActionTurn = false;
		
		return sCommand;
	}

	public String toString()
	{
		String sDisplayInfo = "Ply: " + iPlayerNo + "[" + sPlayerType + "<" + sPlayerName + ">" + " (AI " + bAIControlled + " Pos = " + fPosX + " / " + fPosY + " AI = " + sAIMoveMode + " SpeedKick: " + fKickSpeed + " cool: " + iKickCooldown
			+ " ((AI: " + sAIMoveMode + "/" + sAIStatus + "/" + sAIOrder;
		return sDisplayInfo;
	}

	public String getPlayerName()
	{
		return sPlayerName;
	}

	
}