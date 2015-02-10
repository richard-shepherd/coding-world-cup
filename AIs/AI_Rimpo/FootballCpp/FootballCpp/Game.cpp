#include "stdafx.h"
#include "Game.h"



CGame::CGame()
{
	CPlayerState::InitGlobalPlayerStateVector();
	//creating strategy
	strategyPtr_ = make_shared<CCounterAttackerAI>();
/*#ifdef _DEBUG
	strategyPtr_ = make_shared<CPasserAI>();
#else
	strategyPtr_ = make_shared<CCounterAttackerAI>();
#endif */
	strategyPtr_->CreateAllPlayers();

	//copying over all the players created by strategy. (helps in creating custom player)
	allPlayers_ = strategyPtr_->GetAllPlayers();
	

	//creating team (our and their)
	ourTeamPtr_ = make_shared<CTeam>();
	theirTeamPtr_ = make_shared<CTeam>();

	teams_.push_back(ourTeamPtr_);
	teams_.push_back(theirTeamPtr_);
		
	noOfGoalAttemptsOnUs = 0;
	noOfGoalAttemptsByUs = 0;
	noOfTimesOurTeamOwnBall = 0;
	noOfTimesTheirTeamOwnBall = 0;
	noOfGoalsOur = 0;
	noOfGoalsTheir = 0;
	noOfTicksInOurHalf = 0;
	noOfTicksInTheirHalf = 0;
	
	noOfTicksInOurGoalArea = 0;
	noOfTicksInTheirGoalArea = 0;
	
	noOfAttemptsOnTarget = 0;
	noOfTicksInOurDefenceHalf = 0;
	noOfTicksInTheirDefenceHalf = 0;
	
	noOfTackleWonOur = 0;
	noOfTackleWonTheir = 0;
	prevBallOwner = -1;
}


CGame::~CGame()
{
	
}


