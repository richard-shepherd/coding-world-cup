#pragma once

#include "Position.h"


class CAction
{
public:
	typedef vector<CAction> Vec;
	typedef shared_ptr<CAction> Ptr;
	typedef vector<Ptr> PtrVec;

	enum eActionType
	{
		eNoAction,
		eMove,
		eTurn,
		eKick,
		eTakePossession
	};

	CAction();
	~CAction();

	const char* GetActionString();

	int		 playerNumber_;
	eActionType type_;
	Position destination_;
	float	 direction_;
	float	 speed_;
};

