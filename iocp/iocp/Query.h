#pragma once
//statement�ڵ��� ���� ����� �����ϴµ�
// �� ���� ��� ��ü�� ������ ���õȰ��� test�ؾ���
// �׷��� ���� �ص� �ȴٴ� �˻������ �̾�
// statement�� �������� �ؾ��Ѵٴ°�, �����̽��� ���õȰ��ε� ���� ������ٰ���
// �߷������� ���������� �����Ÿ��� �ƴϴٶ�°�,


#include <iostream>
#include <Windows.h>
#include <sql.h>
#include <sqlext.h>
#include <sqltypes.h>

using namespace std;

class Query
{
	SQLHANDLE SQLEnvHandle;
	SQLHANDLE SQLConnectionHandle;
	SQLHANDLE SQLStatementHandle;
	SQLHANDLE SQLStatementLogin;
	SQLHANDLE SQLStatementGetFriend;
	SQLRETURN retCode;
	wchar_t* tablename;
	
public:
	void print(char* result);
	void showSQLError(unsigned int handleType, const SQLHANDLE& handle);

	bool checkId(wchar_t* id);
	bool JoinProcess(wchar_t* id, wchar_t* pass);
	bool LoginProcess(wchar_t* id, wchar_t* pass);
	wstring getFriendList(wchar_t* id);
	Query();
	~Query();
};

