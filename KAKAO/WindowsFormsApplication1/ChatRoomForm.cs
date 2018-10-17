using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BorderlessForm;
using System.Windows.Forms;
using System.Drawing;
using CustomButton;
using System.Net.Sockets;
using WindowsFormsApplication1;
namespace chatRoomForm
{
    // 채팅룸 버튼에 이생선된 폼 렌퍼런스를 가지고 있다면 어떨까
    // 서로 상호참조해서 이폼이 죽을때 버튼의 레퍼런스를 null로 바꿈 될거같음.
    class ChatRoomForm : borderlessForm
    {
        RichTextBox rtb_chat;
        Panel pn_chat;
        Button bt_send;
        List<showLetters> lstChatText;
        string key;
        showLetters test;
        public ChatRoomForm(string ID)
        {
            lstChatText = new List<showLetters>();
            key = ID;
            bt_send = new Button();
            bt_send.Size = new Size(50, 50);
            bt_send.Location = new Point(250, this.Height - 100);
            bt_send.BackColor = Color.Yellow;
            bt_send.Text = "전송";
            bt_send.FlatStyle = FlatStyle.Flat;
            bt_send.FlatAppearance.BorderSize = 0;
            bt_send.MouseClick += new MouseEventHandler(onbtnDown);

            rtb_chat = new RichTextBox();
            rtb_chat.Size = new Size(250, 100);
            rtb_chat.Location = new Point(0, 0);
            rtb_chat.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtb_chat.KeyDown += new KeyEventHandler(rtb_onEnterDown);
            pn_chat = new Panel();
            pn_chat.Size = new Size(300, 150);
            pn_chat.Location = new Point(0, this.Height - 150);
            pn_chat.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            pn_chat.BackColor = Color.White;

            pn_chat.Controls.Add(rtb_chat);
            
            // do get messages here
            //test = new showLetters(200, "ㅁㄴㄹㄴㅁㄻㄴㄹㄴㅁㄻㄴㄻㄴㄹㄴㅁㄻㄴㄹㄴㅁㄹㄴㅁㄹㄴㅁㄻㄴ", new Point(55,50 ));
            test = new showLetters(200, "ㅁㄴㄹㄴㅁㄻ12555555555555555555555555555555125125ㄴㄹㄴ", new Point(55, 50));
            
            // 상대방적정 수치 width 200, 인덴트 55,50
            //  얘의 길이 최대치는 width에서 결정남. 근데 뒤로 당겨짐
            // 내 적정 수치 width 200 , 인덴트 55,50
            // 애도 width에서 결정남 근데 앞쪽으로 당겨짐.

            this.Controls.Add(bt_send);
            this.Controls.Add(pn_chat);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Font f = new Font("Cambria", 8);
            


            f.Dispose();
        }

        void rtb_onEnterDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                datasend();
                e.Handled = true;
                
            }
        }


        void datasend()
        {
            // protocol
            // |mode = 300 | sz|firend msg|//
            string chat = rtb_chat.Text.ToString();
            rtb_chat.Text = "";
            StringBuilder sb =new StringBuilder( this.key);
            sb.Append(" ");
            sb.Append(chat);
            profileForm.ProfileForm.pushQue(sb.ToString());
            this.Refresh();
        }

        void onbtnDown(object sender, MouseEventArgs e)
        {
            if(rtb_chat.TextLength > 0)
            {
                datasend();
            }
        }


    }
}
