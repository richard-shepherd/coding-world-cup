#include "stdafx.h"
#include "Player.h"


CPlayer::CPlayer() : game_(GetGame()),
					 pitch_(GetGame().GetPitch()),
					 ball_(GetGame().GetBall())
{
	SetMarkedPlayerNumber(eNotMarking);
	isShootCached_ = false;

	ResetGoalKeeperWaitTicks();
	ResetKickingTowardPlayerNumber();
}


CPlayer::~CPlayer()
{

}

int CPlayer::ProcessStaticState(const Value& staticState)
{
	capability_.kickingAbility_ = staticState["kickingAbility"].GetDouble();
	capability_.runningAbility_ = staticState["runningAbility"].GetDouble();
	capability_.ballControlAbility_ = staticState["ballControlAbility"].GetDouble();
	capability_.tacklingAbility_ = staticState["tacklingAbility"].GetDouble();

	return 0;
}
int CPlayer::ProcessDynamicState(const Value& dynamicState)
{
	pos_.x_ = dynamicState["position"]["x"].GetDouble();
	pos_.y_ = dynamicState["position"]["y"].GetDouble();

	hasBall_ = dynamicState["hasBall"].GetBool();
	direction_ = dynamicState["direction"].GetDouble();

	//Flip co-ordinates
	const CTeam::Ptr& ourTeamPtr = GetGame().GetOurTeamPtr();

	if (ourTeamPtr->GetPlayingDirection() == CTeam::eLeft)
	{
		pos_.x_ = pitch_.GetWidth() - pos_.x_;
				
		direction_ = 360.0 - direction_;
	}
	
	//fixing the hange issue due to direction calculated to 0 and player direction as 360
	if (direction_ == 360.0)
	{
		direction_ = 0.0;
	}
	
	return 0;
}

bool CPlayer::IsOurTeamMember()
{
	if (game_.GetOurTeamPtr()->GetTeamNumber() == teamNumber_)
		return true;
		
	return false;	
}
bool CPlayer::IsTheirTeamMember()
{
	if (game_.GetTheirTeamPtr()->GetTeamNumber() == teamNumber_)
		return true;
		
	return false;
}


void CPlayer::MoveToSaveGoal_GoalKeeper(const Position& hittingAt)
{
	// try catching ball.
	Position surePos, almostPos;
	bool isSure = false, isAlmost = false;

	bool isFirstPosClosestToGoal = true;
	Position firstPosClosestToGoal;

	auto& pathVirtualPos = ball_.GetPathVirutalPos(); 
	auto& pathPosTime = ball_.GetPathPosTime();

	bool found = false;
	for (int i = pathVirtualPos.size() - 1; i >= 0; --i)
	{
		//ignore co-ordinate crossing our goal (i.e x < 0)
		if (pathVirtualPos[i].x_ < pitch_.GetOurGoalCentre().x_)
		{
			continue;
		}

		//saving the first pos - incase of player not able to reach any coordinate
		//						 goalkeeper will attempt to go to this first pos.
		if (isFirstPosClosestToGoal)
		{
			firstPosClosestToGoal = pathVirtualPos[i];
			isFirstPosClosestToGoal = false;
		}

		float t1 = CalculateTimeToReachPosition(pathVirtualPos[i]);

		if (t1 < pathPosTime[i])
		{
			surePos = pathVirtualPos[i];
			isSure = true;
		}
		else if (ApproxEqual(t1, pathPosTime[i], 0.055f))
		{
			almostPos = pathVirtualPos[i];
			isAlmost = true;
		}
	}

	if (isSure)
	{
		MoveTo(surePos);
	}
	else if (isAlmost)
	{
		MoveTo(almostPos);
	}
	else
	{
		MoveTo(firstPosClosestToGoal);
	}
}
void CPlayer::MoveTo(const Position& dest)
{
	if (dest.ApproxEqual(pos_,POSITION_TOLERANCE))
	{
		action_.type_ = CAction::eNoAction;
		return;
	}
	action_.type_ = CAction::eMove;
	action_.destination_ = dest;
	action_.speed_ = 100.0;
}
void CPlayer::TurnTo(float direction)
{
	if (ApproxEqual(direction_, direction, DIRECTION_TOLERANCE))
	{
		action_.type_ = CAction::eNoAction;
		return;
	}

	action_.type_ = CAction::eTurn;
	action_.direction_ = direction;
}
void CPlayer::TakePossession()
{
	action_.type_ = CAction::eTakePossession;
}

