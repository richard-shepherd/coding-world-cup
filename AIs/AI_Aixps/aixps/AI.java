/* Pseudo AI written by Guillaume Bastard, 2015
 for "Virtual World Cup" / 2015 CIB Coding Challenge
*/
package aixps;

import aixps.GameState;

import java.util.Random;
import java.lang.Math;

import aixps.UtilAngle;
import aixps.UtilPoint;
import aixps.UtilVector;

class AI
{
	GameState	xGameState = null;
	boolean		bActive = false;

	UtilPoint	pBall;
	UtilVector	vBall;
	double		dBallSpeed;

	UtilPoint	pBallFuture;

	public Player[] xAIPlayersTable = null;

	boolean 	bAttackerHasBall = false;
	boolean 	bDefenderHasBall = false;
	boolean 	bCentralHasBall = false;
	boolean 	bGoalHasBall = false;

	double fSideFactor = 1;

	boolean		bZigZagAttack = true;

	boolean		bAttackedOff = false;

	double		dAttackerZone = 50;
	double		dHalfZone = 50;

	int 		iScoreDiff = 0;


	public AI()
	{
		pBall = new UtilPoint();
		vBall = new UtilVector();
		pBallFuture = new UtilPoint();

		xAIPlayersTable = new Player[6];

		bActive = false;
	}

	public int getAngleDeviation(int iAngle1, int iAngle2)
	{
		iAngle1 = (iAngle1 + 360)%360;
		iAngle2 = (iAngle2 + 360)%360;

		return iAngle2 - iAngle1;
	}

	public int getAbsAngleDeviation(int iAngle1, int iAngle2)
	{
		int iAngleDev = getAngleDeviation(iAngle1, iAngle2);
		int iAngleAbs = Math.abs(iAngleDev);

		return iAngleAbs;
	}	

