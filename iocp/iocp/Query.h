#pragma once
//statement핸들이 쿼리 결과를 저장하는데
// 그 쿼리 결과 자체가 누수와 관련된건지 test해야함
// 그런데 재사용 해도 된다는 검색결과가 이씀
// statement를 재컴파일 해야한다는것, 성능이슈와 관련된것인데 별로 상관없다고함
// 추론적으로 누수현상은 걱정거리가 아니다라는거,


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