void CPlayer::Kick(const Position& destination, float speed)
{
	action_.type_ = CAction::eKick;
	action_.destination_ = destination;
	action_.speed_ = speed;
}

void CPlayer::KickShort(float power)
{
	float direction = GetPosition().AngleWith(pitch_.GetTheirGoalCentre());
	
	float distance = GetPosition().DistanceFrom(pitch_.GetTheirGoalCentre()) - 15.5;
	
	if (IsTheirPlayerBehindMe())
	{
		Vector shootVec = GetVectorFromDirection(direction);
		shootVec = shootVec.Scale(distance);
		
		Position shootPos = GetPosition();
		shootPos.AddVector(shootVec);
			
		Kick(shootPos, ball_.GetSpeedForDistance(distance));
	}
	else
	{
		int randVal = RandomRangeInteger(0,2);
			
		direction = direction + (RandomRangeInteger(0,2) - 1)*20.0;

			//Vector shootVec = GetVectorFromDirection(arr[RandomRangeInteger(2, 4)]);
		Vector shootVec = GetVectorFromDirection(direction);
		shootVec = shootVec.Scale(5.0);
		
			
		Position shootPos = GetPosition();
		shootPos.AddVector(shootVec);
			
		Kick(shootPos, power);
	}
}
void CPlayer::KickShort_Striker()
{
	float distanceFromGoal = ball_.GetPosition().DistanceFrom(pitch_.GetTheirGoalCentre());
	if (distanceFromGoal > 20.0)
	{
		if (distanceFromGoal < 22.0)
		{
			KickShort(35.0f);
		}
		else
		{
			KickShort(40.0f);
		}
		
		ChangeState(CPlayerState::eCounterAttackerStrikerShortKick);
	}
	else
	{
		/*if (distanceFromGoal > 18.5)
		{
			KickShort(35.0f);
			return;
		}*/
		Position shootAt;
		if (!GetShootCache(shootAt))
		{
			//calucate random shoot direction;
			shootAt = GetRandomShootAtGoal();
			SetShootCache(shootAt);
		}
						
					
		float angle = GetPosition().AngleWith(shootAt);
		bool isNoOneClose = IsTheirPlayerNear(STRIKER_NO_ONE_CLOSE);
		
		if (!isNoOneClose && 
			!ApproxEqual(GetDirection(),angle,DIRECTION_TOLERANCE))
		{
			
			TurnTo(angle);
		}
		else
		{
			//no one close and player is little far.
			if (!isNoOneClose && (distanceFromGoal - 15.1f) > 2.5f)
			{
				//MoveTo(shootAt);
				Kick(shootAt,ball_.GetSpeedForDistance(distanceFromGoal - 15.1f) + 5.0);
				ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
			}
			else
			{
				//someone is close or we are close enough - shoot!!
				game_.noOfGoalAttemptsByUs++;
			
				//try to hit dead centre in case of direction still not aligned.
				if (fabsf(GetDirection() - angle) > 90.0)
				{
					shootAt = pitch_.GetTheirGoalCentre();
					//LOGGER->Log("Inaccurate shoot game_time:%f", GetGame().currentTimeSeconds_);
				}
					
				Kick(shootAt, 100.0);
				ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
			
				ResetShootCache();
			}
		}
	}
}