	public double getDistanceFromCoords(double x1, double y1, double x2, double y2)
	{
		double d = 9999;

		//System.err.println("coords: x:" + x1 + " y:" + y1 + "x2:" + x2 + "y2:" + y2);
		d = Math.sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));

		return d;
	}

	public double getDistance(UtilPoint p1, UtilPoint p2)
	{
		double d = 9999;
		
		if (p1 != null & p2 != null)
			d = getDistanceFromCoords(p1.dValueX, p1.dValueY, p2.dValueX, p2.dValueY);

		return d;
	}

	public void setGameState(GameState xState)
	{
		this.xGameState = xState;
	}

	public void estimateBallFuturePosition()
	{
		double dFutureX = 0;
		double dFutureY = 0;

		double dFinalX = 0;
		double dFinalY = 0;
		double dFinalBallDisplacement = 0;

		pBall.set(xGameState.fBallX, xGameState.fBallY);
		vBall.set(xGameState.fBallVectorX, xGameState.fBallVectorY);
		dBallSpeed = xGameState.fBallSpeed / 10.0;

		// Cumulative sum of fBallSpeed ..
		dFinalBallDisplacement = (xGameState.fBallSpeed * (xGameState.fBallSpeed + 1) ) / 2;

		// Apply vector on point
		dFutureX = xGameState.fBallX + (xGameState.fBallVectorX * dBallSpeed);
		dFutureY = xGameState.fBallY + (xGameState.fBallVectorY * dBallSpeed);

		dFinalX = xGameState.fBallX + (xGameState.fBallVectorX * dFinalBallDisplacement);
		dFinalY = xGameState.fBallY + (xGameState.fBallVectorY * dFinalBallDisplacement);

		pBallFuture.set(dFinalX, dFinalY);
	}

	//--------------------------------------------------------------------------
	//	MAIN AI CYCLE
	//--------------------------------------------------------------------------
	public void evaluateSituation()
	{
		int iNoPlayer = 0;

		double[] 	dTabDistanceFromBall = new double[12];
		double[] 	dTabDistanceFromFutureBall = new double[12];
		UtilAngle[] xTabAnglesToOpponentGoal = new UtilAngle[12];
		double[]	dTabDistanceFromOpponentGoal = new double[12];
		double[]	dTabDistanceFromGuard = new double[12];
		double		dDistanceFromGoal = 9999;
		UtilAngle[] xTabAnglesToBall = new UtilAngle[12];	// Angles to ball
		UtilPoint	pGoal = new UtilPoint();
		UtilPoint	pGoalOpponent = new UtilPoint();
		UtilPoint 	pGoalOpponentTweak = new UtilPoint();
		UtilPoint 	pPlayer = new UtilPoint();
		UtilPoint 	pImmediateTarget = new UtilPoint();
		UtilPoint	pPlayerGuard = new UtilPoint();


		//--- Do nothing if not properly initialized
		if (!bActive)
			return;

		if (xGameState == null)
			return;

		//--- Debug purpose
		//xGameState.displayInfo();

		//--------------------------------------------------------------------------
		//	GENERAL STATE
		//--------------------------------------------------------------------------

		Random randomGenerator = new Random();

		pGoalOpponent.set(xGameState.iOpponentGoalCenterX, xGameState.iOpponentGoalCenterY);
		pGoal.set(xGameState.iGoalCenterX, xGameState.iGoalCenterY);

		estimateBallFuturePosition();

		dHalfZone = (double)((xGameState.iPitchHeight + (xGameState.iPitchWidth/2))/2);
		dDistanceFromGoal = getDistance(pGoal, pBall);

		//--------------------------------------------------------------------------
		//	COMPUTE PARAMETERS FOR ALL PLAYERS
		//--------------------------------------------------------------------------
		for (iNoPlayer = 0; iNoPlayer < 12; iNoPlayer++)
		{
			Player xPlayer = null;
			double dDistanceFromBall = 99999;
			double dDistance = 9999;
			xPlayer = xGameState.xPlayersTable[iNoPlayer];

			if (xPlayer != null)
			{
				pPlayer.set(xPlayer.fPosX, xPlayer.fPosY);
				pPlayerGuard.set(xPlayer.fGuardPosX, xPlayer.fGuardPosY);

				//--- Angle towards Opponent Goal
				xTabAnglesToOpponentGoal[iNoPlayer] = new UtilAngle(pGoalOpponent, pPlayer);

				dDistance = getDistance(pPlayer, pGoalOpponent);
				dTabDistanceFromOpponentGoal[iNoPlayer] = dDistance;

				//--- Distance from estimated ball future position
				dDistanceFromBall = getDistance(pPlayer, pBallFuture);
				dTabDistanceFromFutureBall[iNoPlayer] = dDistanceFromBall;	

				//--- Distance from current ball position
				dDistanceFromBall = getDistance(pPlayer, pBall);
				dTabDistanceFromBall[iNoPlayer] = dDistanceFromBall;

				xTabAnglesToBall[iNoPlayer] = new UtilAngle(pBall, pPlayer);

				dTabDistanceFromGuard[iNoPlayer] = getDistance(pPlayerGuard, pBall);

				//--- Determine ball possession
				if (xPlayer.bAIControlled == true && xPlayer.bHasBall == true)
				{
					bAttackerHasBall = false;
					bCentralHasBall = false;
					bDefenderHasBall = false;
					bGoalHasBall = false;					
					if (xPlayer.sPlayerTempRole.equals("A"))
						bAttackerHasBall = true;
					if (xPlayer.sPlayerTempRole.equals("C"))
						bCentralHasBall = true;
					if (xPlayer.sPlayerTempRole.equals("D"))
						bDefenderHasBall = true;
					if (xPlayer.sPlayerRole.equals("G"))
						bGoalHasBall = true;
				}
				else if (xPlayer.bAIControlled == false && xPlayer.bHasBall == true)
				{
					bAttackerHasBall = false;
					bCentralHasBall = false;
					bDefenderHasBall = false;
					bGoalHasBall = false;
				}

			}
		}

		//--------------------------------------------------------------------------
		//	GIVE ORDERS TO CONTROLLED PLAYERS
		//--------------------------------------------------------------------------
		for (iNoPlayer = xGameState.iFirstPlayer; iNoPlayer <= xGameState.iLastPlayer; iNoPlayer++)
		{
			Player xPlayer = null;
			xPlayer = xGameState.xPlayersTable[iNoPlayer];

			//boolean 

			if (xPlayer == null)
				continue;

			//--------------------------------------------------------------------------
			//	BEGIN CYCLE
			//--------------------------------------------------------------------------


			if (xPlayer.iKickCooldown > 0)
				xPlayer.iKickCooldown--;


			//--------------------------------------------------------------------------
			//	SPECIAL GOAL WORKFLOW
			//--------------------------------------------------------------------------

			if (xPlayer.sPlayerRole.equals("G"))
			{
				double dDefaultDistance = 4;
				/// When out of goal area, get a good placement
				if (dDistanceFromGoal > ( xGameState.iPitchGoalAreaRadius + 10 ) ) 
				{
					//int iGoalPlayer = iNoPlayer;
					double dAngleRad = xTabAnglesToBall[iNoPlayer].getAngle();
					//if (dTabDistanceFromBall[iNoPlayer] < xGameState.iPitchGoalAreaRadius + 10)
					//	dDefaultDistance = 3;
					xPlayer.fDestX = xGameState.iGoalCenterX + Math.cos(dAngleRad)*dDefaultDistance;
					xPlayer.fDestY = xGameState.iGoalCenterY + Math.sin(dAngleRad)*dDefaultDistance;
					xPlayer.sAIOrder = "MOVE";		
					xPlayer.sAction = "MOVE";
//					xPlayer.bActionTurn = true;
					//System.err.println("Goal dest = " + px + "/" + py + "|| angle = " + dAngleDegree + " dir= " + iAngleGame);
				}
				else if (dDistanceFromGoal <= xGameState.iPitchGoalAreaRadius)
				{
					//System.err.println(xPlayer);
					if (xPlayer.bHasBall && xPlayer.iKickCooldown <= 0)
					{
						int iRandomY = 0;
						iRandomY = randomGenerator.nextInt(xGameState.iPitchHeight);						
						xPlayer.fKickDestX = pGoalOpponent.dValueX;
						xPlayer.fKickDestY = iRandomY;
						xPlayer.fKickSpeed = 100;
						xPlayer.bActionKick = true;
						xPlayer.sAIOrder = "NONE";
						xPlayer.iKickCooldown = 4;						
					}
					else if (dTabDistanceFromBall[iNoPlayer] < 5)
					{
						xPlayer.fDestX = pBall.dValueX;
						xPlayer.fDestY = pBall.dValueY;
						xPlayer.sAIOrder = "CATCH_BALL";		
						xPlayer.bActionTake = true;
						xPlayer.bActionKick = true;			
					}
					else
					{
						xPlayer.fDestX = pBallFuture.dValueX;
						xPlayer.fDestY = pBallFuture.dValueY;
						xPlayer.sAIOrder = "MOVE";		
					}					
				}
			}


			//--------------------------------------------------------------------------
			//	SPECIAL CENTRAL WORKFLOW
			//--------------------------------------------------------------------------

			if (xPlayer.sPlayerTempRole.equals("C") && bAttackedOff == false)
			{
				xPlayer.bActionTurn = false;
				xPlayer.fDestX = pBall.dValueX;
				xPlayer.fDestY = pBall.dValueY;
				xPlayer.fSpeed = 100;

				xPlayer.iWantedDirection = xTabAnglesToOpponentGoal[iNoPlayer].getAngleIngame();
				//System.err.println("Direction>> Current:" + xPlayer.iDirection + " Wanted: " + xPlayer.iWantedDirection);
				//System.err.println(xPlayer);

				if (bAttackerHasBall)
				{
					{
						xPlayer.fGuardPosX = xGameState.iPitchCentreX + xGameState.iPitchCentreRadius*fSideFactor;
						xPlayer.fGuardPosY = xGameState.iPitchCentreY;								
					}
					xPlayer.fDestX = xPlayer.fGuardPosX;
					xPlayer.fDestY = xPlayer.fGuardPosY;					
				}

				//--- Main final destination is the opponent goal!
				pImmediateTarget.set(pGoalOpponent.dValueX, pGoalOpponent.dValueY);
				pGoalOpponentTweak.set(pGoalOpponent.dValueX, pGoalOpponent.dValueY);

				//------ Randomize a little the goal Y target
				{
					int iGoalWidth = 10;
					int iRandomY = 0;
					
					iGoalWidth = (xGameState.iPitchGoalY2 - xGameState.iPitchGoalY1) - 2;

					iRandomY = randomGenerator.nextInt(iGoalWidth);
					//System.err.println("Random: " + iRandomY + " >> " + xPlayer.fKickDestY);

					pGoalOpponentTweak.set(pGoalOpponent.dValueX, (double) (xGameState.iPitchGoalY1 + 1 + iRandomY));
				}

				////--- For a null order or too far from ball, get moving!
				if (dTabDistanceFromBall[iNoPlayer] > 5)
					xPlayer.sAIOrder = "MOVE_TO_BALL";

				if (dTabDistanceFromGuard[iNoPlayer] > (dHalfZone) )
					xPlayer.sAIOrder = "MOVE_TO_GUARD";

				if (xPlayer.sAIOrder.equals("TAKE_AND_KICK"))
				{
					if (xPlayer.bHasBall)
					{
						xPlayer.sAIOrder.equals("KICK");
						xPlayer.bActionKick = true;

						xPlayer.fKickSpeed = 35 + (xPlayer.fSpeed/100.f)*10;

						//if (bZigZagAttack == true)
						/*{
							int iRandomY = 0;
							int iAmplitude = xGameState.iPitchHeight/20;
							iRandomY = randomGenerator.nextInt(iAmplitude)-iAmplitude/2;
							pImmediateTarget.set(pImmediateTarget.dValueX, pImmediateTarget.dValueY+iRandomY);
						}*/

						// If close enough, try scoring
						if (dTabDistanceFromOpponentGoal[iNoPlayer] < (xGameState.iPitchGoalAreaRadius + 10) )
						{
							xPlayer.fKickSpeed = 50;
							// If the player has a very good accuracy, better target some point between Y1 and Y2
							if (xPlayer.fKickingAbility > 90)
							{
								pImmediateTarget = pGoalOpponentTweak;
								//System.err.println("Target: " + pImmediateTarget);
							}
							xPlayer.iKickCooldown = 5;							
						}

						xPlayer.fKickDestX = pImmediateTarget.dValueX;
						xPlayer.fKickDestY = pImmediateTarget.dValueY;						
						
						xPlayer.sAIOrder = "NONE";
					}
					else
						xPlayer.bActionTake = true;	
				}

				//--- If enough close to ball, turn towards opponent goal, then kick
				if (xPlayer.sAIOrder.equals("MOVE_TO_BALL"))
				{
					if (dTabDistanceFromBall[iNoPlayer] < 0.5)
					{
						int iAngleAbsDev = 0;
						xPlayer.bActionTurn = true;
						xPlayer.iWantedDirection = xTabAnglesToOpponentGoal[iNoPlayer].getAngleIngame();

						iAngleAbsDev = getAbsAngleDeviation(xPlayer.iDirection, xPlayer.iWantedDirection);

						if (iAngleAbsDev == 0)	// Under x degree, engage action
						{
							xPlayer.sAIOrder = "TAKE_AND_KICK";
							xPlayer.bActionTake = true;
							xPlayer.bActionKick = true;
							//--- Set target
							if (xPlayer.iKickCooldown <= 0)
							{


								xPlayer.fKickDestX = pImmediateTarget.dValueX;
								xPlayer.fKickDestY = pImmediateTarget.dValueY;

							}
							
						}
					}
				}

				if (xPlayer.sAIOrder.equals("MOVE_TO_GUARD"))
				{
					xPlayer.fDestX = xPlayer.fGuardPosX;
					xPlayer.fDestY = xPlayer.fGuardPosY;
					xPlayer.sAIOrder = "MOVE";
				}

				//------ If there is no order, give one
				if (xPlayer.sAIOrder.equals("NONE"))
				{
					xPlayer.sAIOrder = "MOVE_TO_BALL";
				}

			}

			//--------------------------------------------------------------------------
			//	SPECIAL ATTACKER WORKFLOW
			//--------------------------------------------------------------------------

			if (xPlayer.sPlayerTempRole.equals("A") && bAttackedOff == false)
			{
				xPlayer.bActionTurn = false;
				xPlayer.fDestX = pBallFuture.dValueX;
				xPlayer.fDestY = pBallFuture.dValueY;
				xPlayer.fSpeed = 100;

				xPlayer.iWantedDirection = xTabAnglesToOpponentGoal[iNoPlayer].getAngleIngame();
				//System.err.println("Direction>> Current:" + xPlayer.iDirection + " Wanted: " + xPlayer.iWantedDirection);
				//System.err.println(xPlayer);

				//--- Main final destination is the opponent goal!
				pImmediateTarget.set(pGoalOpponent.dValueX, pGoalOpponent.dValueY);
				pGoalOpponentTweak.set(pGoalOpponent.dValueX, pGoalOpponent.dValueY);

				if (bGoalHasBall == true || bDefenderHasBall == true)
				{
					xPlayer.fDestX = pBall.dValueX;
					xPlayer.fDestY = pBall.dValueY;					
				}

				//------ Randomize a little the goal Y target
				{
					int iGoalWidth = 10;
					int iRandomY = 0;
					
					iGoalWidth = (xGameState.iPitchGoalY2 - xGameState.iPitchGoalY1) - 2;

					iRandomY = randomGenerator.nextInt(iGoalWidth);
					//System.err.println("Random: " + iRandomY + " >> " + xPlayer.fKickDestY);

					pGoalOpponentTweak.set(pGoalOpponent.dValueX, (double) (xGameState.iPitchGoalY1 + 1 + iRandomY));
				}

				////--- For a null order or too far from ball, get moving!
				if (dTabDistanceFromBall[iNoPlayer] > 5)
					xPlayer.sAIOrder = "MOVE_TO_BALL";

				if (dTabDistanceFromGuard[iNoPlayer] > dAttackerZone )
					xPlayer.sAIOrder = "MOVE_TO_GUARD";

				if (xPlayer.sAIOrder.equals("TAKE_AND_KICK"))
				{
					if (xPlayer.bHasBall)
					{
						xPlayer.sAIOrder.equals("KICK");
						xPlayer.bActionKick = true;

						xPlayer.fKickSpeed = 35 + (xPlayer.fSpeed/100.f)*10;

						if (bZigZagAttack == true)
						{
							int iRandomY = 0;
							int iAmplitude = xGameState.iPitchHeight/20;
							iRandomY = randomGenerator.nextInt(iAmplitude)-iAmplitude/2;
							pImmediateTarget.set(pImmediateTarget.dValueX, pImmediateTarget.dValueY+iRandomY);
						}

						// If close enough, try scoring
						if (dTabDistanceFromOpponentGoal[iNoPlayer] < (xGameState.iPitchGoalAreaRadius + 10) )
						{
							xPlayer.fKickSpeed = 100;
							// If the player has a very good accuracy, better target some point between Y1 and Y2
							if (xPlayer.fKickingAbility > 90)
							{
								pImmediateTarget = pGoalOpponentTweak;
								//System.err.println("Target: " + pImmediateTarget);
							}
							xPlayer.iKickCooldown = 5;							
						}

						xPlayer.fKickDestX = pImmediateTarget.dValueX;
						xPlayer.fKickDestY = pImmediateTarget.dValueY;						
						
						xPlayer.sAIOrder = "NONE";
					}
					else
						xPlayer.bActionTake = true;	
				}

				//--- If enough close to ball, turn towards opponent goal, then kick
				if (xPlayer.sAIOrder.equals("MOVE_TO_BALL"))
				{
					if (dTabDistanceFromBall[iNoPlayer] < 0.5)
					{
						int iAngleAbsDev = 0;
						xPlayer.bActionTurn = true;
						xPlayer.iWantedDirection = xTabAnglesToOpponentGoal[iNoPlayer].getAngleIngame();

						iAngleAbsDev = getAbsAngleDeviation(xPlayer.iDirection, xPlayer.iWantedDirection);

						if (iAngleAbsDev == 0)	// Under x degree, engage action
						{
							xPlayer.sAIOrder = "TAKE_AND_KICK";
							xPlayer.bActionTake = true;
							xPlayer.bActionKick = true;
							//--- Set target
							if (xPlayer.iKickCooldown <= 0)
							{


								xPlayer.fKickDestX = pImmediateTarget.dValueX;
								xPlayer.fKickDestY = pImmediateTarget.dValueY;

							}
							
						}
					}
				}

				if (xPlayer.sAIOrder.equals("MOVE_TO_GUARD"))
				{
					xPlayer.fDestX = xPlayer.fGuardPosX;
					xPlayer.fDestY = xPlayer.fGuardPosY;
					xPlayer.sAIOrder = "MOVE";
				}

				//------ If there is no order, give one
				if (xPlayer.sAIOrder.equals("NONE"))
				{
					xPlayer.sAIOrder = "MOVE_TO_BALL";
				}

			}


			//--------------------------------------------------------------------------
			//	SPECIAL DEFENDER WORKFLOW
			//--------------------------------------------------------------------------

			if (xPlayer.sPlayerTempRole.equals("D"))
			{
				xPlayer.bActionTurn = false;
				xPlayer.fDestX = pBall.dValueX;
				xPlayer.fDestY = pBall.dValueY;
				xPlayer.fSpeed = 100;

				xPlayer.iWantedDirection = xTabAnglesToOpponentGoal[iNoPlayer].getAngleIngame();
				//System.err.println("Direction>> Current:" + xPlayer.iDirection + " Wanted: " + xPlayer.iWantedDirection);
				//System.err.println(xPlayer);

				//--- Main final destination is the opponent goal!
				pImmediateTarget.set(pGoalOpponent.dValueX, pGoalOpponent.dValueY);

				////--- For a null order or too far from ball, get moving!
				if (dTabDistanceFromBall[iNoPlayer] > 5)
					xPlayer.sAIOrder = "MOVE_TO_BALL";

				if (dTabDistanceFromGuard[iNoPlayer] > (dHalfZone/2) )
					xPlayer.sAIOrder = "MOVE_TO_GUARD";

				if (xPlayer.sAIOrder.equals("TAKE_AND_KICK"))
				{
					if (xPlayer.bHasBall)
					{
						xPlayer.sAIOrder.equals("KICK");
						xPlayer.bActionKick = true;

						xPlayer.fKickSpeed = 35 + (xPlayer.fSpeed/100.f)*10;

						// Kick far!
						//if (dTabDistanceFromOpponentGoal[iNoPlayer] < 25)
						{
							xPlayer.fKickSpeed = 100;
							xPlayer.iKickCooldown = 5;							
						}

						xPlayer.fKickDestX = pImmediateTarget.dValueX;
						xPlayer.fKickDestY = pImmediateTarget.dValueY;						
						
						xPlayer.sAIOrder = "NONE";
					}
					else
						xPlayer.bActionTake = true;	
				}

				
				if (xPlayer.sAIOrder.equals("MOVE_TO_BALL"))
				{
					if (dDistanceFromGoal > xGameState.iPitchGoalAreaRadius)
					{
						if (dTabDistanceFromBall[iNoPlayer] < 5)
						{
							int iAngleAbsDev = 0;
							xPlayer.bActionTurn = true;
							xPlayer.iWantedDirection = xTabAnglesToOpponentGoal[iNoPlayer].getAngleIngame();
							iAngleAbsDev = getAbsAngleDeviation(xPlayer.iDirection, xPlayer.iWantedDirection);

							if (iAngleAbsDev <= 5)	// Under x degree, engage action
							{
								xPlayer.sAIOrder = "TAKE_AND_KICK";
								xPlayer.bActionTake = true;
								xPlayer.bActionKick = true;
								//--- Set target
								if (xPlayer.iKickCooldown <= 0)
								{
									xPlayer.fKickDestX = pImmediateTarget.dValueX;
									xPlayer.fKickDestY = pImmediateTarget.dValueY;	
								}
								
							}
						}
					}
					else
					{
						xPlayer.sAIOrder = "MOVE_TO_GUARD";
					}
				}

				if (xPlayer.sAIOrder.equals("MOVE_TO_GUARD"))
				{
					xPlayer.fDestX = xPlayer.fGuardPosX;
					xPlayer.fDestY = xPlayer.fGuardPosY;
					xPlayer.sAIOrder = "MOVE";
				}

				//------ If there is no order, give one
				if (xPlayer.sAIOrder.equals("NONE"))
				{
					xPlayer.sAIOrder = "MOVE_TO_BALL";
				}

			}

			//--------------------------------------------------------------------------
			//	END
			//--------------------------------------------------------------------------

			//--- Set back player to main state table
			if (xPlayer != null)
				xGameState.xPlayersTable[iNoPlayer] = xPlayer;
		}


	}
	//	

	public void setupKickoffPositions(int iTeamKickingOff)
	{
		int iNoPlayer = 0;
		double fGuardPosX = 0;
		double fGuardPosY = 0;

		bAttackerHasBall = false;
		bCentralHasBall = false;
		bDefenderHasBall = false;
		bGoalHasBall = false;

		//--- If team is at right side and must go left
		if (xGameState.sTeamDirection.equals("LEFT"))
			fSideFactor = -1;
		else	// Team is at left side and must go right
			fSideFactor = 1;

		iScoreDiff = xGameState.iScore - xGameState.iOpponentScore;

		dAttackerZone = (dHalfZone/2+15);

		if (iScoreDiff < -3)
			dAttackerZone = (dHalfZone+10);

		if (iScoreDiff < -6)
			dAttackerZone = xGameState.iPitchWidth;

		//--- First, set normal positions
		for (iNoPlayer = xGameState.iFirstPlayer; iNoPlayer <= xGameState.iLastPlayer; iNoPlayer++)
		{
			Player xPlayer = null;
			xPlayer = xGameState.xPlayersTable[iNoPlayer];

			//--- Attackers
			if (iNoPlayer%6 == 0)
			{
				fGuardPosX = xGameState.iPitchCentreX + xGameState.iPitchCentreX*.4*fSideFactor;
				fGuardPosY = xGameState.iPitchCentreY - xGameState.iPitchCentreY*.6;									
			}
			if (iNoPlayer%6 == 1)
			{
				fGuardPosX = xGameState.iPitchCentreX + xGameState.iPitchCentreX*.4*fSideFactor;
				fGuardPosY = xGameState.iPitchCentreY + xGameState.iPitchCentreY*.6;								
			}
			//--- Central
			if (iNoPlayer%6 == 2)
			{
				fGuardPosX = xGameState.iPitchCentreX - xGameState.iPitchCentreRadius*fSideFactor;
				fGuardPosY = xGameState.iPitchCentreY;								
			}
			//--- Defenders
			if (iNoPlayer%6 == 3)
			{
				fGuardPosX = xGameState.iPitchCentreX - xGameState.iPitchCentreX*.5*fSideFactor;
				fGuardPosY = xGameState.iPitchCentreY - xGameState.iPitchCentreY*.5;								
			}
			if (iNoPlayer%6 == 4)
			{
				fGuardPosX = xGameState.iPitchCentreX - xGameState.iPitchCentreX*.5*fSideFactor;
				fGuardPosY = xGameState.iPitchCentreY + xGameState.iPitchCentreY*.5;								
			}
			//--- Goal
			if (iNoPlayer%6 == 5 || xPlayer.sPlayerType.equals("G") )	// Would be G
			{
				fGuardPosX = xGameState.iPitchCentreX - xGameState.iPitchCentreX*fSideFactor;
				fGuardPosY = xGameState.iPitchGoalCentre;			
			}

			xPlayer.fGuardPosX = fGuardPosX;
			xPlayer.fGuardPosY = fGuardPosY;	

			xPlayer.fPosX = xPlayer.fGuardPosX;
			xPlayer.fPosY = xPlayer.fGuardPosY;	

			xPlayer.fDestX = xPlayer.fPosX;
			xPlayer.fDestY = xPlayer.fPosY;				

			xPlayer.sAIOrder = "NONE";
			xPlayer.sAIMoveMode = "PERSONAL_SMALL";

			//--- Set back player to main state table
			if (xPlayer != null)
				xGameState.xPlayersTable[iNoPlayer] = xPlayer;						
		}

		//--- If kicking off, move 2 players


	

	}

	public void setup()
	{

		int iNoPlayer = 0;
		//--- Init AI player table
		for (iNoPlayer = 0; iNoPlayer < 6; iNoPlayer++)
		{
			double fPosX = 0;
			double fPosY = 0;			
			xAIPlayersTable[iNoPlayer] = new Player();
			xAIPlayersTable[iNoPlayer].bAIControlled = true;

			// 0	P ATK  R = 100 / K = 100 / BC = 100
			// 1 	P ATK
			// 2 	P CEN  K = 100 / BC = 100
			// 3	P DEF  R = 100
			// 4	P DEF  T = 100
			// 5	G GOL  K = 100 / BC = 100

			xAIPlayersTable[iNoPlayer].fKickingAbility = 50;
			xAIPlayersTable[iNoPlayer].fRunningAbility = 50;
			xAIPlayersTable[iNoPlayer].fBallControlAbility = 50;
			xAIPlayersTable[iNoPlayer].fTacklingAbility = 50;
			xAIPlayersTable[iNoPlayer].sPlayerType = "P";

			if (iNoPlayer%6 == 0)	// atk 1
			{
				xAIPlayersTable[iNoPlayer].fKickingAbility = 100;
				xAIPlayersTable[iNoPlayer].fRunningAbility = 100;
				xAIPlayersTable[iNoPlayer].fBallControlAbility = 100;
				xAIPlayersTable[iNoPlayer].fTacklingAbility = 50;	
				xAIPlayersTable[iNoPlayer].sPlayerRole = "A";

			}
			if (iNoPlayer%6 == 1)	// atk 2
			{
				xAIPlayersTable[iNoPlayer].fKickingAbility = 75;
				xAIPlayersTable[iNoPlayer].fRunningAbility = 66;
				xAIPlayersTable[iNoPlayer].fBallControlAbility = 75;
				xAIPlayersTable[iNoPlayer].fTacklingAbility = 50;		
				xAIPlayersTable[iNoPlayer].sPlayerRole = "A";

			}
			if (iNoPlayer%6 == 2)	// central
			{
				xAIPlayersTable[iNoPlayer].fKickingAbility = 50;
				xAIPlayersTable[iNoPlayer].fRunningAbility = 75;
				xAIPlayersTable[iNoPlayer].fBallControlAbility = 50;
				xAIPlayersTable[iNoPlayer].fTacklingAbility = 50;	
				xAIPlayersTable[iNoPlayer].sPlayerRole = "C";

			}
			if (iNoPlayer%6 == 3)	// def 1
			{
				xAIPlayersTable[iNoPlayer].fKickingAbility = 75;
				xAIPlayersTable[iNoPlayer].fRunningAbility = 50;
				xAIPlayersTable[iNoPlayer].fBallControlAbility = 75;
				xAIPlayersTable[iNoPlayer].fTacklingAbility = 100;	
				xAIPlayersTable[iNoPlayer].sPlayerRole = "D";

			}
			if (iNoPlayer%6 == 4)	// def 2
			{
				xAIPlayersTable[iNoPlayer].fKickingAbility = 50;
				xAIPlayersTable[iNoPlayer].fRunningAbility = 66;
				xAIPlayersTable[iNoPlayer].fBallControlAbility = 50;
				xAIPlayersTable[iNoPlayer].fTacklingAbility = 100;	
				xAIPlayersTable[iNoPlayer].sPlayerRole = "D";

			}
			if (iNoPlayer%6 == 5)	// goal
			{
				xAIPlayersTable[iNoPlayer].fKickingAbility = 100;
				xAIPlayersTable[iNoPlayer].fRunningAbility = 50;
				xAIPlayersTable[iNoPlayer].fBallControlAbility = 100;
				xAIPlayersTable[iNoPlayer].fTacklingAbility = 50;	

				xAIPlayersTable[iNoPlayer].sPlayerRole = "G";
				xAIPlayersTable[iNoPlayer].sPlayerType = "G";

			}

			xAIPlayersTable[iNoPlayer].sPlayerTempRole = xAIPlayersTable[iNoPlayer].sPlayerRole;

		}
	}

	private class IndexedDistance implements Comparable<IndexedDistance>
	{
		double dValue;
		int iIndex;

		IndexedDistance()
		{
			dValue = 0;
			iIndex = 0;
		}

		void set(int iIndex, double dValue)
		{
			this.iIndex = iIndex;
			this.dValue = dValue;
		}

		public int compareTo(IndexedDistance that)
		{
			if (that == null)
				return -1;

			return (int) (this.dValue - that.dValue);
		}

		public int compare()
		{
			return 0;
		}

		public boolean equals(Object o)
		{
			if (this == o)
				return true;
			if ( ! (o instanceof IndexedDistance))
				return false;
			IndexedDistance that = (IndexedDistance) o;
			if  (that.dValue == this.dValue)
				return true;
			return false;
		}
	}
};