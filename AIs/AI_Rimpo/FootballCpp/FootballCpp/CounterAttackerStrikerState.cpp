#include "stdafx.h"
#include "CounterAttackerStrikerState.h"


void CCounterAttackerStrikerIdleState::Execute(CPlayer* pPlayer)
{
	Position perIntersection;
	auto& pClosestPlayer = game_.GetClosestPlayer();
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	//ball is travelling and iterception is possible
	if (pPlayer->HasBall())
	{
		pPlayer->KickShort_Striker();
	}
	else if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerTakePossession);
	}
	else if (ball_.GetSpeed() > 0 &&
	    GetPerpendicularIntersection(ball_.GetPosition(), ball_.GetVirtualStationaryPosition(), pPlayer->GetPosition(), perIntersection))
	{
		//try to intercept ball
		pPlayer->MoveForBall();
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerInterceptBall);
	}
	else
	{
		//interseption not possible.
		if(pClosestPlayer->GetNumber() != pPlayer->GetNumber() && //not me
		   pClosestPlayer->IsOurTeamMember())					  // is our team
		{
			if (pPlayer->GetPosition().ApproxEqual(pPlayer->GetHomePosition(),POSITION_BIG_TOLERANCE))
			{
			pPlayer->SetHomePosition(pPlayer->GetRandomFreePosition_Striker());
			}
			
			pPlayer->MoveTo(pPlayer->GetHomePosition());
		}
		else if (ball_.IsFreeBall() && !pitch_.IsOurHalf(ball_.GetStationaryPosition()))
		{
			pPlayer->MoveTo(ball_.GetStationaryPosition());
		}
		else
		{
			if (pPlayer->GetPosition().ApproxEqual(pPlayer->GetHomePosition(),POSITION_BIG_TOLERANCE))
			{
			pPlayer->SetHomePosition(pPlayer->GetRandomFreePosition_Striker());
			}
			pPlayer->MoveTo(pPlayer->GetHomePosition());
		}
		
	}
}

void CCounterAttackerStrikerChaseBallState::Execute(CPlayer* pPlayer)
{
	auto& pClosestPlayer = game_.GetClosestPlayer();
	//ball in range take possession
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	if (pPlayer->HasBall())
	{
		pPlayer->KickShort_Striker();
	}
	else if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerTakePossession);
		return;
	}
	else if(pClosestPlayer->GetNumber() != pPlayer->GetNumber() && //not me
		   pClosestPlayer->IsOurTeamMember())					  // is our team
	{
			pPlayer->MoveTo(pPlayer->GetHomePosition());
			pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
	}
	else if (ball_.GetSpeed() == 0 && ball_.IsFreeBall())
	{
		if (!pitch_.IsOurHalf(ball_.GetStationaryPosition()))
			pPlayer->MoveTo(ball_.GetStationaryPosition());
		else
		{
			pPlayer->MoveTo(pPlayer->GetHomePosition());
			pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
		}
	}
	//lost possession
	else if (ball_.IsTheirTeamControlling() 			//their player in control of ball
			|| ball_.IsOurTeamControlling()
			) // not our team member
	{
		//lost possession. go home bitch
		pPlayer->MoveTo(pPlayer->GetHomePosition());
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
	}

}

void CCounterAttackerStrikerInterceptBallState::Execute(CPlayer *pPlayer)
{
	//ball in range take possession
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerTakePossession);
		return;
	}
	else if (ball_.GetSpeed() == 0 && ball_.IsFreeBall())
	{
		if (!pitch_.IsOurHalf(ball_.GetStationaryPosition()))
			pPlayer->MoveTo(ball_.GetStationaryPosition());
		else
		{
			pPlayer->MoveTo(pPlayer->GetHomePosition());
			pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
		}
	}
	else if (ball_.IsTheirTeamControlling()  || ball_.IsOurTeamControlling()	//their player in control of ball
			 //game_.GetClosestPlayer()->IsTheirTeamMember()		//their player is closest
			) // not our team member
	{
		//lost possession. go home bitch
		pPlayer->MoveTo(pPlayer->GetHomePosition());
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
	}
}

void CCounterAttackerStrikerTakePossessionState::Execute(CPlayer* pPlayer)
{
	//ball in range take possession
	if (pPlayer->HasBall())
	{
		pPlayer->KickShort_Striker();
	}
	else if (ball_.GetPosition().DistanceFrom(pPlayer->GetPosition()) < 0.5)
	{
		pPlayer->TakePossession();
		//pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerTakePossession);
	}
	//lost possession
	else if (ball_.IsTheirTeamControlling())//their player in control of ball
	{
		//lost possession. bitch go home
		pPlayer->MoveTo(pPlayer->GetHomePosition());
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
	}
	else if (ball_.GetSpeed() == 0 && ball_.IsFreeBall())
	{
		if (!pitch_.IsOurHalf(ball_.GetStationaryPosition()))
			pPlayer->MoveTo(ball_.GetStationaryPosition());
		else
		{
			pPlayer->MoveTo(pPlayer->GetHomePosition());
			pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
		}
	}
	else if (ball_.IsOurTeamControlling())//our player in control of ball
	{
		//lost possession. bitch go home
		pPlayer->MoveTo(pPlayer->GetHomePosition());
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
	}
}

void CCounterAttackerStrikerShortKickState::Execute(CPlayer* pPlayer)
{
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	
	if (pPlayer->HasBall())
	{
		pPlayer->KickShort_Striker();
	}
	else if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerTakePossession);
	}
	else if (ball_.IsFreeBall())	// no owner
	{
		//pPlayer->MoveTo(ball_.GetStationaryPosition());
		if (!pitch_.IsOurHalf(ball_.GetStationaryPosition()))
			pPlayer->MoveTo(ball_.GetStationaryPosition());
		else
		{
			pPlayer->MoveTo(pPlayer->GetHomePosition());
			pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
		}
	}
	//lost possession
	else if (ball_.IsTheirTeamControlling() 				//their player in control of ball
			 //game_.GetClosestPlayer()->IsTheirTeamMember()		//their player is closest
			) // not our team member
	{
		//lost possession. bitch go home
		pPlayer->MoveTo(pPlayer->GetHomePosition());
		pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
	}
}


void CCounterAttackerZombieState::Execute(CPlayer *pPlayer)
{
	float distanceFromBall = ball_.GetPosition().DistanceFrom(pPlayer->GetPosition());
	
	if (pPlayer->HasBall())
	{
		pPlayer->Kick({ 50.0f, pPlayer->GetPosition().y_},100);
	}
	else if (distanceFromBall < 0.5)
	{
		pPlayer->TakePossession();
	}
}