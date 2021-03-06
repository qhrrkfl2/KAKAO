#include "Query.h"




//// Set login timeout to 5 seconds
//SQLSetConnectAttr(hdbc, SQL_LOGIN_TIMEOUT, (SQLPOINTER)5, 0);
//CHECK_ERROR(retcode, "SQLSetConnectAttr(SQL_LOGIN_TIMEOUT)",
//	hdbc, SQL_HANDLE_DBC);

void Query::print(char * result)
{
	cout << result << endl;
}


void Query::showSQLError(unsigned int handleType, const SQLHANDLE& handle)
{
	SQLCHAR SQLState[1024];
	SQLCHAR message[1024];
	if (SQL_SUCCESS == SQLGetDiagRec(handleType, handle, 1, SQLState, NULL, message, 1024, NULL))
		// Returns the current values of multiple fields of a diagnostic record that contains error, warning, and status information
		cout << "SQL driver message: " << message << "\nSQL state: " << SQLState << "." << endl;
}

// 채팅프로그램이 조인전에 아이디 확인을 강요.
bool Query::checkId(wchar_t * id)
{
	bool retcode = false;
	wstring sId = id;
	wstring sQuery = L"SELECT ID FROM Tb_Member WHERE ID = " + sId;
	wchar_t* query = (wchar_t*)sQuery.data();
	SQLAllocHandle(SQL_HANDLE_STMT,SQLConnectionHandle, &SQLStatementHandle);

	if (SQL_SUCCESS != SQLExecDirectW(SQLStatementHandle, (SQLWCHAR*)query, SQL_NTS));
	this->showSQLError(SQL_HANDLE_STMT, SQLStatementHandle);

	if (SQLFetch(SQLStatementHandle) == SQL_SUCCESS)
		retcode = true;
	else
		retcode = false;
	SQLCloseCursor(SQLStatementHandle);
	SQLFreeHandle(SQL_HANDLE_STMT, SQLStatementHandle);
	return retcode;
}

bool Query::JoinProcess(wchar_t * id, wchar_t * pass)
{
	wstring sId = id;
	wstring sPass = pass;
	bool retcode = false;
	wstring sQuery = L"INSERT Tb_Member VALUES (" + sId + L"," + sPass + L", 0"+ L")";
	wchar_t* query = (wchar_t*)sQuery.data();
	SQLAllocHandle(SQL_HANDLE_STMT, SQLConnectionHandle, &SQLStatementHandle);
	if (SQL_SUCCESS != SQLExecDirectW(SQLStatementHandle, (SQLWCHAR*)query, SQL_NTS))
	{
		showSQLError(SQL_HANDLE_STMT, SQLStatementHandle);
		retcode = false;
	}
	else
		retcode = true;
	
	SQLCloseCursor(SQLStatementHandle);
	SQLFreeHandle(SQL_HANDLE_STMT, SQLStatementHandle);
	return retcode;
}

