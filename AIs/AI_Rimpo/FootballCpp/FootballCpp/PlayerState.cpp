#include"stdafx.h"
#include"PlayerState.h"

#include "Player.h"
#include "GoalkeeperState.h"
#include "CounterAttackerDefenderState.h"
#include "CounterAttackerStrikerState.h"
#include "PasserPlayerState.h"



CPlayerState::CPlayerState() : 		   game_(GetGame()),
									   pitch_(GetGame().GetPitch()),
									   ball_(GetGame().GetBall())
{

}

CPlayerState *CPlayerState::GlobalPlayerState(int type)
{
	auto& globalPlayerStates = GetGame().GetStateVec();
	if( type < globalPlayerStates.size())
		return globalPlayerStates[type];
		
	return nullptr;
}


void CPlayerState::InitGlobalPlayerStateVector()
{
	auto& globalPlayerStates = GetGame().GetStateVec();
		//pre-allocate the whole vector
	if (globalPlayerStates.empty())
	{
		globalPlayerStates.resize(CPlayerState::eLastStateIndex);
	}
	
	
	globalPlayerStates[eIdle] = nullptr,
	globalPlayerStates[eDead] = new CDeadState;
	
	//----------- goal keeper
	globalPlayerStates[eGoalKeeperIdle] 					= new CGoalKeeperIdleState;
	globalPlayerStates[eGoalKeeperGuard] 					= new CGoalKeeperGuardState;
	globalPlayerStates[eGoalKeeperChaseBall] 				= new CGoalKeeperChaseBallState;
	globalPlayerStates[eGoalKeeperTakePossession] 			= new CGoalKeeperTakePossessionState;
	globalPlayerStates[eGoalKeeperKickBall] 				= new CGoalKeeperKickBallState;
	globalPlayerStates[eGoalKeeperInterceptBall] 			= new CGoalKeeperInterceptBallState;
		
	//-- counter attacker - defender
	globalPlayerStates[eCounterAttackerDefenderIdle]   		= new CCounterAttackerDefenderIdleState;
	globalPlayerStates[eCounterAttackerDefenderGoHome]		= new CCounterAttackerDefenderGoHomeState;
	//globalPlayerStates[eCounterAttackerDefenderInterceptBall] = new CCounterAttackerDefenderInterceptBallState;
	globalPlayerStates[eCounterAttackerDefenderDefend]		= new CCounterAttackerDefenderDefendState;
	globalPlayerStates[eCounterAttackerDefenderChaseBall]	= new CCounterAttackerDefenderChaseBallState;
	globalPlayerStates[eCounterAttackerDefenderTakePossession] = new CCounterAttackerDefenderTakePossessionState;
	//globalPlayerStates[eCounterAttackerDefenderGuardPass] = new CCounterAttackerDefenderGuardPassState;
	globalPlayerStates[eCounterAttackerDefenderMark] = new CCounterAttackerDefenderMarkState;
	
		
	//-- counter attacker - striker
	globalPlayerStates[eCounterAttackerStrikerIdle]			 = new CCounterAttackerStrikerIdleState;
	//globalPlayerStates[eCounterAttackerStrikerGoHome]		 = new CCounterAttackerStrikerGoHomeState;
	globalPlayerStates[eCounterAttackerStrikerInterceptBall] = new CCounterAttackerStrikerInterceptBallState;
	//globalPlayerStates[eCounterAttackerStrikerDefend]		 = new CCounterAttackerStrikerDefendState;
	globalPlayerStates[eCounterAttackerStrikerChaseBall]	 = new CCounterAttackerStrikerChaseBallState;
	globalPlayerStates[eCounterAttackerStrikerTakePossession] = new CCounterAttackerStrikerTakePossessionState;
	globalPlayerStates[eCounterAttackerStrikerShortKick]	 = new CCounterAttackerStrikerShortKickState;
	
	//-- counter attacker - zombie
	globalPlayerStates[eCounterAttackerZombie]	 = new CCounterAttackerZombieState;

	globalPlayerStates[ePasserDefenderIdle]	 	 = new CPasserDefenderIdleState;
	globalPlayerStates[ePasserMidfielderIdle]	 = new CPasserMidfielderIdleState;
	globalPlayerStates[ePasserStrikerIdle]	 	 = new CPasserStrikerIdleState;
	
	

}
