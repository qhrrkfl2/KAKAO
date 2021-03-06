﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//  for serialization
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Windows.Forms;   // Note: add reference required: System.Design.dll
using profileForm;
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
}