bool Query::LoginProcess(wchar_t * id, wchar_t * pass)
{
	bool retcode = false;
	bool rstCode = false;
	if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_STMT, SQLConnectionHandle, &SQLStatementLogin))
	{
		print("로그인 스테이트먼트 핸들 얻기 실패");
	}
	wstring sId = id;
	wstring sPass = pass;
	sId = L"'" + sId + L"'";
	sPass = L"'" + sPass +L"'";
	//SELECT * from Tb_Member where ID = '123' AND [password] = '123' ;
	wstring sQuery = L"SELECT * FROM Tb_Member WHERE ID = " + sId + L"AND [password] = " + sPass;
	if (SQL_SUCCESS != SQLExecDirectW(SQLStatementLogin, (SQLWCHAR*)(wchar_t*)sQuery.data(), SQL_NTS))
	{
		showSQLError(SQL_HANDLE_STMT, SQLStatementLogin);
		retcode = false;
	}
	else
	{
		if (SQLFetch(SQLStatementLogin) == SQL_SUCCESS)
		{
			rstCode = true;
		}
		else
		{
			retcode = false;
		}

		SQLCloseCursor(SQLStatementLogin);
		SQLFreeHandle(SQL_HANDLE_STMT, SQLStatementLogin);

		if (rstCode) // 쿼리된 결과가 있다 -> 아이디 비번 맞음.
		{
			SQLHANDLE setOnline;
			SQLAllocHandle(SQL_HANDLE_STMT, SQLConnectionHandle, &setOnline);
			sQuery.empty();
			sQuery = L"UPDATE Tb_Member SET [online] = 1 where ID =" + sId;
			if (SQL_SUCCESS != SQLExecDirectW(SQLStatementLogin, (SQLWCHAR*)(wchar_t*)sQuery.data(), SQL_NTS))
			{
				showSQLError(SQL_HANDLE_STMT, SQLStatementLogin);
				retcode = false;
			}
			else
			{
				retcode = true;
			}

			SQLCloseCursor(setOnline);
			SQLFreeHandle(SQL_HANDLE_STMT, setOnline);
		}
	}
	
	return retcode;
}

bool Query::SendPendingMsg(wchar_t * id, wchar_t * msg)
{

	//exec StoreMSG @varID = 'qhrrkfl', @msg = '214124214421'
	//exec getPDMSG @varID = 'qhrrkfl'
	//https://docs.microsoft.com/en-us/sql/relational-databases/native-client-odbc-stored-procedures/calling-a-stored-procedure?view=sql-server-2017
	bool retcode = false;
	wstring strId = id;
	wstring strMsg = msg;
	SQLHANDLE SPMHandle;
	if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_STMT, SQLConnectionHandle, &SPMHandle));
	{
		this->showSQLError(SQL_HANDLE_STMT, SPMHandle);
	}
	SQLINTEGER SQLINTid = SQL_NTS;
	

	SQLPrepareW(SPMHandle, (SQLWCHAR*)L"{CALL StoreMSG(?, ?)}", SQL_NTS);
	SQLBindParameter(SPMHandle, 1, SQL_PARAM_INPUT, SQL_WCHAR, SQL_CHAR, 12, 0, (SQLWCHAR*)strId.data(), strId.length()*2, &SQLINTid);
	SQLBindParameter(SPMHandle, 2, SQL_PARAM_INPUT, SQL_WCHAR, SQL_CHAR, 256, 0, (SQLWCHAR*)strMsg.data(), strMsg.length()*2 , &SQLINTid);

	if (SQL_SUCCESS != SQLExecute(SPMHandle))
	{
		this->showSQLError(SQL_HANDLE_STMT, SPMHandle);
	}
	else
	{
		retcode = true;
	}
	SQLCloseCursor(SPMHandle);
	SQLFreeHandle(SQL_HANDLE_STMT, SPMHandle);
	return retcode;
}

void Query::getPendingMsg(wchar_t * id, vector<MSGData>* container)
{
	wstring strId = id;
	SQLHANDLE HGPMsg;
	SQLAllocHandle(SQL_HANDLE_STMT, SQLConnectionHandle, &HGPMsg);

	SQLPrepareW(HGPMsg, (SQLWCHAR*)L"{CALL GetMSGandDelete(?)}", SQL_NTS);
	SQLINTEGER SQLNTS = SQL_NTS;
	SQLBindParameter(HGPMsg, 1, SQL_PARAM_INPUT, SQL_WCHAR, SQL_CHAR, 12, 0, (SQLWCHAR*)strId.data(), strId.length() * 2, &SQLNTS);

	if (SQL_SUCCESS != SQLExecute(HGPMsg))
	{
		this->showSQLError(SQL_HANDLE_STMT,HGPMsg);
	}
	MSGData data;
	while (SQLFetch(HGPMsg)==SQL_SUCCESS)
	{
		SQLGetData(HGPMsg, 1, SQL_C_WCHAR, &data.id, sizeof(data.id), NULL);
		SQLGetData(HGPMsg, 3, SQL_C_WCHAR, &data.msg, sizeof(data.msg), NULL);
		container->push_back(data);
	}

	SQLCloseCursor(HGPMsg);
	SQLFreeHandle(SQL_HANDLE_STMT, HGPMsg);
}

