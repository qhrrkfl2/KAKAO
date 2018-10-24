using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using WindowsFormsApplication1;
namespace sockTest
{
    class Program
    {

        public struct TcpHeader
        {
            public uint msgsize;
            public uint mode;
        };

        static void Main(string[] args)
        {

            TcpClient tc = new TcpClient("127.0.0.1", 7000);


            if (tc.Connected)
                System.Console.WriteLine("켜짐");

            NetworkStream st = tc.GetStream();
            ByteField file = new ByteField(256);
            file.ConcStrAfterHead("메세지를 보냄");
            file.setHeadFromField(3);

            st.Write(file.m_field, 0, (int)file.getheader().msgsize);
            System.Console.WriteLine("헤더 데이터 {0} {1}", file.getheader().mode, file.getheader().msgsize);
            st.Close();
            st.Dispose();
            tc.Close();
        }

        static void setHeader(TcpHeader head, Byte[] buff)
        {
            IntPtr ptr = Marshal.AllocHGlobal(8);
            Marshal.StructureToPtr(head, ptr, true);
            Marshal.Copy(ptr, buff, 0, 8);
            Marshal.FreeHGlobal(ptr);
        }
    }
}
