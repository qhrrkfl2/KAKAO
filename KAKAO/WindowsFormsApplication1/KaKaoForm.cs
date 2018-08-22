using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Data;

using CustomButton;
using BorderlessForm;
using Formjoin;
namespace KakaoForm
{
    public class KaKaoForm : Form
    {
        private System.ComponentModel.IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        // 보더스타일이 none일때 캡션힛 메세지 받는법 선언들===================================================
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        //===================================================================================================
        // controlls
        SButton btn_close;
        SButton btn_mini;
        SButton btn_setting;
        DrawButton btn_login;
        DrawButton btn_join;



        TextBox tbx_password;
        TextBox tbx_ID;
        // MISC for do something
        Image logo;
        string id;
        string pass;
        // init here
        
        public KaKaoForm()
        {
            tbx_password = new TextBox();
            // init start
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

            // title btn init
            btn_close = new SButton("resource\\btn_login_close.txt", this.Size.Width - 25, 5);
            btn_close.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ClsBtnMouseClicked);
            btn_mini = new SButton("resource\\btn_login_min.txt", this.Size.Width - 50, 5);
            btn_mini.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MiniBtnMouseClicked);
            btn_setting = new SButton("resource\\btn_login_menu.txt", this.Size.Width - 75, 5);

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
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);



            // end of init
            this.ResumeLayout(false);






            //add cotroll 
            this.Controls.Add(btn_close);
            this.Controls.Add(btn_mini);
            this.Controls.Add(btn_setting);
            this.Controls.Add(tbx_password);
            this.Controls.Add(tbx_ID);
            this.Controls.Add(btn_login);
            this.Controls.Add(btn_join);
        }

        // 보더스타일이 none일때 마우스로 폼 움직이는 로직.
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        // events
        // close btn event
        private void ClsBtnMouseClicked(object sender, EventArgs arg)
        {
            this.Close();
        }
        // minimize
        private void MiniBtnMouseClicked(object sender, EventArgs arg)
        {
            this.WindowState = FormWindowState.Minimized;
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
            // do something
            System.Console.WriteLine(tbx_password.Text);
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
