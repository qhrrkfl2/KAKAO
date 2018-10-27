using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomButton;
using System.Drawing;

using KakaoForm;
// =====================================디자이너====================================================
using System.ComponentModel;
using System.Windows.Forms.Design;   // Note: add reference required: System.Design.dll
// 프로젝트 add 눌러서 reference 눌러서 위 레퍼런스 추가
// ==============================================================================================
namespace BorderlessForm
{

    // [Designer(typeof(MyDesigner))]   // Note: custom designer
    // public partial class UserControl1 : UserControl
    // {
    //     public UserControl1()
    //     {
    //         
    //     }
    //
    //     // Note: property added
    //     [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //     public ListView Employees { get { return listView1; } }
    // }
    //
    // // Note: custom designer class added
    // class MyDesigner : ControlDesigner
    // {
    //     public override void Initialize(IComponent comp)
    //     {
    //         base.Initialize(comp);
    //         var uc = (UserControl1)comp;
    //         EnableDesignMode(uc.Employees, "Employees");
    //     }
    // }
   public class borderlessForm : Form
    {

        ///// <summary>
        ///// Clean up any resources being used.
        ///// </summary>
        ///// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}


        // member variable =======================================================================================================================
        SizeimgButton btn_close;
        SizeimgButton btn_mini;
        SizeimgButton btn_max;

        //variable for Title bar move=======================================================================================================================================
        Rectangle TBHitBox;
        Point OldMousePos;
        bool isMove = false;

        //=======================================================================================================================================

        public borderlessForm()
        {
            init();
            buttonforcapinit();
        }

        // 이것을 오버라이드해서 폼을 초기화 
        // 그리고 폼을 만들고 init()으로 버튼을 넣자
        void init()
        {
            SuspendLayout();
            this.Size = new Size(300, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Opacity = 1;
            this.Name = "kakaoSub";
            this.Text = null;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.SteelBlue;
            TBHitBox = new Rectangle(new Point(0, 0), new Size(this.Size.Width, 100));
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(MouseDownEvent);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(MouseUpEvent);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(MouseMoveEvent);

            this.ResumeLayout(false);
        }

        void buttonforcapinit()
        {
            btn_close = new SizeimgButton("resource\\btn_close_white.txt", 269, 1, 20, 20);
            btn_mini = new SizeimgButton("resource\\btn_min_inactive_white.txt", 249, 1, 20, 20);
            btn_max = new SizeimgButton("resource\\btn_max_inactive_white.txt", 229, 1, 20, 20);
            btn_close.Click += new EventHandler(ClsBtnMouseClicked);
            btn_mini.Click += new EventHandler(MiniBtnMouseClicked);
   

            this.Controls.Add(btn_close);
            this.Controls.Add(btn_max);
            this.Controls.Add(btn_mini);
            this.Invalidate();
        }

        // ==================== for title moving and Mouse Event=================================
        private void MouseDownEvent(object sender, MouseEventArgs arg)
        {
            if (TBHitBox.Contains(arg.X, arg.Y))
            {
                OldMousePos = new Point(arg.X, arg.Y);
                isMove = true;
            }
        }

        private void MouseUpEvent(object sender, MouseEventArgs arg)
        {
            isMove = false;
        }

        private void MouseMoveEvent(object sender, MouseEventArgs arg)
        {
            if (Capture)
            {
                if (isMove == true)
                    Location = new Point(MousePosition.X - OldMousePos.X, MousePosition.Y - OldMousePos.Y);
            }

        }
        //=====================================================================================
        private void ClsBtnMouseClicked(object sender, EventArgs arg)
        {
            this.Close();
        }

        private void MiniBtnMouseClicked(object sender, EventArgs arg)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void MaxBtnMouseClicked(object sender, EventArgs arg)
        {
            this.WindowState = FormWindowState.Maximized;
        }


    }

    
}
