using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BorderlessForm;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Forms;
using CustomButton;
using WindowsFormsApplication1;
using System.Threading;
using System.Runtime.InteropServices;

namespace profileForm
{
    // 프로파일 전역
    // 밑에 프로파일, 채팅룸, 채팅품


    class ProfileForm : borderlessForm
    {


        static Queue<string> QueDataSendPend;
        string myId;
        TcpClient connection;
        NetworkStream stream;
        Panel Pn_tab;
        SButton bn_profile;
        SButton bn_chat;
        Control bn_createBTN;


        public ProfileForm()
        {
            this.SuspendLayout();
            QueDataSendPend = new Queue<string>();
            // 친구목록 받아오기,
            // 오프라인 채팅 받아오기
            Pn_tab = new Panel();
            this.Pn_tab.Anchor = (System.Windows.Forms.AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
            this.Pn_tab.Location = new Point(0, 60);
            this.Pn_tab.Name = "panelTab";
            this.Pn_tab.Size = new Size(this.Size.Width, this.Size.Height - 60);
            this.Pn_tab.BackColor = Color.White;
            bn_profile = new SButton("resource\\navi_btn_friend.txt", 1, 55);
            bn_chat = new SButton("resource\\board_navi_btn_chat.txt", 46, 55);
            bn_chat.setforChatButton();
            bn_profile.setforChatButton();
            bn_profile.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ChatMouseBtnClicked);
            bn_chat.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ChatMouseBtnClicked);
            bn_createBTN = new Control();
            
            
            //버튼 만들기 // 친구목록
            // gui
            // 프로필사진 // 이름 //
            // 더블클릭시 채팅방 생성 1:1 이미 생성된채팅방이면 그 챙팅방내용으로 자식 윈도우 생성

            // 채팅방 버튼 
            // gui
            // 채팅방사진(프로필)// 마지막받은메세지 // 확인했나 안했나

            //채팅방 채팅로그
            // db 채팅방 id 만들기 -> key
            // 채팅 로그
            // 채팅방 유지하는 아이디숫자
            // 받을사람들 목록

            // 채팅 폼
            // 보더리스-> 이 폼의 차일드
            // 맨위에 프로필사진, 옆에 이름, 아래 팬 하나 넣어서 채팅방, 옆에 바 만들고, 아래에 채팅텍스트 박스, 옆에 전송버튼

            // 버튼 테스트
            // 버튼 레이블 쓰지말고 그냥 텍스트 그리는 클래스 하나 구현해야함.
            // this.Size = new Size(300, 500);
            //lstCprofile.Add(new ProFileEX(new Size(300,50), new Point(0, 0), "정으니",this));

            this.FormClosing += new FormClosingEventHandler(closing);
            this.Controls.Add(Pn_tab);
            this.Controls.Add(bn_profile);
            this.Controls.Add(bn_chat);
            this.ResumeLayout(false);
            BtnChatMemManager.getInstance().dicFormList.Add("profileForm", this);
        }


