using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BorderlessForm;
using System.Drawing;
using CustomButton;
namespace Formjoin
{
    class Join : borderlessForm
    {
        enum ESameCheck
        {
            ID,
            PW
        }


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
            if(tb_Password.Text !=  tb_chkPassword.Text)
            {
                System.Console.WriteLine("일치안함");
                System.Console.WriteLine("password:{0} chkpass:{1}", tb_Password.Text,tb_chkPassword.Text);
            }
            else
            {
                System.Console.WriteLine("일치함");
                System.Console.WriteLine("password:{0} chkpass:{1}", tb_Password.Text, tb_chkPassword.Text);
            }
        }


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