void CPlayer::Kick_Defender()
{
		auto& pCentrePlayer = game_.GetOurTeamPtr()->GetPlayerFromPlayerType(CPlayer::eCentreDefender);
		
		Position goHomePos = pCentrePlayer->GetHomePosition();//GetRandomFreePosition_Striker();
		
		float angle = this->GetPosition().AngleWith(goHomePos);
		float distanceFromHomePos = this->GetPosition().DistanceFrom(goHomePos);
		
		float distance = this->GetPosition().DistanceFrom(pCentrePlayer->GetPosition());
		float distanceFromGoal = this->GetPosition().DistanceFrom(pitch_.GetTheirGoalCentre());
				
		
		//no one near you try to short kick.
			if (!this->IsTheirPlayerNearFromFront(DEFENDER_SHORT_KICK_NO_ONE_CLOSE) && 
				distanceFromGoal > DEFENDER_SHOOTING_RANGE	)
			{
				this->KickShort(40.0);
			}	
			else
			{
				//you are a defender but you are very close attempt goal.
				if (distanceFromGoal <= DEFENDER_SHOOTING_RANGE)
				{
					//attempt on goal.
					Position shootAt = GetRandomShootAtGoal();
					
					this->Kick(shootAt, 100.0);
					
					game_.noOfGoalAttemptsByUs++;
				}
				else
				{
					
					if (!this->IsTheirPlayerNear(DEFENDER_NO_ONE_CLOSE) && 
						!ApproxEqual(this->GetDirection(),angle,DIRECTION_TOLERANCE))
					{		
						this->TurnTo(angle);
					}
					else
					{
						
						//pCentrePlayer->SetHomePosition(goHomePos);
						pCentrePlayer->MoveTo(pCentrePlayer->GetHomePosition());
						pCentrePlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
		
						//distance
						/*float speed = 100.0;
						if (distance < 20.0) 
						speed = 70.0;*/
						
						distanceFromHomePos += 3.0;
							
						//Position shootAt = pCentrePlayer->GetAction().destination_;
						//shootAt.y_ -= 20.0; 
						this->Kick(goHomePos, ball_.GetSpeedForDistance(distanceFromHomePos));
					}
				}
				
			//pPlayer->MoveTo({ 8.0f, 25.0 });
			this->ChangeState(CPlayerState::eCounterAttackerDefenderMark);
			}
}

void CPlayer::Kick_GoalKeeper()
{
	int supportingPlayerType = ( (GetGoalKeeperWaitTicks()/ 5)% 2 == 0? CPlayer::eLeftDefender : CPlayer::eRightDefender);
	auto& ourTeamPtr = game_.GetOurTeamPtr();
	auto& pPassPlayer = ourTeamPtr->GetPlayerFromPlayerType(supportingPlayerType);
			
	//int randVal = RandomRangeInteger(0,1);
			//int supportingPlayerType = (pPlayer->GetGoalKeeperWaitTicks() % 2 == 0? CPlayer::eRightDefender : CPlayer::eLeftDefender);
	float direction = GetPosition().AngleWith(pPassPlayer->GetPosition());
			
	IncrementGoalKeeperWaitTicks();
	
	float speed = 100.0;
	
	//GetGoalKeeperWaitTicks() > MAX_GOALKEEPER_WAIT_TICKS		
	if (ApproxEqual(direction,GetDirection(),DIRECTION_TOLERANCE))
	{
		if(IsKickDirectionSafe(direction, speed, 3.0f))
		{
			Kick(pPassPlayer->GetPosition(),speed);
			ResetGoalKeeperWaitTicks();
		}
		else
		{
			if (GetGoalKeeperWaitTicks() > MAX_GOALKEEPER_WAIT_TICKS)
			{
				
				Kick({RandomRangeFloat(25.0f, 50.0f), RandomRangeFloat(0.0f, 50.0f)},speed);
				ResetGoalKeeperWaitTicks();
			}
		}
	}
	else 
	{
		TurnTo(direction);
	}
	
	
	/*auto& ourTeamPtr = game_.GetOurTeamPtr();
	int randVal = RandomRangeInteger(0,1);
	int supportingPlayerType = (randVal == 0? CPlayer::eRightStriker : CPlayer::eLeftStriker);
	
	auto& pPassPlayer = ourTeamPtr->GetPlayerFromPlayerType(supportingPlayerType);
	
	float direction = GetPosition().AngleWith(pPassPlayer->GetHomePosition());
	
	float distance = GetPosition().DistanceFrom(pPassPlayer->GetPosition());
	
	if(ApproxEqual(direction,GetDirection(), 0.2f))
	{
		Kick(pPassPlayer->GetPosition(),ball_.GetSpeedForDistance(distance));
	}
	else 
	{
		TurnTo(direction);
	}*/
	
}

