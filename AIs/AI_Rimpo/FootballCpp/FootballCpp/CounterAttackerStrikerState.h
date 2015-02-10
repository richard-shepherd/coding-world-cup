#pragma once

#include "PlayerState.h"

class CCounterAttackerStrikerIdleState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

class CCounterAttackerStrikerChaseBallState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

class CCounterAttackerStrikerShortKickState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};


class CCounterAttackerStrikerTakePossessionState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

class CCounterAttackerStrikerInterceptBallState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

//used only by Zombie player (all capability zero)
class CCounterAttackerZombieState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};