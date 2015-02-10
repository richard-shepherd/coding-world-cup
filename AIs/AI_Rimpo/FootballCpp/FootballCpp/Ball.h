#pragma once

#include "Position.h"
#include "Vector.h"

#define MAX_BALL_SPEED 30.0f

typedef vector<float> TimeVec;

class CBall
{
public:
	CBall();
	~CBall();
	
	inline Position& GetPosition() { return pos_; }
	inline Vector& GetVector() { return vector_; }

	inline const Position& GetStationaryPosition() { return stationaryPos_; }
	inline const Position& GetVirtualStationaryPosition() { return virtualStationaryPos_; }

	inline int GetOwner() { return controllingPlayerNumber_; }
	inline float GetSpeed() { return speed_; }
	inline void SetSpeed(float speed) { speed_ = speed; }
	


	int ProcessStartOfTurn(const Value& ballInfo);

	void CalculateStationaryPos(float& timeTaken);

	bool IsHittingOurGoal(const Position& targetPos);

	void CorrectToCatchBall_Early(Position& dest, float& timetaken);
	void CorrectToCatchBall_Late(Position& dest, float& timetaken);

	void PositionTimeTaken(const Position& dest, vector<Position>& posVec, vector<float>& timeTakenVec);

	void EstimatePath();

	inline const Position::Vec& GetPathPos() { return pathPos_; }
	inline const Position::Vec& GetPathVirutalPos() { return pathVirtualPos_; }
	inline const TimeVec& GetPathPosTime() { return pathPosTime_; }

	inline bool IsFreeBall() { return (controllingPlayerNumber_ == -1); }
	bool IsOurTeamControlling();
	bool IsTheirTeamControlling();
	
	bool IsOurGoalKeeperControlling();
	bool IsTheirGoalKeeperControlling();
	
	float GetSpeedForDistance(float distance);
	
private:
	
	Position pos_;
	Vector   vector_;
	float	  speed_;
	int		  controllingPlayerNumber_;

	Position stationaryPos_;			// actual stationary position with negative values are normalized
	Position virtualStationaryPos_;		// unrealistic value i.e can be negative (used to check goal hit condition)
	float	 stationaryTimeTaken_;
	
	Position::Vec pathPos_;
	Position::Vec pathVirtualPos_;		// used only for goalkeeper (need to think a better way to avoid this)
	TimeVec	 pathPosTime_;
};

