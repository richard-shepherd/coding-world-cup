#pragma once

#include "CounterAttackerAI.h"


class CPasserAI : public CCounterAttackerAI
{
public:	
	void InitializeOurPlayers();
	void OnTeamInfoEvent();
	void OnStartOfTurnEvent();
	void OnCapabilityRequest();
};