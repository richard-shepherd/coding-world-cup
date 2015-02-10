#include "stdafx.h"
#include "GoalkeeperState.h"


void CGoalKeeperGuardState::Execute(CPlayer *pPlayer)
{
	Position hittingAt;			//hitting on goal line

	if (ball_.GetSpeed() == 0 && pitch_.IsInsideOurGoalArea(ball_.GetPosition()))
	{
		pPlayer->MoveTo(ball_.GetPosition());
		pPlayer->ChangeState(CPlayerState::eGoalKeeperChaseBall);
	}
	else if (pitch_.IsLineHittingOurGoal(ball_.GetPosition(), ball_.GetVirtualStationaryPosition(), hittingAt))
	{
		pPlayer->MoveToSaveGoal_GoalKeeper(hittingAt);
		pPlayer->ChangeState(CPlayerState::eGoalKeeperInterceptBall);
		game_.noOfGoalAttemptsOnUs++;
	}
	else
	{
		pPlayer->MoveToGuardGoal();
	}

}
void CGoalKeeperInterceptBallState::Execute(CPlayer* pPlayer)
{
	//ball in range take possession
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	//auto& pClosestBallPlayer = game_.GetClosestPlayer();
	//auto& theirTeamPtr = game_.GetTheirTeamPtr();
	bool isStationaryBallPosInsideOurGoal = pitch_.IsInsideOurGoalArea(ball_.GetStationaryPosition());
	
	if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
		pPlayer->ChangeState(CPlayerState::eGoalKeeperTakePossession);
		return;
	}
	//Guar Goald - condition
	//1. their team controlling the ball.
	//2. ball is stationary or will travel outside our goal area 
	else if ( ball_.IsTheirTeamControlling() || 
			  (!isStationaryBallPosInsideOurGoal && (ball_.GetSpeed() == 0  || 
													(ball_.GetSpeed() > 0.0 && ball_.GetVector().x_ > 0.0))
			   )
			)
	{
		pPlayer->MoveToGuardGoal();
		pPlayer->ChangeState(CPlayerState::eGoalKeeperGuard);
	}
	else
	{
		//continue intercepting goal
	}
	
}

void CGoalKeeperTakePossessionState::Execute(CPlayer* pPlayer)
{
	//ball in range take possession
	if (ball_.GetOwner() == pPlayer->GetNumber())
	{
		// Note: need to wait and kick
		// For testing - kick towards centre (clearance)
		//pPlayer->Kick(pitch_.GetCentreSpot(), 100.0);
		Position safePos = { GUARD_LINE, pitch_.GetOurGoalCentre().y_ };	// {2.5,25.0}
		pPlayer->MoveTo(safePos);
		pPlayer->IncrementGoalKeeperWaitTicks();
		
		//ourTeamPtr->GetPlayerFromPlayerType(supportingPlayerType);
		
		pPlayer->ChangeState(CPlayerState::eGoalKeeperKickBall);
	}
	else if (ball_.GetPosition().DistanceFrom(pPlayer->GetPosition()) < 0.5)
	{
		pPlayer->TakePossession();
		pPlayer->ChangeState(CPlayerState::eGoalKeeperTakePossession);
	}
	else
	{
		pPlayer->MoveToGuardGoal();
		pPlayer->ChangeState(CPlayerState::eGoalKeeperGuard);
	}
}
void CGoalKeeperChaseBallState::Execute(CPlayer *pPlayer)
{
	//interecpt chase same for now
	//CPlayerState::GlobalPlayerState(CPlayerState::eGoalKeeperInterceptBall)->Execute(pPlayer);
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	
	
	if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
		pPlayer->ChangeState(CPlayerState::eGoalKeeperTakePossession);
		return;
	}
	else if (ball_.IsTheirTeamControlling()) // not our team member
	{
		//possession is not with our team..hell! go run.. gaurd goal.
		pPlayer->MoveToGuardGoal();
		pPlayer->ChangeState(CPlayerState::eGoalKeeperGuard);
	}
}

void CGoalKeeperIdleState::Execute(CPlayer *pPlayer)
{
	
}

void CGoalKeeperKickBallState::Execute(CPlayer *pPlayer)
{
	if (ball_.GetOwner() == pPlayer->GetNumber())
	{
		Position safePos =  { GUARD_LINE, pitch_.GetOurGoalCentre().y_ };	// {2.5,25.0}
		
		//move to safe position.
		if (pPlayer->GetPosition().ApproxEqual(safePos,POSITION_BIG_TOLERANCE))
		{
			pPlayer->Kick_GoalKeeper();
		}
	}
	else
	{
		pPlayer->MoveToGuardGoal();
		pPlayer->ChangeState(CPlayerState::eGoalKeeperGuard);
	}
}
