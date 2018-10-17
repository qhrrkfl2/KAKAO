#pragma once

#include <WinSock2.h>
#include <iostream>
#include <map>
using namespace std;
#define A 54059 /* a prime */
#define B 76963 /* another prime */
#define C 86969 /* yet another prime */
#define FIRSTH 37 /* also prime */

struct Bucket {
	SOCKET sock;
	wstring key;
	map<wstring, SOCKET> openaddress;
};

class myHash
{

	Bucket* m_bucket;
	unsigned int m_size;
	
	unsigned int hashing(wstring key);
public:
	myHash(unsigned int size);
	void put(wstring key, SOCKET data);
	SOCKET get(wstring key);
	void DelElement(wstring key);
	~myHash();
};

