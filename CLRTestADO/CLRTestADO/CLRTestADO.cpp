// c++
#include "stdafx.h"
#include  <gcroot.h>
#include <stdio.h>
#include <string>
#include <iostream>
using namespace std;

// .net
#using <System.dll>
#using <System.Data.dll>
using namespace System;
using namespace System::Data;
using namespace System::Data::SqlClient;
using namespace System::Runtime::InteropServices;



#pragma managed
 class DBCon {
 private:
	 gcroot<SqlConnection^> m_conn;
	 gcroot<SqlCommand^> m_Cmd;
 public:
	DBCon(std::string constr) {
		System::String^ connstr = Marshal::PtrToStringAnsi((IntPtr)(void*)constr.c_str());
		try {
			m_conn = gcnew SqlConnection(connstr);
			m_conn->Open();
		}
		catch (int exception)		{
			// 상황에 맞게 예외처리 ㄱ
			cout << "연결실패" << endl;
		}
		m_Cmd = gcnew SqlCommand;
		m_Cmd->Connection = m_conn;
	}
	~DBCon() {
		// gcroot는 특정 연산자에 대해서 오류남(포인터변수처럼 대하면안됨)
		// 그래서 캐스팅을 때려서 널체크 함.
		if (static_cast<SqlConnection^>(m_conn) != nullptr)	{
			m_conn->Close();
			
			delete m_conn;
		}
	}//생성자밑 파괴자
//=========================================================================================================================================
 public:
	 void testModule(std::string querryStr)	 {
		 String^ k = Marshal::PtrToStringAnsi((IntPtr)(void*)querryStr.c_str());
		 m_Cmd->CommandText = k;
		 SqlDataReader ^reader = m_Cmd->ExecuteReader();
		// do data extraction here


		//==================================
		 reader->Close();
	 }
	 
};

#pragma unmanaged


//Add the pragma preceding a function but not within a function body.
//Add the pragma after #include statements.Do not use these pragmas before #include statements.


int main()
{

	string con = "Data Source = 59.18.223.69; Initial Catalog = TestTableForConnect; User ID=vsUser;Password=1234;";

	DBCon *conn = new DBCon(con);
	
	
	
	
	conn->~DBCon();
    return 0;
}

