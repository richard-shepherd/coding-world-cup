#include "stdafx.h"
#include "Position.h"

float Position::DistanceFrom(const Position& pos) const
{
	float diffX = (x_ - pos.x_);
	float diffY = (y_ - pos.y_);

	return sqrtf(diffX*diffX + diffY*diffY);
}

void Position::AddVector(const Vector& vec)
{
	x_ = x_ + vec.x_;
	y_ = y_ + vec.y_;
}

float Position::AngleWith(const Position& pos) 
{
	float deltaY = y_ - pos.y_;
	float deltaX = x_ - pos.x_;
	int angle = atan2f(deltaY, deltaX) * 180.0 / PI;
	angle += 270;
	angle %= 360;
	return angle;
}

bool Position::ApproxEqual(const Position& pos, float tolerance) const
{
	if (fabsf(x_ - pos.x_) <= tolerance &&
		fabsf(y_ - pos.y_) <= tolerance)
	{
		return true;
	}
	return false;
}

//out of bound co-ordinates are put back in
Position Position::GetRealPosition()
{
	auto& pitch = GetGame().GetPitch();
	
	Position realPos =  *this;
		//correction for bounce case;
		if (realPos.x_ < 0)
		{
			realPos.x_ = -1.0 * realPos.x_;
		}
		else if (realPos.x_ > pitch.GetWidth())
		{
			realPos.x_ = pitch.GetWidth() - (realPos.x_ - pitch.GetWidth());
		}

		if (realPos.y_ < 0)
		{
			realPos.y_ = -1.0 * realPos.y_;
		}
		else if (realPos.y_ > pitch.GetHeight())
		{
			realPos.y_ = pitch.GetHeight() - (realPos.y_ - pitch.GetHeight());
		}
	return realPos;
}