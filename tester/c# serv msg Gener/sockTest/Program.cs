using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using WindowsFormsApplication1;
namespace sockTest
{
    class Program
    {

        static void Main(string[] args)
        {

            

            TcpListener listen = new TcpListener(IPAddress.Any, 7000);
            listen.Start();
            ByteField f1 = new ByteField(256);
            TcpClient sock = listen.AcceptTcpClient();
            NetworkStream stream = sock.GetStream();


          //  if (sock.Connected)
          //  {
          //      stream.Read(f1.m_field, 0, 8);
          //      int len = (int)f1.getheader().msgsize;
          //      stream.Read(f1.m_field, 8, len);
          //      Console.WriteLine("헤더데이터 {0} {1}", f1.getheader().mode, f1.getheader().msgsize);
          //      Console.WriteLine(f1.getMsgStr());
          //      
          //
          //  }
            


            if(sock.Connected)
            {
                Console.WriteLine("정보받음");
                f1.ConcStrAfterHead("으베베");
                f1.setHeadFromField(200);
                stream.Write(f1.m_field, 0, 8);
            }

            //|head || SenderID" "MSG |
            while (true)
            {
                string line = Console.ReadLine();
                StringBuilder sb = new StringBuilder();
                sb.Append("tester");
                sb.Append(" ");
                sb.Append(line);
                line = sb.ToString();
                f1.ConcStrAfterHead(line);
                f1.setHeadFromField(300);
                stream.Write(f1.m_field, 0, (int)f1.getheader().msgsize);
            }

            // do thing you want.


        }


        static public void getmsgfromcli(object stream1)
        {

            NetworkStream stream = (NetworkStream)stream1;
            while (true)
            {
                Byte[] end = new Byte[4];

                int cnt;
                cnt = stream.Read(end, 0, 3);

                if (cnt == 0)
                {
                    return;
                }
            }
        }



        static void getMSG(int headsize, NetworkStream stream, Byte[] buff,int buffsize)
        {
            int cnt = 0;
            while (cnt < 8)
            {
                cnt += stream.Read(buff, cnt, buffsize - cnt);
            }
            TcpHeader head = getheader(buff,headsize);
            while(cnt >= head.msgsize)
            {
                cnt += stream.Read(buff, cnt, buffsize - cnt);
            }
        }

       static public TcpHeader getheader(byte[] buff, int headsize)
        {
            IntPtr ptr = Marshal.AllocHGlobal(headsize);
            Marshal.Copy(buff,0, ptr, headsize);
            TcpHeader head = (TcpHeader)Marshal.PtrToStructure(ptr, typeof(TcpHeader));
            Marshal.FreeHGlobal(ptr);
            return head;

        }
    }


    

    
}


