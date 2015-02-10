#include "stdafx.h"
#include "Action.h"


CAction::CAction()
{
	type_ = eNoAction;
}


CAction::~CAction()
{
}

const char* CAction::GetActionString()
{
	switch (type_)
	{
		case eMove:
			return "\"MOVE\"";
		case eTurn:
			return "\"TURN\"";
		case eKick:
			return "\"KICK\"";
		case eTakePossession:
			return "\"TAKE_POSSESSION\"";
		case eNoAction:
			return "\"NOTHING\"";
	}
	return "\"\"";
}
