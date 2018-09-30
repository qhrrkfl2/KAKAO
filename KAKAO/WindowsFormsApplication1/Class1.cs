using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class ByteField
    {
        public byte[] m_field;
        public ByteField(uint szByteLength)
        {
            m_field = new byte[szByteLength];
        }

        public byte[] getheader(uint headerLen)
        {
            byte[] head = new byte[headerLen];
            Array.Copy(m_field, head, head.Length);
            cutout((uint)head.Length);
            return head;
        }

        void cutout(uint len)
        {
            Array.Copy(m_field, len, m_field, 0, m_field.Length - len);
        }

    }

    //내가 필요한기능
    // 바이트를 인자로 받아 일정 부분을 잘라내고 나머지 바이트를 앞으로 당기는 함수(앞에 헤더를 자른다)
    // 바이트의 일정 부분만을 복사하는 함수.
    // 스트링처럼 바이트를 차곡차곡 쌓아갈수 있는 기능
}
