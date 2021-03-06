﻿using System;
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
        chatList m_chatlog;
        string key;
        showLetters test;
        public ChatRoomForm(string ID, chatList init)
        {

            m_chatlog = init;
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
            this.FormClosing += new FormClosingEventHandler(onClosing);
            this.Controls.Add(bt_send);
            this.Controls.Add(pn_chat);
        }

        void onClosing( object sender, FormClosingEventArgs e  )
        {
            BtnChatMemManager.getInstance().dicFormList.Remove(this.key);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Font f = new Font("Cambria", 10);
            m_chatlog.drawChatballons(e, f);
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
            Graphics G = this.CreateGraphics();
            m_chatlog.addChat(chat, true, G);
            G.Dispose();
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
