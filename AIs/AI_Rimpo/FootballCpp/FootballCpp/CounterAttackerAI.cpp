#include "stdafx.h"
#include "CounterAttackerAI.h"

CCounterAttackerAI::CCounterAttackerAI()
{
	sameBallPosTickCount_ = 0;
	isFirstTickAfterBallMoved_ = true;
}


void CCounterAttackerAI::InitializeOurPlayers()
{
	//x, y co-ordinate home position

	playersHomePos_ = {
		{ 0, 25 },			//eGoalKeeper = 0,
		{ 25, 5 },			//eLeftDefender,
		{ 80, 30 },			//eCentreDefender,
		{ 25, 45 },			//eRightDefender,
		{ 15, 24 },			//eLeftStriker,
		{ 15, 26 }			//eRightStriker
	};

	//facing towards opponent side
	playersHomeDirection_ = { 90, 90, 90, 90, 90, 90 };

	playersKickOffPos_ = {
		{ 0, 25 },			//eGoalKeeper = 0,
		{ 49, 26 },			//eLeftDefender,
		{ 49, 24},			//eCentreDefender,
		{ 25, 35 },			//eRightDefender,
		{ 15, 21.5 },			//eLeftStriker,	note:dead player
		{ 15, 28.5 }			//eRightStriker     note:dead player 
	};

	//facing towards opponent side
	playersKickOffDirection_ = { 90, 90, 90, 90, 90, 90 };

	playersAttackPos_ = {
		{ 0, 25 },			//eGoalKeeper = 0,
		{ 75, 15 },			//eLeftDefender, (same as home)
		{ 45, 40 },			//eRightDefender, (same as home)
		{ 75, 35 },			//eLeftCounterAttacker,
		{ 80, 20 },			//eCentralCounterAttacker,	note:insider kick off circle
		{ 80, 45 }			//eRightCounterAttacker     note:insider kick off circle  
	};

	//facing towards opponent side
	playersAttackDirection_ = { 90, 90, 90, 90, 90, 90 };

	auto& ourTeamPlayers = GetGame().GetOurTeamPtr()->GetPlayers();

	for (auto& pPlayer : ourTeamPlayers)
	{
		pPlayer->SetHomePosition(playersHomePos_[pPlayer->GetType()]);
		pPlayer->SetHomeDirection(playersHomeDirection_[pPlayer->GetType()]);

		pPlayer->SetKickOffPosition(playersKickOffPos_[pPlayer->GetType()]);
		pPlayer->SetKickOffDirection(playersKickOffDirection_[pPlayer->GetType()]);

		pPlayer->SetAttackPosition(playersAttackPos_[pPlayer->GetType()]);
		pPlayer->SetAttackDirection(playersAttackDirection_[pPlayer->GetType()]);
	}
}
void CCounterAttackerAI::OnTeamInfoEvent()
{
	auto& ourTeamPtr = GetGame().GetOurTeamPtr();
	auto& goalKeeperPtr = ourTeamPtr->GetGoalKeeper();

	goalKeeperPtr->SetType(eGoalKeeper);
	goalKeeperPtr->ChangeState(CPlayerState::eGoalKeeperGuard);
	

	auto& ourTeamNonGKPlayers = ourTeamPtr->GetNonGoalKeepers();
	
	int i = CPlayer::eLeftDefender;
	for (auto& pPlayer : ourTeamNonGKPlayers)
	{
		pPlayer->SetType(i++);

		if (pPlayer->GetType() == CPlayer::eLeftDefender)
		{
			pPlayer->ChangeState(CPlayerState::eCounterAttackerDefenderIdle);
			//pPlayer->ChangeState(CPlayerState::eDead);
		}
		else if (pPlayer->GetType() == CPlayer::eRightDefender)
		{
			pPlayer->ChangeState(CPlayerState::eCounterAttackerDefenderIdle);
			//pPlayer->ChangeState(CPlayerState::eDead);
		}
		else if (pPlayer->GetType() == CPlayer::eCentreDefender)
		{
			pPlayer->ChangeState(CPlayerState::eCounterAttackerStrikerIdle);
			//pPlayer->ChangeState(CPlayerState::eDead);
		}
		else
		{
			pPlayer->ChangeState(CPlayerState::eCounterAttackerZombie);
		}
		
		
		/*if (pPlayer->GetType() == CPlayer::eLeftDefender ||
			pPlayer->GetType() == CPlayer::eRightDefender ||
			pPlayer->GetType() == CPlayer::eCentreDefender)
		{
			pPlayer->ChangeState(CPlayerState::eDead);
		}*/
		
	}


	//map player type to player ptr;
	ourTeamPtr->MapPlayerTypeToPlayerPtr();

	InitializeOurPlayers();
}
void CCounterAttackerAI::OnStartOfTurnEvent()
{
	auto& ball = GetGame().GetBall();
	auto& pitch = GetGame().GetPitch();
	auto& ourTeamPtr = GetGame().GetOurTeamPtr();
	auto& ourPlayers = ourTeamPtr->GetPlayers();

	auto& ourGoalKeeper = ourTeamPtr->GetGoalKeeper();
	
	
	GetGame().CalculateAllPlayerToBallSortedDistance();
	GetGame().SortTheirTeamX();
	
	//calulate all path position and stationary position.
	ball.EstimatePath();


	//-----------------------------------------------------------------------
	//Note: This has been put to check hang state of the AI - no activity.
	//----------------------------------------------------------------------
	if (lastBallPos_.x_ == ball.GetPosition().x_ &&
		lastBallPos_.y_ == ball.GetPosition().y_)
			sameBallPosTickCount_++;
	else
		sameBallPosTickCount_ = 0;
			
	if (sameBallPosTickCount_ > 40)
	{
		LOGGER->Log("GAME STUCK!! game_time:%f",GetGame().currentTimeSeconds_); //put breakpoint here signifies hang state.
	}
	
	lastBallPos_ = ball.GetPosition();
	//----------------------------------------------------------------------


	//----------------------------------------------------------------------
	//Note: This has been put to identify attempts that are on target.
	//----------------------------------------------------------------------
	if (ball.GetSpeed() > 0)
	{
		if (isFirstTickAfterBallMoved_)
		{
			Position hittingAt;
			if (pitch.IsLineHittingTheirGoal(ball.GetPosition(), ball.GetVirtualStationaryPosition()))
			{
				GetGame().noOfAttemptsOnTarget++;
			}
			
			isFirstTickAfterBallMoved_ = false;
		}
		
	}
	else
		isFirstTickAfterBallMoved_ = true;
	//----------------------------------------------------------------------
	
	

	for (auto& pPlayer : ourPlayers)
	{
		/*if (pPlayer->GetType() == CPlayer::eLeftDefender ||
			pPlayer->GetType() == CPlayer::eCentreDefender ||
			pPlayer->GetType() == CPlayer::eRightDefender ||
			pPlayer->GetType() == CPlayer::eGoalKeeper)*/
		{
			pPlayer->GetState()->Execute(pPlayer.get());
		}
	}
	
}

