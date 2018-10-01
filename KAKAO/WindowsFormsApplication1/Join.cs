using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Windows.Forms;
using BorderlessForm;
using System.Drawing;
// socket
using System.Net.Sockets;
using CustomButton;
using WindowsFormsApplication1;


namespace Formjoin
{
    class Join : borderlessForm
    {
        int bytesize = 256;
        int szHeader = 8;
        enum ESameCheck
        {
            ID,
            PW
        };

        bool[] samecheck = new bool[2];
        TextBox tb_NickName;
        TextBox tb_ID;
        TextBox tb_Password;
        TextBox tb_chkPassword;
        TextBox tb_Email;
        Button btn_Join;

        Button btn_SameIDCheck;

        Label lb_NickName;
        Label lb_ID;
        Label lb_Password;
        Label lb_Email;
        Label lb_ChkPassword;

        Label lb_war_ID;
        Label lb_war_password;

        string error;

        public Join()
        {
            ConnectLoginServer();
            SuspendLayout();
            tb_ID = new TextBox();
            tb_ID.Location = new Point(90, 80);
            tb_ID.Size = new Size(100, 20);
            tb_ID.TabIndex = 0;

            tb_Password = new TextBox();
            tb_Password.Location = new Point(90, 120);
            tb_Password.Size = new Size(100, 20);
            tb_Password.TabIndex = 1;
            tb_Password.KeyDown += new KeyEventHandler(CustomButton.HelperFunc.passwordKeyDown);


            tb_chkPassword = new TextBox();
            tb_chkPassword.Location = new Point(90, 160);
            tb_chkPassword.Size = new Size(100, 20);
            tb_chkPassword.TabIndex = 2;
            tb_chkPassword.KeyDown += new KeyEventHandler(CustomButton.HelperFunc.passwordKeyDown);




            tb_Email = new TextBox();
            tb_Email.Location = new Point(90, 200);
            tb_Email.Size = new Size(100, 20);
            tb_Email.TabIndex = 3;

            tb_NickName = new TextBox();
            tb_NickName.Location = new Point(90, 240);
            tb_NickName.Size = new Size(100, 20);
            tb_NickName.TabIndex = 4;

            Size label = new Size(60, 15);
            lb_ID = new Label();
            lb_ID.Location = new Point(10, 85);
            lb_ID.Size = label;
            lb_ID.Text = "아이디";

            // ID 경고
            Size warningLabel = new Size(200, 15);
            lb_war_ID = new Label();
            Point p_warID = tb_ID.Location;
            p_warID.Y += 25;
            lb_war_ID.Location = p_warID;
            lb_war_ID.Size = warningLabel;
            lb_war_ID.Text = "영문숫자조합 최대 12자리";

            lb_Password = new Label();
            lb_Password.Location = new Point(10, 125);
            lb_Password.Size = label;
            lb_Password.Text = "비밀번호";

            // PW 경고
            lb_war_password = new Label();
            Point p_warPw = tb_Password.Location;
            p_warPw.Y += 25;
            lb_war_password.Location = p_warPw;
            lb_war_password.Size = warningLabel;
            lb_war_password.Text = "영문숫자조합 최대 12자리";

            lb_ChkPassword = new Label();
            lb_ChkPassword.Location = new Point(10, 165);
            lb_ChkPassword.Size = label;
            lb_ChkPassword.Text = "비번확인";

            lb_Email = new Label();
            lb_Email.Location = new Point(10, 205);
            lb_Email.Size = label;
            lb_Email.Text = "이메일";

            lb_NickName = new Label();
            lb_NickName.Location = new Point(10, 245);
            lb_NickName.Size = label;
            lb_NickName.Text = "닉네임";

            btn_Join = new Button();
            btn_Join.Text = "회원가입";
            btn_Join.Location = new Point(10, 430);
            btn_Join.Size = new Size(75, 25);
            btn_Join.Click += new EventHandler(btn_join_Clicked);

            Point btnLoc;
            // ID중복
            btn_SameIDCheck = new Button();
            btn_SameIDCheck.Text = "중복확인";
            btnLoc = tb_ID.Location;
            btnLoc.X += 130;
            btn_SameIDCheck.Location = btnLoc;
            btn_SameIDCheck.Size = btn_Join.Size;







            this.Controls.Add(lb_Email);
            this.Controls.Add(lb_ID);
            this.Controls.Add(lb_NickName);
            this.Controls.Add(lb_Password);
            this.Controls.Add(tb_Email);
            this.Controls.Add(tb_ID);
            this.Controls.Add(tb_NickName);
            this.Controls.Add(tb_Password);
            this.Controls.Add(btn_Join);
            this.Controls.Add(lb_war_ID);
            this.Controls.Add(lb_war_password);
            this.Controls.Add(btn_SameIDCheck);
            this.Controls.Add(tb_chkPassword);
            this.Controls.Add(lb_ChkPassword);
            this.ResumeLayout(false);
        }





















        void ConnectLoginServer()
        {

        }

        void btn_join_Clicked(object sender, EventArgs arg)
        {
            // 띄어쓰기 등등 체크해야함.
            if (tb_Password.Text != tb_chkPassword.Text)
            {
                MessageBox.Show("비밀번호 확인 다시하세요", "비번체크");
            }
            else
            {
                // 서버에 정보를 보낸후,

                TcpClient tc = new TcpClient("222.99.239.98", 7000);
                string msg = tb_ID.Text + " " + tb_Password.Text;
                byte[] bMsg = Encoding.Unicode.GetBytes(msg);
                TcpHeader head = new TcpHeader();
                head.mode = 1;
                head.msgsize =  (uint)szHeader + (uint)bMsg.Length;
                ByteField joinInfo = new ByteField(256);
                joinInfo.setHeader(head);
                joinInfo.ConcStrAfterHead(bMsg);

                NetworkStream stream = tc.GetStream();
                stream.Write(joinInfo.m_field, 0, joinInfo.m_size);



                int cnt = 0;
                ByteField msgFromServ = new ByteField(bytesize);
                // 헤더수신보장
                cnt += stream.Read(msgFromServ.m_field, 0, bytesize - cnt);
                while (cnt <= 8)
                {
                    cnt += stream.Read(msgFromServ.m_field, cnt - 1, bytesize - cnt);
                }

                int msgze = (int)msgFromServ.getheader().msgsize;

                while(cnt <= msgze)
                {
                    cnt += stream.Read(msgFromServ.m_field, cnt - 1, bytesize - cnt);
                }


                stream.Close();
                tc.Close();
            }
        }
        // mode 종류
        // 1 회원가입 
        // 2 로그인
        // 3 메세지 수신


        // 회원가입 
        // clnt -> "아이디 비번" 구분자 ' ' 무결성은 cnt쪽에서 확인.
        // serv -> "생성 성공 밑 중복 여부 전달"
        // 0x1 성공 0x2 중복 0x3 알수없는 오류

        void btnCheckID(object sender, EventArgs arg)
        {

        }


        void tbIDAutoCheck()
        {

        }

        void tbPWAutoCheck()
        {

        }


        static public void passwordKeyDown(object sender, EventArgs arg)
        {
            TextBox tbx = (TextBox)sender;
            tbx.PasswordChar = '*';
        }

    }
}
