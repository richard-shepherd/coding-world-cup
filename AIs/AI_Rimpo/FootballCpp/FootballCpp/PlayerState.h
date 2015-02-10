#pragma once

class CPlayer;
class CGame;
class CPitch;
class CBall;


class CPlayerState
{
public:
	typedef vector<CPlayerState> Vec;
	typedef CPlayerState* Ptr;
	typedef vector<Ptr> PtrVec;

	enum ePlayerStateType{
		eIdle = 0,
		eDead,
		eShootAtGoal,
		eGoToHome,
		eGoToAttack,
		eShortKick,
		eChaseBall,
		eInterceptPass,
		//----------- goal keeper
		eGoalKeeperIdle,
		eGoalKeeperGuard,
		eGoalKeeperChaseBall,
		eGoalKeeperTakePossession,
		eGoalKeeperKickBall,
		eGoalKeeperInterceptBall,
		
		//-- counter attacker - defender
		eCounterAttackerDefenderIdle,
		eCounterAttackerDefenderGoHome,
		eCounterAttackerDefenderInterceptBall,
		eCounterAttackerDefenderDefend,
		eCounterAttackerDefenderChaseBall,
		eCounterAttackerDefenderTakePossession,
		eCounterAttackerDefenderGuardPass,
		eCounterAttackerDefenderMark,
		
		//-- counter attacker - striker
		eCounterAttackerStrikerIdle,
		eCounterAttackerStrikerGoHome,
		eCounterAttackerStrikerInterceptBall,
		eCounterAttackerStrikerDefend,
		eCounterAttackerStrikerChaseBall,
		eCounterAttackerStrikerTakePossession,
		eCounterAttackerStrikerShortKick,
		
		//-- counter attacker - zombie player
		eCounterAttackerZombie,	
		
		
		ePasserDefenderIdle,
		ePasserMidfielderIdle,
		ePasserStrikerIdle,
		
		//Note: add new state above this
		eLastStateIndex
	};

	CPlayerState();

	virtual void Execute(CPlayer* pPlayer) {}

	static CPlayerState *GlobalPlayerState(int type);
	
	static void	InitGlobalPlayerStateVector();
	

protected:
	CGame& game_;
	CPitch& pitch_;
	CBall& ball_;
};


class CDeadState : public CPlayerState
{
public:	

	void Execute(CPlayer* pPlayer) {}
};
