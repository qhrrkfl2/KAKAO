// iocp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#pragma comment(lib, "ws2_32.lib")
#pragma once
#include <WinSock2.h>
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <Windows.h>
#include <process.h>


#include "Query.h"

#define BUFFERSIZE 256


using namespace std;
typedef struct SockInfo {
	SOCKET hCltSock;
	SOCKADDR clnAdr;
}PER_HANDLE_DATA, *LPPER_HANDLE_DATA;

typedef struct IoInfo {
	OVERLAPPED overlapped;
	WSABUF wsaBuf;
	char buffer[BUFFERSIZE];
	int rwMode;
	int cur;
}PER_IO_DATA, *LPPER_IO_DATA;

struct TcpHeader
{
	unsigned int msgsize;
	unsigned int mode;
};


unsigned WINAPI CompeleteThread(LPVOID completionportIO);
void makeDefaultIOInfo(int mode, IoInfo **IoinfoOut);
Query* db;

int main()
{


	// 디버그 for wchar
	std::wcout.imbue(std::locale("kor")); // 이것을 추가하면 된다.
	std::wcin.imbue(std::locale("kor")); // cin은 이것을 추가
	db = new Query;

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
		_beginthreadex(NULL, 0, CompeleteThread, (LPVOID)hComPort, 0, NULL);
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

		// 포인터 캐스팅한뒤 역참조하면 포인터 데이터 타입 크기만큼 읽기 시작함.
		// 위험하지만 유용함.

		// 잊지말자 캐스팅은 포인터 캐스팅과 마찬가지임.
		// 어쩃든 포인터는 4바이트니까(64비트에서는 8비트 캐스팅) DWORD로 캐스팅해서 넘긴다음에 함수안에서 다시 캐스팅해서 쓰는것
		CreateIoCompletionPort((HANDLE)hclntSOCK, hComPort, (DWORD)sockInfo, 0);

		ioInfo = new IoInfo;
		memset(&(ioInfo->overlapped), 0, sizeof(OVERLAPPED));
		memset(ioInfo->buffer, 0, sizeof(ioInfo->buffer));
		ioInfo->wsaBuf.len = sizeof(ioInfo->buffer);
		ioInfo->wsaBuf.buf = ioInfo->buffer;
		ioInfo->rwMode = 0;
		ioInfo->cur = 0;

		WSARecv(sockInfo->hCltSock, &(ioInfo->wsaBuf), 1, (LPDWORD)&readbyte, (LPDWORD)&flags, &(ioInfo->overlapped), NULL);


	}





	// cleanup network stuff
	closesocket(hSockLis);
	WSACleanup();
	delete db;

	return 0;
}

