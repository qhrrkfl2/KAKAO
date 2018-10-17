using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//  for serialization
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Sockets;
//==============================
using CustomButton;

namespace WindowsFormsApplication1
{
    // 메시지 사이즈는 헤더를 포함한 길이이다!
    public struct TcpHeader
    {
        public uint msgsize;
        public uint mode;
    };

    public class ByteField
    {
        public byte[] m_field;
        //public uint m_index;
        int m_index;
        public readonly int m_size;
        public int m_headsize;

        public int getIndex() { return m_index; }

        public ByteField(int szByteLength)
        {
            m_field = new byte[szByteLength];
            m_size = szByteLength;
            m_index = 0;
            // 헤더사이즈는 내가 설정하는것도 좋으나 사이즈만큼은 미리 정해놓는것이 코딩하기 편함.
            m_headsize = Marshal.SizeOf(typeof(TcpHeader));
        }
        //헤더필드 바이트를 형변환후 리턴 // test완료
        public TcpHeader getheader()
        {
            IntPtr ptr = Marshal.AllocHGlobal(m_headsize);
            Marshal.Copy(m_field, 0, ptr, m_headsize);
            TcpHeader head = (TcpHeader)Marshal.PtrToStructure(ptr, typeof(TcpHeader));
            Marshal.FreeHGlobal(ptr);
            return head;

        }

        // 헤더 필드를 뒤집어 씀. // test완료
        public void setHeader(TcpHeader head)
        {
            IntPtr ptr = Marshal.AllocHGlobal(m_headsize);
            Marshal.StructureToPtr(head, ptr, true);
            Marshal.Copy(ptr, m_field, 0, m_headsize);
            Marshal.FreeHGlobal(ptr);
        }

        public void copy(byte[] str)
        {
            m_index = str.Length;
            m_field = str;
        }

        // 채워진 필드의 뒤로부터 바이트를 합침.
        public void fieldConcatenate(byte[] add)
        {
            int len = add.Length;
            if (m_index + len >= m_size)
                throw new System.InvalidOperationException("out of field range");
            else
            {
                Array.Copy(add, 0, m_field, m_index, add.Length);
                m_index += add.Length;
            }

        }

        // test 완료
        public void fieldConcatenate(string add)
        {
            byte[] bMsg = Encoding.Unicode.GetBytes(add);
            int len = bMsg.Length;
            if (m_index + len >= m_size)
                throw new System.InvalidOperationException("out of field range");
            else
            {
                Array.Copy(bMsg, 0, m_field, m_index, len);
                m_index += len;
            }

        }

        // 헤더 뒤로 데이터를 뒤집어 씁니다. // test완료
        public void ConcStrAfterHead(string msg)
        {
            if (m_headsize == 0)
                throw new System.InvalidOperationException("set header first");

            byte[] bMsg = Encoding.Unicode.GetBytes(msg);
            if (m_headsize + bMsg.Length > m_size)
                throw new System.InvalidOperationException("out of field range");
            Array.Copy(bMsg, 0, m_field, (int)m_headsize, (int)bMsg.Length);
            m_index = bMsg.Length + m_headsize;

        }

        // 헤더 뒤로 데이터 뒤집어씀.
        public void ConcStrAfterHead(byte[] bMsg)
        {
            if (m_headsize == 0)
                throw new System.InvalidOperationException("set header first");

            if (m_headsize + bMsg.Length > m_size)
                throw new System.InvalidOperationException("out of field range");
            Array.Copy(bMsg, 0, m_field, (int)m_headsize, (int)bMsg.Length);
            m_index = bMsg.Length + m_headsize;
        }

        // 현제 이 어플리케이션에서 헤드는 정해져 있으므로 
        // 문자열만 넣고 헤드를 그후에 만들기위함임.
        public void setHeadFromField(uint mode)
        {
            TcpHeader head;
            head.mode = mode;
            head.msgsize = (uint)m_index;
            this.setHeader(head);
        }