float CPlayer::PredictDirection()
{
	auto& theirTeamPtr = game_.GetTheirTeamPtr();
	
	if (theirTeamPtr->IsMember(ball_.GetOwner()))
	{
		auto& pBallPlayer = game_.GetPlayer(ball_.GetOwner());

		Vector vecDir = GetVectorFromDirection(pBallPlayer->GetDirection());
		
		Vector vecDir40 = vecDir.Scale(40.0);

		Position dist40pos = pBallPlayer->GetPosition();
		dist40pos.AddVector(vecDir40);

		Position hitPos;
		if (pitch_.IsLineHittingOurGoal(pBallPlayer->GetPosition(), dist40pos, hitPos))
		{
			if (hitPos.y_ > 25.0)
			{
				return 225.0;
			}
			else if (hitPos.y_ < 25.0)
			{
				return 315.0;
			}
			else
				return 270.0;
		}
		else
			return 270.0;
	}

	return 270.0;
}
void CPlayer::MoveToGuardGoal_Radius()
{
	//Radius guard
	Position ourGoalCentre = pitch_.GetOurGoalCentre();
	Vector goalToBallvector(ourGoalCentre, ball_.GetPosition());


	Vector scaledVec = goalToBallvector.Scale(GUARD_RADIUS);

	ourGoalCentre.AddVector(scaledVec);
	
	if (ourGoalCentre.x_ > GUARD_LINE)
		ourGoalCentre.x_ = GUARD_LINE;
		
	if (ourGoalCentre.y_ < 21.5)
		ourGoalCentre.y_ = 21.5;	

	if (ourGoalCentre.y_ > 27.5)
		ourGoalCentre.y_ = 27.5;	
	

	if (pos_.ApproxEqual(ourGoalCentre, POSITION_GUARD_TOLERANCE))
	{
		//already in postion. so turn to defend
		TurnTo(PredictDirection());
	}
	else{
		MoveTo(ourGoalCentre);
	}

}

void CPlayer::MoveToGuardGoal_Centre()
{
	Position ourGoalCentre = pitch_.GetOurGoalCentre();
	if (pos_.ApproxEqual(ourGoalCentre, POSITION_GUARD_TOLERANCE))
	{
		//already in postion. so turn to defend
		TurnTo(90.0);
	}
	else{
		MoveTo(ourGoalCentre);
	}
}

void CPlayer::MoveToGuardGoal()
{
	//MoveToGuardGoal_Centre();
	//MoveToGuardGoal_LineSave();
	MoveToGuardGoal_Radius();
}
void CPlayer::MoveToGuardGoal_LineSave()
{
	Position ourGoalCentre = pitch_.GetOurGoalCentre();

	ourGoalCentre.x_ = 1.0;

	if (pos_.ApproxEqual(ourGoalCentre, POSITION_GUARD_TOLERANCE))
	{
		//already in postion. so turn to defend
		TurnTo(270.0);
	}
	else{
		MoveTo(ourGoalCentre);
	}
}



void CPlayer::MoveToMarkedPlayer_GuardPass()
{
	SelectMarkedPlayer();
	
	auto& pMarkedPlayer = game_.GetPlayer(GetMarkedPlayerNumber());

	Position ballPos = ball_.GetPosition();
		
	Position markPos = pMarkedPlayer->GetPosition();
	
	float distBallToMarkPlayer = ballPos.DistanceFrom(markPos);
	
	//to solve nan problem - ball and mark pos almost same.
	if (ballPos.ApproxEqual(markPos, POSITION_BIG_TOLERANCE))
	{
		MoveTo(markPos);
		return	;
	}
	
	if (distBallToMarkPlayer < 40.0)
		distBallToMarkPlayer = distBallToMarkPlayer*PERCANTAGE_DIST_FOR_GUARD_PASS;
	else
		distBallToMarkPlayer = distBallToMarkPlayer - 30.0;
		
	Vector towardsBall = markPos.VectorTo(ballPos);
	Vector towardsBallScaled = towardsBall.Scale(distBallToMarkPlayer);

	markPos.AddVector(towardsBallScaled);
	
	MoveTo(markPos);

	
	/*if (pos_.ApproxEqual(markPos, POSITION_BIG_TOLERANCE))
	{
		TurnTo(270.0);
	}
	else
	{
		MoveTo(markPos);
	}*/
}
bool CPlayer::IsSupportingDefenderAlreadyMarking(int playerNumber)
{
	int supportingPlayerType = (GetType() == CPlayer::eLeftDefender ? CPlayer::eRightDefender : CPlayer::eLeftDefender);
	auto& ourTeamPtr = game_.GetOurTeamPtr();
	
	auto& pSupportingPlayer = ourTeamPtr->GetPlayerFromPlayerType(supportingPlayerType);
	
	if(pSupportingPlayer->GetMarkedPlayerNumber() == playerNumber)
		return true;
	return false;
}