        public void AddButton(string key)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { AddButton(key); });
            }
            else
            {
                int cnt = BtnChatMemManager.getInstance().dicChatList.Count;
                BtnChatMemManager.getInstance().dicChatList.Add(key, new ChatLstButton(new Size(this.Width, 50), new Point(0, 50 * cnt), key, this));
            }
        }



        private void closing(object sender, FormClosingEventArgs e)
        {

            e.Cancel = false;
            this.stream.Close();
            this.connection.Close();
        }

       

        private void ChatMouseBtnClicked(object sender, EventArgs arg)//탭버튼
        {
            SButton btn = (SButton)sender;
            if (btn == bn_chat)
            {
                bn_profile.forChatAction();
                Pn_tab.Controls.Clear();
                foreach (KeyValuePair<string, Control> entry in BtnChatMemManager.getInstance().dicChatList)
                {
                    if (Pn_tab.InvokeRequired)
                    {
                        Pn_tab.Invoke((MethodInvoker)delegate { Pn_tab.Controls.Add(entry.Value); });
                    }
                    else
                    {
                        Pn_tab.Controls.Add(entry.Value);
                    }
                }
            }
            else
            {
                bn_chat.forChatAction();
                Pn_tab.Controls.Clear();
                foreach (KeyValuePair<string, Control> entry in BtnChatMemManager.getInstance().dicProfileList)
                {
                    Pn_tab.Controls.Add(entry.Value);
                }
            }
        }


        public void initData(string id, TcpClient tcp, string friendlist)
        {
            // 이것이 call되는 시점은 생성자call된 이후 시점.
            myId = id;
            connection = tcp;
            stream = tcp.GetStream();
            int idx = 0;
            int cnt = 0;
            while (true)
            {
                idx = friendlist.IndexOf(' ');
                if (idx == -1)
                {
                    break;
                }
                string name = friendlist.Substring(0, idx);
                friendlist = friendlist.Substring(idx);
                friendlist = friendlist.TrimStart(' ');
                BtnChatMemManager.getInstance().dicProfileList.Add(name, new ProFileEX(new Size(300, 50), new Point(0, 50 * cnt + 1), name, this));
                // this.Size = new Size(300, 500);
                //lstCprofile.Add(new ProFileEX(new Size(300,50), new Point(0, 0), "정으니",this));
                cnt++;
            }


            Thread recv = new Thread(new ThreadStart(takeMsgFromServ));
            Thread send = new Thread(new ThreadStart(sendMsgToServ));
            recv.Start();
            send.Start();
            //test

        }

        // mustbebackground// 백그라운드로 오는 메세지를 받아뿌려주는 함수
        void takeMsgFromServ()
        {
            MessageReciever MsgRvcer = new MessageReciever(4096);
            MsgRvcer.getMsgFromStream(this.stream);
        }

        // background // 센드 블락킹으로 인한 수신 이펙트 막는거 방지를 위함.
        void sendMsgToServ()
        {
            while (true)
            {
                if (QueDataSendPend.Count != 0)
                {
                    string msg = QueDataSendPend.Dequeue();
                    ByteField sendMsg = new ByteField(256);
                    sendMsg.ConcStrAfterHead(msg);
                    sendMsg.setHeadFromField(300);

                    stream.Write(sendMsg.m_field, 0, (int)sendMsg.getheader().msgsize);
                }
            }
        }

        // 

        public static void pushQue(string msg)
        {
            QueDataSendPend.Enqueue(msg);
        }

        public class MessageReciever
        {

            public byte[] m_field;
            //public uint m_index;
            int m_front;
            int m_rear;
            // m_size는 서큘러 큐의 실제 주소값을 계산하기 위함임..
            public int m_size;
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
                Marshal.Copy(m_field, m_rear + 1, ptr, m_headsize);
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


            void getData(NetworkStream stream)
            {

                if (m_emptySpace < 512)
                {
                    byte[] expand = new byte[m_size * 2];
                    for (int i = 0; i < m_size; i++)
                    {
                        expand[i] = m_field[i];
                    }
                    m_field = expand;
                    m_size = m_size * 2;
                }
                else
                {
                    int transbyte;
                    int cnt = this.getdatalength();

                    while (cnt < 8)
                    {
                        transbyte = stream.Read(m_field, m_front + 1, this.getfieldmaxaddress());
                        cnt += transbyte;
                        addData(transbyte);
                    }
                    TcpHeader head = this.getheader();
                    while (head.msgsize < cnt)
                    {
                        transbyte = stream.Read(m_field, m_front + 1, this.getfieldmaxaddress());
                        cnt += transbyte;
                        addData(transbyte);
                    }
                }
            }

            void processData()
            {

                while (getdatalength() > 8) // 메세지를 2개이상 q받았을시도 처리 
                {
                    TcpHeader head = this.getheader();
                    if (getdatalength() < head.msgsize)
                    {
                        break;
                    }
                    string strmsg = Encoding.Unicode.GetString(m_field, m_rear + 1 + m_headsize, (int)head.msgsize - m_headsize);
                    subData((int)head.msgsize);

                    string[] tok = strmsg.Split(' ');
                    string Id = tok[0];
                    string msg = tok[1];

                    if (BtnChatMemManager.getInstance().dicProfileList.ContainsKey(Id))
                    {

                        ChatLstButton btn = ((ChatLstButton)BtnChatMemManager.getInstance().dicChatList[Id]);
                        if (btn.InvokeRequired)
                            btn.Invoke(btn.myDelegate, msg);
                        else
                            btn.getMsg(msg);
                    }
                    else
                    {
                        ProfileForm profile = (ProfileForm)BtnChatMemManager.getInstance().dicFormList["profileForm"];
                        if (profile.InvokeRequired)
                        {
                            profile.Invoke((MethodInvoker)delegate { profile.AddButton(Id); });
                        }
                    }
                }

            }


            int getdatalength()
            {
                return (m_size - 1) - m_emptySpace;
            }

            int getfieldmaxaddress()
            {
                int retval = 0;
                if (m_front > m_rear || m_front != m_size - 1 || m_front == m_rear)
                {
                    retval = m_size - (m_front + 1);
                    // m_front == m_size -1 일경우는 어차피 입력에서 m_front+1이 되니까 상관없음. else 구문에서 처리 가능.
                }
                else
                {
                    retval = m_rear - 1;
                }
                return retval;
            }

            // 메세지 수신
            // 버그!!버퍼가 터질수있는 위험성이 있음.
            public void getMsgFromStream(NetworkStream stream)
            {

                getData(stream);
                processData();

                // 1. 메세지 처리시 끝일수도있음 -> gethead시 에러
                // 2. 메세지 처리후 헤더를 끝까지 못 받았을수도 있음 -> get head시 에러

            }

        }
    }



}

