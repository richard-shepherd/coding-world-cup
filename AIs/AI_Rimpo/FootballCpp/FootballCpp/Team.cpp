#include "stdafx.h"
#include "Team.h"


CTeam::CTeam()
{
	teamName_ = "";
	maxTeamNumber_ = -1; // dummy small number
	minTeamNumber_ = 9999; //dummy huge number

	firstTime_ = true;  //this will turn after first time to false.
}


CTeam::~CTeam()
{
}

int CTeam::ProcessTeamInfo(const Document& document)
{
	teamNumber_ = document["teamNumber"].GetInt();

	teamIdentifier_ = "team" + std::to_string(teamNumber_);	// e.g. team1 or team2

	CGame& game = GetGame();

	const Value& players = document["players"];

	for (auto itr = players.Begin(); itr != players.End(); ++itr)
	{
		CPlayer::Ptr pPlayer = game.GetPlayer((*itr)["playerNumber"].GetInt());

		if ((*itr)["playerType"] == "G")
		{
			pPlayer->SetType(CPlayer::eGoalKeeper);

			goalKeeperPtr_ = pPlayer;
		}
		else
		{
			pPlayer->SetType(CPlayer::eNonGoalKeeper);

			nonGoalKeepers_.push_back(pPlayer);
		}

		pPlayer->SetTeamNumber(teamNumber_);

		players_.push_back(pPlayer);

		if (maxTeamNumber_ < pPlayer->GetNumber())
			maxTeamNumber_ = pPlayer->GetNumber();

		if (minTeamNumber_ > pPlayer->GetNumber())
			minTeamNumber_ = pPlayer->GetNumber();
		
		firstTime_ = false;
	}

	return 0;
}
int CTeam::ProcessTeamKickOffInfo(const Document& document)
{
	const Value& teamKickOffInfoValue = document[GetTeamIdentifier().c_str()];

	score_ = teamKickOffInfoValue["score"].GetInt();
	teamName_ = teamKickOffInfoValue["name"].GetString();

	if (teamKickOffInfoValue["direction"] == "RIGHT")
	{
		directionType_ = eRight;
	}
	else
	{
		directionType_ = eLeft;
	}


	return 0;
}

int CTeam::ProcessStartOfTurn(const Document& document)
{
	//Note: Some values are commented because seems the info is redundant

	const Value& teamValue = document[GetTeamIdentifier().c_str()]["team"];

	//teamName_ = teamValue["name"].GetInt();
	score_ = teamValue["score"].GetInt();

	if (teamValue["direction"] == "RIGHT")
	{
		directionType_ = eRight;
	}
	else
	{
		directionType_ = eLeft;
	}

	//updating player info
	const Value& playersValue = document[GetTeamIdentifier().c_str()]["players"];

	if (!playersValue.IsArray())
	{
		// not an array
		return -1;
	}

	CGame &game = GetGame();


	//update player type and player details
	if (firstTime_)
	{
		for (SizeType i = 0; i < playersValue.Size(); i++)
		{
			const Value& playerStaticState = playersValue[i]["staticState"];

			int playerNumber = playerStaticState["playerNumber"].GetInt();

			CPlayer::Ptr pPlayer = game.GetPlayer(playerNumber);

			if (playerStaticState["playerType"] == "G")
			{
				pPlayer->SetType(CPlayer::eGoalKeeper);

				goalKeeperPtr_ = pPlayer;
			}
			else
			{
				pPlayer->SetType(CPlayer::eNonGoalKeeper);

				nonGoalKeepers_.push_back(pPlayer);
			}

			pPlayer->SetTeamNumber(teamNumber_);

			players_.push_back(pPlayer);

			if (maxTeamNumber_ < pPlayer->GetNumber())
				maxTeamNumber_ = pPlayer->GetNumber();

			if (minTeamNumber_ > pPlayer->GetNumber())
				minTeamNumber_ = pPlayer->GetNumber();
		}
		firstTime_ = false;
	}

	for (SizeType i = 0; i < playersValue.Size(); i++)
	{
		const Value& playerStaticState = playersValue[i]["staticState"];

		int playerNumber = playerStaticState["playerNumber"].GetInt();

		CPlayer::Ptr pPlayer = game.GetPlayer(playerNumber);

		pPlayer->ProcessStaticState(playerStaticState);
		pPlayer->ProcessDynamicState(playersValue[i]["dynamicState"]);
	}

	return 0;
}

int CTeam::ProcessCapabilitiesRequest(const Document& capValue)
{
	totalCapability_.kickingAbility_ = capValue["totalKickingAbility"].GetDouble();
	totalCapability_.runningAbility_ = capValue["totalRunningAbility"].GetDouble();
	totalCapability_.ballControlAbility_ = capValue["totalBallControlAbility"].GetDouble();
	totalCapability_.tacklingAbility_ = capValue["totalTacklingAbility"].GetDouble();

	return 0;
}



void CTeam::MapPlayerTypeToPlayerPtr()
{
	//this enables us to fetch CPLayer::Ptr from player type (i.e. eLeftDefender, eRightDefender ..)
	playerTypeToPlayer_.resize(players_.size());
	for (auto& pPlayer : players_)
	{
		playerTypeToPlayer_[pPlayer->GetType()] = pPlayer;
	}
}