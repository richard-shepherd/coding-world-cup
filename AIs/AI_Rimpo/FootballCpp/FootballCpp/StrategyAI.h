#pragma once

#include"Position.h"

class CStrategyAI
{
public:
	typedef vector<CStrategyAI> Vec;
	typedef shared_ptr<CStrategyAI> Ptr;
	typedef vector<Ptr> PtrVec;

	CStrategyAI();
	virtual ~CStrategyAI();

	
	inline const CPlayer::PtrVec& GetAllPlayers(){ return allPlayers_; }

	virtual void CreateAllPlayers();		// allocate player memory
		
	virtual void InitializeOurPlayers();	//set home and kick off position
	
	virtual void OnGameStartEvent();
	virtual void OnStartOfTurnEvent();
	virtual void OnGoalEvent();
	virtual void OnKickOffEvent();
	virtual void OnHalfTimeEvent();
	virtual void OnTeamInfoEvent();
 
			
	virtual void OnCapabilityRequest();
	virtual void OnKickOffRequest();
	virtual void OnPlayRequest();

protected:
	CPlayer::PtrVec allPlayers_;

	Position::Vec playersHomePos_;
	vector<float> playersHomeDirection_;

	Position::Vec playersKickOffPos_;
	vector<float> playersKickOffDirection_;
};

