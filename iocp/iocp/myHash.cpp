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
	// 비로그인자 색출
	if (it == spot->openaddress.end()) 
	{
		cout << "ERROR 키를 찾을수 없음" << endl;
		cout << "key = "; wcout << key.data() << endl;
		return NULL;
	}

	return it->second;

}


// 소켓의 정리는 iocp쓰레드에서 해주기 때문에 해시테이블을 비워주는것만으로 충분하다.
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