unsigned WINAPI CompeleteThread(LPVOID completionportIO)
{
	HANDLE hComport = (HANDLE)completionportIO;
	SOCKET sock;
	DWORD bytesTrans;
	LPPER_HANDLE_DATA sockInfo;
	LPPER_IO_DATA ioInfo;
	DWORD flag = 0;
	int readbyte = 0;
	while (1)
	{
		GetQueuedCompletionStatus(hComport, &bytesTrans, (LPDWORD)&sockInfo, (LPOVERLAPPED*)&ioInfo, INFINITE);
		sock = sockInfo->hCltSock;
		if (ioInfo->rwMode == 0)// read completion
		{
			cout << "message received" << endl;
			cout << bytesTrans << endl;
			if (bytesTrans == 0 || bytesTrans == -1)
			{
				closesocket(sock);
				delete sockInfo;
				delete ioInfo;
				continue;
			}
			else if (bytesTrans + ioInfo->cur <= 8)// not tested 버퍼를 2번 나누어 받을때 제대로 받는지 검증
			{

				cout << "적게받음" << endl;
				// 소켓 정보는 cp핸들에서 오는거임
				// io정보는 wsarecv콜에서 넘긴 포인터 캐스팅하여 얻는거임.
				ioInfo->wsaBuf.buf = &(ioInfo->buffer[bytesTrans]);
				ioInfo->wsaBuf.len = ioInfo->wsaBuf.len - bytesTrans;
				ioInfo->cur += bytesTrans;
				WSARecv(sockInfo->hCltSock, &(ioInfo->wsaBuf), 1, (LPDWORD)&readbyte, (LPDWORD)&flag, &(ioInfo->overlapped), NULL);
				continue;
			}


			char* head = ioInfo->buffer;
			TcpHeader *tcphead = (TcpHeader*)head;
			int size = tcphead->msgsize;
			if (bytesTrans + ioInfo->cur < size)
			{
				cout << "헤더받고 난뒤 적게받음" << endl;
				int idx = bytesTrans + ioInfo->cur;
				ioInfo->wsaBuf.buf = &(ioInfo->buffer[idx]);
				ioInfo->wsaBuf.len = ioInfo->wsaBuf.len - bytesTrans;
				ioInfo->cur += bytesTrans;

				if (WSARecv(sockInfo->hCltSock, &(ioInfo->wsaBuf), 1, (LPDWORD)&size, (LPDWORD)&flag, &(ioInfo->overlapped), NULL));
				continue;
			}


			//=== 수신 보장끝 =======================================================================================================================//

			if (tcphead->mode == 1) // 회원가입요청
			{
				wchar_t* buff = (wchar_t*)&ioInfo->buffer[8];
				wchar_t ID[16];
				wchar_t pass[16];
				memset(ID, 0, sizeof(ID));
				memset(pass, 0, sizeof(pass));
				int idx = 0;
				for (int i = 0; i < BUFFERSIZE; i++)
				{
					if (buff[i] != L' ')
						ID[i] = buff[i];
					else
					{
						idx = i + 1;
						break;
					}
				}
				int k = 0;
				while (buff[idx] != L'\0')
				{
					pass[k++] = buff[idx++];
				}

				// db 요청 회원가입
				if (db->JoinProcess(ID, pass))
				{
					//성공T
					TcpHeader JoinSuccess;
					JoinSuccess.mode = 100;
					JoinSuccess.msgsize = 18;
					wchar_t msg[] = L"가입성공!";
					wchar_t head[4];// 8바이트;
					memcpy(head, &JoinSuccess, sizeof(TcpHeader));
					wchar_t temp[256];
					memset(temp, 0, sizeof(temp));
					for (int i = 0; i < 4; i++)
					{
						temp[i] = head[i];
					}
					for (int i = 0; i < 5; i++)
					{
						temp[i + 4] = msg[i];
					}



					IoInfo* forwritecomp = new IoInfo;
					memset(forwritecomp, 0, sizeof(IoInfo));

					memcpy(forwritecomp->buffer, temp, sizeof(msg) + sizeof(head));

					forwritecomp->rwMode = 1;
					forwritecomp->wsaBuf.buf = forwritecomp->buffer;
					forwritecomp->wsaBuf.len = JoinSuccess.msgsize;

					WSASend(sockInfo->hCltSock, &(forwritecomp->wsaBuf), 1, 0, 0, &(forwritecomp->overlapped), NULL);
					continue;
				}
				else
				{
					TcpHeader joinFail;
					joinFail.mode = 101;
					joinFail.msgsize = 18;
					wchar_t msg[] = L"가입실패!";
					wchar_t head[4];// 8바이트;
					memcpy(head, &joinFail, sizeof(TcpHeader));
					wchar_t temp[256];
					memset(temp, 0, sizeof(temp));
					for (int i = 0; i < 4; i++)
					{
						temp[i] = head[i];
					}
					for (int i = 0; i < 5; i++)
					{
						temp[i + 4] = msg[i];
					}



					IoInfo* forwritecomp = new IoInfo;
					memset(forwritecomp, 0, sizeof(IoInfo));

					memcpy(forwritecomp->buffer, temp, sizeof(msg) + sizeof(head));

					forwritecomp->rwMode = 1;
					forwritecomp->wsaBuf.buf = forwritecomp->buffer;
					forwritecomp->wsaBuf.len = joinFail.msgsize;

					WSASend(sockInfo->hCltSock, &(forwritecomp->wsaBuf), 1, 0, 0, &(forwritecomp->overlapped), NULL);
					continue;
					//실패
				}
			}// end of join process

			if (tcphead->mode == 2)// login // 이미 메세지는 받았고 헤드를 분석하여 받을만큼의 길이를 확인하였다.
			{
				wchar_t* buff = (wchar_t*)&ioInfo->buffer[8];
				wchar_t ID[16];
				wchar_t pass[16];
				memset(ID, 0, sizeof(ID));
				memset(pass, 0, sizeof(pass));
				int idx = 0;
				for (int i = 0; i < BUFFERSIZE; i++)
				{
					if (buff[i] != L' ')
						ID[i] = buff[i];
					else
					{
						idx = i + 1;
						break;
					}
				}
				int k = 0;
				while (buff[idx] != L'\0')
				{
					pass[k++] = buff[idx++];
				}

				if (db->LoginProcess(ID, pass))
				{
					TcpHeader loginHead;
					loginHead.mode = 200;
					wstring sfriend =db->getFriendList(ID);
					loginHead.msgsize = 8 + sfriend.size()*2;

					IoInfo* io;
					makeDefaultIOInfo(1, &io);
					io->rwMode = 1;
					memcpy(io->buffer, &loginHead, sizeof(TcpHeader));
					memcpy(&io->buffer[8], sfriend.data(), sfriend.size()*2);
					io->wsaBuf.len = loginHead.msgsize;
					WSASend(sock, &(io->wsaBuf), 1, 0, 0, &(io->overlapped), NULL);
				} // 로그인성공
				else
				{
					TcpHeader loginHead;
					loginHead.mode = 201;
					loginHead.msgsize = 8;
					IoInfo* io;
					makeDefaultIOInfo(1, &io);
					memcpy(io->buffer, &loginHead, sizeof(TcpHeader));
					io->wsaBuf.len = 8;
					WSASend(sock, &(io->wsaBuf), 1, 0, 0, &(io->overlapped), NULL);
				} // 실패
				continue;
			}// end of login


			cout << "이상한 메세지를 받음" << endl;

		}
		else if (ioInfo->rwMode == 1) // writecompletion
		{
			cout << "message sent" << endl;
			delete ioInfo;
		}
		else
		{
			delete ioInfo;
			cout << "이상한 메세지를 받음" << endl;
		}

	}

	cout << "닿으면 안되는 부분" << endl;



	return 0;
}
// 100 성공
// 101 실패
// 200 로그인성공
// 201 로그인실패


void makeDefaultIOInfo(int mode, IoInfo **IoinfoOut)
{
	IoInfo* io = new IoInfo;
	memset(io, 0, sizeof(IoInfo));
	io->rwMode = mode;
	io->wsaBuf.buf = io->buffer;
	io->wsaBuf.len = BUFFERSIZE;
	*IoinfoOut = io;
}

void packMSG(wchar_t* head, wchar_t* msg, int msgsize, wchar_t* buff, int buffsize)
{

}