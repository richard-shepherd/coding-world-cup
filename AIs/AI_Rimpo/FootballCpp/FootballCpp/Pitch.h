#pragma once

#include "Position.h"

class CPitch
{
public:
	CPitch();
	~CPitch();

	inline float GetWidth() { return width_; }
	inline float GetHeight() { return height_; }
	inline const float& GetGoalCentre() { return goalCentre_; }
	inline float GetGoalY1() { return goalY1_; }
	inline float GetGoalY2() { return goalY2_; }
	inline const Position& GetCentreSpot() { return centreSpot_; }
	inline float GetCentreCircleRadius() { return centreCircleRadius_; }
	inline float GetGoalAreadRadius() { return goalAreaRadius_; }

	inline bool IsOurHalf(const Position& pos) { return pos.x_ < width_/2; }

	int ProcessPitch(const Value& pitchValue);

	inline const Position& GetOurGoalCentre(){ return ourGoalCentre_; }
	inline const Position& GetOurGoalY1() { return ourGoalY1_; }
	inline const Position& GetOurGoalY2() { return ourGoalY2_; }
	
	inline const Position& GetTheirGoalCentre(){ return theirGoalCentre_; }
	inline const Position& GetTheirGoalY1() { return theirGoalY1_; }
	inline const Position& GetTheirGoalY2() { return theirGoalY2_; }
	
	bool IsLineHittingOurGoal(const Position& beginPos, const Position& endPos, Position& hittingAt);
	bool IsInsideOurGoalArea(const Position& pos);

	bool IsLineHittingTheirGoal(const Position& beginPos, const Position& endPos);
	bool IsInsideTheirGoalArea(const Position& pos);
private:
	float width_;
	float height_;
	float goalCentre_;
	float goalY1_;
	float goalY2_;
	Position centreSpot_;
	float centreCircleRadius_;
	float goalAreaRadius_;

	Position theirGoalCentre_;
	Position theirGoalY1_;
	Position theirGoalY2_;

	Position ourGoalCentre_;
	Position ourGoalY1_;
	Position ourGoalY2_;
};

