#include "Query.h"



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
	wstring sId = id;
	wstring sQuery = L"SELECT ID FROM Tb_Member WHERE ID = " + sId;
	wchar_t* query = (wchar_t*)sQuery.data();

	if (SQL_SUCCESS != SQLExecDirectW(SQLStatementHandle, (SQLWCHAR*)query, SQL_NTS));
	this->showSQLError(SQL_HANDLE_STMT, SQLStatementHandle);

	if (SQLFetch(SQLStatementHandle) == SQL_SUCCESS)
		return true;
	else
		return false;
}

bool Query::JoinProcess(wchar_t * id, wchar_t * pass)
{
	wstring sId = id;
	wstring sPass = pass;

	wstring sQuery = L"INSERT Tb_Member VALUES (" + sId + L"," + sPass + L", 0"+ L")";
	wchar_t* query = (wchar_t*)sQuery.data();

	if (SQL_SUCCESS != SQLExecDirectW(SQLStatementHandle, (SQLWCHAR*)query, SQL_NTS))
	{
		showSQLError(SQL_HANDLE_STMT, SQLStatementHandle);
		return false;
	}
	else
		return true;
}

bool Query::LoginProcess(wchar_t * id, wchar_t * pass)
{
	wstring sId = id;
	wstring sPass = pass;
	//SELECT * from Tb_Member where ID = '123' AND [password] = '123' ;
	wstring sQuery = L"SELECT * FROM Tb_Member WHERE ID = " + sId + L"AND" L"[password] = " + sPass;
	if (SQL_SUCCESS != SQLExecDirectW(SQLStatementLogin, (SQLWCHAR*)(wchar_t*)sQuery.data(), SQL_NTS))
	{
		showSQLError(SQL_HANDLE_STMT, SQLStatementLogin);
		return false;
	}
	else
	{
		if (SQLFetch(SQLStatementLogin) == SQL_SUCCESS)
		{
			sQuery.empty();
			sQuery = L"UPDATE Tb_Member SET [online] = 1";
			if (SQL_SUCCESS != SQLExecDirectW(SQLStatementLogin, (SQLWCHAR*)(wchar_t*)sQuery.data(), SQL_NTS))
			{
				showSQLError(SQL_HANDLE_STMT, SQLStatementLogin);
				return false;
			}
			else
			{
				return true;
			}
		}
	}

	return false;
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
	switch (SQLDriverConnect(SQLConnectionHandle, NULL, (SQLCHAR*)"DRIVER={SQL Server}; SERVER=221.153.193.89, 1433; DATABASE=Member; UID=vsUser; PWD=1234;", SQL_NTS, retConString, 1024, NULL, SQL_DRIVER_NOPROMPT)) {
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

	if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_STMT, SQLConnectionHandle, &SQLStatementHandle))
		print("alloc statementHandle FAIL");
	if (SQL_SUCCESS != SQLAllocHandle(SQL_HANDLE_STMT, SQLConnectionHandle, &SQLStatementLogin))
		print("alloc statementLogin FAIL");

}


Query::~Query()
{
	SQLFreeHandle(SQL_HANDLE_STMT, SQLStatementHandle);
	SQLFreeHandle(SQL_HANDLE_STMT, SQLStatementLogin);
	
	SQLDisconnect(SQLConnectionHandle);
	SQLFreeHandle(SQL_HANDLE_DBC, SQLConnectionHandle);
	SQLFreeHandle(SQL_HANDLE_ENV, SQLEnvHandle);

	// statement 누수  테스트  할것
}
