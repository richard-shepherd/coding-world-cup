#pragma once

#include "PlayerState.h"


class CGoalKeeperIdleState : public CPlayerState
{
public:
	void Execute(CPlayer* pPlayer);
};


class CGoalKeeperGuardState : public CPlayerState
{
public:
	void Execute(CPlayer* pPlayer);
};


class CGoalKeeperInterceptBallState : public CPlayerState
{
public:
	 void Execute(CPlayer* pPlayer);
};

class CGoalKeeperTakePossessionState : public CPlayerState
{
public:
	void Execute(CPlayer* pPlayer);
};


class CGoalKeeperKickBallState : public CPlayerState
{
public:
	void Execute(CPlayer* pPlayer);
};

class CGoalKeeperChaseBallState : public CPlayerState
{
public:
	void Execute(CPlayer* pPlayer);
};