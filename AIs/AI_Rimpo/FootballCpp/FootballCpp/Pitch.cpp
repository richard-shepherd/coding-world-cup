#include "stdafx.h"
#include "Pitch.h"


CPitch::CPitch()
{
}


CPitch::~CPitch()
{
}


int CPitch::ProcessPitch(const Value& pitchValue)
{
	if (!pitchValue.IsObject())
	{
		return -1;
	}

	width_ = pitchValue["width"].GetDouble();
	height_ = pitchValue["height"].GetDouble();
	goalCentre_ = pitchValue["goalCentre"].GetDouble();
	goalY1_ = pitchValue["goalY1"].GetDouble();
	goalY2_ = pitchValue["goalY2"].GetDouble();
	centreCircleRadius_ = pitchValue["centreCircleRadius"].GetDouble();
	goalAreaRadius_ = pitchValue["goalAreaRadius"].GetDouble();

	centreSpot_.x_ = pitchValue["centreSpot"]["x"].GetDouble();
	centreSpot_.y_ = pitchValue["centreSpot"]["y"].GetDouble();


	ourGoalCentre_.x_ = 0.0;
	ourGoalCentre_.y_ = centreSpot_.y_;


	// For Goalkeeper to protect bigger area than the Y1 and Y2 +/- 0.1 is done 
	ourGoalY1_.x_ = 0.0;
	ourGoalY1_.y_ = goalY1_ - 0.2;			

	ourGoalY2_.x_ = 0.0;
	ourGoalY2_.y_ = goalY2_ + 0.2;			

	theirGoalCentre_.x_ = 100.0;
	theirGoalCentre_.y_ = centreSpot_.y_;

	theirGoalY1_.x_ = 100.0;
	theirGoalY1_.y_ = goalY1_;

	theirGoalY2_.x_ = 100.0;
	theirGoalY2_.y_ = goalY2_;
	
	//LOGGER->Log("Our goal Y1 = (%f, %f) and Y2 = (%f, %f)", ourGoalY1_.x_, ourGoalY1_.y_, ourGoalY2_.x_, ourGoalY2_.y_ );

	return 0;
}
bool CPitch::IsLineHittingOurGoal(const Position& beginPos, const Position& endPos, Position& hittingAt)
{
	return GetLineIntersection(ourGoalY1_, ourGoalY2_, beginPos, endPos, hittingAt);
}

bool CPitch::IsInsideOurGoalArea(const Position& pos)
{
	//check x > 0 is to ensure the x is in the field.
	return pos.x_ > 0 && pos.DistanceFrom(ourGoalCentre_) < (goalAreaRadius_);// - 0.3);
}

bool CPitch::IsLineHittingTheirGoal(const Position& beginPos, const Position& endPos)
{
	Position intersection;
	return GetLineIntersection(theirGoalY1_, theirGoalY2_, beginPos, endPos, intersection);
}
bool CPitch::IsInsideTheirGoalArea(const Position& pos)
{
	//check x > 0 is to ensure the x is in the field.
	return pos.x_ > 0 && pos.DistanceFrom(theirGoalCentre_) < (goalAreaRadius_);//- 0.3);
}