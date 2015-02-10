#pragma once

#include "PlayerState.h"



class CCounterAttackerDefenderIdleState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

class CCounterAttackerDefenderDefendState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

class CCounterAttackerDefenderChaseBallState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

class CCounterAttackerDefenderTakePossessionState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

/*class CCounterAttackerDefenderGuardPassState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};*/

class CCounterAttackerDefenderMarkState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

class CCounterAttackerDefenderGoHomeState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};