#include "stdafx.h"
#include "StrategyAI.h"


CStrategyAI::CStrategyAI()
{
	
}

CStrategyAI::~CStrategyAI()
{
}

void CStrategyAI::CreateAllPlayers()
{
	for (int i = 0; i < MAX_NO_OF_PLAYERS; i++)
	{
		CPlayer::Ptr pPlayer = make_shared<CPlayer>();

		pPlayer->SetNumber(i);

		pPlayer->ChangeState(CPlayerState::eIdle);

		allPlayers_.push_back(pPlayer);
	}
	
}
void CStrategyAI::InitializeOurPlayers()
{
	//x, y co-ordinate home position

	playersHomePos_ = {
		{ 0, 25 },			//eGoalKeeper = 0,
		{ 17, 30 },			//left defender,
		{ 17, 40 },			//centre defender,
		{ 25, 35 },			//right defender,
		{ 25, 25 },			//left attacker,
		{ 25, 45 }			//right attacker
	};

	playersKickOffPos_ = {
		{ 0, 25 },			//eGoalKeeper = 0,
		{ 17, 30 },			//eLeftDefender,
		{ 17, 40 },			//eRightDefender,
		{ 25, 35 },			//eLeftCounterAttacker,
		{ 25, 25 },			//eCentralCounterAttacker,
		{ 25, 45 }			//eRightCounterAttacker
	};
}
void CStrategyAI::OnGameStartEvent()
{

}
void CStrategyAI::OnStartOfTurnEvent()
{

}

void CStrategyAI::OnGoalEvent()
{
}

void CStrategyAI::OnKickOffEvent()
{
}

void CStrategyAI::OnHalfTimeEvent()
{
}

void CStrategyAI::OnTeamInfoEvent()
{
}


//---------------------REQUEST -------------------------------

void CStrategyAI::OnCapabilityRequest()
{
	auto& totalCapability = GetGame().GetOurTeamPtr()->GetTotalCapability();
	auto& ourPlayers = GetGame().GetOurTeamPtr()->GetPlayers();
	
	for (auto& pPlayer : ourPlayers)
	{
		pPlayer->GetCapability().kickingAbility_ = totalCapability.kickingAbility_ / 6.0;
		pPlayer->GetCapability().runningAbility_ = totalCapability.runningAbility_ / 6.0;
		pPlayer->GetCapability().ballControlAbility_ = totalCapability.ballControlAbility_ / 6.0;
		pPlayer->GetCapability().tacklingAbility_ = totalCapability.tacklingAbility_ / 6.0;
	}
}
void CStrategyAI::OnKickOffRequest()
{

}
void CStrategyAI::OnPlayRequest()
{

}

