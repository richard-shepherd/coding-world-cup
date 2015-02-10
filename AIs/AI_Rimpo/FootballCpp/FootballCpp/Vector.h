#pragma once

struct Position;

struct Vector
{
	float x_;
	float y_;

	Vector()
	{
		x_ = y_ = 0.0;
	}

	Vector(float x, float y)
	{
		x_ = x;
		y_ = y;
	}

	Vector Scale(float factor)
	{
		float length = sqrtf(x_*x_ + y_*y_);
		Vector res;
		res.x_ = x_*factor/length;
		res.y_ = y_*factor/length;
		return res;
	}

	Vector(const Position& from, const Position& to);
};