        public void setHeadLenByIndex()
        {
            TcpHeader head = new TcpHeader();
            head.mode = this.getheader().mode;
            head.msgsize = (uint)this.m_index;
            this.setHeader(head);
        }
        public string getMsgStr()
        {

            if (m_index == 0)
            {
                m_index = (int)this.getheader().msgsize;
            }
            string strmsg = Encoding.Unicode.GetString(m_field, (int)m_headsize, (int)(m_index - m_headsize));
            return strmsg;
        }
    }


    public class MessageReciever
    {

        public byte[] m_field;
        //public uint m_index;
        int m_front;
        int m_rear;
        public readonly int m_size;
        int m_emptySpace;
        public int m_headsize;


        public MessageReciever(int size)
        {
            m_field = new byte[size];
            m_size = size;
            m_headsize = Marshal.SizeOf(typeof(TcpHeader));
            m_front = 0;
            m_rear = 0;
            m_emptySpace = m_size - 1;
        }

        public TcpHeader getheader()
        {
            IntPtr ptr = Marshal.AllocHGlobal(m_headsize);
            Marshal.Copy(m_field, m_rear, ptr, m_headsize);
            TcpHeader head = (TcpHeader)Marshal.PtrToStructure(ptr, typeof(TcpHeader));
            Marshal.FreeHGlobal(ptr);
            return head;

        }


        void addData(int num)
        {
            if ((m_front + 1) % m_size != m_rear && m_emptySpace >= num)
            {
                m_front += num;
                m_front = m_front % m_size;
                m_emptySpace -= num;
            }
        }
        // 남은 싸이즈
        // MAXSIZE-(|H - T|+1)
        // 사용 싸이즈
        // (|h-t|+1)
        void subData(int num)
        {
            if (m_front != m_rear && (m_emptySpace + num) <= (m_size - 1))
            {
                m_rear += num;
                m_rear = m_rear % m_size;
                m_emptySpace += num;
            }
        }

        void sendMsgFromField(TcpHeader head)
        {
            int sz = (int)head.msgsize;

            string strmsg = Encoding.Unicode.GetString(m_field, m_rear + 8, (sz - 8));
            subData(sz);
            string[] tok = strmsg.Split(' ');
            string ID = tok[0];
            string msg = tok[1];
            if (BtnChatMemManager.getInstance().dicChatList.ContainsKey(ID))
            {
                ((ChatLstButton)BtnChatMemManager.getInstance().dicChatList[ID]).getMsg(msg);
            }
        }

        int getdatalength()
        {
            return Math.Abs(m_rear - m_front);
        }

        // 메세지 수신
        public void getMsgFromStream(NetworkStream stream)
        {

            int cnt = stream.Read(m_field, m_front + 1, m_emptySpace);
            while (true)
            {
                addData(cnt);
                if (getdatalength() < 8)
                {
                    cnt = stream.Read(m_field, m_front + 1, m_emptySpace);
                    addData(cnt);
                }
                else
                    break;
            }

            TcpHeader head = this.getheader();

            while (true)
            {
                if (getdatalength() < head.msgsize)
                {
                    cnt = stream.Read(m_field, m_front + 1, m_emptySpace);
                    addData(cnt);
                }
                else
                    break;
            }

            while (head.msgsize <= this.getdatalength()) // 메세지를 2개이상 q받았을시도 처리
            {
                string strmsg = Encoding.Unicode.GetString(m_field, m_rear+1 + m_headsize, (int)head.msgsize-m_headsize);
                subData((int)head.msgsize);
                head = this.getheader();

                string[] tok = strmsg.Split(' ');
                string Id = tok[0];
                string msg = tok[1];

                if(BtnChatMemManager.getInstance().dicChatList.ContainsKey(Id))
                {
                    ((ChatLstButton)BtnChatMemManager.getInstance().dicChatList[Id]).getMsg(msg);
                }
                else
                {
                    // 모르는 사람이 메세지 전달하면? ㅋ
                }


            }
        }

    }
}


//내가 필요한기능

// utf-8 인코딩이 영어,한글이랑 바이트수가 다르다고 판단


// 필요한가?
// clean기능.(메세지필드 지워버리고 헤더를 그에 맞게 수정)