void CPlayer::SelectMarkedPlayer()
{
	auto& theirTeamSortedX = game_.GetTheirTeamSortedX();
	
	if (GetMarkedPlayerNumber() == eNotMarking) 
	{
		//set marked player
		if (theirTeamSortedX[0]->GetPosition().y_ < theirTeamSortedX[1]->GetPosition().y_)
		{
			if (GetType() == CPlayer::eLeftDefender)
			{
				SetMarkedPlayerNumber(theirTeamSortedX[0]->GetNumber());
			}
			else
			{
				SetMarkedPlayerNumber(theirTeamSortedX[1]->GetNumber());
			}
		}
		else
		{
			if (GetType() == CPlayer::eLeftDefender)
			{
				SetMarkedPlayerNumber(theirTeamSortedX[1]->GetNumber());
			}
			else
			{
				SetMarkedPlayerNumber(theirTeamSortedX[0]->GetNumber());
			}
		}
	}
	else
	{
		// check if someone else has come close to our goal.
		if ( GetMarkedPlayerNumber() != theirTeamSortedX[0]->GetNumber() &&
		     GetMarkedPlayerNumber() != theirTeamSortedX[1]->GetNumber())
		{
			//Switch of marked player
			int supportingPlayerType = (GetType() == CPlayer::eLeftDefender ? CPlayer::eRightDefender : CPlayer::eLeftDefender);
			
			auto& ourTeamPtr = game_.GetOurTeamPtr();
			auto& pSupportingPlayer = ourTeamPtr->GetPlayerFromPlayerType(supportingPlayerType);
			
			int supportingMarkedPlayerNumber = pSupportingPlayer->GetMarkedPlayerNumber();
			// this player is marking 3rd closest player to our goal
			// and supporting player marking 1st clostest player to our goal
			
			
			if (GetMarkedPlayerNumber() == theirTeamSortedX[2]->GetNumber())
			{
				float distX = 0.0f, distY = 0.0f, distZ = 0.0f;
				
				int newMarkedPlayerNumber = eNotMarking;
				//supporting player marking 
				if (supportingMarkedPlayerNumber == theirTeamSortedX[1]->GetNumber())
				{
					distX =  GetPosition().DistanceFrom(theirTeamSortedX[0]->GetPosition());	//new player at pos 0
					distY =  pSupportingPlayer->GetPosition().DistanceFrom(theirTeamSortedX[0]->GetPosition());	//new player at pos 0
					newMarkedPlayerNumber = theirTeamSortedX[0]->GetNumber();
				}
				else 
				{
					distX =  GetPosition().DistanceFrom(theirTeamSortedX[1]->GetPosition());	//new player at pos 1
					distY =  pSupportingPlayer->GetPosition().DistanceFrom(theirTeamSortedX[1]->GetPosition());	//new player at pos 1
					newMarkedPlayerNumber = theirTeamSortedX[1]->GetNumber();
				}
			
				distZ = GetPosition().DistanceFrom(pSupportingPlayer->GetPosition());
				
				if (distX > distY && distZ < distX)
				{
					//switch defending 
					SetMarkedPlayerNumber(supportingMarkedPlayerNumber);
					pSupportingPlayer->SetMarkedPlayerNumber(newMarkedPlayerNumber);
				}
				else
				{
					SetMarkedPlayerNumber(newMarkedPlayerNumber);
				}
			}
			
			
					
			
			//Logic Easy: 3rd player goes for 1st
			/*if(IsSupportingDefenderAlreadyMarking(theirTeamSortedX[0]->GetNumber()))
			{
				SetMarkedPlayerNumber(theirTeamSortedX[1]->GetNumber());
			}
			else
			{
				SetMarkedPlayerNumber(theirTeamSortedX[0]->GetNumber());
			}*/
			
		}
	
	}
}

