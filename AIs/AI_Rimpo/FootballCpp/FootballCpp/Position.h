#pragma once

#include<math.h>

#include"Vector.h"

#define POSITION_TOLERANCE 0.00001f
#define POSITION_GUARD_TOLERANCE 1.0f


#define POSITION_VERY_BIG_TOLERANCE 1.0f
#define POSITION_BIG_TOLERANCE 0.1f
#define POSITION_MEDIUM_TOLERANCE 0.01f
#define POSITION_SMALL_TOLERANCE 0.00001f


struct Position
{
	typedef vector<Position> Vec;
	float x_;
	float y_;

	Position()
	{
		x_ = 0.0;
		y_ = 0.0;
	}
	
	Position(float x, float y)
	{
		x_ = x;
		y_ = y;
	}

	float DistanceFrom(const Position& pos) const;
	float AngleWith(const Position& pos);
	
	void AddVector(const Vector& vec);

	Vector VectorTo(const Position& pos) { return Vector(pos.x_ - x_, pos.y_ - y_); }

	bool ApproxEqual(const Position& pos, float tolerance) const;

	inline bool IsNegative() { return x_ < 0.0 || y_ < 0.0; }
	
	Position GetRealPosition();	//out of bound co-ordinates are put back in (help in ball bouncing boundary)
	
};

