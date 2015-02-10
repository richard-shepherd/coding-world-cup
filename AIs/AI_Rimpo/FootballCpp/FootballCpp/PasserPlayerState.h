#pragma once

#include "PlayerState.h"

class CPasserDefenderIdleState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};

class CPasserMidfielderIdleState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};


class CPasserStrikerIdleState : public CPlayerState
{
public:
	void Execute(CPlayer *pPlayer);
};