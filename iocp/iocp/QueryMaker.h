#pragma once
#include <cstdarg>
class QueryMaker
{

public:
	void makeSelect(int args, ...);
	// select (coloumname) FROM table
	QueryMaker();
	~QueryMaker();
};

