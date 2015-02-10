#include "stdafx.h"
#include "PasserAI.h"


void CPasserAI::InitializeOurPlayers()
{
	playersHomePos_ = {
		{ 0, 25 },			//eGoalKeeper = 0,
		{ 25, 15 },			//eLeftDefender,
		{ 25, 25 },			//eCentreDefender,
		{ 25, 35 },			//eRightDefender,
		{ 83.0, 25.0 },			//eLeftStriker,
		{ 87.0, 30.0 }			//eRightStriker
	};

	//facing towards opponent side
	playersHomeDirection_ = { 90, 90, 90, 90, 90, 90 };
	
	playersKickOffPos_ = {
		{ 0, 25 },			//eGoalKeeper = 0,
		{ 25, 10 },			//eLeftDefender,
		{ 49, 25},			//eCentreDefender,
		{ 25, 40 },			//eRightDefender,
		//{ 15, 21.5 },			//eLeftStriker,	note:dead player
		//{ 15, 28.5 }			//eRightStriker     note:dead player 
		{ 49, 15 },			//eLeftStriker,	note:dead player
		{ 49, 35 }			//eRightStriker     note:dead player 
	};

	//facing towards opponent side
	playersKickOffDirection_ = { 90, 90, 90, 90, 90, 90 };
	
	auto& ourTeamPlayers = GetGame().GetOurTeamPtr()->GetPlayers();

	for (auto& pPlayer : ourTeamPlayers)
	{
		pPlayer->SetHomePosition(playersHomePos_[pPlayer->GetType()]);
		pPlayer->SetHomeDirection(playersHomeDirection_[pPlayer->GetType()]);

		pPlayer->SetKickOffPosition(playersKickOffPos_[pPlayer->GetType()]);
		pPlayer->SetKickOffDirection(playersKickOffDirection_[pPlayer->GetType()]);

	}
	
	auto& ourTeamPtr = GetGame().GetOurTeamPtr();
	auto& ourPlayers = ourTeamPtr->GetPlayers();
	
	for (auto& pPlayer : ourPlayers)
	{
		//Note: sequence of player added as supporting player is important
		/*if (pPlayer->GetType() == CPlayer::eGoalKeeper)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightDefender));
		}
		else if (pPlayer->GetType() == CPlayer::eLeftDefender)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eGoalKeeper));
		}
		else if (pPlayer->GetType() == CPlayer::eRightDefender)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eCentreDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftStriker));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightStriker));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftDefender));
		}
		else if (pPlayer->GetType() == CPlayer::eCentreDefender)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftStriker));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightStriker));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightDefender));
		}
		else if (pPlayer->GetType() == CPlayer::eLeftStriker)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eCentreDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightStriker));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightDefender));
		}
		else if (pPlayer->GetType() == CPlayer::eRightStriker)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eCentreDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftStriker));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightDefender));
		}*/
		
		if (pPlayer->GetType() == CPlayer::eGoalKeeper)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightDefender));
		}
		else if (pPlayer->GetType() == CPlayer::eLeftDefender)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eCentreDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eGoalKeeper));
		}
		else if (pPlayer->GetType() == CPlayer::eRightDefender)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eCentreDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eGoalKeeper));
		}
		else if (pPlayer->GetType() == CPlayer::eCentreDefender)
		{
			//pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftStriker));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftDefender));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightDefender));
			//pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightStriker));
		}
		/*else if (pPlayer->GetType() == CPlayer::eLeftStriker)
		{
			//pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eRightStriker));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eCentreDefender));
		}
		else if (pPlayer->GetType() == CPlayer::eRightStriker)
		{
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eLeftStriker));
			pPlayer->AddSupportPlayer(ourTeamPtr->GetPlayerFromPlayerType(CPlayer::eCentreDefender));
		}*/
		
	}
	
}
void CPasserAI::OnTeamInfoEvent()
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

		if (pPlayer->GetType() == CPlayer::eLeftDefender || 
			pPlayer->GetType() == CPlayer::eRightDefender)
		{
			pPlayer->ChangeState(CPlayerState::ePasserDefenderIdle);
			//pPlayer->ChangeState(CPlayerState::eDead);
		}
		else if (pPlayer->GetType() == CPlayer::eCentreDefender)
		{
			pPlayer->ChangeState(CPlayerState::ePasserMidfielderIdle);
			//pPlayer->ChangeState(CPlayerState::eDead);
		}
		else
		{
			//pPlayer->ChangeState(CPlayerState::ePasserStrikerIdle);
			pPlayer->ChangeState(CPlayerState::eDead);
		}
		
	}


	//map player type to player ptr;
	ourTeamPtr->MapPlayerTypeToPlayerPtr();
	
	
	InitializeOurPlayers();
}
void CPasserAI::OnStartOfTurnEvent()
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
void CPasserAI::OnCapabilityRequest()
{
		auto& totalCapability = GetGame().GetOurTeamPtr()->GetTotalCapability();
	auto& ourPlayers = GetGame().GetOurTeamPtr()->GetPlayers();

	for (auto& pPlayer : ourPlayers)
	{
		/*if (pPlayer->GetType() == CPlayer::eGoalKeeper)
		{
			//pPlayer->GetCapability().kickingAbility_ = 90.0;
			pPlayer->GetCapability().kickingAbility_ = 90.0;
			pPlayer->GetCapability().runningAbility_ = 100.0;
			pPlayer->GetCapability().ballControlAbility_ = 100.0;
			pPlayer->GetCapability().tacklingAbility_ = 0.0;
		}
		else if (pPlayer->GetType() == CPlayer::eLeftDefender)
		{
			//pPlayer->GetCapability().kickingAbility_ = 65.0;
			pPlayer->GetCapability().kickingAbility_ = 80.0;
			pPlayer->GetCapability().runningAbility_ = 80.0;
			pPlayer->GetCapability().ballControlAbility_ = 65.0;
			pPlayer->GetCapability().tacklingAbility_ = 100.0;
		}
		else if (pPlayer->GetType() == CPlayer::eRightDefender)
		{
			//pPlayer->GetCapability().kickingAbility_ = 70.0;
			pPlayer->GetCapability().kickingAbility_ = 80.0;
			pPlayer->GetCapability().runningAbility_ = 80.0;
			pPlayer->GetCapability().ballControlAbility_ = 70.0;
			pPlayer->GetCapability().tacklingAbility_ = 100.0;
		}
		else if (pPlayer->GetType() == CPlayer::eCentreDefender)
		{
			//pPlayer->GetCapability().kickingAbility_ = 70.0;
			pPlayer->GetCapability().kickingAbility_ = 90.0;
			pPlayer->GetCapability().runningAbility_ = 70.0;
			pPlayer->GetCapability().ballControlAbility_ = 60.0;
			pPlayer->GetCapability().tacklingAbility_ = 60.0;
		}
		else if (pPlayer->GetType() == CPlayer::eLeftStriker)
		{
			//pPlayer->GetCapability().kickingAbility_ = 70.0;
			pPlayer->GetCapability().kickingAbility_ = 60.0;
			pPlayer->GetCapability().runningAbility_ = 70.0;
			pPlayer->GetCapability().ballControlAbility_ = 100.0;
			pPlayer->GetCapability().tacklingAbility_ = 100.0;
		}
		else 
		{
			pPlayer->GetCapability().kickingAbility_ = 0.0;
			pPlayer->GetCapability().runningAbility_ = 0.0;
			pPlayer->GetCapability().ballControlAbility_ = 0.0;
			pPlayer->GetCapability().tacklingAbility_ = 0.0;
		}*/
		
		//pPlayer->GetCapability().kickingAbility_ = 400.0/6;

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