void CPlayer::MoveToMarkedPlayer_Mark()
{
	
	SelectMarkedPlayer();
	
	
	auto& pMarkedPlayer = game_.GetPlayer(GetMarkedPlayerNumber());

		//float distWithY1 = pitch_.GetOurGoalY1().DistanceFrom(pMarkedPlayer->GetPosition());
	Position markPos = pMarkedPlayer->GetPosition();
		
	Vector towardsOurGoalY1 = markPos.VectorTo(pitch_.GetOurGoalY1());
	Vector towardsOurGoalY1_Diff = towardsOurGoalY1.Scale(0.2);

	markPos.AddVector(towardsOurGoalY1_Diff);
	
	MoveTo(ball_.GetStationaryPosition());
	
	return;

	/*if (pos_.ApproxEqual(markPos, POSITION_BIG_TOLERANCE))
	{
		TurnTo(270.0);
	}
	else
	{
		MoveTo(markPos);
	}*/
}

void CPlayer::MoveForBall()
{
	if (ball_.GetSpeed() > 0)
	{
		// try catching ball.
		Position surePos, almostPos;
		bool isSure = false, isAlmost = false;

		auto& pathPos = ball_.GetPathPos(); 
		auto& pathPosTime = ball_.GetPathPosTime();

		//for (int i = pathPos.size() - 1; i >= 0; --i)
		for (size_t i = 0; i < pathPos.size(); ++i)
		{
			//ignore co-ordinate crossing our goal (i.e x < 0)
			if (pitch_.IsInsideTheirGoalArea(pathPos[i]) ||
				pitch_.IsInsideOurGoalArea(pathPos[i])
			)
			{
				continue;
			}
			
			float t1 = CalculateTimeToReachPosition(pathPos[i]);

			if (t1 < pathPosTime[i])
			{
				surePos = pathPos[i];
				isSure = true;
				break;
			}
			else if (ApproxEqual(t1, pathPosTime[i], 0.055f))
			{
				almostPos = pathPos[i];
				isAlmost = true;
			}
		}

		if (isSure)
		{
			MoveTo(surePos);
		}
		else if (isAlmost)
		{
			MoveTo(almostPos);
		}
		else
		{
			MoveTo(ball_.GetStationaryPosition());
		}		
	}
	else
	{
		MoveTo(ball_.GetStationaryPosition());
	}
}

bool CPlayer::IsTheirPlayerNear(float distance)
{
	auto& theirTeamPtr = game_.GetTheirTeamPtr();
	auto& nonGoalKeepers = theirTeamPtr->GetNonGoalKeepers();
	
		
	for (auto& pPlayer : nonGoalKeepers)
	{
		if (IsWithinRange(pPlayer->GetPosition().x_, pos_.x_- distance, pos_.x_ + distance ) &&
			IsWithinRange(pPlayer->GetPosition().y_, pos_.y_- distance, pos_.y_ + distance ) &&
			pPlayer->GetCapability().runningAbility_ > 10	// not a dead player 
			)
		{
				return true;
		}
	}
	
	return false;
}
bool CPlayer::IsTheirPlayerNearFromFront(float distance)
{
	auto& theirTeamPtr = game_.GetTheirTeamPtr();
	auto& nonGoalKeepers = theirTeamPtr->GetNonGoalKeepers();
	
		
	for (auto& pPlayer : nonGoalKeepers)
	{
		if (IsWithinRange(pPlayer->GetPosition().x_, pos_.x_- distance, pos_.x_ + distance ) &&
			IsWithinRange(pPlayer->GetPosition().y_, pos_.y_- distance, pos_.y_ + distance ) &&
			pPlayer->GetCapability().runningAbility_ > 10 &&// not a dead player 
			pPlayer->GetPosition().x_ > pos_.x_  // opponent player in front
			)
		{
				return true;
		}
	}
	
	return false;
}
float CPlayer::CalculateTimeToTurn(float direction)
{
	float timeTaken = 0.0;
	
	
	if (ApproxEqual(direction_, direction, DIRECTION_TOLERANCE))
	{
		return timeTaken;
	}
	float angleToTurn = direction - direction_;
	if (angleToTurn > 180) 
	{
			// We are turning more than 180 degrees to the right,
			// so this is really a turn to the left...
			angleToTurn = angleToTurn - 360;
	}
	if (angleToTurn < -180) 
	{
			// We are turning more than 180 degrees to the left,
			// so this is really a turn to the right...
			angleToTurn = 360 + angleToTurn;
	}

	if (angleToTurn < 0.0) 
	{
		angleToTurn = -1.0 * angleToTurn;
	}

	int noOfTurn = angleToTurn / MAX_TURN_ANGLE;
	
	if ((angleToTurn - angleToTurn*noOfTurn) > 0.0)
	{
		noOfTurn += 1;
	}
	
	return noOfTurn * GAME_AI_CALCULATION_INTERVAL;
}
float CPlayer::CalculateTimeToReachPosition(const Position& dest)
{
	float timeTaken = 0.0;
	
	float destDirection = pos_.AngleWith(dest);

	float timeTakenToTurn = CalculateTimeToTurn(destDirection);

	float distanceToDest = pos_.DistanceFrom(dest);

	float speed = MAX_PLAYER_SPEED*(capability_.runningAbility_ / 100.0);

	float distanceInSingleTurn = speed*GAME_AI_CALCULATION_INTERVAL;

	//To solve cabaility problem.
	int noOfTurn = 0;
	if (distanceInSingleTurn > 0.0f)
	{
		noOfTurn = distanceToDest / distanceInSingleTurn;
	}
	else
	{	//invalid value;
		return 9999999.0f;	//dummy huge time
	}
	

	if ((distanceToDest - distanceInSingleTurn * noOfTurn) > 0.0)
	{
		noOfTurn += 1;
	}
	
	timeTaken = timeTakenToTurn + noOfTurn*GAME_AI_CALCULATION_INTERVAL;

	return timeTaken;
}