wstring Query::getFriendList(wchar_t * id)
{


	wstring sFriend;
	wstring sId = id;
	sId = L"'" + sId + L"'";
	if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_STMT, SQLConnectionHandle, &SQLStatementGetFriend))
	{
		print("alloc getFriendhandle FAIL");
	}

	wstring sQuery = L"SELECT * FROM Friends WHERE ID = " + sId;
	if (SQL_SUCCESS != SQLExecDirectW(SQLStatementGetFriend, (SQLWCHAR*)(wchar_t*)sQuery.data(), SQL_NTS))
	{
		showSQLError(SQL_HANDLE_STMT, SQLStatementGetFriend);
	}
	else
	{
		wchar_t DFriend[12];
		while (SQLFetch(SQLStatementGetFriend) == SQL_SUCCESS)
		{
			SQLGetData(SQLStatementGetFriend, 2, SQL_C_WCHAR, &DFriend, sizeof(DFriend), NULL);
			wstring name = DFriend;
			sFriend += name;
		}

	}
	SQLCloseCursor(SQLStatementGetFriend);
	SQLFreeHandle(SQL_HANDLE_STMT, SQLStatementGetFriend);
	return sFriend;
}



Query::Query()
{

	if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_ENV, SQL_NULL_HANDLE, &SQLEnvHandle))
		// Allocates the environment
		this->print("allocSQLEnvHandle fail");

	if (SQL_SUCCESS != SQLSetEnvAttr(SQLEnvHandle, SQL_ATTR_ODBC_VERSION, (SQLPOINTER)SQL_OV_ODBC3, 0))
		// Sets attributes that govern aspects of environments
		print("setEnvAttr fail");

	if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_DBC, SQLEnvHandle, &SQLConnectionHandle))
		// Allocates the connection
		print("allocSQLEnvHandle fail");

	if (SQL_SUCCESS != SQLSetConnectAttr(SQLConnectionHandle, SQL_LOGIN_TIMEOUT, (SQLPOINTER)5, 0))
		// Sets attributes that govern aspects of connections
		print("set con attr fail");


	SQLCHAR retConString[1024]; // Conection string
	switch (SQLDriverConnect(SQLConnectionHandle, NULL, (SQLCHAR*)"DRIVER={SQL Server}; SERVER=127.0.0.1, 1433; DATABASE=Member; UID=vsUser; PWD=1234;", SQL_NTS, retConString, 1024, NULL, SQL_DRIVER_NOPROMPT)) {
		// Establishes connections to a driver and a data source
	case SQL_SUCCESS:
		break;
	case SQL_SUCCESS_WITH_INFO:
		showSQLError(SQL_HANDLE_DBC, SQLConnectionHandle);
		break;
	case SQL_NO_DATA_FOUND:
		showSQLError(SQL_HANDLE_DBC, SQLConnectionHandle);
		break;
	case SQL_INVALID_HANDLE:
		showSQLError(SQL_HANDLE_DBC, SQLConnectionHandle);
		break;
	case SQL_ERROR:
		showSQLError(SQL_HANDLE_DBC, SQLConnectionHandle);
		break;
	default:
		break;
	}
}


Query::~Query()
{
	SQLFreeHandle(SQL_HANDLE_STMT, SQLStatementHandle);
	SQLFreeHandle(SQL_HANDLE_STMT, SQLStatementLogin);
	SQLFreeHandle(SQL_HANDLE_DBC, SQLConnectionHandle);
	SQLFreeHandle(SQL_HANDLE_ENV, SQLEnvHandle);
	
	SQLDisconnect(SQLConnectionHandle);

	// statement 누수  테스트  할것
}
