#include "myHash.h"

unsigned int myHash::hashing(wstring key)
{
	unsigned int h = FIRSTH;
	for (int i = 0; i < key.size(); i++)
	{
		h = (h*A) ^ (((unsigned int)key[i])*B);
	}
	return (h%C)%m_size;
}

myHash::myHash(unsigned int size)
{
	m_bucket = new Bucket[size];
	
	m_size = size;
}

void myHash::put(wstring key, SOCKET data)
{
	unsigned int index = hashing(key);
	Bucket* spot = &m_bucket[index];
	if (spot->key.empty())
	{
		spot->key = key;
		spot->sock = data;
	}
	else
	{
		pair<wstring, SOCKET> element;
		element.first = key;
		element.second = data;
		spot->openaddress.insert(element);
	}

}

SOCKET myHash::get(wstring key)
{
	unsigned int index = hashing(key);
	Bucket* spot = &m_bucket[index];
	if (spot->key == key)
	{
		return spot->sock;
	}

	
	auto it = spot->openaddress.find(key);
	// ��α����� ����
	if (it == spot->openaddress.end()) 
	{
		cout << "ERROR Ű�� ã���� ����" << endl;
		cout << "key = "; wcout << key.data() << endl;
		return NULL;
	}

	return it->second;

}


// ������ ������ iocp�����忡�� ���ֱ� ������ �ؽ����̺��� ����ִ°͸����� ����ϴ�.
void myHash::DelElement(wstring key)
{
	unsigned int index = hashing(key);
	Bucket* spot = &m_bucket[index];
	if (spot->key == key)
	{
		spot->key.clear();
	}
	else
	{
		spot->openaddress.erase(spot->openaddress.find(key));
	}
}

myHash::~myHash()
{

	delete[] m_bucket;
}
