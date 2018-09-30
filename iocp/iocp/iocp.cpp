// iocp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#pragma comment(lib, "ws2_32.lib")
#pragma once
#include <WinSock2.h>
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <Windows.h>
#include <process.h>
using namespace std;
typedef struct SockInfo {
	SOCKET hCltSock;
	SOCKADDR clnAdr;
}PER_HANDLE_DATA, *LPPER_HANDLE_DATA;

typedef struct IoInfo {
	OVERLAPPED overlapped;
	WSABUF wsaBuf;
	char buffer[256];
	int rwMode;
}PER_IO_DATA, *LPPER_IO_DATA;


unsigned WINAPI EchoThreadMain(LPVOID completionportIO);

int main()
{
	WSADATA wsaData;
	SOCKET hSockLis;
	SOCKADDR_IN addr_Srv;
	SockInfo *sockInfo;
	IoInfo *ioInfo;
	int readbyte, flags = 0;

	// for finding out the number of core
	SYSTEM_INFO sysinfo;

	//for IOCP kernel object
	HANDLE hComPort;


	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
	{
		cout << "startup fail" << endl;
		return 0;
	}

	hComPort = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, 0, 0);


	GetSystemInfo(&sysinfo);
	//for (int i = 0; (unsigned int)i < sysinfo.dwNumberOfProcessors; i++)
	//{
	//	_beginthreadex(NULL, 0, EchoThreadMain, (LPVOID)hComPort, 0, NULL);
	//}

	hSockLis = WSASocket(AF_INET, SOCK_STREAM, 0, NULL, 0, WSA_FLAG_OVERLAPPED);



	memset(&addr_Srv, 0, sizeof(addr_Srv));
	addr_Srv.sin_family = AF_INET;
	addr_Srv.sin_port = htons(atoi("5999"));
	addr_Srv.sin_addr.S_un.S_addr = htonl(INADDR_ANY);
	// htonl  == h to network long
	// htons == s == short
	// 바이트 수에 따른 거였음 잇-힝

	// name // namelen ==> 바이트 수 // 변수의 크기정보를 요구하는거임 

	bind(hSockLis, (sockaddr*)&addr_Srv, sizeof(addr_Srv));

	listen(hSockLis, 5);

	while (1)
	{
		SOCKET hclntSOCK;
		SOCKADDR_IN clntAdr;
		int addrLen = sizeof(clntAdr);
		hclntSOCK = accept(hSockLis, (SOCKADDR*)&clntAdr, &addrLen);

		sockInfo = new SockInfo;
		sockInfo->hCltSock = hclntSOCK;
		memcpy(&(sockInfo->clnAdr), &clntAdr, addrLen);

		// 잊지말자 캐스팅은 포인터 캐스팅과 마찬가지임.
		// 어쩃든 포인터는 4바이트니까(64비트에서는 8비트 캐스팅) DWORD로 캐스팅해서 넘긴다음에 함수안에서 다시 캐스팅해서 쓰는것
		CreateIoCompletionPort((HANDLE)hclntSOCK, hComPort, (DWORD)sockInfo, 0);

		ioInfo = new IoInfo;
		memset(&(ioInfo->overlapped), 0, sizeof(OVERLAPPED));
		ioInfo->wsaBuf.len = 256;
		ioInfo->wsaBuf.buf = ioInfo->buffer;
		ioInfo->rwMode = 0;
		
		WSARecv(sockInfo->hCltSock, &(ioInfo->wsaBuf), 1, (LPDWORD)&readbyte, (LPDWORD)&flags, &(ioInfo->overlapped), NULL);


	}






	closesocket(hSockLis);
	WSACleanup();





	return 0;
}

unsigned WINAPI EchoThreadMain(LPVOID completionportIO)
{
	HANDLE hComport = (HANDLE)completionportIO;
	SOCKET sock;
	DWORD bytesTrans;
	LPPER_HANDLE_DATA sockInfo;
	LPPER_IO_DATA ioInfo;
	DWORD flag = 0;


	while (1)
	{
		GetQueuedCompletionStatus(hComport, &bytesTrans, (LPDWORD)&sockInfo, (LPOVERLAPPED*)&ioInfo, INFINITE);
		sock = sockInfo->hCltSock;
		if (ioInfo->rwMode == 0)// read
		{
			cout << "message received" << endl;
			if (bytesTrans == 0 || bytesTrans == -1)
			{
				closesocket(sock);
				delete sockInfo;
				delete ioInfo;
				continue;
			}

			memset(&(ioInfo->overlapped), 0, sizeof(OVERLAPPED));
			ioInfo->wsaBuf.len = bytesTrans;
			ioInfo->rwMode = 1;
			WSASend(sock, &(ioInfo->wsaBuf), 1, 0, (DWORD)&flag, &(ioInfo->overlapped), NULL);

			ioInfo = new IoInfo;
			memset(ioInfo, 0, sizeof(IoInfo));
			ioInfo->wsaBuf.len = 256;
			ioInfo->wsaBuf.buf = ioInfo->buffer;
			ioInfo->rwMode = 0;
			WSARecv(sock, &(ioInfo->wsaBuf), 1, NULL, &flag, &(ioInfo->overlapped), NULL);


		}
		else
		{
			cout << "message sent" << endl;
			delete ioInfo;
		}

	}





	return 0;
}