Position CPlayer::GetRandomFreePosition_Striker()
{
	float randY = RandomRangeFloat(15.0,35.0);
	
	float x = ball_.GetPosition().x_ + 50.0;
	
	if (x > 82.0)
		x = 82.0;
		
	if (x < 70.0)	
		x = 70.0;
		
	return { x,randY};
}

Position CPlayer::GetRandomShootAtGoal() 
{ 	int randVal = RandomRangeInteger(0,1);
	float randShootYDiff = RandomRangeFloat(3.7, 3.99);
				
	Position shootAt = pitch_.GetTheirGoalCentre();
	shootAt.y_ += (randVal == 0?-1:1)*randShootYDiff;
	return shootAt;
}


bool CPlayer::IsKickDirectionSafe(float direction, float speed, float limitDistance)
{
	auto& testBall = GetGame().GetTestBall();
	auto& theirNonGoalKeepers = GetGame().GetTheirTeamPtr()->GetNonGoalKeepers();
	
	//setting test ball params.
	testBall.GetPosition() = ball_.GetPosition();
	testBall.SetSpeed(speed * MAX_BALL_SPEED/ 100.0f); 
	testBall.GetVector() = GetVectorFromDirection(direction);
	
	float timetaken;
	testBall.CalculateStationaryPos(timetaken);
	
	Position perIntersection;
	
	//float timeToReachBall = this->CalculateTimeToReachPosition(testBall.GetStationaryPosition());
	
	for (auto& pPlayer: theirNonGoalKeepers)
	{

		if(pPlayer->GetCapability().runningAbility_ < 20.0)
			continue;
			
		if (GetPerpendicularIntersection(testBall.GetPosition(), testBall.GetVirtualStationaryPosition(), pPlayer->GetPosition(), perIntersection))
		{
			float dist = perIntersection.DistanceFrom(pPlayer->GetPosition());
			
			if (dist <= limitDistance)
			{
				return false;
			}
			
		}

	}
	
	return true;
}

void CPlayer::GetSafeKickDirection_GoalKeeper(float& direction, float& speed, float limitDistance)
{
	//IsKickDirectionSafe(direction)
}

bool CPlayer::IsTheirPlayerBehindMe()
{
	auto& theirNonGoalKeepers = GetGame().GetTheirTeamPtr()->GetNonGoalKeepers();
	for (auto& pPlayer: theirNonGoalKeepers)
	{
		if(pPlayer->GetCapability().runningAbility_ < 20.0)
			continue;
	
		if (pPlayer->GetPosition().x_ > pos_.x_)
		{
			return false;
		}
	}
	return true;
}
void CPlayer::ChangeState(int type)
{
	pState_ = CPlayerState::GlobalPlayerState(type);
}

