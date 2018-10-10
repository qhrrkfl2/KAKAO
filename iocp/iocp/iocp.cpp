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


	// ����� for wchar
	std::wcout.imbue(std::locale("kor")); // �̰��� �߰��ϸ� �ȴ�.
	std::wcin.imbue(std::locale("kor")); // cin�� �̰��� �߰�
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
	// ����Ʈ ���� ���� �ſ��� ��-��

	// name // namelen ==> ����Ʈ �� // ������ ũ�������� �䱸�ϴ°��� 

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

		// ������ ĳ�����ѵ� �������ϸ� ������ ������ Ÿ�� ũ�⸸ŭ �б� ������.
		// ���������� ������.

		// �������� ĳ������ ������ ĳ���ð� ����������.
		// ��� �����ʹ� 4����Ʈ�ϱ�(64��Ʈ������ 8��Ʈ ĳ����) DWORD�� ĳ�����ؼ� �ѱ������ �Լ��ȿ��� �ٽ� ĳ�����ؼ� ���°�
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
			else if (bytesTrans + ioInfo->cur <= 8)// not tested ���۸� 2�� ������ ������ ����� �޴��� ����
			{

				cout << "���Թ���" << endl;
				// ���� ������ cp�ڵ鿡�� ���°���
				// io������ wsarecv�ݿ��� �ѱ� ������ ĳ�����Ͽ� ��°���.
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
				cout << "����ް� ���� ���Թ���" << endl;
				int idx = bytesTrans + ioInfo->cur;
				ioInfo->wsaBuf.buf = &(ioInfo->buffer[idx]);
				ioInfo->wsaBuf.len = ioInfo->wsaBuf.len - bytesTrans;
				ioInfo->cur += bytesTrans;

				if (WSARecv(sockInfo->hCltSock, &(ioInfo->wsaBuf), 1, (LPDWORD)&size, (LPDWORD)&flag, &(ioInfo->overlapped), NULL));
				continue;
			}


			//=== ���� ���峡 =======================================================================================================================//

			if (tcphead->mode == 1) // ȸ�����Կ�û
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

				// db ��û ȸ������
				if (db->JoinProcess(ID, pass))
				{
					//����T
					TcpHeader JoinSuccess;
					JoinSuccess.mode = 100;
					JoinSuccess.msgsize = 18;
					wchar_t msg[] = L"���Լ���!";
					wchar_t head[4];// 8����Ʈ;
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
					wchar_t msg[] = L"���Խ���!";
					wchar_t head[4];// 8����Ʈ;
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
					//����
				}
			}// end of join process

			if (tcphead->mode == 2)// login // �̹� �޼����� �޾Ұ� ��带 �м��Ͽ� ������ŭ�� ���̸� Ȯ���Ͽ���.
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
				} // �α��μ���
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
				} // ����
				continue;
			}// end of login


			cout << "�̻��� �޼����� ����" << endl;

		}
		else if (ioInfo->rwMode == 1) // writecompletion
		{
			cout << "message sent" << endl;
			delete ioInfo;
		}
		else
		{
			delete ioInfo;
			cout << "�̻��� �޼����� ����" << endl;
		}

	}

	cout << "������ �ȵǴ� �κ�" << endl;



	return 0;
}
// 100 ����
// 101 ����
// 200 �α��μ���
// 201 �α��ν���


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