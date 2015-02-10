#pragma once

#include "Position.h"
#include "Vector.h"

bool GetLineIntersection(const Position& p1, const Position& p2,	//line from p0 to p1 
	const Position& p3, const Position& p4,    //line from p3 to p4
	Position& intersection);


#define RandomRangeFloat(LO, HI)  (LO + static_cast <float> (rand()) /( static_cast <float> (RAND_MAX/(HI-LO))))
#define RandomRangeInteger(LO, HI) (LO + (rand() % (int)(HI - LO + 1)))

#define IsWithinRange(Value, Low , High) ( Low < Value && Value < High )

//Formula for calulcating the perpendicualr instersecton from point p3 to the line (p1,p2)
//k = ((y2 - y1) * (x3 - x1) - (x2 - x1) * (y3 - y1)) / ((y2 - y1) ^ 2 + (x2 - x1) ^ 2)
//x4 = x3 - k * (y2 - y1)
//y4 = y3 + k * (x2 - x1)
bool GetPerpendicularIntersection(const Position& p1, const Position& p2,	//line from p1 to p2
	const Position& p3,						//point p3
	Position& intersection);	//perpendicular intersection point 

//Note: returns true if inside the line intersection else false


Vector GetVectorFromDirection(float direction);

bool ApproxEqual(float a, float b, float tolerance);

