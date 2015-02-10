package aixps;
import aixps.Player;

import aixps.UtilPoint;

class GameState
{


	public Player[] xPlayersTable = null;

	int iInitialGameSeconds = 0;
	int iHalfGameSeconds = 0;
	int iCurrentHalfTimeRemaining = 0;
	int iCurrentTicksLeft = 0;
	int iCurrentHalf = 0;

	double fBallX = 0;
	double fBallY = 0;
	double fBallVectorX = 0;
	double fBallVectorY = 0;
	double fBallSpeed = 0;

	int iTeamNumber = 0;
	int iOpponentTeamNumber = 0;

	int iScore = 0;
	int iOpponentScore = 0;

	String sTeamName = null;
	String sOpponentTeamName = null;

	int iFieldSide = 0;		// 0 = left / 1 = right
	int iOpponentFieldSide = 0;
	String sTeamDirection = null;
	String sOpponentTeamDirection = null;

	int iFieldSizeX = 0;
	int iFieldSizeY = 0;

	int iGoalCenterX = 0;
	int iGoalCenterY = 0;

	int iOpponentGoalCenterX = 0;
	int iOpponentGoalCenterY = 0;

	int iFirstPlayer = 0;
	int iLastPlayer = 0;

	int iPossessionDelay = 0;

	int iPitchWidth;
	int iPitchHeight;
	int iPitchGoalCentre;
	int iPitchGoalY1;
	int iPitchGoalY2;
	int iPitchCentreX;
	int iPitchCentreY;
	int iPitchCentreRadius;
	int iPitchGoalAreaRadius;

	public GameState()
	{
		int iNoPlayer = 0;
		xPlayersTable = new Player[12];

		//--- Init somze default pitch
		iPitchWidth = 100;
		iPitchHeight = 50;
		iPitchGoalCentre = 25;
		iPitchGoalY1 = 21;
		iPitchGoalY2 = 29;
		iPitchCentreX = 50;
		iPitchCentreY = 25;
		iPitchCentreRadius = 10;
		iPitchGoalAreaRadius = 15;

	}

	public void displayInfo()
	{
		System.err.println("Score AI: " + iScore + " Opponent: " + iOpponentScore);
		System.err.println("Total Time: " + iInitialGameSeconds + " Half Time: " + iHalfGameSeconds + " Current Half: " + iCurrentHalf + " Remaining: " + iCurrentHalfTimeRemaining);
	}

	public void updateCycle(int iStep)
	{
		iCurrentTicksLeft -= iStep;
		iCurrentHalfTimeRemaining = iCurrentTicksLeft/10;
	}

	public void setInitialState()
	{
		;
	}

	public void setPitch(int iWidth, int iHeight, int iGoalCentre, int iGoalY1, int iGoalY2,
						int iCentreX, int iCentreY, int iCentreRadius, int iGoalAreaRadius)
	{
		iPitchWidth = iWidth;
		iPitchHeight = iHeight;
		iPitchGoalCentre = iGoalCentre;
		iPitchGoalY1 = iGoalY1;
		iPitchGoalY2 = iGoalY2;
		iPitchCentreX = iCentreX;
		iPitchCentreY = iCentreY;
		iPitchCentreRadius = iCentreRadius;
		iPitchGoalAreaRadius = iGoalAreaRadius;
	}


	public void setInitialTime(int iGameLengthSeconds)
	{
		iInitialGameSeconds = iGameLengthSeconds;
		iHalfGameSeconds = iInitialGameSeconds/2;
		iCurrentHalfTimeRemaining = iHalfGameSeconds;
		iCurrentTicksLeft = iCurrentHalfTimeRemaining*10;
		iCurrentHalf = 0;
	}

	public void setHalfTime()
	{
		iCurrentHalfTimeRemaining = iHalfGameSeconds;
		iCurrentTicksLeft = iCurrentHalfTimeRemaining*10;		
		iCurrentHalf = iCurrentHalf+1;		
	}

	public void setBallPosition(double fPosX, double fPosY)
	{
		fBallX = fPosX;
		fBallY = fPosY;
		//System.err.println("New ball position: " + this.fBallX + "/" + this.fBallY);
	}

	public void setBallVector(double fVectorX, double fVectorY)
	{
		fBallVectorX = fVectorX;
		fBallVectorY = fVectorY;
	}

	public void setBallSpeed(double fSpeed)
	{
		fBallSpeed = fSpeed;
	}

	public void setControlledTeam(int iTeamNumber)
	{
		this.iTeamNumber = iTeamNumber;
  		if (iTeamNumber == 1)
  		{
  			this.iFirstPlayer = 0;
  			this.iLastPlayer = 5;
  		}
  		if (iTeamNumber == 2)
  		{
  			this.iFirstPlayer = 6;
  			this.iLastPlayer = 11;
  		}
	}

	public void setPlayer(int iPlayerNo, String sTag, String sPlayerType)
	{
		xPlayersTable[iPlayerNo].sPlayerType = sPlayerType;
		if (sTag.equals("AI"))
			xPlayersTable[iPlayerNo].bAIControlled = true;
	}

	public void setTeamInfo(int iTeamIndex, String sName, int iScore, String sDirection)
	{
		double fPosX = 0;
		double fPosY = 0;
		int iNoPlayer = 0;

		//System.err.println("TeamIndex: " + iTeamIndex + " TeamName: " + sName + " Score: " + iScore + " Dir: " + sDirection);
		if (iTeamIndex != iTeamNumber)
		{
			this.iOpponentScore = iScore;
			this.sOpponentTeamName = sName;
			this.sOpponentTeamDirection = sDirection;
		}

		if (iTeamIndex == iTeamNumber)
		{
			this.iScore = iScore;
			this.sTeamName = sName;
			this.sTeamDirection = sDirection;
			if (this.sTeamDirection.equals("LEFT"))	// possess the RIGHT side, go to the LEFT
			{
				this.iFieldSide = 1;
				iGoalCenterX = iPitchWidth;
				iGoalCenterY = iPitchGoalCentre;
				iOpponentGoalCenterX = 0;
				iOpponentGoalCenterY = iPitchGoalCentre;					
			}
			else
			{
				this.iFieldSide = 0;
				iGoalCenterX = 0;
				iGoalCenterY = iPitchGoalCentre;
				iOpponentGoalCenterX = iPitchWidth;
				iOpponentGoalCenterY = iPitchGoalCentre;					
			}

		}
	
	}
};