int CGame::Process(const string& sJsonMsg)
{
	Document document;

	if (document.Parse(sJsonMsg.c_str()).HasParseError() == true)
	{
		return -1;
	}
	
	const Value& messageTypeValue = document["messageType"];

	Value::MemberIterator eventTypeItr = document.FindMember("eventType");
	
	if (eventTypeItr != document.MemberEnd())
	{
		//Note: More frequently received event should be above in if condition.
		//      This is to avoiding if else check.

		if (eventTypeItr->value == "START_OF_TURN")
		{
			currentTimeSeconds_ = document["game"]["currentTimeSeconds"].GetDouble();

			ball_.ProcessStartOfTurn(document["ball"]);

			ourTeamPtr_->ProcessStartOfTurn(document);
			theirTeamPtr_->ProcessStartOfTurn(document);

			strategyPtr_->OnStartOfTurnEvent();
			
			//stats
			if(ourTeamPtr_->IsMember(ball_.GetOwner()))
			{
					noOfTimesOurTeamOwnBall++;
			}
			else if(theirTeamPtr_->IsMember(ball_.GetOwner()))
			{
					noOfTimesTheirTeamOwnBall++;
			}
			
			if(pitch_.IsOurHalf(ball_.GetPosition()))
			{
				noOfTicksInOurHalf++;
			}
			else
			{
				noOfTicksInTheirHalf++;
			}
			
			
			if (pitch_.IsInsideOurGoalArea(ball_.GetPosition()))
			{
				noOfTicksInOurGoalArea++;
			}
			else if(pitch_.IsInsideTheirGoalArea(ball_.GetPosition()))
			{	
				noOfTicksInTheirGoalArea++;
			}
			
			if(ball_.GetPosition().x_ < 25.0)
			{
				noOfTicksInOurDefenceHalf++;
			}
			else if (ball_.GetPosition().x_ > 75.0)
			{
				noOfTicksInTheirDefenceHalf++;
			}
			
			if(theirTeamPtr_->IsMember(prevBallOwner) && ourTeamPtr_->IsMember(ball_.GetOwner()))
			{
				noOfTackleWonOur++;
			}
			
			if(ourTeamPtr_->IsMember(prevBallOwner) &&  theirTeamPtr_->IsMember(ball_.GetOwner()))
			{
				noOfTackleWonTheir++;
			}
				
			prevBallOwner = ball_.GetOwner();
			 
			if (currentTimeSeconds_ > 1800.0)
			{
				LOGGER->Log("NoOfAttempts Our:[Attempts:%d OnTarget:%d] Their:%d ", noOfGoalAttemptsByUs, noOfAttemptsOnTarget, noOfGoalAttemptsOnUs);
				LOGGER->Log("NoOfGoals Our:%d Their:%d",noOfGoalsOur, noOfGoalsTheir);
				LOGGER->Log("NOOfTimes Ball Our:%d Their:%d",noOfTimesOurTeamOwnBall, noOfTimesTheirTeamOwnBall);
				LOGGER->Log("NoOfTicks Our Half:%d Their Half:%d",noOfTicksInOurHalf, noOfTicksInTheirHalf);
				LOGGER->Log("NoOfTicks Our Defence Half:%d Their Defence Half:%d",noOfTicksInOurDefenceHalf, noOfTicksInTheirDefenceHalf);
				LOGGER->Log("NoOfTicks Our Goal Area:%d Their Goal Area:%d",noOfTicksInOurGoalArea, noOfTicksInTheirGoalArea);
				LOGGER->Log("NoOfTackleWon Our:%d Their:%d", noOfTackleWonOur, noOfTackleWonTheir);
			}
		}
		else if (eventTypeItr->value == "GOAL")
		{
			if (pitch_.IsOurHalf(ball_.GetPosition()))
			{
				noOfGoalsTheir++;
			}
			else
			{
				noOfGoalsOur++;
			}
			strategyPtr_->OnGoalEvent();
			
			//LOGGER->Log("GOAL!! game_time:%f", GetGame().currentTimeSeconds_);
		}
		else if (eventTypeItr->value == "HALF_TIME")
		{
			strategyPtr_->OnHalfTimeEvent();
			
			LOGGER->Log("NoOfAttempts Our:[Attempts:%d OnTarget:%d] Their:%d ", noOfGoalAttemptsByUs, noOfAttemptsOnTarget, noOfGoalAttemptsOnUs);
			LOGGER->Log("NoOfGoals Our:%d Their:%d",noOfGoalsOur, noOfGoalsTheir);
			LOGGER->Log("NOOfTimes Ball Our:%d Their:%d",noOfTimesOurTeamOwnBall, noOfTimesTheirTeamOwnBall);
			LOGGER->Log("NoOfTicks Our Half:%d Their Half:%d",noOfTicksInOurHalf, noOfTicksInTheirHalf);
			LOGGER->Log("NoOfTicks Our Defence Half:%d Their Defence Half:%d",noOfTicksInOurDefenceHalf, noOfTicksInTheirDefenceHalf);
			LOGGER->Log("NoOfTicks Our Goal Area:%d Their Goal Area:%d",noOfTicksInOurGoalArea, noOfTicksInTheirGoalArea);
			LOGGER->Log("NoOfTackleWon Our:%d Their:%d", noOfTackleWonOur, noOfTackleWonTheir);
			LOGGER->Log("HALF TIME");
		}
		else if (eventTypeItr->value == "KICKOFF")
		{
			ourTeamPtr_->ProcessTeamKickOffInfo(document);
			theirTeamPtr_->ProcessTeamKickOffInfo(document);

			strategyPtr_->OnKickOffEvent();
		}
		else if (eventTypeItr->value == "TEAM_INFO")
		{
			ourTeamPtr_->ProcessTeamInfo(document);

			//setting theirTeam identifier since we dont receive in team info.
			if (ourTeamPtr_->GetTeamIdentifier() == "team1")
			{
				theirTeamPtr_->SetTeamNumber(2);
				theirTeamPtr_->SetTeamIdentifier("team2");
			}
			else
			{
				theirTeamPtr_->SetTeamNumber(1);
				theirTeamPtr_->SetTeamIdentifier("team1");
			}

			strategyPtr_->OnTeamInfoEvent();
		}
		else if (eventTypeItr->value == "GAME_START")
		{
			pitch_.ProcessPitch(document["pitch"]);

			gameLengthSeconds_ = document["gameLengthSeconds"].GetDouble();

			strategyPtr_->OnGameStartEvent();
		}
		

	}
	else
	{
		//Note: More frequently received request should be above in if condition.
		//      This is to avoiding if else check.

		const Value& requestTypeValue = document["requestType"];

		if (requestTypeValue == "PLAY")
		{
			strategyPtr_->OnPlayRequest();

			PrintPlayResponse();
		}
		else if (requestTypeValue == "KICKOFF")
		{
			strategyPtr_->OnKickOffRequest();

			PrintKickOffResponse();
		}
		else if (requestTypeValue == "CONFIGURE_ABILITIES")
		{
	
			ourTeamPtr_->ProcessCapabilitiesRequest(document);

			strategyPtr_->OnCapabilityRequest();

			PrintCapabilityResponse();
		}
	}

	return 0;
}
void CGame::PrintCapabilityResponse()
{
	/*template <class TYPE> std::string Str(const TYPE & t) {
		std::ostringstream os;
		os << t;
		return os.str();
	}*/

	//{"requestType":"CONFIGURE_ABILITIES", "players" : [{"playerNumber":6, "kickingAbility" : 66.6667, "runningAbility" : 66.6667, "ballControlAbility" : 66.6667, "tacklingAbility" : 66.6667}, { "playerNumber":7, "kickingAbility" : 66.6667, "runningAbility" : 66.6667, "ballControlAbility" : 66.6667, "tacklingAbility" : 66.6667 }, { "playerNumber":8, "kickingAbility" : 66.6667, "runningAbility" : 66.6667, "ballControlAbility" : 66.6667, "tacklingAbility" : 66.6667 }, { "playerNumber":9, "kickingAbility" : 66.6667, "runningAbility" : 66.6667, "ballControlAbility" : 66.6667, "tacklingAbility" : 66.6667 }, { "playerNumber":10, "kickingAbility" : 66.6667, "runningAbility" : 66.6667, "ballControlAbility" : 66.6667, "tacklingAbility" : 66.6667 }, { "playerNumber":11, "kickingAbility" : 66.6667, "runningAbility" : 66.6667, "ballControlAbility" : 66.6667, "tacklingAbility" : 66.6667 }]}
	cout << "{\"requestType\":\"CONFIGURE_ABILITIES\",\"players\" : [";

	const CPlayer::PtrVec& ourTeamPlayers = ourTeamPtr_->GetPlayers();

	int i = 0;
	for (auto& pPlayer : ourTeamPlayers)
	{
		if (0 != i++)
			cout << ",";

		cout << "{";
		cout << "\"playerNumber\":" << pPlayer->GetNumber() << ",";

		const CCapability& cap = pPlayer->GetCapability();

		cout << "\"kickingAbility\":" << cap.kickingAbility_ << ",";
		cout << "\"runningAbility\":" << cap.runningAbility_ << ",";
		cout << "\"ballControlAbility\":" << cap.ballControlAbility_ << ",";
		cout << "\"tacklingAbility\":" << cap.tacklingAbility_ << "";
		
		cout << "}";
	}

	cout << "]}" << endl;

}
void CGame::PrintKickOffResponse()
{
	//Need for flipping co-ordinates
	CPitch& pitch = GetGame().GetPitch();
	const CTeam::Ptr& ourTeamPtr = GetGame().GetOurTeamPtr();
		
	cout << "{\"requestType\":\"KICKOFF\", \"players\" : [";

	const CPlayer::PtrVec& ourTeamPlayers = ourTeamPtr_->GetPlayers();

	Position pos;
	float direction;
	int i = 0;
	for (auto& pPlayer : ourTeamPlayers)
	{
		if (0 != i++)
			cout << ",";

		pos = pPlayer->GetKickOffPosition();
		direction = pPlayer->GetKickOffDirection();

		cout << "{\"playerNumber\":" << pPlayer->GetNumber();
		
		//flip co-ordinates
		if (ourTeamPtr->GetPlayingDirection() == CTeam::eLeft)
		{
			pos.x_ = pitch.GetWidth() - pos.x_;
			direction = 360.0 - direction;
		}

		cout << ",\"position\":{\"x\":" << pos.x_
			<< ",\"y\":" << pos.y_;

		cout << "},\"direction\":" << direction;
		
		cout << "}";
	}
	cout << "]}" << endl;
}
void CGame::PrintPlayResponse()
{
	//Need for flipping co-ordinates
	CPitch& pitch = GetGame().GetPitch();
	const CTeam::Ptr& ourTeamPtr = GetGame().GetOurTeamPtr();

	cout << "{\"requestType\":\"PLAY\",\"actions\":[";
	const CPlayer::PtrVec& ourTeamPlayers = ourTeamPtr_->GetPlayers();

	Position destination;
	float	 direction = 0.0;
	int i = 0;
	for (auto& pPlayer : ourTeamPlayers)
	{
		CAction& action = pPlayer->GetAction();

		if (action.type_ == CAction::eNoAction)
			continue;

		if (0 != i++)
			cout << ",";

		destination = action.destination_;
		direction = action.direction_;

		//flip co-ordinates
		if (ourTeamPtr->GetPlayingDirection() == CTeam::eLeft)
		{
			destination.x_ = pitch.GetWidth() - destination.x_;
			direction = 360.0 - direction;
		}

		cout << "{\"playerNumber\":" << pPlayer->GetNumber();
		cout << ",\"action\":" << action.GetActionString();

		switch (action.type_)
		{
		case CAction::eKick:
		case CAction::eMove:
					
			cout << ",\"destination\":{ \"x\":" << destination.x_
				<< ",\"y\":" << destination.y_ << "}";

			cout << ",\"speed\":" << action.speed_;
			break;
		case CAction::eTurn:
			cout << ",\"direction\":" << direction; 
			break;
		}

		cout << "}";
	}
	cout << "]}" << endl;
}