//************Added after Passer AI****************************
bool CPlayer::IsPassSafeTo(CPlayer::Ptr pSupportPlayer)
{
	/*auto& testBall = GetGame().GetTestBall();
	auto& theirNonGoalKeepers = GetGame().GetTheirTeamPtr()->GetNonGoalKeepers();
	
	//setting test ball params.
	testBall.GetPosition() = ball_.GetPosition();
	testBall.SetSpeed(speed * MAX_BALL_SPEED/ 100.0f); 
	testBall.GetVector() = GetVectorFromDirection(direction);
	
	float timetaken;
	testBall.CalculateStationaryPos(timetaken);
	
	Position perIntersection;
	
	//float timeToReachBall = this->CalculateTimeToReachPosition(testBall.GetStationaryPosition());
	*/
	auto& theirNonGoalKeepers = GetGame().GetTheirTeamPtr()->GetNonGoalKeepers();
	
	Position perIntersection;
	
	for (auto& pPlayer: theirNonGoalKeepers)
	{

		if(pPlayer->GetCapability().runningAbility_ < 20.0)
			continue;
			
		if (GetPerpendicularIntersection(GetPosition(), pSupportPlayer->GetPosition(), pPlayer->GetPosition(), perIntersection))
		{	
			float dist = perIntersection.DistanceFrom(pPlayer->GetPosition());
				
			if (dist <= 2.0f)
			{
				return false;
			}
			
		}

	}
		
	
	return true;
}

void CPlayer::Pass()
{
	bool bFoundSafeSupportPlayer = false;
	for (auto& pSupportPlayer: supportPlayers_)
	{
			if (IsPassSafeTo(pSupportPlayer))
			{
				float direction = GetPosition().AngleWith(pSupportPlayer->GetPosition());
				float distance = GetPosition().DistanceFrom(pSupportPlayer->GetPosition());
				
				if (!ApproxEqual(direction,GetDirection(), 0.1f) && 
					!IsTheirPlayerNear(DEFENDER_NO_ONE_CLOSE))
				{
					TurnTo(direction);
				}
				else
				{
					Kick(pSupportPlayer->GetPosition(),ball_.GetSpeedForDistance(distance));
				}
				bFoundSafeSupportPlayer = true;
				break;
			}
	}
	
	//not found - centre kick.
	if (!bFoundSafeSupportPlayer)
	{
		Kick({100.0,25.0},100.0);
	}
}


void CPlayer::KickShortNoStateChange_Striker()
{
	float distanceFromGoal = ball_.GetPosition().DistanceFrom(pitch_.GetTheirGoalCentre());
	if (distanceFromGoal > 20.0)
	{
		if (distanceFromGoal < 22.0)
		{
			KickShort(35.0f);
		}
		else
		{
			KickShort(40.0f);
		}
		
		//ChangeState(CPlayerState::eCounterAttackerStrikerShortKick);
	}
	else
	{
		/*if (distanceFromGoal > 18.5)
		{
			KickShort(35.0f);
			return;
		}*/
		Position shootAt;
		if (!GetShootCache(shootAt))
		{
			//calucate random shoot direction;
			shootAt = GetRandomShootAtGoal();
			SetShootCache(shootAt);
		}
						
					
		float angle = GetPosition().AngleWith(shootAt);
		bool isNoOneClose = IsTheirPlayerNear(STRIKER_NO_ONE_CLOSE);
		
		if (!isNoOneClose && 
			!ApproxEqual(GetDirection(),angle,DIRECTION_TOLERANCE))
		{
			
			TurnTo(angle);
		}
		else
		{
			//no one close and player is little far.
			if (!isNoOneClose && (distanceFromGoal - 15.1f) > 2.5f)
			{
				//MoveTo(shootAt);
				Kick(shootAt,ball_.GetSpeedForDistance(distanceFromGoal - 15.1f) + 5.0);
				//ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
			}
			else
			{
				//someone is close or we are close enough - shoot!!
				game_.noOfGoalAttemptsByUs++;
			
				//try to hit dead centre in case of direction still not aligned.
				if (fabsf(GetDirection() - angle) > 90.0)
				{
					shootAt = pitch_.GetTheirGoalCentre();
					//LOGGER->Log("Inaccurate shoot game_time:%f", GetGame().currentTimeSeconds_);
				}
					
				Kick(shootAt, 100.0);
				//ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
			
				ResetShootCache();
			}
		}
	}
}


bool CPlayer::IsInterceptionValid(const Position& perIntersection)
{
	if (perIntersection.x_  == 0.0 &&
		perIntersection.y_  == 0.0)
			return false;
			
	if (GetPosition().DistanceFrom(perIntersection) < 5.0f)
		return true;
	
	return false;
}