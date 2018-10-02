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

#include <sqlext.h>
#include <sqltypes.h>
#include <sql.h>





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

struct TcpHeader
{
	unsigned int msgsize;
	unsigned int mode;
};

SQLHANDLE SQLEnvHandle = NULL;
SQLHANDLE SQLConnectionHandle = NULL;
SQLHANDLE SQLStatementHandle = NULL;
SQLRETURN retCode = 0;

unsigned WINAPI EchoThreadMain(LPVOID completionportIO);

void showSQLError(unsigned int handleType, const SQLHANDLE& handle)
{
	SQLCHAR SQLState[1024];
	SQLCHAR message[1024];
	if (SQL_SUCCESS == SQLGetDiagRec(handleType, handle, 1, SQLState, NULL, message, 1024, NULL))
		// Returns the current values of multiple fields of a diagnostic record that contains error, warning, and status information
		cout << "SQL driver message: " << message << "\nSQL state: " << SQLState << "." << endl;
}



int main()
{


	// 디버그 for wchar
	std::wcout.imbue(std::locale("kor")); // 이것을 추가하면 된다.
	std::wcin.imbue(std::locale("kor")); // cin은 이것을 추가
	
	if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_ENV, SQL_NULL_HANDLE, &SQLEnvHandle))
		// Allocates the environment
		return -1;

	if (SQL_SUCCESS != SQLSetEnvAttr(SQLEnvHandle, SQL_ATTR_ODBC_VERSION, (SQLPOINTER)SQL_OV_ODBC3, 0))
		// Sets attributes that govern aspects of environments
		return -1;

	if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_DBC, SQLEnvHandle, &SQLConnectionHandle))
		// Allocates the connection
		return -1;

	if (SQL_SUCCESS != SQLSetConnectAttr(SQLConnectionHandle, SQL_LOGIN_TIMEOUT, (SQLPOINTER)5, 0))
		// Sets attributes that govern aspects of connections
		return -1;

	SQLCHAR retConString[1024]; // Conection string
	switch (SQLDriverConnect(SQLConnectionHandle, NULL, (SQLCHAR*)"DRIVER={SQL Server}; SERVER=222.99.239.206, 1433; DATABASE=TestTableForConnect; UID=vsUser; PWD=1234;",
		SQL_NTS, retConString, 1024, NULL, SQL_DRIVER_NOPROMPT))
	{
		// Establishes connections to a driver and a data source
	case SQL_SUCCESS:
		break;
	case SQL_SUCCESS_WITH_INFO:
		break;
	case SQL_NO_DATA_FOUND:
		showSQLError(SQL_HANDLE_DBC, SQLConnectionHandle);
		retCode = -1;
		break;
	case SQL_INVALID_HANDLE:
		showSQLError(SQL_HANDLE_DBC, SQLConnectionHandle);
		retCode = -1;
		break;
	case SQL_ERROR:
		showSQLError(SQL_HANDLE_DBC, SQLConnectionHandle);
		retCode = -1;
		break;
	default:
		break;
	} //  init for ODBC

	if (retCode == -1)
		return -1;
//===================================================================================================================================================================


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
	for (int i = 0; (unsigned int)i < sysinfo.dwNumberOfProcessors; i++)
	{
		_beginthreadex(NULL, 0, EchoThreadMain, (LPVOID)hComPort, 0, NULL);
	}

	hSockLis = WSASocket(AF_INET, SOCK_STREAM, 0, NULL, 0, WSA_FLAG_OVERLAPPED);



	memset(&addr_Srv, 0, sizeof(addr_Srv));
	addr_Srv.sin_family = AF_INET;
	addr_Srv.sin_port = htons(atoi("7000"));
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





	// cleanup network stuff
	closesocket(hSockLis);
	WSACleanup();

	//cleanup ODBC stuff
	SQLFreeHandle(SQL_HANDLE_STMT, SQLStatementHandle);
	SQLDisconnect(SQLConnectionHandle);
	SQLFreeHandle(SQL_HANDLE_DBC, SQLConnectionHandle);
	SQLFreeHandle(SQL_HANDLE_ENV, SQLEnvHandle);

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
		if (ioInfo->rwMode == 0)// read completion
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
			WSASend(sock, &(ioInfo->wsaBuf), 1, 0, 0, &(ioInfo->overlapped), NULL);

			wchar_t buff[256];
			TcpHeader head;
			memcpy(&head, ioInfo->buffer, 8);
			memcpy(buff, &ioInfo->buffer[8], head.msgsize);
			wcout << buff << endl;


			ioInfo = new IoInfo;
			memset(ioInfo, 0, sizeof(IoInfo));
			ioInfo->wsaBuf.len = 256;
			ioInfo->wsaBuf.buf = ioInfo->buffer;
			ioInfo->rwMode = 0;
			WSARecv(sock, &(ioInfo->wsaBuf), 1, NULL, &flag, &(ioInfo->overlapped), NULL);


		}
		else // writecompletion
		{
			cout << "message sent" << endl;
			delete ioInfo;
		}

	}





	return 0;
}