void CGame::CalculateAllPlayerToBallSortedDistance()
{
	//ball is inside goal area.
	ourClosestPlayer_ = nullptr;
	
	if (pitch_.IsInsideTheirGoalArea(ball_.GetPosition()))
	{
		closestPlayer_ = theirTeamPtr_->GetGoalKeeper();
		return;
	}
	else if (pitch_.IsInsideOurGoalArea(ball_.GetPosition()))
	{
		closestPlayer_ = ourTeamPtr_->GetGoalKeeper();
		ourClosestPlayer_ = ourTeamPtr_->GetGoalKeeper();
		return;
	}
	
	auto& ourNonGoalKeeper = ourTeamPtr_->GetNonGoalKeepers();

	double minTimeTaken = 9999999.0;	//dummy max number
	double ourMinTimeTaken = 9999999.0;	//dummy max number
	double t = 0.0;

	//all non goal keeper (from both teams)
	for (auto& teamPtr : teams_)
	{
		auto& nonGoalKeepers = teamPtr->GetNonGoalKeepers();
		for (auto& pPlayer : nonGoalKeepers)
		{
			t = pPlayer->CalculateTimeToReachPosition(ball_.GetStationaryPosition());

			if (minTimeTaken > t)
			{
				minTimeTaken = t;
				closestPlayer_ = pPlayer;
			}
			
			if (ourMinTimeTaken > t && ourTeamPtr_->GetTeamNumber() == pPlayer->GetTeamNumber())
			{
				ourMinTimeTaken = t;
				ourClosestPlayer_ = pPlayer;
			}
		}
	}

	return;
}

//sort their team with respect to x cordinate (ascending order)
void CGame::SortTheirTeamX()
{
	//copy their non-goalkeeper players into a vector if empty
	if (theirTeamSortedX_.empty())
	{
		for(auto& pPlayer : theirTeamPtr_->GetNonGoalKeepers())
		{
			if (pPlayer->GetCapability().runningAbility_ >= 10.0)
			{
				theirTeamSortedX_.push_back(pPlayer);
			}
		}
		
		//theirTeamSortedX_ = theirTeamPtr_->GetNonGoalKeepers();
	}

	//sort the vector.
	sort(begin(theirTeamSortedX_), end(theirTeamSortedX_),
		[](const CPlayer::Ptr & a, const CPlayer::Ptr & b) -> bool
	{
		return a->GetPosition().x_ < b->GetPosition().x_;
	});
}

