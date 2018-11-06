using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Net.Sockets;

using WindowsFormsApplication1;
using CustomButton;

using BorderlessForm;
using Formjoin;
using profileForm;
using chatRoomForm;
namespace KakaoForm
{
    public class KaKaoForm : borderlessForm
    {
        
        DrawButton btn_login;
        DrawButton btn_join;
        


        TextBox tbx_password;
        TextBox tbx_ID;
        // MISC for do something
        Image logo;
        string id;
        string pass;
        // init here
        string server = "127.0.0.1";
        int szHead = 8;
        public KaKaoForm()
        {
          //  // for debug
          //  //==============================

            tbx_password = new TextBox();
            // init start
            this.FormBorderStyle = FormBorderStyle.None;
            SuspendLayout();
            this.Size = new Size(300, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Opacity = 1;
            this.Name = "kakao";
            this.Text = null;
            // 테두리, 크기 조정가능한지, 캡션표시줄 이 있는지 등등..
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.SteelBlue;
            Point formsize = new Point(this.Size);
            // 로그인창에 필요한 그림 그리기
            logo = Image.FromFile("resource\\img_login_img.png");

       
            // textbox for login
            tbx_password.Text = "비밀번호";
            tbx_password.Location = new Point(75, 377);
            tbx_password.Size = new Size(150, 50);
            tbx_password.ForeColor = Color.Gray;
            tbx_password.Font = new Font("Arial", 12, FontStyle.Regular);
            tbx_password.TabIndex = 1;
            tbx_password.GotFocus += new EventHandler(this.tbxPassGotFocus);
            tbx_password.LostFocus += new EventHandler(this.tbxPassLostFocus);
            tbx_password.KeyDown += new KeyEventHandler(this.tbxGetDown);
            tbx_password.MaxLength = 12;

            tbx_ID = new TextBox();
            tbx_ID.Text = "아이디";
            tbx_ID.Location = new Point(75, 350);
            tbx_ID.Size = new Size(150, 50);
            tbx_ID.ForeColor = Color.Gray;
            tbx_ID.Font = new Font("Arial", 12, FontStyle.Regular);
            tbx_ID.GotFocus += new EventHandler(this.tbxIDGotFocus);
            tbx_ID.LostFocus += new EventHandler(this.tbxIDLostFocus);
            tbx_ID.TabIndex = 0;
            // btn for login
            Size szBtnlg = new Size(150, 30);
            btn_login = new DrawButton(linearCenter(this.Size.Width, this.Size.Height, 0.5f, 0.85f, szBtnlg), szBtnlg);
            btn_login.Text = "로그인";
            btn_login.MouseClick += new MouseEventHandler(btnLoginClicked);
            //btn for join
            btn_join = new DrawButton(linearCenter(this.Size.Width, this.Size.Height, 0.5f, 0.92f, szBtnlg), szBtnlg);
            btn_join.Text = "회원가입";
            btn_join.MouseClick += new MouseEventHandler(btnJoinClicked);
            // hit caption msg
            



            // end of init
            this.ResumeLayout(false);






            //add cotroll 
            this.Controls.Add(tbx_password);
            this.Controls.Add(tbx_ID);
            this.Controls.Add(btn_login);
            this.Controls.Add(btn_join);
        }

       

      

        // password_event
        private void tbxPassGotFocus(object sender, EventArgs arg)
        {
            TextBox tbx = (TextBox)sender;
            tbx.ForeColor = Color.Black;
            if (tbx.Text == "비밀번호")
            {
                tbx.Text = "";
            }

        }

        private void tbxPassLostFocus(object sender, EventArgs arg)
        {
            TextBox tbx = (TextBox)sender;
            
            if (tbx.Text == "비밀번호" || tbx.Text == "")
            {
                tbx.PasswordChar = '\0';
                tbx.ForeColor = Color.Gray;
                tbx.Text = "비밀번호";
            }
        }

        private void tbxGetDown(object sender , EventArgs arg)
        {
            TextBox tbx = (TextBox)sender;

            if(tbx.Text.ToString() == ""&& tbx.Text.ToString() == "비밀번호")
            {
                tbx.PasswordChar = '\0';
            }
            else
            {
                tbx.PasswordChar = '*';
            }
        }


        //ID_event
        private void tbxIDGotFocus(object sender, EventArgs arg)
        {
            TextBox tbx = (TextBox)sender;
            if (tbx.Text == "아이디")
            {
                tbx.ForeColor = Color.Black;
                tbx.Text = "";
            }
            else
            {
                tbx.ForeColor = Color.Black;
            }
        }
        private void tbxIDLostFocus(object sender, EventArgs arg)
        {
            TextBox tbx = (TextBox)sender;
            if (tbx.Text == "")
            {
                tbx.ForeColor = Color.Gray;
                tbx.Text = "아이디";
            }
        }

        // img draw for form
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Graphics G = pevent.Graphics;

            Point logoPos = linearCenter(this.Size.Width, this.Size.Height, 0.45F, 0.3F, logo.Size);
            G.DrawImage(logo, logoPos);
        }

       //회원가입
       void btnJoinClicked(object sender, MouseEventArgs e)
        {
            Form join = new Join();
            join.ShowDialog();
        }


        //loginclicked
        void btnLoginClicked(object sender , MouseEventArgs e)
        {
            TcpClient tcpClient = new TcpClient(server, 7000);
            NetworkStream stream = tcpClient.GetStream();
            string id  = tbx_ID.Text;
            string password = tbx_password.Text;
            ByteField loginmsg = new ByteField(256);

            string msg = id + " " + password;
            TcpHeader head;
            head.mode = 2;
            head.msgsize = 0;
            loginmsg.setHeader(head);
            loginmsg.ConcStrAfterHead(msg);
            loginmsg.setHeadLenByIndex();

            stream.Write(loginmsg.m_field, 0, (int)loginmsg.getheader().msgsize);

            int cnt = 0;
            ByteField msgFromServ = new ByteField(256);
            cnt += stream.Read(msgFromServ.m_field, 0, 256 - cnt);
            while (cnt < 8)
            {
                cnt += stream.Read(msgFromServ.m_field, cnt , 256 - cnt);
            }

            if(msgFromServ.getheader().mode == 200)
            {

                // 로그인 시작
                Form client = new ProfileForm();
                ((ProfileForm)client).initData(tbx_ID.Text, tcpClient,msgFromServ.getMsgStr());
                
                client.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("아이디와비밀번호를 확인해주세요");
            }
            
        }

        // 내부함수
        private Point linearCenter(int width, int height, float xScale, float yScale, Size btnSize)
        {

            Point center;
            int xCent = (int)(width * xScale);
            int yCent = (int)(height * yScale);
            int imgWid = (int)(btnSize.Width / 2);
            int imgHei = (int)(btnSize.Height / 2);
            xCent -= imgWid;
            yCent -= imgHei;
            return center = new Point(xCent, yCent);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // KaKaoForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "KaKaoForm";
            this.ResumeLayout(false);

        }
    }
}
