	#pragma once

#include "Position.h"
#include "Action.h"

#include "PlayerState.h"

#define GUARD_RADIUS 4.5f
#define GUARD_LINE 2.5f

#define DIRECTION_TOLERANCE 0.1f


#define MAX_DISTANCE_MARK_TO_GAURD_PASS 15.0f
#define PERCANTAGE_DIST_FOR_GUARD_PASS 0.25f
#define STRIKER_NO_ONE_CLOSE 1.2f
#define DEFENDER_NO_ONE_CLOSE 2.0f
#define DEFENDER_SHORT_KICK_NO_ONE_CLOSE 11.0f

#define DEFENDER_SHOOTING_RANGE 20.0

#define MAX_GOALKEEPER_WAIT_TICKS 20



struct CCapability{
	float kickingAbility_;
	float runningAbility_;
	float ballControlAbility_;
	float tacklingAbility_;
};


class CPitch;
class CBall;

class CPlayer
{
public:
	typedef vector<CPlayer> Vec;
	typedef shared_ptr<CPlayer> Ptr;
	typedef vector<Ptr> PtrVec;

	enum ePlayerType{
		eNonGoalKeeper = -1,
		eGoalKeeper = 0,
		eLeftDefender,
		eCentreDefender,
		eRightDefender,
		eLeftStriker,
		eRightStriker
	};

	enum eMarkingPlayerType
	{
		eNotMarking = -1
	};


	CPlayer();
	virtual ~CPlayer();

	inline void SetNumber(int number) { playerNumber_ = number; }
	inline int  GetNumber() { return playerNumber_; }

	inline void SetType(int type) { playerType_ = type; }
	inline int  GetType() { return playerType_; }

	inline float GetKickOffDirection() { return kickOffDirection_; }
	inline Position& GetKickOffPosition() { return kickOffPos_; }

	inline float GetHomeDirection() { return homeDirection_; }
	inline Position& GetHomePosition() { return homePos_; }

	inline float GetAttackDirection() { return attackDirection_; }
	inline Position& GetAttackPosition() { return attackPos_; }

	inline void SetHomeDirection(float direction) { homeDirection_ = direction; }
	inline void SetHomePosition(const Position& pos) { homePos_ = pos; }

	inline void SetKickOffDirection(float direction) { kickOffDirection_ = direction; }
	inline void SetKickOffPosition(const Position& pos) { kickOffPos_ = pos; }

	inline void SetAttackDirection(float direction) { attackDirection_ = direction; }
	inline void SetAttackPosition(const Position& pos) { attackPos_ = pos; }

	
	void SetTeamNumber(int teamNumber) { teamNumber_ = teamNumber; }
	int GetTeamNumber() { return teamNumber_; }
	
	bool IsOurTeamMember();				//using team number to identify team.
	bool IsTheirTeamMember();

	inline CCapability& GetCapability() { return capability_; }
	inline CAction& GetAction() { return action_; }
	inline Position& GetPosition() { return pos_; }
	inline CPlayerState* GetState() { return pState_; }
	inline float GetDirection() { return direction_; }
	
	inline bool HasBall() { return hasBall_;}

	void ChangeState(int type);

	int ProcessStaticState(const Value& staticState);
	int ProcessDynamicState(const Value& dynamicState);

	// update action
	void MoveTo(const Position& destination);
	void TurnTo(float direction);
	void TakePossession();
	void Kick(const Position& destination, float speed);
	
	void MoveToSaveGoal_GoalKeeper(const Position& hittingAt);

	float CalculateTimeToTurn(float direction);
	float CalculateTimeToReachPosition(const Position& pos);

	float PredictDirection();

	void MoveToGuardGoal();						//called by goalkeeper
	void MoveToGuardGoal_Radius();
	void MoveToGuardGoal_LineSave();
	void MoveToGuardGoal_Centre();
	
	void MoveToMarkedPlayer_GuardPass();
	void MoveToMarkedPlayer_Mark();
	void MoveForBall();					// this should handle all condtion for ball interception,chase_ball
	
	void KickShort(float power);
	void KickShort_Striker();
	

	
	void Kick_Defender();
	void Kick_GoalKeeper();
	
	bool IsSupportingDefenderAlreadyMarking(int playerNumber);
	void SelectMarkedPlayer();
	
	inline void SetMarkedPlayerNumber(int playerNumber) { markedPlayerNumber_ = playerNumber; }
	inline int GetMarkedPlayerNumber() { return markedPlayerNumber_; }
	
	bool IsTheirPlayerNear(float distance);
	bool IsTheirPlayerNearFromFront(float max_distance);
	bool IsTheirPlayerBehindMe();
	
	inline void SetShootCache(const Position& shootAt) { shootAt_ = shootAt; isShootCached_ = true; }
	inline bool GetShootCache(Position& shootAt) { if (isShootCached_) { shootAt = shootAt_;} return isShootCached_; }
	inline void ResetShootCache() {isShootCached_ = false;}
	
	Position GetRandomShootAtGoal();
	
	//only for goalkeeper.
	inline void IncrementGoalKeeperWaitTicks() { goalKeeperWaitTicks_++; }
	inline int GetGoalKeeperWaitTicks() { return goalKeeperWaitTicks_;}
	inline void ResetGoalKeeperWaitTicks() { goalKeeperWaitTicks_ = 0;}
	
	inline  int GetKickingTowardPlayerNumber() { return kickingTowardPlayerNumber_;	}
	inline  void SetKickingTowardPlayerNumber(int playerNumber) { kickingTowardPlayerNumber_ = playerNumber;	}
	inline  void ResetKickingTowardPlayerNumber() { kickingTowardPlayerNumber_ = -1; }
	
	Position GetRandomFreePosition_Striker();
	
	bool IsKickDirectionSafe(float direction, float speed, float limitDistance);
	void GetSafeKickDirection_GoalKeeper(float& direction, float& speed, float limitDistance);
	
	//************Added after passer AI****************************
	inline const PtrVec& GetSupportPlayers() {return supportPlayers_;}
	inline void AddSupportPlayer(CPlayer::Ptr pPlayer) { supportPlayers_.push_back(pPlayer);}
	
	bool IsPassSafeTo(CPlayer::Ptr pPlayer);
	void Pass();
	
	void KickShortNoStateChange_Striker();
	
	bool IsInterceptionValid(const Position& perIntersection);
	
private:
	int playerType_;
	int playerNumber_;
	Position pos_;
	CCapability capability_;
	bool hasBall_;
	float direction_;

	int teamNumber_;

	Position kickOffPos_;
	float kickOffDirection_;

	Position homePos_;
	float homeDirection_;

	Position attackPos_;
	float attackDirection_;

	CAction action_;

	CPlayerState* pState_;

	float timeToReachBall_;

	CGame& game_;
	CPitch& pitch_;
	CBall& ball_;

	int markedPlayerNumber_;
	
	Position shootAt_;
	bool     isShootCached_;
	
	int goalKeeperWaitTicks_;
	int kickingTowardPlayerNumber_;
	
	PtrVec supportPlayers_;
};