void CCounterAttackerAI::OnCapabilityRequest()
{
	auto& totalCapability = GetGame().GetOurTeamPtr()->GetTotalCapability();
	auto& ourPlayers = GetGame().GetOurTeamPtr()->GetPlayers();

	for (auto& pPlayer : ourPlayers)
	{
		pPlayer->GetCapability().kickingAbility_ = totalCapability.kickingAbility_ / 6.0;
		pPlayer->GetCapability().runningAbility_ = totalCapability.runningAbility_ / 6.0;
		pPlayer->GetCapability().ballControlAbility_ = totalCapability.ballControlAbility_ / 6.0;
		pPlayer->GetCapability().tacklingAbility_ = totalCapability.tacklingAbility_ / 6.0;

		if (pPlayer->GetType() == CPlayer::eLeftDefender ||
			pPlayer->GetType() == CPlayer::eCentreDefender ||
			pPlayer->GetType() == CPlayer::eRightDefender ||
			pPlayer->GetType() == CPlayer::eGoalKeeper)
		{
			pPlayer->GetCapability().kickingAbility_ = 100.0;
			pPlayer->GetCapability().runningAbility_ = 100.0;
			pPlayer->GetCapability().ballControlAbility_ = 100.0;
			pPlayer->GetCapability().tacklingAbility_ = 100.0;
		}
		else
		{
			pPlayer->GetCapability().kickingAbility_ = 0.0;
			pPlayer->GetCapability().runningAbility_ = 0.0;
			pPlayer->GetCapability().ballControlAbility_ = 0.0;
			pPlayer->GetCapability().tacklingAbility_ = 0.0;
		}
	}
}
