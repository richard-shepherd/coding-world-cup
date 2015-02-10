#include "stdafx.h"
#include "PasserPlayerState.h"


void CPasserDefenderIdleState::Execute(CPlayer* pPlayer)
{
		//ball in range take possession
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	CPlayer::Ptr pOurClosestPlayer = GetGame().GetOurClosestPlayer();
	
	auto& action = pPlayer->GetAction(); 
	
	Position perIntersection;
	
	if (pPlayer->HasBall())
	{
		pPlayer->KickShortNoStateChange_Striker();
		//pPlayer->Pass();
		
		/*if (pPlayer->GetPosition().ApproxEqual(pPlayer->GetHomePosition(),POSITION_BIG_TOLERANCE))
		{
			pPlayer->SetHomePosition( { pPlayer->GetHomePosition().x_ + 5.0f, pPlayer->GetHomePosition().y_});
		}*/
	}
	else if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
	}
	else if (GetPerpendicularIntersection(ball_.GetPosition(), ball_.GetVirtualStationaryPosition(), pPlayer->GetPosition(), perIntersection) &&
			pPlayer->IsInterceptionValid(perIntersection))
	{
		pPlayer->MoveForBall();
	}
	else if (pOurClosestPlayer && 
			 pOurClosestPlayer->GetNumber() == pPlayer->GetNumber() &&
			 action.type_ != CAction::eKick)
	{
		pPlayer->MoveTo(ball_.GetStationaryPosition());
	}
	else if (ball_.IsOurGoalKeeperControlling())
	{
		pPlayer->MoveTo(pPlayer->GetKickOffPosition());
	}
	else if(ball_.IsOurTeamControlling())
	{
		
		
		pPlayer->MoveTo(pPlayer->GetHomePosition());
	}
	else
	{
		pPlayer->MoveTo(pPlayer->GetHomePosition());
	}
	
}


void CPasserMidfielderIdleState::Execute(CPlayer* pPlayer)
{
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	
	CPlayer::Ptr pClosestPlayer = GetGame().GetClosestPlayer();
	
	auto& action = pPlayer->GetAction(); 
	
	Position perIntersection;
	
	if (pPlayer->HasBall())
	{
		pPlayer->Pass();
		
		if (pPlayer->GetPosition().ApproxEqual(pPlayer->GetHomePosition(),POSITION_BIG_TOLERANCE))
		{
			pPlayer->SetHomePosition( { pPlayer->GetHomePosition().x_ + 5.0f, RandomRangeFloat(5.0f, 45.0f)});
		}
		//pPlayer->KickShortNoStateChange_Striker();
	}
	else if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
	}
	else if (GetPerpendicularIntersection(ball_.GetPosition(), ball_.GetVirtualStationaryPosition(), pPlayer->GetPosition(), perIntersection) &&
			pPlayer->IsInterceptionValid(perIntersection))
	{
		pPlayer->MoveForBall();
	}
	else if (pClosestPlayer && 
			 pClosestPlayer->GetNumber() == pPlayer->GetNumber() &&
			 action.type_ != CAction::eKick)
	{
		pPlayer->MoveTo(ball_.GetStationaryPosition());
	}
	else if(ball_.IsOurTeamControlling()) 
	{
		
		pPlayer->MoveTo(pPlayer->GetHomePosition());
	}
	else
	{
		pPlayer->MoveTo(pPlayer->GetHomePosition());
	}
}

void CPasserStrikerIdleState::Execute(CPlayer* pPlayer)
{
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	CPlayer::Ptr pClosestPlayer = GetGame().GetClosestPlayer();
	
	auto& action = pPlayer->GetAction(); 
	
	Position perIntersection;
	
	if (pPlayer->HasBall())
	{
		if (pPlayer->GetPosition().x_ > 83.0f)
		{
			float direction = pPlayer->GetPosition().AngleWith(pitch_.GetTheirGoalCentre());
			if(!pPlayer->IsTheirPlayerNear(2.0f) &&
				!ApproxEqual(direction,pPlayer->GetDirection(),0.1f))
			{
				pPlayer->TurnTo(direction);
			}
			else
				pPlayer->Kick(pitch_.GetTheirGoalCentre(), 100.0f);
			
		}
		else
		{
			pPlayer->KickShort(40.0);
		}
		pPlayer->KickShortNoStateChange_Striker();
	}
	else if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
	}
	else if (GetPerpendicularIntersection(ball_.GetPosition(), ball_.GetVirtualStationaryPosition(), pPlayer->GetPosition(), perIntersection) &&
			pPlayer->IsInterceptionValid(perIntersection))
	{
		pPlayer->MoveForBall();
	}
	else if (pClosestPlayer && 
			 pClosestPlayer->GetNumber() == pPlayer->GetNumber() &&
			 action.type_ != CAction::eKick)
	{
		pPlayer->MoveTo(ball_.GetStationaryPosition());
	}
	else if(ball_.IsOurTeamControlling()) 
	{
		if (pPlayer->GetPosition().ApproxEqual(pPlayer->GetHomePosition(),POSITION_BIG_TOLERANCE))
		{
			pPlayer->SetHomePosition( { pPlayer->GetHomePosition().x_, RandomRangeFloat(15.0f, 35.0f)});
		}
		
		pPlayer->MoveTo(pPlayer->GetHomePosition());
	}
	else
	{
		pPlayer->MoveTo(pPlayer->GetHomePosition());
	}
}
