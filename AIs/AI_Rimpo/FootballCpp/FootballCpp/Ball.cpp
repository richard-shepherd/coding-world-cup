#include "stdafx.h"
#include "Ball.h"


CBall::CBall()
{
}


CBall::~CBall()
{
}

int CBall::ProcessStartOfTurn(const Value& ballInfo)
{

	pos_.x_ = ballInfo["position"]["x"].GetDouble();
	pos_.y_ = ballInfo["position"]["y"].GetDouble();

	vector_.x_ = ballInfo["vector"]["x"].GetDouble();
	vector_.y_ = ballInfo["vector"]["y"].GetDouble();

	speed_ = ballInfo["speed"].GetDouble();

	controllingPlayerNumber_ = ballInfo["controllingPlayerNumber"].GetInt();

	//Flip co-ordinates
	CPitch& pitch = GetGame().GetPitch();
	const CTeam::Ptr& ourTeamPtr = GetGame().GetOurTeamPtr();

	if (ourTeamPtr->GetPlayingDirection() == CTeam::eLeft)
	{
		pos_.x_ = pitch.GetWidth() - pos_.x_;
		
		//mirror (only x)
		vector_.x_ = (-1)*vector_.x_;

	}

	return 0;
}
void CBall::CalculateStationaryPos(float& timeTaken)
{
	if (speed_ == 0)
	{
		timeTaken = 0.0;
		stationaryPos_ = pos_;
		virtualStationaryPos_ = pos_;
	}
	else
	{
		CPitch& pitch = GetGame().GetPitch();

		float averageSpeed = (speed_ + 0) / 2.0;	// 0 final velocity

		timeTaken = speed_ / FRICTION;

		float distance = timeTaken * averageSpeed;

		Vector disVector = vector_.Scale(distance);

		virtualStationaryPos_ = pos_;

		virtualStationaryPos_.AddVector(disVector);
		
		//out of bound co-ordinates are put back in
		stationaryPos_ = virtualStationaryPos_.GetRealPosition();	
	}
		
}


bool CBall::IsHittingOurGoal(const Position& targetPos)
{
	auto& pitch = GetGame().GetPitch();
	
	
	Position intersection, ballStationary;
	//float timeTaken = 0.0;

		
	return GetLineIntersection(pitch.GetOurGoalY1(), pitch.GetOurGoalY2(), pos_, targetPos, intersection);
}

void CBall::CorrectToCatchBall_Early(Position& dest,float& timetaken)
{
	float max_distance = dest.DistanceFrom(pos_);
	float v = speed_;
	float u = speed_;
	float d = 0.0f;
	float total_distance = 0.0;
	timetaken = 0.0;
	//float prev_total_distance = 0.0;
	for (;u > 0.0;)
	{
		v = u - FRICTION*0.1f;
		d = 0.1f*(v + u) / 2.0f;
		
		if (total_distance + d> max_distance)
		{
			break;
		}
		u = v;
		total_distance += d;
		timetaken += 0.1f;
		//prev_total_distance = total_distance;
		
	}
	
	Vector totalDistVector = vector_.Scale(total_distance);
	
	dest = pos_;
	dest.AddVector(totalDistVector);
}
void CBall::CorrectToCatchBall_Late(Position& dest, float& timetaken)
{
	float max_distance = dest.DistanceFrom(pos_);
	float v = speed_;
	float u = speed_;
	float d = 0.0f;
	float total_distance = 0.0;
	timetaken = 0.0;
	//float prev_total_distance = 0.0;
	for (; u > 0.0;)
	{
		v = u - FRICTION*0.1f;
		d = 0.1f*(v + u) / 2.0f;
		total_distance += d;
		timetaken += 0.1f;
		if (total_distance > max_distance)
		{
			break;
		}
		u = v;

		//prev_total_distance = total_distance;

	}

	Vector totalDistVector = vector_.Scale(total_distance);

	dest = pos_;
	dest.AddVector(totalDistVector);
}
void CBall::PositionTimeTaken(const Position& dest, vector<Position>& posVec, vector<float>& timeTakenVec)
{
	float max_distance = dest.DistanceFrom(pos_);
	float v = speed_;
	float u = speed_;
	float d = 0.0f;
	float total_distance = 0.0;
	float timetaken = 0.0;
	//float prev_total_distance = 0.0;
	for (; u > 0.0;)
	{
		v = u - FRICTION*0.1f;
		d = 0.1f*(v + u) / 2.0f;
		total_distance += d;
		timetaken += 0.1f;

		if (total_distance > max_distance)
		{
			break;
		}

		u = v;

		Vector totalDistVector = vector_.Scale(total_distance);
		Position currentPos = pos_;
		currentPos.AddVector(totalDistVector);
		posVec.push_back(currentPos);
		timeTakenVec.push_back(timetaken);
		//prev_total_distance = total_distance;

	}
}

void CBall::EstimatePath()
{
	//clear old path
	pathVirtualPos_.clear();
	pathPos_.clear();
	pathPosTime_.clear();
	
	//calculate stationary pos of ball
	CalculateStationaryPos(stationaryTimeTaken_);

	float max_distance = GetVirtualStationaryPosition().DistanceFrom(pos_);
	float v = speed_;
	float u = speed_;
	float d = 0.0f;
	float total_distance = 0.0;
	float timetaken = 0.0;
	//float prev_total_distance = 0.0;
	for (; u > 0.0;)
	{
		v = u - FRICTION*0.1f;
		d = 0.1f*(v + u) / 2.0f;
		total_distance += d;
		timetaken += 0.1f;

		if (total_distance > max_distance)
		{
			break;
		}

		u = v;

		Vector totalDistVector = vector_.Scale(total_distance);
		Position currentPos = pos_;
		currentPos.AddVector(totalDistVector);
		
		pathVirtualPos_.push_back(currentPos);
		pathPos_.push_back(currentPos.GetRealPosition());
		pathPosTime_.push_back(timetaken);
	}
	
	/*LOGGER->Log("Virtual Pos");
	for (size_t i = 0; i < pathVirtualPos_.size(); ++i)
	{
		LOGGER->Log("%d pos(%f %f)",i, pathVirtualPos_[i].x_,pathVirtualPos_[i].y_);
	}
	LOGGER->Log("Real Pos");
	for (size_t i = 0; i < pathVirtualPos_.size(); ++i)
	{
		LOGGER->Log("%d pos(%f %f)",i, pathPos_[i].x_,pathPos_[i].y_);
	}
	*/
}

float CBall::GetSpeedForDistance(float distance)
{
	float v = sqrtf(2.0 * FRICTION * distance);
	float percentage =  (v * 100.0f )/MAX_BALL_SPEED; 
	
	return percentage;
}

bool CBall::IsOurTeamControlling()
{ 
	return GetGame().GetOurTeamPtr()->IsMember(GetOwner()); 
}
bool CBall::IsTheirTeamControlling()
{ 
	return GetGame().GetTheirTeamPtr()->IsMember(GetOwner()); 
}
bool CBall::IsOurGoalKeeperControlling()
{
	return GetGame().GetOurTeamPtr()->GetGoalKeeper()->GetNumber() == GetOwner();
}
bool CBall::IsTheirGoalKeeperControlling()
{ 
	return GetGame().GetTheirTeamPtr()->GetGoalKeeper()->GetNumber() == GetOwner();
}
