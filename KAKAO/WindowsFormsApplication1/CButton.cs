using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
// file
using System.IO;
// for image
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using KakaoForm;
using WindowsFormsApplication1;
using System.Net.Sockets;
//=========================디자이너 추가를 위한 네임스페이스============================
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;   // Note: add reference required: System.Design.dll
                                     //=====================================================================================
using chatRoomForm;
//If you did not create the graphics object you should not dispose it, so if you function signature is protected override void OnPaint(PaintEventArgs e) you would NOT dispose e.Graphics.
//However if you create a graphics object in the OnPaint handler you will need to dispose it.
//General rule of thumb(and it is a rule of thumb not a law) if you did not get your object from a Graphics.FromXxxxx() you do not need to call Dispose.


namespace CustomButton
{

    // 멀티이미지 반응형 버튼
    public class CButton : Control
    {


        private Image bImg;
        private Image bMouseOnImage;
        private Image bMouseDownImage;
        private Image Ref;
        public CButton(string path, Size size, Point Loc)
        {
            // prepare for resize
            bImg = Image.FromFile(path);
            this.Size = size;
            this.Location = Loc;
            Ref = bImg;
            this.MouseEnter += new System.EventHandler(this.MousebuttonOn);
            this.MouseLeave += new System.EventHandler(this.MouseBtnOut);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MouseClicked);

        }

        public void SetMouseOnImg(string path)
        {
            bMouseOnImage = Image.FromFile(path);
        }

