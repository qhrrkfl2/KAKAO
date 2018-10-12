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


namespace profileForm
{
    class ProfileForm : borderlessForm
    {
        string myId;
        TcpClient connection;
        NetworkStream stream;
        Panel Pn_tab;
        SButton bn_profile;
        SButton bn_chat;
        string getFriendList;
        List<Control> lstCprofile;
        List<Control> lstCChat;
        public ProfileForm()
        {
            this.SuspendLayout();
            // 친구목록 받아오기,
            // 오프라인 채팅 받아오기
            Pn_tab = new Panel();
            this.Pn_tab.Anchor = (System.Windows.Forms.AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
            this.Pn_tab.Location = new Point(0, 60);
            this.Pn_tab.Name = "panelTab";
            this.Pn_tab.Size = new Size(this.Size.Width, this.Size.Height - 60);
            this.Pn_tab.BackColor = Color.White;
            bn_profile = new SButton("resource\\navi_btn_friend.txt", 0, 55);
            bn_chat = new SButton("resource\\board_navi_btn_chat.txt", 45, 55);
            bn_chat.setforChatButton();
            bn_profile.setforChatButton();
            bn_profile.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ChatMouseBtnClicked);
            bn_chat.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ChatMouseBtnClicked);



            lstCChat = new List<Control>();
            lstCprofile = new List<Control>();
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
        }


        private void closing(object sender , FormClosingEventArgs e)
        {

            //If you call Close() on the client side, nothing is sent to the server to tell it that its closing,
            //it literally just closes it self so that the client can't use it any more. The only reliable way to
            //determine if you're still connected is to try to send data and handle the failure.
            //If you want you could implement your own handshake agreement where when you call Close() you send
            //a special notification to the server alerting it to the fact but there will still be times when that packet never reaches the server.
            e.Cancel = false;
            this.stream.Close();
            this.connection.Close();
        }


      

        private void ChatMouseBtnClicked(object sender, EventArgs arg)
        {
            SButton btn = (SButton)sender;
            if (btn == bn_chat)
            {
                bn_profile.forChatAction();
                Pn_tab.Controls.Clear();
                for (int i = 0; i < lstCChat.Count; i++)
                {
                    Pn_tab.Controls.Add(lstCChat[i]);
                }
            }
            else
            {
                bn_chat.forChatAction();
                Pn_tab.Controls.Clear();
                for (int i = 0; i < lstCprofile.Count; i++)
                {
                    Pn_tab.Controls.Add(lstCprofile[i]);
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
                if(idx == -1)
                {
                    break;
                }
                string name = friendlist.Substring(0,idx);
                friendlist = friendlist.Substring(idx);
                friendlist=friendlist.TrimStart(' ');
                lstCprofile.Add(new ProFileEX(new Size(300,50),new Point(0,50*cnt +1),name,this));

                // this.Size = new Size(300, 500);
                //lstCprofile.Add(new ProFileEX(new Size(300,50), new Point(0, 0), "정으니",this));
                cnt++;
            }


            //test

        }


    }
}
