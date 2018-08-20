// c++
#include "stdafx.h"
#include  <gcroot.h>
#include <stdio.h>
#include <string>
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
	 gcroot<SqlCommand^> m_Comm;
 public:
	DBCon(std::wstring constr) {
		System::String^ connstr = Marshal::PtrToStringUni((IntPtr)(void*)constr.c_str());
		try {
			m_conn = gcnew SqlConnection(connstr);
			m_conn->Open();
		}
		catch (int exception)		{
			// ��Ȳ�� �°� ����ó�� ��
		}
			m_Comm = gcnew SqlCommand;
			m_Comm->Connection = m_conn;
		

	}
		
	~DBCon() {
		if (m_conn != nullptr){
			m_conn->Close();
			delete m_conn;
		}
	}//�����ڹ� �ı���
//=========================================================================================================================================
 public:

	 

};

#pragma unmanaged


//Add the pragma preceding a function but not within a function body.
//Add the pragma after #include statements.Do not use these pragmas before #include statements.


int main()
{


    return 0;
}

