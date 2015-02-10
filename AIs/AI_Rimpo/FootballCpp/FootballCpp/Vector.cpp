#include "stdafx.h"
#include "Vector.h"

Vector::Vector(const Position& from, const Position& to)
{
	x_ = to.x_ - from.x_;
	y_ = to.y_ - from.y_;
}