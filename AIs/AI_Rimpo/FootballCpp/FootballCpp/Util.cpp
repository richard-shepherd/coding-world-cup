#include"stdafx.h"
#include"Util.h"

bool GetLineIntersection(const Position& p1, const Position& p2,	//line from p0 to p1 
	const Position& p3, const Position& p4,    //line from p3 to p4
	Position& intersection)
{
	float s1_x, s1_y, s2_x, s2_y;
	s1_x = p2.x_ - p1.x_;     s1_y = p2.y_ - p1.y_;
	s2_x = p4.x_ - p3.x_;     s2_y = p4.y_ - p3.y_;

	float s, t;
	s = (-s1_y * (p1.x_ - p3.x_) + s1_x * (p1.y_ - p3.y_)) / (-s2_x * s1_y + s1_x * s2_y);
	t = (s2_x * (p1.y_ - p3.y_) - s2_y * (p1.x_ - p3.x_)) / (-s2_x * s1_y + s1_x * s2_y);

	if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
	{
		// Collision detected
		intersection.x_ = p1.x_ + (t * s1_x);
		intersection.y_ = p1.y_ + (t * s1_y);
		return true;
	}

	return false; // No collision
}



//Formula for calulcating the perpendicualr instersecton from point p3 to the line (p1,p2)
//k = ((y2 - y1) * (x3 - x1) - (x2 - x1) * (y3 - y1)) / ((y2 - y1) ^ 2 + (x2 - x1) ^ 2)
//x4 = x3 - k * (y2 - y1)
//y4 = y3 + k * (x2 - x1)
bool GetPerpendicularIntersection(const Position& p1, const Position& p2,	//line from p1 to p2
	const Position& p3,						//point p3
	Position& intersection)	//perpendicular intersection point 
{
	float sx = p2.x_ - p1.x_;
	float sy = p2.y_ - p1.y_;

	float k = (sy * (p3.x_ - p1.x_) - sx * (p3.y_ - p1.y_)) / (sx*sx + sy*sy);
	intersection.x_ = p3.x_ - k * (p2.y_ - p1.y_);
	intersection.y_ = p3.y_ + k * (p2.x_ - p1.x_);


	//point is on the line
	if (p2.x_ > p1.x_ && intersection.x_ > p1.x_  && intersection.x_ < p2.x_)
	{
		return true;
	}
	if (p2.x_ < p1.x_ && intersection.x_ < p1.x_  && intersection.x_ > p2.x_)
	{
		return true;
	}
	//point is outside the line.
	return false;
}


Vector GetVectorFromDirection(float direction) 
{
	float angle = 90.0f - direction;
	float radians = angle * PI / 180.0f;
	float x = cosf(radians);
	float y = -1.0 * sinf(radians);
	return Vector(x, y);
}

bool ApproxEqual(float a, float b, float tolerance) {
	float difference = fabsf(a - b);
	return difference < tolerance;
}