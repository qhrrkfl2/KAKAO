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


//=========================디자이너 추가를 위한 네임스페이스============================
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;   // Note: add reference required: System.Design.dll
//=====================================================================================


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
    }
    //스프라이트 기반 반응형 버튼 스프라이트는 1차원 배열이다.
    public class SButton : Control
    {
        protected Image m_SprImg;
        protected int x, y, sprX, sprY;
        protected Dictionary<string, string> metadata = new Dictionary<string, string>();
        protected RectangleF srcOfImg;
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



    //helper func
    public class HelperFunc
    {
         static public void passwordKeyDown(object sender, EventArgs arg)
        {
            TextBox tbx = (TextBox)sender;
            tbx.PasswordChar = '*';
        }

    }
}// namespace

