// FootballCpp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"


#include "Game.h"

bool Test_GetPerpendicularIntersection()
{
	Position p1{0.0, 0.0};
	Position p2{20.0, 0.0};
	Position p3{10.0,10.0};
	Position intersection;

	GetPerpendicularIntersection(p1, p2, p3, intersection);
	
	p3 = { 30.0, 10.0 };

	GetPerpendicularIntersection(p1, p2, p3, intersection);

	p2 = { 10.0, 10.0 };
	p3 = { 5.0, 0.0 };

	GetPerpendicularIntersection(p1, p2, p3, intersection);

	p3 = { 0.0, -5.0 };
	GetPerpendicularIntersection(p1, p2, p3, intersection);
	return true;
}


int main(int argc, char* argv[])
{
	srand (time(NULL));
	
	Test_GetPerpendicularIntersection();

	LOGGER->Log("START");
	
	CGame& game = GetGame();

	while (1)
	{
		string strInputJson;
		
		cin >> strInputJson;

		//LOGGER->Log("RECEIVED: %s",strInputJson.c_str());
		
		game.Process(strInputJson);
	}
	
	cout << "End of Game. Application Exit!!!" << endl;

	return 0;
}