        public void SetMouseDownImg(string path)
        {
            bMouseDownImage = Image.FromFile(path);
        }
        // override event
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Graphics g = pevent.Graphics;
            g.DrawImage(Ref, new Point());
        }
        // my event
        private void MousebuttonOn(object sedner, EventArgs arg)
        {
            if (bMouseOnImage != null)
                Ref = bMouseOnImage;
        }
        private void MouseClicked(object sender, EventArgs arg)
        {
            if (bMouseOnImage != null)
                Ref = bMouseDownImage;
        }
        private void MouseBtnOut(object sender, EventArgs arg)
        {
            if (bMouseOnImage != null)
                Ref = bImg;
        }
        ~CButton()
        {
            if (bImg != null)
                bImg.Dispose();
            if (bMouseDownImage != null)
                bMouseOnImage.Dispose();
            if (bMouseDownImage != null)
                bMouseDownImage.Dispose();
        }


    }
    //스프라이트 기반 반응형 버튼 스프라이트는 1차원 배열이다.
    public class SButton : Control
    {
        protected Image m_SprImg;
        protected int x, y, sprX, sprY;
        protected Dictionary<string, string> metadata = new Dictionary<string, string>();
        protected RectangleF srcOfImg;
        bool active = false;
        ~SButton()
        {
            m_SprImg.Dispose();
        }


        // x,y == location of btn
        public SButton(string Data, int initX, int initY)
        {
            StreamReader rstm = new StreamReader(Data);
            string line = rstm.ReadLine();
            string data = line;
            string[] toks = data.Split(',');
            foreach (string tok in toks)
            {
                string[] d = tok.Split(':');
                metadata.Add(d[0], d[1]);
            }
            m_SprImg = Image.FromFile(metadata["path"]);
            int idx = int.Parse(metadata["leave"]);
            string[] xySize = metadata["sImg"].Split('X');
            x = int.Parse(xySize[0]);
            y = int.Parse(xySize[1]);
            string[] sprSize = metadata["sSpr"].Split('X');
            sprX = int.Parse(sprSize[0]);
            sprY = int.Parse(sprSize[1]);
            this.GetIdxSize(idx);


            // controll init
            this.Size = new Size(sprX, sprY);
            this.Location = new Point(initX, initY);
            this.MouseEnter += new System.EventHandler(this.MousebuttonOn);
            this.MouseLeave += new System.EventHandler(this.MouseBtnOut);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MouseClicked);
            rstm.Close();

        }
        public SButton(string Data, int initX, int initY, int btnsizeWidth, int btnszieHeight)
        {
            StreamReader rstm = new StreamReader(Data);
            string line = rstm.ReadLine();
            string data = line;
            string[] toks = data.Split(',');
            foreach (string tok in toks)
            {
                string[] d = tok.Split(':');
                metadata.Add(d[0], d[1]);
            }
            m_SprImg = Image.FromFile(metadata["path"]);
            int idx = int.Parse(metadata["leave"]);
            string[] xySize = metadata["sImg"].Split('X');
            x = int.Parse(xySize[0]);
            y = int.Parse(xySize[1]);
            string[] sprSize = metadata["sSpr"].Split('X');
            sprX = int.Parse(sprSize[0]);
            sprY = int.Parse(sprSize[1]);
            this.GetIdxSize(idx);

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);
            this.BackColor = Color.Transparent;

            // controll init
            this.Size = new Size(sprX, sprY);
            this.Location = new Point(initX, initY);
            this.MouseEnter += new System.EventHandler(this.MousebuttonOn);
            this.MouseLeave += new System.EventHandler(this.MouseBtnOut);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MouseClicked);
            rstm.Close();

        }


        // draw
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Graphics g = pevent.Graphics;
            g.DrawImage(m_SprImg, 0, 0, srcOfImg, GraphicsUnit.Pixel);

        }
        // inner func
        private void GetIdxSize(int idx)
        {
            float x = idx * sprX;
            float y = 0;
            srcOfImg = new RectangleF(x, y, sprX, sprY);
        }
        // event
        private void MousebuttonOn(object sedner, EventArgs arg)
        {
            this.GetIdxSize(int.Parse(metadata["enter"]));
            this.Refresh();
        }

        private void MouseClicked(object sender, EventArgs arg)
        {
            this.GetIdxSize(int.Parse(metadata["clicked"]));
            this.Refresh();
        }

        private void MouseBtnOut(object sender, EventArgs arg)
        {
            this.GetIdxSize(int.Parse(metadata["leave"]));
            this.Refresh();
        }


        private void ChatMousebuttonOn(object sedner, EventArgs arg)
        {
            if (active == false)
            {
                this.GetIdxSize(int.Parse(metadata["enter"]));
                this.Refresh();
            }
        }

        private void ChatMouseClicked(object sender, EventArgs arg)
        {
            if (active == false)
            {
                this.GetIdxSize(int.Parse(metadata["clicked"]));
                this.Refresh();
            }
            active = true;
        }

        private void ChatMouseBtnOut(object sender, EventArgs arg)
        {
            if (active == false)
            {
                this.GetIdxSize(int.Parse(metadata["leave"]));
                this.Refresh();
            }
        }

        public void setforChatButton()
        {
            this.MouseEnter -= new System.EventHandler(this.MousebuttonOn);
            this.MouseLeave -= new System.EventHandler(this.MouseBtnOut);
            this.MouseClick -= new System.Windows.Forms.MouseEventHandler(this.MouseClicked);

            this.MouseEnter += new System.EventHandler(this.ChatMousebuttonOn);
            this.MouseLeave += new System.EventHandler(this.ChatMouseBtnOut);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ChatMouseClicked);

        }

        public void forChatAction()
        {
            active = false;
            this.GetIdxSize(int.Parse(metadata["leave"]));
            this.Refresh();
        }


    }


    public class SizeimgButton : SButton
    {
        int imgW;
        int imgH;
        public SizeimgButton(string data, int LocX, int LocY, int imgW, int imgH) : base(data, LocX, LocY)
        {
            this.imgW = imgW;
            this.imgH = imgH;


        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Graphics g = pevent.Graphics;
            Rectangle pos = new Rectangle(this.Location, new Size(imgW, imgH));
            g.DrawImage(m_SprImg, pos, srcOfImg.X, srcOfImg.Y, srcOfImg.Width, srcOfImg.Height, GraphicsUnit.Pixel);
        }

    }




    // arc 모양의 버튼을 그리고 싶을때 사용.
    public class DrawButton : Button
    {
        public DrawButton(Point loc, Size size)
        {
            this.Location = loc;
            this.Size = size;
            // 보더 없애기
            this.TabStop = false;
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            // 보더 없애기 end
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics G = e.Graphics;

            int wid = this.ClientRectangle.Width;
            int hei = this.ClientRectangle.Height;
            Rectangle rect = new Rectangle(0, 0, wid, hei);
            using (GraphicsPath path = GetRoundedRectPath(rect, wid / 100))
            {
                G.FillPath(Brushes.Yellow, path);
            }

            SolidBrush brus = new SolidBrush(Color.Black);
            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;
            G.DrawString(this.Text.ToString(), this.Font, brus, new Rectangle(new Point(), this.Size), format);


        }

        //내부함수
        GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            int diameter = 2 * radius;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            GraphicsPath path = new GraphicsPath();
            //좌상
            path.AddArc(arcRect, 180, 90);
            //우상
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);
            //우하
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // 좌하
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    public class PasswordTb : TextBox
    {
        string password;
        public PasswordTb()
        {
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(getPassword);
        }

        private void getPassword(object sender, EventArgs arg)
        {
            password = this.Text;
            if (arg.ToString() != "\n")
            {
                int len = Text.ToString().Length;
                for (int i = 0; i < len; i++)
                {
                    Text += "*";
                }
            }
            else
            {
                // do password things here
            }

        }



    }





    public class ProfileList : Control
    {
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

        string ID;
        // 미구현
        string Mention;
        Image picture;
        Label lb_profilemention;
        //====================================
        Label lb_name;
        public ProfileList(Size size, Point pt, string id)
        {

            this.Location = pt;
            this.Size = size;
            BackColor = Color.White;
            ID = id;


            // 버튼을 앵커스타일 주면 알아서 앞으로 붙는다.
            lb_name = new Label();
            lb_name.BorderStyle = BorderStyle.None;
            lb_name.Size = new Size(50, 50);
            lb_name.Location = new Point(51, 25);
            //lb_name.Text = id;

            lb_profilemention = new Label();
            lb_profilemention.BorderStyle = BorderStyle.None;
            lb_profilemention.Size = new Size(50, 50);
            lb_profilemention.Location = new Point(250, 25);

            // test contents 정식으로 디비에서 받아올때 여기내용을 바꿀것
            picture = Image.FromFile("resource\\testprofile.png");
            lb_name.Text = id;
            lb_profilemention.Text = "뒤지라우";


            // 선택되면 백그라운드도 바꾸면됨.
            this.Controls.Add(lb_name);
            this.Controls.Add(lb_profilemention);

        }




        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle pos = new Rectangle(new Point(0,0), new Size(50, 50));
            g.DrawImage(picture, pos, 0, 0, 450, 600, GraphicsUnit.Pixel);
        }

    }



    class ProFileEX : ProfileList
    {
        Form m_parent;
        int name = 0;
        public ProFileEX(Size size, Point pt, string id, Form parent) : base(size, pt, id)
        {
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MouseDoubleClicked);
            this.m_parent = parent;
            this.Refresh();
        }

        private void MouseDoubleClicked(object sender, EventArgs arg)
        {

            Form f = new ChatRoomForm();
            f.Show();

        }
    }


    //helper func
    public class HelperFunc
    {
        static public void passwordKeyDown(object sender, EventArgs arg)
        {
            TextBox tbx = (TextBox)sender;
            tbx.PasswordChar = '*';
        }



        public Point getHeightCenter(Size parent, Size child)
        {
            int half = parent.Width / 2;
            half -= (child.Width / 2);
            return new Point(0, half);
        }

        public Point getLinearPercentage(Point vecA, Point vecB, float percentageScale)
        {

            float x = Math.Abs(vecA.X - vecB.X);
            float y = Math.Abs(vecA.Y - vecB.Y);
            x *= percentageScale;
            y *= percentageScale;

            Point result = new Point((int)x, (int)y);
            return result;
        }


        static public void GetMessage(ByteField field, NetworkStream stream)
        {
            int bytesize = 8;
            int cnt = 0;
            ByteField msgFromServ = field;
            // 헤더수신보장
            cnt += stream.Read(msgFromServ.m_field, 0, bytesize - cnt);
            while (cnt <= 8)
            {
                cnt += stream.Read(msgFromServ.m_field, cnt - 1, bytesize - cnt);
            }

            int msgze = (int)msgFromServ.getheader().msgsize;

            while (cnt <= msgze)
            {
                cnt += stream.Read(msgFromServ.m_field, cnt - 1, bytesize - cnt);
            }
        }


    }
}// namespace

